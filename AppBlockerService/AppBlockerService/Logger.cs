using Newtonsoft.Json;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AppBlockerService
{
    public class Logger : IDisposable
    {
        private readonly string _logsPath;
        private readonly object _fileLock = new object();
        private readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();
        private readonly Timer _flushTimer;
        private bool _disposed = false;

        public Logger(string logsPath)
        {
            _logsPath = logsPath;
            _flushTimer = new Timer(FlushLogs, null, 1000, 1000);
        }

        public void Log(LogEntry entry)
        {
            lock (_logQueue)
            {
                _logQueue.Enqueue(entry);
            }
        }

        public void LogProcessSnapshot(List<LogEntry> entries)
        {
            var snapshotEntry = new LogEntry
            {
                EventType = EventType.ProcessSnapshot,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    { "ProcessCount", entries.Count },
                    { "Snapshot", entries }
                }
            };

            Log(snapshotEntry);
        }

        private void FlushLogs(object state)
        {
            List<LogEntry> entriesToWrite;
            lock (_logQueue)
            {
                if (_logQueue.Count == 0)
                    return;

                entriesToWrite = new List<LogEntry>(_logQueue);
                _logQueue.Clear();
            }

            var groupedEntries = new Dictionary<string, List<LogEntry>>();

            foreach (var entry in entriesToWrite)
            {
                string logFile = GetLogFileName(entry.EventType);
                if (!groupedEntries.ContainsKey(logFile))
                    groupedEntries[logFile] = new List<LogEntry>();

                groupedEntries[logFile].Add(entry);
            }

            foreach (var kvp in groupedEntries)
            {
                WriteToLogFile(kvp.Key, kvp.Value);
            }
        }
        public void LogHealth(DeviceHealthSnapshot snapshot)
        {
            lock (_fileLock)
            {
                string fileName = Path.Combine(_logsPath, $"health_log_{DateTime.UtcNow:yyyyMMdd}.jsonl");

                try
                {
                    using (var writer = new StreamWriter(fileName, true))
                    {
                        string jsonLine = JsonConvert.SerializeObject(snapshot);
                        writer.WriteLine(jsonLine);
                    }
                }
                catch (Exception ex)
                {
                    // Fallback to event log if file writing fails
                    EventLog.WriteEntry("AppBlockerService",
                        $"Failed to write health log: {ex.Message}",
                        EventLogEntryType.Error);
                }
            }
        }
        private string GetLogFileName(EventType eventType)
        {
            string dateString = DateTime.UtcNow.ToString("yyyyMMdd");

            switch (eventType)
            {
                case EventType.ProcessSnapshot:
                    return $"process_log_{dateString}.jsonl";
                case EventType.BlockAction:
                    return $"block_actions_{dateString}.jsonl";
                case EventType.DeviceHealth:
                    return $"health_log_{dateString}.jsonl";
                default:
                    return $"service_log_{dateString}.jsonl";
            }
        }

        private void WriteToLogFile(string fileName, List<LogEntry> entries)
        {
            lock (_fileLock)
            {
                string filePath = Path.Combine(_logsPath, fileName);

                try
                {
                    using (var writer = new StreamWriter(filePath, true))
                    {
                        foreach (var entry in entries)
                        {
                            string jsonLine = JsonConvert.SerializeObject(entry);
                            writer.WriteLine(jsonLine);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Fallback to event log if file writing fails
                    EventLog.WriteEntry("AppBlockerService",
                        $"Failed to write log: {ex.Message}",
                        EventLogEntryType.Error);
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _flushTimer?.Dispose();
                FlushLogs(null); // Write any remaining logs
            }
        }
    }
}