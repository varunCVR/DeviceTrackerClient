using System;
using System.Collections.Generic;

namespace SharedModels
{
    public class DeviceHealthSnapshot
    {
        public DateTime TimestampUtc { get; set; }
        public string MachineName { get; set; }

        public double CpuUsagePercent { get; set; }

        public long TotalRamMb { get; set; }
        public long UsedRamMb { get; set; }
        public long FreeRamMb { get; set; }
        public double RamUsagePercent { get; set; }

        public List<DiskHealthInfo> Disks { get; set; } = new List<DiskHealthInfo>();

        public TemperatureInfo Temperature { get; set; }
        public BatteryInfo Battery { get; set; }

        public TimeSpan Uptime { get; set; }
    }

    public class DiskHealthInfo
    {
        public string DriveLetter { get; set; }       // e.g. "C:"
        public long TotalSpaceGb { get; set; }
        public long FreeSpaceGb { get; set; }
        public long UsedSpaceGb { get; set; }
        public double UsagePercent { get; set; }
    }

    public class TemperatureInfo
    {
        public bool IsSupported { get; set; }
        public double? Celsius { get; set; }          // null if not available
        public string SensorSource { get; set; }      // e.g. "MSAcpi_ThermalZoneTemperature"
    }

    public class BatteryInfo
    {
        public bool IsPresent { get; set; }
        public float? ChargePercent { get; set; }     // 0–100
        public string Status { get; set; }           // "Charging", "Discharging", "Full", "NotPresent", etc.
    }
}