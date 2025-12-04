using System;
using System.Collections.Generic;

namespace DeviceTrackerClient
{
    /* public class ActivityLog
     {
         public string EventType { get; set; } // "AppUsage", "SystemEvent", etc.
         public string Description { get; set; }
         public DateTime Timestamp { get; set; }
         public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
     }
 */

    public class AppUsageLog
    {
        public string ProcessName { get; set; }
        public string WindowTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ExecutablePath { get; set; }
    }

    public class SystemEventLog
    {
        public string EventType { get; set; } // "Logon", "Logoff", "Lock", "Unlock"
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
    }
    public class InstalledApp
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string InstallDate { get; set; }
        public string InstallLocation { get; set; }
    }
}