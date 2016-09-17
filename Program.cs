using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Microsoft.SqlServer.Server;
using System.Data;

namespace SW_Arch_Project
{
    class Program
    {
        //Create string to aid in database creation
        static string connectionString = "Data Source=" + System.AppDomain.CurrentDomain.BaseDirectory + "newdatabase.db;Version=3;";

        //Create Connection to Database
        SQLiteConnection swad_db = new SQLiteConnection(connectionString);

        //String to store credit card number
        string ccNum;

        //String to store address
        string address;

        //String to store user inputed username
        string username;

        //List of names returned from database
        List<string> names = new List<string>();

        //list of items returned from database
        List<string> items = new List<string>();

        //list of prices from database
        List<float> price = new List<float>();

        //list for quantity of each item in database
        List<int> quantity = new List<int>();

        //Stores information of items added to cart
        Dictionary<string, int> cart = new Dictionary<string, int>();

        bool loop;

        //Function used for adding items to cart
        //Users can also choose to check out, which is called from this function
        //If item selected are already in cart, this function asks users whether they want to add or remove the item
        void add_items()
        {
            Console.WriteLine("Enter item # to add to or remove from the cart\n - or - \nPress C to checkout.");
            //user_input used to check what users choose to add to cart or if they choose to check out
            string user_input = Console.ReadLine().ToLower();
            //If c is chosen by user, confirm_purchase function is called
            if (user_input == "c")
            {
                Console.Clear();
                confirm_purchase();
            }
            else
            {
                //input quantity used to get user input for quantity of item to order
                int input_quantity = 0;
                //Open database
                swad_db.Open();
                //Select all entries from the item table
                string sql = "SELECT * FROM item";
                SQLiteCommand command = new SQLiteCommand(sql, swad_db);
                SQLiteDataReader reader = command.ExecuteReader();
                string item_db_quantity = "0";
                {
                    //Read through the entries in the item table
                    while (reader.Read())
                    {
                        if (user_input == reader["itemID"].ToString())
                        {
                            //if the item selected is already in the users cart, the system will ask whether they want to add or remove item
                            if (cart.ContainsKey(reader["itemName"].ToString()))
                            {
                                Console.WriteLine("This item is currently in you cart. Would you like to add or remove this item? (A/r)");
                                //while loop used to make sure user enters either "a" or "r"
                                loop = true;
                                while (loop == true)
                                {
                                    string conf = Console.ReadLine().ToLower();
                                    //If user enters "a", items are added to the cart
                                    if (conf == "a" || conf == "")
                                    {
                                        Console.WriteLine("How many would you like to purchase: ");
                                        input_quantity = Convert.ToInt32(Console.ReadLine());
                                        //check to make sure quantity is not less than 0
                                        if (Convert.ToInt32(reader["quantity"]) - input_quantity < 0)
                                            input_quantity = Convert.ToInt32(reader["quantity"]);
                                        //increase quantity of item in cart
                                        cart[reader["itemName"].ToString()] += input_quantity;
                                        //increase quantity of item in cart
                                        item_db_quantity = (Convert.ToInt32(reader["quantity"].ToString()) - input_quantity).ToString();
                                        //make loop false since user entered correct value
                                        loop = false;
                                    }
                                    //If user enters "r", items are removed from the cart
                                    else if (conf == "r")
                                    {
                                        Console.WriteLine("How many would you like to remove: ");
                                        input_quantity = Convert.ToInt32(Console.ReadLine());
                                        if (input_quantity > cart[reader["itemName"].ToString()])
                                            input_quantity = cart[reader["itemName"].ToString()];
                                        cart[reader["itemName"].ToString()] -= input_quantity;
                                        item_db_quantity = (Convert.ToInt32(reader["quantity"].ToString()) + input_quantity).ToString();
                                        //if the item is out, remove it from cart
                                        if (cart[reader["itemName"].ToString()] < 1)
                                        {
                                            cart.Remove(reader["itemName"].ToString());
                                        }
                                        //make loop false since user entered correct value
                                        loop = false;
                                    }
                                    else
                                    {
                                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                                        ClearCurrentConsoleLine();
                                    }
                                }
                            }
                            //if the item selected was not in the cart, the system asks how many to add and adds them to the cart and updates database
                            else
                            {
                                //Get the quantity of items from user input
                                Console.WriteLine("How many would you like to purchase: ");
                                input_quantity = Convert.ToInt32(Console.ReadLine());
                                //System makes sure the quantity is not below 0
                                if (Convert.ToInt32(reader["quantity"]) - input_quantity < 0)
                                    input_quantity = Convert.ToInt32(reader["quantity"]);
                                //add item to cart
                                cart.Add(reader["itemName"].ToString(), input_quantity);
                                Console.WriteLine("The item " + reader["itemName"] + " has been added to the cart");
                                item_db_quantity = (Convert.ToInt32(reader["quantity"].ToString()) - input_quantity).ToString();
                                //if the item is out, remove it from cart
                                if (cart[reader["itemName"].ToString()] < 1)
                                {
                                    cart.Remove(reader["itemName"].ToString());
                                }
                            }
                        }
                    }
                }
                //close database and display items after function has ran
                swad_db.Close();
                display_items();
            }
        }

        //getUsernames()  retrieves the list of usernames from the databse
        void getUsernames()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                try
                {
                    con.Open();

                    if (con.State == ConnectionState.Open)
                    {
                        string stm = "select username from user";

                        using (SQLiteCommand cmd = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    names.Add(rdr.GetString(0));
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: ");
                    Console.WriteLine(ex);
                }
            }
        }

        //getPrices() retrieves the list of prices from the database
        void getPrices()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                try
                {
                    con.Open();

                    if (con.State == ConnectionState.Open)
                    {
                        string stm = "SELECT price FROM item";

                        using (SQLiteCommand cmd = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    price.Add(rdr.GetFloat(0));
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: ");
                    Console.WriteLine(ex);
                }
            }
        }
        //get_purchases() retrieves the past purchases the user has made
        void get_purchases()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {

                SQLiteCommand cmd = new SQLiteCommand(connectionString);

                string stm = ("SELECT username, itemname, creditinfo, price FROM pastpurchases");
                con.Open();

                using (cmd = new SQLiteCommand(stm, con))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            if (rdr.GetString(0) == username)
                            {
                                Console.WriteLine(rdr.GetString(0) + " " + rdr.GetValue(1) + " " + rdr.GetValue(2) + " " + rdr.GetValue(3));
                            }
                        }
                    }


                    con.Close();
                }
            }
            Console.WriteLine("\nPress any key to return to the main menu.");
        }

        //login() logs the user into the system. It checks to make sure the username is in the database before letting the user in the system.
        void login()
        {
            //user inputs username
            Console.Write("Username: ");
            username = Console.ReadLine().ToLower();

            //retrieve usernames from database
            getUsernames();

            //ensure username is in database
            if (names.Contains(username))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Access Granted");
                Console.ResetColor();
                System.Threading.Thread.Sleep(200);
                display_menu();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Try again");
                Console.ResetColor();
                System.Threading.Thread.Sleep(300);
                Console.Clear();
                login();
            }
        }
        //display_menu() displays the menu for the user and calls on other functions based on user input
        void display_menu()
        {
            //display choices for user
            Console.Clear();
            Console.WriteLine("1. Shop");
            Console.WriteLine("2. Past Purchases");
            Console.WriteLine("3. Logout\n");

            //retrieve users input for option
            string number;
            Console.Write("Enter Number: ");
            number = Console.ReadLine();

            //if users choose to shop, display the items they can choose from
            if (number == "1")
            {
                display_items();
            }

            //if users choose to view past purchases, call get_purchases() function
            if (number == "2")
            {
                Console.Clear();
                Console.WriteLine("Your Past Purchases: \n");
                get_purchases();
                Console.ReadKey();
                display_menu();
            }
            if (number == "3")
            {
                Console.Clear();
                login();
            }
        }
        //display_shoppingcart() loops through cart dictionary, printing out the items in the cart
        void display_shoppingcart()
        {
            getPrices();
            Console.WriteLine("Current Cart:\n");

            float total = 0;

            foreach (KeyValuePair<string, int> kvp in cart)
            {
                Console.WriteLine("Item = {0}, Quantity = {1}", kvp.Key, kvp.Value);

                for (int i = 0; i < items.Count; i++)
                {
                    if (kvp.Key == items[i])
                    {
                        Console.WriteLine("$" + price[i] * kvp.Value);
                        total = total + (price[i] * kvp.Value);
                    }
                }
            }

            Console.WriteLine("\nTotal:\t$" + total);
        }
        //display_items() retrieves the items from the database and displays them to the user
        //it then prints the shopping cart, it takes the user to add items
        void display_items()
        {
            Console.Clear();
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                try
                {
                    con.Open();

                    if (con.State == ConnectionState.Open)
                    {
                        string stm = "SELECT category, itemName, quantity, price FROM item";

                        using (SQLiteCommand cmd = new SQLiteCommand(stm, con))
                        {
                            using (SQLiteDataReader rdr = cmd.ExecuteReader())
                            {
                                int i = 1;

                                items.Clear();

                                while (rdr.Read())
                                {
                                    items.Add(rdr.GetString(1));
                                    quantity.Add(rdr.GetInt32(2));

                                    Console.WriteLine(i + ". " + rdr.GetString(0) + " " + rdr.GetString(1) + " "
                                    + rdr.GetInt32(2) + " " + rdr.GetFloat(3));

                                    i++;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: ");
                    Console.WriteLine(ex);
                }

                Console.WriteLine("-----------------------------------------");

                display_shoppingcart();

                Console.WriteLine("-----------------------------------------");

                add_items();
            }
        }
        //confirm_purchase() gathers the users shipping address and credit card information and stores them to the database
        //it also gets one final check from the user to make sure they want to complete the transaction
        void confirm_purchase()
        {
            string confPurchase;
            bool ccNumCheck = true;
            display_shoppingcart();
            Console.WriteLine("Enter Shipping Address: \n");
            address = Console.ReadLine();
            Console.WriteLine("Shipping Address Saved!\n");
            // -------------------------------
            //Use address to manipulate database
            //-------------------------------



            //loop used to ensure valid 10 digit credit card
            while (ccNumCheck == true)
            {

                ccNumCheck = false; //set to false so it exits loop if nothing resets it to true

                Console.WriteLine("Enter Valid 10 Digit Credit Card Number: \n");
                ccNum = Console.ReadLine();

                //check each character in ccNUM
                foreach (char c in ccNum)
                {
                    if (c < '0' || c > '9')
                    {
                        ccNumCheck = true;
                    }
                }
                if (ccNum.Length != 10)
                {
                    ccNumCheck = true;
                }
                if (ccNumCheck == true)
                { Console.WriteLine("Credit Card Number Not Accepted! \n"); }
            }

            // -------------------------------
            //Use CC Number to manipulate database
            //-------------------------------

            Console.Clear();
            display_shoppingcart();
            Console.WriteLine("Confirm Purhcase? (Y/n) ");
            confPurchase = Console.ReadLine().ToLower();
            loop = true;
            //loop used to ensure "y" or "n" is entered
            while (loop == true)
            {
                if (confPurchase == "y" || confPurchase == "")
                {
                    save_purchases();
                    //save past purchase to database
                    Console.WriteLine("Thank you for shopping with us! Your items should be delivered soon.\n\n");
                    cart.Clear();
                    System.Threading.Thread.Sleep(500);
                    display_menu();
                }
                else if (confPurchase == "n")
                {
                    display_menu();
                }
                else
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ClearCurrentConsoleLine();
                }
            }
        }

        //The main function creates an object of the program class. It calls the login function on the object.
        // The login function calls display_menu,which calls display_items, which calls add_items, which calls confirm_purchase
        static void Main(string[] args)
        {
            Program obj = new Program();

            obj.login();

            Console.ReadLine();
        }
        //Used to clear the console line
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor);
        }
        //save the purchase to the database for  when the user wants to view their past purchases
        void save_purchases()
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionString))
            {
                try
                {
                    SQLiteCommand cmd = new SQLiteCommand();

                    con.Open();
                    cmd.Connection = con;
                    cmd.CommandText = @"insert into pastPurchases (itemName, creditInfo, address, price, username) values (@itemName, @creditinfo, @address, @price, @username)";

                    foreach (KeyValuePair<string, int> entry in cart)
                    {
                        int j = 0;
                        for (int i = 0; i < items.Count(); i++)
                        {
                            Console.WriteLine(items[i]);
                            Console.WriteLine(entry.Key);
                            if (items[i] == entry.Key)
                                j = i;
                        }
                        
                        cmd.Parameters.Add(new SQLiteParameter("@itemName", entry.Key));
                        cmd.Parameters.Add(new SQLiteParameter("@creditInfo", ccNum));
                        cmd.Parameters.Add(new SQLiteParameter("@address", address));
                        cmd.Parameters.Add(new SQLiteParameter("@price", (entry.Value * price[j]).ToString()));
                        cmd.Parameters.Add(new SQLiteParameter("@username", username));

                        cmd.ExecuteNonQuery();

                        Console.WriteLine(j);
                        Console.WriteLine(quantity[j]);

                        //update database
                        string sql2 = "UPDATE item SET quantity='" + (quantity[j] - entry.Value) + "' WHERE  itemName='" + entry.Key + "'";
                        SQLiteCommand command2 = new SQLiteCommand(sql2, con);
                        command2.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: ");
                    Console.WriteLine(ex);
                }
            }
        }
    }
}






