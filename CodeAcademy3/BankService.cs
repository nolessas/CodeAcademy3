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

            // Move the deposit logic here
            account.Balance += amount;
            account.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Now,
                Amount = amount,
                Type = "Deposit"
            });

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

    public void CreateAccount(BankAccount account)
    {
        _accountRepository.Add(account);
    }
}
