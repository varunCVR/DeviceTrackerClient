using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DeviceTrackerClient.Logging;
using DeviceTrackerClient.SystemMonitoring;

namespace DeviceTrackerClient.Monitors
{
    public class AppInventory
    {
        private Thread workerThread;
        private bool isRunning = false;

        public int ScanIntervalMinutes { get; set; } = 60; // scan every 1 hour

        private readonly EnhancedSystemMonitor systemMonitor = new EnhancedSystemMonitor();

        public void Start()
        {
            if (isRunning) return;

            isRunning = true;
            workerThread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "AppInventoryThread"
            };

            workerThread.Start();
            PersistentLogger.Instance.LogMessage("AppInventory started.");
        }

        public void Stop()
        {
            isRunning = false;
            PersistentLogger.Instance.LogMessage("AppInventory stopping...");
        }

        private void WorkerLoop()
        {
            while (isRunning)
            {
                try
                {
                    // This will scan and log installed apps
                    systemMonitor.CheckInstalledApps();

                    // Get the installed applications as detailed list
                    List<InstalledAppDetail> apps = systemMonitor.GetInstalledApplications();

                    // Log summary of installed apps
                    PersistentLogger.Instance.LogEvent(new
                    {
                        EventType = "InstalledAppSnapshot",
                        Timestamp = DateTimeOffset.Now,
                        TotalApps = apps.Count,
                        Apps = apps.Select(app => new
                        {
                            app.Name,
                            app.Version,
                            app.Publisher,
                            app.InstallDate
                        }).Take(50).ToList() // Log first 50 apps to avoid huge logs
                    });

                    // Also log to console for debugging
                    Console.WriteLine($"AppInventory: Found {apps.Count} installed applications");

                    if (apps.Count > 0)
                    {
                        Console.WriteLine("Sample apps:");
                        foreach (var app in apps.Take(10))
                        {
                            Console.WriteLine($"  • {app.Name} v{app.Version}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    PersistentLogger.Instance.LogError("Error in AppInventory loop", ex);
                    Console.WriteLine($"AppInventory Error: {ex.Message}");
                }

                Thread.Sleep(ScanIntervalMinutes * 60 * 1000);
            }
        }
    }
}