using IconCommander.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
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

            // Wire up the grid events for tag loading
            zidGrid1.Click += zidGrid1_Click;
            zidGrid1.KeyUp += zidGrid1_KeyUp;

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

        private void zidGrid1_Click(object sender, EventArgs e)
        {
            LoadTagsForSelectedIcon();
        }

        private void zidGrid1_KeyUp(object sender, KeyEventArgs e)
        {
            // Load tags when navigating with keyboard
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown ||
                e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
            {
                LoadTagsForSelectedIcon();
            }
        }

        private void LoadTagsForSelectedIcon()
        {
            try
            {
                lstTags.Items.Clear();
                txtNewTag.Clear();

                int? iconId = GetSelectedId();
                if (!iconId.HasValue)
                {
                    lblSelectedIcon.Text = "(No icon selected)";
                    return;
                }

                // Get icon name
                DataRow selectedRow = GetSelectedRow();
                string iconName = selectedRow["Name"]?.ToString() ?? "Unknown";
                lblSelectedIcon.Text = iconName;

                // Load tags for this icon
                string sql = $@"SELECT Tag FROM IconTags
                               WHERE Icon = {iconId.Value}
                               ORDER BY Tag";

                var response = Conx.ExecuteTable(sql);

                if (response.IsOK)
                {
                    foreach (DataRow row in response.Result.Rows)
                    {
                        lstTags.Items.Add(row["Tag"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error loading tags: {ex.Message}", "Tag Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void btnAddNewTag_Click(object sender, EventArgs e)
        {
            AddNewTag();
        }

        private void txtNewTag_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                AddNewTag();
            }
        }

        private void AddNewTag()
        {
            try
            {
                int? iconId = GetSelectedId();
                if (!iconId.HasValue)
                {
                    MessageBoxDialog.Show("Please select an icon first.", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                string tag = txtNewTag.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(tag))
                {
                    MessageBoxDialog.Show("Please enter a tag.", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Check if tag already exists for this icon
                string checkSql = $@"SELECT COUNT(*) FROM IconTags
                                    WHERE Icon = {iconId.Value}
                                    AND LOWER(Tag) = '{tag.Replace("'", "''")}'";
                var checkResponse = Conx.ExecuteScalar(checkSql);

                int existingCount = checkResponse.IsOK && checkResponse.Result != null
                    ? Convert.ToInt32(checkResponse.Result)
                    : 0;

                if (existingCount > 0)
                {
                    MessageBoxDialog.Show($"Tag '{tag}' already exists for this icon.", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    return;
                }

                // Add the tag
                string insertSql = $@"INSERT INTO IconTags (Icon, Tag)
                                     VALUES ({iconId.Value}, '{tag.Replace("'", "''")}')";
                var insertResponse = Conx.ExecuteNonQuery(insertSql);

                if (insertResponse.IsOK)
                {
                    txtNewTag.Clear();
                    LoadTagsForSelectedIcon();
                    MessageBoxDialog.Show($"Tag '{tag}' added successfully!", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                }
                else
                {
                    MessageBoxDialog.Show($"Failed to add tag: {Conx.LastMessage}", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error adding tag: {ex.Message}", "Tag Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void btnRemoveTag_Click(object sender, EventArgs e)
        {
            try
            {
                int? iconId = GetSelectedId();
                if (!iconId.HasValue)
                {
                    MessageBoxDialog.Show("Please select an icon first.", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                if (lstTags.SelectedItems.Count == 0)
                {
                    MessageBoxDialog.Show("Please select one or more tags to remove.", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Confirm deletion
                string tagsList = string.Join(", ", lstTags.SelectedItems.Cast<string>());
                DialogResult confirm = MessageBoxDialog.Show(
                    $"Are you sure you want to remove the following tag(s)?\n\n{tagsList}",
                    "Remove Tags",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    theme);

                if (confirm != DialogResult.Yes)
                    return;

                int removedCount = 0;
                foreach (string tag in lstTags.SelectedItems)
                {
                    string deleteSql = $@"DELETE FROM IconTags
                                         WHERE Icon = {iconId.Value}
                                         AND LOWER(Tag) = '{tag.ToLower().Replace("'", "''")}'";
                    var response = Conx.ExecuteNonQuery(deleteSql);

                    if (response.IsOK)
                        removedCount++;
                }

                LoadTagsForSelectedIcon();
                MessageBoxDialog.Show($"{removedCount} tag(s) removed successfully!", "Tag Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error removing tags: {ex.Message}", "Tag Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void btnAddExistingTag_Click(object sender, EventArgs e)
        {
            try
            {
                int? iconId = GetSelectedId();
                if (!iconId.HasValue)
                {
                    MessageBoxDialog.Show("Please select an icon first.", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Get all distinct tags from the database
                string sql = "SELECT DISTINCT Tag FROM IconTags ORDER BY Tag";
                var response = Conx.ExecuteTable(sql);

                if (!response.IsOK || response.Result.Rows.Count == 0)
                {
                    MessageBoxDialog.Show("No existing tags found in the database.", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    return;
                }

                // Get tags already assigned to this icon
                string existingTagsSql = $@"SELECT Tag FROM IconTags
                                           WHERE Icon = {iconId.Value}";
                var existingResponse = Conx.ExecuteTable(existingTagsSql);

                HashSet<string> existingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (existingResponse.IsOK)
                {
                    foreach (DataRow row in existingResponse.Result.Rows)
                    {
                        existingTags.Add(row["Tag"].ToString());
                    }
                }

                // Create selection dialog
                List<string> allTags = new List<string>();
                foreach (DataRow row in response.Result.Rows)
                {
                    string tag = row["Tag"].ToString();
                    if (!existingTags.Contains(tag))
                    {
                        allTags.Add(tag);
                    }
                }

                if (allTags.Count == 0)
                {
                    MessageBoxDialog.Show("All existing tags are already assigned to this icon.", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    return;
                }

                // Show multi-selection dialog
                var selectedTags = ShowTagSelectionDialog(allTags);

                if (selectedTags != null && selectedTags.Count > 0)
                {
                    int addedCount = 0;
                    foreach (string tag in selectedTags)
                    {
                        string insertSql = $@"INSERT INTO IconTags (Icon, Tag)
                                             VALUES ({iconId.Value}, '{tag.Replace("'", "''")}')";
                        var insertResponse = Conx.ExecuteNonQuery(insertSql);

                        if (insertResponse.IsOK)
                            addedCount++;
                    }

                    LoadTagsForSelectedIcon();
                    MessageBoxDialog.Show($"{addedCount} tag(s) added successfully!", "Tag Manager",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error adding existing tags: {ex.Message}", "Tag Manager",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private List<string> ShowTagSelectionDialog(List<string> availableTags)
        {
            Form selectionForm = new Form();
            ListBox listBox = new ListBox();
            Button btnOk = new Button();
            Button btnCancel = new Button();
            Label label = new Label();

            selectionForm.Text = "Select Tags";
            label.Text = $"Select one or more tags to add ({availableTags.Count} available):";

            listBox.SelectionMode = SelectionMode.MultiExtended;
            listBox.Items.AddRange(availableTags.ToArray());

            btnOk.Text = "OK";
            btnCancel.Text = "Cancel";
            btnOk.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(12, 12, 460, 20);
            listBox.SetBounds(12, 40, 460, 300);
            btnOk.SetBounds(316, 350, 75, 23);
            btnCancel.SetBounds(397, 350, 75, 23);

            selectionForm.ClientSize = new Size(484, 385);
            selectionForm.Controls.AddRange(new Control[] { label, listBox, btnOk, btnCancel });
            selectionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            selectionForm.StartPosition = FormStartPosition.CenterParent;
            selectionForm.MinimizeBox = false;
            selectionForm.MaximizeBox = false;
            selectionForm.AcceptButton = btnOk;
            selectionForm.CancelButton = btnCancel;

            DialogResult result = selectionForm.ShowDialog();

            if (result == DialogResult.OK && listBox.SelectedItems.Count > 0)
            {
                return listBox.SelectedItems.Cast<string>().ToList();
            }

            return null;
        }

        private void btnCompareTags_Click(object sender, EventArgs e)
        {
            try
            {
                int? iconId1 = GetSelectedId();
                if (!iconId1.HasValue)
                {
                    MessageBoxDialog.Show("Please select an icon first.", "Compare Tags",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Get the first icon's name
                DataRow selectedRow = GetSelectedRow();
                string icon1Name = selectedRow["Name"]?.ToString() ?? "Unknown";

                // Get all icons for selection
                string sql = "SELECT Id, Name FROM Icons WHERE Id != " + iconId1.Value + " ORDER BY Name";
                var response = Conx.ExecuteTable(sql);

                if (!response.IsOK || response.Result.Rows.Count == 0)
                {
                    MessageBoxDialog.Show("No other icons found to compare with.", "Compare Tags",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    return;
                }

                // Show icon selection dialog
                var selectedIcon = ShowIconSelectionDialog(response.Result);

                if (selectedIcon.HasValue)
                {
                    // Get tags for both icons
                    string tags1Sql = $@"SELECT Tag FROM IconTags
                                        WHERE Icon = {iconId1.Value}
                                        ORDER BY Tag";
                    var tags1Response = Conx.ExecuteTable(tags1Sql);

                    string tags2Sql = $@"SELECT Tag FROM IconTags
                                        WHERE Icon = {selectedIcon.Value}
                                        ORDER BY Tag";
                    var tags2Response = Conx.ExecuteTable(tags2Sql);

                    // Get icon2 name
                    string icon2NameSql = $"SELECT Name FROM Icons WHERE Id = {selectedIcon.Value}";
                    var icon2NameResponse = Conx.ExecuteScalar(icon2NameSql);
                    string icon2Name = icon2NameResponse.IsOK && icon2NameResponse.Result != null
                        ? icon2NameResponse.Result.ToString()
                        : "Unknown";

                    // Build comparison lists
                    HashSet<string> tags1 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    HashSet<string> tags2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    if (tags1Response.IsOK)
                    {
                        foreach (DataRow row in tags1Response.Result.Rows)
                            tags1.Add(row["Tag"].ToString());
                    }

                    if (tags2Response.IsOK)
                    {
                        foreach (DataRow row in tags2Response.Result.Rows)
                            tags2.Add(row["Tag"].ToString());
                    }

                    // Calculate differences
                    var onlyInIcon1 = tags1.Except(tags2, StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
                    var onlyInIcon2 = tags2.Except(tags1, StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
                    var inBoth = tags1.Intersect(tags2, StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();

                    // Show comparison dialog
                    ShowTagComparisonDialog(icon1Name, icon2Name, onlyInIcon1, onlyInIcon2, inBoth);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error comparing tags: {ex.Message}", "Compare Tags",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private int? ShowIconSelectionDialog(DataTable icons)
        {
            Form selectionForm = new Form();
            ListBox listBox = new ListBox();
            Button btnOk = new Button();
            Button btnCancel = new Button();
            Label label = new Label();

            selectionForm.Text = "Select Icon to Compare";
            label.Text = "Select an icon to compare tags with:";

            foreach (DataRow row in icons.Rows)
            {
                ComboboxItem item = new ComboboxItem
                {
                    Text = row["Name"].ToString(),
                    Value = row["Id"]
                };
                listBox.Items.Add(item);
            }

            btnOk.Text = "OK";
            btnCancel.Text = "Cancel";
            btnOk.DialogResult = DialogResult.OK;
            btnCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(12, 12, 460, 20);
            listBox.SetBounds(12, 40, 460, 300);
            btnOk.SetBounds(316, 350, 75, 23);
            btnCancel.SetBounds(397, 350, 75, 23);

            selectionForm.ClientSize = new Size(484, 385);
            selectionForm.Controls.AddRange(new Control[] { label, listBox, btnOk, btnCancel });
            selectionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            selectionForm.StartPosition = FormStartPosition.CenterParent;
            selectionForm.MinimizeBox = false;
            selectionForm.MaximizeBox = false;
            selectionForm.AcceptButton = btnOk;
            selectionForm.CancelButton = btnCancel;

            DialogResult result = selectionForm.ShowDialog();

            if (result == DialogResult.OK && listBox.SelectedItem != null)
            {
                return Convert.ToInt32(((ComboboxItem)listBox.SelectedItem).Value);
            }

            return null;
        }

        private void ShowTagComparisonDialog(string icon1Name, string icon2Name,
            List<string> onlyInIcon1, List<string> onlyInIcon2, List<string> inBoth)
        {
            Form comparisonForm = new Form();
            Label lblIcon1 = new Label();
            Label lblIcon2 = new Label();
            Label lblCommon = new Label();
            ListBox lstIcon1Tags = new ListBox();
            ListBox lstIcon2Tags = new ListBox();
            ListBox lstCommonTags = new ListBox();
            Button btnClose = new Button();
            Label lblStats = new Label();

            comparisonForm.Text = "Tag Comparison";

            lblIcon1.Text = $"Only in '{icon1Name}' ({onlyInIcon1.Count}):";
            lblIcon2.Text = $"Only in '{icon2Name}' ({onlyInIcon2.Count}):";
            lblCommon.Text = $"Common tags ({inBoth.Count}):";

            lstIcon1Tags.Items.AddRange(onlyInIcon1.ToArray());
            lstIcon2Tags.Items.AddRange(onlyInIcon2.ToArray());
            lstCommonTags.Items.AddRange(inBoth.ToArray());

            lblStats.Text = $"Total tags in '{icon1Name}': {onlyInIcon1.Count + inBoth.Count} | " +
                           $"Total tags in '{icon2Name}': {onlyInIcon2.Count + inBoth.Count} | " +
                           $"Similarity: {CalculateSimilarity(onlyInIcon1.Count + inBoth.Count, onlyInIcon2.Count + inBoth.Count, inBoth.Count):F1}%";

            btnClose.Text = "Close";
            btnClose.DialogResult = DialogResult.OK;

            lblIcon1.SetBounds(12, 12, 250, 20);
            lstIcon1Tags.SetBounds(12, 35, 250, 380);

            lblIcon2.SetBounds(272, 12, 250, 20);
            lstIcon2Tags.SetBounds(272, 35, 250, 380);

            lblCommon.SetBounds(532, 12, 250, 20);
            lstCommonTags.SetBounds(532, 35, 250, 380);

            lblStats.SetBounds(12, 425, 770, 20);
            btnClose.SetBounds(707, 455, 75, 23);

            comparisonForm.ClientSize = new Size(794, 490);
            comparisonForm.Controls.AddRange(new Control[] {
                lblIcon1, lstIcon1Tags, lblIcon2, lstIcon2Tags,
                lblCommon, lstCommonTags, lblStats, btnClose
            });
            comparisonForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            comparisonForm.StartPosition = FormStartPosition.CenterParent;
            comparisonForm.MinimizeBox = false;
            comparisonForm.MaximizeBox = false;
            comparisonForm.AcceptButton = btnClose;

            comparisonForm.ShowDialog();
        }

        private double CalculateSimilarity(int total1, int total2, int common)
        {
            if (total1 == 0 && total2 == 0)
                return 100.0;

            if (total1 == 0 || total2 == 0)
                return 0.0;

            // Jaccard similarity: intersection / union
            int union = total1 + total2 - common;
            return (double)common / union * 100.0;
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

        private void btnAddTags_Click(object sender, EventArgs e)
        {
            try
            {
                int? iconId = GetSelectedId();
                if (!iconId.HasValue)
                {
                    MessageBoxDialog.Show("Please select an icon to add tags.", "Add Tags",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Get icon name for display
                DataRow selectedRow = GetSelectedRow();
                string iconName = selectedRow["Name"]?.ToString() ?? "Unknown";

                // Show input dialog for tags
                string input = ShowInputDialog(
                    $"Enter tags for icon '{iconName}' (comma or space-separated):",
                    "Add Tags");

                if (string.IsNullOrWhiteSpace(input))
                {
                    return; // User cancelled or entered nothing
                }

                // Parse tags - split by comma, space, semicolon, or pipe
                char[] separators = new char[] { ',', ' ', ';', '|', '\t' };
                string[] tags = input.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                if (tags.Length == 0)
                {
                    MessageBoxDialog.Show("No valid tags entered.", "Add Tags",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    return;
                }

                int addedCount = 0;
                int duplicateCount = 0;

                foreach (string tag in tags)
                {
                    string cleanTag = tag.Trim().ToLower();
                    if (string.IsNullOrEmpty(cleanTag))
                        continue;

                    // Check if tag already exists for this icon
                    string checkSql = $@"SELECT COUNT(*) FROM IconTags
                                        WHERE Icon = {iconId.Value}
                                        AND LOWER(Tag) = '{cleanTag.Replace("'", "''")}'";
                    var checkResponse = Conx.ExecuteScalar(checkSql);

                    int existingCount = checkResponse.IsOK && checkResponse.Result != null
                        ? Convert.ToInt32(checkResponse.Result)
                        : 0;

                    if (existingCount == 0)
                    {
                        // Add the tag
                        string insertSql = $@"INSERT INTO IconTags (Icon, Tag)
                                            VALUES ({iconId.Value}, '{cleanTag.Replace("'", "''")}')";
                        var insertResponse = Conx.ExecuteNonQuery(insertSql);

                        if (insertResponse.IsOK)
                            addedCount++;
                    }
                    else
                    {
                        duplicateCount++;
                    }
                }

                // Show results
                string message = $"{addedCount} tag(s) added successfully!";
                if (duplicateCount > 0)
                {
                    message += $"\n{duplicateCount} tag(s) already existed and were skipped.";
                }

                MessageBoxDialog.Show(message, "Add Tags",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error adding tags: {ex.Message}", "Add Tags",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        /// <summary>
        /// Shows a simple input dialog for text entry
        /// </summary>
        private string ShowInputDialog(string prompt, string title)
        {
            Form inputForm = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            inputForm.Text = title;
            label.Text = prompt;
            textBox.Text = "";

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 50, 372, 20);
            buttonOk.SetBounds(228, 92, 75, 23);
            buttonCancel.SetBounds(309, 92, 75, 23);

            label.AutoSize = true;
            inputForm.ClientSize = new Size(396, 127);
            inputForm.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.MinimizeBox = false;
            inputForm.MaximizeBox = false;
            inputForm.AcceptButton = buttonOk;
            inputForm.CancelButton = buttonCancel;

            DialogResult dialogResult = inputForm.ShowDialog();
            return dialogResult == DialogResult.OK ? textBox.Text : null;
        }

        private void zidGrid1_OnSelectionChanged(object sender, EventArgs e)
        {
            if(zidGrid1.GridControl.SelectedRows != null && zidGrid1.GridControl.SelectedRows.Count > 0)
                LoadTagsForSelectedIcon();
        }
    }
}
