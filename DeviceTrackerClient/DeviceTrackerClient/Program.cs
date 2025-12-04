using System;
using System.ServiceProcess;

namespace DeviceTrackerClient
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Check for test mode
            if (args.Length > 0 && args[0] == "/testblock")
            {
                TestAppBlocker.RunTest();
                return;
            }

            if (args.Length > 0 && args[0] == "/quicktest")
            {
                TestAppBlocker.QuickTest();
                return;
            }

            if (Environment.UserInteractive)
            {
                // Console mode for debugging
                Console.WriteLine("Device Tracker Client");
                Console.WriteLine("=====================");
                Console.WriteLine("1. Install as Service");
                Console.WriteLine("2. Run in Debug Mode");
                Console.WriteLine("3. Test App Blocker");
                Console.WriteLine("4. Exit");

                var choice = Console.ReadKey();
                Console.WriteLine();

                if (choice.KeyChar == '1')
                {
                    InstallService();
                }
                else if (choice.KeyChar == '2')
                {
                    var service = new DeviceTrackerService();
                    service.DebugRun();
                }
                else if (choice.KeyChar == '3')
                {
                    TestAppBlocker.RunTest();
                }
            }
            else
            {
                // Run as Windows Service
                ServiceBase.Run(new DeviceTrackerService());
            }
        }

        static void InstallService()
        {
            try
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
                System.Diagnostics.Process.Start("sc", $"create DeviceTrackerService binPath= \"{path}\" start= auto");
                Console.WriteLine("Service installed. Starting...");
                System.Diagnostics.Process.Start("sc", "start DeviceTrackerService");
                Console.WriteLine("Service started!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}