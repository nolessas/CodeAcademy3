public interface IAccountRepository
{
    BankAccount GetById(Guid id);
    BankAccount GetByCardNumber(string cardNumber);
    void Add(BankAccount account);
    void Update(BankAccount account);
    List<BankAccount> GetAllAccounts();
}
