namespace IconCommander.Forms
{
    partial class TagEditForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TagEditForm));
            this.themeManager1 = new ZidUtilities.CommonCode.Win.Controls.ThemeManager(this.components);
            this.lblCurrentTags = new System.Windows.Forms.Label();
            this.tokenSelectCurrentTags = new ZidUtilities.CommonCode.Win.Controls.TokenSelect();
            this.lblNewTag = new System.Windows.Forms.Label();
            this.txtNewTag = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblCurrentCount = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pictIconImage = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictIconImage)).BeginInit();
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
            this.themeManager1.ParentForm = this;
            this.themeManager1.Theme = ZidUtilities.CommonCode.Win.ZidThemes.Blue;
            // 
            // lblCurrentTags
            // 
            this.lblCurrentTags.AutoSize = true;
            this.lblCurrentTags.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCurrentTags.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCurrentTags.Location = new System.Drawing.Point(0, 0);
            this.lblCurrentTags.Name = "lblCurrentTags";
            this.lblCurrentTags.Size = new System.Drawing.Size(80, 15);
            this.lblCurrentTags.TabIndex = 0;
            this.lblCurrentTags.Text = "Current Tags:";
            // 
            // tokenSelectCurrentTags
            // 
            this.tokenSelectCurrentTags.BackColor = System.Drawing.Color.White;
            this.tokenSelectCurrentTags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tokenSelectCurrentTags.Dock = System.Windows.Forms.DockStyle.Top;
            this.tokenSelectCurrentTags.Location = new System.Drawing.Point(0, 111);
            this.tokenSelectCurrentTags.MinimumSize = new System.Drawing.Size(100, 32);
            this.tokenSelectCurrentTags.Name = "tokenSelectCurrentTags";
            this.tokenSelectCurrentTags.Size = new System.Drawing.Size(788, 100);
            this.tokenSelectCurrentTags.TabIndex = 1;
            // 
            // lblNewTag
            // 
            this.lblNewTag.AutoSize = true;
            this.lblNewTag.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNewTag.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNewTag.Location = new System.Drawing.Point(0, 211);
            this.lblNewTag.Name = "lblNewTag";
            this.lblNewTag.Size = new System.Drawing.Size(83, 15);
            this.lblNewTag.TabIndex = 5;
            this.lblNewTag.Text = "Add New Tag:";
            // 
            // txtNewTag
            // 
            this.txtNewTag.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtNewTag.Location = new System.Drawing.Point(0, 0);
            this.txtNewTag.Name = "txtNewTag";
            this.txtNewTag.Size = new System.Drawing.Size(788, 23);
            this.txtNewTag.TabIndex = 6;
            this.txtNewTag.TextChanged += new System.EventHandler(this.txtNewTag_TextChanged);
            this.txtNewTag.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNewTag_KeyDown);
            // 
            // btnAdd
            // 
            this.btnAdd.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnAdd.Enabled = false;
            this.btnAdd.Location = new System.Drawing.Point(0, 25);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(788, 35);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Image = global::IconCommander.Properties.Resources.accept_button;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSave.Location = new System.Drawing.Point(12, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(140, 40);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Image = global::IconCommander.Properties.Resources.cancel;
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(158, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(140, 40);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblCurrentCount
            // 
            this.lblCurrentCount.AutoSize = true;
            this.lblCurrentCount.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblCurrentCount.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblCurrentCount.Location = new System.Drawing.Point(80, 0);
            this.lblCurrentCount.Name = "lblCurrentCount";
            this.lblCurrentCount.Size = new System.Drawing.Size(46, 15);
            this.lblCurrentCount.TabIndex = 8;
            this.lblCurrentCount.Text = "(0 tags)";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 311);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(788, 50);
            this.panel1.TabIndex = 10;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lblCurrentCount);
            this.panel2.Controls.Add(this.lblCurrentTags);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 88);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(788, 23);
            this.panel2.TabIndex = 11;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnAdd);
            this.panel3.Controls.Add(this.txtNewTag);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 226);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(788, 60);
            this.panel3.TabIndex = 12;
            // 
            // pictIconImage
            // 
            this.pictIconImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictIconImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictIconImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictIconImage.Location = new System.Drawing.Point(0, 0);
            this.pictIconImage.Name = "pictIconImage";
            this.pictIconImage.Size = new System.Drawing.Size(788, 88);
            this.pictIconImage.TabIndex = 13;
            this.pictIconImage.TabStop = false;
            // 
            // TagEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 361);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.lblNewTag);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tokenSelectCurrentTags);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.pictIconImage);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TagEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Tags";
            this.Load += new System.EventHandler(this.TagEditForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictIconImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZidUtilities.CommonCode.Win.Controls.ThemeManager themeManager1;
        private System.Windows.Forms.Label lblCurrentTags;
        private ZidUtilities.CommonCode.Win.Controls.TokenSelect tokenSelectCurrentTags;
        private System.Windows.Forms.Label lblNewTag;
        private System.Windows.Forms.TextBox txtNewTag;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblCurrentCount;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictIconImage;
    }
}
