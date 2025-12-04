using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DeviceTrackerClient.Configuration;

namespace DeviceTrackerClient.Services
{
    public class AppBlocker
    {
        private Timer _blockerTimer;
        private List<string> _blockedApps = new List<string>();
        private readonly ClientConfig _config;
        private readonly LoggerService _logger;

        public AppBlocker(LoggerService logger)
        {
            _config = ClientConfig.Load();
            _logger = logger;
            _blockedApps = _config.BlockedApplications;
        }

        public void Start()
        {
            _blockerTimer = new Timer(CheckAndBlockApps, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public void Stop()
        {
            _blockerTimer?.Dispose();
        }

        private void CheckAndBlockApps(object state)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        var processName = process.ProcessName.ToLower();

                        foreach (var blockedApp in _blockedApps)
                        {
                            if (processName.Contains(blockedApp.ToLower()))
                            {
                                process.Kill();
                                LogBlockedApp(processName);
                                break;
                            }
                        }
                    }
                    catch
                    {
                        // No permission to kill
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AppBlocker: {ex.Message}");
            }
        }

        private void LogBlockedApp(string appName)
        {
            _logger.LogSystemEvent("AppBlocked", Environment.UserName, new Dictionary<string, object>
            {
                { "BlockedApp", appName },
                { "Timestamp", DateTime.Now }
            });
        }

        public void UpdateBlockedApps(List<string> apps)
        {
            _blockedApps = apps;
            _config.BlockedApplications = apps;
            _config.Save();
        }
    }
}