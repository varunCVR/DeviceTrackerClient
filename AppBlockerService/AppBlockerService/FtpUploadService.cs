using Newtonsoft.Json;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AppBlockerService
{
    public class FtpUploadService : IDisposable
    {
        private readonly FtpConfig _config;
        private readonly Logger _logger;
        private System.Timers.Timer _uploadTimer;
        private readonly object _uploadLock = new object();
        private bool _isUploading = false;
        private string _machineName;

        // Track uploaded files to avoid duplicates
        private HashSet<string> _uploadedFiles = new HashSet<string>();

        public FtpUploadService(FtpConfig config, Logger logger)
        {
            _config = config ?? new FtpConfig();
            _logger = logger;
            _machineName = Environment.MachineName;

            // Load previously uploaded files tracking
            LoadUploadedFilesCache();
        }

        public void Start()
        {
            if (!_config.Enabled)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object> { { "FTP", "FTP upload is disabled in config" } }
                });
                return;
            }

            _uploadTimer = new System.Timers.Timer(_config.UploadIntervalSeconds * 1000);
            _uploadTimer.Elapsed += async (sender, e) => await UploadTimerElapsedAsync();
            _uploadTimer.AutoReset = true;
            _uploadTimer.Start();

            _logger.Log(new LogEntry
            {
                EventType = EventType.ServiceStart,
                Details = new Dictionary<string, object>
                {
                    { "FTP", "FTP upload service started" },
                    { "Interval", $"{_config.UploadIntervalSeconds} seconds" },
                    { "Server", _config.Server }
                }
            });

            // Do initial upload immediately
            Task.Run(() => UploadLogsAsync());
        }

        public void Stop()
        {
            _uploadTimer?.Stop();
            _uploadTimer?.Dispose();

            SaveUploadedFilesCache();

            _logger.Log(new LogEntry
            {
                EventType = EventType.ServiceStop,
                Details = new Dictionary<string, object> { { "FTP", "FTP upload service stopped" } }
            });
        }

        private async Task UploadTimerElapsedAsync()
        {
            if (_isUploading)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object> { { "FTP", "Upload already in progress, skipping" } }
                });
                return;
            }

            await UploadLogsAsync();
        }

        public async Task UploadLogsAsync()
        {
            lock (_uploadLock)
            {
                if (_isUploading) return;
                _isUploading = true;
            }

            try
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.DeviceHealth, // Reuse this event type
                    Details = new Dictionary<string, object> { { "FTP", "Starting log upload" } }
                });

                // Get list of log files
                var logFiles = GetLogFilesToUpload();

                if (logFiles.Count == 0)
                {
                    _logger.Log(new LogEntry
                    {
                        EventType = EventType.DeviceHealth,
                        Details = new Dictionary<string, object> { { "FTP", "No new log files to upload" } }
                    });
                    return;
                }

                _logger.Log(new LogEntry
                {
                    EventType = EventType.DeviceHealth,
                    Details = new Dictionary<string, object>
                    {
                        { "FTP", $"Found {logFiles.Count} file(s) to upload" },
                        { "Files", string.Join(", ", logFiles.Select(f => Path.GetFileName(f))) }
                    }
                });

                // Create remote directory
                await CreateRemoteDirectoryAsync();

                // Upload each file
                int successCount = 0;
                int failCount = 0;

                foreach (var filePath in logFiles)
                {
                    bool uploaded = await UploadFileWithRetryAsync(filePath);

                    if (uploaded)
                    {
                        successCount++;
                        _uploadedFiles.Add(GetFileIdentifier(filePath));
                    }
                    else
                    {
                        failCount++;
                    }
                }

                // Save upload cache
                SaveUploadedFilesCache();

                _logger.Log(new LogEntry
                {
                    EventType = EventType.DeviceHealth,
                    Details = new Dictionary<string, object>
                    {
                        { "FTP", $"Upload complete: {successCount} succeeded, {failCount} failed" }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object>
                    {
                        { "FTP", $"Upload error: {ex.Message}" },
                        { "StackTrace", ex.StackTrace }
                    }
                });
            }
            finally
            {
                lock (_uploadLock)
                {
                    _isUploading = false;
                }
            }
        }

        private List<string> GetLogFilesToUpload()
        {
            var filesToUpload = new List<string>();

            try
            {
                if (!Directory.Exists(_config.LocalLogsPath))
                    return filesToUpload;

                // Get all JSONL log files
                var logFiles = Directory.GetFiles(_config.LocalLogsPath, "*.jsonl");

                foreach (var filePath in logFiles)
                {
                    // Skip files that are too small or recently modified (might be in use)
                    var fileInfo = new FileInfo(filePath);

                    // Skip if file is empty or being written to
                    if (fileInfo.Length == 0 ||
                        (DateTime.Now - fileInfo.LastWriteTime).TotalSeconds < 10)
                        continue;

                    // Check if we've already uploaded this file
                    string fileIdentifier = GetFileIdentifier(filePath);
                    if (!_uploadedFiles.Contains(fileIdentifier))
                    {
                        filesToUpload.Add(filePath);
                    }
                }

                // Also upload the inventory file
                string inventoryFile = Path.Combine(_config.LocalLogsPath, "installed_apps_inventory.json");
                if (File.Exists(inventoryFile))
                {
                    string inventoryIdentifier = GetFileIdentifier(inventoryFile);
                    if (!_uploadedFiles.Contains(inventoryIdentifier))
                    {
                        filesToUpload.Add(inventoryFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object>
                    {
                        { "FTP", $"Error getting log files: {ex.Message}" }
                    }
                });
            }

            return filesToUpload;
        }

        private string GetFileIdentifier(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            // Use filename + last write time as identifier (in case file gets updated)
            return $"{Path.GetFileName(filePath)}|{fileInfo.LastWriteTimeUtc:yyyyMMddHHmmss}";
        }

        private async Task<bool> UploadFileWithRetryAsync(string localFilePath)
        {
            int retryCount = 0;

            while (retryCount < _config.RetryCount)
            {
                try
                {
                    if (retryCount > 0)
                    {
                        _logger.Log(new LogEntry
                        {
                            EventType = EventType.Error,
                            Details = new Dictionary<string, object>
                            {
                                { "FTP", $"Retry {retryCount}/{_config.RetryCount} for {Path.GetFileName(localFilePath)}" }
                            }
                        });

                        await Task.Delay(_config.RetryDelaySeconds * 1000);
                    }

                    bool success = await UploadFileAsync(localFilePath);

                    if (success)
                        return true;
                }
                catch (Exception ex)
                {
                    _logger.Log(new LogEntry
                    {
                        EventType = EventType.Error,
                        Details = new Dictionary<string, object>
                        {
                            { "FTP", $"Upload attempt {retryCount + 1} failed: {ex.Message}" },
                            { "File", Path.GetFileName(localFilePath) }
                        }
                    });
                }

                retryCount++;
            }

            return false;
        }

        private async Task<bool> UploadFileAsync(string localFilePath)
        {
            string fileName = Path.GetFileName(localFilePath);
            string remotePath = _config.GetRemoteFilePath(_machineName, fileName);

            _logger.Log(new LogEntry
            {
                EventType = EventType.DeviceHealth,
                Details = new Dictionary<string, object>
                {
                    { "FTP", $"Uploading {fileName} to {remotePath}" }
                }
            });

            // Create FTP request
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{_config.Server}{remotePath}");
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(_config.Username, _config.Password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            // Read file content
            byte[] fileContents;
            using (var stream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                fileContents = new byte[stream.Length];
                await stream.ReadAsync(fileContents, 0, fileContents.Length);
            }

            // Upload file
            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
            }

            // Get response
            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.DeviceHealth,
                    Details = new Dictionary<string, object>
                    {
                        { "FTP", $"Upload complete: {response.StatusDescription}" },
                        { "File", fileName }
                    }
                });

                return response.StatusCode == FtpStatusCode.ClosingData ||
                       response.StatusCode == FtpStatusCode.FileActionOK;
            }
        }

        private async Task CreateRemoteDirectoryAsync()
        {
            try
            {
                string remoteDir = _config.GetRemoteMachinePath(_machineName);

                // Check if directory exists
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{_config.Server}{remoteDir}");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(_config.Username, _config.Password);

                try
                {
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        // Directory exists
                        response.Close();
                        return;
                    }
                }
                catch (WebException ex) when (((FtpWebResponse)ex.Response).StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // Directory doesn't exist, create it
                    request = (FtpWebRequest)WebRequest.Create($"ftp://{_config.Server}{remoteDir}");
                    request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    request.Credentials = new NetworkCredential(_config.Username, _config.Password);

                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        _logger.Log(new LogEntry
                        {
                            EventType = EventType.DeviceHealth,
                            Details = new Dictionary<string, object>
                            {
                                { "FTP", $"Created remote directory: {remoteDir}" }
                            }
                        });
                        response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object>
                    {
                        { "FTP", $"Error creating remote directory: {ex.Message}" }
                    }
                });
                throw;
            }
        }

        private void LoadUploadedFilesCache()
        {
            try
            {
                string cacheFile = Path.Combine(_config.LocalLogsPath, "ftp_upload_cache.json");
                if (File.Exists(cacheFile))
                {
                    string json = File.ReadAllText(cacheFile);
                    var cache = JsonConvert.DeserializeObject<UploadCache>(json);
                    if (cache != null)
                    {
                        _uploadedFiles = new HashSet<string>(cache.UploadedFiles);
                    }
                }
            }
            catch
            {
                // If cache loading fails, start fresh
                _uploadedFiles = new HashSet<string>();
            }
        }

        private void SaveUploadedFilesCache()
        {
            try
            {
                string cacheFile = Path.Combine(_config.LocalLogsPath, "ftp_upload_cache.json");
                var cache = new UploadCache
                {
                    MachineName = _machineName,
                    LastUpdate = DateTime.UtcNow,
                    UploadedFiles = _uploadedFiles.ToList()
                };

                string json = JsonConvert.SerializeObject(cache, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(cacheFile, json);
            }
            catch (Exception ex)
            {
                _logger.Log(new LogEntry
                {
                    EventType = EventType.Error,
                    Details = new Dictionary<string, object>
                    {
                        { "FTP", $"Error saving upload cache: {ex.Message}" }
                    }
                });
            }
        }

        public void Dispose()
        {
            Stop();
            SaveUploadedFilesCache();
        }

        // Cache model for uploaded files tracking
        private class UploadCache
        {
            public string MachineName { get; set; }
            public DateTime LastUpdate { get; set; }
            public List<string> UploadedFiles { get; set; } = new List<string>();
        }
    }
}