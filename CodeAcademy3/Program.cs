using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
