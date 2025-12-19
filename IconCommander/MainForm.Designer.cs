namespace IconCommander
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.projectStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.databaseStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.veinsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.importVeinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iconsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.manageToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.importIconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataBaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDataBaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sqliteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mSSQLServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.themeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.themeManager1 = new ZidUtilities.CommonCode.Win.Controls.ThemeManager(this.components);
            this.topFilterPanel = new System.Windows.Forms.Panel();
            this.btnApplyFilter = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.tokenSelectTags = new ZidUtilities.CommonCode.Win.Controls.TokenSelect();
            this.lblTags = new System.Windows.Forms.Label();
            this.tokenSelectVeins = new ZidUtilities.CommonCode.Win.Controls.TokenSelect();
            this.lblVeins = new System.Windows.Forms.Label();
            this.tokenSelectCollections = new ZidUtilities.CommonCode.Win.Controls.TokenSelect();
            this.lblCollections = new System.Windows.Forms.Label();
            this.chkListTypes = new System.Windows.Forms.CheckedListBox();
            this.lblTypes = new System.Windows.Forms.Label();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.bufferFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.grpProjectInfo = new System.Windows.Forms.GroupBox();
            this.lblBuffer = new System.Windows.Forms.Label();
            this.btnRemoveFromBuffer = new System.Windows.Forms.Button();
            this.btnMerge = new System.Windows.Forms.Button();
            this.btnExportToProject = new System.Windows.Forms.Button();
            this.centerRightSplitContainer = new System.Windows.Forms.SplitContainer();
            this.centerPanel = new System.Windows.Forms.Panel();
            this.iconsFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.topCenterPanel = new System.Windows.Forms.Panel();
            this.labTags = new System.Windows.Forms.Label();
            this.labIconName = new System.Windows.Forms.Label();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.cmbPageSize = new System.Windows.Forms.ComboBox();
            this.lblPageSize = new System.Windows.Forms.Label();
            this.lblTotalResults = new System.Windows.Forms.Label();
            this.cmbIconSize = new System.Windows.Forms.ComboBox();
            this.lblIconSize = new System.Windows.Forms.Label();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.btnMoveToBuffer = new System.Windows.Forms.Button();
            this.iconDetailsFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblIconDetails = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panelProjectInfo = new System.Windows.Forms.Panel();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.topFilterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.grpProjectInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.centerRightSplitContainer)).BeginInit();
            this.centerRightSplitContainer.Panel1.SuspendLayout();
            this.centerRightSplitContainer.Panel2.SuspendLayout();
            this.centerRightSplitContainer.SuspendLayout();
            this.centerPanel.SuspendLayout();
            this.topCenterPanel.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel,
            this.projectStatusLabel,
            this.databaseStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 666);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.statusStrip.Size = new System.Drawing.Size(1546, 24);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(39, 19);
            this.toolStripStatusLabel.Text = "Ready";
            // 
            // projectStatusLabel
            // 
            this.projectStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.projectStatusLabel.Name = "projectStatusLabel";
            this.projectStatusLabel.Size = new System.Drawing.Size(67, 19);
            this.projectStatusLabel.Text = "No project";
            // 
            // databaseStatusLabel
            // 
            this.databaseStatusLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.databaseStatusLabel.Name = "databaseStatusLabel";
            this.databaseStatusLabel.Size = new System.Drawing.Size(77, 19);
            this.databaseStatusLabel.Text = "No database";
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectToolStripMenuItem,
            this.iconsToolStripMenuItem,
            this.dataBaseToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(1546, 24);
            this.menuStrip.TabIndex = 2;
            this.menuStrip.Text = "menuStrip1";
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createProjectToolStripMenuItem,
            this.openProjectToolStripMenuItem,
            this.editProjectToolStripMenuItem});
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.projectToolStripMenuItem.Text = "&Project";
            //
            // createProjectToolStripMenuItem
            //
            this.createProjectToolStripMenuItem.Name = "createProjectToolStripMenuItem";
            this.createProjectToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.createProjectToolStripMenuItem.Text = "&Create Project...";
            this.createProjectToolStripMenuItem.Click += new System.EventHandler(this.createProjectToolStripMenuItem_Click);
            //
            // openProjectToolStripMenuItem
            //
            this.openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
            this.openProjectToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openProjectToolStripMenuItem.Text = "&Open Project...";
            this.openProjectToolStripMenuItem.Click += new System.EventHandler(this.openProjectToolStripMenuItem_Click);
            //
            // editProjectToolStripMenuItem
            //
            this.editProjectToolStripMenuItem.Name = "editProjectToolStripMenuItem";
            this.editProjectToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.editProjectToolStripMenuItem.Text = "&Edit Project...";
            this.editProjectToolStripMenuItem.Click += new System.EventHandler(this.editProjectToolStripMenuItem_Click);
            // 
            // iconsToolStripMenuItem
            // 
            this.iconsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.collectionsToolStripMenuItem,
            this.veinsToolStripMenuItem,
            this.iconsToolStripMenuItem1});
            this.iconsToolStripMenuItem.Name = "iconsToolStripMenuItem";
            this.iconsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.iconsToolStripMenuItem.Text = "Icons";
            // 
            // collectionsToolStripMenuItem
            // 
            this.collectionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageToolStripMenuItem});
            this.collectionsToolStripMenuItem.Name = "collectionsToolStripMenuItem";
            this.collectionsToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.collectionsToolStripMenuItem.Text = "Collections";
            // 
            // manageToolStripMenuItem
            // 
            this.manageToolStripMenuItem.Name = "manageToolStripMenuItem";
            this.manageToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.manageToolStripMenuItem.Text = "Manage...";
            this.manageToolStripMenuItem.Click += new System.EventHandler(this.manageCollectionsToolStripMenuItem_Click);
            // 
            // veinsToolStripMenuItem
            // 
            this.veinsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageToolStripMenuItem1,
            this.importVeinToolStripMenuItem});
            this.veinsToolStripMenuItem.Name = "veinsToolStripMenuItem";
            this.veinsToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.veinsToolStripMenuItem.Text = "Veins";
            // 
            // manageToolStripMenuItem1
            // 
            this.manageToolStripMenuItem1.Name = "manageToolStripMenuItem1";
            this.manageToolStripMenuItem1.Size = new System.Drawing.Size(144, 22);
            this.manageToolStripMenuItem1.Text = "Manage...";
            this.manageToolStripMenuItem1.Click += new System.EventHandler(this.manageVeinsToolStripMenuItem_Click);
            // 
            // importVeinToolStripMenuItem
            // 
            this.importVeinToolStripMenuItem.Name = "importVeinToolStripMenuItem";
            this.importVeinToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.importVeinToolStripMenuItem.Text = "Import Vein...";
            this.importVeinToolStripMenuItem.Click += new System.EventHandler(this.importVeinToolStripMenuItem_Click);
            // 
            // iconsToolStripMenuItem1
            // 
            this.iconsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageToolStripMenuItem2,
            this.importIconsToolStripMenuItem});
            this.iconsToolStripMenuItem1.Name = "iconsToolStripMenuItem1";
            this.iconsToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.iconsToolStripMenuItem1.Text = "Icons";
            // 
            // manageToolStripMenuItem2
            // 
            this.manageToolStripMenuItem2.Name = "manageToolStripMenuItem2";
            this.manageToolStripMenuItem2.Size = new System.Drawing.Size(150, 22);
            this.manageToolStripMenuItem2.Text = "Manage...";
            this.manageToolStripMenuItem2.Click += new System.EventHandler(this.manageIconsToolStripMenuItem_Click);
            // 
            // importIconsToolStripMenuItem
            // 
            this.importIconsToolStripMenuItem.Name = "importIconsToolStripMenuItem";
            this.importIconsToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.importIconsToolStripMenuItem.Text = "Import Icons...";
            this.importIconsToolStripMenuItem.Click += new System.EventHandler(this.importIconsToolStripMenuItem_Click);
            // 
            // dataBaseToolStripMenuItem
            // 
            this.dataBaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openDataBaseToolStripMenuItem});
            this.dataBaseToolStripMenuItem.Name = "dataBaseToolStripMenuItem";
            this.dataBaseToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.dataBaseToolStripMenuItem.Text = "&Data Base";
            // 
            // openDataBaseToolStripMenuItem
            // 
            this.openDataBaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sqliteToolStripMenuItem,
            this.mSSQLServerToolStripMenuItem});
            this.openDataBaseToolStripMenuItem.Name = "openDataBaseToolStripMenuItem";
            this.openDataBaseToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.openDataBaseToolStripMenuItem.Text = "&Open Data Base...";
            // 
            // sqliteToolStripMenuItem
            // 
            this.sqliteToolStripMenuItem.Name = "sqliteToolStripMenuItem";
            this.sqliteToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.sqliteToolStripMenuItem.Text = "Sqlite...";
            this.sqliteToolStripMenuItem.Click += new System.EventHandler(this.sqliteToolStripMenuItem_Click);
            // 
            // mSSQLServerToolStripMenuItem
            // 
            this.mSSQLServerToolStripMenuItem.Name = "mSSQLServerToolStripMenuItem";
            this.mSSQLServerToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.mSSQLServerToolStripMenuItem.Text = "MS SQL Server..";
            this.mSSQLServerToolStripMenuItem.Click += new System.EventHandler(this.mSSQLServerToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.themeToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // themeToolStripMenuItem
            // 
            this.themeToolStripMenuItem.Name = "themeToolStripMenuItem";
            this.themeToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.themeToolStripMenuItem.Text = "&Theme";
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
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ParentForm = this;
            this.themeManager1.Theme = ZidUtilities.CommonCode.Win.ZidThemes.Default;
            // 
            // topFilterPanel
            // 
            this.topFilterPanel.Controls.Add(this.btnApplyFilter);
            this.topFilterPanel.Controls.Add(this.txtSearch);
            this.topFilterPanel.Controls.Add(this.lblSearch);
            this.topFilterPanel.Controls.Add(this.tokenSelectTags);
            this.topFilterPanel.Controls.Add(this.lblTags);
            this.topFilterPanel.Controls.Add(this.tokenSelectVeins);
            this.topFilterPanel.Controls.Add(this.lblVeins);
            this.topFilterPanel.Controls.Add(this.tokenSelectCollections);
            this.topFilterPanel.Controls.Add(this.lblCollections);
            this.topFilterPanel.Controls.Add(this.chkListTypes);
            this.topFilterPanel.Controls.Add(this.lblTypes);
            this.topFilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topFilterPanel.Location = new System.Drawing.Point(0, 24);
            this.topFilterPanel.Name = "topFilterPanel";
            this.topFilterPanel.Padding = new System.Windows.Forms.Padding(8);
            this.topFilterPanel.Size = new System.Drawing.Size(1546, 100);
            this.topFilterPanel.TabIndex = 4;
            // 
            // btnApplyFilter
            // 
            this.btnApplyFilter.Location = new System.Drawing.Point(1320, 54);
            this.btnApplyFilter.Name = "btnApplyFilter";
            this.btnApplyFilter.Size = new System.Drawing.Size(200, 40);
            this.btnApplyFilter.TabIndex = 10;
            this.btnApplyFilter.Text = "Apply Filter";
            this.btnApplyFilter.UseVisualStyleBackColor = true;
            this.btnApplyFilter.Click += new System.EventHandler(this.btnApplyFilter_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(1320, 28);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(200, 20);
            this.txtSearch.TabIndex = 9;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSearch.Location = new System.Drawing.Point(1317, 12);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(68, 13);
            this.lblSearch.TabIndex = 8;
            this.lblSearch.Text = "Search Text:";
            this.toolTip1.SetToolTip(this.lblSearch, "Type the text you want to search");
            // 
            // tokenSelectTags
            // 
            this.tokenSelectTags.Location = new System.Drawing.Point(947, 28);
            this.tokenSelectTags.MinimumSize = new System.Drawing.Size(100, 32);
            this.tokenSelectTags.Name = "tokenSelectTags";
            this.tokenSelectTags.Size = new System.Drawing.Size(327, 60);
            this.tokenSelectTags.TabIndex = 7;
            // 
            // lblTags
            // 
            this.lblTags.AutoSize = true;
            this.lblTags.Location = new System.Drawing.Point(944, 8);
            this.lblTags.Name = "lblTags";
            this.lblTags.Size = new System.Drawing.Size(34, 13);
            this.lblTags.TabIndex = 6;
            this.lblTags.Text = "Tags:";
            // 
            // tokenSelectVeins
            // 
            this.tokenSelectVeins.Location = new System.Drawing.Point(601, 27);
            this.tokenSelectVeins.MinimumCharactersForDropdown = 0;
            this.tokenSelectVeins.MinimumSize = new System.Drawing.Size(100, 32);
            this.tokenSelectVeins.Name = "tokenSelectVeins";
            this.tokenSelectVeins.Size = new System.Drawing.Size(327, 60);
            this.tokenSelectVeins.TabIndex = 5;
            // 
            // lblVeins
            // 
            this.lblVeins.AutoSize = true;
            this.lblVeins.Location = new System.Drawing.Point(598, 8);
            this.lblVeins.Name = "lblVeins";
            this.lblVeins.Size = new System.Drawing.Size(36, 13);
            this.lblVeins.TabIndex = 4;
            this.lblVeins.Text = "Veins:";
            // 
            // tokenSelectCollections
            // 
            this.tokenSelectCollections.Location = new System.Drawing.Point(255, 28);
            this.tokenSelectCollections.MinimumCharactersForDropdown = 0;
            this.tokenSelectCollections.MinimumSize = new System.Drawing.Size(100, 32);
            this.tokenSelectCollections.Name = "tokenSelectCollections";
            this.tokenSelectCollections.Size = new System.Drawing.Size(327, 60);
            this.tokenSelectCollections.TabIndex = 3;
            // 
            // lblCollections
            // 
            this.lblCollections.AutoSize = true;
            this.lblCollections.Location = new System.Drawing.Point(255, 11);
            this.lblCollections.Name = "lblCollections";
            this.lblCollections.Size = new System.Drawing.Size(61, 13);
            this.lblCollections.TabIndex = 2;
            this.lblCollections.Text = "Collections:";
            // 
            // chkListTypes
            // 
            this.chkListTypes.CheckOnClick = true;
            this.chkListTypes.FormattingEnabled = true;
            this.chkListTypes.Items.AddRange(new object[] {
            "Image",
            "Icon",
            "SVG"});
            this.chkListTypes.Location = new System.Drawing.Point(11, 28);
            this.chkListTypes.Name = "chkListTypes";
            this.chkListTypes.Size = new System.Drawing.Size(239, 64);
            this.chkListTypes.TabIndex = 1;
            // 
            // lblTypes
            // 
            this.lblTypes.AutoSize = true;
            this.lblTypes.Location = new System.Drawing.Point(11, 11);
            this.lblTypes.Name = "lblTypes";
            this.lblTypes.Size = new System.Drawing.Size(39, 13);
            this.lblTypes.TabIndex = 0;
            this.lblTypes.Text = "Types:";
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 124);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.leftPanel);
            this.mainSplitContainer.Panel1MinSize = 200;
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.centerRightSplitContainer);
            this.mainSplitContainer.Size = new System.Drawing.Size(1546, 542);
            this.mainSplitContainer.SplitterDistance = 250;
            this.mainSplitContainer.TabIndex = 5;
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.bufferFlowPanel);
            this.leftPanel.Controls.Add(this.lblBuffer);
            this.leftPanel.Controls.Add(this.btnRemoveFromBuffer);
            this.leftPanel.Controls.Add(this.btnMerge);
            this.leftPanel.Controls.Add(this.btnExportToProject);
            this.leftPanel.Controls.Add(this.grpProjectInfo);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftPanel.Location = new System.Drawing.Point(0, 0);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Padding = new System.Windows.Forms.Padding(5);
            this.leftPanel.Size = new System.Drawing.Size(250, 542);
            this.leftPanel.TabIndex = 0;
            // 
            // bufferFlowPanel
            // 
            this.bufferFlowPanel.AutoScroll = true;
            this.bufferFlowPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bufferFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferFlowPanel.Location = new System.Drawing.Point(5, 222);
            this.bufferFlowPanel.Name = "bufferFlowPanel";
            this.bufferFlowPanel.Size = new System.Drawing.Size(240, 267);
            this.bufferFlowPanel.TabIndex = 3;
            // 
            // grpProjectInfo
            // 
            this.grpProjectInfo.Controls.Add(this.panelProjectInfo);
            this.grpProjectInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpProjectInfo.Location = new System.Drawing.Point(5, 5);
            this.grpProjectInfo.Name = "grpProjectInfo";
            this.grpProjectInfo.Size = new System.Drawing.Size(240, 168);
            this.grpProjectInfo.TabIndex = 1;
            this.grpProjectInfo.TabStop = false;
            this.grpProjectInfo.Text = "Current Project";
            // 
            // lblBuffer
            // 
            this.lblBuffer.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBuffer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBuffer.Location = new System.Drawing.Point(5, 197);
            this.lblBuffer.Name = "lblBuffer";
            this.lblBuffer.Size = new System.Drawing.Size(240, 25);
            this.lblBuffer.TabIndex = 2;
            this.lblBuffer.Text = "Buffer Zone (Selected Icons)";
            this.lblBuffer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRemoveFromBuffer
            // 
            this.btnRemoveFromBuffer.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnRemoveFromBuffer.Location = new System.Drawing.Point(5, 173);
            this.btnRemoveFromBuffer.Name = "btnRemoveFromBuffer";
            this.btnRemoveFromBuffer.Size = new System.Drawing.Size(240, 24);
            this.btnRemoveFromBuffer.TabIndex = 6;
            this.btnRemoveFromBuffer.Text = "Remove (Double-Click)";
            this.btnRemoveFromBuffer.UseVisualStyleBackColor = true;
            this.btnRemoveFromBuffer.Click += new System.EventHandler(this.btnRemoveFromBuffer_Click);
            // 
            // btnMerge
            // 
            this.btnMerge.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnMerge.Location = new System.Drawing.Point(5, 489);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(240, 24);
            this.btnMerge.TabIndex = 4;
            this.btnMerge.Text = "Merge (Select 2)";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
            // 
            // btnExportToProject
            // 
            this.btnExportToProject.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnExportToProject.Location = new System.Drawing.Point(5, 513);
            this.btnExportToProject.Name = "btnExportToProject";
            this.btnExportToProject.Size = new System.Drawing.Size(240, 24);
            this.btnExportToProject.TabIndex = 5;
            this.btnExportToProject.Text = "Export to Project";
            this.btnExportToProject.UseVisualStyleBackColor = true;
            this.btnExportToProject.Click += new System.EventHandler(this.btnExportToProject_Click);
            // 
            // centerRightSplitContainer
            // 
            this.centerRightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.centerRightSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.centerRightSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.centerRightSplitContainer.Name = "centerRightSplitContainer";
            // 
            // centerRightSplitContainer.Panel1
            // 
            this.centerRightSplitContainer.Panel1.Controls.Add(this.centerPanel);
            // 
            // centerRightSplitContainer.Panel2
            // 
            this.centerRightSplitContainer.Panel2.Controls.Add(this.rightPanel);
            this.centerRightSplitContainer.Panel2MinSize = 200;
            this.centerRightSplitContainer.Size = new System.Drawing.Size(1292, 542);
            this.centerRightSplitContainer.SplitterDistance = 1062;
            this.centerRightSplitContainer.TabIndex = 0;
            // 
            // centerPanel
            // 
            this.centerPanel.Controls.Add(this.iconsFlowPanel);
            this.centerPanel.Controls.Add(this.topCenterPanel);
            this.centerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.centerPanel.Location = new System.Drawing.Point(0, 0);
            this.centerPanel.Name = "centerPanel";
            this.centerPanel.Size = new System.Drawing.Size(1062, 542);
            this.centerPanel.TabIndex = 0;
            // 
            // iconsFlowPanel
            // 
            this.iconsFlowPanel.AutoScroll = true;
            this.iconsFlowPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iconsFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iconsFlowPanel.Location = new System.Drawing.Point(0, 54);
            this.iconsFlowPanel.Name = "iconsFlowPanel";
            this.iconsFlowPanel.Padding = new System.Windows.Forms.Padding(5);
            this.iconsFlowPanel.Size = new System.Drawing.Size(1062, 488);
            this.iconsFlowPanel.TabIndex = 1;
            // 
            // topCenterPanel
            // 
            this.topCenterPanel.Controls.Add(this.labTags);
            this.topCenterPanel.Controls.Add(this.labIconName);
            this.topCenterPanel.Controls.Add(this.btnNextPage);
            this.topCenterPanel.Controls.Add(this.lblPageInfo);
            this.topCenterPanel.Controls.Add(this.btnPrevPage);
            this.topCenterPanel.Controls.Add(this.cmbPageSize);
            this.topCenterPanel.Controls.Add(this.lblPageSize);
            this.topCenterPanel.Controls.Add(this.lblTotalResults);
            this.topCenterPanel.Controls.Add(this.cmbIconSize);
            this.topCenterPanel.Controls.Add(this.lblIconSize);
            this.topCenterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topCenterPanel.Location = new System.Drawing.Point(0, 0);
            this.topCenterPanel.Name = "topCenterPanel";
            this.topCenterPanel.Padding = new System.Windows.Forms.Padding(5);
            this.topCenterPanel.Size = new System.Drawing.Size(1062, 54);
            this.topCenterPanel.TabIndex = 0;
            // 
            // labTags
            // 
            this.labTags.Location = new System.Drawing.Point(1, 35);
            this.labTags.Name = "labTags";
            this.labTags.Size = new System.Drawing.Size(1019, 18);
            this.labTags.TabIndex = 9;
            this.labTags.Text = "Tags:";
            // 
            // labIconName
            // 
            this.labIconName.AutoSize = true;
            this.labIconName.Location = new System.Drawing.Point(766, 13);
            this.labIconName.Name = "labIconName";
            this.labIconName.Size = new System.Drawing.Size(65, 13);
            this.labIconName.TabIndex = 8;
            this.labIconName.Text = "Icon Name: ";
            // 
            // btnNextPage
            // 
            this.btnNextPage.Location = new System.Drawing.Point(510, 8);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 23);
            this.btnNextPage.TabIndex = 5;
            this.btnNextPage.Text = "Next >";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Location = new System.Drawing.Point(415, 13);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(62, 13);
            this.lblPageInfo.TabIndex = 4;
            this.lblPageInfo.Text = "Page 1 of 1";
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.Location = new System.Drawing.Point(330, 8);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(75, 23);
            this.btnPrevPage.TabIndex = 3;
            this.btnPrevPage.Text = "< Previous";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            // 
            // cmbPageSize
            // 
            this.cmbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPageSize.FormattingEnabled = true;
            this.cmbPageSize.Items.AddRange(new object[] {
            "25",
            "50",
            "100",
            "200",
            "500"});
            this.cmbPageSize.Location = new System.Drawing.Point(685, 10);
            this.cmbPageSize.Name = "cmbPageSize";
            this.cmbPageSize.Size = new System.Drawing.Size(75, 21);
            this.cmbPageSize.TabIndex = 7;
            this.cmbPageSize.SelectedIndexChanged += new System.EventHandler(this.cmbPageSize_SelectedIndexChanged);
            // 
            // lblPageSize
            // 
            this.lblPageSize.AutoSize = true;
            this.lblPageSize.Location = new System.Drawing.Point(600, 13);
            this.lblPageSize.Name = "lblPageSize";
            this.lblPageSize.Size = new System.Drawing.Size(75, 13);
            this.lblPageSize.TabIndex = 6;
            this.lblPageSize.Text = "Results/Page:";
            // 
            // lblTotalResults
            // 
            this.lblTotalResults.AutoSize = true;
            this.lblTotalResults.Location = new System.Drawing.Point(210, 13);
            this.lblTotalResults.Name = "lblTotalResults";
            this.lblTotalResults.Size = new System.Drawing.Size(71, 13);
            this.lblTotalResults.TabIndex = 2;
            this.lblTotalResults.Text = "Total: 0 icons";
            // 
            // cmbIconSize
            // 
            this.cmbIconSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIconSize.FormattingEnabled = true;
            this.cmbIconSize.Items.AddRange(new object[] {
            "16px",
            "32px",
            "48px",
            "64px",
            "128px",
            "256px"});
            this.cmbIconSize.Location = new System.Drawing.Point(95, 10);
            this.cmbIconSize.Name = "cmbIconSize";
            this.cmbIconSize.Size = new System.Drawing.Size(100, 21);
            this.cmbIconSize.TabIndex = 1;
            this.cmbIconSize.SelectedIndexChanged += new System.EventHandler(this.cmbIconSize_SelectedIndexChanged);
            // 
            // lblIconSize
            // 
            this.lblIconSize.AutoSize = true;
            this.lblIconSize.Location = new System.Drawing.Point(8, 13);
            this.lblIconSize.Name = "lblIconSize";
            this.lblIconSize.Size = new System.Drawing.Size(67, 13);
            this.lblIconSize.TabIndex = 0;
            this.lblIconSize.Text = "Display Size:";
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.btnMoveToBuffer);
            this.rightPanel.Controls.Add(this.iconDetailsFlowPanel);
            this.rightPanel.Controls.Add(this.lblIconDetails);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(0, 0);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new System.Windows.Forms.Padding(5);
            this.rightPanel.Size = new System.Drawing.Size(226, 542);
            this.rightPanel.TabIndex = 0;
            // 
            // btnMoveToBuffer
            // 
            this.btnMoveToBuffer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnMoveToBuffer.Location = new System.Drawing.Point(5, 513);
            this.btnMoveToBuffer.Name = "btnMoveToBuffer";
            this.btnMoveToBuffer.Size = new System.Drawing.Size(216, 24);
            this.btnMoveToBuffer.TabIndex = 2;
            this.btnMoveToBuffer.Text = "Move to Buffer";
            this.btnMoveToBuffer.UseVisualStyleBackColor = true;
            this.btnMoveToBuffer.Click += new System.EventHandler(this.btnMoveToBuffer_Click);
            // 
            // iconDetailsFlowPanel
            // 
            this.iconDetailsFlowPanel.AutoScroll = true;
            this.iconDetailsFlowPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iconDetailsFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iconDetailsFlowPanel.Location = new System.Drawing.Point(5, 30);
            this.iconDetailsFlowPanel.Name = "iconDetailsFlowPanel";
            this.iconDetailsFlowPanel.Size = new System.Drawing.Size(216, 507);
            this.iconDetailsFlowPanel.TabIndex = 1;
            // 
            // lblIconDetails
            // 
            this.lblIconDetails.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblIconDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIconDetails.Location = new System.Drawing.Point(5, 5);
            this.lblIconDetails.Name = "lblIconDetails";
            this.lblIconDetails.Size = new System.Drawing.Size(216, 25);
            this.lblIconDetails.TabIndex = 0;
            this.lblIconDetails.Text = "Icon Details";
            this.lblIconDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelProjectInfo
            // 
            this.panelProjectInfo.AutoScroll = true;
            this.panelProjectInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelProjectInfo.Location = new System.Drawing.Point(3, 16);
            this.panelProjectInfo.Name = "panelProjectInfo";
            this.panelProjectInfo.Size = new System.Drawing.Size(234, 149);
            this.panelProjectInfo.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1546, 690);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.topFilterPanel);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Icon Commander";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.topFilterPanel.ResumeLayout(false);
            this.topFilterPanel.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.grpProjectInfo.ResumeLayout(false);
            this.centerRightSplitContainer.Panel1.ResumeLayout(false);
            this.centerRightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.centerRightSplitContainer)).EndInit();
            this.centerRightSplitContainer.ResumeLayout(false);
            this.centerPanel.ResumeLayout(false);
            this.topCenterPanel.ResumeLayout(false);
            this.topCenterPanel.PerformLayout();
            this.rightPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel projectStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel databaseStatusLabel;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataBaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDataBaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem themeToolStripMenuItem;
        private ZidUtilities.CommonCode.Win.Controls.ThemeManager themeManager1;
        private System.Windows.Forms.ToolStripMenuItem iconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collectionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem veinsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem iconsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem importIconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importVeinToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem sqliteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mSSQLServerToolStripMenuItem;
        private System.Windows.Forms.Panel topFilterPanel;
        private System.Windows.Forms.Label lblTypes;
        private System.Windows.Forms.CheckedListBox chkListTypes;
        private ZidUtilities.CommonCode.Win.Controls.TokenSelect tokenSelectCollections;
        private System.Windows.Forms.Label lblCollections;
        private ZidUtilities.CommonCode.Win.Controls.TokenSelect tokenSelectVeins;
        private System.Windows.Forms.Label lblVeins;
        private ZidUtilities.CommonCode.Win.Controls.TokenSelect tokenSelectTags;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Button btnApplyFilter;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.GroupBox grpProjectInfo;
        private System.Windows.Forms.Label lblBuffer;
        private System.Windows.Forms.FlowLayoutPanel bufferFlowPanel;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.Button btnExportToProject;
        private System.Windows.Forms.Button btnRemoveFromBuffer;
        private System.Windows.Forms.SplitContainer centerRightSplitContainer;
        private System.Windows.Forms.Panel centerPanel;
        private System.Windows.Forms.Panel topCenterPanel;
        private System.Windows.Forms.ComboBox cmbIconSize;
        private System.Windows.Forms.Label lblIconSize;
        private System.Windows.Forms.Label lblTotalResults;
        private System.Windows.Forms.Label lblPageSize;
        private System.Windows.Forms.ComboBox cmbPageSize;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.FlowLayoutPanel iconsFlowPanel;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Label lblIconDetails;
        private System.Windows.Forms.FlowLayoutPanel iconDetailsFlowPanel;
        private System.Windows.Forms.Button btnMoveToBuffer;
        private System.Windows.Forms.Label labIconName;
        private System.Windows.Forms.Label labTags;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panelProjectInfo;
    }
}

