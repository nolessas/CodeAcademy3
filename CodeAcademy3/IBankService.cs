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
    void CreateAccount(BankAccount account);
}
