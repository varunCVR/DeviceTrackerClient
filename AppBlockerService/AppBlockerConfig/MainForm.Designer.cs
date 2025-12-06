namespace AppBlockerConfig
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabProcesses = new System.Windows.Forms.TabPage();
            this.panelProcessButtons = new System.Windows.Forms.Panel();
            this.lblProcessCount = new System.Windows.Forms.Label();
            this.btnUnblockSelectedProcess = new System.Windows.Forms.Button();
            this.btnBlockSelectedProcess = new System.Windows.Forms.Button();
            this.btnRefreshProcesses = new System.Windows.Forms.Button();
            this.dgvProcesses = new System.Windows.Forms.DataGridView();
            this.colProcessName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabInventory = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblInventoryCount = new System.Windows.Forms.Label();
            this.btnUnblockSelectedApp = new System.Windows.Forms.Button();
            this.btnBlockSelectedApp = new System.Windows.Forms.Button();
            this.btnRescanInventory = new System.Windows.Forms.Button();
            this.dgvInventory = new System.Windows.Forms.DataGridView();
            this.colDisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInventoryExePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInventoryStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelSearch = new System.Windows.Forms.Panel();
            this.txtSearchInventory = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabBlockList = new System.Windows.Forms.TabPage();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.lblServiceStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblBlockedCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStartService = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStopService = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblOpenLogs = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgvBlockList = new System.Windows.Forms.DataGridView();
            this.colBlockedProcess = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBlockedExePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAddedBy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAddedAt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnRemoveBlock = new System.Windows.Forms.Button();
            this.btnAddBlock = new System.Windows.Forms.Button();
            this.txtExePath = new System.Windows.Forms.TextBox();
            this.txtProcessName = new System.Windows.Forms.TextBox();
            this.lblExePath = new System.Windows.Forms.Label();
            this.lblProcessName = new System.Windows.Forms.Label();
            this.tabHealth = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            this.ramLabel = new System.Windows.Forms.Label();
            this.cpuLabel = new System.Windows.Forms.Label();
            this.pbRam = new System.Windows.Forms.ProgressBar();
            this.pbCpu = new System.Windows.Forms.ProgressBar();
            this.lblTemperature = new System.Windows.Forms.Label();
            this.lblBatteryStatus = new System.Windows.Forms.Label();
            this.lblUptime = new System.Windows.Forms.Label();
            this.lblRamUsage = new System.Windows.Forms.Label();
            this.lblCpuUsage = new System.Windows.Forms.Label();
            this.lbHealthHistory = new System.Windows.Forms.ListBox();
            this.dgvHealthDisks = new System.Windows.Forms.DataGridView();
            this.colDrive = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUsed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFree = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTotal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUsage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabFtp = new System.Windows.Forms.TabPage();
            this.StatusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblFtpStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblFtpControl = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnViewFtpLog = new System.Windows.Forms.Button();
            this.btnUploadNow = new System.Windows.Forms.Button();
            this.gbFtpLog = new System.Windows.Forms.GroupBox();
            this.btnFtpDiagnostic = new System.Windows.Forms.Button();
            this.dgvFtpLog = new System.Windows.Forms.DataGridView();
            this.colTimestamp = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatuss = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gbFtpSettings = new System.Windows.Forms.GroupBox();
            this.btnSaveFtp = new System.Windows.Forms.Button();
            this.btnTestFtp = new System.Windows.Forms.Button();
            this.chkFtpEnabled = new System.Windows.Forms.CheckBox();
            this.txtFtpPass = new System.Windows.Forms.TextBox();
            this.lblFtpPass = new System.Windows.Forms.Label();
            this.txtFtpUser = new System.Windows.Forms.TextBox();
            this.lblFtpUser = new System.Windows.Forms.Label();
            this.txtFtpServer = new System.Windows.Forms.TextBox();
            this.lblServer = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.healthRefreshTimer = new System.Windows.Forms.Timer(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabProcesses.SuspendLayout();
            this.panelProcessButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcesses)).BeginInit();
            this.tabInventory.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInventory)).BeginInit();
            this.panelSearch.SuspendLayout();
            this.tabBlockList.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBlockList)).BeginInit();
            this.tabHealth.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHealthDisks)).BeginInit();
            this.tabFtp.SuspendLayout();
            this.StatusStrip1.SuspendLayout();
            this.gbFtpLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFtpLog)).BeginInit();
            this.gbFtpSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabProcesses);
            this.tabControl.Controls.Add(this.tabInventory);
            this.tabControl.Controls.Add(this.tabBlockList);
            this.tabControl.Controls.Add(this.tabHealth);
            this.tabControl.Controls.Add(this.tabFtp);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(800, 491);
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.tabControl.TabIndex = 0;
            // 
            // tabProcesses
            // 
            this.tabProcesses.Controls.Add(this.panelProcessButtons);
            this.tabProcesses.Controls.Add(this.dgvProcesses);
            this.tabProcesses.Location = new System.Drawing.Point(4, 22);
            this.tabProcesses.Name = "tabProcesses";
            this.tabProcesses.Padding = new System.Windows.Forms.Padding(3);
            this.tabProcesses.Size = new System.Drawing.Size(792, 465);
            this.tabProcesses.TabIndex = 0;
            this.tabProcesses.Text = "Running Processes";
            this.tabProcesses.UseVisualStyleBackColor = true;
            // 
            // panelProcessButtons
            // 
            this.panelProcessButtons.BackColor = System.Drawing.SystemColors.Control;
            this.panelProcessButtons.Controls.Add(this.lblProcessCount);
            this.panelProcessButtons.Controls.Add(this.btnUnblockSelectedProcess);
            this.panelProcessButtons.Controls.Add(this.btnBlockSelectedProcess);
            this.panelProcessButtons.Controls.Add(this.btnRefreshProcesses);
            this.panelProcessButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelProcessButtons.Location = new System.Drawing.Point(3, 362);
            this.panelProcessButtons.Name = "panelProcessButtons";
            this.panelProcessButtons.Size = new System.Drawing.Size(786, 100);
            this.panelProcessButtons.TabIndex = 1;
            // 
            // lblProcessCount
            // 
            this.lblProcessCount.AutoSize = true;
            this.lblProcessCount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblProcessCount.Location = new System.Drawing.Point(0, 87);
            this.lblProcessCount.Name = "lblProcessCount";
            this.lblProcessCount.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.lblProcessCount.Size = new System.Drawing.Size(73, 13);
            this.lblProcessCount.TabIndex = 3;
            this.lblProcessCount.Text = "Processes: 0";
            this.lblProcessCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnUnblockSelectedProcess
            // 
            this.btnUnblockSelectedProcess.Location = new System.Drawing.Point(292, 30);
            this.btnUnblockSelectedProcess.Name = "btnUnblockSelectedProcess";
            this.btnUnblockSelectedProcess.Size = new System.Drawing.Size(75, 23);
            this.btnUnblockSelectedProcess.TabIndex = 2;
            this.btnUnblockSelectedProcess.Text = "Unblock Selected";
            this.btnUnblockSelectedProcess.UseVisualStyleBackColor = true;
            this.btnUnblockSelectedProcess.Click += new System.EventHandler(this.btnUnblockSelectedProcess_Click);
            // 
            // btnBlockSelectedProcess
            // 
            this.btnBlockSelectedProcess.Location = new System.Drawing.Point(158, 30);
            this.btnBlockSelectedProcess.Name = "btnBlockSelectedProcess";
            this.btnBlockSelectedProcess.Size = new System.Drawing.Size(75, 23);
            this.btnBlockSelectedProcess.TabIndex = 1;
            this.btnBlockSelectedProcess.Text = "Block Selected";
            this.btnBlockSelectedProcess.UseVisualStyleBackColor = true;
            this.btnBlockSelectedProcess.Click += new System.EventHandler(this.btnBlockSelectedProcess_Click);
            // 
            // btnRefreshProcesses
            // 
            this.btnRefreshProcesses.Location = new System.Drawing.Point(30, 30);
            this.btnRefreshProcesses.Name = "btnRefreshProcesses";
            this.btnRefreshProcesses.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshProcesses.TabIndex = 0;
            this.btnRefreshProcesses.Text = "Refresh";
            this.btnRefreshProcesses.UseVisualStyleBackColor = true;
            this.btnRefreshProcesses.Click += new System.EventHandler(this.btnRefreshProcesses_Click);
            // 
            // dgvProcesses
            // 
            this.dgvProcesses.AllowUserToAddRows = false;
            this.dgvProcesses.AllowUserToDeleteRows = false;
            this.dgvProcesses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProcesses.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colProcessName,
            this.colPID,
            this.colExePath,
            this.colStatus});
            this.dgvProcesses.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvProcesses.Location = new System.Drawing.Point(3, 3);
            this.dgvProcesses.Name = "dgvProcesses";
            this.dgvProcesses.ReadOnly = true;
            this.dgvProcesses.RowHeadersVisible = false;
            this.dgvProcesses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProcesses.Size = new System.Drawing.Size(786, 353);
            this.dgvProcesses.TabIndex = 0;
            // 
            // colProcessName
            // 
            this.colProcessName.HeaderText = "Process Name";
            this.colProcessName.Name = "colProcessName";
            this.colProcessName.ReadOnly = true;
            this.colProcessName.Width = 250;
            // 
            // colPID
            // 
            this.colPID.HeaderText = "PID";
            this.colPID.Name = "colPID";
            this.colPID.ReadOnly = true;
            // 
            // colExePath
            // 
            this.colExePath.HeaderText = "EXE Path";
            this.colExePath.Name = "colExePath";
            this.colExePath.ReadOnly = true;
            this.colExePath.Width = 250;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "Status";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // tabInventory
            // 
            this.tabInventory.Controls.Add(this.panel1);
            this.tabInventory.Controls.Add(this.dgvInventory);
            this.tabInventory.Controls.Add(this.panelSearch);
            this.tabInventory.Location = new System.Drawing.Point(4, 22);
            this.tabInventory.Name = "tabInventory";
            this.tabInventory.Padding = new System.Windows.Forms.Padding(3);
            this.tabInventory.Size = new System.Drawing.Size(792, 465);
            this.tabInventory.TabIndex = 1;
            this.tabInventory.Text = "Installed Applications";
            this.tabInventory.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblInventoryCount);
            this.panel1.Controls.Add(this.btnUnblockSelectedApp);
            this.panel1.Controls.Add(this.btnBlockSelectedApp);
            this.panel1.Controls.Add(this.btnRescanInventory);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 412);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(786, 50);
            this.panel1.TabIndex = 4;
            // 
            // lblInventoryCount
            // 
            this.lblInventoryCount.AutoSize = true;
            this.lblInventoryCount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblInventoryCount.Location = new System.Drawing.Point(0, 37);
            this.lblInventoryCount.Name = "lblInventoryCount";
            this.lblInventoryCount.Size = new System.Drawing.Size(76, 13);
            this.lblInventoryCount.TabIndex = 3;
            this.lblInventoryCount.Text = "Applications: 0";
            this.lblInventoryCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnUnblockSelectedApp
            // 
            this.btnUnblockSelectedApp.Location = new System.Drawing.Point(229, 11);
            this.btnUnblockSelectedApp.Name = "btnUnblockSelectedApp";
            this.btnUnblockSelectedApp.Size = new System.Drawing.Size(75, 23);
            this.btnUnblockSelectedApp.TabIndex = 2;
            this.btnUnblockSelectedApp.Text = "Unblock Selected";
            this.btnUnblockSelectedApp.UseVisualStyleBackColor = true;
            this.btnUnblockSelectedApp.Click += new System.EventHandler(this.btnUnblockSelectedApp_Click);
            // 
            // btnBlockSelectedApp
            // 
            this.btnBlockSelectedApp.Location = new System.Drawing.Point(132, 11);
            this.btnBlockSelectedApp.Name = "btnBlockSelectedApp";
            this.btnBlockSelectedApp.Size = new System.Drawing.Size(75, 23);
            this.btnBlockSelectedApp.TabIndex = 1;
            this.btnBlockSelectedApp.Text = "Block Selected";
            this.btnBlockSelectedApp.UseVisualStyleBackColor = true;
            this.btnBlockSelectedApp.Click += new System.EventHandler(this.btnBlockSelectedApp_Click);
            // 
            // btnRescanInventory
            // 
            this.btnRescanInventory.Location = new System.Drawing.Point(34, 11);
            this.btnRescanInventory.Name = "btnRescanInventory";
            this.btnRescanInventory.Size = new System.Drawing.Size(75, 23);
            this.btnRescanInventory.TabIndex = 0;
            this.btnRescanInventory.Text = "Rescan Inventory";
            this.btnRescanInventory.UseVisualStyleBackColor = true;
            this.btnRescanInventory.Click += new System.EventHandler(this.btnRescanInventory_Click);
            // 
            // dgvInventory
            // 
            this.dgvInventory.AllowUserToAddRows = false;
            this.dgvInventory.AllowUserToDeleteRows = false;
            this.dgvInventory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInventory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDisplayName,
            this.colInventoryExePath,
            this.colSource,
            this.colInventoryStatus});
            this.dgvInventory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvInventory.Location = new System.Drawing.Point(3, 43);
            this.dgvInventory.Name = "dgvInventory";
            this.dgvInventory.ReadOnly = true;
            this.dgvInventory.RowHeadersVisible = false;
            this.dgvInventory.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvInventory.Size = new System.Drawing.Size(786, 419);
            this.dgvInventory.TabIndex = 3;
            // 
            // colDisplayName
            // 
            this.colDisplayName.HeaderText = "Display Name";
            this.colDisplayName.Name = "colDisplayName";
            this.colDisplayName.ReadOnly = true;
            // 
            // colInventoryExePath
            // 
            this.colInventoryExePath.HeaderText = "EXE Path";
            this.colInventoryExePath.Name = "colInventoryExePath";
            this.colInventoryExePath.ReadOnly = true;
            // 
            // colSource
            // 
            this.colSource.HeaderText = "Source";
            this.colSource.Name = "colSource";
            this.colSource.ReadOnly = true;
            // 
            // colInventoryStatus
            // 
            this.colInventoryStatus.HeaderText = "Status";
            this.colInventoryStatus.Name = "colInventoryStatus";
            this.colInventoryStatus.ReadOnly = true;
            // 
            // panelSearch
            // 
            this.panelSearch.Controls.Add(this.txtSearchInventory);
            this.panelSearch.Controls.Add(this.label1);
            this.panelSearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSearch.Location = new System.Drawing.Point(3, 3);
            this.panelSearch.Name = "panelSearch";
            this.panelSearch.Size = new System.Drawing.Size(786, 40);
            this.panelSearch.TabIndex = 0;
            // 
            // txtSearchInventory
            // 
            this.txtSearchInventory.Location = new System.Drawing.Point(76, 17);
            this.txtSearchInventory.Name = "txtSearchInventory";
            this.txtSearchInventory.Size = new System.Drawing.Size(333, 20);
            this.txtSearchInventory.TabIndex = 2;
            this.txtSearchInventory.Click += new System.EventHandler(this.txtSearchInventory_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Search";
            // 
            // tabBlockList
            // 
            this.tabBlockList.Controls.Add(this.statusStrip);
            this.tabBlockList.Controls.Add(this.splitContainer1);
            this.tabBlockList.Location = new System.Drawing.Point(4, 22);
            this.tabBlockList.Name = "tabBlockList";
            this.tabBlockList.Padding = new System.Windows.Forms.Padding(3);
            this.tabBlockList.Size = new System.Drawing.Size(792, 465);
            this.tabBlockList.TabIndex = 2;
            this.tabBlockList.Text = "Block List";
            this.tabBlockList.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblServiceStatus,
            this.lblBlockedCount,
            this.lblStartService,
            this.lblStopService,
            this.lblOpenLogs});
            this.statusStrip.Location = new System.Drawing.Point(3, 440);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(786, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            // 
            // lblServiceStatus
            // 
            this.lblServiceStatus.Name = "lblServiceStatus";
            this.lblServiceStatus.Size = new System.Drawing.Size(474, 17);
            this.lblServiceStatus.Spring = true;
            this.lblServiceStatus.Text = "Service Status: Unknown";
            // 
            // lblBlockedCount
            // 
            this.lblBlockedCount.Name = "lblBlockedCount";
            this.lblBlockedCount.Size = new System.Drawing.Size(91, 17);
            this.lblBlockedCount.Text = "Blocked Apps: 0";
            // 
            // lblStartService
            // 
            this.lblStartService.Name = "lblStartService";
            this.lblStartService.Size = new System.Drawing.Size(71, 17);
            this.lblStartService.Text = "Start Service";
            this.lblStartService.Click += new System.EventHandler(this.lblStartService_Click);
            // 
            // lblStopService
            // 
            this.lblStopService.Name = "lblStopService";
            this.lblStopService.Size = new System.Drawing.Size(71, 17);
            this.lblStopService.Text = "Stop Service";
            this.lblStopService.Click += new System.EventHandler(this.lblStopService_Click);
            // 
            // lblOpenLogs
            // 
            this.lblOpenLogs.Name = "lblOpenLogs";
            this.lblOpenLogs.Size = new System.Drawing.Size(64, 17);
            this.lblOpenLogs.Text = "Open Logs";
            this.lblOpenLogs.Click += new System.EventHandler(this.lblOpenLogs_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgvBlockList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnRemoveBlock);
            this.splitContainer1.Panel2.Controls.Add(this.btnAddBlock);
            this.splitContainer1.Panel2.Controls.Add(this.txtExePath);
            this.splitContainer1.Panel2.Controls.Add(this.txtProcessName);
            this.splitContainer1.Panel2.Controls.Add(this.lblExePath);
            this.splitContainer1.Panel2.Controls.Add(this.lblProcessName);
            this.splitContainer1.Size = new System.Drawing.Size(786, 459);
            this.splitContainer1.SplitterDistance = 329;
            this.splitContainer1.TabIndex = 0;
            // 
            // dgvBlockList
            // 
            this.dgvBlockList.AllowUserToAddRows = false;
            this.dgvBlockList.AllowUserToDeleteRows = false;
            this.dgvBlockList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBlockList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colBlockedProcess,
            this.colBlockedExePath,
            this.colAddedBy,
            this.colAddedAt});
            this.dgvBlockList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBlockList.Location = new System.Drawing.Point(0, 0);
            this.dgvBlockList.Name = "dgvBlockList";
            this.dgvBlockList.ReadOnly = true;
            this.dgvBlockList.Size = new System.Drawing.Size(786, 329);
            this.dgvBlockList.TabIndex = 0;
            // 
            // colBlockedProcess
            // 
            this.colBlockedProcess.HeaderText = "Process Name";
            this.colBlockedProcess.MinimumWidth = 10;
            this.colBlockedProcess.Name = "colBlockedProcess";
            this.colBlockedProcess.ReadOnly = true;
            this.colBlockedProcess.Width = 300;
            // 
            // colBlockedExePath
            // 
            this.colBlockedExePath.HeaderText = "EXE Path";
            this.colBlockedExePath.Name = "colBlockedExePath";
            this.colBlockedExePath.ReadOnly = true;
            this.colBlockedExePath.Width = 120;
            // 
            // colAddedBy
            // 
            this.colAddedBy.HeaderText = "Added By";
            this.colAddedBy.Name = "colAddedBy";
            this.colAddedBy.ReadOnly = true;
            // 
            // colAddedAt
            // 
            this.colAddedAt.HeaderText = "Added At";
            this.colAddedAt.Name = "colAddedAt";
            this.colAddedAt.ReadOnly = true;
            this.colAddedAt.Width = 150;
            // 
            // btnRemoveBlock
            // 
            this.btnRemoveBlock.Location = new System.Drawing.Point(186, 79);
            this.btnRemoveBlock.Name = "btnRemoveBlock";
            this.btnRemoveBlock.Size = new System.Drawing.Size(121, 23);
            this.btnRemoveBlock.TabIndex = 5;
            this.btnRemoveBlock.Text = "Remove Selected";
            this.btnRemoveBlock.UseVisualStyleBackColor = true;
            this.btnRemoveBlock.Click += new System.EventHandler(this.btnRemoveBlock_Click);
            // 
            // btnAddBlock
            // 
            this.btnAddBlock.Location = new System.Drawing.Point(70, 79);
            this.btnAddBlock.Name = "btnAddBlock";
            this.btnAddBlock.Size = new System.Drawing.Size(110, 23);
            this.btnAddBlock.TabIndex = 4;
            this.btnAddBlock.Text = "Add to Block List";
            this.btnAddBlock.UseVisualStyleBackColor = true;
            this.btnAddBlock.Click += new System.EventHandler(this.btnAddBlock_Click);
            // 
            // txtExePath
            // 
            this.txtExePath.Location = new System.Drawing.Point(120, 45);
            this.txtExePath.Name = "txtExePath";
            this.txtExePath.Size = new System.Drawing.Size(100, 20);
            this.txtExePath.TabIndex = 3;
            // 
            // txtProcessName
            // 
            this.txtProcessName.Location = new System.Drawing.Point(120, 16);
            this.txtProcessName.Name = "txtProcessName";
            this.txtProcessName.Size = new System.Drawing.Size(100, 20);
            this.txtProcessName.TabIndex = 2;
            // 
            // lblExePath
            // 
            this.lblExePath.AutoSize = true;
            this.lblExePath.Location = new System.Drawing.Point(20, 45);
            this.lblExePath.Name = "lblExePath";
            this.lblExePath.Size = new System.Drawing.Size(56, 13);
            this.lblExePath.TabIndex = 1;
            this.lblExePath.Text = "EXE Path:";
            // 
            // lblProcessName
            // 
            this.lblProcessName.AutoSize = true;
            this.lblProcessName.Location = new System.Drawing.Point(17, 16);
            this.lblProcessName.Name = "lblProcessName";
            this.lblProcessName.Size = new System.Drawing.Size(79, 13);
            this.lblProcessName.TabIndex = 0;
            this.lblProcessName.Text = "Process Name:";
            // 
            // tabHealth
            // 
            this.tabHealth.Controls.Add(this.splitContainer2);
            this.tabHealth.Location = new System.Drawing.Point(4, 22);
            this.tabHealth.Name = "tabHealth";
            this.tabHealth.Padding = new System.Windows.Forms.Padding(3);
            this.tabHealth.Size = new System.Drawing.Size(792, 465);
            this.tabHealth.TabIndex = 3;
            this.tabHealth.Text = "Health";
            this.tabHealth.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.button1);
            this.splitContainer2.Panel1.Controls.Add(this.ramLabel);
            this.splitContainer2.Panel1.Controls.Add(this.cpuLabel);
            this.splitContainer2.Panel1.Controls.Add(this.pbRam);
            this.splitContainer2.Panel1.Controls.Add(this.pbCpu);
            this.splitContainer2.Panel1.Controls.Add(this.lblTemperature);
            this.splitContainer2.Panel1.Controls.Add(this.lblBatteryStatus);
            this.splitContainer2.Panel1.Controls.Add(this.lblUptime);
            this.splitContainer2.Panel1.Controls.Add(this.lblRamUsage);
            this.splitContainer2.Panel1.Controls.Add(this.lblCpuUsage);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.lbHealthHistory);
            this.splitContainer2.Panel2.Controls.Add(this.dgvHealthDisks);
            this.splitContainer2.Size = new System.Drawing.Size(786, 459);
            this.splitContainer2.SplitterDistance = 262;
            this.splitContainer2.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(29, 16);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Check";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnTestHealth_Click);
            // 
            // ramLabel
            // 
            this.ramLabel.AutoSize = true;
            this.ramLabel.Location = new System.Drawing.Point(26, 394);
            this.ramLabel.Name = "ramLabel";
            this.ramLabel.Size = new System.Drawing.Size(31, 13);
            this.ramLabel.TabIndex = 9;
            this.ramLabel.Text = "RAM";
            // 
            // cpuLabel
            // 
            this.cpuLabel.AutoSize = true;
            this.cpuLabel.Location = new System.Drawing.Point(26, 348);
            this.cpuLabel.Name = "cpuLabel";
            this.cpuLabel.Size = new System.Drawing.Size(29, 13);
            this.cpuLabel.TabIndex = 8;
            this.cpuLabel.Text = "CPU";
            // 
            // pbRam
            // 
            this.pbRam.Location = new System.Drawing.Point(26, 410);
            this.pbRam.Name = "pbRam";
            this.pbRam.Size = new System.Drawing.Size(210, 23);
            this.pbRam.TabIndex = 6;
            // 
            // pbCpu
            // 
            this.pbCpu.Location = new System.Drawing.Point(26, 364);
            this.pbCpu.Name = "pbCpu";
            this.pbCpu.Size = new System.Drawing.Size(210, 23);
            this.pbCpu.TabIndex = 5;
            // 
            // lblTemperature
            // 
            this.lblTemperature.AutoSize = true;
            this.lblTemperature.Location = new System.Drawing.Point(23, 246);
            this.lblTemperature.Name = "lblTemperature";
            this.lblTemperature.Size = new System.Drawing.Size(73, 13);
            this.lblTemperature.TabIndex = 4;
            this.lblTemperature.Text = "Temperature :";
            // 
            // lblBatteryStatus
            // 
            this.lblBatteryStatus.AutoSize = true;
            this.lblBatteryStatus.Location = new System.Drawing.Point(23, 212);
            this.lblBatteryStatus.Name = "lblBatteryStatus";
            this.lblBatteryStatus.Size = new System.Drawing.Size(46, 13);
            this.lblBatteryStatus.TabIndex = 3;
            this.lblBatteryStatus.Text = "Battery :";
            // 
            // lblUptime
            // 
            this.lblUptime.AutoSize = true;
            this.lblUptime.Location = new System.Drawing.Point(23, 169);
            this.lblUptime.Name = "lblUptime";
            this.lblUptime.Size = new System.Drawing.Size(46, 13);
            this.lblUptime.TabIndex = 2;
            this.lblUptime.Text = "Uptime :";
            // 
            // lblRamUsage
            // 
            this.lblRamUsage.AutoSize = true;
            this.lblRamUsage.Location = new System.Drawing.Point(23, 130);
            this.lblRamUsage.Name = "lblRamUsage";
            this.lblRamUsage.Size = new System.Drawing.Size(37, 13);
            this.lblRamUsage.TabIndex = 1;
            this.lblRamUsage.Text = "RAM :";
            // 
            // lblCpuUsage
            // 
            this.lblCpuUsage.AutoSize = true;
            this.lblCpuUsage.Location = new System.Drawing.Point(23, 96);
            this.lblCpuUsage.Name = "lblCpuUsage";
            this.lblCpuUsage.Size = new System.Drawing.Size(38, 13);
            this.lblCpuUsage.TabIndex = 0;
            this.lblCpuUsage.Text = "CPU : ";
            // 
            // lbHealthHistory
            // 
            this.lbHealthHistory.FormattingEnabled = true;
            this.lbHealthHistory.Location = new System.Drawing.Point(13, 16);
            this.lbHealthHistory.Name = "lbHealthHistory";
            this.lbHealthHistory.Size = new System.Drawing.Size(490, 329);
            this.lbHealthHistory.TabIndex = 0;
            // 
            // dgvHealthDisks
            // 
            this.dgvHealthDisks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHealthDisks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDrive,
            this.colUsed,
            this.colFree,
            this.colTotal,
            this.colUsage});
            this.dgvHealthDisks.Location = new System.Drawing.Point(13, 348);
            this.dgvHealthDisks.Name = "dgvHealthDisks";
            this.dgvHealthDisks.Size = new System.Drawing.Size(490, 104);
            this.dgvHealthDisks.TabIndex = 7;
            // 
            // colDrive
            // 
            this.colDrive.HeaderText = "Drive";
            this.colDrive.Name = "colDrive";
            // 
            // colUsed
            // 
            this.colUsed.HeaderText = "Used (GB)";
            this.colUsed.Name = "colUsed";
            // 
            // colFree
            // 
            this.colFree.HeaderText = "Free (GB)";
            this.colFree.Name = "colFree";
            // 
            // colTotal
            // 
            this.colTotal.HeaderText = "Total (GB)";
            this.colTotal.Name = "colTotal";
            // 
            // colUsage
            // 
            this.colUsage.HeaderText = "Usage %";
            this.colUsage.Name = "colUsage";
            // 
            // tabFtp
            // 
            this.tabFtp.Controls.Add(this.StatusStrip1);
            this.tabFtp.Controls.Add(this.btnClearLog);
            this.tabFtp.Controls.Add(this.btnViewFtpLog);
            this.tabFtp.Controls.Add(this.btnUploadNow);
            this.tabFtp.Controls.Add(this.gbFtpLog);
            this.tabFtp.Controls.Add(this.gbFtpSettings);
            this.tabFtp.Location = new System.Drawing.Point(4, 22);
            this.tabFtp.Name = "tabFtp";
            this.tabFtp.Padding = new System.Windows.Forms.Padding(3);
            this.tabFtp.Size = new System.Drawing.Size(792, 465);
            this.tabFtp.TabIndex = 4;
            this.tabFtp.Text = "FTP Settings";
            this.tabFtp.UseVisualStyleBackColor = true;
            // 
            // StatusStrip1
            // 
            this.StatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblFtpStatus,
            this.lblFtpControl});
            this.StatusStrip1.Location = new System.Drawing.Point(3, 440);
            this.StatusStrip1.Name = "StatusStrip1";
            this.StatusStrip1.Size = new System.Drawing.Size(786, 22);
            this.StatusStrip1.TabIndex = 5;
            this.StatusStrip1.Text = "FTP";
            // 
            // lblFtpStatus
            // 
            this.lblFtpStatus.Name = "lblFtpStatus";
            this.lblFtpStatus.Size = new System.Drawing.Size(69, 17);
            this.lblFtpStatus.Text = "FTP: Offline";
            // 
            // lblFtpControl
            // 
            this.lblFtpControl.Name = "lblFtpControl";
            this.lblFtpControl.Size = new System.Drawing.Size(73, 17);
            this.lblFtpControl.Text = "Upload Now";
            this.lblFtpControl.ToolTipText = "Trigger manual FTP upload";
            this.lblFtpControl.Click += new System.EventHandler(this.lblFtpControl_Click);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(450, 412);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(75, 23);
            this.btnClearLog.TabIndex = 4;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // btnViewFtpLog
            // 
            this.btnViewFtpLog.Location = new System.Drawing.Point(356, 411);
            this.btnViewFtpLog.Name = "btnViewFtpLog";
            this.btnViewFtpLog.Size = new System.Drawing.Size(75, 23);
            this.btnViewFtpLog.TabIndex = 3;
            this.btnViewFtpLog.Text = "View FTP Logs";
            this.btnViewFtpLog.UseVisualStyleBackColor = true;
            this.btnViewFtpLog.Click += new System.EventHandler(this.btnViewFtpLog_Click);
            // 
            // btnUploadNow
            // 
            this.btnUploadNow.Location = new System.Drawing.Point(267, 412);
            this.btnUploadNow.Name = "btnUploadNow";
            this.btnUploadNow.Size = new System.Drawing.Size(75, 23);
            this.btnUploadNow.TabIndex = 2;
            this.btnUploadNow.Text = "Upload Now";
            this.btnUploadNow.UseVisualStyleBackColor = true;
            this.btnUploadNow.Click += new System.EventHandler(this.btnUploadNow_Click);
            // 
            // gbFtpLog
            // 
            this.gbFtpLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFtpLog.Controls.Add(this.button2);
            this.gbFtpLog.Controls.Add(this.btnFtpDiagnostic);
            this.gbFtpLog.Controls.Add(this.dgvFtpLog);
            this.gbFtpLog.Location = new System.Drawing.Point(10, 205);
            this.gbFtpLog.Name = "gbFtpLog";
            this.gbFtpLog.Size = new System.Drawing.Size(774, 204);
            this.gbFtpLog.TabIndex = 1;
            this.gbFtpLog.TabStop = false;
            this.gbFtpLog.Text = "FTP Upload Log";
            // 
            // btnFtpDiagnostic
            // 
            this.btnFtpDiagnostic.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFtpDiagnostic.Location = new System.Drawing.Point(346, 171);
            this.btnFtpDiagnostic.Name = "btnFtpDiagnostic";
            this.btnFtpDiagnostic.Size = new System.Drawing.Size(75, 23);
            this.btnFtpDiagnostic.TabIndex = 2;
            this.btnFtpDiagnostic.Text = "Button";
            this.btnFtpDiagnostic.UseVisualStyleBackColor = true;
            this.btnFtpDiagnostic.Click += new System.EventHandler(this.btnFtpDiagnostic_Click);
            // 
            // dgvFtpLog
            // 
            this.dgvFtpLog.AllowUserToAddRows = false;
            this.dgvFtpLog.AllowUserToDeleteRows = false;
            this.dgvFtpLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFtpLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFtpLog.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTimestamp,
            this.colFile,
            this.colStatuss,
            this.colMessage});
            this.dgvFtpLog.Location = new System.Drawing.Point(16, 20);
            this.dgvFtpLog.Name = "dgvFtpLog";
            this.dgvFtpLog.ReadOnly = true;
            this.dgvFtpLog.RowHeadersVisible = false;
            this.dgvFtpLog.Size = new System.Drawing.Size(752, 145);
            this.dgvFtpLog.TabIndex = 0;
            // 
            // colTimestamp
            // 
            this.colTimestamp.HeaderText = "Timestamp";
            this.colTimestamp.Name = "colTimestamp";
            this.colTimestamp.ReadOnly = true;
            this.colTimestamp.Width = 150;
            // 
            // colFile
            // 
            this.colFile.HeaderText = "File";
            this.colFile.Name = "colFile";
            this.colFile.ReadOnly = true;
            this.colFile.Width = 200;
            // 
            // colStatuss
            // 
            this.colStatuss.HeaderText = "Status";
            this.colStatuss.Name = "colStatuss";
            this.colStatuss.ReadOnly = true;
            // 
            // colMessage
            // 
            this.colMessage.HeaderText = "Message";
            this.colMessage.Name = "colMessage";
            this.colMessage.ReadOnly = true;
            this.colMessage.Width = 200;
            // 
            // gbFtpSettings
            // 
            this.gbFtpSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFtpSettings.Controls.Add(this.btnSaveFtp);
            this.gbFtpSettings.Controls.Add(this.btnTestFtp);
            this.gbFtpSettings.Controls.Add(this.chkFtpEnabled);
            this.gbFtpSettings.Controls.Add(this.txtFtpPass);
            this.gbFtpSettings.Controls.Add(this.lblFtpPass);
            this.gbFtpSettings.Controls.Add(this.txtFtpUser);
            this.gbFtpSettings.Controls.Add(this.lblFtpUser);
            this.gbFtpSettings.Controls.Add(this.txtFtpServer);
            this.gbFtpSettings.Controls.Add(this.lblServer);
            this.gbFtpSettings.Location = new System.Drawing.Point(10, 10);
            this.gbFtpSettings.Name = "gbFtpSettings";
            this.gbFtpSettings.Size = new System.Drawing.Size(776, 180);
            this.gbFtpSettings.TabIndex = 0;
            this.gbFtpSettings.TabStop = false;
            this.gbFtpSettings.Text = "FTP Server Settings";
            // 
            // btnSaveFtp
            // 
            this.btnSaveFtp.Location = new System.Drawing.Point(101, 139);
            this.btnSaveFtp.Name = "btnSaveFtp";
            this.btnSaveFtp.Size = new System.Drawing.Size(75, 23);
            this.btnSaveFtp.TabIndex = 8;
            this.btnSaveFtp.Text = "Save Settings";
            this.btnSaveFtp.UseVisualStyleBackColor = true;
            this.btnSaveFtp.Click += new System.EventHandler(this.btnSaveFtp_Click);
            // 
            // btnTestFtp
            // 
            this.btnTestFtp.Location = new System.Drawing.Point(13, 139);
            this.btnTestFtp.Name = "btnTestFtp";
            this.btnTestFtp.Size = new System.Drawing.Size(75, 23);
            this.btnTestFtp.TabIndex = 7;
            this.btnTestFtp.Text = "Test Connection";
            this.btnTestFtp.UseVisualStyleBackColor = true;
            this.btnTestFtp.Click += new System.EventHandler(this.btnTestFtp_Click);
            // 
            // chkFtpEnabled
            // 
            this.chkFtpEnabled.AutoSize = true;
            this.chkFtpEnabled.Checked = true;
            this.chkFtpEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFtpEnabled.Location = new System.Drawing.Point(16, 116);
            this.chkFtpEnabled.Name = "chkFtpEnabled";
            this.chkFtpEnabled.Size = new System.Drawing.Size(119, 17);
            this.chkFtpEnabled.TabIndex = 6;
            this.chkFtpEnabled.Text = "Enable FTP Upload";
            this.chkFtpEnabled.UseVisualStyleBackColor = true;
            // 
            // txtFtpPass
            // 
            this.txtFtpPass.Location = new System.Drawing.Point(76, 78);
            this.txtFtpPass.Name = "txtFtpPass";
            this.txtFtpPass.PasswordChar = '#';
            this.txtFtpPass.Size = new System.Drawing.Size(100, 20);
            this.txtFtpPass.TabIndex = 5;
            this.txtFtpPass.Text = "ICT75k";
            // 
            // lblFtpPass
            // 
            this.lblFtpPass.AutoSize = true;
            this.lblFtpPass.Location = new System.Drawing.Point(13, 78);
            this.lblFtpPass.Name = "lblFtpPass";
            this.lblFtpPass.Size = new System.Drawing.Size(56, 13);
            this.lblFtpPass.TabIndex = 4;
            this.lblFtpPass.Text = "Password:";
            // 
            // txtFtpUser
            // 
            this.txtFtpUser.Location = new System.Drawing.Point(75, 47);
            this.txtFtpUser.Name = "txtFtpUser";
            this.txtFtpUser.Size = new System.Drawing.Size(100, 20);
            this.txtFtpUser.TabIndex = 3;
            this.txtFtpUser.Text = "ftpUpload@ict75k.ldts.in";
            // 
            // lblFtpUser
            // 
            this.lblFtpUser.AutoSize = true;
            this.lblFtpUser.Location = new System.Drawing.Point(10, 49);
            this.lblFtpUser.Name = "lblFtpUser";
            this.lblFtpUser.Size = new System.Drawing.Size(58, 13);
            this.lblFtpUser.TabIndex = 2;
            this.lblFtpUser.Text = "Username:";
            // 
            // txtFtpServer
            // 
            this.txtFtpServer.Location = new System.Drawing.Point(75, 20);
            this.txtFtpServer.Name = "txtFtpServer";
            this.txtFtpServer.Size = new System.Drawing.Size(100, 20);
            this.txtFtpServer.TabIndex = 1;
            this.txtFtpServer.Text = "ict75k.ldts.in";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(10, 20);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(41, 13);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "Server:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(440, 170);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnQuickTest_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 491);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "App Blocker Configuration";
            this.tabControl.ResumeLayout(false);
            this.tabProcesses.ResumeLayout(false);
            this.panelProcessButtons.ResumeLayout(false);
            this.panelProcessButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcesses)).EndInit();
            this.tabInventory.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInventory)).EndInit();
            this.panelSearch.ResumeLayout(false);
            this.panelSearch.PerformLayout();
            this.tabBlockList.ResumeLayout(false);
            this.tabBlockList.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBlockList)).EndInit();
            this.tabHealth.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHealthDisks)).EndInit();
            this.tabFtp.ResumeLayout(false);
            this.tabFtp.PerformLayout();
            this.StatusStrip1.ResumeLayout(false);
            this.StatusStrip1.PerformLayout();
            this.gbFtpLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvFtpLog)).EndInit();
            this.gbFtpSettings.ResumeLayout(false);
            this.gbFtpSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabProcesses;
        private System.Windows.Forms.TabPage tabInventory;
        private System.Windows.Forms.TabPage tabBlockList;
        private System.Windows.Forms.DataGridView dgvProcesses;
        private System.Windows.Forms.Panel panelProcessButtons;
        private System.Windows.Forms.Button btnBlockSelectedProcess;
        private System.Windows.Forms.Button btnRefreshProcesses;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Button btnUnblockSelectedProcess;
        private System.Windows.Forms.Label lblProcessCount;
        private System.Windows.Forms.TextBox txtSearchInventory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgvInventory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisplayName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInventoryExePath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInventoryStatus;
        private System.Windows.Forms.Label lblInventoryCount;
        private System.Windows.Forms.Button btnUnblockSelectedApp;
        private System.Windows.Forms.Button btnBlockSelectedApp;
        private System.Windows.Forms.Button btnRescanInventory;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgvBlockList;
        private System.Windows.Forms.Button btnRemoveBlock;
        private System.Windows.Forms.Button btnAddBlock;
        private System.Windows.Forms.TextBox txtExePath;
        private System.Windows.Forms.TextBox txtProcessName;
        private System.Windows.Forms.Label lblExePath;
        private System.Windows.Forms.Label lblProcessName;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblServiceStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblBlockedCount;
        private System.Windows.Forms.ToolStripStatusLabel lblStartService;
        private System.Windows.Forms.ToolStripStatusLabel lblStopService;
        private System.Windows.Forms.ToolStripStatusLabel lblOpenLogs;
        private System.Windows.Forms.TabPage tabHealth;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgvHealthDisks;
        private System.Windows.Forms.ProgressBar pbRam;
        private System.Windows.Forms.ProgressBar pbCpu;
        private System.Windows.Forms.Label lblTemperature;
        private System.Windows.Forms.Label lblBatteryStatus;
        private System.Windows.Forms.Label lblUptime;
        private System.Windows.Forms.Label lblRamUsage;
        private System.Windows.Forms.Label lblCpuUsage;
        private System.Windows.Forms.ListBox lbHealthHistory;
        private System.Windows.Forms.Timer healthRefreshTimer;
        private System.Windows.Forms.Label ramLabel;
        private System.Windows.Forms.Label cpuLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDrive;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUsed;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFree;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTotal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUsage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBlockedProcess;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBlockedExePath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddedBy;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddedAt;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProcessName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExePath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.TabPage tabFtp;
        private System.Windows.Forms.GroupBox gbFtpSettings;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.TextBox txtFtpUser;
        private System.Windows.Forms.Label lblFtpUser;
        private System.Windows.Forms.TextBox txtFtpServer;
        private System.Windows.Forms.CheckBox chkFtpEnabled;
        private System.Windows.Forms.TextBox txtFtpPass;
        private System.Windows.Forms.Label lblFtpPass;
        private System.Windows.Forms.GroupBox gbFtpLog;
        private System.Windows.Forms.DataGridView dgvFtpLog;
        private System.Windows.Forms.Button btnSaveFtp;
        private System.Windows.Forms.Button btnTestFtp;
        private System.Windows.Forms.StatusStrip StatusStrip1;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Button btnViewFtpLog;
        private System.Windows.Forms.Button btnUploadNow;
        private System.Windows.Forms.Button btnFtpDiagnostic;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTimestamp;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFile;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatuss;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessage;
        private System.Windows.Forms.ToolStripStatusLabel lblFtpStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblFtpControl;
        private System.Windows.Forms.Button button2;
    }
}