using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

public class Program
{
    public static void Main(string[] args)
    {
        List<BankAccount> accounts = null;
        try
        {
            accounts = FileOperations.LoadAccounts();
            var accountRepository = new AccountRepository(accounts);
            var bankService = new BankService(accountRepository);
            var atm = new ATM(bankService, accountRepository);

            atm.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error: {ex.Message}");
            Console.WriteLine("The program will now exit.");
        }
        finally
        {
            // Save accounts when exiting, even if an exception occurred
            if (accounts != null)
            {
                FileOperations.SaveAccounts(accounts);
            }
        }
    }
}

public class BankAccount
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; }
    public string Pin { get; set; }
    public decimal Balance { get; set; }
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();

    public void Deposit(decimal amount)
    {
        if (amount <= 0 || amount % 5 != 0)
        {
            throw new ArgumentException("Invalid deposit amount. Please use denominations of 5, 10, 20, or 50.");
        }

        Balance += amount;
        Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Now,
            Amount = amount,
            Type = "Deposit"
        });
    }
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
    bool DepositMoney(Guid accountId, decimal amount);
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
        try
        {
            var account = _accountRepository.GetById(accountId);
            if (account == null)
                throw new ArgumentException("Account not found");
            if (account.Balance < requestedAmount)
                throw new InvalidOperationException("Insufficient funds");

            // Check daily withdrawal limits
            if (IsWithdrawalLimitExceeded(accountId, requestedAmount))
            {
                Console.WriteLine("Daily withdrawal limit exceeded. Maximum 10 withdrawals or $1000 per day.");
                return false;
            }

            var (denominations, actualAmount) = CalculateDenominations(requestedAmount);

            if (actualAmount == 0)
                return false;

            UpdateAccountBalance(account, actualAmount);
            _accountRepository.Update(account);
            return true;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid account: {ex.Message}");
            return false;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Withdrawal failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during withdrawal: {ex.Message}");
            return false;
        }
    }

    public bool DepositMoney(Guid accountId, decimal amount)
    {
        try
        {
            var account = _accountRepository.GetById(accountId);
            if (account == null)
                throw new ArgumentException("Account not found");

            if (amount <= 0 || amount % 5 != 0)
                throw new ArgumentException("Invalid deposit amount. Please use denominations of 5, 10, 20, or 50.");

            account.Deposit(amount);
            _accountRepository.Update(account);
            return true;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Deposit failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during deposit: {ex.Message}");
            return false;
        }
    }

    private bool IsWithdrawalLimitExceeded(Guid accountId, decimal amount)
    {
        var todayWithdrawals = GetTodayWithdrawals(accountId);
        var totalWithdrawnToday = todayWithdrawals.Sum(t => Math.Abs(t.Amount));
        return (totalWithdrawnToday + amount > 1000) || (todayWithdrawals.Count >= 10);
    }

    private List<Transaction> GetTodayWithdrawals(Guid accountId)
    {
        var account = _accountRepository.GetById(accountId);
        return account?.Transactions
            .Where(t => t.Date.Date == DateTime.Today && t.Type == "Withdrawal")
            .ToList() ?? new List<Transaction>();
    }

    private void UpdateAccountBalance(BankAccount account, decimal amount)
    {
        try
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            account.Balance -= amount;
            account.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                Amount = -amount,
                Type = "Withdrawal"
            });
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Error updating account balance: {ex.Message}");
            throw;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid amount: {ex.Message}");
            throw;
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"Arithmetic overflow: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error updating account balance: {ex.Message}");
            throw;
        }
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
    private readonly IAccountRepository _accountRepository;
    private BankAccount _currentAccount;

    public ATM(IBankService bankService, IAccountRepository accountRepository)
    {
        _bankService = bankService;
        _accountRepository = accountRepository;
    }

    public void Run()
    {
        // Authenticate user with a maximum of 3 attempts
        if (!AuthenticateUser())
        {
            Console.WriteLine("Too many failed attempts. The program will now exit.");
            return;
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
            Console.WriteLine("Welcome to the C++++ ATM");
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

            if (choice == "6") // Exit option
            {
                break;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    private void DisplayMenuOptions()
    {
        Console.Clear();
        Console.WriteLine("Welcome to C++++ ATM");
        Console.WriteLine("1. Check Balance");
        Console.WriteLine("2. Withdraw Money");
        Console.WriteLine("3. Deposit Money");
        Console.WriteLine("4. View Recent Transactions");
        Console.WriteLine("5. Change PIN");
        Console.WriteLine("6. Exit");
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
                DepositMoney();
                break;
            case "4":
                ViewRecentTransactions();
                break;
            case "5":
                ChangePin();
                break;
            case "6":
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

    private void DepositMoney()
    {
        Console.Write("Enter amount to deposit (must be in denominations of 5, 10, 20, or 50): ");
        if (decimal.TryParse(Console.ReadLine(), out decimal amount))
        {
            if (_bankService.DepositMoney(_currentAccount.Id, amount))
            {
                Console.WriteLine($"Successfully deposited ${amount}");
            }
            else
            {
                Console.WriteLine("Deposit failed. Please check the amount and try again.");
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
    private const string FILE_PATH = "accounts.json";

    public static void SaveAccounts(List<BankAccount> accounts)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FILE_PATH, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving accounts: {ex.Message}");
        }
    }

    public static List<BankAccount> LoadAccounts()
    {
        List<BankAccount> accounts = new List<BankAccount>();
        try
        {
            if (File.Exists(FILE_PATH))
            {
                string jsonString = File.ReadAllText(FILE_PATH);
                accounts = JsonSerializer.Deserialize<List<BankAccount>>(jsonString);
            }
            else
            {
                Console.WriteLine("Accounts file not found. Please ensure 'accounts.json' exists in the program directory.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading accounts: {ex.Message}");
        }
        return accounts;
    }
}