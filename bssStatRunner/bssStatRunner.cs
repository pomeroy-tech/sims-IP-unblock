using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using System.Text.Json;
using System.Threading;

class BssStstRunner
{
public static FileSystemWatcher watcher;
public static System.Timers.Timer aTimer;
public static String curUser;
public static String User;
public static Boolean disconnect;
public static String installloc;

    static void Main()
    {
        installloc = (AppDomain.CurrentDomain.BaseDirectory).Remove((AppDomain.CurrentDomain.BaseDirectory).Length-4);
        var props = new Dictionary<string, string>();
        char[] eq = {'='};
        foreach (string line in (File.ReadAllLines($"{installloc}\\conf\\bssStatRunner.properties")))
            props.Add((line.Split(eq, StringSplitOptions.None)[0]), (line.Split(eq, StringSplitOptions.None)[1]));
        String eventjson = Environment.GetEnvironmentVariable("SSHLOGEVENT");
        //FileLogger(eventjson, "MAIN");
        disconnect = false;
        aTimer = new System.Timers.Timer();
        aTimer.Elapsed+=new ElapsedEventHandler(OnTimedEvent);
        aTimer.Interval=300000;
        aTimer.Enabled=true;
        DateTime currentDateTime = DateTime.Now;
        RotateLogs(Int32.Parse(props["Days_to_Keep_Logs"]));
        try
        {
            var sshlog = JsonSerializer.Deserialize<Dictionary<string, object>>(eventjson);
            //FileLogger(sshlog["event"].ToString(), "MAIN--EVENT");
            sshlog = JsonSerializer.Deserialize<Dictionary<string, object>>(sshlog["event"].ToString());
            //FileLogger(sshlog["conn"].ToString(), "MAIN--CONN");
            sshlog = JsonSerializer.Deserialize<Dictionary<string, object>>(sshlog["conn"].ToString());
            //FileLogger(sshlog["windowsAccount"].ToString(), "MAIN--curUser");
            curUser = sshlog["windowsAccount"].ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with the JSON: {ex.Message}");
            FileLogger($"Error with the JSON: {ex.Message}", "MAIN--SSHLOG");
            FileLogger(eventjson, "MAIN--SSHLOG");
        }

        string[] userSplit;
        char[] delim = {'\\'};
        userSplit = (curUser.Split(delim, StringSplitOptions.None)).Skip(1).ToArray();
        User = userSplit[0];
        FileLogger($"********** Begin {curUser} *************", "MAIN");
        FileLogger("Execute Fetch", "MAIN");
        FetchIPAddress();
        do
        {
            QueryForCommand();
            Thread.Sleep(2000);
        }
        while (!(disconnect));
        using (StreamWriter response = new StreamWriter($"{installloc}\\temp\\{User}.response"))
        {
             response.WriteLine("timeout");
             response.Close();
        }
    }

    private static void RotateLogs(int days)
    {
        FileLogger($"Rotating Logs older than {days} days old", "ROTATELOGS");
        foreach (string file in Directory.GetFiles($"{installloc}\\Logs"))
        {
            if (DateTime.Compare(File.GetCreationTime(file), DateTime.Now.Subtract(TimeSpan.FromDays(days))) <= 0 )
            {
                File.Delete(file);
                FileLogger($"Deleting:  {file}", "ROTATELOGS");
            }

        }
    }
    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        FileLogger("Timer Expired Disconnecting", "ONTIMEDEVENT");
        disconnect = true;
    }
    private static void QueryForCommand()
    {
        string command;
        foreach (string file in Directory.GetFiles($"{installloc}\\temp"))
        {
            if (file == $"{installloc}\\temp\\{User}.command")
            {
                command = File.ReadAllText(file);
                if (String.Equals(command, @"quit", StringComparison.OrdinalIgnoreCase))
                {
                    FileLogger($"{User} has typed quit -- Disconnecting", "QueryForCommand");
                    disconnect = true;
                }
                if (String.Equals(command, @"get", StringComparison.OrdinalIgnoreCase))
                {
                    FileLogger($"{User} has refreshed blocked IPs", "QueryForCommand");
                    FetchIPAddress();
                }
                else if (IsValidIpAddress(command))
                {
                    UnblockIpAddress(command);
                    FileLogger(command, "QueryForCommand");
                }                    
                else
                {
                    FileLogger("Invalid Command", "QueryForCommand");
                    FileLogger(command, "QueryForCommand");
                }
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
        string log = $"{installloc}\\logs\\SIMS_Unblock_IP_Runner_{logdate}.log";
        using (StreamWriter writer = new StreamWriter(log, true))
        {
             writer.WriteLine($"{formattedDateTime} -- {function} -- {toBeLogged}");
             writer.Close();
        }
    }
    
    static void FetchIPAddress()
    {
        try
        {
            using (StreamWriter fetch = new StreamWriter($"{installloc}\\temp\\{User}.fetch"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Program Files\\Bitvise SSH Server\\BssStat.exe",
                    Arguments = "-i",
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
                    FileLogger(output, "FETCHIPADDRESS");
                    fetch.WriteLine(output);
                    fetch.Close();
                }
            }

            Console.WriteLine("Command executed successfully.");
            FileLogger("Command executed successfully.", "FETCHIPADDRESS");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running the command: {ex.Message}");
            FileLogger($"Error running the command: {ex.Message}", "FETCHIPADDRESS");
        }
    }
    static void UnblockIpAddress(string ipAddress)
    {
        try
        {
            using (StreamWriter unblock = new StreamWriter($"{installloc}\\temp\\{User}.unblock"))
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
                    FileLogger(output, "UNBLOCKIPADDRESS");
                    unblock.WriteLine(output);
                    unblock.Close();

                }

                Console.WriteLine("Command executed successfully.");
                FileLogger("BSSStat Command executed successfully.", "UNBLOCKIPADDRESS");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running the command: {ex.Message}");
            FileLogger($"Error running the command: {ex.Message}", "UNBLOCKIPADDRESS");
        }
        FetchIPAddress();
    }
}

