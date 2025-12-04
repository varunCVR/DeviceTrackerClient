using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DeviceTrackerClient.Configuration
{
    public class ClientConfig
    {
        public string ServerUrl { get; set; } = "http://localhost:5000";
        public string ClientId { get; set; }
        public string MachineName { get; set; }
        public List<string> BlockedApplications { get; set; } = new List<string>();
        public int SyncIntervalMinutes { get; set; } = 5;
        public bool AllowRemoteWipe { get; set; } = true;
        public bool AllowRemoteLock { get; set; } = true;
        public bool AutoStart { get; set; } = true;
        public bool IsServiceInstalled { get; set; } = false;

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "DeviceTracker",
            "client_config.json");

        public static ClientConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<ClientConfig>(json) ?? new ClientConfig();
                }
            }
            catch { }

            return new ClientConfig();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
}