using IconCommander.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using ZidUtilities.CommonCode;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.Controls.Grid;
using ZidUtilities.CommonCode.Win.CRUD;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    public partial class VeinsForm : Form
    {
        private string connectionString;
        private ZidThemes theme;
        private int selectedRowIndex = -1;
        private IIconCommanderDb Conx;


        public VeinsForm(string dbConnectionString, ZidThemes currentTheme)
        {
            InitializeComponent();
            
            if (Properties.Settings.Default.IsSqlite)
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

        private void VeinsForm_Load(object sender, EventArgs e)
        {
            // Apply theme
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            // Load data
            LoadVeins();
        }

        private void LoadVeins()
        {
            try
            {
                var response = Conx.ExecuteTable(@"
                    SELECT
                        v.Id,
                        v.Collection,
                        c.Name AS CollectionName,
                        v.Name,
                        v.Description,
                        v.Path,
                        v.IsIcon,
                        v.IsImage,
                        v.IsSvg
                    FROM Veins v
                    LEFT JOIN Collections c ON v.Collection = c.Id
                    ORDER BY v.Name");

                if (response.IsOK)
                {
                    zidGrid1.DataSource = response.Result;
                }
                else
                {
                    string errorMessage = response.Errors.Count > 0 && response.Errors[0].Exception != null
                        ? response.Errors[0].Exception.Message
                        : response.Errors[0].Message;

                    MessageBoxDialog.Show($"Error loading veins: {errorMessage}", "Veins",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error loading veins: {ex.Message}", "Veins",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private int? GetSelectedId()
        {
            if (zidGrid1.SelectedRow != null)
                return Convert.ToInt32(zidGrid1.SelectedRow.Cells["Id"].Value);

            return null;
        }

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

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Collection", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Description", typeof(string));
                table.Columns.Add("Path", typeof(string));
                table.Columns.Add("IsIcon", typeof(int));
                table.Columns.Add("IsImage", typeof(int));
                table.Columns.Add("IsSvg", typeof(int));

                UIGenerator formGen = new UIGenerator(table, "Veins");
                formGen.FormWidth = 800;
                formGen.Theme = theme;

                // Set Primary Keys
                formGen.ClearAllPrimaryKeys();
                formGen.SetPrimaryKey("Id");

                // Set Exclusions
                formGen.SetExclusion("Id");

                // Set Aliases
                formGen.SetAlias("Collection", "Collection");
                formGen.SetAlias("Name", "Vein Name");
                formGen.SetAlias("Description", "Description");
                formGen.SetAlias("Path", "Vein Path");
                formGen.SetAlias("IsIcon", "Contains Icons");
                formGen.SetAlias("IsImage", "Contains Images");
                formGen.SetAlias("IsSvg", "Contains SVG");

                // Set Required Fields
                formGen.SetRequired("Collection");
                formGen.SetRequired("Name");
                formGen.SetRequired("Path");
                formGen.SetRequired("IsIcon");
                formGen.SetRequired("IsImage");
                formGen.SetRequired("IsSvg");

                // Set Field Formatting
                formGen.SetFieldFormat("Path", FieldFormat.Folder);
                formGen.SetFieldFormat("IsIcon", FieldFormat.Check);
                formGen.SetFieldFormat("IsImage", FieldFormat.Check);
                formGen.SetFieldFormat("IsSvg", FieldFormat.Check);


                // Set Collection as a list from database
                var collectionsResponse = Conx.ExecuteTable("SELECT Id, Name FROM Collections ORDER BY Name");
                if (collectionsResponse.IsOK && collectionsResponse.Result.Rows.Count > 0)
                {
                    Dictionary<string, object> collections = new Dictionary<string, object>();
                    foreach (DataRow row in collectionsResponse.Result.Rows)
                    {
                        collections.Add(row["Name"].ToString(), int.Parse(row["Id"].ToString()));
                    }

                    // Set Foreign Key Collections
                    formGen.SetForeignKey("Collection", collections);
                }

                var result = formGen.ShowInsertDialog();

                if (result != null)
                {
                    string sql;
                    
                    if(Properties.Settings.Default.IsSqlite)
                        sql = result.SqliteScript;
                    else
                        sql = result.SqlServerScript;
                    
                    Conx.ExecuteNonQuery(sql);
                    if (Conx.Error)
                    {
                        MessageBoxDialog.Show($"Failed to create vein: {Conx.LastMessage}", "Veins",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    }
                    else
                    {
                        MessageBoxDialog.Show("Vein created successfully!", "Veins",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                        LoadVeins();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error inserting vein: {ex.Message}", "Veins",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                int? id = GetSelectedId();
                if (!id.HasValue)
                {
                    MessageBoxDialog.Show("Please select a vein to update.", "Veins",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Get current data
                var response = Conx.ExecuteTable($"SELECT * FROM Veins WHERE Id = {id.Value}");

                if (response.IsOK && response.Result.Rows.Count > 0)
                {
                    DataTable table = response.Result;

                    UIGenerator formGen = new UIGenerator(table, "Veins");
                    formGen.FormWidth = 800;
                    formGen.Theme = theme;

                    // Set Primary Keys
                    formGen.ClearAllPrimaryKeys();
                    formGen.SetPrimaryKey("Id");

                    // Set Exclusions
                    formGen.SetExclusion("Id");

                    // Set Aliases
                    formGen.SetAlias("Collection", "Collection");
                    formGen.SetAlias("Name", "Vein Name");
                    formGen.SetAlias("Description", "Description");
                    formGen.SetAlias("Path", "Vein Path");
                    formGen.SetAlias("IsIcon", "Contains Icons");
                    formGen.SetAlias("IsImage", "Contains Images");
                    formGen.SetAlias("IsSvg", "Contains SVG");

                    // Set Required Fields
                    formGen.SetRequired("Collection");
                    formGen.SetRequired("Name");
                    formGen.SetRequired("Path");
                    formGen.SetRequired("IsIcon");
                    formGen.SetRequired("IsImage");
                    formGen.SetRequired("IsSvg");

                    // Set Field Formatting
                    formGen.SetFieldFormat("Path", FieldFormat.Folder);
                    formGen.SetFieldFormat("IsIcon", FieldFormat.Check);
                    formGen.SetFieldFormat("IsImage", FieldFormat.Check);
                    formGen.SetFieldFormat("IsSvg", FieldFormat.Check);

                    // Set Collection as a list from database
                    var collectionsResponse = Conx.ExecuteTable("SELECT Id, Name FROM Collections ORDER BY Name");
                    if (collectionsResponse.IsOK && collectionsResponse.Result.Rows.Count > 0)
                    {
                        Dictionary<string, object> collections = new Dictionary<string, object>();
                        foreach (DataRow row in collectionsResponse.Result.Rows)
                        {
                            collections.Add(row["Name"].ToString(), int.Parse(row["Id"].ToString()));
                        }

                        // Set Foreign Key Collections
                        formGen.SetForeignKey("Collection", collections);
                    }

                    var result = formGen.ShowUpdateDialog(table.Rows[0]);

                    if (result != null)
                    {
                        string sql;
                        if (Properties.Settings.Default.IsSqlite)
                            sql = result.SqliteScript;
                        else
                            sql = result.SqlServerScript;
                        
                        Conx.ExecuteNonQuery(sql);

                        if (Conx.Error)
                        {
                            MessageBoxDialog.Show($"Failed to update vein: {Conx.LastMessage}", "Veins",
                                MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                        }
                        else
                        {
                            MessageBoxDialog.Show("Vein updated successfully!", "Veins",
                                MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                            LoadVeins();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error updating vein: {ex.Message}", "Veins",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow selectedRow = GetSelectedRow();
                if (selectedRow == null)
                {
                    MessageBoxDialog.Show("Please select a vein to delete.", "Veins",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                int id = Convert.ToInt32(selectedRow["Id"]);
                string name = selectedRow["Name"]?.ToString() ?? "Unknown";

                DialogResult confirm = MessageBoxDialog.Show(
                    $"Are you sure you want to delete the vein '{name}'?",
                    "Delete Vein",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    theme);

                if (confirm == DialogResult.Yes)
                {
                    string sql = $"DELETE FROM Veins WHERE Id = {id}";
                    
                    Conx.ExecuteNonQuery(sql);

                    if (Conx.Error)
                    {
                        MessageBoxDialog.Show($"Failed to delete vein: {Conx.LastMessage}", "Veins",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    }
                    else
                    {
                        MessageBoxDialog.Show("Vein deleted successfully!", "Veins",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                        LoadVeins();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error deleting vein: {ex.Message}", "Veins",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadVeins();
        }
    }
}
