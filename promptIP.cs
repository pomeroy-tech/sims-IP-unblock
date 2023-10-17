using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        Console.WriteLine("Enter an IP address: ");
        string ipAddress = Console.ReadLine();

        if (IsValidIpAddress(ipAddress))
        {
            RunCommandWithIpAddress(ipAddress);
        }
        else
        {
            Console.WriteLine("Invalid IP address format. Please enter a valid IP address.");
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

    static void RunCommandWithIpAddress(string ipAddress)
    {
        try
        {
            string command = $"PowerShell.exe -f run_as.ps1 -Command \"unblock.ps1 {ipAddress}\" -Username simsadmin -Password aVYX9zqG%XxIF^26sfTb -Wait";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine(output);
            }

            Console.WriteLine("Command executed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running the command: {ex.Message}");
        }
    }
}