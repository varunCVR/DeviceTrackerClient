namespace DeviceTrackerConfig.Forms
{
    partial class BlockAppsDialog
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
            this.lstAppsToBlock = new System.Windows.Forms.ListBox();
            this.lblSelectedCount = new System.Windows.Forms.Label();
            this.btnBlockAll = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstAppsToBlock
            // 
            this.lstAppsToBlock.FormattingEnabled = true;
            this.lstAppsToBlock.Location = new System.Drawing.Point(13, 13);
            this.lstAppsToBlock.Name = "lstAppsToBlock";
            this.lstAppsToBlock.Size = new System.Drawing.Size(120, 95);
            this.lstAppsToBlock.TabIndex = 0;
            // 
            // lblSelectedCount
            // 
            this.lblSelectedCount.AutoSize = true;
            this.lblSelectedCount.Location = new System.Drawing.Point(32, 131);
            this.lblSelectedCount.Name = "lblSelectedCount";
            this.lblSelectedCount.Size = new System.Drawing.Size(120, 13);
            this.lblSelectedCount.TabIndex = 1;
            this.lblSelectedCount.Text = "Selected: 0 applications";
            // 
            // btnBlockAll
            // 
            this.btnBlockAll.Location = new System.Drawing.Point(35, 163);
            this.btnBlockAll.Name = "btnBlockAll";
            this.btnBlockAll.Size = new System.Drawing.Size(75, 23);
            this.btnBlockAll.TabIndex = 2;
            this.btnBlockAll.Text = "Block All";
            this.btnBlockAll.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(153, 162);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // BlockAppsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnBlockAll);
            this.Controls.Add(this.lblSelectedCount);
            this.Controls.Add(this.lstAppsToBlock);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BlockAppsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Block Selected Applications";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstAppsToBlock;
        private System.Windows.Forms.Label lblSelectedCount;
        private System.Windows.Forms.Button btnBlockAll;
        private System.Windows.Forms.Button btnCancel;
    }
}