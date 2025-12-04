using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;  // Add reference to System.Management
using System.Threading;
using System.Threading.Tasks;
using DeviceTrackerClient.Configuration;
using DeviceTrackerClient.Core.Models;
using Newtonsoft.Json;

namespace DeviceTrackerClient.Services
{
    public class AppBlockerService : IDisposable
    {
        private readonly LoggerService _logger;
        private readonly ClientConfig _config;
        private readonly string _blockRulesPath;

        private List<BlockRule> _blockRules = new List<BlockRule>();
        private ManagementEventWatcher _processStartWatcher;
        private Timer _periodicChecker;
        private Dictionary<int, DateTime> _recentlyBlocked = new Dictionary<int, DateTime>();
        private bool _isDisposed = false;

        public AppBlockerService(LoggerService logger)
        {
            _logger = logger;
            _config = ClientConfig.Load();

            _blockRulesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "DeviceTracker",
                "block_rules.json");

            LoadBlockRules();
        }

        public void Start()
        {
            try
            {
                // Start real-time WMI monitoring
                StartWmiMonitoring();

                // Also run periodic checks as backup
                _periodicChecker = new Timer(PeriodicCheck, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

                _logger.LogSystemEvent("AppBlockerStarted", Environment.UserName,
                    new Dictionary<string, object> { { "RuleCount", _blockRules.Count(r => r.IsEnabled) } });
            }
            catch (Exception ex)
            {
                _logger.LogSystemEvent("AppBlockerStartError", Environment.UserName,
                    new Dictionary<string, object> { { "Error", ex.Message } });

                // Fallback to timer-only mode
                _periodicChecker = new Timer(PeriodicCheck, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            }
        }

        public void Stop()
        {
            _processStartWatcher?.Stop();
            _processStartWatcher?.Dispose();
            _periodicChecker?.Dispose();
        }

        private void StartWmiMonitoring()
        {
            try
            {
                // Query for process creation events
                string query = @"SELECT * FROM Win32_ProcessStartTrace";

                _processStartWatcher = new ManagementEventWatcher(query);
                _processStartWatcher.EventArrived += ProcessStartWatcher_EventArrived;
                _processStartWatcher.Start();

                Console.WriteLine("WMI Process monitoring started.");
            }
            catch (Exception ex)
            {
                _logger.LogSystemEvent("WMIStartError", Environment.UserName,
                    new Dictionary<string, object> { { "Error", ex.Message } });
                throw;
            }
        }

        private void ProcessStartWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                // Get process details from WMI event
                string processName = e.NewEvent.Properties["ProcessName"].Value.ToString();
                int processId = Convert.ToInt32(e.NewEvent.Properties["ProcessId"].Value);
                string commandLine = e.NewEvent.Properties["ProcessId"].Value.ToString();

                // Skip system processes
                if (IsSystemProcess(processName)) return;

                // Skip if recently blocked (avoid duplicate handling)
                if (_recentlyBlocked.ContainsKey(processId) &&
                    (DateTime.Now - _recentlyBlocked[processId]).TotalSeconds < 5)
                {
                    return;
                }

                // Get process object to check window title and path
                Process process = null;
                try
                {
                    process = Process.GetProcessById(processId);

                    // Check if this process should be blocked
                    if (ShouldBlockProcess(process))
                    {
                        BlockProcessImmediately(process);
                    }
                }
                catch
                {
                    // Process might have already terminated
                }
                finally
                {
                    process?.Dispose();
                }
            }
            catch (Exception ex)
            {
                // Log but don't crash
                System.Diagnostics.Debug.WriteLine($"WMI Event Error: {ex.Message}");
            }
        }

        private void PeriodicCheck(object state)
        {
            if (_isDisposed) return;

            try
            {
                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    try
                    {
                        if (ShouldBlockProcess(process))
                        {
                            BlockProcessImmediately(process);
                        }
                    }
                    catch
                    {
                        // Process access denied or already terminated
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

                // Clean up recently blocked cache (older than 1 minute)
                CleanRecentlyBlockedCache();
            }
            catch (Exception ex)
            {
                _logger.LogSystemEvent("PeriodicCheckError", Environment.UserName,
                    new Dictionary<string, object> { { "Error", ex.Message } });
            }
        }

        private bool ShouldBlockProcess(Process process)
        {
            // Skip system processes
            if (IsSystemProcess(process.ProcessName)) return false;

            // Skip if recently blocked
            if (_recentlyBlocked.ContainsKey(process.Id) &&
                (DateTime.Now - _recentlyBlocked[process.Id]).TotalSeconds < 30)
            {
                return false;
            }

            string processName = process.ProcessName.ToLower();
            string windowTitle = string.Empty;
            string filePath = string.Empty;

            try { windowTitle = process.MainWindowTitle?.ToLower() ?? ""; } catch { }
            try { filePath = process.MainModule?.FileName?.ToLower() ?? ""; } catch { }

            // Check each enabled blocking rule
            foreach (var rule in _blockRules.Where(r => r.IsEnabled))
            {
                if (MatchesRule(processName, windowTitle, filePath, rule))
                {
                    return true;
                }
            }

            return false;
        }

        private bool MatchesRule(string processName, string windowTitle, string filePath, BlockRule rule)
        {
            string pattern = rule.Pattern.ToLower();
            string processLower = processName.ToLower();
            string windowLower = windowTitle.ToLower();
            string fileLower = filePath.ToLower();

            switch (rule.MatchType)
            {
                case "ProcessName":
                    // Match process name (partial)
                    return processLower.Contains(pattern);

                case "WindowTitle":
                    // Match window title
                    return !string.IsNullOrEmpty(windowLower) &&
                           windowLower.Contains(pattern);

                case "FilePath":
                    // Match file path
                    return !string.IsNullOrEmpty(fileLower) &&
                           fileLower.Contains(pattern);

                case "ExactProcessName":
                    // Exact process name match
                    return processLower == pattern ||
                           processLower == pattern + ".exe";

                case "ExactFilePath":
                    // Exact file path match
                    return fileLower == pattern.ToLower();

                case "StartsWith":
                    // Process name starts with pattern
                    return processLower.StartsWith(pattern);

                case "EndsWith":
                    // Process name ends with pattern
                    return processLower.EndsWith(pattern) ||
                           processLower.EndsWith(pattern + ".exe");

                default:
                    return false;
            }
        }
        public void DebugBlocking(string testAppName)
        {
            try
            {
                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.ProcessName.ToLower().Contains(testAppName.ToLower()))
                        {
                            string processName = process.ProcessName.ToLower();
                            string windowTitle = "";
                            string filePath = "";

                            try { windowTitle = process.MainWindowTitle?.ToLower() ?? ""; } catch { }
                            try { filePath = process.MainModule?.FileName?.ToLower() ?? ""; } catch { }

                            Console.WriteLine($"DEBUG: Found {testAppName}:");
                            Console.WriteLine($"  Process: {processName}");
                            Console.WriteLine($"  Window: {windowTitle}");
                            Console.WriteLine($"  Path: {filePath}");

                            // Check each rule
                            foreach (var rule in _blockRules.Where(r => r.IsEnabled))
                            {
                                bool matches = MatchesRule(processName, windowTitle, filePath, rule);
                                Console.WriteLine($"  Rule '{rule.Pattern}' ({rule.MatchType}): {matches}");
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Error: {ex.Message}");
            }
        }
        private void BlockProcessImmediately(Process process)
        {
            try
            {
                Console.WriteLine($"DEBUG: Attempting to block process: {process.ProcessName} (ID: {process.Id})");

                // Log all details
                string details = $"Process: {process.ProcessName}, ID: {process.Id}, " +
                                $"HasExited: {process.HasExited}, Responding: {process.Responding}";
                Console.WriteLine($"DEBUG: {details}");

                // Log before blocking
                LogBlockAttempt(process);

                // Try graceful termination first
                if (!process.HasExited && process.Responding)
                {
                    Console.WriteLine("DEBUG: Attempting graceful termination...");
                    bool closed = process.CloseMainWindow();
                    Console.WriteLine($"DEBUG: CloseMainWindow returned: {closed}");
                    Thread.Sleep(200); // Longer wait
                }

                // Force kill if still running
                if (!process.HasExited)
                {
                    Console.WriteLine("DEBUG: Force killing process...");
                    process.Kill();
                    bool exited = process.WaitForExit(1000);
                    Console.WriteLine($"DEBUG: Kill completed. Exited: {exited}");
                }
                else
                {
                    Console.WriteLine("DEBUG: Process already exited.");
                }

                // Log successful block
                LogBlockedApp(process);
                Console.WriteLine($"DEBUG: Successfully blocked {process.ProcessName}");

                // Add to recently blocked cache
                _recentlyBlocked[process.Id] = DateTime.Now;

                // Increment trigger count for the matching rule
                IncrementRuleTriggerCount(process);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error blocking process: {ex.Message}");
                Console.WriteLine($"DEBUG: StackTrace: {ex.StackTrace}");

                _logger.LogSystemEvent("AppBlockFailed", Environment.UserName,
                    new Dictionary<string, object>
                    {
                { "Process", process.ProcessName },
                { "ProcessId", process.Id },
                { "Error", ex.Message },
                { "StackTrace", ex.StackTrace }
                    });
            }
        }

        private void LogBlockAttempt(Process process)
        {
            try
            {
                string windowTitle = "";
                string filePath = "";

                try { windowTitle = process.MainWindowTitle; } catch { }
                try { filePath = process.MainModule?.FileName ?? ""; } catch { }

                _logger.LogSystemEvent("AppBlockAttempt", Environment.UserName,
                    new Dictionary<string, object>
                    {
                        { "ProcessName", process.ProcessName },
                        { "ProcessId", process.Id },
                        { "WindowTitle", windowTitle },
                        { "FilePath", filePath },
                        { "Timestamp", DateTime.Now }
                    });
            }
            catch { }
        }

        private void LogBlockedApp(Process process)
        {
            try
            {
                string windowTitle = "";
                string filePath = "";

                try { windowTitle = process.MainWindowTitle; } catch { }
                try { filePath = process.MainModule?.FileName ?? ""; } catch { }

                _logger.LogSystemEvent("AppBlocked", Environment.UserName,
                    new Dictionary<string, object>
                    {
                        { "ProcessName", process.ProcessName },
                        { "ProcessId", process.Id },
                        { "WindowTitle", windowTitle },
                        { "FilePath", filePath },
                        { "Timestamp", DateTime.Now }
                    });
            }
            catch { }
        }

        private void IncrementRuleTriggerCount(Process process)
        {
            try
            {
                string processName = process.ProcessName.ToLower();
                string windowTitle = string.Empty;
                string filePath = string.Empty;

                try { windowTitle = process.MainWindowTitle?.ToLower() ?? ""; } catch { }
                try { filePath = process.MainModule?.FileName?.ToLower() ?? ""; } catch { }

                foreach (var rule in _blockRules)
                {
                    if (MatchesRule(processName, windowTitle, filePath, rule))
                    {
                        rule.TriggerCount++;
                        rule.LastTriggered = DateTime.Now;
                        SaveBlockRules();
                        break;
                    }
                }
            }
            catch { }
        }

        private bool IsSystemProcess(string processName)
        {
            string[] systemProcesses =
            {
                "svchost", "services", "lsass", "wininit", "csrss",
                "smss", "system", "system idle process", "winlogon",
                "taskhost", "dwm", "explorer", "conhost", "sihost",
                "runtimebroker", "ctfmon", "dllhost", "taskmgr"
            };

            return systemProcesses.Contains(processName.ToLower());
        }

        private void CleanRecentlyBlockedCache()
        {
            var toRemove = _recentlyBlocked
                .Where(kvp => (DateTime.Now - kvp.Value).TotalMinutes > 2)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in toRemove)
            {
                _recentlyBlocked.Remove(key);
            }
        }

        // ===== RULE MANAGEMENT =====

        public void AddBlockRule(BlockRule rule)
        {
            // Check if rule already exists
            if (_blockRules.Any(r => r.Name.Equals(rule.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Rule '{rule.Name}' already exists.");
            }

            _blockRules.Add(rule);
            SaveBlockRules();

            _logger.LogSystemEvent("BlockRuleAdded", Environment.UserName,
                new Dictionary<string, object>
                {
                    { "RuleName", rule.Name },
                    { "Pattern", rule.Pattern },
                    { "MatchType", rule.MatchType },
                    { "IsEnabled", rule.IsEnabled }
                });
        }

        public void RemoveBlockRule(string ruleName)
        {
            var rule = _blockRules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
            if (rule != null)
            {
                _blockRules.Remove(rule);
                SaveBlockRules();

                _logger.LogSystemEvent("BlockRuleRemoved", Environment.UserName,
                    new Dictionary<string, object> { { "RuleName", ruleName } });
            }
        }

        public void UpdateBlockRule(BlockRule updatedRule)
        {
            var existingRule = _blockRules.FirstOrDefault(r => r.Name.Equals(updatedRule.Name, StringComparison.OrdinalIgnoreCase));
            if (existingRule != null)
            {
                existingRule.Pattern = updatedRule.Pattern;
                existingRule.MatchType = updatedRule.MatchType;
                existingRule.IsEnabled = updatedRule.IsEnabled;
                existingRule.UseGracefulTermination = updatedRule.UseGracefulTermination;
                SaveBlockRules();
            }
        }

        public List<BlockRule> GetBlockRules()
        {
            return new List<BlockRule>(_blockRules);
        }

        public void EnableBlockRule(string ruleName, bool enable)
        {
            var rule = _blockRules.FirstOrDefault(r => r.Name.Equals(ruleName, StringComparison.OrdinalIgnoreCase));
            if (rule != null)
            {
                rule.IsEnabled = enable;
                SaveBlockRules();
            }
        }

        // Quick block methods
        public void BlockByProcessName(string processName, bool exactMatch = false)
        {
            AddBlockRule(new BlockRule
            {
                Name = $"Block {processName}",
                Pattern = processName,
                MatchType = exactMatch ? "ExactProcessName" : "ProcessName",
                IsEnabled = true,
                UseGracefulTermination = false,
                CreatedAt = DateTime.Now
            });
        }

        public void BlockByWindowTitle(string windowTitle)
        {
            AddBlockRule(new BlockRule
            {
                Name = $"Block window: {windowTitle}",
                Pattern = windowTitle,
                MatchType = "WindowTitle",
                IsEnabled = true,
                UseGracefulTermination = true,
                CreatedAt = DateTime.Now
            });
        }

        public void BlockByFilePath(string filePath, bool exactMatch = false)
        {
            AddBlockRule(new BlockRule
            {
                Name = $"Block file: {Path.GetFileName(filePath)}",
                Pattern = filePath,
                MatchType = exactMatch ? "ExactFilePath" : "FilePath",
                IsEnabled = true,
                UseGracefulTermination = false,
                CreatedAt = DateTime.Now
            });
        }

        // ===== DATA PERSISTENCE =====

        private void LoadBlockRules()
        {
            try
            {
                if (File.Exists(_blockRulesPath))
                {
                    var json = File.ReadAllText(_blockRulesPath);
                    _blockRules = JsonConvert.DeserializeObject<List<BlockRule>>(json)
                                ?? new List<BlockRule>();
                }

                // Add default test rule if empty
                if (_blockRules.Count == 0)
                {
                    AddDefaultRules();
                }
            }
            catch (Exception ex)
            {
                _logger.LogSystemEvent("LoadBlockRulesError", Environment.UserName,
                    new Dictionary<string, object> { { "Error", ex.Message } });
                _blockRules = new List<BlockRule>();
                AddDefaultRules();
            }
        }

        private void AddDefaultRules()
        {
            // Add NOTEPAD as a default blocked app (ENABLED for testing)
            _blockRules.Add(new BlockRule
            {
                Name = "Block Notepad",
                Pattern = "notepad",  // This will match "notepad.exe"
                MatchType = "ProcessName",
                IsEnabled = true,  // ENABLED BY DEFAULT FOR TESTING
                UseGracefulTermination = false,
                CreatedAt = DateTime.Now
            });

            // You can add more default rules here...

            SaveBlockRules();
        }

        private void SaveBlockRules()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_blockRules, Formatting.Indented);
                File.WriteAllText(_blockRulesPath, json);

                // Also update config
                _config.BlockedApplications = _blockRules
                    .Where(r => r.IsEnabled)
                    .Select(r => r.Pattern)
                    .ToList();
                _config.Save();
            }
            catch (Exception ex)
            {
                _logger.LogSystemEvent("SaveBlockRulesError", Environment.UserName,
                    new Dictionary<string, object> { { "Error", ex.Message } });
            }
        }

        // ===== TEST METHODS =====

        public void TestBlocking()
        {
            try
            {
                // Test by trying to start notepad
                Process.Start("notepad.exe");
                _logger.LogSystemEvent("BlockerTest", Environment.UserName,
                    new Dictionary<string, object> { { "Test", "Started notepad to test blocking" } });
            }
            catch { }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                Stop();
            }
        }
    }

    public class BlockRule
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string MatchType { get; set; } // ProcessName, WindowTitle, FilePath, ExactProcessName, ExactFilePath, StartsWith
        public bool IsEnabled { get; set; } = true;
        public bool UseGracefulTermination { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastTriggered { get; set; }
        public int TriggerCount { get; set; } = 0;
    }
}