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
        while (true)
        {
            if (!HandleInitialPrompt())
            {
                break;
            }

            if (_currentAccount != null)
            {
                MainMenu();
            }

            Console.WriteLine("Do you want to perform another transaction? (Y/N)");
            if (Console.ReadLine()?.Trim().ToUpper() != "Y")
            {
                break;
            }
            _currentAccount = null;
        }
    }

    private bool HandleInitialPrompt()
    {
        Console.Clear();
        Console.WriteLine("Welcome to the C++++ ATM");
        Console.WriteLine("1. Login to existing account");
        Console.WriteLine("2. Create new account");
        Console.WriteLine("3. Exit");
        Console.Write("Select an option: ");

        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        Console.WriteLine();

        switch (keyInfo.KeyChar)
        {
            case '1':
                return AuthenticateUser();
            case '2':
                CreateNewAccount();
                return true;
            case '3':
                return false;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                Console.ReadKey();
                return true;
        }
    }

    private void CreateNewAccount()
    {
        Console.Clear();
        Console.WriteLine("Creating a new account...");

        string cardNumber = GenerateUniqueCardNumber();
        string pin = GeneratePin();
        Guid accountId = Guid.NewGuid();

        BankAccount newAccount = new BankAccount
        {
            Id = accountId,
            CardNumber = cardNumber,
            Pin = pin,
            Balance = 0
        };

        _accountRepository.Add(newAccount);

        Console.WriteLine("New account created successfully!");
        Console.WriteLine($"Your card number is: {cardNumber}");
        Console.WriteLine($"Your PIN is: {pin}");
        Console.WriteLine("Please remember these details for future logins.");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private string GenerateUniqueCardNumber()
    {
        Random random = new Random();
        string cardNumber;
        do
        {
            cardNumber = string.Join("", Enumerable.Range(0, 16).Select(_ => random.Next(10).ToString()));
        } while (_accountRepository.GetByCardNumber(cardNumber) != null);

        return cardNumber;
    }

    private string GeneratePin()
    {
        return new Random().Next(1000, 10000).ToString("D4");
    }

    private bool AuthenticateUser()
    {
        int attempts = 0;
        while (attempts < 3)
        {
            Console.Clear();
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
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            Console.WriteLine();
            ProcessMenuChoice(keyInfo.KeyChar.ToString());

            if (keyInfo.KeyChar == '6') // Exit option
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
