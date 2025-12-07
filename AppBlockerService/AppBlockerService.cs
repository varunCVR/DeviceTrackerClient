using Newtonsoft.Json;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace AppBlockerService
{
    public partial class AppBlockerService : ServiceBase
    {
        private System.Threading.Timer _monitoringTimer;
        private System.Threading.Timer _inventoryTimer;
        private FileSystemWatcher _configWatcher;
        private BlockList _blockList = new BlockList();
        private readonly string _configPath = @"C:\ProgramData\AppBlocker\config";
        private readonly string _logsPath = @"C:\ProgramData\AppBlocker\logs";
        private readonly string _blockListFile = @"C:\ProgramData\AppBlocker\config\blocked_apps.json";
        private readonly object _lock = new object();
        private InstalledAppScanner _scanner;
        private Logger _logger;
        private System.Timers.Timer _healthTimer;
        private DeviceHealthMonitor _healthMonitor;
        private FtpUploadService _ftpUploadService;
        private System.Timers.Timer _ftpUploadTimer;
        public AppBlockerService()
        {
            ServiceName = "AppBlockerService";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            // Create directories if they don't exist
            Directory.CreateDirectory(_configPath);
            Directory.CreateDirectory(_logsPath);

            _logger = new Logger(_logsPath);
            _scanner = new InstalledAppScanner(_logger);

            // Log service start
            _logger.Log(new LogEntry
            {
                EventType = EventType.ServiceStart,
                UserName = Environment.UserName,
                Details = new Dictionary<string, object> { { "Version", "1.0.0" } }
            });
            // Initialize and start FTP upload service
            var ftpConfig = new FtpConfig
            {
                Server = "ict75k.ldts.in",
                Username = "ftpUpload@ict75k.ldts.in",
                Password = "ICT75k",
                Enabled = true,
                UploadIntervalSeconds = 60,
                LocalLogsPath = _logsPath
            };

            //_ftpUploadService = new FtpUploadService(ftpConfig, _logger);
            _ftpUploadService.Start();

            // Also set up a separate timer to trigger manual uploads if needed
            _ftpUploadTimer = new System.Timers.Timer(60000); // 60 seconds
            //_ftpUploadTimer.Elapsed += async (s, e) =>
            //{
            //    // This ensures upload happens even if the FTP service timer misses it
            //    //await _ftpUploadService.UploadLogsAsync();
            //};
            _ftpUploadTimer.AutoReset = true;
            _ftpUploadTimer.Start();
            // Load block list
            LoadBlockList();
            try
            {
                // Your existing startup code (logger, monitors, etc.)

                string ftpConfigPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "AppBlocker",
                    "ftp_upload_config.json");

                // Load FTP config
                var config = FtpUploadConfig.Load(ftpConfigPath);

                // Start uploader
                _ftpUploadService = new FtpUploadService(config);
                _ftpUploadService.Start();

                Console.WriteLine("[FTP] Upload service started.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FTP] Failed to start uploader: " + ex);
            }
            _healthMonitor = new DeviceHealthMonitor();
            // Start process monitoring (every 1 second)
            _monitoringTimer = new System.Threading.Timer(MonitorProcesses, null, 0, 1000);

            // Start inventory scanner (every 6 hours)
            _inventoryTimer = new System.Threading.Timer(ScanInventory, null, TimeSpan.Zero, TimeSpan.FromHours(6));
            // Start health monitoring (every 60 seconds)
            _healthTimer = new System.Timers.Timer(60000); // 60 seconds
            _healthTimer.Elapsed += HealthTimer_Elapsed;
            _healthTimer.AutoReset = true;
            _healthTimer.Start();
            // Watch for config changes
            SetupConfigWatcher();

            // Initial inventory scan
            Task.Run(() => ScanInventory(null));
            Task.Run(() => CollectHealthData());
        }
        private void HealthTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CollectHealthData();
        }
        private void CollectHealthData()
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Collecting health data...");
                var snapshot = _healthMonitor.Collect();

                // Write debug info to console
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Health Data Collected:");
                Console.WriteLine($"  CPU: {snapshot.CpuUsagePercent}%");
                Console.WriteLine($"  RAM: {snapshot.RamUsagePercent}% ({snapshot.UsedRamMb}MB/{snapshot.TotalRamMb}MB)");
                Console.WriteLine($"  Uptime: {snapshot.Uptime}");
                Console.WriteLine($"  Battery Present: {snapshot.Battery?.IsPresent}");
                Console.WriteLine($"  Battery Status: {snapshot.Battery?.Status}");
                Console.WriteLine($"  Disks: {snapshot.Disks?.Count ?? 0}");

                _logger.LogHealth(snapshot);

                _logger.Log(new LogEntry
                {
                    EventType = EventType.DeviceHealth,
                    Details = new Dictionary<string, object>
            {
                { "CPU", $"{snapshot.CpuUsagePercent}%" },
                { "RAM", $"{snapshot.RamUsagePercent}%" },
                { "Uptime", snapshot.Uptime.ToString(@"dd\.hh\:mm\:ss") },
                { "Battery", snapshot.Battery?.Status ?? "Unknown" }
            }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Health collection error: {ex.Message}");
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object>
            {
                { "HealthCollectionError", ex.Message }
            }
                });
            }
        }
        // Add this class to SharedModels or AppBlockerService
        public static class EnvironmentHelper
        {
            private static readonly DateTime _startTime = DateTime.UtcNow;
            private static readonly long _startTickCount = Environment.TickCount;

            // Simulate TickCount64 for .NET Framework 4.8
            public static long TickCount64
            {
                get
                {
                    try
                    {
                        // Try to use reflection to access the real TickCount64 if it exists
                        var prop = typeof(Environment).GetProperty("TickCount64",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        if (prop != null)
                        {
                            return (long)prop.GetValue(null);
                        }
                    }
                    catch
                    {
                        // Reflection failed, use our fallback
                    }

                    // Fallback method for .NET Framework < 4.8
                    return CalculateTickCount64();
                }
            }

            private static long CalculateTickCount64()
            {
                int currentTickCount = Environment.TickCount;

                // Handle wrap-around (TickCount is a 32-bit signed int)
                long tickCount64;

                if (_startTickCount <= currentTickCount)
                {
                    // No wrap-around or wrapped once forward
                    tickCount64 = currentTickCount - _startTickCount;
                }
                else
                {
                    // Wrap-around occurred
                    tickCount64 = (int.MaxValue - _startTickCount) + (currentTickCount - int.MinValue);
                }

                // Add time since we started tracking
                TimeSpan elapsed = DateTime.UtcNow - _startTime;
                tickCount64 += (long)elapsed.TotalMilliseconds;

                return tickCount64;
            }
        }
        protected override void OnStop()
        {
            _monitoringTimer?.Dispose();
            _inventoryTimer?.Dispose();
            _configWatcher?.Dispose();

            _logger.Log(new LogEntry
            {
                EventType = EventType.ServiceStop,
                UserName = Environment.UserName
            });

            _logger.Dispose();
            _healthTimer?.Stop();
            _healthTimer?.Dispose();
            _healthMonitor?.Dispose();
            _ftpUploadTimer?.Stop();
            _ftpUploadTimer?.Dispose();
            _ftpUploadService?.Dispose();
            //_ftpUploadService?.Stop();
            _ftpUploadService?.Dispose();
        }

        private void LoadBlockList()
        {
            try
            {
                if (File.Exists(_blockListFile))
                {
                    var json = File.ReadAllText(_blockListFile);
                    _blockList = JsonConvert.DeserializeObject<BlockList>(json) ?? new BlockList();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object> { { "Error", ex.Message } }
                });
            }
        }

        private void MonitorProcesses(object state)
        {
            try
            {
                var processes = Process.GetProcesses();
                var snapshotEntries = new List<LogEntry>();

                foreach (var process in processes)
                {
                    try
                    {
                        // Log process snapshot
                        snapshotEntries.Add(new LogEntry
                        {
                            EventType = EventType.ProcessSnapshot,
                            ProcessName = process.ProcessName,
                            ProcessId = process.Id,
                            UserName = GetProcessUserName(process)
                        });

                        // Check if process should be blocked
                        CheckAndBlockProcess(process);
                    }
                    catch (Exception ex)
                    {
                        // Ignore access denied errors
                        if (!ex.Message.Contains("Access is denied"))
                        {
                            _logger.Log(new LogEntry
                            {
                                EventType = EventType.Error,
                                ProcessName = process.ProcessName,
                                ProcessId = process.Id,
                                Details = new Dictionary<string, object> { { "Error", ex.Message } }
                            });
                        }
                    }
                }

                // Log snapshot in batch
                if (snapshotEntries.Count > 0)
                {
                    _logger.LogProcessSnapshot(snapshotEntries);
                }

                // Clean up process objects
                foreach (var process in processes)
                {
                    process.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object> { { "Error", ex.Message } }
                });
            }
        }

        private void CheckAndBlockProcess(Process process)
        {
            string exePath = null;
            try
            {
                exePath = process.MainModule?.FileName;
            }
            catch
            {
                // Can't access main module
            }

            foreach (var blockedApp in _blockList.BlockedProcesses)
            {
                bool shouldBlock = false;

                // Check by process name (case-insensitive)
                if (!string.IsNullOrEmpty(blockedApp.ProcessName) &&
                    process.ProcessName.Equals(blockedApp.ProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    shouldBlock = true;
                }

                // Check by executable path (case-insensitive)
                if (!shouldBlock && !string.IsNullOrEmpty(exePath) && !string.IsNullOrEmpty(blockedApp.ExePath) &&
                    exePath.Equals(blockedApp.ExePath, StringComparison.OrdinalIgnoreCase))
                {
                    shouldBlock = true;
                }

                if (shouldBlock)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(1000);

                        _logger.Log(new LogEntry
                        {
                            EventType = EventType.BlockAction,
                            ProcessName = process.ProcessName,
                            ProcessId = process.Id,
                            ExePath = exePath,
                            Details = new Dictionary<string, object>
                            {
                                { "BlockedBy", blockedApp.ProcessName ?? blockedApp.ExePath },
                                { "Reason", "Block list match" }
                            }
                        });
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(new LogEntry
                        {
                            EventType = EventType.Error,
                            ProcessName = process.ProcessName,
                            ProcessId = process.Id,
                            Details = new Dictionary<string, object> { { "KillError", ex.Message } }
                        });
                    }
                }
            }
        }

        private string GetProcessUserName(Process process)
        {
            try
            {
                return process.StartInfo?.UserName ?? Environment.UserName;
            }
            catch
            {
                return "Unknown";
            }
        }

        private void ScanInventory(object state)
        {
            try
            {
                var inventory = _scanner.ScanAll();
                var inventoryFile = Path.Combine(_logsPath, "installed_apps_inventory.json");

                var inventoryData = new Inventory
                {
                    Apps = inventory,
                    LastScanned = DateTime.UtcNow
                };

                File.WriteAllText(inventoryFile, JsonConvert.SerializeObject(inventoryData, Newtonsoft.Json.Formatting.Indented));

                _logger.Log(new LogEntry
                {
                    EventType = EventType.Inventory,
                    Details = new Dictionary<string, object>
                    {
                        { "AppCount", inventory.Count },
                        { "InventoryFile", inventoryFile }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object> { { "InventoryError", ex.Message } }
                });
            }
        }

        private void SetupConfigWatcher()
        {
            _configWatcher = new FileSystemWatcher(_configPath)
            {
                Filter = "blocked_apps.json",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            _configWatcher.Changed += (s, e) =>
            {
                // Debounce reload to avoid multiple rapid reloads
                Thread.Sleep(100);
                LoadBlockList();
            };

            _configWatcher.EnableRaisingEvents = true;
        }
    }
}