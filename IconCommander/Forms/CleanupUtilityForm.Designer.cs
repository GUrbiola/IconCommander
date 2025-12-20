namespace IconCommander.Forms
{
    partial class CleanupUtilityForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.themeManager1 = new ZidUtilities.CommonCode.Win.Controls.ThemeManager(this.components);
            this.grpScan = new System.Windows.Forms.GroupBox();
            this.btnScan = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.grpResults = new System.Windows.Forms.GroupBox();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.grpProgress = new System.Windows.Forms.GroupBox();
            this.lblProgress = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.grpActions = new System.Windows.Forms.GroupBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnRepair = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpScan.SuspendLayout();
            this.grpResults.SuspendLayout();
            this.grpProgress.SuspendLayout();
            this.grpActions.SuspendLayout();
            this.SuspendLayout();
            //
            // themeManager1
            //
            this.themeManager1.ParentForm = this;
            this.themeManager1.Theme = ZidUtilities.CommonCode.Win.ZidThemes.Default;
            //
            // grpScan
            //
            this.grpScan.Controls.Add(this.lblInfo);
            this.grpScan.Controls.Add(this.btnScan);
            this.grpScan.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpScan.Location = new System.Drawing.Point(10, 10);
            this.grpScan.Name = "grpScan";
            this.grpScan.Size = new System.Drawing.Size(764, 80);
            this.grpScan.TabIndex = 0;
            this.grpScan.TabStop = false;
            this.grpScan.Text = "Scan Database";
            //
            // btnScan
            //
            this.btnScan.Location = new System.Drawing.Point(15, 45);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(150, 28);
            this.btnScan.TabIndex = 0;
            this.btnScan.Text = "Scan for Corrupted Files";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            //
            // lblInfo
            //
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(15, 22);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(550, 13);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "This utility will scan all IconFiles in the database and identify files with empty or corrupted binary data.";
            //
            // grpResults
            //
            this.grpResults.Controls.Add(this.lstLog);
            this.grpResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpResults.Location = new System.Drawing.Point(10, 90);
            this.grpResults.Name = "grpResults";
            this.grpResults.Padding = new System.Windows.Forms.Padding(10);
            this.grpResults.Size = new System.Drawing.Size(764, 350);
            this.grpResults.TabIndex = 1;
            this.grpResults.TabStop = false;
            this.grpResults.Text = "Scan Results";
            //
            // lstLog
            //
            this.lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstLog.FormattingEnabled = true;
            this.lstLog.HorizontalScrollbar = true;
            this.lstLog.ItemHeight = 14;
            this.lstLog.Location = new System.Drawing.Point(10, 23);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(744, 317);
            this.lstLog.TabIndex = 0;
            //
            // grpProgress
            //
            this.grpProgress.Controls.Add(this.progressBar);
            this.grpProgress.Controls.Add(this.lblProgress);
            this.grpProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpProgress.Location = new System.Drawing.Point(10, 440);
            this.grpProgress.Name = "grpProgress";
            this.grpProgress.Size = new System.Drawing.Size(764, 60);
            this.grpProgress.TabIndex = 2;
            this.grpProgress.TabStop = false;
            this.grpProgress.Text = "Progress";
            //
            // lblProgress
            //
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(15, 20);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(38, 13);
            this.lblProgress.TabIndex = 0;
            this.lblProgress.Text = "Ready";
            //
            // progressBar
            //
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(15, 36);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(733, 16);
            this.progressBar.TabIndex = 1;
            //
            // grpActions
            //
            this.grpActions.Controls.Add(this.btnClose);
            this.grpActions.Controls.Add(this.btnRepair);
            this.grpActions.Controls.Add(this.btnDelete);
            this.grpActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.grpActions.Location = new System.Drawing.Point(10, 500);
            this.grpActions.Name = "grpActions";
            this.grpActions.Size = new System.Drawing.Size(764, 60);
            this.grpActions.TabIndex = 3;
            this.grpActions.TabStop = false;
            this.grpActions.Text = "Actions";
            //
            // btnDelete
            //
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(15, 22);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(150, 28);
            this.btnDelete.TabIndex = 0;
            this.btnDelete.Text = "Delete Corrupted Files";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            //
            // btnRepair
            //
            this.btnRepair.Enabled = false;
            this.btnRepair.Location = new System.Drawing.Point(180, 22);
            this.btnRepair.Name = "btnRepair";
            this.btnRepair.Size = new System.Drawing.Size(150, 28);
            this.btnRepair.TabIndex = 2;
            this.btnRepair.Text = "Repair from Original Files";
            this.btnRepair.UseVisualStyleBackColor = true;
            this.btnRepair.Click += new System.EventHandler(this.btnRepair_Click);
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(659, 22);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 28);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // CleanupUtilityForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 570);
            this.Controls.Add(this.grpResults);
            this.Controls.Add(this.grpProgress);
            this.Controls.Add(this.grpActions);
            this.Controls.Add(this.grpScan);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "CleanupUtilityForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Database Cleanup Utility";
            this.Load += new System.EventHandler(this.CleanupUtilityForm_Load);
            this.grpScan.ResumeLayout(false);
            this.grpScan.PerformLayout();
            this.grpResults.ResumeLayout(false);
            this.grpProgress.ResumeLayout(false);
            this.grpProgress.PerformLayout();
            this.grpActions.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private ZidUtilities.CommonCode.Win.Controls.ThemeManager themeManager1;
        private System.Windows.Forms.GroupBox grpScan;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.GroupBox grpResults;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.GroupBox grpProgress;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.GroupBox grpActions;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnRepair;
        private System.Windows.Forms.Button btnClose;
    }
}
