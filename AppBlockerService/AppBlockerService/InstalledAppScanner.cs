using Microsoft.Win32;
using SharedModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AppBlockerService
{
    public class InstalledAppScanner
    {
        private readonly Logger _logger;

        public InstalledAppScanner(Logger logger)
        {
            _logger = logger;
        }

        public List<InstalledApp> ScanAll()
        {
            var apps = new List<InstalledApp>();

            apps.AddRange(ScanRegistry());
            apps.AddRange(ScanProgramFiles());
            apps.AddRange(ScanWindowsApps());
            apps.AddRange(ScanAppData());
            apps.AddRange(ScanPathDirectories());
            apps.AddRange(ScanStartMenu());
            apps.AddRange(ScanSystemDirectories());

            // Remove duplicates
            return apps.GroupBy(a => a.ExePath?.ToLowerInvariant())
                      .Select(g => g.First())
                      .ToList();
        }

        private List<InstalledApp> ScanRegistry()
        {
            var apps = new List<InstalledApp>();
            var registryKeys = new[]
            {
                @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
                @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var keyPath in registryKeys)
            {
                // HKLM
                using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                {
                    if (key != null)
                        apps.AddRange(ScanRegistryKey(key, "RegistryHKLM"));
                }

                // HKCU
                using (var key = Registry.CurrentUser.OpenSubKey(keyPath))
                {
                    if (key != null)
                        apps.AddRange(ScanRegistryKey(key, "RegistryHKCU"));
                }
            }

            return apps;
        }

        private List<InstalledApp> ScanRegistryKey(RegistryKey key, string source)
        {
            var apps = new List<InstalledApp>();

            foreach (string subKeyName in key.GetSubKeyNames())
            {
                try
                {
                    using (var subKey = key.OpenSubKey(subKeyName))
                    {
                        var displayName = subKey?.GetValue("DisplayName") as string;
                        var displayIcon = subKey?.GetValue("DisplayIcon") as string;
                        var installLocation = subKey?.GetValue("InstallLocation") as string;
                        var uninstallString = subKey?.GetValue("UninstallString") as string;

                        string exePath = null;

                        // Try to extract EXE path from DisplayIcon
                        if (!string.IsNullOrEmpty(displayIcon) && displayIcon.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            exePath = CleanIconPath(displayIcon);
                        }

                        // Try to find EXE in install location
                        if (string.IsNullOrEmpty(exePath) && !string.IsNullOrEmpty(installLocation))
                        {
                            var exeFiles = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);
                            if (exeFiles.Length > 0)
                                exePath = exeFiles[0];
                        }

                        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                        {
                            apps.Add(new InstalledApp
                            {
                                DisplayName = displayName ?? Path.GetFileNameWithoutExtension(exePath),
                                ExePath = exePath,
                                Source = source,
                                DiscoveredAt = DateTime.UtcNow
                            });
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
                            { "RegistryScanError", ex.Message },
                            { "Key", key.Name }
                        }
                    });
                }
            }

            return apps;
        }

        private string CleanIconPath(string iconPath)
        {
            // Remove icon index (e.g., "C:\Path\app.exe,0")
            if (iconPath.Contains(','))
                iconPath = iconPath.Substring(0, iconPath.IndexOf(','));

            // Remove quotes
            return iconPath.Trim('"');
        }

        private List<InstalledApp> ScanProgramFiles()
        {
            var apps = new List<InstalledApp>();
            var programFilesPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var path in programFilesPaths.Where(Directory.Exists))
            {
                apps.AddRange(ScanDirectoryForExes(path, "ProgramFiles"));
            }

            return apps;
        }

        private List<InstalledApp> ScanWindowsApps()
        {
            var apps = new List<InstalledApp>();
            var windowsAppsPath = @"C:\Program Files\WindowsApps";

            if (Directory.Exists(windowsAppsPath))
            {
                apps.AddRange(ScanDirectoryForExes(windowsAppsPath, "WindowsApps"));
            }

            return apps;
        }

        private List<InstalledApp> ScanAppData()
        {
            var apps = new List<InstalledApp>();
            var usersPath = @"C:\Users";

            if (!Directory.Exists(usersPath))
                return apps;

            foreach (var userDir in Directory.GetDirectories(usersPath))
            {
                var userName = Path.GetFileName(userDir);
                if (userName.Equals("Public", StringComparison.OrdinalIgnoreCase))
                    continue;

                var appDataPaths = new[]
                {
                    Path.Combine(userDir, "AppData", "Local"),
                    Path.Combine(userDir, "AppData", "Local", "Programs"),
                    Path.Combine(userDir, "AppData", "Roaming")
                };

                foreach (var path in appDataPaths.Where(Directory.Exists))
                {
                    apps.AddRange(ScanDirectoryForExes(path, $"AppData-{userName}"));
                }
            }

            return apps;
        }

        private List<InstalledApp> ScanPathDirectories()
        {
            var apps = new List<InstalledApp>();
            var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

            foreach (var dir in path.Split(';').Where(Directory.Exists))
            {
                try
                {
                    var exeFiles = Directory.GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly);
                    foreach (var exeFile in exeFiles)
                    {
                        apps.Add(new InstalledApp
                        {
                            DisplayName = Path.GetFileNameWithoutExtension(exeFile),
                            ExePath = exeFile,
                            Source = "PATH",
                            DiscoveredAt = DateTime.UtcNow
                        });
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip directories we can't access
                }
            }

            return apps;
        }

        private List<InstalledApp> ScanStartMenu()
        {
            var apps = new List<InstalledApp>();
            var startMenuPaths = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
            };

            foreach (var path in startMenuPaths.Where(Directory.Exists))
            {
                apps.AddRange(ScanDirectoryForShortcuts(path, "StartMenu"));
            }

            return apps;
        }

        private List<InstalledApp> ScanDirectoryForShortcuts(string directory, string source)
        {
            var apps = new List<InstalledApp>();

            try
            {
                var shortcutFiles = Directory.GetFiles(directory, "*.lnk", SearchOption.AllDirectories);

                foreach (var shortcut in shortcutFiles)
                {
                    try
                    {
                        var targetPath = ResolveShortcut(shortcut);
                        if (!string.IsNullOrEmpty(targetPath) && targetPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            apps.Add(new InstalledApp
                            {
                                DisplayName = Path.GetFileNameWithoutExtension(shortcut),
                                ExePath = targetPath,
                                Source = source,
                                DiscoveredAt = DateTime.UtcNow
                            });
                        }
                    }
                    catch
                    {
                        // Skip invalid shortcuts
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }

            return apps;
        }

        private string ResolveShortcut(string shortcutPath)
        {
            try
            {
                // Alternative method without COM reference
                // Read the shortcut file manually
                using (var fs = new FileStream(shortcutPath, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    fs.Seek(0x14, SeekOrigin.Begin);     // Skip header
                    fs.Seek(0x4, SeekOrigin.Current);    // Skip file size

                    int flags = br.ReadInt32();
                    bool hasTargetIdList = (flags & 0x01) > 0;

                    if (hasTargetIdList)
                    {
                        int targetIdListSize = br.ReadInt16();
                        fs.Seek(targetIdListSize, SeekOrigin.Current);
                    }

                    int fileAttributes = br.ReadInt32();
                    long creationTime = br.ReadInt64();
                    long accessTime = br.ReadInt64();
                    long writeTime = br.ReadInt64();

                    int fileSize = br.ReadInt32();
                    int iconIndex = br.ReadInt32();

                    int showCommand = br.ReadInt32();
                    short hotKey = br.ReadInt16();
                    short reserved1 = br.ReadInt16();
                    int reserved2 = br.ReadInt32();
                    int reserved3 = br.ReadInt32();

                    // Read the path strings
                    int pathOffset = br.ReadInt32();

                    // Save position
                    long savedPos = fs.Position;

                    // Read target path
                    fs.Seek(pathOffset, SeekOrigin.Begin);
                    List<byte> pathBytes = new List<byte>();
                    byte b = br.ReadByte();
                    while (b != 0)
                    {
                        pathBytes.Add(b);
                        b = br.ReadByte();
                    }

                    string targetPath = Encoding.Unicode.GetString(pathBytes.ToArray());

                    // Return to saved position
                    fs.Seek(savedPos, SeekOrigin.Begin);

                    return targetPath;
                }
            }
            catch
            {
                // If binary reading fails, try using Windows Script Host Shell
                try
                {
                    Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); // Windows Script Host Shell Object
                    dynamic shell = Activator.CreateInstance(t);
                    var shortcut = shell.CreateShortcut(shortcutPath);
                    string targetPath = shortcut.TargetPath;
                    Marshal.ReleaseComObject(shell);
                    return targetPath;
                }
                catch
                {
                    return null;
                }
            }
        }

        private List<InstalledApp> ScanSystemDirectories()
        {
            var apps = new List<InstalledApp>();
            var systemPaths = new[]
            {
                Path.Combine(Environment.SystemDirectory),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64")
            };

            foreach (var path in systemPaths.Where(Directory.Exists))
            {
                var systemApps = ScanDirectoryForExes(path, "System");
                foreach (var app in systemApps)
                {
                    app.IsSystem = true;
                }
                apps.AddRange(systemApps);
            }

            return apps;
        }

        private List<InstalledApp> ScanDirectoryForExes(string directory, string source)
        {
            var apps = new List<InstalledApp>();

            try
            {
                var exeFiles = Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories);

                foreach (var exeFile in exeFiles)
                {
                    try
                    {
                        apps.Add(new InstalledApp
                        {
                            DisplayName = Path.GetFileNameWithoutExtension(exeFile),
                            ExePath = exeFile,
                            Source = source,
                            DiscoveredAt = DateTime.UtcNow
                        });
                    }
                    catch (PathTooLongException)
                    {
                        // Skip files with too long paths
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Skip directories we can't access
            }
            catch (DirectoryNotFoundException)
            {
                // Skip non-existent directories
            }

            return apps;
        }
    }
}