using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactoring
{

    public class Tusc
    {

        static string name;
        static string password;
        static List<User> userList;
        static List<Product> productListing; 

        public static void Start(List<User> usersList, List<Product> productList)
        {

            userList = usersList;
            productListing = productList;

            Console.WriteLine("Welcome to TUSC");
            Console.WriteLine("---------------");

            bool loggedIn = false; // Is logged in?
            // Login
            Login:
            
            name = promptForUserName();

            bool validUser = false; 
            if (!string.IsNullOrEmpty(name))
            {
                validUser = isUserInList();

                if (validUser)
                {
                    password = promptForUserPassword();

                    bool validPassword = isPasswordValid();

                    if (validPassword == true)
                    {
                        loggedIn = true;

                        // Show welcome message
                        initializeConsole(ConsoleColor.Green);
                        Console.WriteLine("Login successful! Welcome " + name + "!");
                        Console.ResetColor();
                        
                        double balance = showRemainingBalance();

                        processRequests(ref balance);

                        return;
                    }
                    else
                    {
                        // Invalid Password
                        initializeConsole(ConsoleColor.Red);
                        Console.WriteLine("You entered an invalid password.");
                        Console.ResetColor();

                        goto Login;
                    }
                }
                else
                {
                    // Invalid User
                    initializeConsole(ConsoleColor.Red);
                    Console.WriteLine("You entered an invalid user.");
                    Console.ResetColor();

                    goto Login;
                }
            }

            // Prevent console from closing
            Console.WriteLine();
            Console.WriteLine("Press Enter key to exit");
            Console.ReadLine();
        }

        private static void processRequests(ref double balance)
        {
            int numberOfProductRequested;
            string answer;
            // Show product list
            while (true)
            {
                // Prompt for user input
                Console.WriteLine();
                Console.WriteLine("What would you like to buy?");
                displayProductList();
                numberOfProductRequested = requestNumberOfProduct();

                // Check if user entered number that equals product count
                if (numberOfProductRequested == productListing.Count)
                {
                    // Update balance
                    updateBalance(balance);

                    // Write out new balance
                    updateJsonFiles();

                    // Prevent console from closing
                    Console.WriteLine();
                    Console.WriteLine("Press Enter key to exit");
                    Console.ReadLine();
                    return;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("You want to buy: " + productListing[numberOfProductRequested].Name);
                    Console.WriteLine("Your balance is " + balance.ToString("C"));

                    // Prompt for user input
                    Console.WriteLine("Enter amount to purchase:");
                    answer = Console.ReadLine();
                    int quantity = Convert.ToInt32(answer);
                    bool continueProcessing; 
                    // Check if balance - quantity * price is less than 0
                    continueProcessing = validateSale(balance, numberOfProductRequested, quantity);

                    if (continueProcessing)
                    {
                        continue; 
                    }

                    // Check if quantity is greater than zero
                    if (quantity > 0)
                    {
                        // Balance = Balance - Price * Quantity
                        balance = finalizeSale(balance, numberOfProductRequested, quantity);
                    }
                    else
                    {
                        // Quantity is less than zero
                        initializeConsole(ConsoleColor.Yellow);
                        Console.WriteLine("Purchase cancelled");
                        Console.ResetColor();
                    }
                }
            }
        }

        private static double finalizeSale(double balance, int numberOfProductRequested, int quantity)
        {
            balance = balance - productListing[numberOfProductRequested].Price * quantity;

            // Quanity = Quantity - Quantity
            productListing[numberOfProductRequested].Qty = productListing[numberOfProductRequested].Qty - quantity;

            initializeConsole(ConsoleColor.Green);
            Console.WriteLine("You bought " + quantity + " " + productListing[numberOfProductRequested].Name);
            Console.WriteLine("Your new balance is " + balance.ToString("C"));
            Console.ResetColor();
            return balance;
        }

        private static bool validateSale(double balance, int numberOfProductRequested, int quantity)
        {
            if (balance - productListing[numberOfProductRequested].Price * quantity < 0)
            {
                initializeConsole(ConsoleColor.Red);
                Console.WriteLine("You do not have enough money to buy that.");
                Console.ResetColor();
                return true; 
            }


            // Check if quantity is less than quantity
            if (productListing[numberOfProductRequested].Qty < quantity)
            {
                initializeConsole(ConsoleColor.Red);
                Console.WriteLine("Sorry, " + productListing[numberOfProductRequested].Name + " is out of stock");
                Console.ResetColor();
                return true; 
            }

            return false; 
        }

        private static void updateJsonFiles()
        {
            string json = JsonConvert.SerializeObject(userList, Formatting.Indented);
            File.WriteAllText(@"Data/Users.json", json);

            // Write out new quantities
            string json2 = JsonConvert.SerializeObject(productListing, Formatting.Indented);
            File.WriteAllText(@"Data/Products.json", json2);
        }

        private static int requestNumberOfProduct()
        {
            string answer;
            int numberOfProductRequested;
            
            Console.WriteLine("Enter a number:");
            answer = Console.ReadLine();
            numberOfProductRequested = Convert.ToInt32(answer);
            numberOfProductRequested = numberOfProductRequested - 1;

            return numberOfProductRequested; 
        }

        private static void updateBalance(double balance)
        {
            foreach (var user in userList)
            {
                // Check that name and password match
                if (user.Name == name && user.Pwd == password)
                {
                    user.Bal = balance;
                }
            }
        }

        private static void initializeConsole(ConsoleColor color)
        {
            Console.Clear();
            Console.ForegroundColor = color;
            Console.WriteLine();
        }

        private static double showRemainingBalance()
        {
            double balance = 0;
            for (int userRow = 0; userRow < 5; userRow++)
            {
                User user = userList[userRow];

                // Check that name and password match
                if (user.Name == name && user.Pwd == password)
                {
                    balance = user.Bal;

                    // Show balance 
                    Console.WriteLine();
                    Console.WriteLine("Your balance is " + user.Bal.ToString("C"));
                }
            }
            return balance;
        }

        private static string promptForUserPassword()
        {
            Console.WriteLine("Enter Password:");
            string pwd = Console.ReadLine();
            return pwd;
        }

        private static bool isPasswordValid()
        {
            bool valPwd = false; 
            for (int userRow = 0; userRow < userList.Count; userRow++)
            {
                User user = userList[userRow];

                if (user.Name == name && user.Pwd == password)
                {
                    valPwd = true;
                }
            }
            return valPwd;
        }

        private static string promptForUserName()
        {
            Console.WriteLine();
            Console.WriteLine("Enter Username:");
            string name = Console.ReadLine();
            return name;
        }

        private static bool isUserInList()
        {
            bool valUsr = false; 
            for (int userRow = 0; userRow < userList.Count; userRow++)
            {
                User user = userList[userRow];
                // Check that name matches
                if (user.Name == name)
                {
                    valUsr = true;
                }
            }
            return valUsr;
        }

        private static void displayProductList()
        {
            for (int productRow = 0; productRow < productListing.Count; productRow++)
            {
                Product product = productListing[productRow];
                Console.WriteLine(productRow + 1 + ": " + product.Name + " (" + product.Price.ToString("C") + ")");
            }
            Console.WriteLine(productListing.Count + 1 + ": Exit");
        }
    }
}
