using System;
using System.Collections.Generic;

namespace SharedModels
{
    public enum EventType
    {
        ProcessSnapshot,
        BlockAction,
        Inventory,
        ServiceStart,
        ServiceStop,
        Error,
        DeviceHealth
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public EventType EventType { get; set; }
        public string MachineName { get; set; } = Environment.MachineName;
        public string UserName { get; set; }
        public string ProcessName { get; set; }
        public int? ProcessId { get; set; }
        public string ExePath { get; set; }
        public string SourceLocation { get; set; }
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }
}