using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;
using DeviceTrackerClient.Logging;

namespace DeviceTrackerClient.SystemMonitoring
{
    /// <summary>
    /// Tracks installed applications and detects changes vs baseline.
    /// </summary>
    public class EnhancedSystemMonitor
    {
        private readonly string baseDir;
        private readonly string baselineFile;

        public EnhancedSystemMonitor()
        {
            // ALWAYS resolve ProgramData path safely
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            if (string.IsNullOrWhiteSpace(programData))
            {
                // fallback for rare cases
                programData = @"C:\ProgramData";
            }

            // build DeviceTracker directory
            baseDir = Path.Combine(programData, "DeviceTracker");

            // ensure directory exists
            try
            {
                if (!Directory.Exists(baseDir))
                    Directory.CreateDirectory(baseDir);
            }
            catch (Exception ex)
            {
                PersistentLogger.Instance.LogError("Failed to create DeviceTracker directory", ex);
            }

            // NOW safely create baseline file path
            baselineFile = Path.Combine(baseDir, "baseline_apps.json");
        }

        /// <summary>
        /// Collects installed apps, compares with baseline, logs changes.
        /// </summary>
        public void CheckInstalledApps()
        {
            try
            {
                var currentApps = GetInstalledApplications();
                var baseline = LoadBaseline();

                // Debug: Log what we found
                PersistentLogger.Instance.LogEvent(new
                {
                    EventType = "AppScanDebug",
                    CurrentAppsCount = currentApps.Count,
                    BaselineAppsCount = baseline.Count,
                    Timestamp = DateTimeOffset.Now
                });

                // Find newly installed apps
                var newlyInstalled = new List<InstalledAppDetail>();
                foreach (var currentApp in currentApps)
                {
                    if (!baseline.Any(baselineApp =>
                        string.Equals(baselineApp.Name, currentApp.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        newlyInstalled.Add(currentApp);
                    }
                }

                // Find uninstalled apps
                var uninstalled = new List<InstalledAppDetail>();
                foreach (var baselineApp in baseline)
                {
                    if (!currentApps.Any(currentApp =>
                        string.Equals(currentApp.Name, baselineApp.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        uninstalled.Add(baselineApp);
                    }
                }

                // Log new apps
                foreach (var app in newlyInstalled)
                {
                    PersistentLogger.Instance.LogEvent(new
                    {
                        EventType = "AppInstalled",
                        AppName = app.Name,
                        Version = app.Version,
                        Publisher = app.Publisher,
                        InstallLocation = app.InstallLocation,
                        Timestamp = DateTimeOffset.Now
                    });
                }

                // Log removed apps
                foreach (var app in uninstalled)
                {
                    PersistentLogger.Instance.LogEvent(new
                    {
                        EventType = "AppUninstalled",
                        AppName = app.Name,
                        Version = app.Version,
                        Publisher = app.Publisher,
                        Timestamp = DateTimeOffset.Now
                    });
                }

                // Save new baseline
                SaveBaseline(currentApps);

                // Log summary
                PersistentLogger.Instance.LogEvent(new
                {
                    EventType = "AppScanComplete",
                    TotalDetected = currentApps.Count,
                    NewApps = newlyInstalled.Count,
                    RemovedApps = uninstalled.Count,
                    Timestamp = DateTimeOffset.Now
                });
            }
            catch (Exception ex)
            {
                PersistentLogger.Instance.LogError("Error in CheckInstalledApps()", ex);
            }
        }

        /// <summary>
        /// Loads baseline file safely.
        /// </summary>
        private List<InstalledAppDetail> LoadBaseline()
        {
            try
            {
                if (!File.Exists(baselineFile))
                    return new List<InstalledAppDetail>();

                var json = File.ReadAllText(baselineFile);
                var list = JsonConvert.DeserializeObject<List<InstalledAppDetail>>(json);
                return list ?? new List<InstalledAppDetail>();
            }
            catch (Exception ex)
            {
                PersistentLogger.Instance.LogError("Failed to read baseline file", ex);
                return new List<InstalledAppDetail>();
            }
        }

        /// <summary>
        /// Stores new baseline safely.
        /// </summary>
        private void SaveBaseline(List<InstalledAppDetail> apps)
        {
            try
            {
                var json = JsonConvert.SerializeObject(apps, Formatting.Indented);
                File.WriteAllText(baselineFile, json);
            }
            catch (Exception ex)
            {
                PersistentLogger.Instance.LogError("Failed to save baseline file", ex);
            }
        }

        /// <summary>
        /// Reads installed apps from registry.
        /// </summary>
        public List<InstalledAppDetail> GetInstalledApplications()
        {
            var result = new List<InstalledAppDetail>();

            string[] registryPaths = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            RegistryKey[] roots = new[]
            {
                Registry.LocalMachine,
                Registry.CurrentUser
            };

            foreach (var root in roots)
            {
                foreach (var path in registryPaths)
                {
                    try
                    {
                        using (var key = root.OpenSubKey(path))
                        {
                            if (key == null) continue;

                            foreach (var subKeyName in key.GetSubKeyNames())
                            {
                                try
                                {
                                    using (var sub = key.OpenSubKey(subKeyName))
                                    {
                                        if (sub == null) continue;

                                        string name = sub.GetValue("DisplayName") as string;
                                        string version = sub.GetValue("DisplayVersion") as string;
                                        string publisher = sub.GetValue("Publisher") as string;
                                        string installDate = sub.GetValue("InstallDate") as string;
                                        string installLocation = sub.GetValue("InstallLocation") as string;
                                        string uninstallString = sub.GetValue("UninstallString") as string;
                                        string installSource = sub.GetValue("InstallSource") as string;
                                        object estimatedSizeObj = sub.GetValue("EstimatedSize");
                                        int estimatedSize = estimatedSizeObj != null ? Convert.ToInt32(estimatedSizeObj) : 0;

                                        if (!string.IsNullOrWhiteSpace(name))
                                        {
                                            var app = new InstalledAppDetail
                                            {
                                                Name = name.Trim(),
                                                Version = version ?? "Unknown",
                                                Publisher = publisher ?? "Unknown",
                                                InstallDate = installDate ?? "Unknown",
                                                InstallLocation = installLocation ?? "Unknown",
                                                UninstallString = uninstallString ?? "Unknown",
                                                InstallSource = installSource ?? "Unknown",
                                                EstimatedSizeMB = estimatedSize,
                                                RegistryPath = $"{root.Name}\\{path}\\{subKeyName}"
                                            };

                                            // Manual distinct check
                                            if (!result.Any(r =>
                                                string.Equals(r.Name, app.Name, StringComparison.OrdinalIgnoreCase)))
                                            {
                                                result.Add(app);
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }
            }

            return result.OrderBy(x => x.Name).ToList();
        }
    }

    public class InstalledAppDetail
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string InstallDate { get; set; }
        public string InstallLocation { get; set; }
        public string UninstallString { get; set; }
        public string InstallSource { get; set; }
        public int EstimatedSizeMB { get; set; }
        public string RegistryPath { get; set; }
    }
}