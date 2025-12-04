using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace DeviceTrackerClient
{
    public class AppInventoryService
    {
        private readonly LoggerService loggerService;

        public AppInventoryService(LoggerService logger)
        {
            loggerService = logger;
        }

        public List<InstalledApp> GetInstalledApps()
        {
            var installedApps = new List<InstalledApp>();

            // Look in both 64-bit and 32-bit registry locations
            string[] registryPaths = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (string registryPath in registryPaths)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        ScanRegistryKey(key, installedApps);
                    }
                }
            }

            // Also check current user installations
            using (var userKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (userKey != null)
                {
                    ScanRegistryKey(userKey, installedApps);
                }
            }

            return installedApps;
        }

        private void ScanRegistryKey(RegistryKey key, List<InstalledApp> installedApps)
        {
            foreach (string subKeyName in key.GetSubKeyNames())
            {
                using (var subKey = key.OpenSubKey(subKeyName))
                {
                    string displayName = subKey?.GetValue("DisplayName") as string;
                    string displayVersion = subKey?.GetValue("DisplayVersion") as string;
                    string publisher = subKey?.GetValue("Publisher") as string;
                    string installDate = subKey?.GetValue("InstallDate") as string;
                    string installLocation = subKey?.GetValue("InstallLocation") as string;

                    if (!string.IsNullOrEmpty(displayName))
                    {
                        installedApps.Add(new InstalledApp
                        {
                            Name = displayName,
                            Version = displayVersion ?? "Unknown",
                            Publisher = publisher ?? "Unknown",
                            InstallDate = installDate ?? "Unknown",
                            InstallLocation = installLocation ?? "Unknown"
                        });
                    }
                }
            }
        }

        public void LogInstalledApps()
        {
            try
            {
                var apps = GetInstalledApps();
                foreach (var app in apps)
                {
                    loggerService.LogSystemEvent("InstalledApp", Environment.UserName,
                        new Dictionary<string, object>
                        {
                            { "AppName", app.Name },
                            { "Version", app.Version },
                            { "Publisher", app.Publisher },
                            { "InstallLocation", app.InstallLocation }
                        });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging installed apps: {ex.Message}");
            }
        }
    }
}