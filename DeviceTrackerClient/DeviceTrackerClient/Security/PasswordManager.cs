using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace DeviceTrackerClient
{
    public class PasswordManager
    {
        private readonly string passwordFile;
        private readonly byte[] encryptionKey;

        public PasswordManager()
        {
            passwordFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DeviceTracker", "config.json");
            // Use a fixed key for demo - in production, generate a unique key per installation
            encryptionKey = Encoding.UTF8.GetBytes("16ByteKey12345678"); // 16 bytes for AES
        }

        public void SetPassword(string password)
        {
            var config = new PasswordConfig
            {
                PasswordHash = EncryptString(password),
                IsPasswordSet = true
            };

            Directory.CreateDirectory(Path.GetDirectoryName(passwordFile));
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(passwordFile, json);
        }

        public bool VerifyPassword(string password)
        {
            if (!File.Exists(passwordFile))
                return false; // No password set yet

            var json = File.ReadAllText(passwordFile);
            var config = JsonConvert.DeserializeObject<PasswordConfig>(json);

            if (config == null || !config.IsPasswordSet)
                return false;

            string decryptedPassword = DecryptString(config.PasswordHash);
            return password == decryptedPassword;
        }

        public bool IsPasswordSet()
        {
            if (!File.Exists(passwordFile)) return false;

            var json = File.ReadAllText(passwordFile);
            var config = JsonConvert.DeserializeObject<PasswordConfig>(json);
            return config?.IsPasswordSet ?? false;
        }

        private string EncryptString(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.IV = new byte[16]; // Zero IV for demo

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string DecryptString(string cipherText)
        {
            var buffer = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.Key = encryptionKey;
                aes.IV = new byte[16]; // Zero IV for demo

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(buffer))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }

    public class PasswordConfig
    {
        public string PasswordHash { get; set; }
        public bool IsPasswordSet { get; set; }
    }
}