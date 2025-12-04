using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace DeviceTrackerClient
{
    public class TestAppLogger
    {
        public static void TestAndLogAllApplications()
        {
            string logFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "AppDetectionTest.txt");

            var sb = new StringBuilder();
            sb.AppendLine($"Application Detection Test - {DateTime.Now}");
            sb.AppendLine("=".PadRight(80, '='));

            // Test 1: Registry Applications
            sb.AppendLine("\n1. REGISTRY INSTALLED APPLICATIONS:");
            sb.AppendLine("-".PadRight(80, '-'));
            var registryApps = GetRegistryApps();
            sb.AppendLine($"Found: {registryApps.Count} applications");
            foreach (var app in registryApps.Take(50)) // Show first 50
            {
                sb.AppendLine($"  • {app.Name} v{app.Version} - {app.Publisher}");
            }

            // Test 2: Running Processes
            sb.AppendLine("\n\n2. RUNNING PROCESSES:");
            sb.AppendLine("-".PadRight(80, '-'));
            var runningApps = GetRunningProcesses();
            sb.AppendLine($"Found: {runningApps.Count} processes");
            foreach (var app in runningApps.Take(30)) // Show first 30
            {
                sb.AppendLine($"  • {app.Name} - {app.Path}");
            }

            // Test 3: Start Menu
            sb.AppendLine("\n\n3. START MENU APPLICATIONS:");
            sb.AppendLine("-".PadRight(80, '-'));
            var startMenuApps = GetStartMenuApps();
            sb.AppendLine($"Found: {startMenuApps.Count} shortcuts");
            foreach (var app in startMenuApps.Take(20))
            {
                sb.AppendLine($"  • {app}");
            }

            // Write to file
            File.WriteAllText(logFile, sb.ToString());
            Process.Start("notepad.exe", logFile);
        }

        private static List<AppInfo> GetRegistryApps()
        {
            var apps = new List<AppInfo>();

            string[] registryPaths = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var path in registryPaths)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        ScanKey(key, apps);
                    }
                }

                using (var key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        ScanKey(key, apps);
                    }
                }
            }

            return apps;
        }

        private static void ScanKey(RegistryKey key, List<AppInfo> apps)
        {
            foreach (string subKeyName in key.GetSubKeyNames())
            {
                try
                {
                    using (var subKey = key.OpenSubKey(subKeyName))
                    {
                        string name = subKey?.GetValue("DisplayName") as string;
                        string version = subKey?.GetValue("DisplayVersion") as string;
                        string publisher = subKey?.GetValue("Publisher") as string;

                        if (!string.IsNullOrEmpty(name))
                        {
                            apps.Add(new AppInfo
                            {
                                Name = name,
                                Version = version ?? "Unknown",
                                Publisher = publisher ?? "Unknown"
                            });
                        }
                    }
                }
                catch { }
            }
        }

        private static List<ProcessInfo> GetRunningProcesses()
        {
            var processes = new List<ProcessInfo>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    string path = "Unknown";
                    try { path = process.MainModule?.FileName ?? "Unknown"; } catch { }

                    processes.Add(new ProcessInfo
                    {
                        Name = process.ProcessName,
                        Path = path,
                        Id = process.Id
                    });
                }
                catch { }
                finally
                {
                    process.Dispose();
                }
            }

            return processes;
        }

        private static List<string> GetStartMenuApps()
        {
            var apps = new List<string>();
            string startMenuPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                "Programs");

            if (Directory.Exists(startMenuPath))
            {
                foreach (var file in Directory.GetFiles(startMenuPath, "*.lnk", SearchOption.AllDirectories))
                {
                    apps.Add(Path.GetFileNameWithoutExtension(file));
                }
            }

            return apps;
        }
    }

    public class AppInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
    }

    public class ProcessInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Id { get; set; }
    }
}