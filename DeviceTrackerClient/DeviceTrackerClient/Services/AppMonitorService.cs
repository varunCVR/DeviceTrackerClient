using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Win32;

namespace DeviceTrackerClient
{
    public class AppMonitorService
    {
        private readonly LoggerService logger;
        private Timer processPollTimer;
        private HashSet<string> previousProcesses;

        public AppMonitorService(LoggerService loggerService)
        {
            logger = loggerService;
            previousProcesses = new HashSet<string>();
        }

        public void StartMonitoring()
        {
            StartProcessMonitoring();
            StartSystemEventMonitoring();
        }

        private void StartProcessMonitoring()
        {
            // Use timer-based polling instead of WMI events
            processPollTimer = new Timer(CheckRunningProcesses, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private void CheckRunningProcesses(object state)
        {
            try
            {
                var currentProcesses = new HashSet<string>();
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowTitle.Length > 0)
                        {
                            string processName = process.ProcessName;
                            string windowTitle = process.MainWindowTitle;
                            string executablePath = "Unknown";

                            try
                            {
                                executablePath = process.MainModule?.FileName ?? "Unknown";
                            }
                            catch
                            {
                                // Access denied to MainModule
                            }

                            currentProcesses.Add(processName);

                            // Log if this is a new process we haven't seen before
                            if (!previousProcesses.Contains(processName))
                            {
                                logger.LogApplicationUsage(processName, windowTitle, executablePath);
                            }
                        }
                    }
                    catch
                    {
                        // Ignore processes we can't access
                    }
                    finally
                    {
                        process?.Dispose();
                    }
                }

                previousProcesses = currentProcesses;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error monitoring processes: {ex.Message}");
            }
        }

        private void StartSystemEventMonitoring()
        {
            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            Microsoft.Win32.SystemEvents.SessionEnding += SystemEvents_SessionEnding;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            string userName = Environment.UserName;

            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    logger.LogSystemEvent("Lock", userName);
                    break;
                case SessionSwitchReason.SessionUnlock:
                    logger.LogSystemEvent("Unlock", userName);
                    break;
                case SessionSwitchReason.ConsoleConnect:
                    logger.LogSystemEvent("ConsoleConnect", userName);
                    break;
                case SessionSwitchReason.ConsoleDisconnect:
                    logger.LogSystemEvent("ConsoleDisconnect", userName);
                    break;
            }
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            string userName = Environment.UserName;

            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                    logger.LogSystemEvent("Logoff", userName);
                    break;
                case SessionEndReasons.SystemShutdown:
                    logger.LogSystemEvent("Shutdown", userName);
                    break;
            }
        }

        public void StopMonitoring()
        {
            processPollTimer?.Dispose();
            Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            Microsoft.Win32.SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
        }
    }
}