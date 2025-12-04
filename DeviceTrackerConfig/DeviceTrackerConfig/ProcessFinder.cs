using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace DeviceTrackerConfig.Services
{
    public class ProcessFinder
    {
        public static void FindProcessInfo(string appName)
        {
            string result = $"=== Process Finder: {appName} ===\n\n";

            // 1. Search running processes
            result += "1. RUNNING PROCESSES:\n";
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    if (process.ProcessName.ToLower().Contains(appName.ToLower()) ||
                        (!string.IsNullOrEmpty(process.MainWindowTitle) &&
                         process.MainWindowTitle.ToLower().Contains(appName.ToLower())))
                    {
                        result += $"   • Process Name: {process.ProcessName}.exe\n";
                        result += $"     PID: {process.Id}\n";
                        result += $"     Window Title: {process.MainWindowTitle}\n";
                        try
                        {
                            result += $"     File Path: {process.MainModule?.FileName}\n";
                        }
                        catch { }
                        result += "\n";
                    }
                }
                catch { }
                finally
                {
                    process.Dispose();
                }
            }

            // 2. Search program files for executables
            result += "2. INSTALLED EXECUTABLES:\n";
            string[] searchPaths = {
                @"C:\Program Files",
                @"C:\Program Files (x86)",
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var path in searchPaths)
            {
                if (Directory.Exists(path))
                {
                    SearchForExecutables(path, appName, result);
                }
            }

            // Show results
            MessageBox.Show(result, $"Process Finder: {appName}",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void SearchForExecutables(string folderPath, string appName, string result)
        {
            try
            {
                var exeFiles = Directory.GetFiles(folderPath, "*.exe", SearchOption.AllDirectories);
                foreach (var exe in exeFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(exe);
                    if (fileName.ToLower().Contains(appName.ToLower()))
                    {
                        result += $"   • Found: {exe}\n";
                    }
                }
            }
            catch { }
        }
    }
}