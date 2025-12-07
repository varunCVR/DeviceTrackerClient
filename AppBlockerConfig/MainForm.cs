using Newtonsoft.Json;
using SharedModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace AppBlockerConfig
{
    public partial class MainForm : Form
    {
        // Configuration paths
        private readonly string _configPath = @"C:\ProgramData\AppBlocker\config";
        private readonly string _logsPath = @"C:\ProgramData\AppBlocker\logs";
        private readonly string _blockListFile;
        private readonly string _inventoryFile;

        // Data
        private BlockList _blockList = new BlockList();
        private Inventory _inventory = new Inventory();
        private List<Process> _currentProcesses = new List<Process>();
        private ServiceController _serviceController;
        private System.Windows.Forms.Timer _refreshTimer; // Explicitly specify Forms Timer
        private List<DeviceHealthSnapshot> healthHistory = new List<DeviceHealthSnapshot>();
        private FileSystemWatcher _blockListWatcher;
        private string _ftpConfigFile;
        private FtpConfig _ftpConfig = new FtpConfig();
        // Thread safety
        private readonly object _processLock = new object();

        public MainForm()
        {
            InitializeComponent();

            this.Load += MainForm_Load;

            _blockListFile = Path.Combine(_configPath, "blocked_apps.json");
            _inventoryFile = Path.Combine(_logsPath, "installed_apps_inventory.json");
            _ftpConfigFile = Path.Combine(_configPath, "ftp_config.json"); // Add this
            // Ensure directories exist
            Directory.CreateDirectory(_configPath);
            Directory.CreateDirectory(_logsPath);

            // Setup auto-refresh timer - Use Forms Timer explicitly
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 3000; // 3 seconds
            _refreshTimer.Tick += RefreshTimer_Tick;

            // 🔹 Wire health timer here too (see next step)
            healthRefreshTimer.Interval = 5000;                // 5 sec UI refresh
            healthRefreshTimer.Tick += HealthRefreshTimer_Tick;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadBlockList();
            SetupBlockListWatcher();
            LoadInventory();
            CheckServiceStatus();
            RefreshProcesses(); // This method should exist now
            LoadFtpConfig();
            _refreshTimer.Start();

            // Make status labels clickable
            lblStartService.Click += lblStartService_Click;
            lblStopService.Click += lblStopService_Click;
            lblOpenLogs.Click += lblOpenLogs_Click;
            //SetupHealthTab();
            LoadHealthData();
            healthRefreshTimer.Start();
        }

        #region Timer & Background Operations
        // Add this helper method at the top of the MainForm class
        private IEnumerable<string> ReadLastLines(string filePath, int count)
        {
            if (!File.Exists(filePath))
                yield break;

            var buffer = new List<string>();
            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    buffer.Add(line);
                    if (buffer.Count > count)
                        buffer.RemoveAt(0);
                }
            }

            foreach (var line in buffer)
                yield return line;
        }
        // Add FTP Configuration Methods
        private void LoadFtpConfig()
        {
            try
            {
                if (File.Exists(_ftpConfigFile))
                {
                    var json = File.ReadAllText(_ftpConfigFile);
                    _ftpConfig = JsonConvert.DeserializeObject<FtpConfig>(json) ?? new FtpConfig();
                }

                // Update UI with loaded config
                UpdateFtpConfigUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading FTP config: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _ftpConfig = new FtpConfig();
            }
        }

        private void UpdateFtpConfigUI()
        {
            if (txtFtpServer != null) txtFtpServer.Text = _ftpConfig.Server;
            if (txtFtpUser != null) txtFtpUser.Text = _ftpConfig.Username;
            if (txtFtpPass != null) txtFtpPass.Text = _ftpConfig.Password;
            if (chkFtpEnabled != null) chkFtpEnabled.Checked = _ftpConfig.Enabled;
        }

        private void SaveFtpConfig()
        {
            try
            {
                // Update config from UI
                _ftpConfig.Server = txtFtpServer.Text;
                _ftpConfig.Username = txtFtpUser.Text;
                _ftpConfig.Password = txtFtpPass.Text;
                _ftpConfig.Enabled = chkFtpEnabled.Checked;

                var json = JsonConvert.SerializeObject(_ftpConfig, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_ftpConfigFile, json);

                MessageBox.Show("FTP settings saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving FTP config: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event Handlers for FTP Tab
        private async void btnTestFtp_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnTestFtp.Enabled = false;
                btnTestFtp.Text = "Testing...";

                string server = txtFtpServer.Text;
                string username = txtFtpUser.Text;
                string password = txtFtpPass.Text;

                if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please fill in all FTP server details.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Test connection
                if (await TestFtpConnectionAsync())
                {
                    // Try to create and list directories
                    await CreateRemoteDirectoryStructureAsync();

                    MessageBox.Show($" FTP Connection Successful!\n\nServer: {server}\nUsername: {username}\n\nDirectories created successfully.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    UpdateFtpStatus(true, "Connected");
                }
                else
                {
                    MessageBox.Show(" FTP Connection Failed!\n\nPlease check:\n1. Server address\n2. Username/Password\n3. Internet connection\n4. Firewall settings",
                        "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    UpdateFtpStatus(false, "Failed");
                }
            }
            catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
            {
                string errorMessage = $"FTP Error {ftpResponse.StatusCode}:\n{ftpResponse.StatusDescription}";

                // Provide helpful suggestions based on error code
                switch (ftpResponse.StatusCode)
                {
                    case FtpStatusCode.NotLoggedIn:
                        errorMessage += "\n\nSuggestion: Check username and password.";
                        break;
                    case FtpStatusCode.ActionNotTakenFileUnavailable:
                        errorMessage += "\n\nSuggestion: Directory doesn't exist. This is normal for first connection.";
                        break;
                    case FtpStatusCode.ServiceNotAvailable:
                        errorMessage += "\n\nSuggestion: FTP service might be disabled on the server.";
                        break;
                }

                MessageBox.Show(errorMessage, "FTP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateFtpStatus(false, "Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateFtpStatus(false, "Error");
            }
            finally
            {
                Cursor = Cursors.Default;
                btnTestFtp.Enabled = true;
                btnTestFtp.Text = "Test Connection";
            }
        }
        private async void btnFtpDiagnostic_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnFtpDiagnostic.Enabled = false;

                string server = txtFtpServer.Text;
                string username = txtFtpUser.Text;
                string password = txtFtpPass.Text;

                StringBuilder diagnostic = new StringBuilder();
                diagnostic.AppendLine("=== FTP Diagnostic Report ===");
                diagnostic.AppendLine($"Time: {DateTime.Now}");
                diagnostic.AppendLine($"Server: {server}");
                diagnostic.AppendLine($"Username: {username}");
                diagnostic.AppendLine($"Password: {(string.IsNullOrEmpty(password) ? "Empty" : "Provided")}");

                dgvFtpLog.Rows.Clear();

                // Test 1: Basic connection
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 1", "Testing basic connection...");
                diagnostic.AppendLine("\n1. Basic Connection Test:");

                try
                {
                    FtpWebRequest testRequest = (FtpWebRequest)WebRequest.Create($"ftp://{server}/");
                    testRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                    testRequest.Credentials = new NetworkCredential(username, password);
                    testRequest.UsePassive = true;
                    testRequest.Timeout = 5000;

                    using (FtpWebResponse response = (FtpWebResponse)testRequest.GetResponse())
                    {
                        diagnostic.AppendLine($"    Success: {response.StatusDescription}");
                        AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 1", "✓ Connected");
                    }
                }
                catch (Exception ex)
                {
                    diagnostic.AppendLine($"    Failed: {ex.Message}");
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 1", $"✗ {ex.Message}");
                }

                // Test 2: Directory creation
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 2", "Testing directory creation...");
                diagnostic.AppendLine("\n2. Directory Creation Test:");

                try
                {
                    string testDir = $"/test_{Guid.NewGuid().ToString().Substring(0, 8)}";
                    await CreateFtpDirectoryAsync(server, username, password, testDir);
                    diagnostic.AppendLine($"    Success: Created {testDir}");
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 2", $"✓ Created {testDir}");

                    // Clean up
                    await DeleteFtpDirectoryAsync(server, username, password, testDir);
                }
                catch (Exception ex)
                {
                    diagnostic.AppendLine($"    Failed: {ex.Message}");
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 2", $"✗ {ex.Message}");
                }

                // Test 3: File upload
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 3", "Testing file upload...");
                diagnostic.AppendLine("\n3. File Upload Test:");

                try
                {
                    // Create a test file
                    string testFile = Path.Combine(Path.GetTempPath(), $"ftp_test_{Guid.NewGuid().ToString().Substring(0, 8)}.txt");
                    File.WriteAllText(testFile, $"FTP test from {Environment.MachineName} at {DateTime.Now}");

                    string remoteTestFile = $"/test_upload_{Path.GetFileName(testFile)}";

                    FtpWebRequest uploadRequest = (FtpWebRequest)WebRequest.Create($"ftp://{server}{remoteTestFile}");
                    uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    uploadRequest.Credentials = new NetworkCredential(username, password);
                    uploadRequest.UsePassive = true;

                    byte[] fileBytes = File.ReadAllBytes(testFile);
                    using (Stream requestStream = uploadRequest.GetRequestStream())
                    {
                        requestStream.Write(fileBytes, 0, fileBytes.Length);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)uploadRequest.GetResponse())
                    {
                        diagnostic.AppendLine($"    Success: Uploaded to {remoteTestFile}");
                        AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 3", $"✓ Upload successful");
                    }

                    // Clean up
                    File.Delete(testFile);
                }
                catch (Exception ex)
                {
                    diagnostic.AppendLine($"    Failed: {ex.Message}");
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Test 3", $"✗ {ex.Message}");
                }

                // Show results
                MessageBox.Show(diagnostic.ToString(), "FTP Diagnostic Results",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Diagnostic", "Complete", "Diagnostic finished");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Diagnostic failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnFtpDiagnostic.Enabled = true;
            }
        }

        private async Task DeleteFtpDirectoryAsync(string server, string username, string password, string directoryPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{server}{directoryPath}");
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;
                request.Credentials = new NetworkCredential(username, password);

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    response.Close();
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        private void TestFtpConnection(string server, string username, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{server}/");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.Timeout = 10000; // 10 seconds timeout

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    MessageBox.Show($"✅ FTP Connection Successful!\n\nServer: {server}\nStatus: {response.StatusDescription}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Update FTP status in UI
                    UpdateFtpStatus(true, "Connected");
                }
            }
            catch (WebException ex)
            {
                if (ex.Response is FtpWebResponse ftpResponse)
                {
                    throw new Exception($"FTP Error {ftpResponse.StatusCode}: {ftpResponse.StatusDescription}");
                }
                throw new Exception($"Network Error: {ex.Message}");
            }
        }

        private void UpdateFtpStatus(bool isConnected, string message)
        {
            if (lblFtpStatus != null)
            {
                lblFtpStatus.Text = $"FTP: {message}";
                lblFtpStatus.ForeColor = isConnected ? Color.Green : Color.Red;
            }
        }

        private void btnSaveFtp_Click(object sender, EventArgs e)
        {
            SaveFtpConfig();
        }

        private async void btnUploadNow_Click(object sender, EventArgs e)
        {
            try
            {
                // Disable button during upload
                btnUploadNow.Enabled = false;
                btnUploadNow.Text = "Uploading...";
                Cursor = Cursors.WaitCursor;

                // Clear log for new session
                dgvFtpLog.Rows.Clear();

                // Validate inputs
                if (string.IsNullOrEmpty(txtFtpServer.Text) ||
                    string.IsNullOrEmpty(txtFtpUser.Text) ||
                    string.IsNullOrEmpty(txtFtpPass.Text))
                {
                    MessageBox.Show("Please fill in all FTP server details.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Starting",
                    "Manual FTP upload initiated");

                // Perform upload
                await DirectFtpUploadAsync();

                // Success message
                MessageBox.Show("FTP upload completed successfully. Check the log below for details.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Upload failed: {ex.Message}\n\nCheck the FTP log for details.",
                    "FTP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Restore UI
                btnUploadNow.Enabled = true;
                btnUploadNow.Text = "Upload Now";
                Cursor = Cursors.Default;
            }
        }

        private async Task TriggerManualFtpUploadAsync()
        {
            // Create a trigger file for the service
            string triggerFile = Path.Combine(_logsPath, "ftp_upload_trigger.txt");
            File.WriteAllText(triggerFile, DateTime.UtcNow.ToString("o"));

            // Also try direct upload if service is not running
            await DirectFtpUploadAsync();
        }

        private async Task DirectFtpUploadAsync()
        {
            try
            {
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Starting",
                    "Manual FTP upload initiated");

                // Show current machine name
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Info",
                    $"Machine: {Environment.MachineName}");

                // Test connection first
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Testing",
                    "Testing FTP connection...");

                if (!await TestFtpConnectionAsync())
                {
                    throw new Exception("FTP connection test failed");
                }

                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Connected",
                    "FTP connection successful");

                // Get log files
                var logFiles = GetLogFilesForUpload();

                if (logFiles.Count == 0)
                {
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Warning",
                        "No log files found to upload");
                    return;
                }

                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Info",
                    $"Found {logFiles.Count} file(s) to upload");

                // Upload files
                int successCount = 0;

                foreach (var filePath in logFiles)
                {
                    string fileName = Path.GetFileName(filePath);

                    try
                    {
                        bool uploaded;

                        // Special handling for inventory file
                        if (fileName.IndexOf("inventory", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            uploaded = await UploadInventoryFileAsync(filePath);
                        }
                        else
                        {
                            uploaded = await UploadSingleFileAsync(filePath);
                        }

                        if (uploaded)
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), fileName, "Failed", ex.Message);
                    }
                }

                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Complete",
                    $"Upload finished: {successCount}/{logFiles.Count} files uploaded");
            }
            catch (Exception ex)
            {
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "System", "Error", ex.Message);
                throw;
            }
        }
        // Add a new button to your FTP tab in designer:
        // Name: btnQuickTest, Text: "Quick Test", Location: 580, 115, Size: 100, 25

        private void btnQuickTest_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // Create a simple test file
                string testFile = Path.GetTempFileName();
                File.WriteAllText(testFile, $"Test from {Environment.MachineName} at {DateTime.Now}");

                string server = txtFtpServer.Text;
                string username = txtFtpUser.Text;
                string password = txtFtpPass.Text;

                // Try uploading to root with a simple name
                string remoteFile = $"/test_{DateTime.Now:HHmmss}.txt";

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{server}{remoteFile}");
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.UseBinary = true;

                byte[] fileBytes = File.ReadAllBytes(testFile);

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    MessageBox.Show($"✅ Success!\nUploaded to: {remoteFile}\nSize: {fileBytes.Length} bytes",
                        "Quick Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                File.Delete(testFile);
            }
            catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
            {
                MessageBox.Show($"❌ FTP Error {ftpResponse.StatusCode}:\n{ftpResponse.StatusDescription}",
                    "Quick Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error: {ex.Message}", "Quick Test Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private async Task<bool> TestFtpConnectionAsync()
        {
            try
            {
                string server = txtFtpServer.Text;
                string username = txtFtpUser.Text;
                string password = txtFtpPass.Text;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{server}/");
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.Timeout = 10000;

                using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                {
                    return response.StatusCode == FtpStatusCode.OpeningData ||
                           response.StatusCode == FtpStatusCode.DataAlreadyOpen;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task CreateRemoteDirectoryStructureAsync()
        {
            string server = txtFtpServer.Text;
            string username = txtFtpUser.Text;
            string password = txtFtpPass.Text;

            // Clean machine name for path
            string machineName = CleanFileName(Environment.MachineName);

            AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Directory", "Info",
                $"Machine name: '{Environment.MachineName}' -> Cleaned: '{machineName}'");

            string remoteBasePath = $"/logs/{machineName}";

            AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Directory", "Creating",
                $"Creating directory structure: {remoteBasePath}");

            try
            {
                // Create logs directory if it doesn't exist
                bool logsCreated = await CreateFtpDirectoryAsync(server, username, password, "/logs");

                if (logsCreated)
                {
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "/logs", "Success", "Directory created/exists");
                }

                // Create machine-specific directory
                bool machineDirCreated = await CreateFtpDirectoryAsync(server, username, password, remoteBasePath);

                if (machineDirCreated)
                {
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), remoteBasePath, "Success", "Machine directory created");
                }
                else
                {
                    throw new Exception($"Failed to create machine directory: {remoteBasePath}");
                }
            }
            catch (Exception ex)
            {
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Directory", "Error", ex.Message);
                throw;
            }
        }

        private async Task<bool> CreateFtpDirectoryAsync(string server, string username, string password, string directoryPath)
        {
            try
            {
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Directory Check", "Testing",
                    $"Checking directory: {directoryPath}");

                // Check if directory exists
                FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create($"ftp://{server}{directoryPath}");
                listRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                listRequest.Credentials = new NetworkCredential(username, password);
                listRequest.UsePassive = true;
                listRequest.Timeout = 5000;

                try
                {
                    using (FtpWebResponse listResponse = (FtpWebResponse)await listRequest.GetResponseAsync())
                    {
                        // Directory exists
                        AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), directoryPath, "Exists",
                            "Directory already exists");
                        listResponse.Close();
                        return true;
                    }
                }
                catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse &&
                                              ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // Directory doesn't exist, create it
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), directoryPath, "Creating",                            
                        "Directory doesn't exist, creating...");

                    FtpWebRequest createRequest = (FtpWebRequest)WebRequest.Create($"ftp://{server}{directoryPath}");
                    createRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                    createRequest.Credentials = new NetworkCredential(username, password);
                    createRequest.UsePassive = true;
                    createRequest.Timeout = 5000;

                    using (FtpWebResponse createResponse = (FtpWebResponse)await createRequest.GetResponseAsync())
                    {
                        AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), directoryPath, "Created",
                            $"Success: {createResponse.StatusDescription}");
                        createResponse.Close();
                        return true;
                    }
                }
                catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
                {
                    string errorMessage = $"FTP Error {ftpResponse.StatusCode}: {ftpResponse.StatusDescription}";
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), directoryPath, "FTP Error", errorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), directoryPath, "Error",
                    $"Unexpected error: {ex.Message}");
                return false;
            }
        }

        private List<string> GetLogFilesForUpload()
        {
            var files = new List<string>();

            try
            {
                // Get today's log files
                string today = DateTime.Now.ToString("yyyyMMdd");
                string pattern = $"*_{today}.jsonl";

                var todayFiles = Directory.GetFiles(_logsPath, pattern);
                files.AddRange(todayFiles);

                // Also include inventory file if it exists
                string inventoryFile = Path.Combine(_logsPath, "installed_apps_inventory.json");
                if (File.Exists(inventoryFile))
                {
                    files.Add(inventoryFile);
                }

                // Limit to 5 files for manual upload
                files = files.Take(5).ToList();
            }
            catch (Exception ex)
            {
                AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "File List", "Error", ex.Message);
            }

            return files;
        }

        private async Task<bool> UploadSingleFileAsync(string localFilePath)
        {
            string server = txtFtpServer.Text;
            string username = txtFtpUser.Text;
            string password = txtFtpPass.Text;

            string fileName = CleanFileName(Path.GetFileName(localFilePath));
            string machineName = CleanFileName(Environment.MachineName);

            // Try multiple path options if the first one fails
            string[] remotePathOptions = new[]
            {
        // Option 1: Standard path
        $"/logs/{machineName}/{fileName}",
        
        // Option 2: Root directory (if /logs doesn't work)
        $"/{fileName}",
        
        // Option 3: Simple machine name (remove special chars)
        $"/{machineName.Replace("_", "").Replace("-", "")}/{fileName}",
        
        // Option 4: Generic name
        $"/logs/computer_{Environment.UserName}/{fileName}"
    };

            foreach (string remotePath in remotePathOptions)
            {
                try
                {
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Upload", "Attempting",
                        $"Trying path: {remotePath}");

                    // Create FTP request
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{server}{remotePath}");
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(username, password);
                    request.UsePassive = true;
                    request.UseBinary = true;
                    request.KeepAlive = false;
                    request.Timeout = 10000;

                    // Read file
                    byte[] fileContents;
                    using (FileStream stream = File.OpenRead(localFilePath))
                    {
                        fileContents = new byte[stream.Length];
                        await stream.ReadAsync(fileContents, 0, (int)stream.Length);
                    }

                    // Upload
                    using (Stream requestStream = await request.GetRequestStreamAsync())
                    {
                        await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
                    }

                    // Get response
                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Upload", "Success",
                            $"✓ Uploaded to: {remotePath}");
                        return true;
                    }
                }
                catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
                {
                    string errorMessage = $"Path '{remotePath}': FTP {ftpResponse.StatusCode}";
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Upload", "Failed", errorMessage);

                    // Continue to next option
                    continue;
                }
                catch (Exception ex)
                {
                    AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Upload", "Error",
                        $"Path '{remotePath}': {ex.Message}");
                    continue;
                }
            }

            // All options failed
            throw new Exception("All upload path options failed");
        }

        private string CleanFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "unknown";

            // List of characters that cause FTP 553 errors
            char[] invalidFtpChars = new char[]
            {
        ':', ';', '|', '=', '+', ',', '?', '*',
        '<', '>', '"', '[', ']', '\\', '/', ' '
            };

            // Replace invalid characters with underscore
            foreach (char c in invalidFtpChars)
            {
                fileName = fileName.Replace(c.ToString(), "_");
            }

            // Also remove any standard invalid path characters
            var invalidPathChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidPathChars)
            {
                fileName = fileName.Replace(c.ToString(), "_");
            }

            // Remove trailing dots or spaces (Windows restrictions)
            fileName = fileName.Trim('.', ' ');

            // Ensure it's not empty after cleaning
            if (string.IsNullOrEmpty(fileName))
                fileName = "unknown";

            // Ensure it's not too long
            if (fileName.Length > 50)
            {
                fileName = fileName.Substring(0, 47) + "...";
            }

            return fileName;
        }
        private async Task<bool> UploadInventoryFileAsync(string localFilePath)
        {
            string server = txtFtpServer.Text;
            string username = txtFtpUser.Text;
            string password = txtFtpPass.Text;

            // Rename inventory file to avoid underscore issues
            string cleanFileName = "inventory.json";

            // Try multiple simple paths
            string[] remotePathOptions = new[]
            {
        $"/logs/inventory_{DateTime.Now:yyyyMMdd}.json",
        $"/inventory.json",
        $"/data.json",
        $"/logs/data.json"
    };

            foreach (string remotePath in remotePathOptions)
            {
                try
                {
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{server}{remotePath}");
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.Credentials = new NetworkCredential(username, password);
                    request.UsePassive = true;
                    request.UseBinary = true;

                    byte[] fileContents = File.ReadAllBytes(localFilePath);

                    using (Stream requestStream = await request.GetRequestStreamAsync())
                    {
                        await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
                    {
                        AddFtpLogEntry(DateTime.Now.ToString("HH:mm:ss"), "Inventory", "Success",
                            $"Uploaded to: {remotePath}");
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }
        private async Task UploadFileToFtpAsync(string localFilePath)
        {
            string fileName = Path.GetFileName(localFilePath);
            string remotePath = $"/logs/{Environment.MachineName}/{fileName}";

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{_ftpConfig.Server}{remotePath}");
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(_ftpConfig.Username, _ftpConfig.Password);
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            byte[] fileContents = File.ReadAllBytes(localFilePath);

            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(fileContents, 0, fileContents.Length);
            }

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            {
                // Success
            }
        }

        private void AddFtpLogEntry(string timestamp, string file, string status, string message)
        {
            if (dgvFtpLog != null)
            {
                int rowIndex = dgvFtpLog.Rows.Add(
                    timestamp,
                    file,
                    status,
                    message
                );

                // Color code based on status
                if (status == "Success" || status.Contains("Connected"))
                {
                    dgvFtpLog.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else if (status == "Failed" || status.Contains("Error"))
                {
                    dgvFtpLog.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                }
                else
                {
                    dgvFtpLog.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                }

                // Auto-scroll to latest entry
                dgvFtpLog.FirstDisplayedScrollingRowIndex = rowIndex;
            }
        }

        private void btnViewFtpLog_Click(object sender, EventArgs e)
        {
            try
            {
                string ftpLogFile = Path.Combine(_logsPath, "ftp_upload_log.jsonl");
                if (File.Exists(ftpLogFile))
                {
                    Process.Start("notepad.exe", ftpLogFile);
                }
                else
                {
                    MessageBox.Show("No FTP log file found yet. Logs will appear after uploads.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            if (dgvFtpLog != null)
            {
                dgvFtpLog.Rows.Clear();
            }
        }

        private void lblFtpControl_Click(object sender, EventArgs e)
        {
            btnUploadNow_Click(sender, e);
        }
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // Refresh processes in background
            Task.Run(() => RefreshProcessesSafe());
        }

        // ADD THIS MISSING METHOD
        private void RefreshProcesses()
        {
            // Call the safe method
            Task.Run(() => RefreshProcessesSafe());
        }
        // Add this method
        private void SetupBlockListWatcher()
        {
            try
            {
                _blockListWatcher = new FileSystemWatcher(_configPath)
                {
                    Filter = "blocked_apps.json",
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                };

                _blockListWatcher.Changed += (sender, e) =>
                {
                    // Debounce - wait a moment for file write to complete
                    System.Threading.Thread.Sleep(100);

                    // Reload block list on UI thread
                    this.Invoke((MethodInvoker)delegate
                    {
                        LoadBlockList();
                    });
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to setup file watcher: {ex.Message}");
            }
        }
        private void RefreshProcessesSafe()
        {
            try
            {
                lock (_processLock)
                {
                    // Clear previous processes
                    foreach (var process in _currentProcesses)
                    {
                        try { process.Dispose(); } catch { }
                    }
                    _currentProcesses.Clear();

                    // Get current processes
                    var processes = Process.GetProcesses();
                    _currentProcesses.AddRange(processes);

                    // Update UI on main thread
                    this.Invoke((MethodInvoker)delegate
                    {
                        UpdateProcessGrid(processes);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing processes: {ex.Message}");
            }
        }
        private void HealthRefreshTimer_Tick(object sender, EventArgs e)
        {
            LoadHealthData();
        }
        private void LoadHealthData()
        {
            try
            {
                string healthLogFile = CommonPaths.GetCurrentHealthLogFileName();

                if (File.Exists(healthLogFile))
                {
                    // Read last 100 lines from health log using our helper method
                    var lines = ReadLastLines(healthLogFile, 100).ToList();
                    healthHistory.Clear();

                    foreach (var line in lines)
                    {
                        try
                        {
                            var snapshot = JsonConvert.DeserializeObject<DeviceHealthSnapshot>(line);
                            if (snapshot != null)
                            {
                                healthHistory.Add(snapshot);
                            }
                        }
                        catch
                        {
                            // Skip invalid lines
                        }
                    }

                    // Update UI with latest snapshot
                    if (healthHistory.Count > 0)
                    {
                        // Pick most recent snapshot that actually has data
                        var latest = healthHistory
                            .Where(h => (h.TotalRamMb > 0 || h.RamUsagePercent > 0 || h.Uptime > TimeSpan.Zero))
                            .OrderBy(h => h.TimestampUtc)
                            .LastOrDefault();

                        if (latest != null)
                        {
                            UpdateHealthUI(latest);
                            UpdateHealthHistoryList();
                        }
                    }

                }
                else
                {
                    // No health data yet
                    if (lblCpuUsage != null) lblCpuUsage.Text = "No data";
                    if (pbCpu != null) pbCpu.Value = 0;
                    if (lblRamUsage != null) lblRamUsage.Text = "No data";
                    if (pbRam != null) pbRam.Value = 0;
                    if (lblUptime != null) lblUptime.Text = "No data";
                    if (lblBatteryStatus != null) lblBatteryStatus.Text = "No data";
                    if (lblTemperature != null) lblTemperature.Text = "No data";
                    if (dgvHealthDisks != null) dgvHealthDisks.Rows.Clear();
                    if (lbHealthHistory != null) lbHealthHistory.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading health data: {ex.Message}");
            }
        }

        private void UpdateHealthUI(DeviceHealthSnapshot snapshot)
        {
            if (snapshot == null)
            {
                // Handle null snapshot
                if (lblCpuUsage != null) lblCpuUsage.Text = "No data";
                if (pbCpu != null) pbCpu.Value = 0;
                if (lblRamUsage != null) lblRamUsage.Text = "No data";
                if (pbRam != null) pbRam.Value = 0;
                if (lblUptime != null) lblUptime.Text = "No data";
                if (lblBatteryStatus != null) lblBatteryStatus.Text = "No data";
                if (lblTemperature != null) lblTemperature.Text = "No data";
                if (dgvHealthDisks != null) dgvHealthDisks.Rows.Clear();
                return;
            }

            try
            {
                // Update CPU
                if (pbCpu != null)
                {
                    pbCpu.Value = (int)Math.Min(100, Math.Max(0, snapshot.CpuUsagePercent));
                }
                if (lblCpuUsage != null)
                {
                    lblCpuUsage.Text = $"{snapshot.CpuUsagePercent:F1}%";
                }

                // Update RAM
                if (pbRam != null)
                {
                    pbRam.Value = (int)Math.Min(100, Math.Max(0, snapshot.RamUsagePercent));
                }
                if (lblRamUsage != null)
                {
                    if (snapshot.TotalRamMb > 0)
                    {
                        double totalGB = snapshot.TotalRamMb / 1024.0;
                        double usedGB = snapshot.UsedRamMb / 1024.0;
                        lblRamUsage.Text = $"{usedGB:F1} GB / {totalGB:F1} GB ({snapshot.RamUsagePercent:F1}%)";
                    }
                    else
                    {
                        lblRamUsage.Text = "N/A";
                    }
                }

                // Update Uptime
                if (lblUptime != null)
                {
                    if (snapshot.Uptime.TotalSeconds > 0)
                    {
                        if (snapshot.Uptime.Days > 0)
                        {
                            lblUptime.Text = $"{snapshot.Uptime.Days}d {snapshot.Uptime.Hours:00}:{snapshot.Uptime.Minutes:00}:{snapshot.Uptime.Seconds:00}";
                        }
                        else
                        {
                            lblUptime.Text = $"{snapshot.Uptime.Hours:00}:{snapshot.Uptime.Minutes:00}:{snapshot.Uptime.Seconds:00}";
                        }
                    }
                    else
                    {
                        lblUptime.Text = "Unknown";
                    }
                }

                // Update Battery
                // In UpdateHealthUI method, update the battery section:
                if (lblBatteryStatus != null)
                {
                    if (snapshot.Battery != null)
                    {
                        if (snapshot.Battery.IsPresent)
                        {
                            string chargeText = snapshot.Battery.ChargePercent.HasValue ?
                                $"{snapshot.Battery.ChargePercent:F1}%" : "N/A";
                            lblBatteryStatus.Text = $"{chargeText} - {snapshot.Battery.Status}";

                            // Color coding
                            if (snapshot.Battery.Status.Contains("Charg") ||
                                snapshot.Battery.Status == "Full" ||
                                snapshot.Battery.Status == "OnAC")
                            {
                                lblBatteryStatus.ForeColor = Color.Green;
                            }
                            else if (snapshot.Battery.Status.Contains("Low") ||
                                     snapshot.Battery.Status.Contains("Critical"))
                            {
                                lblBatteryStatus.ForeColor = Color.Red;
                            }
                            else if (snapshot.Battery.Status == "Discharging")
                            {
                                lblBatteryStatus.ForeColor = Color.Orange;
                            }
                            else
                            {
                                lblBatteryStatus.ForeColor = Color.Black;
                            }
                        }
                        else
                        {
                            // Desktop PC
                            // Check if we have power status from WMI
                            if (snapshot.Battery.Status == "PluggedIn" ||
                                snapshot.Battery.Status == "OnAC")
                            {
                                lblBatteryStatus.Text = "Desktop PC (Plugged In)";
                                lblBatteryStatus.ForeColor = Color.Green;
                            }
                            else
                            {
                                lblBatteryStatus.Text = "Desktop PC (No Battery)";
                                lblBatteryStatus.ForeColor = Color.Blue;
                            }
                        }
                    }
                    else
                    {
                        lblBatteryStatus.Text = "Unknown";
                        lblBatteryStatus.ForeColor = Color.Gray;
                    }
                }

                // Update Temperature
                if (lblTemperature != null)
                {
                    if (snapshot.Temperature != null && snapshot.Temperature.IsSupported && snapshot.Temperature.Celsius.HasValue)
                    {
                        lblTemperature.Text = $"{snapshot.Temperature.Celsius:F1}°C";

                        // Color coding for temperature
                        if (snapshot.Temperature.Celsius > 80)
                            lblTemperature.ForeColor = Color.Red;
                        else if (snapshot.Temperature.Celsius > 60)
                            lblTemperature.ForeColor = Color.Orange;
                        else
                            lblTemperature.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblTemperature.Text = "Not Available";
                        lblTemperature.ForeColor = Color.Gray;
                    }
                }

                // Update Disk Grid
                if (dgvHealthDisks != null)
                {
                    dgvHealthDisks.Rows.Clear();
                    if (snapshot.Disks != null && snapshot.Disks.Count > 0)
                    {
                        foreach (var disk in snapshot.Disks.OrderBy(d => d.DriveLetter))
                        {
                            int rowIndex = dgvHealthDisks.Rows.Add(
                                disk.DriveLetter,
                                $"{disk.UsedSpaceGb:N1}",
                                $"{disk.FreeSpaceGb:N1}",
                                $"{disk.TotalSpaceGb:N1}",
                                $"{disk.UsagePercent:F1}%"
                            );

                            // Color code based on usage
                            if (disk.UsagePercent > 90)
                            {
                                dgvHealthDisks.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                                dgvHealthDisks.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                            }
                            else if (disk.UsagePercent > 75)
                            {
                                dgvHealthDisks.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                                dgvHealthDisks.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
                            }
                            else
                            {
                                dgvHealthDisks.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                                dgvHealthDisks.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkGreen;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating health UI: {ex.Message}");
            }
        }
        // Add this as a test button in MainForm
        private void btnTestHealth_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("Testing health data collection...");

                // Test CPU
                using (var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
                {
                    cpuCounter.NextValue();
                    System.Threading.Thread.Sleep(1000);
                    float cpu = cpuCounter.NextValue();
                    MessageBox.Show($"CPU Test: {cpu}%");
                }

                // Test RAM
                using (var ramCounter = new PerformanceCounter("Memory", "Available MBytes"))
                {
                    float ram = ramCounter.NextValue();
                    MessageBox.Show($"Available RAM: {ram}MB");
                }

                // Test Uptime
                long tickCount = Environment.TickCount;
                TimeSpan uptime = TimeSpan.FromMilliseconds(tickCount < 0 ? uint.MaxValue + tickCount : tickCount);
                MessageBox.Show($"Uptime: {uptime.Days}d {uptime.Hours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}");

                // Test Battery
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    var batteries = searcher.Get();
                    MessageBox.Show($"Batteries found: {batteries.Count}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Test failed: {ex.Message}");
            }
        }

        // Update UpdateHealthHistoryList method
        private void UpdateHealthHistoryList()
        {
            if (lbHealthHistory == null) return;

            lbHealthHistory.Items.Clear();

            // Get last 50 entries (or fewer if not enough)
            int startIndex = Math.Max(0, healthHistory.Count - 50);
            for (int i = healthHistory.Count - 1; i >= startIndex; i--)
            {
                var snapshot = healthHistory[i];
                string time = snapshot.TimestampUtc.ToLocalTime().ToString("HH:mm:ss");
                string entry = $"[{time}] CPU: {snapshot.CpuUsagePercent:F1}%, RAM: {snapshot.RamUsagePercent:F1}%, Uptime: {snapshot.Uptime:hh\\:mm\\:ss}";
                lbHealthHistory.Items.Add(entry);
            }

        }
        private void UpdateProcessGrid(Process[] processes)
        {
            if (dgvProcesses.InvokeRequired)
            {
                dgvProcesses.Invoke(new Action<Process[]>(UpdateProcessGrid), processes);
                return;
            }

            try
            {
                dgvProcesses.SuspendLayout();
                dgvProcesses.Rows.Clear();

                foreach (var process in processes.OrderBy(p => p.ProcessName))
                {
                    string exePath = "N/A";
                    try
                    {
                        exePath = process.MainModule?.FileName ?? "N/A";
                    }
                    catch
                    {
                        // Access denied
                    }

                    bool isBlocked = _blockList.BlockedProcesses.Any(b =>
                        (!string.IsNullOrEmpty(b.ProcessName) &&
                         process.ProcessName.Equals(b.ProcessName, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(b.ExePath) &&
                         exePath != "N/A" &&
                         exePath.Equals(b.ExePath, StringComparison.OrdinalIgnoreCase)));

                    int rowIndex = dgvProcesses.Rows.Add(
                        process.ProcessName,
                        process.Id,
                        exePath,
                        isBlocked ? "BLOCKED" : ""
                    );

                    if (isBlocked)
                    {
                        dgvProcesses.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                        dgvProcesses.Rows[rowIndex].DefaultCellStyle.Font =
                            new Font(dgvProcesses.DefaultCellStyle.Font, FontStyle.Bold);
                    }
                }

                dgvProcesses.ResumeLayout();
                lblProcessCount.Text = $"Processes: {processes.Length}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating process grid: {ex.Message}");
            }
        }

        #endregion

        #region Data Loading Methods

        private void LoadBlockList()
        {
            try
            {
                if (File.Exists(_blockListFile))
                {
                    var json = File.ReadAllText(_blockListFile);
                    _blockList = JsonConvert.DeserializeObject<BlockList>(json) ?? new BlockList();

                    // Remove any duplicate entries
                    _blockList.BlockedProcesses = _blockList.BlockedProcesses
                        .GroupBy(b => new {
                            ProcessName = b.ProcessName?.ToLowerInvariant() ?? "",
                            ExePath = b.ExePath?.ToLowerInvariant() ?? ""
                        })
                        .Select(g => g.First())
                        .ToList();
                }
                else
                {
                    _blockList = new BlockList();
                    SaveBlockList(); // Create empty file
                }

                UpdateBlockListStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading block list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _blockList = new BlockList();
            }
        }

        private void SaveBlockList()
        {
            try
            {
                // Sort the list for better readability
                _blockList.BlockedProcesses = _blockList.BlockedProcesses
                    .OrderBy(b => b.ProcessName ?? "")
                    .ThenBy(b => b.ExePath ?? "")
                    .ToList();

                var json = JsonConvert.SerializeObject(_blockList, Newtonsoft.Json.Formatting.Indented);

                // Use atomic write to avoid corruption
                string tempFile = _blockListFile + ".tmp";
                File.WriteAllText(tempFile, json);

                // Replace original file
                if (File.Exists(_blockListFile))
                    File.Delete(_blockListFile);

                File.Move(tempFile, _blockListFile);

                // Update UI
                UpdateBlockListStatus();

                // Notify service of change
                try
                {
                    File.SetLastWriteTimeUtc(_blockListFile, DateTime.UtcNow);
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving block list: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadInventory()
        {
            try
            {
                if (File.Exists(_inventoryFile))
                {
                    var json = File.ReadAllText(_inventoryFile);
                    _inventory = JsonConvert.DeserializeObject<Inventory>(json) ?? new Inventory();
                    DisplayInventory();
                }
            }
            catch (Exception ex)
            {
                // Inventory might not exist yet
                Debug.WriteLine($"Inventory load error: {ex.Message}");
            }
        }

        private void CheckServiceStatus()
        {
            try
            {
                _serviceController = new ServiceController("AppBlockerService");
                UpdateServiceStatusUI();
            }
            catch (InvalidOperationException)
            {
                lblServiceStatus.Text = "Service Status: Not Installed";
                UpdateServiceButtons(false);
            }
        }

        private void UpdateServiceStatusUI()
        {
            if (_serviceController != null)
            {
                try
                {
                    _serviceController.Refresh();
                    lblServiceStatus.Text = $"Service Status: {_serviceController.Status}";

                    UpdateServiceButtons(_serviceController.Status == ServiceControllerStatus.Running);

                    // Color coding
                    if (_serviceController.Status == ServiceControllerStatus.Running)
                    {
                        lblServiceStatus.ForeColor = Color.DarkGreen;
                    }
                    else if (_serviceController.Status == ServiceControllerStatus.Stopped)
                    {
                        lblServiceStatus.ForeColor = Color.DarkRed;
                    }
                    else
                    {
                        lblServiceStatus.ForeColor = Color.DarkOrange;
                    }
                }
                catch
                {
                    lblServiceStatus.Text = "Service Status: Error";
                    UpdateServiceButtons(false);
                }
            }
        }

        private void UpdateServiceButtons(bool isRunning)
        {
            lblStartService.Enabled = !isRunning;
            lblStopService.Enabled = isRunning;

            lblStartService.ForeColor = lblStartService.Enabled ? Color.Black : Color.Gray;
            lblStopService.ForeColor = lblStopService.Enabled ? Color.Black : Color.Gray;
        }

        private void DisplayInventory()
        {
            try
            {
                dgvInventory.SuspendLayout();
                dgvInventory.Rows.Clear();

                if (_inventory?.Apps == null) return;

                // Filter out system apps by default
                var filteredApps = _inventory.Apps
                    .Where(a => !a.IsSystem)
                    .OrderBy(a => a.DisplayName)
                    .ToList();

                foreach (var app in filteredApps)
                {
                    bool isBlocked = _blockList.BlockedProcesses.Any(b =>
                        (!string.IsNullOrEmpty(b.ExePath) &&
                         app.ExePath.Equals(b.ExePath, StringComparison.OrdinalIgnoreCase)));

                    int rowIndex = dgvInventory.Rows.Add(
                        app.DisplayName ?? Path.GetFileNameWithoutExtension(app.ExePath),
                        app.ExePath,
                        app.Source,
                        isBlocked ? "BLOCKED" : ""
                    );

                    if (isBlocked)
                    {
                        dgvInventory.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                        dgvInventory.Rows[rowIndex].DefaultCellStyle.Font =
                            new Font(dgvInventory.DefaultCellStyle.Font, FontStyle.Bold);
                    }
                }

                dgvInventory.ResumeLayout();
                lblInventoryCount.Text = $"Applications: {filteredApps.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying inventory: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateBlockListStatus()
        {
            // Update blocked count label
            lblBlockedCount.Text = $"Blocked Apps: {_blockList.BlockedProcesses.Count}";

            // Refresh Block List tab - Show ALL blocked apps from JSON
            dgvBlockList.SuspendLayout();
            dgvBlockList.Rows.Clear();

            foreach (var blockedApp in _blockList.BlockedProcesses)
            {
                dgvBlockList.Rows.Add(
                    blockedApp.ProcessName ?? "N/A",
                    blockedApp.ExePath ?? "N/A",
                    blockedApp.AddedBy,
                    blockedApp.AddedAt.ToLocalTime().ToString("g")
                );
            }

            dgvBlockList.ResumeLayout();

            // Also refresh process grid to show blocked status
            RefreshBlockedStatusInProcessGrid();
            RefreshBlockedStatusInInventoryGrid();
        }

        private void RefreshBlockedStatusInProcessGrid()
        {
            if (dgvProcesses.Rows.Count == 0) return;

            foreach (DataGridViewRow row in dgvProcesses.Rows)
            {
                string processName = row.Cells["colProcessName"].Value?.ToString() ?? "";
                string exePath = row.Cells["colExePath"].Value?.ToString() ?? "";

                bool isBlocked = _blockList.BlockedProcesses.Any(b =>
                    (!string.IsNullOrEmpty(b.ProcessName) &&
                     processName.Equals(b.ProcessName, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(b.ExePath) &&
                     exePath != "N/A" && !string.IsNullOrEmpty(exePath) &&
                     exePath.Equals(b.ExePath, StringComparison.OrdinalIgnoreCase)));

                row.Cells["colStatus"].Value = isBlocked ? "BLOCKED" : "";

                if (isBlocked)
                {
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                    row.DefaultCellStyle.Font = new Font(dgvProcesses.DefaultCellStyle.Font, FontStyle.Bold);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.Font = dgvProcesses.DefaultCellStyle.Font;
                }
            }
        }

        private void RefreshBlockedStatusInInventoryGrid()
        {
            if (dgvInventory.Rows.Count == 0) return;

            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                string exePath = row.Cells["colInventoryExePath"].Value?.ToString() ?? "";

                bool isBlocked = _blockList.BlockedProcesses.Any(b =>
                    !string.IsNullOrEmpty(b.ExePath) &&
                    !string.IsNullOrEmpty(exePath) &&
                    exePath.Equals(b.ExePath, StringComparison.OrdinalIgnoreCase));

                row.Cells["colInventoryStatus"].Value = isBlocked ? "BLOCKED" : "";

                if (isBlocked)
                {
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                    row.DefaultCellStyle.Font = new Font(dgvInventory.DefaultCellStyle.Font, FontStyle.Bold);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.Font = dgvInventory.DefaultCellStyle.Font;
                }
            }
        }
        #endregion

        #region Process Tab Event Handlers

        private void btnRefreshProcesses_Click(object sender, EventArgs e)
        {
            RefreshProcesses();
        }

        private void btnBlockSelectedProcess_Click(object sender, EventArgs e)
        {
            if (dgvProcesses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select processes to block.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (DataGridViewRow row in dgvProcesses.SelectedRows)
            {
                string processName = row.Cells["colProcessName"].Value.ToString();
                string exePath = row.Cells["colExePath"].Value.ToString();

                if (exePath == "N/A" || string.IsNullOrEmpty(exePath))
                {
                    // Block by process name only
                    var blockedApp = new BlockedApp
                    {
                        ProcessName = processName,
                        AddedBy = Environment.UserName,
                        AddedAt = DateTime.UtcNow
                    };

                    if (!_blockList.BlockedProcesses.Contains(blockedApp))
                    {
                        _blockList.BlockedProcesses.Add(blockedApp);
                    }
                }
                else
                {
                    // Block by both process name and exe path
                    var blockedApp = new BlockedApp
                    {
                        ProcessName = processName,
                        ExePath = exePath,
                        AddedBy = Environment.UserName,
                        AddedAt = DateTime.UtcNow
                    };

                    if (!_blockList.BlockedProcesses.Contains(blockedApp))
                    {
                        _blockList.BlockedProcesses.Add(blockedApp);
                    }
                }
            }

            SaveBlockList();
            RefreshProcesses();
        }

        private void btnUnblockSelectedProcess_Click(object sender, EventArgs e)
        {
            if (dgvProcesses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select processes to unblock.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool anyRemoved = false;

            foreach (DataGridViewRow row in dgvProcesses.SelectedRows)
            {
                string processName = row.Cells["colProcessName"].Value.ToString();
                string exePath = row.Cells["colExePath"].Value.ToString();

                var toRemove = _blockList.BlockedProcesses
                    .Where(b => (b.ProcessName != null &&
                                b.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)) ||
                               (b.ExePath != null &&
                                exePath != "N/A" &&
                                b.ExePath.Equals(exePath, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                foreach (var item in toRemove)
                {
                    _blockList.BlockedProcesses.Remove(item);
                    anyRemoved = true;
                }
            }

            if (anyRemoved)
            {
                SaveBlockList();
                RefreshProcesses(); // This will update the display
            }
        }

        #endregion

        #region Inventory Tab Event Handlers

        private void btnRescanInventory_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Rescan inventory? This may take a few minutes.\nCheck service logs for progress.",
                    "Rescan Inventory",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Wait a moment then reload
                    Task.Delay(2000).ContinueWith(t =>
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            LoadInventory();
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBlockSelectedApp_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select applications to block.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (DataGridViewRow row in dgvInventory.SelectedRows)
            {
                string exePath = row.Cells["colInventoryExePath"].Value.ToString();
                string displayName = row.Cells["colDisplayName"].Value.ToString();

                var blockedApp = new BlockedApp
                {
                    ProcessName = Path.GetFileNameWithoutExtension(exePath),
                    ExePath = exePath,
                    AddedBy = Environment.UserName,
                    AddedAt = DateTime.UtcNow
                };

                if (!_blockList.BlockedProcesses.Contains(blockedApp))
                {
                    _blockList.BlockedProcesses.Add(blockedApp);
                }
            }

            SaveBlockList();
            DisplayInventory();
        }

        private void btnUnblockSelectedApp_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select applications to unblock.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (DataGridViewRow row in dgvInventory.SelectedRows)
            {
                string exePath = row.Cells["colInventoryExePath"].Value.ToString();

                var toRemove = _blockList.BlockedProcesses
                    .Where(b => b.ExePath != null &&
                               b.ExePath.Equals(exePath, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var item in toRemove)
                {
                    _blockList.BlockedProcesses.Remove(item);
                }
            }

            SaveBlockList();
            DisplayInventory();
        }

        private void txtSearchInventory_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearchInventory.Text.ToLower();

            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                string displayName = row.Cells["colDisplayName"].Value?.ToString().ToLower() ?? "";
                string exePath = row.Cells["colInventoryExePath"].Value?.ToString().ToLower() ?? "";

                bool visible = displayName.Contains(searchText) || exePath.Contains(searchText);
                row.Visible = visible;
            }
        }

        #endregion

        #region Block List Tab Event Handlers

        private void btnAddBlock_Click(object sender, EventArgs e)
        {
            string processName = txtProcessName.Text.Trim();
            string exePath = txtExePath.Text.Trim();

            if (string.IsNullOrEmpty(processName) && string.IsNullOrEmpty(exePath))
            {
                MessageBox.Show("Please enter either Process Name or EXE Path.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var blockedApp = new BlockedApp
            {
                ProcessName = string.IsNullOrEmpty(processName) ? null : processName,
                ExePath = string.IsNullOrEmpty(exePath) ? null : exePath,
                AddedBy = Environment.UserName,
                AddedAt = DateTime.UtcNow
            };

            if (!_blockList.BlockedProcesses.Contains(blockedApp))
            {
                _blockList.BlockedProcesses.Add(blockedApp);
                SaveBlockList();

                txtProcessName.Clear();
                txtExePath.Clear();

                MessageBox.Show("Application added to block list.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("This app is already blocked.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnRemoveBlock_Click(object sender, EventArgs e)
        {
            if (dgvBlockList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select items to remove.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool anyRemoved = false;

            foreach (DataGridViewRow row in dgvBlockList.SelectedRows)
            {
                string processName = row.Cells["colBlockedProcess"].Value?.ToString();
                string exePath = row.Cells["colBlockedExePath"].Value?.ToString();

                // Find exact match
                var toRemove = _blockList.BlockedProcesses
                    .Where(b =>
                        (b.ProcessName == processName || (string.IsNullOrEmpty(processName) && b.ProcessName == null)) &&
                        (b.ExePath == exePath || (string.IsNullOrEmpty(exePath) && b.ExePath == null)))
                    .ToList();

                foreach (var item in toRemove)
                {
                    _blockList.BlockedProcesses.Remove(item);
                    anyRemoved = true;
                }
            }

            if (anyRemoved)
            {
                SaveBlockList();
                // Update all displays
                UpdateBlockListStatus();
                RefreshProcesses();
                DisplayInventory();
            }
        }

        #endregion

        #region Service Control Event Handlers

        private void lblStartService_Click(object sender, EventArgs e)
        {
            if (!lblStartService.Enabled) return;

            try
            {
                if (_serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    _serviceController.Start();
                    _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                    CheckServiceStatus();
                    MessageBox.Show("Service started successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting service: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblStopService_Click(object sender, EventArgs e)
        {
            if (!lblStopService.Enabled) return;

            try
            {
                if (_serviceController.Status == ServiceControllerStatus.Running)
                {
                    _serviceController.Stop();
                    _serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    CheckServiceStatus();
                    MessageBox.Show("Service stopped successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping service: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblOpenLogs_Click(object sender, EventArgs e)
        {
            try
            {
                if (Directory.Exists(_logsPath))
                {
                    Process.Start("explorer.exe", _logsPath);
                }
                else
                {
                    MessageBox.Show("Logs directory does not exist yet.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening logs folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Form Events

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();

            foreach (var process in _currentProcesses)
            {
                try { process.Dispose(); } catch { }
            }
            healthRefreshTimer?.Stop();
            healthRefreshTimer?.Dispose();
            _blockListWatcher?.Dispose();
            SaveFtpConfig();
        }

        #endregion

        
    }
}