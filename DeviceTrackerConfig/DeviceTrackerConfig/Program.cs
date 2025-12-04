using System;
using System.Security.Principal;
using System.Windows.Forms;
using DeviceTrackerConfig.Forms;

namespace DeviceTrackerConfig
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Check for admin rights
            if (!IsRunningAsAdministrator())
            {
                // Restart as admin
                var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exeName)
                    {
                        UseShellExecute = true,
                        Verb = "runas",
                        Arguments = string.Join(" ", args)
                    });
                }
                catch
                {
                    // User cancelled UAC prompt
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check for silent/minimized startup
            bool startMinimized = args.Length > 0 && args[0] == "/minimized";

            if (startMinimized)
            {
                // Just show notification and exit (for auto-start)
                ShowStartupNotification();
            }
            else
            {
                // Show config form
                Application.Run(new ConfigForm());
            }
        }

        static bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void ShowStartupNotification()
        {
            try
            {
                NotifyIcon trayIcon = new NotifyIcon();
                trayIcon.Icon = System.Drawing.SystemIcons.Application;
                trayIcon.BalloonTipTitle = "Device Tracker";
                trayIcon.BalloonTipText = "Device Tracker is running in background";
                trayIcon.Visible = true;
                trayIcon.ShowBalloonTip(3000);

                System.Threading.Thread.Sleep(3500);
                trayIcon.Dispose();
            }
            catch { }
        }
    }
}