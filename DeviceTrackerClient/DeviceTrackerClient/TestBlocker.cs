using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DeviceTrackerClient
{
    public class TestBlocker
    {
        public static void TestNotepadBlocking()
        {
            Console.WriteLine("=== Testing Notepad Blocking ===");
            Console.WriteLine("Creating block rule for notepad...");

            // Create a simple blocker
            var blocker = new SimpleAppBlocker();
            blocker.AddBlockRule("notepad", "ProcessName");

            Console.WriteLine("Rule added. Starting blocker monitoring...");
            blocker.Start();

            Console.WriteLine("Opening Notepad in 2 seconds...");
            Thread.Sleep(2000);

            Process.Start("notepad.exe");

            Console.WriteLine("Notepad launched. It should close within 3 seconds if blocking works.");
            Console.WriteLine("Press any key to stop testing...");
            Console.ReadKey();

            blocker.Stop();
        }
    }

    public class SimpleAppBlocker
    {
        private System.Threading.Timer _timer;
        private bool _blockNotepad = false;

        public void AddBlockRule(string pattern, string matchType)
        {
            if (pattern.ToLower().Contains("notepad"))
            {
                _blockNotepad = true;
                Console.WriteLine($"Block rule added: {pattern} ({matchType})");
            }
        }

        public void Start()
        {
            _timer = new System.Threading.Timer(CheckProcesses, null, 0, 1000); // Check every second
        }

        public void Stop()
        {
            _timer?.Dispose();
        }

        private void CheckProcesses(object state)
        {
            try
            {
                if (_blockNotepad)
                {
                    var processes = Process.GetProcessesByName("notepad");
                    foreach (var process in processes)
                    {
                        try
                        {
                            Console.WriteLine($"Found notepad (PID: {process.Id}), killing...");
                            process.Kill();
                            process.WaitForExit(1000);
                            Console.WriteLine("Notepad killed successfully!");

                            // Log to file
                            File.AppendAllText(
                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                "block_test.txt"),
                                $"[{DateTime.Now}] Blocked notepad (PID: {process.Id})\n");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error killing notepad: {ex.Message}");
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckProcesses: {ex.Message}");
            }
        }
    }
}