using System;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace DeviceTrackerClient.Logging
{
    /// <summary>
    /// A thread-safe JSONL file logger with daily rotation.
    /// Logs are written to C:\ProgramData\DeviceTracker\logs\YYYY-MM-DD.jsonl
    /// </summary>
    public sealed class PersistentLogger
    {
        private static readonly Lazy<PersistentLogger> lazyInstance =
            new Lazy<PersistentLogger>(() => new PersistentLogger(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static PersistentLogger Instance => lazyInstance.Value;

        private readonly string baseDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DeviceTracker");

        private readonly string logsDir;

        private readonly object fileLock = new object();

        private PersistentLogger()
        {
            logsDir = Path.Combine(baseDir, "logs");
            Directory.CreateDirectory(logsDir);
        }

        /// <summary>
        /// Writes a structured event as one JSON line: JSONL format.
        /// </summary>
        public void LogEvent(object eventObject)
        {
            try
            {
                string json = JsonConvert.SerializeObject(eventObject, new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore
                });

                string file = Path.Combine(logsDir, $"{DateTime.Now:yyyy-MM-dd}.jsonl");

                lock (fileLock)
                {
                    File.AppendAllText(file, json + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // Emergency fallback
                try
                {
                    File.AppendAllText(
                        Path.Combine(logsDir, "logger_error.txt"),
                        $"{DateTime.Now:u} LOG ERROR: {ex}\n");
                }
                catch { }
            }
        }

        /// <summary>
        /// Helper method to log a simple message.
        /// </summary>
        public void LogMessage(string message)
        {
            LogEvent(new
            {
                EventType = "Info",
                Message = message,
                Timestamp = DateTimeOffset.Now
            });
        }

        /// <summary>
        /// Helper method for error logging.
        /// </summary>
        public void LogError(string message, Exception ex = null)
        {
            LogEvent(new
            {
                EventType = "Error",
                Message = message,
                Exception = ex?.ToString(),
                Timestamp = DateTimeOffset.Now
            });
        }
    }
}
