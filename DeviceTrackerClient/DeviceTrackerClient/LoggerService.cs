using DeviceTrackerClient.Configuration;
using DeviceTrackerClient.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DeviceTrackerClient
{
    public class LoggerService
    {
        private readonly string logFilePath;
        private readonly List<ActivityLog> pendingLogs = new List<ActivityLog>();
        private readonly Timer logTimer;
        private readonly object lockObject = new object();

        public LoggerService()
        {
            // Use default path in ProgramData
            logFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "DeviceTracker",
                "logs",
                $"logs_{DateTime.Now:yyyy-MM-dd}.json");

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

            // Save logs every 30 seconds
            logTimer = new Timer(SaveLogs, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        // For backward compatibility
        public LoggerService(string filePath)
        {
            logFilePath = filePath;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            logTimer = new Timer(SaveLogs, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        public void LogApplicationUsage(string processName, string windowTitle, string executablePath)
        {
            var log = new ActivityLog
            {
                EventType = "AppUsage",
                Timestamp = DateTime.Now,
                Description = $"Application used: {processName}",
                AdditionalData = new Dictionary<string, object>
                {
                    { "ProcessName", processName },
                    { "WindowTitle", windowTitle },
                    { "ExecutablePath", executablePath }
                }
            };

            AddLog(log);
        }

        public void LogSystemEvent(string eventType, string userName)
        {
            var log = new ActivityLog
            {
                EventType = "SystemEvent",
                Timestamp = DateTime.Now,
                Description = $"System event: {eventType}",
                AdditionalData = new Dictionary<string, object>
                {
                    { "EventType", eventType },
                    { "UserName", userName }
                }
            };

            AddLog(log);
        }

        public void LogSystemEvent(string eventType, string userName, Dictionary<string, object> additionalData)
        {
            var log = new ActivityLog
            {
                EventType = "SystemEvent",
                Timestamp = DateTime.Now,
                Description = $"System event: {eventType}",
                AdditionalData = new Dictionary<string, object>
                {
                    { "EventType", eventType },
                    { "UserName", userName }
                }
            };

            // Add the additional data
            foreach (var data in additionalData)
            {
                log.AdditionalData[data.Key] = data.Value;
            }

            AddLog(log);
        }

        private void AddLog(ActivityLog log)
        {
            // Add ClientId
            try
            {
                var config = ClientConfig.Load();
                log.ClientId = config.ClientId ?? Environment.MachineName;
            }
            catch
            {
                log.ClientId = Environment.MachineName;
            }

            lock (lockObject)
            {
                pendingLogs.Add(log);
            }
        }

        private void SaveLogs(object state)
        {
            List<ActivityLog> logsToSave;

            lock (lockObject)
            {
                if (pendingLogs.Count == 0) return;

                logsToSave = new List<ActivityLog>(pendingLogs);
                pendingLogs.Clear();
            }

            try
            {
                // Read existing logs
                List<ActivityLog> allLogs = new List<ActivityLog>();
                if (File.Exists(logFilePath))
                {
                    var existingJson = File.ReadAllText(logFilePath);
                    if (!string.IsNullOrEmpty(existingJson))
                    {
                        allLogs = JsonConvert.DeserializeObject<List<ActivityLog>>(existingJson) ?? new List<ActivityLog>();
                    }
                }

                // Add new logs
                allLogs.AddRange(logsToSave);

                // Write back to file with formatting
                var json = JsonConvert.SerializeObject(allLogs, Formatting.Indented);
                File.WriteAllText(logFilePath, json);
            }
            catch (Exception ex)
            {
                // Log error, but don't crash the application
                System.Diagnostics.Debug.WriteLine($"Error saving logs: {ex.Message}");
            }
        }
    }
}