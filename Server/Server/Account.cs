using System;
using System.IO;

namespace Server
{
    class Account
    {
        public static bool CreateAccount(string username, string password)
        {
            username = Util.MakeStringCooler(username);
            string accountResult = GetAccount(username, password);
            if (accountResult == "Account Not Found!")
            {
                File.AppendAllLines("C:/Users/Kullanici/Desktop/Server/Database/Accounts/Accounts.txt", new string[] { username + ":" + password });
                Console.WriteLine("Account Created!: " + username + ":" + password);
                return true;
            }
            else Console.WriteLine("Error: Account Exists!");
            return false;
        }
        public static string GetAccount(string username, string password)
        {
            string[] allAccounts = File.ReadAllLines("C:/Users/Kullanici/Desktop/Server/Database/Accounts/Accounts.txt");
            string Username = "";
            string Password = "";

            for (int i = 0; i < allAccounts.Length; i++)
            {
                int j = allAccounts[i].IndexOf(":");
                Username = allAccounts[i].Substring(0, j);
                Password = allAccounts[i].Substring(j + 1, allAccounts[i].Length - j - 1);
                if (username == Username)
                {
                    if (password == Password) return "Success!";
                    return "Wrong Password!";
                }
            }
            return "Account Not Found!";
        }
    }
}
