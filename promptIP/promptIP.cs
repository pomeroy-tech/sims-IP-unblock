using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Timers;
class PromptIP
{
public static FileSystemWatcher watcher;
public static System.Timers.Timer aTimer;
public static String curUser;
public static String User;
public static Boolean disconnect;
public static String installloc;
    static void Main()
    {
        disconnect = false;
        aTimer = new System.Timers.Timer();
        aTimer.Elapsed+=new ElapsedEventHandler(OnTimedEvent);
        aTimer.Interval=300000;
        aTimer.Enabled=true;
        string ipAddress;
        installloc = (AppDomain.CurrentDomain.BaseDirectory).Remove((AppDomain.CurrentDomain.BaseDirectory).Length-4);
        string curUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        string[] userSplit;
        char[] delim = {'\\'};
        userSplit = (curUser.Split(delim, StringSplitOptions.None)).Skip(1).ToArray();
        User = userSplit[0];
        FileLogger($"**********Login: {curUser} *************", "MAIN");
        
        do
        {
            string response;
            foreach (string respfile in Directory.GetFiles($"{installloc}\\temp"))
            {
                if (respfile == $"{installloc}\\temp\\{User}.response")
                {
                    response = File.ReadAllText(respfile);
                    if (String.Equals(response, @"timeout", StringComparison.OrdinalIgnoreCase))
                    {
                        FileLogger($"{User} backend has timedout -- Disconnecting", "BackendResponse");
                        Console.WriteLine("Your Session has timed out this SSH session will disconnect in 5 seconds");
                        Thread.Sleep(5000);
                        File.Delete(respfile);
                        disconnect = true;
                        break;
                    }
                }
            }
            Console.WriteLine("These IP's are currently blocked");
            Console.WriteLine(GetBlockedIpAddresses());
            Console.WriteLine("Enter an IP address to unblock or type quit to exit or r to refresh: ");
            ipAddress = Console.ReadLine();
            FileLogger(ipAddress, "MAIN");
            if (IsValidIpAddress(ipAddress))
            {
                Console.WriteLine(UnblockIpAddress(ipAddress));
            }
            else if (String.Equals(ipAddress, @"quit", StringComparison.OrdinalIgnoreCase))
            {
                FileLogger($"{curUser} typed QUIT", "MAIN");
                disconnect = true;
                DropConnection();
                break;
            }
            else if (String.Equals(ipAddress, @"r", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Refreshing IP Addresses.");
                FileLogger("Refreshing IP Addresses", "MAIN");
                continue;
            }
            else
            {
                Console.WriteLine("Invalid IP address format. Please enter a valid IP address.");
                FileLogger("Invalid IP address format. Please enter a valid IP address.", "MAIN");
            }
        }
        while (!(disconnect));
        FileLogger($"**********DISCONNECT: {curUser} *************", "MAIN");
        foreach (string file in Directory.GetFiles($"{installloc}\\temp"))
        {
            if (file == $"{installloc}\\temp\\{User}.*")
            {
                FileLogger($"Deleting Leftover file -- {file}", "MAIN");
                File.Delete(file);
            }
        }
    }

    static bool IsValidIpAddress(string ipAddress)
    {
        string ipPattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                           @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                           @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                           @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        return Regex.IsMatch(ipAddress, ipPattern);
    }
    static void FileLogger(string toBeLogged, string function)
    {
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("dddd, MMMM dd yyyy HH:mm:ss");
        string logdate = currentDateTime.ToString("yyyy-MM-dd");
        string log = $"{installloc}\\logs\\SIMS_Unblock_IP_PromptIP_{logdate}.log";
        using (StreamWriter writer = new StreamWriter(log, true))
        {
             writer.WriteLine($"{formattedDateTime} -- {function} -- {toBeLogged}");
             writer.Close();
        }
    }
    
    static string UnblockIpAddress(string ipAddress)
    {

        try
        {
            string output = "";
            using (StreamWriter unblocktemp = new StreamWriter($"{installloc}\\temp\\{User}.command"))
            {
                unblocktemp.Write(ipAddress);
            }
            FileLogger(output, "UNBLOCKIPADDESS");
            Console.WriteLine("Command executed successfully.");
            FileLogger("Command executed successfully.", "UNBLOCKIPADDRESS");
            do
            {
                if (File.Exists($"{installloc}\\temp\\{User}.fetch"))
                {
                    Thread.Sleep(4000);
                    output = File.ReadAllText($"{installloc}\\temp\\{User}.unblock");
                    File.Delete($"{installloc}\\temp\\{User}.unblock");
                }
            }
            while (output == "");
            FileLogger(output, "UNBLOCKIPADDRESS");
            FileLogger($"Blocked Ip addresses {ipAddress} unblocked successfully.", "UNBLOCKIPADDRESS");
            return output;
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running the command: {ex.Message}");
            FileLogger($"Error running the command: {ex.Message}", "UNBLOCKIPADDRESS");
            return $"Error running the command: {ex.Message}";
        }
        ;
    }
    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        FileLogger("Timer Expired Disconnecting", "ONTIMEDEVENT");
        DropConnection();
        disconnect = true;
        FileLogger($"Time Expired for ${User}", "ONTIMEDEVENT");
        FileLogger($"END EXECUTION for ${User}", "ONTIMEDEVENT");
        foreach (string file in Directory.GetFiles($"{installloc}\\temp"))
        {
            if (file == $"{installloc}\\temp\\{User}.*")
            {
                FileLogger($"Deleting Leftover file -- {file}", "ONTIMEDEVENT");
                File.Delete(file);
            }
        }
        Environment.Exit(0);
    }

    static string GetBlockedIpAddresses()
    {
        try
        {   
            using (StreamWriter get = new StreamWriter($"{installloc}\\temp\\{User}.command"))
            {
                get.Write("get");
            }
            string output = "";

            do
            {
                if (File.Exists($"{installloc}\\temp\\{User}.fetch"))
                {
                    Thread.Sleep(3000);
                    output = File.ReadAllText($"{installloc}\\temp\\{User}.fetch");
                    File.Delete($"{installloc}\\temp\\{User}.fetch");
                }
            }
            while (output == "");
            FileLogger(output, "GETBLOCKEDIPADDRESSES");
            FileLogger("Blocked Ip addresses retrieved successfully.", "GETBLOCKEDIPADDRESSES");
            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving the IPs: {ex.Message}");
            FileLogger($"Error retrieving the IPs: {ex.Message}", "GETBLOCKEDIPADDRESSES");
            return $"Error retrieving the IPs: {ex.Message}";
        }
    }
    static void DropConnection()
    {
        try
        {
            using (StreamWriter bye = new StreamWriter($"{installloc}\\temp\\{User}.command"))
            {
                bye.Write("quit");
            }
            Console.WriteLine("Command executed successfully.");
            FileLogger("Wrote Drop Connection Command.", "DROPCONNECTION");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running the command: {ex.Message}");
            FileLogger($"Error Writing Drop Connection Command: {ex.Message}", "DROPCONNECTION");
        }

    }
}
