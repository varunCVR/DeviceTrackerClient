namespace DeviceTrackerConfig.Forms
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblServerUrl = new System.Windows.Forms.Label();
            this.lblClientId = new System.Windows.Forms.Label();
            this.lblMachineName = new System.Windows.Forms.Label();
            this.txtServerUrl = new System.Windows.Forms.TextBox();
            this.txtClientId = new System.Windows.Forms.TextBox();
            this.txtMachineName = new System.Windows.Forms.TextBox();
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.chkServiceInstalled = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnInstallService = new System.Windows.Forms.Button();
            this.btnViewLogs = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.btnTestDetection = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.tabBlocking = new System.Windows.Forms.TabPage();
            this.grpQuickBlock = new System.Windows.Forms.GroupBox();
            this.btnQuickTest = new System.Windows.Forms.Button();
            this.btnQuickBlock = new System.Windows.Forms.Button();
            this.txtQuickBlock = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnViewRules = new System.Windows.Forms.Button();
            this.btnExportList = new System.Windows.Forms.Button();
            this.btnUnblockSelected = new System.Windows.Forms.Button();
            this.btnBlockSelected = new System.Windows.Forms.Button();
            this.btnRefreshApps = new System.Windows.Forms.Button();
            this.lblBlockedCount = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lstDetectedApps = new System.Windows.Forms.ListView();
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProcess = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnTestBlocking = new System.Windows.Forms.Button();
            this.grpAddRule = new System.Windows.Forms.GroupBox();
            this.btnSaveRule = new System.Windows.Forms.Button();
            this.chkGracefulTerminate = new System.Windows.Forms.CheckBox();
            this.cmbMatchType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRulePattern = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRuleName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRefreshBlocks = new System.Windows.Forms.Button();
            this.btnEnableBlock = new System.Windows.Forms.Button();
            this.btnRemoveBlock = new System.Windows.Forms.Button();
            this.btnAddBlock = new System.Windows.Forms.Button();
            this.lstBlockedApps = new System.Windows.Forms.ListBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabControl.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.tabBlocking.SuspendLayout();
            this.grpQuickBlock.SuspendLayout();
            this.grpAddRule.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblServerUrl
            // 
            this.lblServerUrl.AutoSize = true;
            this.lblServerUrl.Location = new System.Drawing.Point(37, 39);
            this.lblServerUrl.Name = "lblServerUrl";
            this.lblServerUrl.Size = new System.Drawing.Size(66, 13);
            this.lblServerUrl.TabIndex = 0;
            this.lblServerUrl.Text = "Server URL:";
            // 
            // lblClientId
            // 
            this.lblClientId.AutoSize = true;
            this.lblClientId.Location = new System.Drawing.Point(37, 78);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(50, 13);
            this.lblClientId.TabIndex = 1;
            this.lblClientId.Text = "Client ID:";
            // 
            // lblMachineName
            // 
            this.lblMachineName.AutoSize = true;
            this.lblMachineName.Location = new System.Drawing.Point(37, 117);
            this.lblMachineName.Name = "lblMachineName";
            this.lblMachineName.Size = new System.Drawing.Size(82, 13);
            this.lblMachineName.TabIndex = 2;
            this.lblMachineName.Text = "Machine Name:";
            // 
            // txtServerUrl
            // 
            this.txtServerUrl.Location = new System.Drawing.Point(169, 39);
            this.txtServerUrl.Name = "txtServerUrl";
            this.txtServerUrl.Size = new System.Drawing.Size(203, 20);
            this.txtServerUrl.TabIndex = 3;
            // 
            // txtClientId
            // 
            this.txtClientId.Location = new System.Drawing.Point(169, 75);
            this.txtClientId.Name = "txtClientId";
            this.txtClientId.ReadOnly = true;
            this.txtClientId.Size = new System.Drawing.Size(203, 20);
            this.txtClientId.TabIndex = 4;
            // 
            // txtMachineName
            // 
            this.txtMachineName.Location = new System.Drawing.Point(169, 114);
            this.txtMachineName.Name = "txtMachineName";
            this.txtMachineName.ReadOnly = true;
            this.txtMachineName.Size = new System.Drawing.Size(203, 20);
            this.txtMachineName.TabIndex = 5;
            // 
            // chkAutoStart
            // 
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Location = new System.Drawing.Point(145, 158);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new System.Drawing.Size(117, 17);
            this.chkAutoStart.TabIndex = 6;
            this.chkAutoStart.Text = "Start with Windows";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // chkServiceInstalled
            // 
            this.chkServiceInstalled.AutoSize = true;
            this.chkServiceInstalled.Enabled = false;
            this.chkServiceInstalled.Location = new System.Drawing.Point(145, 182);
            this.chkServiceInstalled.Name = "chkServiceInstalled";
            this.chkServiceInstalled.Size = new System.Drawing.Size(104, 17);
            this.chkServiceInstalled.TabIndex = 7;
            this.chkServiceInstalled.Text = "Service Installed";
            this.chkServiceInstalled.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(40, 211);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(91, 23);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Save Settings";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnInstallService
            // 
            this.btnInstallService.Location = new System.Drawing.Point(158, 211);
            this.btnInstallService.Name = "btnInstallService";
            this.btnInstallService.Size = new System.Drawing.Size(91, 23);
            this.btnInstallService.TabIndex = 9;
            this.btnInstallService.Text = "Install Service";
            this.btnInstallService.UseVisualStyleBackColor = true;
            this.btnInstallService.Click += new System.EventHandler(this.btnInstallService_Click);
            // 
            // btnViewLogs
            // 
            this.btnViewLogs.Location = new System.Drawing.Point(269, 211);
            this.btnViewLogs.Name = "btnViewLogs";
            this.btnViewLogs.Size = new System.Drawing.Size(92, 23);
            this.btnViewLogs.TabIndex = 10;
            this.btnViewLogs.Text = "View Logs";
            this.btnViewLogs.UseVisualStyleBackColor = true;
            this.btnViewLogs.Click += new System.EventHandler(this.btnViewLogs_Click);
            // 
            // btnUninstall
            // 
            this.btnUninstall.Location = new System.Drawing.Point(226, 263);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(75, 23);
            this.btnUninstall.TabIndex = 11;
            this.btnUninstall.Text = "Uninstall";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // btnTestDetection
            // 
            this.btnTestDetection.Location = new System.Drawing.Point(107, 263);
            this.btnTestDetection.Name = "btnTestDetection";
            this.btnTestDetection.Size = new System.Drawing.Size(75, 23);
            this.btnTestDetection.TabIndex = 12;
            this.btnTestDetection.Text = "Test App Detection";
            this.btnTestDetection.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabSettings);
            this.tabControl.Controls.Add(this.tabBlocking);
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1126, 434);
            this.tabControl.TabIndex = 13;
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.txtServerUrl);
            this.tabSettings.Controls.Add(this.btnTestDetection);
            this.tabSettings.Controls.Add(this.lblServerUrl);
            this.tabSettings.Controls.Add(this.btnUninstall);
            this.tabSettings.Controls.Add(this.lblClientId);
            this.tabSettings.Controls.Add(this.btnViewLogs);
            this.tabSettings.Controls.Add(this.txtClientId);
            this.tabSettings.Controls.Add(this.btnInstallService);
            this.tabSettings.Controls.Add(this.txtMachineName);
            this.tabSettings.Controls.Add(this.btnSave);
            this.tabSettings.Controls.Add(this.lblMachineName);
            this.tabSettings.Controls.Add(this.chkServiceInstalled);
            this.tabSettings.Controls.Add(this.chkAutoStart);
            this.tabSettings.Location = new System.Drawing.Point(4, 22);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabSettings.Size = new System.Drawing.Size(740, 408);
            this.tabSettings.TabIndex = 0;
            this.tabSettings.Text = "Settings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // tabBlocking
            // 
            this.tabBlocking.Controls.Add(this.grpQuickBlock);
            this.tabBlocking.Controls.Add(this.btnViewRules);
            this.tabBlocking.Controls.Add(this.btnExportList);
            this.tabBlocking.Controls.Add(this.btnUnblockSelected);
            this.tabBlocking.Controls.Add(this.btnBlockSelected);
            this.tabBlocking.Controls.Add(this.btnRefreshApps);
            this.tabBlocking.Controls.Add(this.lblBlockedCount);
            this.tabBlocking.Controls.Add(this.lblStatus);
            this.tabBlocking.Controls.Add(this.lstDetectedApps);
            this.tabBlocking.Controls.Add(this.btnTestBlocking);
            this.tabBlocking.Controls.Add(this.grpAddRule);
            this.tabBlocking.Controls.Add(this.btnRefreshBlocks);
            this.tabBlocking.Controls.Add(this.btnEnableBlock);
            this.tabBlocking.Controls.Add(this.btnRemoveBlock);
            this.tabBlocking.Controls.Add(this.btnAddBlock);
            this.tabBlocking.Controls.Add(this.lstBlockedApps);
            this.tabBlocking.Location = new System.Drawing.Point(4, 22);
            this.tabBlocking.Name = "tabBlocking";
            this.tabBlocking.Padding = new System.Windows.Forms.Padding(3);
            this.tabBlocking.Size = new System.Drawing.Size(1118, 408);
            this.tabBlocking.TabIndex = 1;
            this.tabBlocking.Text = "Application Blocking";
            this.tabBlocking.UseVisualStyleBackColor = true;
            // 
            // grpQuickBlock
            // 
            this.grpQuickBlock.Controls.Add(this.btnQuickTest);
            this.grpQuickBlock.Controls.Add(this.btnQuickBlock);
            this.grpQuickBlock.Controls.Add(this.txtQuickBlock);
            this.grpQuickBlock.Controls.Add(this.label4);
            this.grpQuickBlock.Location = new System.Drawing.Point(253, 285);
            this.grpQuickBlock.Name = "grpQuickBlock";
            this.grpQuickBlock.Size = new System.Drawing.Size(481, 100);
            this.grpQuickBlock.TabIndex = 15;
            this.grpQuickBlock.TabStop = false;
            this.grpQuickBlock.Text = "Quick Block";
            // 
            // btnQuickTest
            // 
            this.btnQuickTest.Location = new System.Drawing.Point(361, 53);
            this.btnQuickTest.Name = "btnQuickTest";
            this.btnQuickTest.Size = new System.Drawing.Size(75, 23);
            this.btnQuickTest.TabIndex = 3;
            this.btnQuickTest.Text = "Test";
            this.btnQuickTest.UseVisualStyleBackColor = true;
            this.btnQuickTest.Click += new System.EventHandler(this.btnQuickTest_Click);
            // 
            // btnQuickBlock
            // 
            this.btnQuickBlock.Location = new System.Drawing.Point(361, 24);
            this.btnQuickBlock.Name = "btnQuickBlock";
            this.btnQuickBlock.Size = new System.Drawing.Size(75, 23);
            this.btnQuickBlock.TabIndex = 2;
            this.btnQuickBlock.Text = "Block";
            this.btnQuickBlock.UseVisualStyleBackColor = true;
            this.btnQuickBlock.Click += new System.EventHandler(this.btnQuickBlock_Click);
            // 
            // txtQuickBlock
            // 
            this.txtQuickBlock.Location = new System.Drawing.Point(95, 39);
            this.txtQuickBlock.Name = "txtQuickBlock";
            this.txtQuickBlock.Size = new System.Drawing.Size(242, 20);
            this.txtQuickBlock.TabIndex = 1;
            this.txtQuickBlock.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtQuickBlock_KeyPress);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Block App:";
            // 
            // btnViewRules
            // 
            this.btnViewRules.Location = new System.Drawing.Point(577, 256);
            this.btnViewRules.Name = "btnViewRules";
            this.btnViewRules.Size = new System.Drawing.Size(75, 23);
            this.btnViewRules.TabIndex = 14;
            this.btnViewRules.Text = "View Rules";
            this.btnViewRules.UseVisualStyleBackColor = true;
            this.btnViewRules.Click += new System.EventHandler(this.btnViewRules_Click);
            // 
            // btnExportList
            // 
            this.btnExportList.Location = new System.Drawing.Point(496, 256);
            this.btnExportList.Name = "btnExportList";
            this.btnExportList.Size = new System.Drawing.Size(75, 23);
            this.btnExportList.TabIndex = 13;
            this.btnExportList.Text = "Export List";
            this.btnExportList.UseVisualStyleBackColor = true;
            this.btnExportList.Click += new System.EventHandler(this.btnExportList_Click);
            // 
            // btnUnblockSelected
            // 
            this.btnUnblockSelected.Location = new System.Drawing.Point(529, 227);
            this.btnUnblockSelected.Name = "btnUnblockSelected";
            this.btnUnblockSelected.Size = new System.Drawing.Size(75, 23);
            this.btnUnblockSelected.TabIndex = 12;
            this.btnUnblockSelected.Text = "Unblock";
            this.btnUnblockSelected.UseVisualStyleBackColor = true;
            this.btnUnblockSelected.Click += new System.EventHandler(this.btnUnblockSelected_Click);
            // 
            // btnBlockSelected
            // 
            this.btnBlockSelected.Location = new System.Drawing.Point(448, 227);
            this.btnBlockSelected.Name = "btnBlockSelected";
            this.btnBlockSelected.Size = new System.Drawing.Size(75, 23);
            this.btnBlockSelected.TabIndex = 11;
            this.btnBlockSelected.Text = "Block";
            this.btnBlockSelected.UseVisualStyleBackColor = true;
            this.btnBlockSelected.Click += new System.EventHandler(this.btnBlockSelected_Click);
            // 
            // btnRefreshApps
            // 
            this.btnRefreshApps.Location = new System.Drawing.Point(659, 205);
            this.btnRefreshApps.Name = "btnRefreshApps";
            this.btnRefreshApps.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshApps.TabIndex = 10;
            this.btnRefreshApps.Text = "Refresh";
            this.btnRefreshApps.UseVisualStyleBackColor = true;
            this.btnRefreshApps.Click += new System.EventHandler(this.btnRefreshApps_Click);
            // 
            // lblBlockedCount
            // 
            this.lblBlockedCount.AutoSize = true;
            this.lblBlockedCount.Location = new System.Drawing.Point(250, 232);
            this.lblBlockedCount.Name = "lblBlockedCount";
            this.lblBlockedCount.Size = new System.Drawing.Size(84, 13);
            this.lblBlockedCount.TabIndex = 9;
            this.lblBlockedCount.Text = "Blocked: 0 apps";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(250, 205);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(167, 13);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Click Refresh to scan applications";
            // 
            // lstDetectedApps
            // 
            this.lstDetectedApps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colStatus,
            this.colName,
            this.colProcess,
            this.colVersion,
            this.colType,
            this.colFileName});
            this.lstDetectedApps.FullRowSelect = true;
            this.lstDetectedApps.GridLines = true;
            this.lstDetectedApps.HideSelection = false;
            this.lstDetectedApps.Location = new System.Drawing.Point(230, 18);
            this.lstDetectedApps.Name = "lstDetectedApps";
            this.lstDetectedApps.Size = new System.Drawing.Size(882, 178);
            this.lstDetectedApps.TabIndex = 7;
            this.lstDetectedApps.UseCompatibleStateImageBehavior = false;
            this.lstDetectedApps.View = System.Windows.Forms.View.Details;
            this.lstDetectedApps.DoubleClick += new System.EventHandler(this.lstDetectedApps_DoubleClick);
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 58;
            // 
            // colName
            // 
            this.colName.Text = "Application Name";
            this.colName.Width = 120;
            // 
            // colProcess
            // 
            this.colProcess.Text = "Process Name";
            this.colProcess.Width = 98;
            // 
            // colVersion
            // 
            this.colVersion.Text = "Version";
            this.colVersion.Width = 65;
            // 
            // colType
            // 
            this.colType.Text = "Type";
            this.colType.Width = 86;
            // 
            // colFileName
            // 
            this.colFileName.Text = "File Name";
            this.colFileName.Width = 176;
            // 
            // btnTestBlocking
            // 
            this.btnTestBlocking.Location = new System.Drawing.Point(614, 227);
            this.btnTestBlocking.Name = "btnTestBlocking";
            this.btnTestBlocking.Size = new System.Drawing.Size(102, 23);
            this.btnTestBlocking.TabIndex = 6;
            this.btnTestBlocking.Text = "Test Blocking";
            this.btnTestBlocking.UseVisualStyleBackColor = true;
            this.btnTestBlocking.Click += new System.EventHandler(this.btnTestBlocking_Click);
            // 
            // grpAddRule
            // 
            this.grpAddRule.Controls.Add(this.btnSaveRule);
            this.grpAddRule.Controls.Add(this.chkGracefulTerminate);
            this.grpAddRule.Controls.Add(this.cmbMatchType);
            this.grpAddRule.Controls.Add(this.label3);
            this.grpAddRule.Controls.Add(this.txtRulePattern);
            this.grpAddRule.Controls.Add(this.label2);
            this.grpAddRule.Controls.Add(this.txtRuleName);
            this.grpAddRule.Controls.Add(this.label1);
            this.grpAddRule.Location = new System.Drawing.Point(17, 82);
            this.grpAddRule.Name = "grpAddRule";
            this.grpAddRule.Size = new System.Drawing.Size(207, 197);
            this.grpAddRule.TabIndex = 5;
            this.grpAddRule.TabStop = false;
            this.grpAddRule.Text = "Add Block Rule";
            // 
            // btnSaveRule
            // 
            this.btnSaveRule.Location = new System.Drawing.Point(21, 159);
            this.btnSaveRule.Name = "btnSaveRule";
            this.btnSaveRule.Size = new System.Drawing.Size(75, 23);
            this.btnSaveRule.TabIndex = 7;
            this.btnSaveRule.Text = "Save Rule";
            this.btnSaveRule.UseVisualStyleBackColor = true;
            // 
            // chkGracefulTerminate
            // 
            this.chkGracefulTerminate.AutoSize = true;
            this.chkGracefulTerminate.Location = new System.Drawing.Point(21, 136);
            this.chkGracefulTerminate.Name = "chkGracefulTerminate";
            this.chkGracefulTerminate.Size = new System.Drawing.Size(124, 17);
            this.chkGracefulTerminate.TabIndex = 6;
            this.chkGracefulTerminate.Text = "Graceful Termination";
            this.chkGracefulTerminate.UseVisualStyleBackColor = true;
            // 
            // cmbMatchType
            // 
            this.cmbMatchType.FormattingEnabled = true;
            this.cmbMatchType.Items.AddRange(new object[] {
            "ProcessName",
            "WindowTitle",
            "FilePath",
            "ExactProcessName",
            "ExactFilePath"});
            this.cmbMatchType.Location = new System.Drawing.Point(102, 98);
            this.cmbMatchType.Name = "cmbMatchType";
            this.cmbMatchType.Size = new System.Drawing.Size(98, 21);
            this.cmbMatchType.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Match Type:";
            // 
            // txtRulePattern
            // 
            this.txtRulePattern.Location = new System.Drawing.Point(102, 59);
            this.txtRulePattern.Name = "txtRulePattern";
            this.txtRulePattern.Size = new System.Drawing.Size(98, 20);
            this.txtRulePattern.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Pattern to Match:";
            // 
            // txtRuleName
            // 
            this.txtRuleName.Location = new System.Drawing.Point(102, 21);
            this.txtRuleName.Name = "txtRuleName";
            this.txtRuleName.Size = new System.Drawing.Size(98, 20);
            this.txtRuleName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rule Name:";
            // 
            // btnRefreshBlocks
            // 
            this.btnRefreshBlocks.Location = new System.Drawing.Point(17, 338);
            this.btnRefreshBlocks.Name = "btnRefreshBlocks";
            this.btnRefreshBlocks.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshBlocks.TabIndex = 4;
            this.btnRefreshBlocks.Text = "Refresh";
            this.btnRefreshBlocks.UseVisualStyleBackColor = true;
            // 
            // btnEnableBlock
            // 
            this.btnEnableBlock.Location = new System.Drawing.Point(98, 338);
            this.btnEnableBlock.Name = "btnEnableBlock";
            this.btnEnableBlock.Size = new System.Drawing.Size(90, 23);
            this.btnEnableBlock.TabIndex = 3;
            this.btnEnableBlock.Text = "Enable/Disable";
            this.btnEnableBlock.UseVisualStyleBackColor = true;
            // 
            // btnRemoveBlock
            // 
            this.btnRemoveBlock.Location = new System.Drawing.Point(98, 309);
            this.btnRemoveBlock.Name = "btnRemoveBlock";
            this.btnRemoveBlock.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveBlock.TabIndex = 2;
            this.btnRemoveBlock.Text = "Remove";
            this.btnRemoveBlock.UseVisualStyleBackColor = true;
            // 
            // btnAddBlock
            // 
            this.btnAddBlock.Location = new System.Drawing.Point(17, 309);
            this.btnAddBlock.Name = "btnAddBlock";
            this.btnAddBlock.Size = new System.Drawing.Size(75, 23);
            this.btnAddBlock.TabIndex = 1;
            this.btnAddBlock.Text = "Add Block Rule";
            this.btnAddBlock.UseVisualStyleBackColor = true;
            // 
            // lstBlockedApps
            // 
            this.lstBlockedApps.FormattingEnabled = true;
            this.lstBlockedApps.Location = new System.Drawing.Point(17, 18);
            this.lstBlockedApps.Name = "lstBlockedApps";
            this.lstBlockedApps.Size = new System.Drawing.Size(124, 56);
            this.lstBlockedApps.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(740, 408);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1150, 458);
            this.Controls.Add(this.tabControl);
            this.Name = "ConfigForm";
            this.Text = "ConfigForm";
            this.tabControl.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            this.tabBlocking.ResumeLayout(false);
            this.tabBlocking.PerformLayout();
            this.grpQuickBlock.ResumeLayout(false);
            this.grpQuickBlock.PerformLayout();
            this.grpAddRule.ResumeLayout(false);
            this.grpAddRule.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblServerUrl;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.Label lblMachineName;
        private System.Windows.Forms.TextBox txtServerUrl;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.TextBox txtMachineName;
        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.CheckBox chkServiceInstalled;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnInstallService;
        private System.Windows.Forms.Button btnViewLogs;
        private System.Windows.Forms.Button btnUninstall;
        private System.Windows.Forms.Button btnTestDetection;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabBlocking;
        private System.Windows.Forms.Button btnRefreshBlocks;
        private System.Windows.Forms.Button btnEnableBlock;
        private System.Windows.Forms.Button btnRemoveBlock;
        private System.Windows.Forms.Button btnAddBlock;
        private System.Windows.Forms.ListBox lstBlockedApps;
        private System.Windows.Forms.GroupBox grpAddRule;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRuleName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSaveRule;
        private System.Windows.Forms.CheckBox chkGracefulTerminate;
        private System.Windows.Forms.ComboBox cmbMatchType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRulePattern;
        private System.Windows.Forms.Button btnTestBlocking;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ListView lstDetectedApps;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colProcess;
        private System.Windows.Forms.ColumnHeader colVersion;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colFileName;
        private System.Windows.Forms.Button btnBlockSelected;
        private System.Windows.Forms.Button btnRefreshApps;
        private System.Windows.Forms.Label lblBlockedCount;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox grpQuickBlock;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnViewRules;
        private System.Windows.Forms.Button btnExportList;
        private System.Windows.Forms.Button btnUnblockSelected;
        private System.Windows.Forms.TextBox txtQuickBlock;
        private System.Windows.Forms.Button btnQuickTest;
        private System.Windows.Forms.Button btnQuickBlock;
    }
}