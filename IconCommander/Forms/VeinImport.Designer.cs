namespace IconCommander.Forms
{
    partial class VeinImport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VeinImport));
            this.themeManager1 = new ZidUtilities.CommonCode.Win.Controls.ThemeManager(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.cmbVeins = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtVeinPath = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.breadCrumbVeinPath = new ZidUtilities.CommonCode.Win.Controls.AddressBar.AddressBar();
            this.label4 = new System.Windows.Forms.Label();
            this.txtTagsFile = new System.Windows.Forms.TextBox();
            this.btnTagsFile = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.treeVeinRoot = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.logs = new System.Windows.Forms.ListBox();
            this.txtCollection = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.labIconCount = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.labFileCount = new System.Windows.Forms.Label();
            this.chkImage = new System.Windows.Forms.CheckBox();
            this.chkIcon = new System.Windows.Forms.CheckBox();
            this.chkSvg = new System.Windows.Forms.CheckBox();
            this.labTagCount = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.chkReloadJsFile = new System.Windows.Forms.CheckBox();
            this.chkCleanJsFile = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // themeManager1
            // 
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ParentForm = this;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Vein to Import";
            // 
            // cmbVeins
            // 
            this.cmbVeins.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVeins.FormattingEnabled = true;
            this.cmbVeins.Location = new System.Drawing.Point(15, 25);
            this.cmbVeins.Name = "cmbVeins";
            this.cmbVeins.Size = new System.Drawing.Size(655, 21);
            this.cmbVeins.TabIndex = 1;
            this.cmbVeins.SelectedIndexChanged += new System.EventHandler(this.cmbVeins_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Vein Path";
            // 
            // txtVeinPath
            // 
            this.txtVeinPath.Enabled = false;
            this.txtVeinPath.Location = new System.Drawing.Point(15, 66);
            this.txtVeinPath.Name = "txtVeinPath";
            this.txtVeinPath.Size = new System.Drawing.Size(655, 20);
            this.txtVeinPath.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Vein Path";
            // 
            // breadCrumbVeinPath
            // 
            this.breadCrumbVeinPath.BackColor = System.Drawing.Color.White;
            this.breadCrumbVeinPath.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.breadCrumbVeinPath.CurrentNode = null;
            this.breadCrumbVeinPath.Enabled = false;
            this.breadCrumbVeinPath.ForeColor = System.Drawing.SystemColors.Window;
            this.breadCrumbVeinPath.Location = new System.Drawing.Point(15, 107);
            this.breadCrumbVeinPath.MinimumSize = new System.Drawing.Size(331, 23);
            this.breadCrumbVeinPath.Name = "breadCrumbVeinPath";
            this.breadCrumbVeinPath.RootNode = null;
            this.breadCrumbVeinPath.SelectedStyle = ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline)));
            this.breadCrumbVeinPath.Size = new System.Drawing.Size(1316, 30);
            this.breadCrumbVeinPath.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(673, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(180, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Js File with Keywords/Tags for Icons";
            // 
            // txtTagsFile
            // 
            this.txtTagsFile.Enabled = false;
            this.txtTagsFile.Location = new System.Drawing.Point(676, 66);
            this.txtTagsFile.Name = "txtTagsFile";
            this.txtTagsFile.Size = new System.Drawing.Size(625, 20);
            this.txtTagsFile.TabIndex = 8;
            // 
            // btnTagsFile
            // 
            this.btnTagsFile.Location = new System.Drawing.Point(1307, 65);
            this.btnTagsFile.Name = "btnTagsFile";
            this.btnTagsFile.Size = new System.Drawing.Size(24, 20);
            this.btnTagsFile.TabIndex = 9;
            this.btnTagsFile.Text = "...";
            this.btnTagsFile.UseVisualStyleBackColor = true;
            this.btnTagsFile.Click += new System.EventHandler(this.btnTagsFile_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 144);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Vein Root Folder";
            // 
            // treeVeinRoot
            // 
            this.treeVeinRoot.ImageIndex = 0;
            this.treeVeinRoot.ImageList = this.imageList1;
            this.treeVeinRoot.Location = new System.Drawing.Point(15, 163);
            this.treeVeinRoot.Name = "treeVeinRoot";
            this.treeVeinRoot.SelectedImageIndex = 8;
            this.treeVeinRoot.Size = new System.Drawing.Size(655, 292);
            this.treeVeinRoot.TabIndex = 11;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "folder_vertical_open.png");
            this.imageList1.Images.SetKeyName(1, "folder_vertical_document.png");
            this.imageList1.Images.SetKeyName(2, "file_extension_txt.png");
            this.imageList1.Images.SetKeyName(3, "file_extension_png.png");
            this.imageList1.Images.SetKeyName(4, "file_extension_jpeg.png");
            this.imageList1.Images.SetKeyName(5, "file_extension_bmp.png");
            this.imageList1.Images.SetKeyName(6, "document_yellow.png");
            this.imageList1.Images.SetKeyName(7, "draw_star.png");
            this.imageList1.Images.SetKeyName(8, "zone_select.png");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(673, 144);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(133, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Log of Activities Performed";
            // 
            // logs
            // 
            this.logs.FormattingEnabled = true;
            this.logs.Location = new System.Drawing.Point(676, 163);
            this.logs.Name = "logs";
            this.logs.Size = new System.Drawing.Size(655, 342);
            this.logs.TabIndex = 13;
            // 
            // txtCollection
            // 
            this.txtCollection.Enabled = false;
            this.txtCollection.Location = new System.Drawing.Point(676, 25);
            this.txtCollection.Name = "txtCollection";
            this.txtCollection.Size = new System.Drawing.Size(655, 20);
            this.txtCollection.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(673, 8);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Collection";
            // 
            // bgWorker
            // 
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.WorkerSupportsCancellation = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            // 
            // btnCancel
            // 
            this.btnCancel.Enabled = false;
            this.btnCancel.Image = global::IconCommander.Properties.Resources.cancel;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(15, 515);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(655, 43);
            this.btnCancel.TabIndex = 16;
            this.btnCancel.Text = "Cancel Vein Import";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnImport
            // 
            this.btnImport.Image = global::IconCommander.Properties.Resources.more_imports;
            this.btnImport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnImport.Location = new System.Drawing.Point(15, 462);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(655, 43);
            this.btnImport.TabIndex = 6;
            this.btnImport.Text = "Start to Import Icons";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(677, 520);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(91, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Icons on this Vein";
            // 
            // labIconCount
            // 
            this.labIconCount.AutoSize = true;
            this.labIconCount.Location = new System.Drawing.Point(677, 539);
            this.labIconCount.Name = "labIconCount";
            this.labIconCount.Size = new System.Drawing.Size(13, 13);
            this.labIconCount.TabIndex = 18;
            this.labIconCount.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(801, 520);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(86, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "Files on this Vein";
            // 
            // labFileCount
            // 
            this.labFileCount.AutoSize = true;
            this.labFileCount.Location = new System.Drawing.Point(801, 539);
            this.labFileCount.Name = "labFileCount";
            this.labFileCount.Size = new System.Drawing.Size(13, 13);
            this.labFileCount.TabIndex = 20;
            this.labFileCount.Text = "0";
            // 
            // chkImage
            // 
            this.chkImage.AutoSize = true;
            this.chkImage.Enabled = false;
            this.chkImage.Location = new System.Drawing.Point(1039, 527);
            this.chkImage.Name = "chkImage";
            this.chkImage.Size = new System.Drawing.Size(79, 17);
            this.chkImage.TabIndex = 21;
            this.chkImage.Text = "Are Images";
            this.chkImage.UseVisualStyleBackColor = true;
            // 
            // chkIcon
            // 
            this.chkIcon.AutoSize = true;
            this.chkIcon.Enabled = false;
            this.chkIcon.Location = new System.Drawing.Point(1151, 527);
            this.chkIcon.Name = "chkIcon";
            this.chkIcon.Size = new System.Drawing.Size(71, 17);
            this.chkIcon.TabIndex = 22;
            this.chkIcon.Text = "Are Icons";
            this.chkIcon.UseVisualStyleBackColor = true;
            // 
            // chkSvg
            // 
            this.chkSvg.AutoSize = true;
            this.chkSvg.Enabled = false;
            this.chkSvg.Location = new System.Drawing.Point(1259, 527);
            this.chkSvg.Name = "chkSvg";
            this.chkSvg.Size = new System.Drawing.Size(72, 17);
            this.chkSvg.TabIndex = 23;
            this.chkSvg.Text = "Are SVGs";
            this.chkSvg.UseVisualStyleBackColor = true;
            // 
            // labTagCount
            // 
            this.labTagCount.AutoSize = true;
            this.labTagCount.Location = new System.Drawing.Point(920, 539);
            this.labTagCount.Name = "labTagCount";
            this.labTagCount.Size = new System.Drawing.Size(13, 13);
            this.labTagCount.TabIndex = 25;
            this.labTagCount.Text = "0";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(920, 520);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(89, 13);
            this.label11.TabIndex = 24;
            this.label11.Text = "Tags on this Vein";
            // 
            // chkReloadJsFile
            // 
            this.chkReloadJsFile.AutoSize = true;
            this.chkReloadJsFile.Checked = true;
            this.chkReloadJsFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkReloadJsFile.Location = new System.Drawing.Point(1209, 49);
            this.chkReloadJsFile.Name = "chkReloadJsFile";
            this.chkReloadJsFile.Size = new System.Drawing.Size(92, 17);
            this.chkReloadJsFile.TabIndex = 26;
            this.chkReloadJsFile.Text = "Reload Js File";
            this.chkReloadJsFile.UseVisualStyleBackColor = true;
            // 
            // chkCleanJsFile
            // 
            this.chkCleanJsFile.AutoSize = true;
            this.chkCleanJsFile.Checked = true;
            this.chkCleanJsFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCleanJsFile.Location = new System.Drawing.Point(939, 49);
            this.chkCleanJsFile.Name = "chkCleanJsFile";
            this.chkCleanJsFile.Size = new System.Drawing.Size(120, 17);
            this.chkCleanJsFile.TabIndex = 27;
            this.chkCleanJsFile.Text = "Clean Js File Tables";
            this.chkCleanJsFile.UseVisualStyleBackColor = true;
            // 
            // VeinImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1343, 570);
            this.Controls.Add(this.chkCleanJsFile);
            this.Controls.Add(this.chkReloadJsFile);
            this.Controls.Add(this.labTagCount);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.chkSvg);
            this.Controls.Add(this.chkIcon);
            this.Controls.Add(this.chkImage);
            this.Controls.Add(this.labFileCount);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.labIconCount);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtCollection);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.logs);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.treeVeinRoot);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnTagsFile);
            this.Controls.Add(this.txtTagsFile);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.breadCrumbVeinPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtVeinPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbVeins);
            this.Controls.Add(this.label1);
            this.Name = "VeinImport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Vein From Root Folder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VeinImport_FormClosing);
            this.Load += new System.EventHandler(this.VeinImport_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZidUtilities.CommonCode.Win.Controls.ThemeManager themeManager1;
        private ZidUtilities.CommonCode.Win.Controls.AddressBar.AddressBar breadCrumbVeinPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtVeinPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbVeins;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnTagsFile;
        private System.Windows.Forms.TextBox txtTagsFile;
        private System.Windows.Forms.ListBox logs;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TreeView treeVeinRoot;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtCollection;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ImageList imageList1;
        private System.ComponentModel.BackgroundWorker bgWorker;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label labIconCount;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label labFileCount;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chkSvg;
        private System.Windows.Forms.CheckBox chkIcon;
        private System.Windows.Forms.CheckBox chkImage;
        private System.Windows.Forms.Label labTagCount;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox chkCleanJsFile;
        private System.Windows.Forms.CheckBox chkReloadJsFile;
    }
}