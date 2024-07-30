using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class Program
{
    public static void Main(string[] args)
    {
        // Load accounts from file, initialize services, and run the ATM
        var accounts = FileOperations.LoadAccounts();
        var accountRepository = new AccountRepository(accounts);
        var bankService = new BankService(accountRepository);
        var atm = new ATM(bankService);

        atm.Run();

        // Save updated account information back to file
        FileOperations.SaveAccounts(accounts);
    }
}

public class BankAccount
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; }
    public string Pin { get; set; }
    public decimal Balance { get; set; }
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
}

public class Transaction
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; }
}

public interface IBankService
{
    bool ValidateCard(string cardNumber, string pin);
    bool ChangePin(string cardNumber, string oldPin, string newPin);
    decimal GetBalance(Guid accountId);
    List<Transaction> GetRecentTransactions(Guid accountId, int count);
    bool WithdrawMoney(Guid accountId, decimal amount);
    (Dictionary<int, int>, decimal) CalculateDenominations(decimal amount);
    BankAccount GetByCardNumber(string cardNumber);
}

public class BankService : IBankService
{
    private readonly IAccountRepository _accountRepository;

    public BankService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public bool ValidateCard(string cardNumber, string pin)
    {
        var account = _accountRepository.GetByCardNumber(cardNumber);
        return account != null && account.Pin == pin;
    }

    public bool ChangePin(string cardNumber, string oldPin, string newPin)
    {
        var account = _accountRepository.GetByCardNumber(cardNumber);
        if (account == null || account.Pin != oldPin)
            return false;

        account.Pin = newPin;
        _accountRepository.Update(account);
        return true;
    }

    public decimal GetBalance(Guid accountId)
    {
        var account = _accountRepository.GetById(accountId);
        return account?.Balance ?? 0;
    }

    public List<Transaction> GetRecentTransactions(Guid accountId, int count)
    {
        var account = _accountRepository.GetById(accountId);
        return account?.Transactions.OrderByDescending(t => t.Date).Take(count).ToList() ?? new List<Transaction>();
    }

    public bool WithdrawMoney(Guid accountId, decimal requestedAmount)
    {
        var account = _accountRepository.GetById(accountId);
        if (account == null || account.Balance < requestedAmount)
            return false;

        // Check daily withdrawal limits
        if (IsWithdrawalLimitExceeded(accountId, requestedAmount))
        {
            Console.WriteLine("Daily withdrawal limit exceeded. Maximum 10 withdrawals or $1000 per day.");
            return false;
        }

        var (denominations, actualAmount) = CalculateDenominations(requestedAmount);

        if (actualAmount == 0)
            return false;

        // Update account balance and add transaction
        UpdateAccountBalance(account, actualAmount);
        _accountRepository.Update(account);
        return true;
    }

    private bool IsWithdrawalLimitExceeded(Guid accountId, decimal amount)
    {
        return amount > 1000 || GetTodayTransactionsCount(accountId) >= 10;
    }

    private void UpdateAccountBalance(BankAccount account, decimal amount)
    {
        account.Balance -= amount;
        account.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Now,
            Amount = -amount,
            Type = "Withdrawal"
        });
    }

    private int GetTodayTransactionsCount(Guid accountId)
    {
        var account = _accountRepository.GetById(accountId);
        return account?.Transactions.Count(t => t.Date.Date == DateTime.Today && t.Type == "Withdrawal") ?? 0;
    }

    public (Dictionary<int, int>, decimal) CalculateDenominations(decimal requestedAmount)
    {
        var denominations = new Dictionary<int, int>
        {
            {100, 0}, {50, 0}, {20, 0}, {10, 0}, {5, 0}
        };

        decimal actualAmount = 0;

        foreach (var denom in denominations.Keys.OrderByDescending(k => k))
        {
            int count = (int)(requestedAmount / denom);
            denominations[denom] = count;
            actualAmount += count * denom;
            requestedAmount -= count * denom;
        }

        return (denominations, actualAmount);
    }

    public BankAccount GetByCardNumber(string cardNumber)
    {
        return _accountRepository.GetByCardNumber(cardNumber);
    }
}

public interface IAccountRepository
{
    BankAccount GetById(Guid id);
    BankAccount GetByCardNumber(string cardNumber);
    void Add(BankAccount account);
    void Update(BankAccount account);
    List<BankAccount> GetAllAccounts();
}

public class AccountRepository : IAccountRepository
{
    private List<BankAccount> _accounts;

    public AccountRepository(List<BankAccount> accounts)
    {
        _accounts = accounts;
    }

    public BankAccount GetById(Guid id)
    {
        return _accounts.FirstOrDefault(a => a.Id == id);
    }

    public BankAccount GetByCardNumber(string cardNumber)
    {
        return _accounts.FirstOrDefault(a => a.CardNumber == cardNumber);
    }

    public void Add(BankAccount account)
    {
        _accounts.Add(account);
    }

    public void Update(BankAccount account)
    {
        var index = _accounts.FindIndex(a => a.Id == account.Id);
        if (index != -1)
            _accounts[index] = account;
    }

    public List<BankAccount> GetAllAccounts()
    {
        return _accounts;
    }
}

public class ATM
{
    private readonly IBankService _bankService;
    private BankAccount _currentAccount;

    public ATM(IBankService bankService)
    {
        _bankService = bankService;
    }

    public void Run()
    {
        // Authenticate user with a maximum of 3 attempts
        if (!AuthenticateUser())
        {
            Console.WriteLine("Too many failed attempts. The program will now exit.");
            Environment.Exit(0);
        }

        // If authentication successful, show main menu
        MainMenu();
    }

    private bool AuthenticateUser()
    {
        int attempts = 0;
        while (attempts < 3)
        {
            Console.Clear();
            Console.WriteLine("Welcome to the ATM");
            Console.Write("Enter card number: ");
            string cardNumber = Console.ReadLine();
            Console.Write("Enter PIN: ");
            string pin = Console.ReadLine();

            if (_bankService.ValidateCard(cardNumber, pin))
            {
                _currentAccount = _bankService.GetByCardNumber(cardNumber);
                return true;
            }
            else
            {
                attempts++;
                Console.WriteLine($"Invalid card number or PIN. Attempts remaining: {3 - attempts}");
                Console.ReadKey();
            }
        }
        return false;
    }

    private void MainMenu()
    {
        while (true)
        {
            DisplayMenuOptions();
            string choice = Console.ReadLine();
            ProcessMenuChoice(choice);

            if (choice == "5") // Exit option
                break;

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    private void DisplayMenuOptions()
    {
        Console.Clear();
        Console.WriteLine("1. Check Balance");
        Console.WriteLine("2. Withdraw Money");
        Console.WriteLine("3. View Recent Transactions");
        Console.WriteLine("4. Change PIN");
        Console.WriteLine("5. Exit");
        Console.Write("Select an option: ");
    }

    private void ProcessMenuChoice(string choice)
    {
        switch (choice)
        {
            case "1":
                CheckBalance();
                break;
            case "2":
                WithdrawMoney();
                break;
            case "3":
                ViewRecentTransactions();
                break;
            case "4":
                ChangePin();
                break;
            case "5":
                _currentAccount = null;
                break;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                break;
        }
    }

    private void CheckBalance()
    {
        decimal balance = _bankService.GetBalance(_currentAccount.Id);
        Console.WriteLine($"Your current balance is: ${balance}");
    }

    private void WithdrawMoney()
    {
        Console.Write("Enter amount to withdraw: ");
        if (decimal.TryParse(Console.ReadLine(), out decimal requestedAmount))
        {
            var (denominations, actualAmount) = _bankService.CalculateDenominations(requestedAmount);

            if (_bankService.WithdrawMoney(_currentAccount.Id, actualAmount))
            {
                Console.WriteLine($"Successfully withdrawn ${actualAmount}");
                Console.WriteLine("Denominations:");
                foreach (var denom in denominations)
                {
                    if (denom.Value > 0)
                        Console.WriteLine($"${denom.Key} bills: {denom.Value}");
                }

                if (actualAmount < requestedAmount)
                {
                    Console.WriteLine($"Note: ${requestedAmount - actualAmount} could not be dispensed due to lack of smaller denominations.");
                }
            }
            else
            {
                Console.WriteLine("Withdrawal failed. Please check your balance or withdrawal limits.");
            }
        }
        else
        {
            Console.WriteLine("Invalid amount entered.");
        }
    }

    private void ViewRecentTransactions()
    {
        var transactions = _bankService.GetRecentTransactions(_currentAccount.Id, 5);
        Console.WriteLine("Recent Transactions:");
        foreach (var transaction in transactions)
        {
            Console.WriteLine($"{transaction.Date}: {transaction.Type} ${Math.Abs(transaction.Amount)}");
        }
    }

    private void ChangePin()
    {
        Console.Write("Enter current PIN: ");
        string currentPin = Console.ReadLine();
        Console.Write("Enter new PIN: ");
        string newPin = Console.ReadLine();

        if (_bankService.ChangePin(_currentAccount.CardNumber, currentPin, newPin))
        {
            Console.WriteLine("PIN changed successfully.");
        }
        else
        {
            Console.WriteLine("Failed to change PIN. Please check your current PIN.");
        }
    }
}

public class FileOperations
{
    private const string FILE_PATH = "accounts.txt";

    public static void SaveAccounts(List<BankAccount> accounts)
    {
        using (StreamWriter writer = new StreamWriter(FILE_PATH))
        {
            foreach (var account in accounts)
            {
                writer.WriteLine($"{account.Id},{account.CardNumber},{account.Pin},{account.Balance}");
                foreach (var transaction in account.Transactions)
                {
                    writer.WriteLine($"T,{transaction.Id},{transaction.Date},{transaction.Amount},{transaction.Type}");
                }
            }
        }
    }

    public static List<BankAccount> LoadAccounts()
    {
        List<BankAccount> accounts = new List<BankAccount>();
        if (File.Exists(FILE_PATH))
        {
            using (StreamReader reader = new StreamReader(FILE_PATH))
            {
                string line;
                BankAccount currentAccount = null;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts[0] != "T")
                    {
                        currentAccount = new BankAccount
                        {
                            Id = Guid.Parse(parts[0]),
                            CardNumber = parts[1],
                            Pin = parts[2],
                            Balance = decimal.Parse(parts[3])
                        };
                        accounts.Add(currentAccount);
                    }
                    else
                    {
                        currentAccount.Transactions.Add(new Transaction
                        {
                            Id = Guid.Parse(parts[1]),
                            Date = DateTime.Parse(parts[2]),
                            Amount = decimal.Parse(parts[3]),
                            Type = parts[4]
                        });
                    }
                }
            }
        }
        return accounts;
    }
}
