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
