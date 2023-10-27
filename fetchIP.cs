using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection.Metadata;
using System.ComponentModel;


class FetchIP
{

    public static void Main()
    {
        
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("dddd, MMMM dd yyyy HH:mm:ss");
        FileLogger($"********** {formattedDateTime} *************");
        FileLogger("Execute Fetch");
        FetchIPAddress();

    }

    static void FileLogger(string toBeLogged)
    {
        DateTime currentDateTime = DateTime.Now;
        string logdate = currentDateTime.ToString("yyyy-MM-dd");
        string log = $"c:\\sims_unblock\\logs\\SIMS_Unblock_IP_fetch_{logdate}.log";
        using (StreamWriter writer = new StreamWriter(log, true))
        {
             writer.WriteLine(toBeLogged);
        }
    }
    
    static void FetchIPAddress()
    {
        try
        {
            using (StreamWriter fetch = new StreamWriter("c:\\sims_unblock\\temp\\blocked.in"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Program Files\\Bitvise SSH Server\\BssStat.exe",
                    Arguments = $"-i",
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
                    fetch.WriteLine(output);
                }
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

