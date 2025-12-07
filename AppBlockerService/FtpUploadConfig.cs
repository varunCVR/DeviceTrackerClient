using System;
using System.IO;
using Newtonsoft.Json;

namespace AppBlockerService
{
    public class FtpUploadConfig
    {
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; } = 22;
        public bool Enabled { get; set; } = true;
        public int UploadIntervalSeconds { get; set; } = 60;
        public int RetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;
        public string RemoteBasePath { get; set; } = "/logs";
        public string LocalLogsPath { get; set; } =
            @"C:\ProgramData\AppBlocker\logs";

        public static FtpUploadConfig Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"FTP config not found at {path}");

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<FtpUploadConfig>(json)
                   ?? throw new InvalidOperationException("Invalid FTP config JSON.");
        }
    }
}
