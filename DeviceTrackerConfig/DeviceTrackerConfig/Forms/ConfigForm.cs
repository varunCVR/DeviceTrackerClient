using DeviceTrackerClient;
using DeviceTrackerClient.Communication;
using DeviceTrackerClient.Configuration;
using DeviceTrackerConfig.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;

namespace DeviceTrackerConfig.Forms
{
    public partial class ConfigForm : Form
    {
        private ClientConfig _config;
        private PasswordManager _passwordManager;
        private AppDetectorService _appDetector;
        private List<AppDetectorService.DetectedApp> _detectedApps;
        private List<string> _blockedApps;

        public ConfigForm()
        {
            InitializeComponent();
            InitializeServices();
            LoadSettings();
        }

        private void InitializeServices()
        {
            try
            {
                _config = ClientConfig.Load();
                _passwordManager = new PasswordManager();
                _appDetector = new AppDetectorService();
                _detectedApps = new List<AppDetectorService.DetectedApp>();
                _blockedApps = new List<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing services: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSettings()
        {
            try
            {
                // Find controls in Settings tab
                var txtServerUrl = tabSettings.Controls.Find("txtServerUrl", true).FirstOrDefault() as TextBox;
                var txtClientId = tabSettings.Controls.Find("txtClientId", true).FirstOrDefault() as TextBox;
                var txtMachineName = tabSettings.Controls.Find("txtMachineName", true).FirstOrDefault() as TextBox;
                var chkAutoStart = tabSettings.Controls.Find("chkAutoStart", true).FirstOrDefault() as CheckBox;
                var chkServiceInstalled = tabSettings.Controls.Find("chkServiceInstalled", true).FirstOrDefault() as CheckBox;

                if (txtServerUrl != null) txtServerUrl.Text = _config.ServerUrl;
                if (txtClientId != null) txtClientId.Text = _config.ClientId ?? "Not generated yet";
                if (txtMachineName != null) txtMachineName.Text = Environment.MachineName;
                if (chkAutoStart != null) chkAutoStart.Checked = _config.AutoStart;

                // Check if service is installed
                if (chkServiceInstalled != null) chkServiceInstalled.Checked = IsServiceInstalled();

                // Generate client ID if not exists
                if (string.IsNullOrEmpty(_config.ClientId))
                {
                    _config.ClientId = GenerateClientId();
                    _config.Save();
                    if (txtClientId != null) txtClientId.Text = _config.ClientId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDetectedApps()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // Clear existing items
                lstDetectedApps.Items.Clear();
                _blockedApps.Clear();

                // Get blocked apps
                _blockedApps = _appDetector.GetBlockedApps();

                // Scan for apps
                lblStatus.Text = "Scanning for applications...";
                lblStatus.ForeColor = Color.Blue;
                Application.DoEvents();

                _detectedApps = _appDetector.ScanAllApplications();

                // Populate list
                foreach (var app in _detectedApps)
                {
                    // Check if app is blocked
                    bool isBlocked = _blockedApps.Any(b =>
                        app.Name.ToLower().Contains(b.ToLower()) ||
                        app.ProcessName.ToLower().Contains(b.ToLower()) ||
                        (app.FilePath != null && app.FilePath.ToLower().Contains(b.ToLower())));

                    app.IsBlocked = isBlocked;

                    // Create list item
                    string status = isBlocked ? "[BLOCKED]" : "";
                    string fileName = !string.IsNullOrEmpty(app.FilePath) ?
                        Path.GetFileName(app.FilePath) : "Unknown";

                    var item = new ListViewItem(new[] {
                        status,
                        app.Name,
                        app.ProcessName,
                        app.Version,
                        app.Type,
                        fileName
                    });

                    item.Tag = app; // Store app object
                    item.ForeColor = isBlocked ? Color.Red : Color.Black;

                    lstDetectedApps.Items.Add(item);
                }

                lblStatus.Text = $"Found {_detectedApps.Count} applications";
                lblStatus.ForeColor = Color.Green;
                lblBlockedCount.Text = $"Blocked: {_detectedApps.Count(a => a.IsBlocked)} apps";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading apps: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error scanning applications";
                lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string GenerateClientId()
        {
            var random = new Random();
            return $"{Environment.MachineName}_{DateTime.Now:yyyyMMdd}_{random.Next(1000, 9999)}";
        }

        private bool IsServiceInstalled()
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController service in services)
                {
                    if (service.ServiceName.Equals("DeviceTrackerService", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // ===== EVENT HANDLERS FOR BLOCKING TAB =====

        private void btnRefreshApps_Click(object sender, EventArgs e)
        {
            LoadDetectedApps();
        }

        private void btnBlockSelected_Click(object sender, EventArgs e)
        {
            if (lstDetectedApps.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select applications to block.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedApps = new List<AppDetectorService.DetectedApp>();
            foreach (ListViewItem item in lstDetectedApps.SelectedItems)
            {
                selectedApps.Add((AppDetectorService.DetectedApp)item.Tag);
            }

            var result = MessageBox.Show($"Block {selectedApps.Count} selected application(s)?\n\n" +
                "They will be blocked by process name.",
                "Confirm Block", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                foreach (var app in selectedApps)
                {
                    AddBlockRule(app.ProcessName, "ProcessName");
                }

                MessageBox.Show($"{selectedApps.Count} application(s) blocked.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadDetectedApps(); // Refresh list
            }
        }

        private void btnUnblockSelected_Click(object sender, EventArgs e)
        {
            if (lstDetectedApps.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select applications to unblock.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"Unblock {lstDetectedApps.SelectedItems.Count} selected application(s)?",
                "Confirm Unblock", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                foreach (ListViewItem item in lstDetectedApps.SelectedItems)
                {
                    var app = (AppDetectorService.DetectedApp)item.Tag;
                    RemoveBlockRule(app.ProcessName);
                }

                LoadDetectedApps(); // Refresh
            }
        }

        private void btnTestBlocking_Click(object sender, EventArgs e)
        {
            try
            {
                // Test by opening notepad
                Process.Start("notepad.exe");
                MessageBox.Show("Notepad launched. If blocking is enabled, it should close immediately.\n\n" +
                    "Check C:\\ProgramData\\DeviceTracker\\logs for blocking events.",
                    "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportList_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text Files|*.txt|CSV Files|*.csv|All Files|*.*";
                saveDialog.Title = "Export Application List";
                saveDialog.FileName = $"Applications_{DateTime.Now:yyyyMMdd}.txt";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportAppList(saveDialog.FileName);
                }
            }
        }

        private void btnViewRules_Click(object sender, EventArgs e)
        {
            try
            {
                string rulesPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "DeviceTracker",
                    "block_rules.json");

                if (File.Exists(rulesPath))
                {
                    var json = File.ReadAllText(rulesPath);
                    var rules = JsonConvert.DeserializeObject<List<BlockRule>>(json) ?? new List<BlockRule>();

                    string ruleText = $"Block Rules ({rules.Count}):\n\n";
                    foreach (var rule in rules)
                    {
                        ruleText += $"{rule.Name}\n";
                        ruleText += $"  Pattern: {rule.Pattern}\n";
                        ruleText += $"  Type: {rule.MatchType}\n";
                        ruleText += $"  Enabled: {rule.IsEnabled}\n";
                        ruleText += $"  Triggered: {rule.TriggerCount} times\n";
                        if (rule.LastTriggered.HasValue)
                            ruleText += $"  Last: {rule.LastTriggered.Value:yyyy-MM-dd HH:mm:ss}\n";
                        ruleText += "\n";
                    }

                    MessageBox.Show(ruleText, "Block Rules",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No block rules found.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnQuickBlock_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtQuickBlock.Text))
            {
                AddBlockRule(txtQuickBlock.Text, "ProcessName");
                txtQuickBlock.Clear();
                LoadDetectedApps();
            }
        }

        private void btnQuickTest_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtQuickBlock.Text))
            {
                try
                {
                    // Try to start the app to test blocking
                    var processName = txtQuickBlock.Text.Replace(".exe", "");
                    var processes = Process.GetProcessesByName(processName);

                    if (processes.Length > 0)
                    {
                        MessageBox.Show($"{processName} is already running.\n" +
                            "If blocking works, it should close when you click OK.",
                            "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Trying to start {txtQuickBlock.Text}...\n" +
                            "If blocking works, it should close immediately.",
                            "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Process.Start(txtQuickBlock.Text);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lstDetectedApps_DoubleClick(object sender, EventArgs e)
        {
            if (lstDetectedApps.SelectedItems.Count > 0)
            {
                var app = (AppDetectorService.DetectedApp)lstDetectedApps.SelectedItems[0].Tag;

                var details = $"Name: {app.Name}\n" +
                             $"Process: {app.ProcessName}\n" +
                             $"Path: {app.FilePath}\n" +
                             $"Version: {app.Version}\n" +
                             $"Publisher: {app.Publisher}\n" +
                             $"Type: {app.Type}\n" +
                             $"Detected: {app.DetectedTime}\n" +
                             $"Status: {(app.IsBlocked ? "BLOCKED" : "Allowed")}";

                MessageBox.Show(details, "Application Details",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void txtQuickBlock_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnQuickBlock_Click(sender, e);
                e.Handled = true;
            }
        }

        // ===== BLOCKING FUNCTIONS =====

        private void AddBlockRule(string pattern, string matchType)
        {
            try
            {
                string rulesPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "DeviceTracker",
                    "block_rules.json");

                List<BlockRule> rules = new List<BlockRule>();

                if (File.Exists(rulesPath))
                {
                    var json = File.ReadAllText(rulesPath);
                    rules = JsonConvert.DeserializeObject<List<BlockRule>>(json) ?? new List<BlockRule>();
                }

                // Check if rule already exists
                if (!rules.Any(r => r.Pattern.Equals(pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    rules.Add(new BlockRule
                    {
                        Name = $"Block {pattern}",
                        Pattern = pattern,
                        MatchType = matchType,
                        IsEnabled = true,
                        UseGracefulTermination = false,
                        CreatedAt = DateTime.Now
                    });

                    var json = JsonConvert.SerializeObject(rules, Formatting.Indented);
                    File.WriteAllText(rulesPath, json);

                    MessageBox.Show($"Added block rule for: {pattern}", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Rule for {pattern} already exists.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding block rule: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveBlockRule(string pattern)
        {
            try
            {
                string rulesPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "DeviceTracker",
                    "block_rules.json");

                if (File.Exists(rulesPath))
                {
                    var json = File.ReadAllText(rulesPath);
                    var rules = JsonConvert.DeserializeObject<List<BlockRule>>(json) ?? new List<BlockRule>();

                    rules.RemoveAll(r => r.Pattern.Equals(pattern, StringComparison.OrdinalIgnoreCase));

                    json = JsonConvert.SerializeObject(rules, Formatting.Indented);
                    File.WriteAllText(rulesPath, json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing rule: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportAppList(string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"Application List - Generated on {DateTime.Now}");
                    writer.WriteLine($"Machine: {Environment.MachineName}");
                    writer.WriteLine($"Total Applications: {_detectedApps.Count}");
                    writer.WriteLine($"Blocked Applications: {_detectedApps.Count(a => a.IsBlocked)}");
                    writer.WriteLine();
                    writer.WriteLine("=".PadRight(100, '='));
                    writer.WriteLine();

                    foreach (var app in _detectedApps.OrderBy(a => a.Name))
                    {
                        writer.WriteLine($"Name: {app.Name}");
                        writer.WriteLine($"  Process: {app.ProcessName}");
                        writer.WriteLine($"  Path: {app.FilePath}");
                        writer.WriteLine($"  Version: {app.Version}");
                        writer.WriteLine($"  Type: {app.Type}");
                        writer.WriteLine($"  Status: {(app.IsBlocked ? "BLOCKED" : "Allowed")}");
                        writer.WriteLine();
                    }
                }

                MessageBox.Show($"List exported to:\n{filePath}", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== SETTINGS TAB EVENT HANDLERS =====

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var txtServerUrl = tabSettings.Controls.Find("txtServerUrl", true).FirstOrDefault() as TextBox;
                var chkAutoStart = tabSettings.Controls.Find("chkAutoStart", true).FirstOrDefault() as CheckBox;

                if (txtServerUrl != null) _config.ServerUrl = txtServerUrl.Text.Trim();
                if (chkAutoStart != null) _config.AutoStart = chkAutoStart.Checked;

                _config.Save();

                if (_config.AutoStart)
                {
                    SetAutoStart();
                }

                MessageBox.Show("Settings saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnInstallService_Click(object sender, EventArgs e)
        {
            try
            {
                string servicePath = FindServiceExecutable();

                if (string.IsNullOrEmpty(servicePath))
                {
                    MessageBox.Show("Could not find DeviceTrackerClient.exe\n\n" +
                        "Build the DeviceTrackerClient project first, then try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult result = MessageBox.Show(
                    $"Install DeviceTrackerService from:\n{servicePath}\n\n" +
                    "This will install the service to run automatically on startup.",
                    "Confirm Installation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                if (IsServiceInstalled())
                {
                    Process.Start("sc", "stop DeviceTrackerService").WaitForExit(5000);
                    Process.Start("sc", "delete DeviceTrackerService").WaitForExit(5000);
                }

                var installCmd = $"create DeviceTrackerService binPath= \"{servicePath}\" start= auto DisplayName= \"Device Tracker Service\"";
                var installProcess = Process.Start("sc", installCmd);
                installProcess.WaitForExit(5000);

                var startProcess = Process.Start("sc", "start DeviceTrackerService");
                startProcess.WaitForExit(5000);

                MessageBox.Show("Service installed and started successfully!\n\n" +
                    "Check Windows Services (services.msc) to verify.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var chkServiceInstalled = tabSettings.Controls.Find("chkServiceInstalled", true).FirstOrDefault() as CheckBox;
                if (chkServiceInstalled != null) chkServiceInstalled.Checked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error installing service: {ex.Message}\n\n" +
                    "Make sure you're running as Administrator.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "DeviceTracker",
                    "logs");

                if (Directory.Exists(logDir))
                {
                    Process.Start("explorer.exe", $"\"{logDir}\"");
                }
                else
                {
                    MessageBox.Show("Log directory not found. No logs have been created yet.\n\n" +
                        "Directory will be created when service starts: " + logDir,
                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log directory: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            using (var passwordForm = new PasswordForm())
            {
                if (passwordForm.ShowDialog(this) == DialogResult.OK)
                {
                    UninstallApplication();
                }
            }
        }

        private void SetAutoStart()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    string configToolPath = Application.ExecutablePath;
                    key.SetValue("DeviceTrackerConfig", $"\"{configToolPath}\" /minimized");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Warning: Could not set auto-start: {ex.Message}", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RemoveAutoStart()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.DeleteValue("DeviceTrackerConfig", false);
                }
            }
            catch { }
        }

        private void UninstallApplication()
        {
            try
            {
                DialogResult confirm = MessageBox.Show(
                    "This will:\n" +
                    "1. Stop DeviceTrackerService\n" +
                    "2. Remove the service\n" +
                    "3. Remove from startup\n" +
                    "4. Delete logs and data (optional)\n\n" +
                    "Are you sure you want to uninstall?",
                    "Confirm Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                    return;

                if (IsServiceInstalled())
                {
                    Process.Start("sc", "stop DeviceTrackerService").WaitForExit(5000);
                    Process.Start("sc", "delete DeviceTrackerService").WaitForExit(5000);
                }

                RemoveAutoStart();

                DialogResult deleteData = MessageBox.Show(
                    "Delete all logs and configuration data?\n\n" +
                    "This will remove all tracking history and settings.",
                    "Delete Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (deleteData == DialogResult.Yes)
                {
                    string dataDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "DeviceTracker");

                    if (Directory.Exists(dataDir))
                    {
                        Directory.Delete(dataDir, true);
                    }
                }

                MessageBox.Show("Uninstalled successfully!\n\n" +
                    "The application will now close.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during uninstall: {ex.Message}\n\n" +
                    "You may need to manually:\n" +
                    "1. Stop 'DeviceTrackerService' in Services\n" +
                    "2. Delete C:\\ProgramData\\DeviceTracker",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnFindProcess_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtQuickBlock.Text))
            {
                ProcessFinder.FindProcessInfo(txtQuickBlock.Text);
            }
            else
            {
                MessageBox.Show("Enter an application name first.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSmartBlock_Click(object sender, EventArgs e)
        {
            if (lstDetectedApps.SelectedItems.Count > 0)
            {
                var app = (AppDetectorService.DetectedApp)lstDetectedApps.SelectedItems[0].Tag;

                // Try multiple blocking strategies
                var result = MessageBox.Show($"How do you want to block {app.Name}?\n\n" +
                    "1. By Process Name: " + app.ProcessName + "\n" +
                    "2. By File Path: " + app.FilePath + "\n" +
                    "3. By Window Title (if running)",
                    "Smart Block", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Block by process name
                    AddBlockRule(app.ProcessName, "ProcessName");
                }
                else if (result == DialogResult.No)
                {
                    // Block by file path
                    if (!string.IsNullOrEmpty(app.FilePath) && app.FilePath != "Unknown")
                    {
                        AddBlockRule(app.FilePath, "FilePath");
                    }
                    else
                    {
                        MessageBox.Show("File path not available.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                LoadDetectedApps();
            }
        }
        private string FindServiceExecutable()
        {
            string[] possiblePaths =
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DeviceTrackerClient.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\DeviceTrackerClient\\bin\\Debug\\DeviceTrackerClient.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\DeviceTrackerClient\\bin\\Release\\DeviceTrackerClient.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Device Tracker\\DeviceTrackerClient.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Device Tracker\\DeviceTrackerClient.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DeviceTrackerClient.exe")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }
    }

    // Simple BlockRule class for JSON
    public class BlockRule
    {
        public string Name { get; set; }
        public string Pattern { get; set; }
        public string MatchType { get; set; }
        public bool IsEnabled { get; set; }
        public bool UseGracefulTermination { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastTriggered { get; set; }
        public int TriggerCount { get; set; }
    }
}