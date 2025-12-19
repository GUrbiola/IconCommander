namespace IconCommander.Forms
{
    partial class IconsForm
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
            this.zidGrid1 = new ZidUtilities.CommonCode.Win.Controls.Grid.ZidGrid();
            this.themeManager1 = new ZidUtilities.CommonCode.Win.Controls.ThemeManager(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnInsert = new System.Windows.Forms.ToolStripButton();
            this.btnUpdate = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAddToBuffer = new System.Windows.Forms.ToolStripButton();
            this.btnAddTags = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelTags = new System.Windows.Forms.Panel();
            this.btnCompareTags = new System.Windows.Forms.Button();
            this.btnAddExistingTag = new System.Windows.Forms.Button();
            this.btnRemoveTag = new System.Windows.Forms.Button();
            this.txtNewTag = new System.Windows.Forms.TextBox();
            this.btnAddNewTag = new System.Windows.Forms.Button();
            this.lstTags = new System.Windows.Forms.ListBox();
            this.lblSelectedIcon = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelTags.SuspendLayout();
            this.SuspendLayout();
            // 
            // zidGrid1
            // 
            this.zidGrid1.CellFont = new System.Drawing.Font("Calibri", 9F);
            this.zidGrid1.ContextMenuImageSize = new System.Drawing.Size(32, 32);
            this.zidGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zidGrid1.Location = new System.Drawing.Point(0, 0);
            this.zidGrid1.Name = "zidGrid1";
            this.zidGrid1.Size = new System.Drawing.Size(693, 525);
            this.zidGrid1.TabIndex = 0;
            this.zidGrid1.TitleFont = new System.Drawing.Font("Calibri", 9.25F, System.Drawing.FontStyle.Bold);
            this.zidGrid1.OnSelectionChanged += new System.EventHandler(this.zidGrid1_OnSelectionChanged);
            // 
            // themeManager1
            // 
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ParentForm = this;
            this.themeManager1.Theme = ZidUtilities.CommonCode.Win.ZidThemes.Default;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnInsert,
            this.btnUpdate,
            this.btnDelete,
            this.toolStripSeparator1,
            this.btnAddToBuffer,
            this.btnAddTags,
            this.toolStripSeparator2,
            this.btnRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1000, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnInsert
            // 
            this.btnInsert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(40, 22);
            this.btnInsert.Text = "Insert";
            this.btnInsert.Visible = false;
            this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(49, 22);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.Visible = false;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(44, 22);
            this.btnDelete.Text = "Delete";
            this.btnDelete.Visible = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnAddToBuffer
            // 
            this.btnAddToBuffer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddToBuffer.Name = "btnAddToBuffer";
            this.btnAddToBuffer.Size = new System.Drawing.Size(82, 22);
            this.btnAddToBuffer.Text = "Add to Buffer";
            this.btnAddToBuffer.Visible = false;
            this.btnAddToBuffer.Click += new System.EventHandler(this.btnAddToBuffer_Click);
            // 
            // btnAddTags
            // 
            this.btnAddTags.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAddTags.Name = "btnAddTags";
            this.btnAddTags.Size = new System.Drawing.Size(59, 22);
            this.btnAddTags.Text = "Add Tags";
            this.btnAddTags.Click += new System.EventHandler(this.btnAddTags_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRefresh
            // 
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(50, 22);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.zidGrid1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelTags);
            this.splitContainer1.Size = new System.Drawing.Size(1000, 525);
            this.splitContainer1.SplitterDistance = 693;
            this.splitContainer1.TabIndex = 2;
            // 
            // panelTags
            // 
            this.panelTags.Controls.Add(this.btnCompareTags);
            this.panelTags.Controls.Add(this.btnAddExistingTag);
            this.panelTags.Controls.Add(this.btnRemoveTag);
            this.panelTags.Controls.Add(this.txtNewTag);
            this.panelTags.Controls.Add(this.btnAddNewTag);
            this.panelTags.Controls.Add(this.lstTags);
            this.panelTags.Controls.Add(this.lblSelectedIcon);
            this.panelTags.Controls.Add(this.label1);
            this.panelTags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTags.Location = new System.Drawing.Point(0, 0);
            this.panelTags.Name = "panelTags";
            this.panelTags.Size = new System.Drawing.Size(303, 525);
            this.panelTags.TabIndex = 0;
            // 
            // btnCompareTags
            // 
            this.btnCompareTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompareTags.Location = new System.Drawing.Point(10, 489);
            this.btnCompareTags.Name = "btnCompareTags";
            this.btnCompareTags.Size = new System.Drawing.Size(283, 28);
            this.btnCompareTags.TabIndex = 7;
            this.btnCompareTags.Text = "Compare Tags with Another Icon";
            this.btnCompareTags.UseVisualStyleBackColor = true;
            this.btnCompareTags.Click += new System.EventHandler(this.btnCompareTags_Click);
            // 
            // btnAddExistingTag
            // 
            this.btnAddExistingTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddExistingTag.Location = new System.Drawing.Point(10, 455);
            this.btnAddExistingTag.Name = "btnAddExistingTag";
            this.btnAddExistingTag.Size = new System.Drawing.Size(283, 28);
            this.btnAddExistingTag.TabIndex = 6;
            this.btnAddExistingTag.Text = "Add Existing Tag";
            this.btnAddExistingTag.UseVisualStyleBackColor = true;
            this.btnAddExistingTag.Click += new System.EventHandler(this.btnAddExistingTag_Click);
            // 
            // btnRemoveTag
            // 
            this.btnRemoveTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveTag.Location = new System.Drawing.Point(10, 421);
            this.btnRemoveTag.Name = "btnRemoveTag";
            this.btnRemoveTag.Size = new System.Drawing.Size(283, 28);
            this.btnRemoveTag.TabIndex = 5;
            this.btnRemoveTag.Text = "Remove Selected Tag";
            this.btnRemoveTag.UseVisualStyleBackColor = true;
            this.btnRemoveTag.Click += new System.EventHandler(this.btnRemoveTag_Click);
            // 
            // txtNewTag
            // 
            this.txtNewTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNewTag.Location = new System.Drawing.Point(10, 360);
            this.txtNewTag.Name = "txtNewTag";
            this.txtNewTag.Size = new System.Drawing.Size(283, 20);
            this.txtNewTag.TabIndex = 4;
            this.txtNewTag.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtNewTag_KeyPress);
            // 
            // btnAddNewTag
            // 
            this.btnAddNewTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddNewTag.Location = new System.Drawing.Point(10, 387);
            this.btnAddNewTag.Name = "btnAddNewTag";
            this.btnAddNewTag.Size = new System.Drawing.Size(283, 28);
            this.btnAddNewTag.TabIndex = 3;
            this.btnAddNewTag.Text = "Add New Tag";
            this.btnAddNewTag.UseVisualStyleBackColor = true;
            this.btnAddNewTag.Click += new System.EventHandler(this.btnAddNewTag_Click);
            // 
            // lstTags
            // 
            this.lstTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTags.FormattingEnabled = true;
            this.lstTags.Location = new System.Drawing.Point(10, 68);
            this.lstTags.Name = "lstTags";
            this.lstTags.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstTags.Size = new System.Drawing.Size(283, 277);
            this.lstTags.TabIndex = 2;
            // 
            // lblSelectedIcon
            // 
            this.lblSelectedIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSelectedIcon.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectedIcon.Location = new System.Drawing.Point(10, 30);
            this.lblSelectedIcon.Name = "lblSelectedIcon";
            this.lblSelectedIcon.Size = new System.Drawing.Size(283, 23);
            this.lblSelectedIcon.TabIndex = 1;
            this.lblSelectedIcon.Text = "(No icon selected)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Tag Manager:";
            // 
            // IconsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 550);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "IconsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage Icons";
            this.Load += new System.EventHandler(this.IconsForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelTags.ResumeLayout(false);
            this.panelTags.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZidUtilities.CommonCode.Win.Controls.Grid.ZidGrid zidGrid1;
        private ZidUtilities.CommonCode.Win.Controls.ThemeManager themeManager1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnInsert;
        private System.Windows.Forms.ToolStripButton btnUpdate;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnAddToBuffer;
        private System.Windows.Forms.ToolStripButton btnAddTags;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelTags;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSelectedIcon;
        private System.Windows.Forms.ListBox lstTags;
        private System.Windows.Forms.Button btnAddNewTag;
        private System.Windows.Forms.TextBox txtNewTag;
        private System.Windows.Forms.Button btnRemoveTag;
        private System.Windows.Forms.Button btnAddExistingTag;
        private System.Windows.Forms.Button btnCompareTags;
    }
}
