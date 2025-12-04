using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace DeviceTrackerConfig.Services
{
    public class AppDetectorService
    {
        public class DetectedApp
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ProcessName { get; set; }
            public string FilePath { get; set; }
            public string Version { get; set; }
            public string Publisher { get; set; }
            public string InstallDate { get; set; }
            public string Type { get; set; } // Installed, Running, Portable
            public string Source { get; set; } // Registry, Process, StartMenu
            public long FileSize { get; set; }
            public DateTime DetectedTime { get; set; }
            public bool IsBlocked { get; set; }
        }

        public List<DetectedApp> ScanAllApplications()
        {
            var allApps = new List<DetectedApp>();

            // 1. Scan installed apps from registry
            allApps.AddRange(ScanInstalledApps());

            // 2. Scan currently running processes
            allApps.AddRange(ScanRunningProcesses());

            // 3. Scan common program folders
            allApps.AddRange(ScanProgramFolders());

            // 4. Scan Start Menu shortcuts
            allApps.AddRange(ScanStartMenuShortcuts());

            // Remove duplicates (by file path or name)
            return allApps
                .GroupBy(app => app.FilePath)
                .Select(group => group.First())
                .OrderBy(app => app.Name)
                .ToList();
        }

        private List<DetectedApp> ScanInstalledApps()
        {
            var apps = new List<DetectedApp>();

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

        private void ScanRegistryKey(RegistryKey key, List<DetectedApp> apps, string root)
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
                            // Try to find the main executable
                            string mainExe = FindMainExecutable(installLocation);

                            apps.Add(new DetectedApp
                            {
                                Id = $"REG_{root}_{subKeyName}",
                                Name = displayName,
                                ProcessName = Path.GetFileNameWithoutExtension(mainExe) ?? displayName,
                                FilePath = mainExe ?? installLocation,
                                Version = displayVersion ?? "Unknown",
                                Publisher = publisher ?? "Unknown",
                                InstallDate = installDate ?? "Unknown",
                                Type = "Installed",
                                Source = "Registry",
                                FileSize = GetFileSize(mainExe),
                                DetectedTime = DateTime.Now
                            });
                        }
                    }
                }
                catch { }
            }
        }

        private List<DetectedApp> ScanRunningProcesses()
        {
            var apps = new List<DetectedApp>();
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    // Skip system processes
                    if (string.IsNullOrEmpty(process.ProcessName) ||
                        process.ProcessName.ToLower() == "idle" ||
                        process.ProcessName.ToLower() == "system")
                        continue;

                    string executablePath = "Unknown";
                    try
                    {
                        executablePath = process.MainModule?.FileName ?? "Unknown";
                    }
                    catch { }

                    // Skip if we already have this in installed apps
                    if (apps.Any(a => a.FilePath.Equals(executablePath, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    apps.Add(new DetectedApp
                    {
                        Id = $"PROC_{process.Id}_{process.ProcessName}",
                        Name = process.ProcessName,
                        ProcessName = process.ProcessName,
                        FilePath = executablePath,
                        Version = GetFileVersion(executablePath),
                        Publisher = "Running Process",
                        InstallDate = "Unknown",
                        Type = "Running",
                        Source = "Process",
                        FileSize = GetFileSize(executablePath),
                        DetectedTime = DateTime.Now
                    });
                }
                catch { }
                finally
                {
                    process.Dispose();
                }
            }

            return apps;
        }

        private List<DetectedApp> ScanProgramFolders()
        {
            var apps = new List<DetectedApp>();

            string[] programFolders = {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"C:\Program Files",
                @"C:\Program Files (x86)",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming")
            };

            foreach (var folder in programFolders.Distinct())
            {
                if (Directory.Exists(folder))
                {
                    ScanFolderForExecutables(folder, apps, "ProgramFolder");
                }
            }

            return apps;
        }

        private void ScanFolderForExecutables(string folderPath, List<DetectedApp> apps, string source)
        {
            try
            {
                // Look for executables
                var exeFiles = Directory.GetFiles(folderPath, "*.exe", SearchOption.AllDirectories)
                    .Take(1000); // Limit to avoid too many results

                foreach (var exePath in exeFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(exePath);

                        // Skip small system files and duplicates
                        if (fileInfo.Length < 10000 ||
                            apps.Any(a => a.FilePath.Equals(exePath, StringComparison.OrdinalIgnoreCase)))
                            continue;

                        var versionInfo = FileVersionInfo.GetVersionInfo(exePath);

                        apps.Add(new DetectedApp
                        {
                            Id = $"FILE_{Path.GetFileNameWithoutExtension(exePath)}",
                            Name = versionInfo.ProductName ?? Path.GetFileNameWithoutExtension(exePath),
                            ProcessName = Path.GetFileNameWithoutExtension(exePath),
                            FilePath = exePath,
                            Version = versionInfo.FileVersion ?? versionInfo.ProductVersion ?? "Unknown",
                            Publisher = versionInfo.CompanyName ?? "Unknown",
                            InstallDate = fileInfo.CreationTime.ToString("yyyy-MM-dd"),
                            Type = "Portable",
                            Source = source,
                            FileSize = fileInfo.Length,
                            DetectedTime = DateTime.Now
                        });
                    }
                    catch { }
                }
            }
            catch { }
        }

        private List<DetectedApp> ScanStartMenuShortcuts()
        {
            var apps = new List<DetectedApp>();

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

        private void ScanShortcutFolder(string folderPath, List<DetectedApp> apps, string source)
        {
            try
            {
                var shortcutFiles = Directory.GetFiles(folderPath, "*.lnk", SearchOption.AllDirectories);

                foreach (var shortcutPath in shortcutFiles)
                {
                    try
                    {
                        // Read shortcut target
                        var shell = new IWshRuntimeLibrary.WshShell();
                        var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);

                        if (shortcut.TargetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            var versionInfo = FileVersionInfo.GetVersionInfo(shortcut.TargetPath);

                            apps.Add(new DetectedApp
                            {
                                Id = $"SHORTCUT_{Path.GetFileNameWithoutExtension(shortcutPath)}",
                                Name = Path.GetFileNameWithoutExtension(shortcutPath),
                                ProcessName = Path.GetFileNameWithoutExtension(shortcut.TargetPath),
                                FilePath = shortcut.TargetPath,
                                Version = versionInfo.FileVersion ?? versionInfo.ProductVersion ?? "Unknown",
                                Publisher = versionInfo.CompanyName ?? "Unknown",
                                InstallDate = File.GetCreationTime(shortcutPath).ToString("yyyy-MM-dd"),
                                Type = "Shortcut",
                                Source = source,
                                FileSize = new FileInfo(shortcut.TargetPath).Length,
                                DetectedTime = DateTime.Now
                            });
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private string FindMainExecutable(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return null;

            try
            {
                // Look for common executable patterns
                var exeFiles = Directory.GetFiles(folderPath, "*.exe", SearchOption.AllDirectories);

                // Prefer files that match folder name or common patterns
                var folderName = Path.GetFileName(folderPath);

                var mainExe = exeFiles.FirstOrDefault(f =>
                    Path.GetFileNameWithoutExtension(f).Equals(folderName, StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileNameWithoutExtension(f).ToLower().Contains("setup") == false);

                return mainExe ?? exeFiles.FirstOrDefault();
            }
            catch
            {
                return null;
            }
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

        private long GetFileSize(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return new FileInfo(filePath).Length;
                }
            }
            catch { }
            return 0;
        }

        // Get blocked apps from rules file
        public List<string> GetBlockedApps()
        {
            var blocked = new List<string>();

            try
            {
                string rulesPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "DeviceTracker",
                    "block_rules.json");

                if (File.Exists(rulesPath))
                {
                    var json = File.ReadAllText(rulesPath);
                    var rules = JsonConvert.DeserializeObject<List<BlockRule>>(json) ?? new List<BlockRule>();

                    blocked = rules
                        .Where(r => r.IsEnabled)
                        .Select(r => r.Pattern)
                        .ToList();
                }
            }
            catch { }

            return blocked;
        }
    }

    // Simple BlockRule class for JSON deserialization
    public class BlockRule
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string MatchType { get; set; }
        public bool IsEnabled { get; set; }
        public bool UseGracefulTermination { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastTriggered { get; set; }
        public int TriggerCount { get; set; }
    }
}