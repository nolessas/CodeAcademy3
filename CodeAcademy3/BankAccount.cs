public class BankAccount
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; }
    public string Pin { get; set; }
    public decimal Balance { get; set; }
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
}
