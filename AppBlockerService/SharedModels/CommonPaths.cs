using System;
using System.IO;

namespace SharedModels
{
    public static class CommonPaths
    {
        public static readonly string BasePath = @"C:\ProgramData\AppBlocker";
        public static readonly string ConfigPath = Path.Combine(BasePath, "config");
        public static readonly string LogsPath = Path.Combine(BasePath, "logs");

        public static readonly string BlockListFile = Path.Combine(ConfigPath, "blocked_apps.json");
        public static readonly string InventoryFile = Path.Combine(LogsPath, "installed_apps_inventory.json");

        // Health log file pattern
        public static string GetHealthLogFileName(DateTime date)
        {
            return Path.Combine(LogsPath, $"health_log_{date:yyyyMMdd}.jsonl");
        }

        public static string GetCurrentHealthLogFileName()
        {
            return GetHealthLogFileName(DateTime.UtcNow);
        }
    }
}