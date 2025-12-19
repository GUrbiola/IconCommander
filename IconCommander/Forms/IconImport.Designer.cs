namespace IconCommander.Forms
{
    partial class IconImport
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
            this.themeManager1 = new ZidUtilities.CommonCode.Win.Controls.ThemeManager(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.cmbCollections = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbVeins = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCollection = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtVeinName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtVeinPath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lstFiles = new System.Windows.Forms.ListBox();
            this.btnAddFiles = new System.Windows.Forms.Button();
            this.btnRemoveSelected = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.lblFileCount = new System.Windows.Forms.Label();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.logs = new System.Windows.Forms.ListBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            //
            // themeManager1
            //
            this.themeManager1.ParentForm = this;
            this.themeManager1.Theme = ZidUtilities.CommonCode.Win.ZidThemes.Default;
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Collection";
            //
            // cmbCollections
            //
            this.cmbCollections.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCollections.FormattingEnabled = true;
            this.cmbCollections.Location = new System.Drawing.Point(15, 25);
            this.cmbCollections.Name = "cmbCollections";
            this.cmbCollections.Size = new System.Drawing.Size(400, 21);
            this.cmbCollections.TabIndex = 1;
            this.cmbCollections.SelectedIndexChanged += new System.EventHandler(this.cmbCollections_SelectedIndexChanged);
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select Vein";
            //
            // cmbVeins
            //
            this.cmbVeins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVeins.Enabled = false;
            this.cmbVeins.FormattingEnabled = true;
            this.cmbVeins.Location = new System.Drawing.Point(15, 71);
            this.cmbVeins.Name = "cmbVeins";
            this.cmbVeins.Size = new System.Drawing.Size(400, 21);
            this.cmbVeins.TabIndex = 3;
            this.cmbVeins.SelectedIndexChanged += new System.EventHandler(this.cmbVeins_SelectedIndexChanged);
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 105);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Collection Name:";
            //
            // txtCollection
            //
            this.txtCollection.Location = new System.Drawing.Point(104, 102);
            this.txtCollection.Name = "txtCollection";
            this.txtCollection.ReadOnly = true;
            this.txtCollection.Size = new System.Drawing.Size(311, 20);
            this.txtCollection.TabIndex = 5;
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 131);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Vein Name:";
            //
            // txtVeinName
            //
            this.txtVeinName.Location = new System.Drawing.Point(104, 128);
            this.txtVeinName.Name = "txtVeinName";
            this.txtVeinName.ReadOnly = true;
            this.txtVeinName.Size = new System.Drawing.Size(311, 20);
            this.txtVeinName.TabIndex = 7;
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 157);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Vein Path:";
            //
            // txtVeinPath
            //
            this.txtVeinPath.Location = new System.Drawing.Point(104, 154);
            this.txtVeinPath.Name = "txtVeinPath";
            this.txtVeinPath.ReadOnly = true;
            this.txtVeinPath.Size = new System.Drawing.Size(566, 20);
            this.txtVeinPath.TabIndex = 9;
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 190);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Files to Import (0):";
            //
            // lstFiles
            //
            this.lstFiles.FormattingEnabled = true;
            this.lstFiles.Location = new System.Drawing.Point(15, 206);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstFiles.Size = new System.Drawing.Size(655, 147);
            this.lstFiles.TabIndex = 11;
            //
            // btnAddFiles
            //
            this.btnAddFiles.Enabled = false;
            this.btnAddFiles.Location = new System.Drawing.Point(15, 359);
            this.btnAddFiles.Name = "btnAddFiles";
            this.btnAddFiles.Size = new System.Drawing.Size(100, 23);
            this.btnAddFiles.TabIndex = 12;
            this.btnAddFiles.Text = "Add Files...";
            this.btnAddFiles.UseVisualStyleBackColor = true;
            this.btnAddFiles.Click += new System.EventHandler(this.btnAddFiles_Click);
            //
            // btnRemoveSelected
            //
            this.btnRemoveSelected.Location = new System.Drawing.Point(121, 359);
            this.btnRemoveSelected.Name = "btnRemoveSelected";
            this.btnRemoveSelected.Size = new System.Drawing.Size(120, 23);
            this.btnRemoveSelected.TabIndex = 13;
            this.btnRemoveSelected.Text = "Remove Selected";
            this.btnRemoveSelected.UseVisualStyleBackColor = true;
            this.btnRemoveSelected.Click += new System.EventHandler(this.btnRemoveSelected_Click);
            //
            // btnClearAll
            //
            this.btnClearAll.Location = new System.Drawing.Point(247, 359);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(100, 23);
            this.btnClearAll.TabIndex = 14;
            this.btnClearAll.Text = "Clear All";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.btnClearAll_Click);
            //
            // label7
            //
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(520, 364);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "File Count:";
            //
            // lblFileCount
            //
            this.lblFileCount.AutoSize = true;
            this.lblFileCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFileCount.Location = new System.Drawing.Point(587, 364);
            this.lblFileCount.Name = "lblFileCount";
            this.lblFileCount.Size = new System.Drawing.Size(14, 13);
            this.lblFileCount.TabIndex = 16;
            this.lblFileCount.Text = "0";
            //
            // btnImport
            //
            this.btnImport.Enabled = false;
            this.btnImport.Location = new System.Drawing.Point(15, 394);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(100, 30);
            this.btnImport.TabIndex = 17;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            //
            // btnCancel
            //
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(121, 394);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 18;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // label8
            //
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 437);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Import Log:";
            //
            // logs
            //
            this.logs.FormattingEnabled = true;
            this.logs.Location = new System.Drawing.Point(15, 453);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(655, 95);
            this.logs.TabIndex = 20;
            //
            // progressBar1
            //
            this.progressBar1.Location = new System.Drawing.Point(15, 554);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(655, 23);
            this.progressBar1.TabIndex = 21;
            //
            // bgWorker
            //
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            //
            // IconImport
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 591);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.lblFileCount);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnClearAll);
            this.Controls.Add(this.btnRemoveSelected);
            this.Controls.Add(this.btnAddFiles);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtVeinPath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtVeinName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtCollection);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbVeins);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbCollections);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IconImport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Icons";
            this.Load += new System.EventHandler(this.IconImport_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZidUtilities.CommonCode.Win.Controls.ThemeManager themeManager1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbCollections;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbVeins;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtCollection;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtVeinName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtVeinPath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListBox lstFiles;
        private System.Windows.Forms.Button btnAddFiles;
        private System.Windows.Forms.Button btnRemoveSelected;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblFileCount;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ListBox logs;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.ComponentModel.BackgroundWorker bgWorker;
    }
}
