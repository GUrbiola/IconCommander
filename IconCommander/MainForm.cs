using IconCommander.Controls;
using IconCommander.DataAccess;
using IconCommander.Forms;
using IconCommander.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZidUtilities.CommonCode;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.CRUD;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander
{
    public partial class MainForm : Form
    {
        private string databaseConnectionString;
        private string projectPath;
        private string projectName;
        private IIconCommanderDb conx;
        private int currentIconSize = 64; // Default display size
        private List<IconDisplayControl> bufferIcons = new List<IconDisplayControl>();
        private Dictionary<int, int> bufferZoneIds = new Dictionary<int, int>(); // IconFileId -> BufferZoneId mapping
        private IconDisplayControl selectedIconForDetails;

        // Pagination variables
        private DataTable allFilteredIcons;
        private int currentPage = 1;
        private int pageSize = 100;
        private int totalPages = 1;
        private int totalResults = 0;

        public Project SelectedProject { get; set; }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Load saved theme preference
            string savedTheme = Properties.Settings.Default.SelectedTheme;
            ZidThemes selectedTheme = ZidThemes.None;
            if (Enum.TryParse<ZidThemes>(savedTheme, out selectedTheme))
                themeManager1.Theme = selectedTheme;
            else
                themeManager1.Theme = ZidThemes.None;
            themeManager1.ApplyTheme();

            Dictionary<string, ZidThemes> themes = new Dictionary<string, ZidThemes>();
            foreach (var theme in Enum.GetValues(typeof(ZidThemes)))
                themes.Add(theme.ToString(), (ZidThemes)theme);

            themes.OrderBy(t => t.Key);
            foreach (var theme in themes.OrderBy(t => t.Key))
            {
                ToolStripMenuItem menuOption = new System.Windows.Forms.ToolStripMenuItem();
                menuOption.Name = $"ZidTheme_{theme.Key}";
                menuOption.Text = theme.Key;
                menuOption.Tag = theme.Value;
                if (theme.Key == savedTheme)
                    menuOption.Checked = true;

                menuOption.Click += MenuOption_Click;

                themeToolStripMenuItem.DropDownItems.Add(menuOption);
            }

            // Set default icon size
            cmbIconSize.SelectedIndex = 3; // 64px

            // Set default page size
            cmbPageSize.SelectedIndex = 2; // 100

            // Position filter controls
            PositionFilterControls();

            if (Properties.Settings.Default.IsSqlite)
            {
                if (!Properties.Settings.Default.ConnectionString.IsEmpty())
                {
                    LoadDbConnection(Properties.Settings.Default.ConnectionString, true);
                }
            }
            else
            {
                if (!Properties.Settings.Default.ConnectionString.IsEmpty())
                {
                    LoadDbConnection(Properties.Settings.Default.ConnectionString, false);
                }
            }
        }

        private void LoadDbConnection(string connectionString, bool IsSqlite)
        {
            databaseConnectionString = connectionString;
            if (IsSqlite)
            {
                conx = new SqliteConnector(connectionString);
                if (!conx.TestConnection())
                {
                    MessageBoxDialog.Show($"Error while trying to open data base: {conx.LastMessage}", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                }
                else
                {
                    Properties.Settings.Default.ConnectionString = conx.ConnectionString;
                    Properties.Settings.Default.IsSqlite = true;
                    Properties.Settings.Default.Save();

                    if (conx.ValidateSchema())
                    {
                        UpdateStatusBar(conx.DataBase);
                        LoadFilters();

                        // Create BufferZone table if it doesn't exist
                        bool tableCreated = conx.CreateBufferZoneTable();
                        if (!tableCreated)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to create BufferZone table: {conx.LastMessage}");
                            toolStripStatusLabel.Text = "Warning: BufferZone table creation failed";
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("BufferZone table created or already exists");
                        }

                        // Load buffer zone icons
                        LoadBufferZone();
                    }
                    else
                    {
                        MessageBoxDialog.Show($"Database schema is not what was expected: {conx.LastMessage}", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                    }
                }
            }
            else
            {
                conx = new SqlConnector(connectionString);
                if (!conx.TestConnection())
                {
                    MessageBoxDialog.Show($"Error while trying to open data base: {conx.LastMessage}", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                }
                else
                {
                    Properties.Settings.Default.ConnectionString = conx.ConnectionString;
                    Properties.Settings.Default.IsSqlite = false;
                    Properties.Settings.Default.Save();

                    if (conx.ValidateSchema())
                    {
                        UpdateStatusBar(conx.DataBase);
                        LoadFilters();

                        // Create BufferZone table if it doesn't exist
                        bool tableCreated = conx.CreateBufferZoneTable();
                        if (!tableCreated)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to create BufferZone table: {conx.LastMessage}");
                            toolStripStatusLabel.Text = "Warning: BufferZone table creation failed";
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("BufferZone table created or already exists");
                        }

                        // Load buffer zone icons
                        LoadBufferZone();
                    }
                    else
                    {
                        MessageBoxDialog.Show($"Database schema is not what was expected: {conx.LastMessage}", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                    }
                }
            }
        }

        private void LoadFilters()
        {
            if (conx == null)
                return;

            // Load Collections
            var collectionsResponse = conx.ExecuteTable("SELECT Id, Name FROM Collections ORDER BY Name");
            if (collectionsResponse.IsOK)
            {
                var collectionsDict = new Dictionary<string, object>();
                foreach (DataRow row in collectionsResponse.Result.Rows)
                {
                    collectionsDict.Add(row["Name"].ToString(), row["Id"]);
                }
                tokenSelectCollections.SetDataSource(collectionsDict);
            }

            // Load Veins
            var veinsResponse = conx.ExecuteTable("SELECT Id, Name FROM Veins ORDER BY Name");
            if (veinsResponse.IsOK)
            {
                var veinsDict = new Dictionary<string, object>();
                foreach (DataRow row in veinsResponse.Result.Rows)
                {
                    veinsDict.Add(row["Name"].ToString(), row["Id"]);
                }
                tokenSelectVeins.SetDataSource(veinsDict);
            }

            // Load Tags (distinct tags from IconTags)
            var tagsResponse = conx.ExecuteTable("SELECT DISTINCT Tag FROM IconTags ORDER BY Tag");
            if (tagsResponse.IsOK)
            {
                var tagsDict = new Dictionary<string, object>();
                foreach (DataRow row in tagsResponse.Result.Rows)
                {
                    string tag = row["Tag"].ToString();
                    tagsDict.Add(tag, tag);
                }
                tokenSelectTags.SetDataSource(tagsDict);
            }

            // Check all types by default
            for (int i = 0; i < chkListTypes.Items.Count; i++)
            {
                chkListTypes.SetItemChecked(i, true);
            }
        }

        private void MenuOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (sender != null)
            {
                ZidThemes theme = (ZidThemes)menuItem.Tag;
                if (themeManager1 != null)
                {
                    themeManager1.Theme = theme;
                    Properties.Settings.Default.SelectedTheme = theme.ToString();
                    Properties.Settings.Default.Save();
                    themeManager1.ApplyTheme();
                }

                // Uncheck all other menu items
                foreach (ToolStripMenuItem item in themeToolStripMenuItem.DropDownItems)
                    item.Checked = false;

                menuItem.Checked = true;
            }
        }

        private void UpdateStatusBar(string dataBase)
        {
            if (!string.IsNullOrEmpty(projectPath))
            {
                projectStatusLabel.Text = $"Project: {SelectedProject.Name} ({SelectedProject.Path})";
            }
            else
            {
                projectStatusLabel.Text = "No project";
            }

            if (!databaseConnectionString.IsEmpty())
            {
                if (Properties.Settings.Default.IsSqlite)
                    databaseStatusLabel.Text = $"Sqlite: {dataBase}";
                else
                    databaseStatusLabel.Text = $"MS SQL: {dataBase}";
            }
            else
            {
                databaseStatusLabel.Text = "No database";
            }
        }

        private void createProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Path", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("ResourceFile", typeof(string));
            table.Columns.Add("ResourceFolder", typeof(string));
            table.Columns.Add("SaveIconsTo", typeof(string));
            table.Columns.Add("ProjectFile", typeof(string));
            table.Columns.Add("UpdateProjectFile", typeof(int));

            UIGenerator formGen = new UIGenerator(table, "Projects");
            formGen.Theme = themeManager1.Theme;

            //Set Primary Keys
            formGen.ClearAllPrimaryKeys();
            formGen.SetPrimaryKey("Id");

            //Set Exclusions
            formGen.SetExclusion("Id");

            //Set Aliases
            formGen.SetAlias("Path", "Project's Root Folder");
            formGen.SetAlias("Type", "Type of Project");
            formGen.SetAlias("ResourceFile", "Resource File (.resx)");
            formGen.SetAlias("ResourceFolder", "Resource Folder");
            formGen.SetAlias("SaveIconsTo", "Export Icons To");
            formGen.SetAlias("ProjectFile", "Set Project File");
            formGen.SetAlias("UpdateProjectFile", "Update Project File on Icon Export");

            //Set Required Fields
            formGen.SetRequired("Name");
            formGen.SetRequired("Path");
            formGen.SetRequired("Type");
            formGen.SetRequired("SaveIconsTo");
            formGen.SetRequired("ProjectFile");
            formGen.SetRequired("UpdateProjectFile");

            //Set Field Formatting
            formGen.SetFieldFormat("Path", FieldFormat.Folder);

            FormatConfig typeConfig = new FormatConfig();
            typeConfig.ListItems = new Dictionary<string, object>();
            typeConfig.ListItems.Add("Web", "Web");
            typeConfig.ListItems.Add("Windows", "Windows");
            formGen.SetFieldFormat("Type", FieldFormat.List, typeConfig);

            FormatConfig rfConfig = new FormatConfig();
            rfConfig.FileFilter = "Resource Files (*.resx)|*.resx|All Files (*.*)|*.*";
            formGen.SetFieldFormat("ResourceFile", FieldFormat.File, rfConfig);

            formGen.SetFieldFormat("ResourceFolder", FieldFormat.Folder);

            FormatConfig saveIconsToConfig = new FormatConfig();
            saveIconsToConfig.ListItems = new Dictionary<string, object>();
            saveIconsToConfig.ListItems.Add("Folder", "Folder");
            saveIconsToConfig.ListItems.Add("Resource File", "File");
            saveIconsToConfig.ListItems.Add("Both", "Both");
            formGen.SetFieldFormat("SaveIconsTo", FieldFormat.List, saveIconsToConfig);

            FormatConfig projectFileConfig = new FormatConfig();
            projectFileConfig.FileFilter = "CSharp Project Files (*.csproj)|*.csproj|All Files (*.*)|*.*";
            formGen.SetFieldFormat("ProjectFile", FieldFormat.File, projectFileConfig);

            formGen.SetFieldFormat("UpdateProjectFile", FieldFormat.Check);

            formGen.FormWidth = 950;

            var result = formGen.ShowInsertDialog();

            if (result != null)
            {
                string sql = Properties.Settings.Default.IsSqlite ? result.SqliteScript : result.SqlServerScript;
                conx.ExecuteNonQuery(sql);
                if (conx.Error)
                {
                    MessageBoxDialog.Show($"Failed to create new project: {conx.Error}", "Projects", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                }
                else
                {
                    MessageBoxDialog.Show("Project created properly!", "Projects", MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);
                    projectPath = result["Path"].ToString();
                    projectName = result["Name"].ToString();

                    UpdateStatusBar(conx.DataBase);
                }
            }
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            SqlResponse<DataTable> response = conx.ExecuteTable("SELECT * FROM Projects");

            if (response.IsOK)
            {
                SingleSelectionDialog dialog = new SingleSelectionDialog();
                dialog.DialogTitle = "Select a Project";
                dialog.Message = "List of projects registered in the application";
                dialog.Required = true;
                dialog.Theme = themeManager1.Theme;
                dialog.SetDataSource(response.Result, "Name", "Id");

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string sql = "SELECT * FROM Projects WHERE Id = @Id@";
                    sql = sql.Replace("@Id@", dialog.SelectedValue.ToString());
                    SqlResponse<DataTable> response2 = conx.ExecuteTable(sql);
                    if (response2.IsOK)
                    {
                        DataTable res = response2.Result;
                        SelectedProject = new Project();
                        SelectedProject.Id = int.Parse(res.Rows[0]["Id"].ToString());
                        SelectedProject.Name = res.Rows[0]["Name"].ToString();
                        SelectedProject.Type = res.Rows[0]["Type"].ToString();
                        SelectedProject.Path = res.Rows[0]["Path"].ToString();
                        SelectedProject.ResourceFile = res.Rows[0]["ResourceFile"].ToString();
                        SelectedProject.ResourceFolder = res.Rows[0]["ResourceFolder"].ToString();
                        SelectedProject.SaveIconsTo = res.Rows[0]["SaveIconsTo"].ToString();
                        SelectedProject.ProjectFile = res.Rows[0]["ProjectFile"].ToString();
                        SelectedProject.UpdateProjectFile = int.Parse(res.Rows[0]["UpdateProjectFile"].ToString());

                        //MessageBoxDialog.Show("Project opened properly!", "Projects", MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);
                        projectPath = SelectedProject.Path;
                        projectName = SelectedProject.Name;

                        UpdateStatusBar(conx.DataBase);
                        UpdateProjectInfo();

                        // Reload buffer zone for this project
                        LoadBufferZone();
                    }
                    else
                    {
                        string errorMessage = response2.Errors[0].Exception != null ? response2.Errors[0].Exception.Message : response2.Errors[0].Message;
                        MessageBoxDialog.Show($"Error while selecting the project from DB: {errorMessage}", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                        return;
                    }
                }
            }
            else
            {
                string errorMessage = response.Errors[0].Exception != null ? response.Errors[0].Exception.Message : response.Errors[0].Message;
                MessageBoxDialog.Show($"Error while trying to obtain projects from DB: {errorMessage}", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }
        }

        private void editProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            if (SelectedProject == null)
            {
                MessageBoxDialog.Show("No project is currently selected! Please open a project first.", "Edit Project", MessageBoxButtons.OK, MessageBoxIcon.Warning, themeManager1.Theme);
                return;
            }

            // Load current project data from database
            string sql = "SELECT * FROM Projects WHERE Id = @Id@";
            sql = sql.Replace("@Id@", SelectedProject.Id.ToString());
            SqlResponse<DataTable> response = conx.ExecuteTable(sql);

            if (!response.IsOK)
            {
                string errorMessage = response.Errors[0].Exception != null ? response.Errors[0].Exception.Message : response.Errors[0].Message;
                MessageBoxDialog.Show($"Error loading project data: {errorMessage}", "Edit Project", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            if (response.Result.Rows.Count == 0)
            {
                MessageBoxDialog.Show("Project not found in database!", "Edit Project", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            DataRow projectRow = response.Result.Rows[0];

            // Create schema for UIGenerator
            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Path", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("ResourceFile", typeof(string));
            table.Columns.Add("ResourceFolder", typeof(string));
            table.Columns.Add("SaveIconsTo", typeof(string));
            table.Columns.Add("ProjectFile", typeof(string));
            table.Columns.Add("UpdateProjectFile", typeof(int));

            UIGenerator formGen = new UIGenerator(table, "Projects");
            formGen.Theme = themeManager1.Theme;

            //Set Primary Keys
            formGen.ClearAllPrimaryKeys();
            formGen.SetPrimaryKey("Id");

            //Set Exclusions
            formGen.SetExclusion("Id");

            //Set Aliases
            formGen.SetAlias("Path", "Project's Root Folder");
            formGen.SetAlias("Type", "Type of Project");
            formGen.SetAlias("ResourceFile", "Resource File (.resx)");
            formGen.SetAlias("ResourceFolder", "Resource Folder");
            formGen.SetAlias("SaveIconsTo", "Export Icons To");
            formGen.SetAlias("ProjectFile", "Set Project File");
            formGen.SetAlias("UpdateProjectFile", "Update Project File on Icon Export");

            //Set Required Fields
            formGen.SetRequired("Name");
            formGen.SetRequired("Path");
            formGen.SetRequired("Type");
            formGen.SetRequired("SaveIconsTo");
            formGen.SetRequired("ProjectFile");
            formGen.SetRequired("UpdateProjectFile");

            //Set Field Formatting
            formGen.SetFieldFormat("Path", FieldFormat.Folder);

            FormatConfig typeConfig = new FormatConfig();
            typeConfig.ListItems = new Dictionary<string, object>();
            typeConfig.ListItems.Add("Web", "Web");
            typeConfig.ListItems.Add("Windows", "Windows");
            formGen.SetFieldFormat("Type", FieldFormat.List, typeConfig);

            FormatConfig rfConfig = new FormatConfig();
            rfConfig.FileFilter = "Resource Files (*.resx)|*.resx|All Files (*.*)|*.*";
            formGen.SetFieldFormat("ResourceFile", FieldFormat.File, rfConfig);

            formGen.SetFieldFormat("ResourceFolder", FieldFormat.Folder);

            FormatConfig saveIconsToConfig = new FormatConfig();
            saveIconsToConfig.ListItems = new Dictionary<string, object>();
            saveIconsToConfig.ListItems.Add("Folder", "Folder");
            saveIconsToConfig.ListItems.Add("Resource File", "File");
            saveIconsToConfig.ListItems.Add("Both", "Both");
            formGen.SetFieldFormat("SaveIconsTo", FieldFormat.List, saveIconsToConfig);

            FormatConfig projectFileConfig = new FormatConfig();
            projectFileConfig.FileFilter = "CSharp Project Files (*.csproj)|*.csproj|All Files (*.*)|*.*";
            formGen.SetFieldFormat("ProjectFile", FieldFormat.File, projectFileConfig);

            formGen.SetFieldFormat("UpdateProjectFile", FieldFormat.Check);

            formGen.FormWidth = 950;

            // Show update dialog with current data
            var result = formGen.ShowUpdateDialog(projectRow);

            if (result != null)
            {
                string updateSql = Properties.Settings.Default.IsSqlite ? result.SqliteScript : result.SqlServerScript;
                conx.ExecuteNonQuery(updateSql);
                if (conx.Error)
                {
                    MessageBoxDialog.Show($"Failed to update project: {conx.Error}", "Projects", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                }
                else
                {
                    MessageBoxDialog.Show("Project updated successfully!", "Projects", MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);

                    // Reload the selected project to reflect changes
                    SelectedProject.Name = result["Name"].ToString();
                    SelectedProject.Type = result["Type"].ToString();
                    SelectedProject.Path = result["Path"].ToString();
                    SelectedProject.ResourceFile = result["ResourceFile"].ToString();
                    SelectedProject.ResourceFolder = result["ResourceFolder"].ToString();
                    SelectedProject.SaveIconsTo = result["SaveIconsTo"].ToString();
                    SelectedProject.ProjectFile = result["ProjectFile"].ToString();
                    SelectedProject.UpdateProjectFile = int.Parse(result["UpdateProjectFile"].ToString());

                    projectPath = SelectedProject.Path;
                    projectName = SelectedProject.Name;

                    UpdateStatusBar(conx.DataBase);
                    UpdateProjectInfo();
                }
            }
        }

        private void UpdateProjectInfo()
        {
            panelProjectInfo.Controls.Clear();

            if (SelectedProject == null)
            {
                panelProjectInfo.Controls.Add(CreateLabel(Text: "No project selected", Bold: true, ToolTip: "", MaxLength: 0));
                return;
            }
            else
            {
                panelProjectInfo.Controls.Add(CreateLabel(Text: SelectedProject.SaveIconsTo, Bold: true, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: "Export To:", Bold: false, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: SelectedProject.ResourceFolder, Bold: true, ToolTip: SelectedProject.ResourceFolder, MaxLength: 30));
                panelProjectInfo.Controls.Add(CreateLabel(Text: "Resource Folder:", Bold: false, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: SelectedProject.ResourceFile, Bold: true, ToolTip: SelectedProject.ResourceFile, MaxLength: 30));
                panelProjectInfo.Controls.Add(CreateLabel(Text: "Resource File:", Bold: false, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: SelectedProject.Path, Bold: true, ToolTip: SelectedProject.Path, MaxLength: 30));
                panelProjectInfo.Controls.Add(CreateLabel(Text: "Path:", Bold: false, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: SelectedProject.Type, Bold: true, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: "Type:", Bold: false, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: SelectedProject.Name, Bold: true, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: "Name:", Bold: false, ToolTip: "", MaxLength: 0));
                panelProjectInfo.Controls.Add(CreateLabel(Text: SelectedProject.ProjectFile, Bold: true, ToolTip: SelectedProject.ProjectFile, MaxLength: 30));
                panelProjectInfo.Controls.Add(CreateLabel(Text: "Project File:", Bold: false, ToolTip: "", MaxLength: 0));
            }
        }

        private System.Windows.Forms.Control CreateLabel(string Text, bool Bold, string ToolTip, int MaxLength)
        {
            Label back = new Label();
            if(MaxLength > 0)
            {
                if (Text.Length > MaxLength)
                {
                    back.Text = "..." + Text.Substring(Text.Length - (MaxLength - 3));
                }
            }
            else
            {
                back.Text = Text;
            }
            if(Bold)
                back.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            else
                back.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            
            if(!ToolTip.IsEmpty())
                this.toolTip1.SetToolTip(back, ToolTip);

            back.Dock = DockStyle.Top;
            back.AutoSize = false;
            back.Height = 15;

            themeManager1.ApplyThemeTo(back);
            return back;
        }

        private void importIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            IconImport dialog = new IconImport(databaseConnectionString, themeManager1.Theme);
            dialog.ShowDialog();
        }

        private void manageIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            IconsForm iconsForm = new IconsForm(databaseConnectionString, themeManager1.Theme);
            iconsForm.ShowDialog();
        }

        private void bufferToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            IconBufferForm bufferForm = new IconBufferForm(databaseConnectionString, themeManager1.Theme, SelectedProject);
            bufferForm.ShowDialog();
        }

        private void manageCollectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            CollectionsForm collectionsForm = new CollectionsForm(databaseConnectionString, themeManager1.Theme);
            if (collectionsForm.ShowDialog() == DialogResult.OK)
            {
                LoadFilters(); // Reload filters after changes
            }
        }

        private void manageVeinsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            VeinsForm veinsForm = new VeinsForm(databaseConnectionString, themeManager1.Theme);
            if (veinsForm.ShowDialog() == DialogResult.OK)
            {
                LoadFilters(); // Reload filters after changes
            }
        }

        private void importVeinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (databaseConnectionString.IsEmpty())
            {
                MessageBoxDialog.Show("There is no database connection defined!", "Database Connection", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            VeinImport dialog = new VeinImport(databaseConnectionString, themeManager1.Theme);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadFilters(); // Reload filters after import
            }
        }

        private void sqliteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "SQLite Database (*.db)|*.db|SQLite Database (*.sqlite)|*.sqlite|All Files (*.*)|*.*";
                openDialog.Title = "Open SQLite Database";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    SQLiteConnectionStringBuilder conString = new SQLiteConnectionStringBuilder();
                    conString.DataSource = openDialog.FileName;
                    conString.DefaultTimeout = 5000;
                    conString.SyncMode = SynchronizationModes.Off;
                    conString.JournalMode = SQLiteJournalModeEnum.Memory;
                    conString.ReadOnly = false;
                    LoadDbConnection(connectionString: conString.ConnectionString, IsSqlite: true);
                }
            }
        }

        private void mSSQLServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlConnectForm connect = new SqlConnectForm();
            if (connect.ShowDialog() == DialogResult.OK)
            {
                LoadDbConnection(connectionString: connect.ConnectionString, IsSqlite: false);
            }
        }

        // ========================== FILTER AND DISPLAY LOGIC ==========================

        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            if (conx == null)
            {
                MessageBoxDialog.Show("No database connection!", "Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning, themeManager1.Theme);
                return;
            }

            Cursor = Cursors.WaitCursor;
            toolStripStatusLabel.Text = "Applying filter...";

            try
            {
                // Build SQL query based on filters
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT DISTINCT i.Id, i.Name, i.Vein");
                sql.AppendLine("FROM Icons i");
                sql.AppendLine("INNER JOIN IconFiles f ON i.Id = f.Icon");

                bool hasWhereClause = false;

                // Type filter - only apply if not all are selected
                List<string> types = new List<string>();
                bool imageChecked = chkListTypes.GetItemChecked(0);
                bool iconChecked = chkListTypes.GetItemChecked(1);
                bool svgChecked = chkListTypes.GetItemChecked(2);

                // Only apply filter if at least one is selected and not all are selected
                if ((imageChecked || iconChecked || svgChecked) && !(imageChecked && iconChecked && svgChecked))
                {
                    if (imageChecked)
                    {
                        // Add all common image MIME types
                        types.Add("'image/png'");
                        types.Add("'image/jpeg'");
                        types.Add("'image/jpg'");
                        types.Add("'image/gif'");
                        types.Add("'image/bmp'");
                        types.Add("'image/tiff'");
                        types.Add("'image/tif'");
                        types.Add("'image/webp'");
                        types.Add("'image/x-icon'");
                        types.Add("'Image'");  // Legacy format
                        types.Add("'*'");      // Catch-all
                    }

                    if (iconChecked)
                    {
                        types.Add("'Icon'");
                        types.Add("'image/x-icon'");
                        types.Add("'image/vnd.microsoft.icon'");
                    }

                    if (svgChecked)
                    {
                        types.Add("'SVG'");
                        types.Add("'image/svg+xml'");
                    }

                    sql.AppendLine($"WHERE f.Type IN ({string.Join(", ", types)})");
                    hasWhereClause = true;
                }

                // Collections filter
                var selectedCollections = tokenSelectCollections.SelectedValues;
                if (selectedCollections.Any())
                {
                    string collectionsFilter = string.Join(", ", selectedCollections.Select(v => v.ToString()));
                    sql.AppendLine($"{(hasWhereClause ? "AND" : "WHERE")} i.Vein IN (SELECT Id FROM Veins WHERE Collection IN ({collectionsFilter}))");
                    hasWhereClause = true;
                }

                // Veins filter
                var selectedVeins = tokenSelectVeins.SelectedValues;
                if (selectedVeins.Any())
                {
                    string veinsFilter = string.Join(", ", selectedVeins.Select(v => v.ToString()));
                    sql.AppendLine($"{(hasWhereClause ? "AND" : "WHERE")} i.Vein IN ({veinsFilter})");
                    hasWhereClause = true;
                }

                // Tags filter
                var selectedTags = tokenSelectTags.SelectedValues;
                if (selectedTags.Any())
                {
                    string tagsFilter = string.Join("', '", selectedTags.Select(v => v.ToString()));
                    sql.AppendLine($"{(hasWhereClause ? "AND" : "WHERE")} i.Id IN (SELECT Icon FROM IconTags WHERE Tag IN ('{tagsFilter}'))");
                    hasWhereClause = true;
                }

                // Search filter
                if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    string searchText = txtSearch.Text.Trim();
                    sql.AppendLine($"{(hasWhereClause ? "AND" : "WHERE")} (i.Name LIKE '%{searchText}%' OR i.Id IN (SELECT Icon FROM IconTags WHERE Tag LIKE '%{searchText}%'))");
                    hasWhereClause = true;
                }

                sql.AppendLine("ORDER BY i.Name");
                //sql.AppendLine("LIMIT 500"); // Limit results to prevent UI freeze

                var response = conx.ExecuteTable(sql.ToString());

                if (response.IsOK)
                {
                    // Store all filtered results and initialize pagination
                    allFilteredIcons = response.Result;
                    totalResults = allFilteredIcons.Rows.Count;
                    currentPage = 1;
                    totalPages = (int)Math.Ceiling((double)totalResults / pageSize);

                    // Update UI labels
                    lblTotalResults.Text = $"Total: {totalResults} icons";
                    UpdatePaginationUI();

                    // Load first page
                    LoadCurrentPage();
                    toolStripStatusLabel.Text = $"Found {totalResults} icons";
                }
                else
                {
                    MessageBoxDialog.Show($"Error applying filter: {response.Errors[0].Message}", "Filter Error", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                    toolStripStatusLabel.Text = "Filter error";
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error applying filter: {ex.Message}", "Filter Error", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                toolStripStatusLabel.Text = "Filter error";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LoadIcons(DataTable iconsTable)
        {
            // Clear existing icons
            iconsFlowPanel.SuspendLayout();
            iconsFlowPanel.Controls.Clear();

            if (iconsTable == null || iconsTable.Rows.Count == 0)
            {
                iconsFlowPanel.ResumeLayout();
                return;
            }

            // Add icon controls
            foreach (DataRow row in iconsTable.Rows)
            {
                int iconId = Convert.ToInt32(row["Id"]);
                string iconName = row["Name"].ToString();

                // Get all icon files for this icon to select the best size match
                string sql = $"SELECT Id, BinData, Extension, Type, Size FROM IconFiles WHERE Icon = {iconId}";
                var iconFileResponse = conx.ExecuteTable(sql);

                if (iconFileResponse.IsOK && iconFileResponse.Result.Rows.Count > 0)
                {
                    // Find the best matching size
                    DataRow bestMatch = null;
                    int targetSize = currentIconSize * currentIconSize; // Target area
                    int closestSizeDiff = int.MaxValue;

                    foreach (DataRow fileRow in iconFileResponse.Result.Rows)
                    {
                        byte[] tempData = fileRow["BinData"] as byte[];
                        if (tempData != null && tempData.Length > 0)
                        {
                            try
                            {
                                using (MemoryStream ms = new MemoryStream(tempData))
                                using (Image img = Image.FromStream(ms))
                                {
                                    // Find the dimension closest to requested display size
                                    int maxDim = Math.Max(img.Width, img.Height);
                                    int sizeDiff = Math.Abs(maxDim - currentIconSize);

                                    if (sizeDiff < closestSizeDiff)
                                    {
                                        closestSizeDiff = sizeDiff;
                                        bestMatch = fileRow;
                                    }
                                }
                            }
                            catch
                            {
                                // Skip invalid images
                            }
                        }
                    }

                    if (bestMatch == null)
                        continue;

                    int iconFileId = Convert.ToInt32(bestMatch["Id"]);
                    byte[] binData = bestMatch["BinData"] as byte[];

                    if (binData != null && binData.Length > 0)
                    {
                        try
                        {
                            // Validate image data before creating control
                            using (MemoryStream ms = new MemoryStream(binData))
                            {
                                using (Image testImage = Image.FromStream(ms))
                                {
                                    // Image is valid, create control
                                    IconDisplayControl iconCtrl = new IconDisplayControl();
                                    iconCtrl.Width = currentIconSize + 10;
                                    iconCtrl.Height = currentIconSize + 10; // Just padding for border
                                    iconCtrl.IconFileId = iconFileId;
                                    iconCtrl.FileName = iconName;
                                    iconCtrl.ImageData = binData;
                                    iconCtrl.IconClicked += IconCtrl_IconClicked;
                                    iconCtrl.IconDoubleClicked += IconCtrl_IconDoubleClicked;

                                    iconsFlowPanel.Controls.Add(iconCtrl);
                                }
                            }
                        }
                        catch
                        {
                            // Skip invalid image data silently
                        }
                    }
                }
            }

            iconsFlowPanel.ResumeLayout();
        }

        private void LoadCurrentPage()
        {
            if (allFilteredIcons == null || allFilteredIcons.Rows.Count == 0)
            {
                iconsFlowPanel.Controls.Clear();
                return;
            }

            // Calculate start and end indices for current page
            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, totalResults);

            // Create a DataTable with just the current page's rows
            DataTable pageData = allFilteredIcons.Clone();
            for (int i = startIndex; i < endIndex; i++)
            {
                pageData.ImportRow(allFilteredIcons.Rows[i]);
            }

            // Load the icons for the current page
            LoadIcons(pageData);
        }

        private void UpdatePaginationUI()
        {
            if (totalPages == 0)
                totalPages = 1;

            lblPageInfo.Text = $"Page {currentPage} of {totalPages}";
            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadCurrentPage();
                UpdatePaginationUI();
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadCurrentPage();
                UpdatePaginationUI();
            }
        }

        private void IconCtrl_IconClicked(object sender, EventArgs e)
        {
            IconDisplayControl iconCtrl = sender as IconDisplayControl;
            if (iconCtrl != null)
            {
                selectedIconForDetails = iconCtrl;
                LoadIconDetails(iconCtrl.IconFileId);
            }
        }

        private void IconCtrl_IconDoubleClicked(object sender, EventArgs e)
        {
            // Double click moves to buffer
            IconDisplayControl iconCtrl = sender as IconDisplayControl;
            if (iconCtrl != null)
            {
                AddToBuffer(iconCtrl);
            }
        }

        private void LoadIconDetails(int iconFileId)
        {
            iconDetailsFlowPanel.SuspendLayout();
            iconDetailsFlowPanel.Controls.Clear();

            // Get icon information
            string sql = $"SELECT i.Id, i.Name, f.Id AS FileId, f.Extension, f.Type, f.Size, f.BinData FROM Icons i " +
                         $"INNER JOIN IconFiles f ON i.Id = f.Icon WHERE f.Icon = (SELECT Icon FROM IconFiles WHERE Id = {iconFileId}) ORDER BY f.Size";

            var response = conx.ExecuteTable(sql);

            if (response.IsOK && response.Result.Rows.Count > 0)
            {
                string iconName = response.Result.Rows[0]["Name"].ToString();
                foreach (DataRow row in response.Result.Rows)
                {
                    byte[] binData = row["BinData"] as byte[];
                    if (binData != null && binData.Length > 0)
                    {
                        try
                        {
                            // Create a panel to hold both image and details
                            Panel detailPanel = new Panel();
                            detailPanel.Width = iconDetailsFlowPanel.Width - 20;
                            detailPanel.Height = 150;
                            detailPanel.BorderStyle = BorderStyle.FixedSingle;
                            detailPanel.Margin = new Padding(5);

                            // Get actual image dimensions first
                            int width = 0, height = 0;
                            using (MemoryStream ms = new MemoryStream(binData))
                            {
                                using (Image img = Image.FromStream(ms))
                                {
                                    width = img.Width;
                                    height = img.Height;
                                }
                            }

                            // Calculate control height based on image size (with some padding)
                            int iconControlHeight = Math.Max(80, height + 20);
                            int labelHeight = 40;

                            // Adjust panel height to fit image and label
                            detailPanel.Height = iconControlHeight + labelHeight + 10;

                            // Create icon display control
                            IconDisplayControl iconCtrl = new IconDisplayControl();
                            iconCtrl.Width = detailPanel.Width;
                            iconCtrl.Height = iconControlHeight;
                            iconCtrl.Dock = DockStyle.Top;
                            iconCtrl.IconFileId = Convert.ToInt32(row["FileId"]);
                            iconCtrl.ImageData = binData;
                            iconCtrl.IconClicked += DetailIcon_Clicked;

                            // Create details label - only show size
                            Label lblDetails = new Label();
                            lblDetails.AutoSize = false;
                            lblDetails.Width = detailPanel.Width;
                            lblDetails.Height = labelHeight;
                            lblDetails.Dock = DockStyle.Fill;
                            lblDetails.Padding = new Padding(5);
                            lblDetails.Font = new Font(lblDetails.Font.FontFamily, 9, FontStyle.Bold);
                            lblDetails.TextAlign = ContentAlignment.MiddleCenter;
                            lblDetails.Text = $"{width} x {height} px";

                            // Add controls to panel
                            detailPanel.Controls.Add(lblDetails);
                            detailPanel.Controls.Add(iconCtrl);

                            iconDetailsFlowPanel.Controls.Add(detailPanel);
                        }
                        catch
                        {
                            // Skip invalid images
                        }
                    }
                }
                labIconName.Text = $"Icon Name: {iconName}";

                List<string> tags = conx.GetTagsForIcon(iconName);
                labTags.Text = $"Tags: {String.Join(", ", tags)}";
            }

            iconDetailsFlowPanel.ResumeLayout();
        }

        private void DetailIcon_Clicked(object sender, EventArgs e)
        {
            // Select this specific file for buffer operations
            IconDisplayControl iconCtrl = sender as IconDisplayControl;
            if (iconCtrl != null)
            {
                // Deselect all others (now within panels)
                foreach (Control panelCtrl in iconDetailsFlowPanel.Controls)
                {
                    if (panelCtrl is Panel)
                    {
                        foreach (Control ctrl in panelCtrl.Controls)
                        {
                            if (ctrl is IconDisplayControl detailIcon)
                            {
                                detailIcon.IsSelected = false;
                            }
                        }
                    }
                }

                iconCtrl.IsSelected = true;
            }
        }

        private void cmbIconSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIconSize.SelectedIndex < 0)
                return;

            string selected = cmbIconSize.SelectedItem.ToString();
            currentIconSize = int.Parse(selected.Replace("px", ""));

            // Reload icons with new size
            if (iconsFlowPanel.Controls.Count > 0)
            {
                foreach (Control ctrl in iconsFlowPanel.Controls)
                {
                    if (ctrl is IconDisplayControl)
                    {
                        ctrl.Width = currentIconSize + 10;
                        ctrl.Height = currentIconSize + 10; // Just padding, no label space needed
                    }
                }
            }
        }

        private void cmbPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPageSize.SelectedIndex < 0)
                return;

            string selected = cmbPageSize.SelectedItem.ToString();
            pageSize = int.Parse(selected);

            // Recalculate pagination and reload current page
            if (allFilteredIcons != null && allFilteredIcons.Rows.Count > 0)
            {
                totalPages = (int)Math.Ceiling((double)totalResults / pageSize);
                currentPage = 1; // Reset to first page
                UpdatePaginationUI();
                LoadCurrentPage();
            }
        }

        // ========================== BUFFER OPERATIONS ==========================

        private void btnMoveToBuffer_Click(object sender, EventArgs e)
        {
            // Find selected icon in details panel (now within panels)
            IconDisplayControl selectedIcon = null;
            foreach (Control panelCtrl in iconDetailsFlowPanel.Controls)
            {
                if (panelCtrl is Panel)
                {
                    foreach (Control ctrl in panelCtrl.Controls)
                    {
                        if (ctrl is IconDisplayControl iconCtrl && iconCtrl.IsSelected)
                        {
                            selectedIcon = iconCtrl;
                            break;
                        }
                    }
                }
                if (selectedIcon != null) break;
            }

            if (selectedIcon != null)
            {
                AddToBuffer(selectedIcon);
            }
            else if (selectedIconForDetails != null)
            {
                // Use the main icon if no detail is selected
                AddToBuffer(selectedIconForDetails);
            }
            else
            {
                MessageBoxDialog.Show("Please select an icon first!", "Move to Buffer", MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);
            }
        }

        private void AddToBuffer(IconDisplayControl iconCtrl)
        {
            // Check if already in buffer
            if (bufferIcons.Any(b => b.IconFileId == iconCtrl.IconFileId))
            {
                MessageBoxDialog.Show("Icon already in buffer!", "Buffer", MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);
                return;
            }

            // Get actual image dimensions to size the control properly
            int width = 64, height = 64;  // Default size
            if (iconCtrl.ImageData != null && iconCtrl.ImageData.Length > 0)
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream(iconCtrl.ImageData))
                    {
                        using (Image img = Image.FromStream(ms))
                        {
                            width = img.Width;
                            height = img.Height;
                        }
                    }
                }
                catch
                {
                    // Use default size if image can't be read
                }
            }

            // Insert into database
            int? projectId = SelectedProject?.Id;
            var insertResult = conx.BufferZone_Insert(iconCtrl.IconFileId, projectId);

            if (!insertResult.IsOK || insertResult.Result <= 0)
            {
                string errorMsg = insertResult.Errors != null && insertResult.Errors.Count > 0
                    ? insertResult.Errors[0].Message
                    : "Failed to insert into database";
                MessageBoxDialog.Show($"Failed to add to buffer: {errorMsg}", "Buffer", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                System.Diagnostics.Debug.WriteLine($"BufferZone_Insert failed: {errorMsg}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Successfully added icon to buffer. BufferZoneId: {insertResult.Result}");

            // Clone the icon control for buffer with appropriate size
            IconDisplayControl bufferIcon = new IconDisplayControl();
            bufferIcon.Width = width + 10;  // Image width + border padding
            bufferIcon.Height = height + 10; // Image height + border padding
            bufferIcon.IconFileId = iconCtrl.IconFileId;
            bufferIcon.FileName = iconCtrl.FileName;
            bufferIcon.ImageData = iconCtrl.ImageData;
            bufferIcon.IconClicked += BufferIcon_Clicked;
            bufferIcon.IconDoubleClicked += BufferIcon_DoubleClicked;

            // Store the BufferZone ID
            bufferZoneIds[iconCtrl.IconFileId] = insertResult.Result;

            bufferIcons.Add(bufferIcon);
            bufferFlowPanel.Controls.Add(bufferIcon);

            toolStripStatusLabel.Text = $"Added to buffer. Total: {bufferIcons.Count}";
        }

        private void LoadBufferZone()
        {
            if (conx == null)
                return;

            // Clear current buffer
            bufferIcons.Clear();
            bufferZoneIds.Clear();
            bufferFlowPanel.Controls.Clear();

            // Load icons from database
            int? projectId = SelectedProject?.Id;
            var response = conx.BufferZone_GetByProject(projectId);

            if (response.IsOK)
            {
                if (response.Result.Rows.Count > 0)
                {
                    foreach (DataRow row in response.Result.Rows)
                    {
                        int bufferZoneId = Convert.ToInt32(row["Id"]);
                        int iconFileId = Convert.ToInt32(row["IconFile"]);
                        byte[] binData = row["BinData"] as byte[];
                        string fileName = row["IconName"].ToString();

                        if (binData != null && binData.Length > 0)
                        {
                            try
                            {
                                // Get image dimensions
                                int width = 64, height = 64;
                                using (MemoryStream ms = new MemoryStream(binData))
                                {
                                    using (Image img = Image.FromStream(ms))
                                    {
                                        width = img.Width;
                                        height = img.Height;
                                    }
                                }

                                // Create icon control
                                IconDisplayControl bufferIcon = new IconDisplayControl();
                                bufferIcon.Width = width + 10;
                                bufferIcon.Height = height + 10;
                                bufferIcon.IconFileId = iconFileId;
                                bufferIcon.FileName = fileName;
                                bufferIcon.ImageData = binData;
                                bufferIcon.IconClicked += BufferIcon_Clicked;
                                bufferIcon.IconDoubleClicked += BufferIcon_DoubleClicked;

                                // Store the BufferZone ID
                                bufferZoneIds[iconFileId] = bufferZoneId;

                                bufferIcons.Add(bufferIcon);
                                bufferFlowPanel.Controls.Add(bufferIcon);
                            }
                            catch (Exception ex)
                            {
                                // Log but skip invalid images
                                System.Diagnostics.Debug.WriteLine($"Error loading buffer icon: {ex.Message}");
                            }
                        }
                    }

                    toolStripStatusLabel.Text = $"Loaded {bufferIcons.Count} icons from buffer zone";
                }
                else
                {
                    toolStripStatusLabel.Text = "Buffer zone is empty";
                }
            }
            else
            {
                // Show error
                string errorMsg = response.Errors != null && response.Errors.Count > 0
                    ? response.Errors[0].Message
                    : "Unknown error loading buffer zone";
                toolStripStatusLabel.Text = $"Error loading buffer: {errorMsg}";
                System.Diagnostics.Debug.WriteLine($"Error loading buffer zone: {errorMsg}");
            }
        }

        private void BufferIcon_DoubleClicked(object sender, EventArgs e)
        {
            // Double-click removes from buffer
            IconDisplayControl iconCtrl = sender as IconDisplayControl;
            if (iconCtrl != null)
            {
                RemoveFromBuffer(iconCtrl);
            }
        }

        private void RemoveFromBuffer(IconDisplayControl iconCtrl)
        {
            if (iconCtrl == null)
                return;

            // Get BufferZone ID
            if (!bufferZoneIds.ContainsKey(iconCtrl.IconFileId))
            {
                MessageBoxDialog.Show("Could not find buffer zone record!", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            int bufferZoneId = bufferZoneIds[iconCtrl.IconFileId];

            // Delete from database
            var deleteResult = conx.BufferZone_Delete(bufferZoneId);

            if (!deleteResult.IsOK)
            {
                MessageBoxDialog.Show($"Failed to remove from buffer: {deleteResult.Errors[0].Message}", "Remove", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                return;
            }

            // Remove from UI and lists
            bufferZoneIds.Remove(iconCtrl.IconFileId);
            bufferIcons.Remove(iconCtrl);
            bufferFlowPanel.Controls.Remove(iconCtrl);

            // Update button states
            int selectedCount = bufferIcons.Count(i => i.IsSelected);
            btnMerge.Text = selectedCount == 2 ? "Merge (2 Selected)" : $"Merge (Select 2 - {selectedCount} selected)";
            btnMerge.Enabled = selectedCount == 2;

            toolStripStatusLabel.Text = $"Removed from buffer. Total: {bufferIcons.Count}";
        }

        private void btnRemoveFromBuffer_Click(object sender, EventArgs e)
        {
            // Get all selected icons
            var selectedIcons = bufferIcons.Where(i => i.IsSelected).ToList();

            if (selectedIcons.Count == 0)
            {
                MessageBoxDialog.Show("Please select at least one icon to remove.\n\nTip: You can also double-click an icon to remove it.", "Remove from Buffer", MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);
                return;
            }

            // Confirm removal
            var result = MessageBoxDialog.Show($"Are you sure you want to remove {selectedIcons.Count} icon(s) from the buffer?", "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question, themeManager1.Theme);

            if (result == DialogResult.Yes)
            {
                // Remove each selected icon
                foreach (var icon in selectedIcons)
                {
                    RemoveFromBuffer(icon);
                }
            }
        }

        private void BufferIcon_Clicked(object sender, EventArgs e)
        {
            IconDisplayControl iconCtrl = sender as IconDisplayControl;
            if (iconCtrl != null)
            {
                // Toggle selection
                iconCtrl.IsSelected = !iconCtrl.IsSelected;

                // Update button states
                int selectedCount = bufferIcons.Count(i => i.IsSelected);
                btnMerge.Text = selectedCount == 2 ? "Merge (2 Selected)" : $"Merge (Select 2 - {selectedCount} selected)";
                btnMerge.Enabled = selectedCount == 2;
            }
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            var selectedIcons = bufferIcons.Where(i => i.IsSelected).ToList();

            if (selectedIcons.Count != 2)
            {
                MessageBoxDialog.Show("Please select exactly 2 icons to merge!", "Merge", MessageBoxButtons.OK, MessageBoxIcon.Warning, themeManager1.Theme);
                return;
            }

            try
            {
                IconDisplayControl icon1 = selectedIcons[0];
                IconDisplayControl icon2 = selectedIcons[1];

                // Validate that both icons have image data
                if (icon1.ImageData == null || icon1.ImageData.Length == 0)
                {
                    MessageBoxDialog.Show($"Icon '{icon1.FileName}' has no image data. Cannot merge.", "Merge",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                    return;
                }

                if (icon2.ImageData == null || icon2.ImageData.Length == 0)
                {
                    MessageBoxDialog.Show($"Icon '{icon2.FileName}' has no image data. Cannot merge.", "Merge",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                    return;
                }

                // Determine which icon is bigger by calculating actual dimensions
                int size1 = GetImageSize(icon1.ImageData);
                int size2 = GetImageSize(icon2.ImageData);

                IconDisplayControl bigIcon = size1 >= size2 ? icon1 : icon2;
                IconDisplayControl smallIcon = size1 >= size2 ? icon2 : icon1;

                // Open merge dialog
                MergeForm mergeForm = new MergeForm(
                    databaseConnectionString,
                    themeManager1.Theme,
                    bigIcon.IconFileId,
                    smallIcon.IconFileId,
                    bigIcon.FileName,
                    smallIcon.FileName,
                    bigIcon.ImageData,
                    smallIcon.ImageData);

                if (mergeForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBoxDialog.Show("Icons merged successfully!\n\nThe merged icon has been saved to the 'Merged Icons' collection.",
                        "Merge", MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);

                    // Optionally reload current page to show the merged result if it matches filters
                    LoadCurrentPage();
                    LoadBufferZone();
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error merging icons:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                    "Merge Error", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
            }
        }

        private int GetImageSize(byte[] imageData)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    using (Image img = Image.FromStream(ms))
                    {
                        return img.Width * img.Height;
                    }
                }
            }
            catch
            {
                // If we can't load the image, return 0
                return 0;
            }
        }

        private void btnExportToProject_Click(object sender, EventArgs e)
        {
            if (SelectedProject == null)
            {
                MessageBoxDialog.Show("Please open a project first!\n\nGo to Project → Create Project or Project → Open Project", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning, themeManager1.Theme);
                return;
            }

            var selectedIcons = bufferIcons.Where(i => i.IsSelected).ToList();

            if (selectedIcons.Count == 0)
            {
                MessageBoxDialog.Show("Please select at least one icon from the buffer to export!\n\nClick on icons in the buffer panel to select them.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning, themeManager1.Theme);
                return;
            }

            try
            {
                // Show confirmation
                string message = $"Export {selectedIcons.Count} icon(s) to project?\n\n" +
                                $"Project: {SelectedProject.Name}\n" +
                                $"Type: {SelectedProject.Type}\n" +
                                $"Path: {SelectedProject.Path}\n\n" +
                                $"Continue?";

                DialogResult confirmResult = MessageBoxDialog.Show(message, "Export Icons",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, themeManager1.Theme);

                if (confirmResult != DialogResult.Yes)
                    return;

                // Prepare icon data for export
                List<IconExportData> iconsToExport = new List<IconExportData>();

                foreach (var iconControl in selectedIcons)
                {
                    // Query database for icon file details
                    string sql = "SELECT FileName, Extension FROM IconFiles WHERE Id = @id";
                    var parameters = new Dictionary<string, string>
                    {
                        { "@id", iconControl.IconFileId.ToString() }
                    };

                    var response = conx.ExecuteTable(sql, parameters);

                    if (response.IsOK && response.Result.Rows.Count > 0)
                    {
                        DataRow row = response.Result.Rows[0];
                        string fileName = row["FileName"].ToString();
                        string extension = row["Extension"].ToString();

                        iconsToExport.Add(new IconExportData
                        {
                            IconFileId = iconControl.IconFileId,
                            FileName = fileName,
                            Extension = extension,
                            BinData = iconControl.ImageData
                        });
                    }
                }

                // Perform export
                ExportManager exportManager = new ExportManager(conx);
                ExportResult result = exportManager.ExportIcons(SelectedProject, iconsToExport);

                // Show results
                if (result.HasErrors)
                {
                    string errors = string.Join("\n", result.Errors);
                    MessageBoxDialog.Show($"Export failed with errors:\n\n{errors}",
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
                }
                else if (result.IsSuccess)
                {
                    string successMessage = $"Successfully exported {result.SuccessCount} icon(s)!\n\n";

                    if (result.ExportedFiles.Count > 0)
                        successMessage += $"Files created: {result.ExportedFiles.Count}\n";

                    if (result.ResourcesAdded.Count > 0)
                        successMessage += $"Resources added: {result.ResourcesAdded.Count}\n";

                    if (result.ProjectFilesUpdated > 0)
                        successMessage += $"Project files updated: {result.ProjectFilesUpdated}\n";

                    if (result.Warnings.Count > 0)
                    {
                        successMessage += $"\nWarnings:\n{string.Join("\n", result.Warnings)}";
                    }

                    MessageBoxDialog.Show(successMessage, "Export Successful",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);

                    // Optionally deselect icons after export
                    foreach (var icon in selectedIcons)
                    {
                        icon.IsSelected = false;
                    }
                }
                else
                {
                    MessageBoxDialog.Show("No icons were exported.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, themeManager1.Theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error during export:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error, themeManager1.Theme);
            }
        }

        private void PositionFilterControls()
        {
            // Get available width
            int availableWidth = topFilterPanel.Width - topFilterPanel.Padding.Left - topFilterPanel.Padding.Right;

            // Define spacing and sizes
            int leftMargin = 11;
            int topMargin = 11;
            int labelHeight = 15;
            int controlSpacing = 15;

            // Calculate proportional widths based on available space
            int typesWidth = 239;  // Fixed width for checkbox list
            int tokenSelectWidth = Math.Max(150, (availableWidth - typesWidth - controlSpacing * 4 - leftMargin - 220) / 3);
            int searchWidth = 200;

            int currentX = leftMargin;

            // Position Types label and checklist
            lblTypes.Location = new Point(currentX, topMargin);
            chkListTypes.Location = new Point(currentX, topMargin + labelHeight + 2);
            chkListTypes.Width = typesWidth;
            currentX += typesWidth + controlSpacing;

            // Position Collections label and TokenSelect
            lblCollections.Location = new Point(currentX, topMargin);
            tokenSelectCollections.Location = new Point(currentX, topMargin + labelHeight + 2);
            tokenSelectCollections.Width = tokenSelectWidth;
            currentX += tokenSelectWidth + controlSpacing;

            // Position Veins label and TokenSelect
            lblVeins.Location = new Point(currentX, topMargin);
            tokenSelectVeins.Location = new Point(currentX, topMargin + labelHeight + 2);
            tokenSelectVeins.Width = tokenSelectWidth;
            currentX += tokenSelectWidth + controlSpacing;

            // Position Tags label and TokenSelect
            lblTags.Location = new Point(currentX, topMargin);
            tokenSelectTags.Location = new Point(currentX, topMargin + labelHeight + 2);
            tokenSelectTags.Width = tokenSelectWidth;
            currentX += tokenSelectWidth + controlSpacing;

            // Position Search label, textbox, and button at the right
            int searchX = availableWidth - searchWidth + leftMargin;
            lblSearch.Location = new Point(searchX, topMargin);
            txtSearch.Location = new Point(searchX, topMargin + labelHeight + 2);
            txtSearch.Width = searchWidth;
            btnApplyFilter.Location = new Point(searchX, topMargin + labelHeight + 26);
            btnApplyFilter.Width = searchWidth;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Recalculate filter control positions
            PositionFilterControls();
        }
    }
}
