using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DeviceTrackerClient.Services;

namespace DeviceTrackerClient
{
    public partial class MainForm : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private LoggerService loggerService;
        private AppMonitorService appMonitorService;
        private AppInventoryService appInventoryService;
        private readonly string logFilePath;
        private PasswordManager passwordManager;
        private bool isFirstRun = true;

        public MainForm()
        {
            // First, show a message so we know it's starting
            MessageBox.Show("Device Tracker is starting...", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try
            {
                InitializeComponent();
                passwordManager = new PasswordManager();
                // Use Desktop for easier access during testing
                logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DeviceTracker", "logs.json");

                // Create directory
                Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));

                // Initialize components in order
                InitializeTrayIcon();

                // Show balloon tip immediately
                trayIcon.ShowBalloonTip(5000, "Device Tracker", "Application has started successfully!", ToolTipIcon.Info);

                // Initialize services
                InitializeServices();

                // Set auto-start
                SetAutoStart();

                // Hide form
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Hide();
            }
            catch (Exception ex)
            {
                // Write error to file
                string errorFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "DeviceTrackerStartupError.txt");
                File.WriteAllText(errorFile, $"ERROR: {DateTime.Now}\n{ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Startup Error: {ex.Message}\nCheck Desktop for error file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 200);
            this.Name = "MainForm";
            this.Text = "Device Tracker";
            this.ResumeLayout(false);
        }

        private void InitializeServices()
        {
            // First, create a test log file to verify write permissions
            string testFile = Path.Combine(Path.GetDirectoryName(logFilePath), "test.txt");
            File.WriteAllText(testFile, $"Test write at {DateTime.Now}");

            // Initialize services - USE THE NEW CONSTRUCTOR WITHOUT PARAMETERS
            loggerService = new LoggerService();  // CHANGED THIS LINE
            appInventoryService = new AppInventoryService(loggerService);
            appMonitorService = new AppMonitorService(loggerService);

            // Initialize passwordManager if not already done
            passwordManager = new PasswordManager();  // ADD THIS LINE

            // Start monitoring
            appMonitorService.StartMonitoring();

            // Log initial events
            loggerService.LogSystemEvent("Startup", Environment.UserName);
            appInventoryService.LogInstalledApps();

            // Show success message
            trayIcon.ShowBalloonTip(3000, "Success", "All services initialized!", ToolTipIcon.Info);
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Device Tracker";

            // Try different icons
            try
            {
                trayIcon.Icon = SystemIcons.Application;
            }
            catch
            {
                // If that fails, create a simple icon
                Bitmap bmp = new Bitmap(16, 16);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.FillRectangle(Brushes.Blue, 0, 0, 16, 16);
                    g.DrawString("DT", new Font("Arial", 8), Brushes.White, 0, 0);
                }
                trayIcon.Icon = Icon.FromHandle(bmp.GetHicon());
            }

            // Create context menu
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open Logs Folder", null, OnViewLogs);
            trayMenu.Items.Add("Show Test Message", null, (s, e) =>
            {
                MessageBox.Show("Application is running!", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            trayMenu.Items.Add("Exit", null, OnExit);
            trayIcon.ContextMenuStrip = trayMenu;

            trayIcon.Visible = true;

            // Double-click event
            trayIcon.DoubleClick += (s, e) =>
            {
                MessageBox.Show($"Device Tracker is running!\nLogs at: {logFilePath}", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void SetAutoStart()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    string appPath = Application.ExecutablePath;
                    key.SetValue("DeviceTracker", appPath);
                }
                loggerService.LogSystemEvent("AutoStartEnabled", Environment.UserName);
            }
            catch (Exception ex)
            {
                loggerService.LogSystemEvent("AutoStartError", Environment.UserName);
                Debug.WriteLine($"Auto-start error: {ex.Message}");
            }
        }

        private void OnViewLogs(object sender, EventArgs e)
        {
            try
            {
                string logDir = Path.GetDirectoryName(logFilePath);
                if (Directory.Exists(logDir))
                {
                    Process.Start("explorer.exe", logDir);
                }
                else
                {
                    MessageBox.Show($"Log directory not found: {logDir}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (passwordManager == null || (!passwordManager.IsPasswordSet() && isFirstRun))
            {
                // First run - set password
                SetInitialPassword();
            }
            else
            {
                // Verify password before exit
                VerifyPasswordForExit();
            }
        }

        private void SetInitialPassword()
        {
            string password = ShowPasswordDialog("Set Password", "This is your first run. Set a password for uninstalling:");
            if (!string.IsNullOrEmpty(password))
            {
                passwordManager.SetPassword(password);
                isFirstRun = false;
                MessageBox.Show("Password set successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void VerifyPasswordForExit()
        {
            string password = ShowPasswordDialog("Enter Password", "Enter password to exit:");
            if (passwordManager.VerifyPassword(password))
            {
                loggerService?.LogSystemEvent("Shutdown", Environment.UserName);
                appMonitorService?.StopMonitoring();
                trayIcon.Visible = false;
                Application.Exit();
            }
            else
            {
                MessageBox.Show("Incorrect password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ShowPasswordDialog(string title, string prompt)
        {
            using (Form form = new Form())
            {
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = prompt;
                textBox.UseSystemPasswordChar = true;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                form.ClientSize = new System.Drawing.Size(396, 107);
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                trayIcon?.Dispose();
                trayMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}