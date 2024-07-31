using System.Text.Json;

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