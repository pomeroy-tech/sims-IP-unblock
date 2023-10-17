using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: StartApp.exe <application> <parameter>");
            return;
        }
""
        string applicationPath = args[0];
        string parameter = args[1];

        StartApp(applicationPath, parameter);
    }

    static void StartApp(string applicationPath, string parameter)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = applicationPath,
                Arguments = parameter,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting the application: {ex.Message}");
        }
    }
}