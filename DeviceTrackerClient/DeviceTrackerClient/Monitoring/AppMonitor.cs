using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using DeviceTrackerClient.Logging;

namespace DeviceTrackerClient.Monitors
{
    public class AppMonitor
    {
        private Thread workerThread;
        private bool isRunning = false;

        public int IntervalSeconds { get; set; } = 5; // check every 5 seconds

        public void Start()
        {
            if (isRunning) return;

            isRunning = true;
            workerThread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "AppMonitorThread"
            };

            workerThread.Start();
            PersistentLogger.Instance.LogMessage("AppMonitor started.");
        }

        public void Stop()
        {
            isRunning = false;
            PersistentLogger.Instance.LogMessage("AppMonitor stopping...");
        }

        private void WorkerLoop()
        {
            while (isRunning)
            {
                try
                {
                    var process = GetForegroundProcess();
                    if (process != null)
                    {
                        PersistentLogger.Instance.LogEvent(new
                        {
                            EventType = "AppUsage",
                            Description = $"Foreground App: {process.ProcessName}",
                            Timestamp = DateTimeOffset.Now,
                            AdditionalData = new
                            {
                                process.ProcessName,
                                process.MainWindowTitle
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    PersistentLogger.Instance.LogError("Error in AppMonitor loop", ex);
                }

                Thread.Sleep(IntervalSeconds * 1000);
            }
        }

        // Get foreground process via Win32 API

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int pid);

        private Process GetForegroundProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return null;

            GetWindowThreadProcessId(hwnd, out int pid);
            if (pid == 0) return null;

            try
            {
                return Process.GetProcessById(pid);
            }
            catch
            {
                return null;
            }
        }
    }
}
