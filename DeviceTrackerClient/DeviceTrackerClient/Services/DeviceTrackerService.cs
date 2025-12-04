using DeviceTrackerClient.Communication;
using DeviceTrackerClient.Configuration;
using DeviceTrackerClient.Core.Models;
using DeviceTrackerClient.Services;
using DeviceTrackerClient.SystemMonitoring;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceTrackerClient
{
    public class DeviceTrackerService : ServiceBase
    {
        private LoggerService _logger;
        private AppMonitorService _appMonitor;
        private AppInventoryService _appInventory;
        //private AppBlocker _appBlockerr;
        private ApiClient _apiClient;
        private EnhancedSystemMonitor _systemMonitor;
        private AppScannerService _appScanner;
        private AppBlockerService _appBlocker;

        private Timer _syncTimer;
        private Timer _heartbeatTimer;
        private Timer _commandPollTimer;
        private bool _stopping = false;

        public DeviceTrackerService()
        {
            ServiceName = "DeviceTrackerService";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            // Initialize on background thread to avoid service timeout
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    InitializeServices();
                    StartTimers();

                    // Log startup
                    _logger.LogSystemEvent("ServiceStarted", Environment.UserName);
                    Console.WriteLine("Device Tracker Service started successfully.");
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry($"Service startup failed: {ex.Message}", EventLogEntryType.Error);
                }
            });
        }

        // Update InitializeServices():
        private void InitializeServices()
        {
            // Load config
            var config = ClientConfig.Load();

            // Generate client ID if not exists
            if (string.IsNullOrEmpty(config.ClientId))
            {
                config.ClientId = $"{Environment.MachineName}_{Guid.NewGuid().ToString().Substring(0, 8)}";
                config.MachineName = Environment.MachineName;
                config.Save();
            }

            // Initialize services
            _logger = new LoggerService();
            _apiClient = new ApiClient();
            _appMonitor = new AppMonitorService(_logger);
            _appInventory = new AppInventoryService(_logger);
            _appBlocker = new AppBlockerService(_logger);  // Updated
            _systemMonitor = new EnhancedSystemMonitor();
            _appScanner = new AppScannerService(_logger);

            // Start monitoring
            _appMonitor.StartMonitoring();
            _appBlocker.Start();  // Start blocker
            _appScanner.Start();

            // Initial scan
            _systemMonitor.CheckInstalledApps();
            _appInventory.LogInstalledApps();
        }

        private void StartTimers()
        {
            // Sync logs every 5 minutes
            _syncTimer = new Timer(state =>
            {
                if (!_stopping)
                {
                    _apiClient.SendOfflineQueue();
                }
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5));

            // Heartbeat every minute
            _heartbeatTimer = new Timer(state =>
            {
                if (!_stopping)
                {
                    _apiClient.SendHeartbeat();
                }
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            // Poll for commands every 2 minutes
            _commandPollTimer = new Timer(state =>
            {
                if (!_stopping)
                {
                    ProcessCommands();
                }
            }, null, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        }

        private void ProcessCommands()
        {
            try
            {
                var commands = _apiClient.GetCommands();
                foreach (var command in commands)
                {
                    ExecuteCommand(command);
                }
            }
            catch (Exception ex)
            {
                _logger.LogSystemEvent("CommandProcessingError", Environment.UserName,
                    new Dictionary<string, object> { { "Error", ex.Message } });
            }
        }

        private void ExecuteCommand(Command command)
        {
            try
            {
                switch (command.Type)
                {
                    case "BlockApp":
                        var appToBlock = command.Data;
                        var config = ClientConfig.Load();
                        config.BlockedApplications.Add(appToBlock);
                        config.Save();
                        //_appBlockerr.UpdateBlockedApps(config.BlockedApplications);
                        _logger.LogSystemEvent("AppBlockedByAdmin", Environment.UserName,
                            new Dictionary<string, object> { { "App", appToBlock } });
                        break;

                    case "UnblockApp":
                        var appToUnblock = command.Data;
                        config = ClientConfig.Load();
                        config.BlockedApplications.Remove(appToUnblock);
                        config.Save();
                        //_appBlockerr.UpdateBlockedApps(config.BlockedApplications);
                        break;

                    case "Lock":
                        // Lock workstation
                        LockWorkstation();
                        break;

                    case "Wipe":
                        // Secure wipe logs (for demo, just delete logs)
                        WipeData();
                        break;

                    case "Update":
                        // Update logic would go here
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogSystemEvent("CommandExecutionError", Environment.UserName,
                    new Dictionary<string, object> { { "Error", ex.Message } });
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern void LockWorkStation();

        private void LockWorkstation()
        {
            LockWorkStation();
        }

        private void WipeData()
        {
            var dataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "DeviceTracker");

            if (Directory.Exists(dataDir))
            {
                Directory.Delete(dataDir, true);
            }
        }

        protected override void OnStop()
        {
            _stopping = true;

            try
            {
                // Stop all timers
                _syncTimer?.Dispose();
                _heartbeatTimer?.Dispose();
                _commandPollTimer?.Dispose();

                // Stop services
                _appMonitor?.StopMonitoring();
                _appBlocker?.Stop();  // Stop blocker
                _appScanner?.Stop();

                // Log shutdown
                _logger?.LogSystemEvent("ServiceStopped", Environment.UserName);

                // Send final logs (sync, no async)
                _apiClient.SendOfflineQueue();
            }
            catch { }

            base.OnStop();
        }

        // Debug mode
        public void DebugRun()
        {
            Console.WriteLine("=== Device Tracker Debug Mode ===");
            Console.WriteLine("Starting services...");

            InitializeServices();
            StartTimers();

            Console.WriteLine("Services started. Press 'Q' to quit.");

            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                // Keep running
            }

            OnStop();
        }
    }
}