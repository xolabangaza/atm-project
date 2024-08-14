using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FinalAtmProject
{
    class Program
    {
       public static string ConnectionString = @"Data Source=DESKTOP-6RT5AA5;Initial Catalog=XolasAtm;Integrated Security=True;Encrypt=False";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the ATM");
            Console.Write("Please enter your PIN: ");
            string pin = Console.ReadLine();


            int userId = AuthenticateUser(pin, out string userName);

            if (userId != -1)
            {
                Console.WriteLine("Welcome " + userName);
                while (true)
                {
                    Console.WriteLine("1. Deposit Money");
                    Console.WriteLine("2. Withdraw Money");
                    Console.WriteLine("3. View Balance");
                    Console.WriteLine("4. Exit");
                    Console.Write("Please select an option: ");
                    string option = Console.ReadLine();
                    Console.WriteLine("\n"+"WHICH ACCOUNT YOU WANT TO USE? ");

                    switch (option)
                    {
                        case "1":
                            DepositMoney(userId);
                            break;
                        case "2":
                            WithdrawMoney(userId);
                            break;
                        case "3":
                            ViewBalance(userId);
                            break;
                        case "4":
                            return;
                        default:
                            Console.WriteLine("Invalid option, please try again.");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid PIN, access denied.");
            }
        }

        static int AuthenticateUser(string pin, out string userName)
        {
            userName = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT UserID, UserName FROM Users WHERE Pin = @Pin";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Pin", pin);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(0);
                                userName = reader.GetString(1);
                                return userId;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }

         }
            
           
        
        static int SelectAccount(int userId, out decimal balance)
        {
            balance = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT AccountID, AccountNumber, Balance FROM Accounts WHERE UserID = @UserID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                int accountId = reader.GetInt32(0);
                                string accountNumber = reader.GetString(1);
                                balance = reader.GetDecimal(2);
                                Console.WriteLine("________________________________________ "+ "\n" + "\n" + "AccountID: " + accountId + ". Account Number: " + accountNumber + "\n" + "________________________________________ " + "\n");
                            }

                            Console.Write("Enter AccountID to Select an account:");
                            int accID = int.Parse(Console.ReadLine());

                            query = "SELECT AccountID, Balance FROM Accounts WHERE AccountID = @AccountID";
                            using (SqlCommand cmd = new SqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@AccountID", accID);
                                //cmd.Parameters.AddWithValue("@UserID", userId);
                                using (SqlDataReader rdr = cmd.ExecuteReader())
                                {
                                    if (rdr.Read())
                                    {
                                        balance = rdr.GetDecimal(1);
                                        return rdr.GetInt32(0);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Invalid account selection.");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No accounts found.");
                            return -1;
                        }
                    }
                }
            }
            return -1;
        }

        static void DepositMoney(int userId)
        {
            int accountId = SelectAccount(userId, out decimal balance);
            if (accountId == -1) return;

            Console.Write("Enter amount to deposit: ");
            decimal amount = decimal.Parse(Console.ReadLine());

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string updateBalanceQuery = "UPDATE Accounts SET Balance = Balance + @Amount WHERE AccountID = @AccountID";
                using (SqlCommand command = new SqlCommand(updateBalanceQuery, connection))
                {
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@AccountID", accountId);
                    command.ExecuteNonQuery();
                }

                string insertTransactionQuery = "INSERT INTO Transactions (AccountID, TransactionType, Amount, TransactionDateTime) VALUES (@AccountID, @TransactionType, @Amount, @TransactionDateTime)";
                using (SqlCommand command = new SqlCommand(insertTransactionQuery, connection))
                {
                    command.Parameters.AddWithValue("@AccountID", accountId);
                    command.Parameters.AddWithValue("@TransactionType", "Deposit");
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@TransactionDateTime", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }

            Console.WriteLine("Deposit successful.");
        }

        static void WithdrawMoney(int userId)
        {
            int accountId = SelectAccount(userId, out decimal balance);
            if (accountId == -1) return;

            Console.Write("Enter amount to withdraw: ");
            decimal amount = decimal.Parse(Console.ReadLine());

            if (balance >= amount)
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string updateBalanceQuery = "UPDATE Accounts SET Balance = Balance - @Amount WHERE AccountID = @AccountID";
                    using (SqlCommand command = new SqlCommand(updateBalanceQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Amount", amount);
                        command.Parameters.AddWithValue("@AccountID", accountId);
                        command.ExecuteNonQuery();
                    }

                    string insertTransactionQuery = "INSERT INTO Transactions (AccountID, TransactionType, Amount, TransactionDateTime) VALUES (@AccountID, @TransactionType, @Amount, @TransactionDateTime)";
                    using (SqlCommand command = new SqlCommand(insertTransactionQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccountID", accountId);
                        command.Parameters.AddWithValue("@TransactionType", "Withdrawal");
                        command.Parameters.AddWithValue("@Amount", amount);
                        command.Parameters.AddWithValue("@TransactionDateTime", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("Withdrawal successful.");
            }
            else
            {
                Console.WriteLine("Insufficient funds.");
            }
        }

        static void ViewBalance(int userId)
        {
            int accountId = SelectAccount(userId, out decimal balance);
            if (accountId == -1) return;

            Console.WriteLine("Current balance: " + balance);
        }

    }

}