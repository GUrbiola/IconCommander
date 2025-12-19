using IconCommander.DataAccess;
using System;
using System.Collections;
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
    public partial class IconsForm : Form
    {
        private string connectionString;
        private ZidThemes theme;
        private int selectedRowIndex = -1;
        private IIconCommanderDb Conx;

        public IconsForm(string dbConnectionString, ZidThemes currentTheme)
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

        private void IconsForm_Load(object sender, EventArgs e)
        {
            // Apply theme
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            // Load data
            LoadIcons();
        }

        private void LoadIcons()
        {
            try
            {
                // Query with JOINs to show Collection and Vein names
                string sql = @"
SELECT
    i.Id,
    i.Name,
    i.IsIco,
    i.IsImage,
    i.IsSvg,
    i.Vein,
    v.Name AS VeinName,
    v.Collection,
    c.Name AS CollectionName
FROM 
	Icons i LEFT OUTER JOIN Veins v ON i.Vein = v.Id
	LEFT OUTER JOIN Collections c ON v.Collection = c.Id
ORDER BY 
	i.Name";

                var response = Conx.ExecuteTable(sql);

                if (response.IsOK)
                {
                    zidGrid1.DataSource = response.Result;
                }
                else
                {
                    string errorMessage = response.Errors.Count > 0 && response.Errors[0].Exception != null
                        ? response.Errors[0].Exception.Message
                        : response.Errors[0].Message;

                    MessageBoxDialog.Show($"Error loading icons: {errorMessage}", "Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error loading icons: {ex.Message}", "Icons",
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
                // Get Veins for foreign key dropdown
                var veinsResponse = Conx.ExecuteTable("SELECT Id, Name FROM Veins ORDER BY Name");
                if (!veinsResponse.IsOK)
                {
                    MessageBoxDialog.Show("Failed to load veins for selection.", "Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    return;
                }

    //            i.Id,
    //i.Name,
    //i.IsIco,
    //i.IsImage,
    //i.IsSvg,
    //i.Vein,
    //v.Name AS VeinName,
    //v.Collection,
    //c.Name AS CollectionName

                DataTable table = new DataTable();
                table.Columns.Add("Id", typeof(int));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("IsIco", typeof(string));
                table.Columns.Add("IsImage", typeof(int));
                table.Columns.Add("IsSvg", typeof(int));
                table.Columns.Add("Vein", typeof(int));

                UIGenerator formGen = new UIGenerator(table, "Icons");
                formGen.FormWidth = 800;
                formGen.Theme = theme;

                // Set Primary Keys
                formGen.ClearAllPrimaryKeys();
                formGen.SetPrimaryKey("Id");

                // Set Exclusions
                formGen.SetExclusion("Id");

                // Set Aliases
                formGen.SetAlias("Name", "Icon Name");
                formGen.SetAlias("IsIco", "Is an Icon");
                formGen.SetAlias("IsImage", "Is an Image");
                formGen.SetAlias("IsSvg", "Is a SVG File");
                formGen.SetAlias("Vein", "Vein");

                // Set Required Fields
                formGen.SetRequired("Name");
                formGen.SetRequired("Vein");

                // Set Foreign Key for Vein
                Dictionary<string, object> veins = new Dictionary<string, object>();
                foreach (DataRow row in veinsResponse.Result.Rows)
                {
                    veins.Add(row["Name"].ToString(), int.Parse(row["Id"].ToString()));
                }
                formGen.SetForeignKey("Vein", veins);

                var result = formGen.ShowInsertDialog();

                if (result != null)
                {
                    string sql = Properties.Settings.Default.IsSqlite ? result.SqliteScript : result.SqlServerScript;
                    Conx.ExecuteNonQuery(sql);

                    if (Conx.Error)
                    {
                        MessageBoxDialog.Show($"Failed to create icon: {Conx.LastMessage}", "Icons",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    }
                    else
                    {
                        MessageBoxDialog.Show("Icon created successfully!", "Icons",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                        LoadIcons();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error inserting icon: {ex.Message}", "Icons",
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
                    MessageBoxDialog.Show("Please select an icon to update.", "Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Get Veins for foreign key dropdown
                var veinsResponse = Conx.ExecuteTable("SELECT Id, Name FROM Veins ORDER BY Name");
                if (!veinsResponse.IsOK)
                {
                    MessageBoxDialog.Show("Failed to load veins for selection.", "Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    return;
                }

                // Get current data
                var response = Conx.ExecuteTable($"SELECT * FROM Icons WHERE Id = {id.Value}");

                if (response.IsOK && response.Result.Rows.Count > 0)
                {
                    DataTable table = response.Result;

                    UIGenerator formGen = new UIGenerator(table, "Icons");
                    formGen.FormWidth = 800;
                    formGen.Theme = theme;

                    // Set Primary Keys
                    formGen.ClearAllPrimaryKeys();
                    formGen.SetPrimaryKey("Id");

                    // Set Exclusions
                    formGen.SetExclusion("Id");

                    // Set Aliases
                    formGen.SetAlias("Name", "Icon Name");
                    formGen.SetAlias("IsIco", "Is an Icon");
                    formGen.SetAlias("IsImage", "Is an Image");
                    formGen.SetAlias("IsSvg", "Is a SVG File");
                    formGen.SetAlias("Vein", "Vein");

                    // Set Required Fields
                    formGen.SetRequired("Name");
                    formGen.SetRequired("Vein");

                    // Set Foreign Key for Vein
                    Dictionary<string, object> veins = new Dictionary<string, object>();
                    foreach (DataRow row in veinsResponse.Result.Rows)
                    {
                        veins.Add(row["Name"].ToString(), int.Parse(row["Id"].ToString()));
                    }
                    formGen.SetForeignKey("Vein", veins);

                    var result = formGen.ShowUpdateDialog(table.Rows[0]);

                    if (result != null)
                    {
                        string sql = Properties.Settings.Default.IsSqlite ? result.SqliteScript : result.SqlServerScript;
                        Conx.ExecuteNonQuery(sql);

                        if (Conx.Error)
                        {
                            MessageBoxDialog.Show($"Failed to update icon: {Conx.LastMessage}", "Icons",
                                MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                        }
                        else
                        {
                            MessageBoxDialog.Show("Icon updated successfully!", "Icons",
                                MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                            LoadIcons();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error updating icon: {ex.Message}", "Icons",
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
                    MessageBoxDialog.Show("Please select an icon to delete.", "Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                int id = Convert.ToInt32(selectedRow["Id"]);
                string name = selectedRow["Name"]?.ToString() ?? "Unknown";

                DialogResult confirm = MessageBoxDialog.Show(
                    $"Are you sure you want to delete the icon '{name}'?\n\nThis will also delete all associated icon files, tags, and merge recipes.",
                    "Delete Icon",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    theme);

                if (confirm == DialogResult.Yes)
                {
                    // Delete with cascading (MergeRecipes, IconTags, IconFiles, Icons)
                    string sql = $@"
DELETE FROM MergeRecipes
WHERE BigIcon IN (SELECT Id FROM IconFiles WHERE Icon = {id})
   OR SmallIcon IN (SELECT Id FROM IconFiles WHERE Icon = {id})
   OR IconResult IN (SELECT Id FROM IconFiles WHERE Icon = {id});
DELETE FROM BufferZone WHERE IconFile IN (SELECT Id FROM IconFiles WHERE Icon = {id});
DELETE FROM IconTags WHERE Icon = {id};
DELETE FROM IconFiles WHERE Icon = {id};
DELETE FROM Icons WHERE Id = {id};";

                    Conx.ExecuteNonQuery(sql);

                    if (Conx.Error)
                    {
                        MessageBoxDialog.Show($"Failed to delete icon: {Conx.LastMessage}", "Icons",
                            MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    }
                    else
                    {
                        MessageBoxDialog.Show("Icon deleted successfully!", "Icons",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                        LoadIcons();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error deleting icon: {ex.Message}", "Icons",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadIcons();
        }

        private void btnAddToBuffer_Click(object sender, EventArgs e)
        {
            try
            {
                int? iconId = GetSelectedId();
                if (!iconId.HasValue)
                {
                    MessageBoxDialog.Show("Please select an icon to add to the buffer.", "Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Get all IconFiles for this icon
                string sql = $"SELECT Id FROM IconFiles WHERE Icon = {iconId.Value}";
                var response = Conx.ExecuteTable(sql);

                if (response.IsOK && response.Result.Rows.Count > 0)
                {
                    int addedCount = 0;
                    foreach (DataRow row in response.Result.Rows)
                    {
                        int iconFileId = Convert.ToInt32(row["Id"]);

                        // Check if already in buffer
                        string checkSql = $"SELECT COUNT(*) FROM IconBuffer WHERE IconFile = {iconFileId}";
                        var checkResponse = Conx.ExecuteScalar(checkSql);

                        int existingCount = checkResponse.IsOK && checkResponse.Result != null
                            ? Convert.ToInt32(checkResponse.Result)
                            : 0;

                        if (existingCount == 0)
                        {
                            // Add to buffer
                            string insertSql = $@"INSERT INTO IconBuffer (IconFile, CreationDate)
                                                VALUES ({iconFileId}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}')";
                            Conx.ExecuteNonQuery(insertSql);
                            addedCount++;
                        }
                    }

                    if (addedCount > 0)
                    {
                        MessageBoxDialog.Show($"{addedCount} icon file(s) added to buffer!", "Icons",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    }
                    else
                    {
                        MessageBoxDialog.Show("All icon files are already in the buffer.", "Icons",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    }
                }
                else
                {
                    MessageBoxDialog.Show("No icon files found for this icon.", "Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error adding to buffer: {ex.Message}", "Icons",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }
    }
}
