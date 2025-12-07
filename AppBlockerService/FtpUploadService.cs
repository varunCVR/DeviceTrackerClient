using System;
using System.IO;
using System.Linq;
using System.Threading;
using Renci.SshNet;

namespace AppBlockerService
{
    public class FtpUploadService : IDisposable
    {
        private readonly FtpUploadConfig _config;
        private Timer _timer;
        private bool _isRunning;

        public FtpUploadService(FtpUploadConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Start()
        {
            if (!_config.Enabled)
                return;

            // First run immediately, then every UploadIntervalSeconds
            _timer = new Timer(_ => RunUploadSafe(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(_config.UploadIntervalSeconds));
        }

        private void RunUploadSafe()
        {
            if (_isRunning) return; // prevent overlap if upload takes > interval

            _isRunning = true;
            try
            {
                UploadAllJsonFilesWithRetry();
            }
            catch (Exception ex)
            {
                // TODO: replace with your logger
                Console.WriteLine($"[FTP] Fatal error: {ex}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private void UploadAllJsonFilesWithRetry()
        {
            for (int attempt = 1; attempt <= _config.RetryCount; attempt++)
            {
                try
                {
                    UploadAllJsonFiles();
                    return; // success
                }
                catch (Exception ex)
                {
                    // TODO: replace with your logger
                    Console.WriteLine(
                        $"[FTP] Upload attempt {attempt} failed: {ex.Message}");

                    if (attempt == _config.RetryCount)
                        throw;

                    Thread.Sleep(TimeSpan.FromSeconds(_config.RetryDelaySeconds));
                }
            }
        }

        private void UploadAllJsonFiles()
        {
            if (!Directory.Exists(_config.LocalLogsPath))
            {
                Console.WriteLine($"[FTP] Local logs path not found: {_config.LocalLogsPath}");
                return;
            }

            using (var client = new SftpClient(
                       _config.Server,
                       _config.Port,
                       _config.Username,
                       _config.Password))
            {
                client.Connect();

                // Ensure base remote folder exists
                EnsureRemoteDirectory(client, _config.RemoteBasePath);

                // 1) Upload JSON directly under LocalLogsPath (if any)
                var rootJsonFiles = Directory
                    .GetFiles(_config.LocalLogsPath, "*.json", SearchOption.TopDirectoryOnly);

                foreach (var file in rootJsonFiles)
                {
                    var fileName = Path.GetFileName(file);
                    var remotePath = CombineUnixPath(_config.RemoteBasePath, fileName);
                    UploadFile(client, file, remotePath);
                }

                // 2) For each device directory under LocalLogsPath
                var deviceDirectories = Directory
                    .GetDirectories(_config.LocalLogsPath, "*", SearchOption.TopDirectoryOnly);

                foreach (var deviceDir in deviceDirectories)
                {
                    var deviceName = Path.GetFileName(deviceDir);
                    if (string.IsNullOrWhiteSpace(deviceName))
                        continue;

                    var remoteDeviceDir = CombineUnixPath(_config.RemoteBasePath, deviceName);
                    EnsureRemoteDirectory(client, remoteDeviceDir);

                    var jsonFiles = Directory.GetFiles(deviceDir, "*.json", SearchOption.TopDirectoryOnly);

                    foreach (var localFile in jsonFiles)
                    {
                        var fileName = Path.GetFileName(localFile);
                        var remoteFilePath = CombineUnixPath(remoteDeviceDir, fileName);

                        UploadFile(client, localFile, remoteFilePath);
                    }
                }

                client.Disconnect();
            }
        }

        private static string CombineUnixPath(params string[] parts)
        {
            var cleaned = parts
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => p.Trim().Replace("\\", "/").TrimEnd('/'))
                .ToArray();

            var combined = string.Join("/", cleaned);
            if (!combined.StartsWith("/")) combined = "/" + combined;
            return combined;
        }

        private static void EnsureRemoteDirectory(SftpClient client, string path)
        {
            var normalized = path.Replace("\\", "/");
            if (!normalized.StartsWith("/")) normalized = "/" + normalized;

            var segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var current = "";

            foreach (var segment in segments)
            {
                current += "/" + segment;
                if (!client.Exists(current))
                {
                    client.CreateDirectory(current);
                }
            }
        }

        private static void UploadFile(SftpClient client, string localPath, string remotePath)
        {
            using (var fs = File.OpenRead(localPath))
            {
                // true = overwrite if exists
                client.UploadFile(fs, remotePath, true);
            }

            // TODO: replace with your logger
            Console.WriteLine($"[FTP] Uploaded {localPath} -> {remotePath}");
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
