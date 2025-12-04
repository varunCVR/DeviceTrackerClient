using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using DeviceTrackerClient.Configuration;
using DeviceTrackerClient.Core.Models;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace DeviceTrackerClient.Services
{
    public class AppScannerService
    {
        private readonly LoggerService _logger;
        private Timer _scanTimer;
        private List<AppInfo> _previousScan;
        private readonly string _scanHistoryPath;
        private readonly ClientConfig _config;

        public AppScannerService(LoggerService logger)
        {
            _logger = logger;
            _config = ClientConfig.Load();

            _scanHistoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "DeviceTracker",
                "scan_history.json");

            _previousScan = LoadPreviousScan();
        }

        public void Start()
        {
            // Scan immediately on start
            PerformFullScan();

            // Scan every 6 hours
            _scanTimer = new Timer(_ => PerformFullScan(), null,
                TimeSpan.FromHours(6), TimeSpan.FromHours(6));
        }

        public void Stop()
        {
            _scanTimer?.Dispose();
        }

        private void PerformFullScan()
        {
            try
            {
                var currentScan = ScanAllApplications();
                DetectChanges(_previousScan, currentScan);

                // Save current scan for next comparison
                _previousScan = currentScan;
                SaveScanHistory(currentScan);

                // Log complete inventory
                LogInventory(currentScan);
            }
            catch (Exception ex)
            {
                _logger.LogSystemEvent("AppScanError", Environment.UserName,
                    new Dictionary<string, object> { { "Error", ex.Message } });
            }
        }

        private List<AppInfo> ScanAllApplications()
        {
            var apps = new List<AppInfo>();

            // 1. Scan Registry (Installed Applications)
            apps.AddRange(ScanRegistryApplications());

            // 2. Scan Running Processes
            apps.AddRange(ScanRunningProcesses());

            // 3. Scan Start Menu shortcuts
            apps.AddRange(ScanStartMenuApplications());

            // 4. Scan Desktop shortcuts
            apps.AddRange(ScanDesktopApplications());

            // Manual distinct by Id (no DistinctBy in .NET Framework 4.8)
            var distinctApps = new List<AppInfo>();
            var seenIds = new HashSet<string>();

            foreach (var app in apps)
            {
                if (!seenIds.Contains(app.Id))
                {
                    seenIds.Add(app.Id);
                    distinctApps.Add(app);
                }
            }

            return distinctApps;
        }

        private List<AppInfo> ScanRegistryApplications()
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
                        ScanRegistryKey(key, apps, "LocalMachine");
                    }
                }

                using (var key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        ScanRegistryKey(key, apps, "CurrentUser");
                    }
                }
            }

            return apps;
        }

        private void ScanRegistryKey(RegistryKey key, List<AppInfo> apps, string root)
        {
            foreach (string subKeyName in key.GetSubKeyNames())
            {
                try
                {
                    using (var subKey = key.OpenSubKey(subKeyName))
                    {
                        string displayName = subKey?.GetValue("DisplayName") as string;
                        string displayVersion = subKey?.GetValue("DisplayVersion") as string;
                        string publisher = subKey?.GetValue("Publisher") as string;
                        string installDate = subKey?.GetValue("InstallDate") as string;
                        string installLocation = subKey?.GetValue("InstallLocation") as string;
                        string executablePath = subKey?.GetValue("DisplayIcon") as string;

                        if (!string.IsNullOrEmpty(displayName))
                        {
                            apps.Add(new AppInfo
                            {
                                Id = $"REG_{root}_{subKeyName}",
                                Name = displayName,
                                Version = displayVersion ?? "Unknown",
                                Publisher = publisher ?? "Unknown",
                                InstallDate = installDate ?? "Unknown",
                                InstallPath = installLocation ?? executablePath ?? "Unknown",
                                Type = "Installed",
                                Source = "Registry",
                                DetectionTime = DateTime.Now
                            });
                        }
                    }
                }
                catch { }
            }
        }

        private List<AppInfo> ScanRunningProcesses()
        {
            var apps = new List<AppInfo>();
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    if (!string.IsNullOrEmpty(process.ProcessName) &&
                        !process.ProcessName.Equals("Idle", StringComparison.OrdinalIgnoreCase) &&
                        !process.ProcessName.Equals("System", StringComparison.OrdinalIgnoreCase))
                    {
                        string executablePath = "Unknown";
                        try
                        {
                            executablePath = process.MainModule?.FileName ?? "Unknown";
                        }
                        catch { }

                        apps.Add(new AppInfo
                        {
                            Id = $"PROC_{process.Id}_{process.ProcessName}",
                            Name = process.ProcessName,
                            Version = GetFileVersion(executablePath),
                            Publisher = "Running Process",
                            InstallDate = "Unknown",
                            InstallPath = executablePath,
                            Type = "Running",
                            Source = "Process",
                            DetectionTime = DateTime.Now
                        });
                    }
                }
                catch { }
                finally
                {
                    process.Dispose();
                }
            }

            return apps;
        }

        private List<AppInfo> ScanStartMenuApplications()
        {
            var apps = new List<AppInfo>();

            string[] startMenuPaths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs")
            };

            foreach (var startMenuPath in startMenuPaths)
            {
                if (Directory.Exists(startMenuPath))
                {
                    ScanShortcutFolder(startMenuPath, apps, "StartMenu");
                }
            }

            return apps;
        }

        private List<AppInfo> ScanDesktopApplications()
        {
            var apps = new List<AppInfo>();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (Directory.Exists(desktopPath))
            {
                ScanShortcutFolder(desktopPath, apps, "Desktop");
            }

            return apps;
        }

        private void ScanShortcutFolder(string folderPath, List<AppInfo> apps, string source)
        {
            try
            {
                foreach (var file in Directory.GetFiles(folderPath, "*.lnk", SearchOption.AllDirectories))
                {
                    try
                    {
                        var shell = new IWshRuntimeLibrary.WshShell();
                        var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(file);

                        if (!string.IsNullOrEmpty(shortcut.TargetPath) &&
                            (shortcut.TargetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                             shortcut.TargetPath.EndsWith(".msi", StringComparison.OrdinalIgnoreCase)))
                        {
                            apps.Add(new AppInfo
                            {
                                Id = $"SHORT_{Path.GetFileNameWithoutExtension(file)}",
                                Name = Path.GetFileNameWithoutExtension(file),
                                Version = GetFileVersion(shortcut.TargetPath),
                                Publisher = "Shortcut",
                                InstallDate = File.GetCreationTime(file).ToString("yyyyMMdd"),
                                InstallPath = shortcut.TargetPath,
                                Type = "Shortcut",
                                Source = source,
                                DetectionTime = DateTime.Now
                            });
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private string GetFileVersion(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                    return versionInfo.FileVersion ?? versionInfo.ProductVersion ?? "Unknown";
                }
            }
            catch { }
            return "Unknown";
        }

        private void DetectChanges(List<AppInfo> previous, List<AppInfo> current)
        {
            // Find newly installed apps
            var newApps = current.Where(c => !previous.Any(p => p.Id == c.Id)).ToList();
            foreach (var app in newApps)
            {
                _logger.LogSystemEvent("AppDetected", Environment.UserName,
                    new Dictionary<string, object>
                    {
                        { "AppName", app.Name },
                        { "Version", app.Version },
                        { "Type", app.Type },
                        { "Source", app.Source },
                        { "Path", app.InstallPath }
                    });
            }

            // Find removed apps
            var removedApps = previous.Where(p => !current.Any(c => c.Id == p.Id)).ToList();
            foreach (var app in removedApps)
            {
                _logger.LogSystemEvent("AppRemoved", Environment.UserName,
                    new Dictionary<string, object>
                    {
                        { "AppName", app.Name },
                        { "Version", app.Version },
                        { "Type", app.Type }
                    });
            }
        }

        private void LogInventory(List<AppInfo> apps)
        {
            _logger.LogSystemEvent("AppInventory", Environment.UserName,
                new Dictionary<string, object>
                {
                    { "TotalApps", apps.Count },
                    { "InstalledApps", apps.Where(a => a.Type == "Installed").Count() },
                    { "RunningApps", apps.Where(a => a.Type == "Running").Count() },
                    { "AppList", apps.Select(a => $"{a.Name} ({a.Version}) - {a.Type}").ToList() }
                });
        }

        private List<AppInfo> LoadPreviousScan()
        {
            try
            {
                if (File.Exists(_scanHistoryPath))
                {
                    var json = File.ReadAllText(_scanHistoryPath);
                    return JsonConvert.DeserializeObject<List<AppInfo>>(json) ?? new List<AppInfo>();
                }
            }
            catch { }
            return new List<AppInfo>();
        }

        private void SaveScanHistory(List<AppInfo> scan)
        {
            try
            {
                var json = JsonConvert.SerializeObject(scan, Formatting.Indented);
                File.WriteAllText(_scanHistoryPath, json);
            }
            catch { }
        }
    }

    public class AppInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string InstallDate { get; set; }
        public string InstallPath { get; set; }
        public string Type { get; set; } // Installed, Running, Shortcut
        public string Source { get; set; } // Registry, Process, StartMenu, Desktop
        public DateTime DetectionTime { get; set; }
    }
}