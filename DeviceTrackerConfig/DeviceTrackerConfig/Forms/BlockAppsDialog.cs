using DeviceTrackerConfig.Services;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DeviceTrackerConfig.Forms
{
    public partial class BlockAppsDialog : Form
    {
        private List<AppDetectorService.DetectedApp> _selectedApps;

        public BlockAppsDialog(List<AppDetectorService.DetectedApp> selectedApps)
        {
            InitializeComponent();
            _selectedApps = selectedApps;
            LoadAppsList();
        }

        private void LoadAppsList()
        {
            lstAppsToBlock.Items.Clear();

            foreach (var app in _selectedApps)
            {
                lstAppsToBlock.Items.Add($"{app.Name} ({app.ProcessName})");
            }

            lblSelectedCount.Text = $"Selected: {_selectedApps.Count} application(s)";
        }

        private void btnBlockAll_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}