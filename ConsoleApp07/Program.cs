using System.ComponentModel;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;



/*              Restaurant 
 *              
Project Requirements:
- Implement attractive menu display with item selection
- Manage table occupancy and orders
- Load menu items from files
- Track order details including date/time
- Generate and optionally print/email receipts
- Save all data to files
- Create user-friendly console interface
- Implement robust error handling and data validation

IMPORTANT: Develop with unit testing in mind. All major functionalities 
should be covered by unit tests to ensure code reliability and ease of maintenance.

*/



namespace ConsoleApp07
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MenuManager menuManager = new MenuManager();
                TableManager tableManager = new TableManager();

                // Initialize tables
                for (int i = 1; i <= 10; i++)
                {
                    tableManager.AddTable(new Table(i, 4)); // Assuming all tables have a capacity of 4
                }

                // Load past orders
                tableManager.LoadPastOrders();

                while (true)
                {
                    Console.WriteLine("\nRestaurant Management System");
                    Console.WriteLine("1. Display Menu");
                    Console.WriteLine("2. Display Table Status");
                    Console.WriteLine("3. Display Current Orders");
                    Console.WriteLine("4. Occupy Table");
                    Console.WriteLine("5. Place Order");
                    Console.WriteLine("6. Free Table");
                    Console.WriteLine("7. Print Receipt");
                    Console.WriteLine("8. Email Receipt");
                    Console.WriteLine("9. View Order History");
                    Console.WriteLine("10. Exit");
                    Console.Write("Enter your choice: ");

                    string choice = Console.ReadLine();

                    try
                    {
                        switch (choice)
                        {
                            case "1":
                                menuManager.DisplayMenu();
                                break;
                            case "2":
                                tableManager.DisplayTableStatus();
                                break;
                            case "3":
                                OrderDisplay.DisplayOrders(tableManager);
                                break;
                            case "4":
                                OccupyTable(tableManager);
                                break;
                            case "5":
                                PlaceOrder(tableManager, menuManager);
                                break;
                            case "6":
                                FreeTable(tableManager);
                                break;
                            case "7":
                                PrintReceipt(tableManager);
                                break;
                            case "8":
                                EmailReceipt(tableManager);
                                break;
                            case "9":
                                ViewOrderHistory(tableManager);
                                break;
                            case "10":
                                return;
                            default:
                                Console.WriteLine("Invalid choice. Please try again.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError("An error occurred", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError("Error during initialization", ex);
                return;
            }
        }

        static void OccupyTable(TableManager tableManager)
        {
            Console.Write("Enter table number to occupy: ");
            if (InputValidator.ValidateInteger(Console.ReadLine(), out int tableNumber, 1))
            {
                Table table = tableManager.GetTable(tableNumber);
                if (table != null)
                {
                    try
                    {
                        table.Occupy();
                        Console.WriteLine($"Table {tableNumber} is now occupied.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid table number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid table number.");
            }
        }

        static void PlaceOrder(TableManager tableManager, MenuManager menuManager)
        {
            Console.Write("Enter table number to place order: ");
            if (InputValidator.ValidateInteger(Console.ReadLine(), out int tableNumber, 1))
            {
                Table table = tableManager.GetTable(tableNumber);
                if (table != null && table.IsOccupied && table.CurrentOrder != null)
                {
                    while (true)
                    {
                        Console.Write("Enter item name (or 'done' to finish): ");
                        string itemName = Console.ReadLine();
                        if (itemName.ToLower() == "done") break;

                        MenuItem item = menuManager.SelectItem(itemName);
                        if (item != null)
                        {
                            Console.Write("Enter quantity: ");
                            if (InputValidator.ValidateInteger(Console.ReadLine(), out int quantity, 1))
                            {
                                item.Quantity = quantity;
                                table.CurrentOrder.AddItem(item);
                                Console.WriteLine($"Added {quantity} {item.Name} to the order.");
                            }
                            else
                            {
                                Console.WriteLine("Invalid quantity. Item not added.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Item not found in the menu.");
                        }
                    }
                    Console.WriteLine($"Order placed for Table {tableNumber}. Total: ${table.CurrentOrder.CalculateTotal():F2}");
                    FileHandler.WriteOrderData(table.CurrentOrder);
                    ReceiptGenerator.PrintReceipt(table.CurrentOrder);
                }
                else
                {
                    Console.WriteLine("Invalid table number or table is not occupied.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid table number.");
            }
        }

        static void FreeTable(TableManager tableManager)
        {
            Console.Write("Enter table number to free: ");
            if (InputValidator.ValidateInteger(Console.ReadLine(), out int tableNumber, 1))
            {
                Table table = tableManager.GetTable(tableNumber);
                if (table != null)
                {
                    try
                    {
                        table.Free();
                        Console.WriteLine($"Table {tableNumber} is now free.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid table number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid table number.");
            }
        }

        static void PrintReceipt(TableManager tableManager)
        {
            Console.Write("Enter table number to print receipt: ");
            if (InputValidator.ValidateInteger(Console.ReadLine(), out int tableNumber, 1))
            {
                Table table = tableManager.GetTable(tableNumber);
                if (table != null && table.IsOccupied && table.CurrentOrder != null)
                {
                    ReceiptGenerator.PrintReceipt(table.CurrentOrder);
                }
                else
                {
                    Console.WriteLine("Invalid table number or table is not occupied.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid table number.");
            }
        }

        static void EmailReceipt(TableManager tableManager)
        {
            Console.Write("Enter table number to email receipt: ");
            if (InputValidator.ValidateInteger(Console.ReadLine(), out int tableNumber, 1))
            {
                Table table = tableManager.GetTable(tableNumber);
                if (table != null && table.IsOccupied && table.CurrentOrder != null)
                {
                    Console.Write("Enter email address: ");
                    string email = Console.ReadLine();
                    if (InputValidator.ValidateEmail(email))
                    {
                        ReceiptGenerator.EmailReceipt(table.CurrentOrder, email);
                    }
                    else
                    {
                        Console.WriteLine("Invalid email address.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid table number or table is not occupied.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid table number.");
            }
        }

        static void ViewOrderHistory(TableManager tableManager)
        {
            Console.Write("Enter table number to view order history: ");
            if (InputValidator.ValidateInteger(Console.ReadLine(), out int tableNumber, 1))
            {
                Table table = tableManager.GetTable(tableNumber);
                if (table != null)
                {
                    if (table.PastOrders.Count > 0)
                    {
                        Console.WriteLine($"Order history for Table {tableNumber}:");
                        foreach (var order in table.PastOrders)
                        {
                            Console.WriteLine($"Order Time: {order.OrderTime}");
                            Console.WriteLine($"Total: ${order.CalculateTotal():F2}");
                            Console.WriteLine("Items:");
                            foreach (var item in order.Items)
                            {
                                Console.WriteLine($"  - {item.Name} x{item.Quantity} - ${item.Price * item.Quantity:F2}");
                            }
                            Console.WriteLine(new string('-', 40));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No order history for Table {tableNumber}.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid table number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid table number.");
            }
        }
    }
    public class MenuManager
    {
        private List<MenuItem> _menuItems;

        public MenuManager()
        {
            _menuItems = new List<MenuItem>();
            LoadMenuItems();
        }

        public void LoadMenuItems()
        {
            _menuItems = FileHandler.ReadMenuItems();
        }

        public void SaveMenuItems()
        {
            FileHandler.WriteMenuItems(_menuItems);
        }

        public void DisplayMenu()
        {
            Console.WriteLine("Menu:");
            foreach (var item in _menuItems)
            {
                Console.WriteLine($"[ ] {item}");
            }
        }

        public MenuItem? SelectItem(string itemName)
        {
            return _menuItems.FirstOrDefault(item => item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        public void AddMenuItem(MenuItem item)
        {
            item.Id = _menuItems.Count > 0 ? _menuItems.Max(i => i.Id) + 1 : 1;
            _menuItems.Add(item);
            SaveMenuItems();
        }

        public void RemoveMenuItem(MenuItem item)
        {
            _menuItems.Remove(item);
            SaveMenuItems();
        }
    }
    public class Table
    {
        public int Number { get; }
        public int Capacity { get; }
        public bool IsOccupied { get; private set; }
        public Order? CurrentOrder { get; private set; }
        public List<Order> PastOrders { get; } = new List<Order>();

        public Table(int number, int capacity)
        {
            Number = number;
            Capacity = capacity;
            IsOccupied = false;
            CurrentOrder = null;
        }

        public void Occupy()
        {
            if (IsOccupied)
                throw new InvalidOperationException("Table is already occupied.");
            IsOccupied = true;
            CurrentOrder = new Order(this);
        }

        public void Free()
        {
            if (!IsOccupied)
                throw new InvalidOperationException("Table is not occupied.");
            IsOccupied = false;
            CurrentOrder = null;
        }

        public override string ToString()
        {
            return $"Table {Number} (Capacity: {Capacity}) - {(IsOccupied ? "Occupied" : "Free")}";
        }
    }
    public class Order
    {
        public Table Table { get; }
        public List<MenuItem> Items { get; }
        public DateTime OrderTime { get; set; }

        public Order(Table table)
        {
            Table = table;
            Items = new List<MenuItem>();
            OrderTime = DateTime.Now;
        }

        public void AddItem(MenuItem item)
        {
            Items.Add(item);
        }

        public void RemoveItem(MenuItem item)
        {
            Items.Remove(item);
        }

        public decimal CalculateTotal()
        {
            return Items.Sum(item => item.Price * item.Quantity);
        }

        public override string ToString()
        {
            return $"Order for Table {Table.Number} - {Items.Count} items, Total: ${CalculateTotal():F2}";
        }
    }
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }

        public MenuItem(int id, string name, decimal price, string category, int quantity = 0)
        {
            Id = id;
            Name = name;
            Price = price;
            Category = category;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return $"{Name} - ${Price:F2} ({Category})";
        }
    }
    public class FileHandler
    {
        private const string MenuFilePath = "menu.txt";
        private const string OrdersFilePath = "orders.txt";

        public static List<MenuItem> ReadMenuItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            if (File.Exists(MenuFilePath))
            {
                string[] lines = File.ReadAllLines(MenuFilePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 4)
                    {
                        int id = int.Parse(parts[0].Trim());
                        string name = parts[1].Trim();
                        decimal price = decimal.Parse(parts[2].Trim());
                        string category = parts[3].Trim();
                        menuItems.Add(new MenuItem(id, name, price, category));
                    }
                }
            }
            return menuItems;
        }

        public static void WriteMenuItems(List<MenuItem> menuItems)
        {
            using (StreamWriter writer = new StreamWriter(MenuFilePath))
            {
                foreach (var item in menuItems)
                {
                    writer.WriteLine($"{item.Id},{item.Name},{item.Price},{item.Category}");
                }
            }
        }

        public static void WriteOrderData(Order order)
        {
            using (StreamWriter writer = new StreamWriter(OrdersFilePath, true))
            {
                writer.WriteLine($"Order Time: {order.OrderTime}");
                writer.WriteLine($"Table: {order.Table.Number}");
                writer.WriteLine("Items:");
                foreach (var item in order.Items)
                {
                    writer.WriteLine($"- {item.Name}, ${item.Price:F2}, Quantity: {item.Quantity}");
                }
                writer.WriteLine($"Total: ${order.CalculateTotal():F2}");
                writer.WriteLine(new string('-', 40));
            }
        }

        public static List<Order> ReadOrderData(TableManager tableManager)
        {
            List<Order> orders = new List<Order>();
            if (File.Exists(OrdersFilePath))
            {
                string[] lines = File.ReadAllLines(OrdersFilePath);
                Order currentOrder = null;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.StartsWith("Order Time:"))
                    {
                        if (currentOrder != null)
                        {
                            orders.Add(currentOrder);
                        }
                        string dateTimeString = line.Substring("Order Time:".Length).Trim();
                        DateTime orderTime = DateTime.Parse(dateTimeString);
                        if (i + 1 < lines.Length && lines[i + 1].StartsWith("Table:"))
                        {
                            int tableNumber = int.Parse(lines[i + 1].Substring("Table:".Length).Trim());
                            Table table = tableManager.GetTable(tableNumber);
                            currentOrder = new Order(table) { OrderTime = orderTime };
                        }
                    }
                    else if (line.StartsWith("-"))
                    {
                        string[] parts = line.Substring(1).Split(',');
                        if (parts.Length == 3)
                        {
                            string name = parts[0].Trim();
                            decimal price = decimal.Parse(parts[1].Trim().Substring(1));
                            int quantity = int.Parse(parts[2].Substring("Quantity:".Length).Trim());
                            currentOrder?.AddItem(new MenuItem(0, name, price, "", quantity));
                        }
                    }
                }
                if (currentOrder != null)
                {
                    orders.Add(currentOrder);
                }
            }
            return orders;
        }
    }
    public class ReceiptGenerator
    {
        public static string GenerateReceipt(Order order)
        {
            StringBuilder receipt = new StringBuilder();
            receipt.AppendLine("===== Restaurant Receipt =====");
            receipt.AppendLine($"Date: {order.OrderTime}");
            receipt.AppendLine($"Table: {order.Table.Number}");
            receipt.AppendLine("-----------------------------");
            receipt.AppendLine("Items:");
            foreach (var item in order.Items)
            {
                receipt.AppendLine($"{item.Name} x{item.Quantity} - ${item.Price * item.Quantity:F2}");
            }
            receipt.AppendLine("-----------------------------");
            receipt.AppendLine($"Total: ${order.CalculateTotal():F2}");
            receipt.AppendLine("Thank you for dining with us!");
            receipt.AppendLine("=============================");

            return receipt.ToString();
        }

        public static void PrintReceipt(Order order)
        {
            string receipt = GenerateReceipt(order);
            Console.WriteLine("Printing receipt...");
            Console.WriteLine(receipt);
            Console.WriteLine("Receipt printed successfully.");
        }

        public static void EmailReceipt(Order order, string emailAddress)
        {
            string receipt = GenerateReceipt(order);
            Console.WriteLine($"Sending receipt to {emailAddress}...");
            // Simulate email sending
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine("Receipt sent successfully.");
        }
    }
    public class TableManager
    {
        private List<Table> _tables;

        public TableManager()
        {
            _tables = new List<Table>();
        }

        public void AddTable(Table table)
        {
            _tables.Add(table);
        }

        public Table? GetTable(int tableNumber)
        {
            return _tables.FirstOrDefault(t => t.Number == tableNumber);
        }

        public void DisplayTableStatus()
        {
            foreach (var table in _tables)
            {
                Console.WriteLine(table);
            }
        }

        public IEnumerable<Table> GetAllTables()
        {
            return _tables;
        }

        public void LoadPastOrders()
        {
            List<Order> pastOrders = FileHandler.ReadOrderData(this);
            foreach (var order in pastOrders)
            {
                Table table = GetTable(order.Table.Number);
                if (table != null)
                {
                    table.PastOrders.Add(order);
                }
            }
        }
    }
    public class OrderDisplay
    {
        public static void DisplayOrders(TableManager tableManager)
        {
            Console.WriteLine("Current Orders:");
            foreach (var table in tableManager.GetAllTables())
            {
                if (table.IsOccupied && table.CurrentOrder != null)
                {
                    Console.WriteLine($"Table {table.Number} (Capacity: {table.Capacity}):");
                    foreach (var item in table.CurrentOrder.Items)
                    {
                        Console.WriteLine($"  - {item.Name} x{item.Quantity} - ${item.Price * item.Quantity:F2}");
                    }
                    Console.WriteLine($"  Total: ${table.CurrentOrder.CalculateTotal():F2}");
                    Console.WriteLine();
                }
            }
        }
    }
    public class DateTimeManager
    {
        public static string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static bool TryParseDateTime(string input, out DateTime result)
        {
            return DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out result);
        }
    }
    public class InputValidator
    {
        public static bool ValidateInteger(string input, out int result, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            if (int.TryParse(input, out result))
            {
                return result >= minValue && result <= maxValue;
            }
            return false;
        }

        public static bool ValidateDecimal(string input, out decimal result, decimal minValue = decimal.MinValue, decimal maxValue = decimal.MaxValue)
        {
            if (decimal.TryParse(input, out result))
            {
                return result >= minValue && result <= maxValue;
            }
            return false;
        }

        public static bool ValidateString(string input, int minLength = 0, int maxLength = int.MaxValue)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length >= minLength && input.Length <= maxLength;
        }

        public static bool ValidateEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
    public class ErrorHandler
    {
        private const string LogFilePath = "error_log.txt";

        public static void LogError(string message, Exception ex = null)
        {
            string errorMessage = $"[{DateTimeManager.GetCurrentDateTime()}] ERROR: {message}";
            if (ex != null)
            {
                errorMessage += $"\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
            }

            Console.WriteLine(errorMessage);
            WriteToLogFile(errorMessage);
        }

        private static void WriteToLogFile(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine(message);
                    writer.WriteLine(new string('-', 50));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}