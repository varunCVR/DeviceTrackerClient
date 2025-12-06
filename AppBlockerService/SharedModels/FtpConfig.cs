using System;
using System.IO;

namespace SharedModels
{
    public class FtpConfig
    {
        public string Server { get; set; } = "ict75k.ldts.in";
        public string Username { get; set; } = "ftpUpload@ict75k.ldts.in";
        public string Password { get; set; } = "ICT75k@2023";
        public int Port { get; set; } = 22; // Default FTP port
        public bool Enabled { get; set; } = true;
        public int UploadIntervalSeconds { get; set; } = 60; // Upload every 60 seconds
        public int RetryCount { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 5;

        // Remote directory structure
        public string RemoteBasePath { get; set; } = "/logs";

        // Local paths
        public string LocalLogsPath { get; set; } = @"C:\ProgramData\AppBlocker\logs";

        public string GetRemoteMachinePath(string machineName)
        {
            // Clean machine name for path (remove invalid characters)
            string cleanName = CleanPathName(machineName);
            return $"{RemoteBasePath}/{cleanName}";
        }

        public string GetRemoteFilePath(string machineName, string fileName)
        {
            return $"{GetRemoteMachinePath(machineName)}/{fileName}";
        }

        private string CleanPathName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "unknown";

            // Remove invalid path characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                name = name.Replace(c.ToString(), "_");
            }

            return name.Trim();
        }
    }
}