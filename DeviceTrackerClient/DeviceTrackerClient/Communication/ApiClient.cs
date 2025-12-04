using DeviceTrackerClient.Configuration;
using DeviceTrackerClient.Core.Models;
using DeviceTrackerClient.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace DeviceTrackerClient.Communication
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _offlineQueuePath;
        private readonly ClientConfig _config;

        public ApiClient()
        {
            _config = ClientConfig.Load();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _offlineQueuePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "DeviceTracker",
                "offline_queue.json");

            Directory.CreateDirectory(Path.GetDirectoryName(_offlineQueuePath));
        }

        public bool SendLogs(List<ActivityLog> logs)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    clientId = _config.ClientId,
                    machineName = Environment.MachineName,
                    logs = logs
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = _httpClient.PostAsync($"{_config.ServerUrl}/api/logs/batch", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully sent {logs.Count} logs");
                    return true;
                }

                // If failed, save to offline queue
                SaveToOfflineQueue(logs);
                return false;
            }
            catch (Exception)
            {
                SaveToOfflineQueue(logs);
                return false;
            }
        }

        private void SaveToOfflineQueue(List<ActivityLog> logs)
        {
            try
            {
                List<ActivityLog> queue = new List<ActivityLog>();

                // Load existing queue if exists
                if (File.Exists(_offlineQueuePath))
                {
                    var existingJson = File.ReadAllText(_offlineQueuePath);
                    queue = JsonConvert.DeserializeObject<List<ActivityLog>>(existingJson)
                           ?? new List<ActivityLog>();
                }

                queue.AddRange(logs);

                var json = JsonConvert.SerializeObject(queue, Formatting.Indented);
                File.WriteAllText(_offlineQueuePath, json);
            }
            catch { }
        }

        public List<ActivityLog> GetOfflineQueue()
        {
            try
            {
                if (File.Exists(_offlineQueuePath))
                {
                    var json = File.ReadAllText(_offlineQueuePath);
                    return JsonConvert.DeserializeObject<List<ActivityLog>>(json)
                           ?? new List<ActivityLog>();
                }
            }
            catch { }
            return new List<ActivityLog>();
        }

        public bool SendOfflineQueue()
        {
            var queue = GetOfflineQueue();
            if (queue.Count == 0) return true;

            if (SendLogs(queue))
            {
                // Clear queue on success
                File.Delete(_offlineQueuePath);
                return true;
            }
            return false;
        }

        public List<Command> GetCommands()
        {
            try
            {
                var response = _httpClient.GetAsync($"{_config.ServerUrl}/api/commands/{_config.ClientId}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<List<Command>>(json)
                           ?? new List<Command>();
                }
            }
            catch { }
            return new List<Command>();
        }

        public bool SendHeartbeat()
        {
            try
            {
                var response = _httpClient.PostAsync($"{_config.ServerUrl}/api/heartbeat/{_config.ClientId}", null).Result;
                return response.IsSuccessStatusCode;
            }
            catch { }
            return false;
        }
        public void ProcessCommand(Command command, AppBlockerService blocker)
        {
            try
            {
                switch (command.Type.ToLower())
                {
                    case "blockapp":
                        blocker.BlockByProcessName(command.Data);
                        break;

                    case "blockwindow":
                        blocker.BlockByWindowTitle(command.Data);
                        break;

                    case "blockfile":
                        blocker.BlockByFilePath(command.Data);
                        break;

                    case "unblockapp":
                        blocker.RemoveBlockRule(command.Data);
                        break;

                    case "listblocked":
                        // Return list of blocked apps
                        var rules = blocker.GetBlockRules();
                        // Send to server
                        break;

                    case "enableblock":
                        blocker.EnableBlockRule(command.Data, true);
                        break;

                    case "disableblock":
                        blocker.EnableBlockRule(command.Data, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing command: {ex.Message}");
            }
        }
    }

    public class Command
    {
        public string Id { get; set; }
        public string Type { get; set; }  // BlockApp, UnblockApp, Lock, Wipe, Update
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}