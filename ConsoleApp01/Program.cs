namespace ConsoleApp01
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
    public class BankAccount
    {   
        public Guid Id { get; set; }
        public string CardNumber { get; set; }
        public string Pin {  get; set; }
        public decimal Balance { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
    public class Transaction
    {
        public Guid Id { get; set; }
        public DateTime Date {get; set; }
        public decimal Amount { get; set; }
        public string Type {  get; set; }
    }
    public interface IBankService
    {
        bool ValidateCard(string cardNumber, string pin);
        bool ChangePin(string cardNumber, string oldPin, string newPin);
        decimal GetBalance(Guid accountId);
        List<Transaction> GetRecentTransactions(Guid accountId, int count);
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
        public decimal GetBalance(Guid accountId)
        {
            var account = _accountRepository.GetById(accountId);
            return account?.Balance ?? 0;
        }
    }
    public interface IAccountRepository
    {
        BankAccount GetById(Guid id);
        BankAccount GetByCardNumber(string cardNumber);
        void Add(BankAccount account);
        void Update(BankAccount account);

    }
    public class AccountRepository : IAccountRepository
    {
        private List<BankAccount> _accounts = new List<BankAccount>();
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
            //
        }
    }
}