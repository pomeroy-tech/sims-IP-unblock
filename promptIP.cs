using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using System.ComponentModel;
using System.Linq








;
class PromptIP
{
    static void Main()
    {
        string ipAddress;
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("dddd, MMMM dd yyyy HH:mm:ss");
        string curUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;       

        {
            FileLogger($"********** {formattedDateTime} *************");
            FileLogger(curUser);
            Console.WriteLine("These IP's are currently blocked");
            Console.WriteLine(GetBlockedIpAddresses());
            Console.WriteLine("Enter an IP address to unblock: ");
            do {
                ipAddress = Console.ReadLine();
                FileLogger(ipAddress);
            }
            while (!(IsValidIpAddress(ipAddress)));

            if (IsValidIpAddress(ipAddress))
            {
                UnblockIpAddress(ipAddress, curUser);
            }
            else
            {
                Console.WriteLine("Invalid IP address format. Please enter a valid IP address.");
                FileLogger("Invalid IP address format. Please enter a valid IP address.");
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
    static void FileLogger(string toBeLogged)
    {
        DateTime currentDateTime = DateTime.Now;
        string logdate = currentDateTime.ToString("yyyy-MM-dd");
        string log = $"c:\\sims_unblock\\logs\\SIMS_Unblock_IP_prompt_{logdate}.log";
        using (StreamWriter writer = new StreamWriter(log, true))
        {
             writer.WriteLine(toBeLogged);
        }
    }
    
    static void UnblockIpAddress(string ipAddress, string curUser)
    {

        try
        {
            string output = "";
            string[] userSplit;
            char[] delim = {'\\'};
            userSplit = (curUser.Split(delim, StringSplitOptions.None)).Skip(1).ToArray();
            string User = userSplit[0];
            using (StreamWriter unblocktemp = new StreamWriter($"c:\\sims_unblock\\temp\\{User}.out"))
            {
                unblocktemp.Write(ipAddress);
            }
            FileLogger(output);
            Console.WriteLine("Command executed successfully.");
            FileLogger("Command executed successfully.");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running the command: {ex.Message}");
            FileLogger($"Error running the command: {ex.Message}");
        }
    }
    static string GetBlockedIpAddresses()
    {
        try
        {   
            string output = "";
            do
            {
                if (File.Exists("c:\\sims_unblock\\temp\\blocked.in"))
                {
                    output = File.ReadAllText("c:\\sims_unblock\\temp\\blocked.in");
                    File.Delete("c:\\sims_unblock\\temp\\blocked.in");
                }
            }
            while (output == "");
            FileLogger(output);
            FileLogger("Blocked Ip addresses retrieved successfully.");
            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving the IPs: {ex.Message}");
            FileLogger($"Error retrieving the IPs: {ex.Message}");
            return $"Error retrieving the IPs: {ex.Message}";
        }
    }
}
