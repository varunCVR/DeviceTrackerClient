using SharedModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using static AppBlockerService.AppBlockerService;

namespace AppBlockerService
{
    public class DeviceHealthMonitor : IDisposable
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;
        private DateTime _lastCpuCheck = DateTime.MinValue;
        private float _lastCpuValue = 0;
        private readonly object _cpuLock = new object();

        public DeviceHealthMonitor()
        {
            InitializePerformanceCounters();
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                // Initialize CPU performance counter
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                // Get first reading to initialize
                _cpuCounter.NextValue();
                System.Threading.Thread.Sleep(100);
                _lastCpuCheck = DateTime.Now;

                // Initialize RAM counter
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                _ramCounter.NextValue(); // Initialize
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize performance counters: {ex.Message}");
                _cpuCounter = null;
                _ramCounter = null;
            }
        }

        public DeviceHealthSnapshot Collect()
        {
            var snapshot = new DeviceHealthSnapshot
            {
                TimestampUtc = DateTime.UtcNow,
                MachineName = Environment.MachineName
            };

            try
            {
                snapshot.CpuUsagePercent = GetCpuUsage();
                GetMemoryInfo(snapshot);
                GetDiskInfo(snapshot);
                GetTemperatureInfo(snapshot);
                GetBatteryInfo(snapshot);
                GetUptimeInfo(snapshot);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error collecting health data: {ex.Message}");
            }

            return snapshot;
        }

        private double GetCpuUsage()
        {
            try
            {
                // Method 1: Performance Counter (most accurate)
                if (_cpuCounter != null)
                {
                    var now = DateTime.Now;
                    if ((now - _lastCpuCheck).TotalSeconds >= 1)
                    {
                        _lastCpuValue = _cpuCounter.NextValue();
                        _lastCpuCheck = now;
                    }
                    return Math.Round(_lastCpuValue, 2);
                }

                // Method 2: WMI
                Console.WriteLine("Using WMI for CPU...");
                using (var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor"))
                {
                    double totalLoad = 0;
                    int processorCount = 0;

                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["LoadPercentage"] != null)
                        {
                            totalLoad += Convert.ToDouble(obj["LoadPercentage"]);
                            processorCount++;
                        }
                    }

                    if (processorCount > 0)
                    {
                        double avgLoad = totalLoad / processorCount;
                        Console.WriteLine($"CPU Load from WMI: {avgLoad}%");
                        return Math.Round(avgLoad, 2);
                    }
                }

                // Method 3: Alternative WMI query
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name='_Total'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["PercentProcessorTime"] != null)
                        {
                            double cpu = Convert.ToDouble(obj["PercentProcessorTime"]);
                            Console.WriteLine($"CPU from PerfOS: {cpu}%");
                            return Math.Round(cpu, 2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting CPU usage: {ex.Message}");
            }

            Console.WriteLine("Returning default CPU value: 0%");
            return 0;
        }

        private void GetMemoryInfo(DeviceHealthSnapshot snapshot)
        {
            try
            {
                // Method 1: Performance Counter for available memory
                long availableMB = 0;
                if (_ramCounter != null)
                {
                    availableMB = (long)_ramCounter.NextValue();
                    snapshot.FreeRamMb = availableMB;
                }

                // Method 2: WMI for total memory
                Console.WriteLine("Getting RAM info from WMI...");
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["TotalVisibleMemorySize"] != null && obj["FreePhysicalMemory"] != null)
                        {
                            long totalKB = Convert.ToInt64(obj["TotalVisibleMemorySize"]);
                            long freeKB = Convert.ToInt64(obj["FreePhysicalMemory"]);

                            snapshot.TotalRamMb = totalKB / 1024;

                            if (availableMB == 0) // If performance counter failed
                            {
                                snapshot.FreeRamMb = freeKB / 1024;
                            }

                            snapshot.UsedRamMb = snapshot.TotalRamMb - snapshot.FreeRamMb;

                            if (snapshot.TotalRamMb > 0)
                            {
                                snapshot.RamUsagePercent = Math.Round((snapshot.UsedRamMb / (double)snapshot.TotalRamMb) * 100, 2);
                            }

                            Console.WriteLine($"RAM: {snapshot.UsedRamMb}MB / {snapshot.TotalRamMb}MB ({snapshot.RamUsagePercent}%)");
                            return;
                        }
                    }
                }

                // Method 3: Alternative WMI query
                using (var mc = new ManagementClass("Win32_ComputerSystem"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        if (mo["TotalPhysicalMemory"] != null)
                        {
                            ulong totalBytes = Convert.ToUInt64(mo["TotalPhysicalMemory"]);
                            snapshot.TotalRamMb = (long)(totalBytes / (1024 * 1024));

                            if (availableMB > 0)
                            {
                                snapshot.FreeRamMb = availableMB;
                                snapshot.UsedRamMb = snapshot.TotalRamMb - snapshot.FreeRamMb;

                                if (snapshot.TotalRamMb > 0)
                                {
                                    snapshot.RamUsagePercent = Math.Round((snapshot.UsedRamMb / (double)snapshot.TotalRamMb) * 100, 2);
                                }
                            }

                            Console.WriteLine($"RAM from ComputerSystem: Total={snapshot.TotalRamMb}MB");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting memory info: {ex.Message}");
                snapshot.TotalRamMb = 0;
                snapshot.UsedRamMb = 0;
                snapshot.FreeRamMb = 0;
                snapshot.RamUsagePercent = 0;
            }
        }

        private void GetDiskInfo(DeviceHealthSnapshot snapshot)
        {
            try
            {
                Console.WriteLine("Getting disk info...");
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Network))
                    {
                        try
                        {
                            var diskInfo = new DiskHealthInfo
                            {
                                DriveLetter = drive.Name.Replace("\\", ""),
                                TotalSpaceGb = drive.TotalSize / (1024 * 1024 * 1024),
                                FreeSpaceGb = drive.TotalFreeSpace / (1024 * 1024 * 1024)
                            };

                            diskInfo.UsedSpaceGb = diskInfo.TotalSpaceGb - diskInfo.FreeSpaceGb;
                            diskInfo.UsagePercent = diskInfo.TotalSpaceGb > 0 ?
                                Math.Round((diskInfo.UsedSpaceGb / (double)diskInfo.TotalSpaceGb) * 100, 2) : 0;

                            snapshot.Disks.Add(diskInfo);
                            Console.WriteLine($"Disk {diskInfo.DriveLetter}: {diskInfo.UsedSpaceGb}GB / {diskInfo.TotalSpaceGb}GB ({diskInfo.UsagePercent}%)");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error reading drive {drive.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting disk info: {ex.Message}");
            }
        }

        private void GetTemperatureInfo(DeviceHealthSnapshot snapshot)
        {
            snapshot.Temperature = new TemperatureInfo { IsSupported = false };

            try
            {
                Console.WriteLine("Checking for temperature sensors...");

                // Try multiple WMI sources for temperature
                string[] wmiQueries = new string[]
                {
                    @"root\WMI", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature",
                    @"root\WMI", "SELECT CurrentTemperature FROM Win32_PerfFormattedData_Counters_ThermalZoneInformation",
                    @"root\CIMV2", "SELECT CurrentTemperature FROM Win32_TemperatureProbe"
                };

                for (int i = 0; i < wmiQueries.Length; i += 2)
                {
                    try
                    {
                        using (var searcher = new ManagementObjectSearcher(wmiQueries[i], wmiQueries[i + 1]))
                        {
                            foreach (ManagementObject obj in searcher.Get())
                            {
                                if (obj["CurrentTemperature"] != null)
                                {
                                    double rawTemp = Convert.ToDouble(obj["CurrentTemperature"]);
                                    double celsius;

                                    // Different sensors report in different units
                                    if (rawTemp > 1000) // Likely in tenths of Kelvin
                                    {
                                        celsius = Math.Round(rawTemp / 10.0 - 273.15, 1);
                                    }
                                    else if (rawTemp > 100) // Likely in Kelvin
                                    {
                                        celsius = Math.Round(rawTemp - 273.15, 1);
                                    }
                                    else // Likely already in Celsius
                                    {
                                        celsius = Math.Round(rawTemp, 1);
                                    }

                                    snapshot.Temperature = new TemperatureInfo
                                    {
                                        IsSupported = true,
                                        Celsius = celsius,
                                        SensorSource = wmiQueries[i + 1]
                                    };

                                    Console.WriteLine($"Temperature: {celsius}°C from {wmiQueries[i + 1]}");
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Temperature sensor {wmiQueries[i + 1]} failed: {ex.Message}");
                    }
                }

                Console.WriteLine("No temperature sensors found or accessible");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting temperature: {ex.Message}");
            }
        }

        private void GetBatteryInfo(DeviceHealthSnapshot snapshot)
        {
            try
            {
                Console.WriteLine("Checking battery info...");

                // Use WMI for battery info
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    var batteries = searcher.Get();

                    if (batteries.Count > 0)
                    {
                        Console.WriteLine($"Found {batteries.Count} battery/batteries");

                        foreach (ManagementObject battery in batteries)
                        {
                            try
                            {
                                float? chargePercent = null;
                                string status = "Unknown";

                                // Get battery charge percentage
                                if (battery["EstimatedChargeRemaining"] != null)
                                {
                                    chargePercent = Convert.ToSingle(battery["EstimatedChargeRemaining"]);
                                    Console.WriteLine($"Battery charge: {chargePercent}%");
                                }

                                // Get battery status
                                if (battery["BatteryStatus"] != null)
                                {
                                    int batteryStatus = Convert.ToInt32(battery["BatteryStatus"]);
                                    status = GetBatteryStatusString(batteryStatus);
                                    Console.WriteLine($"Battery status code: {batteryStatus} = {status}");
                                }
                                else if (battery["PowerOnline"] != null)
                                {
                                    bool isCharging = Convert.ToBoolean(battery["PowerOnline"]);
                                    status = isCharging ? "Charging" : "Discharging";
                                    Console.WriteLine($"PowerOnline: {isCharging} = {status}");
                                }

                                // Get other battery info
                                string deviceId = battery["DeviceID"]?.ToString() ?? "Unknown";
                                string chemistry = battery["Chemistry"]?.ToString() ?? "Unknown";
                                Console.WriteLine($"Battery DeviceID: {deviceId}, Chemistry: {chemistry}");

                                snapshot.Battery = new BatteryInfo
                                {
                                    IsPresent = true,
                                    ChargePercent = chargePercent,
                                    Status = status
                                };

                                return;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error reading battery properties: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        // No battery found - desktop PC
                        Console.WriteLine("No batteries found (Desktop PC)");
                        snapshot.Battery = new BatteryInfo
                        {
                            IsPresent = false,
                            Status = "PluggedIn" // Desktop PCs are always "plugged in"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting battery info: {ex.Message}");
                snapshot.Battery = new BatteryInfo
                {
                    IsPresent = false,
                    Status = "Error"
                };
            }
        }

        private string GetBatteryStatusString(int batteryStatus)
        {
            // Common BatteryStatus values from WMI
            switch (batteryStatus)
            {
                case 1: return "Discharging";
                case 2: return "OnAC (Charging)";
                case 3: return "Fully Charged";
                case 4: return "Low";
                case 5: return "Critical";
                case 6: return "Charging";
                case 7: return "ChargingHigh";
                case 8: return "ChargingWarning";
                case 9: return "Undefined";
                case 10: return "PartiallyCharged";
                default: return $"Unknown ({batteryStatus})";
            }
        }
        private double GetCpuUsageAlternative()
        {
            try
            {
                // Similar to how Task Manager works
                using (var pc = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total"))
                {
                    pc.NextValue();
                    Thread.Sleep(1000);
                    return Math.Round(pc.NextValue(), 2);
                }
            }
            catch
            {
                try
                {
                    // Another alternative
                    using (var pc = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                    {
                        pc.NextValue();
                        Thread.Sleep(1000);
                        return Math.Round(pc.NextValue(), 2);
                    }
                }
                catch
                {
                    return 0;
                }
            }
        }
        private void GetUptimeInfo(DeviceHealthSnapshot snapshot)
        {
            try
            {
                // Method 1: Environment.TickCount64 (most reliable)
                long tickCount = EnvironmentHelper.TickCount64;

                // Handle negative values (system has been up for more than 24.9 days)
                if (tickCount < 0)
                {
                    tickCount = int.MaxValue + (long)uint.MaxValue + tickCount;
                }

                snapshot.Uptime = TimeSpan.FromMilliseconds(tickCount);
                Console.WriteLine($"Uptime from TickCount64: {snapshot.Uptime.Days}d {snapshot.Uptime.Hours:00}:{snapshot.Uptime.Minutes:00}:{snapshot.Uptime.Seconds:00}");

                // Method 2: WMI for verification
                using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["LastBootUpTime"] != null)
                        {
                            try
                            {
                                string wmiTime = obj["LastBootUpTime"].ToString();
                                DateTime lastBoot = ManagementDateTimeConverter.ToDateTime(wmiTime);
                                TimeSpan wmiUptime = DateTime.Now - lastBoot;

                                Console.WriteLine($"Uptime from WMI: {wmiUptime.Days}d {wmiUptime.Hours:00}:{wmiUptime.Minutes:00}:{wmiUptime.Seconds:00}");
                                Console.WriteLine($"Last boot: {lastBoot}");

                                // Use WMI uptime if it seems reasonable
                                if (wmiUptime.TotalDays < 365) // Less than a year (reasonable)
                                {
                                    snapshot.Uptime = wmiUptime;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error converting WMI time: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting uptime: {ex.Message}");
                snapshot.Uptime = TimeSpan.Zero;
            }
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _cpuCounter = null;

            _ramCounter?.Dispose();
            _ramCounter = null;
        }
    }
}