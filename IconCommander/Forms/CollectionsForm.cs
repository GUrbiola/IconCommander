using IconCommander.DataAccess;
using System;
using System.Data;
using System.Windows.Forms;
using ZidUtilities.CommonCode;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.Controls.Grid;
using ZidUtilities.CommonCode.Win.CRUD;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    /// <summary>
    /// CRUD management form for the <c>Collections</c> table.
    /// Collections are the top-level organisational units that group related icon sets
    /// (e.g. "Font Awesome", "Material Design").  The form uses <c>UIGenerator</c> to
    /// produce Insert and Update dialogs at runtime and a <c>ZidGrid</c> for display.
    /// </summary>
    public partial class CollectionsForm : Form
    {
        private string connectionString;
        private ZidThemes theme;
        private int selectedRowIndex = -1;
        private IIconCommanderDb Conx;

        /// <summary>
        /// Initialises the form, establishes the database connection, and registers ZidGrid plugins.
        /// </summary>
        /// <param name="dbConnectionString">Active database connection string.</param>
        /// <param name="currentTheme">Theme to apply to this form.</param>
        public CollectionsForm(string dbConnectionString, ZidThemes currentTheme)
        {
            InitializeComponent();

            if(Properties.Settings.Default.IsSqlite)
                Conx = new SqliteConnector();
            else
                Conx = new SqlConnector();
            Conx.Initialize(dbConnectionString);

            connectionString = dbConnectionString;
            theme = currentTheme;

            zidGrid1.Plugins.Add(new ZidUtilities.CommonCode.Win.Controls.Grid.Plugins.DataExportPlugin());
            zidGrid1.Plugins.Add(new ZidUtilities.CommonCode.Win.Controls.Grid.Plugins.ColumnVisibilityPlugin());
            zidGrid1.Plugins.Add(new ZidUtilities.CommonCode.Win.Controls.Grid.Plugins.CopySpecialPlugin());
            zidGrid1.Plugins.Add(new ZidUtilities.CommonCode.Win.Controls.Grid.Plugins.FreezeColumnsPlugin());
            zidGrid1.Plugins.Add(new ZidUtilities.CommonCode.Win.Controls.Grid.Plugins.QuickFilterPlugin());

            foreach (var option in zidGrid1.GetDefaultMenuOptions())
                zidGrid1.CustomMenuItems.Add(option);

        }

        /// <summary>Applies the active theme and performs the initial data load.</summary>
        private void CollectionsForm_Load(object sender, EventArgs e)
        {
            // Apply theme
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            // Load data
            LoadCollections();
        }

        /// <summary>Queries all rows from the <c>Collections</c> table and binds the result to <c>zidGrid1</c>.</summary>
        private void LoadCollections()
        {
            try
            {
                var response = Conx.ExecuteTable("SELECT * FROM Collections");

                if (response.IsOK)
                {
                    zidGrid1.DataSource = response.Result;
                }
                else
                {
                    string errorMessage = response.Errors.Count > 0 && response.Errors[0].Exception != null
                        ? response.Errors[0].Exception.Message
                        : response.Errors[0].Message;

                    MessageBoxDialog.Show($"Error loading collections: {errorMessage}", "Collections",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error loading collections: {ex.Message}", "Collections",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        /// <summary>Returns the <c>Id</c> of the currently selected grid row, or <c>null</c> when nothing is selected.</summary>
        private int? GetSelectedId()
        {
            if(zidGrid1.SelectedRow != null)
                return Convert.ToInt32(zidGrid1.SelectedRow.Cells["Id"].Value);

            return null;
        }

        /// <summary>Returns the underlying <see cref="DataRow"/> for the currently selected grid row, or <c>null</c> when nothing is selected.</summary>
        private DataRow GetSelectedRow()
        {
            if (zidGrid1.SelectedRow != null)
            {
                DataGridViewRow selectedRow = zidGrid1.SelectedRow;
                DataRow dataRow = ((DataRowView)selectedRow.DataBoundItem).Row;

                return dataRow;
            }

            return null;
        }

        /// <summary>Opens a <c>UIGenerator</c> insert dialog; on confirmation executes the generated INSERT SQL and refreshes the grid.</summary>
        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Description", typeof(string));
                table.Columns.Add("Comments", typeof(string));
                table.Columns.Add("License", typeof(int));

                UIGenerator formGen = new UIGenerator(table, "Collections");
                formGen.FormWidth = 800;
                formGen.Theme = theme;

                // Set Primary Keys
                formGen.ClearAllPrimaryKeys();
                formGen.SetPrimaryKey("Id");

                // Set Exclusions
                formGen.SetExclusion("Id");

                // Set Aliases
                formGen.SetAlias("Name", "Collection Name");
                formGen.SetAlias("Description", "Description");
                formGen.SetAlias("Comments", "Comments");
                formGen.SetAlias("License", "License ID");

                // Set Required Fields
                formGen.SetRequired("Name");

                var result = formGen.ShowInsertDialog();

                if (result != null)
                {
                    string sql = Properties.Settings.Default.IsSqlite ? result.SqliteScript : result.SqlServerScript;
                    
                    Conx.ExecuteNonQuery(sql);

                    if (Conx.Error)
                    {
                        MessageBoxDialog.Show($"Failed to create collection: {Conx.LastMessage}", "Collections",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    }
                    else
                    {
                        MessageBoxDialog.Show("Collection created successfully!", "Collections",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                        LoadCollections();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error inserting collection: {ex.Message}", "Collections",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        /// <summary>Opens a <c>UIGenerator</c> update dialog pre-populated with the selected row; on confirmation executes the generated UPDATE SQL and refreshes the grid.</summary>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                int? id = GetSelectedId();
                if (!id.HasValue)
                {
                    MessageBoxDialog.Show("Please select a collection to update.", "Collections",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Get current data
                var response = Conx.ExecuteTable($"SELECT * FROM Collections WHERE Id = {id.Value}");

                if (response.IsOK && response.Result.Rows.Count > 0)
                {
                    DataTable table = response.Result;

                    UIGenerator formGen = new UIGenerator(table, "Collections");
                    formGen.FormWidth = 800; 
                    formGen.Theme = theme;

                    // Set Primary Keys
                    formGen.ClearAllPrimaryKeys();
                    formGen.SetPrimaryKey("Id");

                    // Set Exclusions
                    formGen.SetExclusion("Id");

                    // Set Aliases
                    formGen.SetAlias("Name", "Collection Name");
                    formGen.SetAlias("Description", "Description");
                    formGen.SetAlias("Comments", "Comments");
                    formGen.SetAlias("License", "License ID");

                    // Set Required Fields
                    formGen.SetRequired("Name");

                    var result = formGen.ShowUpdateDialog(table.Rows[0]);

                    if (result != null)
                    {
                        string sql = Properties.Settings.Default.IsSqlite ? result.SqliteScript : result.SqlServerScript;

                        Conx.ExecuteNonQuery(sql);

                        if (Conx.Error)
                        {
                            MessageBoxDialog.Show($"Failed to update collection: {Conx.LastMessage}", "Collections",
                                MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                        }
                        else
                        {
                            MessageBoxDialog.Show("Collection updated successfully!", "Collections",
                                MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                            LoadCollections();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error updating collection: {ex.Message}", "Collections",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        /// <summary>Prompts for confirmation then deletes the selected collection from the database and refreshes the grid.</summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow selectedRow = GetSelectedRow();
                if (selectedRow == null)
                {
                    MessageBoxDialog.Show("Please select a collection to delete.", "Collections",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                int id = Convert.ToInt32(selectedRow["Id"]);
                string name = selectedRow["Name"]?.ToString() ?? "Unknown";

                DialogResult confirm = MessageBoxDialog.Show(
                    $"Are you sure you want to delete the collection '{name}'?",
                    "Delete Collection",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    theme);

                if (confirm == DialogResult.Yes)
                {
                    string sql = $"DELETE FROM Collections WHERE Id = {id}";
                    Conx.ExecuteNonQuery(sql);

                    if (Conx.Error)
                    {
                        MessageBoxDialog.Show($"Failed to delete collection: {Conx.LastMessage}", "Collections",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    }
                    else
                    {
                        MessageBoxDialog.Show("Collection deleted successfully!", "Collections",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                        LoadCollections();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error deleting collection: {ex.Message}", "Collections",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        /// <summary>Reloads the collections grid from the database.</summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadCollections();
        }
    }
}
