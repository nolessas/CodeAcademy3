using System.ComponentModel;
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



namespace ConsoleApp08
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
    public class MenuManager
    {
        //Loads food and drink items from files
        //Displays menu items with checkboxes and colors
        //Handles item selection


    }
    public class Table
    {
        //Properties: Number, Capacity, IsOccupied
        //Methods to occupy and free up tables
    }
    public class Order
    {
        //Links to a Table
        //Contains list of ordered items
        //Calculates total cost
    }
    public class MenuItem
    {
        //Properties: Name, Price, Category (food/drink), Quantity
    }
    public class FileHandler
    {
        //Reads menu items from files
        //Writes order data to files
    }
    public class ReceiptGenerator
    {
        //Creates receipts for restaurant and customer
        //Optionally sends receipts via email

    }
    public class TableManager
    {
        //Keeps track of all tables
        //Displays table status(free/occupied)
    }
    public class OrderDisplay
    {
        //Shows orders for each table
        //Includes table number, capacity, ordered items
    }
    public class DateTimeManager
    {
        //Handles date and time for orders
    }
    public class InputValidator
    {
        //Validates user inputs
    }
    public class ErrorHandler
    {
        //Manages and logs errors
    }
}
