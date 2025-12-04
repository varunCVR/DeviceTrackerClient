using System;
using System.Diagnostics;
using System.Threading;
using DeviceTrackerClient.Services;

namespace DeviceTrackerClient
{
    public static class TestAppBlocker
    {
        public static void RunTest()
        {
            Console.WriteLine("=== APP BLOCKER TEST ===");
            Console.WriteLine("Testing real-time process blocking...");

            // Create logger and blocker
            var logger = new LoggerService();
            var blocker = new AppBlockerService(logger);

            // Start the blocker
            blocker.Start();

            Console.WriteLine("\n1. Testing NOTEPAD blocking (should close immediately):");
            Console.WriteLine("   Opening notepad...");

            try
            {
                // Try to open notepad multiple times
                for (int i = 1; i <= 3; i++)
                {
                    Console.WriteLine($"   Attempt {i}: Opening notepad...");
                    Process.Start("notepad.exe");
                    Thread.Sleep(1000); // Wait a bit

                    // Check if notepad is running
                    var notepads = Process.GetProcessesByName("notepad");
                    if (notepads.Length == 0)
                    {
                        Console.WriteLine($"   ✓ Notepad was blocked successfully!");
                    }
                    else
                    {
                        Console.WriteLine($"   ✗ Notepad is still running!");
                        foreach (var p in notepads) p.Kill();
                    }

                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error: {ex.Message}");
            }

            Console.WriteLine("\n2. Testing CHROME blocking:");
            blocker.BlockByProcessName("chrome");
            Console.WriteLine("   Chrome blocking rule added.");

            Console.WriteLine("\n3. Testing CMD blocking:");
            blocker.BlockByProcessName("cmd");
            Console.WriteLine("   CMD blocking rule added.");

            Console.WriteLine("\n4. Checking current rules:");
            var rules = blocker.GetBlockRules();
            foreach (var rule in rules)
            {
                Console.WriteLine($"   - {rule.Name}: {rule.Pattern} ({rule.MatchType}) - {(rule.IsEnabled ? "ENABLED" : "DISABLED")}");
            }

            Console.WriteLine("\n=== TEST COMPLETE ===");
            Console.WriteLine("Check C:\\ProgramData\\DeviceTracker\\logs\\ for blocking events.");
            Console.WriteLine("Press any key to stop blocker...");
            Console.ReadKey();

            blocker.Stop();
        }

        public static void QuickTest()
        {
            Console.WriteLine("Quick test: Blocking notepad...");

            var logger = new LoggerService();
            var blocker = new AppBlockerService(logger);
            blocker.Start();

            // Add notepad block rule (if not already there)
            blocker.BlockByProcessName("notepad", exactMatch: false);

            Console.WriteLine("Try opening notepad now. It should close immediately.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}