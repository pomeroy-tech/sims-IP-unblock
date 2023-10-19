using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using System.ComponentModel;
using System.Linq;


class UnblockIP
{

    static void Main()
    {
        string ipAddress;
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("dddd, MMMM dd yyyy HH:mm:ss");

        {
            FileLogger($"********** {formattedDateTime} *************");
            FileLogger("Execute UnBlock");
            string[] files = Directory.GetFiles("c:\\sims_unblock\\temp");
            foreach (string file in files)
            {
                string[] fileSplit;
                char[] delim = {'.'};
                fileSplit = (file.Split(delim, StringSplitOptions.None)).Skip(1).ToArray();
                string FileExt = fileSplit[0];
                if (FileExt == "out")
                {
                    ipAddress = File.ReadAllText(file);
                    if (IsValidIpAddress(ipAddress))
                    {
                        UnblockIpAddress(ipAddress);
                        FileLogger(ipAddress);
                    }                    
                    else
                    {
                        FileLogger("Invalid IP Address");
                        FileLogger(ipAddress);
                    }
                    File.Delete(file);
                }
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
        string log = $"c:\\sims_unblock\\logs\\SIMS_Unblock_IP_unblock_{logdate}.log";
        using (StreamWriter writer = new StreamWriter(log, true))
        {
             writer.WriteLine(toBeLogged);
        }
    }
    
    static void UnblockIpAddress(string ipAddress)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\Bitvise SSH Server\\BssStat.exe",
                Arguments = $"-u {ipAddress}",
                WorkingDirectory = "C:\\Program Files\\Bitvise SSH Server",
                Verb = "runas",
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
                FileLogger(output);

            }

            Console.WriteLine("Command executed successfully.");
            FileLogger("Command executed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running the command: {ex.Message}");
            FileLogger($"Error running the command: {ex.Message}");
        }
    }
}

