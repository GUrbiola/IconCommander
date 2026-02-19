using IconCommander.DataAccess;
using IconCommander.Models;
using Svg;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    /// <summary>
    /// Standalone dialog for browsing and managing the icon buffer zone.
    /// Displays buffered icons in a list with a live preview panel, and provides
    /// operations to remove items, clear the buffer, merge two icons via
    /// <see cref="MergeForm"/>, or export selected icons to the currently active
    /// <see cref="Project"/> using <see cref="ExportManager"/>.
    /// </summary>
    public partial class IconBufferForm : Form
    {
        private string connectionString;
        private ZidThemes theme;
        private IIconCommanderDb Conx;
        private DataTable bufferData;
        private Project currentProject;

        /// <summary>
        /// Initialises the form, stores connection details, and creates the appropriate
        /// database connector based on the <c>IsSqlite</c> application setting.
        /// </summary>
        /// <param name="dbConnectionString">Active database connection string.</param>
        /// <param name="currentTheme">Theme to apply to this form.</param>
        /// <param name="project">Currently selected project used for export operations; may be <c>null</c>.</param>
        public IconBufferForm(string dbConnectionString, ZidThemes currentTheme, Project project = null)
        {
            InitializeComponent();
            connectionString = dbConnectionString;
            theme = currentTheme;
            currentProject = project;

            if (Properties.Settings.Default.IsSqlite)
                Conx = new SqliteConnector();
            else
                Conx = new SqlConnector();

            Conx.Initialize(connectionString);
        }

        /// <summary>Applies the active theme and performs the initial buffer load.</summary>
        private void IconBufferForm_Load(object sender, EventArgs e)
        {
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            LoadBuffer();
        }

        /// <summary>
        /// Queries the <c>BufferZone</c> table with full JOIN context (IconFiles, Icons, Veins, Collections)
        /// and populates <c>lstBuffer</c>. Each entry shows the filename, approximate dimensions,
        /// collection, and vein. On empty result, shows an informational message.
        /// </summary>
        private void LoadBuffer()
        {
            try
            {
                string sql = @"
SELECT
    ib.Id AS BufferId,
    ib.IconFile,
    if_.FileName,
    if_.Extension,
    if_.Type,
    if_.Size,
    if_.BinData,
    i.Name AS IconName,
    v.Name AS VeinName,
    c.Name AS CollectionName
FROM 
	dbo.BufferZone ib INNER JOIN IconFiles if_ ON ib.IconFile = if_.Id
	INNER JOIN Icons i ON if_.Icon = i.Id
	INNER JOIN Veins v ON i.Vein = v.Id
	INNER JOIN Collections c ON v.Collection = c.Id
ORDER BY
	ib.CreationDate DESC   
";

                var response = Conx.ExecuteTable(sql);

                if (response.IsOK)
                {
                    bufferData = response.Result;
                    lstBuffer.Items.Clear();

                    if (bufferData.Rows.Count == 0)
                    {
                        lblCount.Text = "0";
                        MessageBoxDialog.Show("The icon buffer is empty.\n\nTo add icons to the buffer, you can import them using:\nIcons → Import Icons...\n\nOr they will be added automatically during vein imports.",
                            "Icon Buffer", MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                        UpdateButtons();
                        return;
                    }

                    foreach (DataRow row in bufferData.Rows)
                    {
                        string fileName = row["FileName"].ToString();
                        string extension = row["Extension"].ToString();
                        string collectionName = row["CollectionName"].ToString();
                        string veinName = row["VeinName"].ToString();
                        int width = 0;
                        int height = 0;

                        // Try to get dimensions from Size field (width * height)
                        if (row["Size"] != DBNull.Value)
                        {
                            int size = Convert.ToInt32(row["Size"]);
                            // Approximate square root for display
                            width = (int)Math.Sqrt(size);
                            height = width;
                        }

                        string displayText = $"{fileName}{extension} ({width}x{height}) - {collectionName}/{veinName}";
                        lstBuffer.Items.Add(displayText);
                    }

                    lblCount.Text = bufferData.Rows.Count.ToString();
                    UpdateButtons();
                }
                else
                {
                    string errors = response.Errors != null && response.Errors.Count > 0
                        ? string.Join("\n", response.Errors.Select(e => e.Message))
                        : "Unknown error";
                    MessageBoxDialog.Show($"Error loading buffer data:\n{errors}", "Icon Buffer",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error loading buffer: {ex.Message}", "Icon Buffer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        /// <summary>
        /// Enables or disables action buttons based on the current list selection and context:
        /// Remove requires ≥ 1 selection; Merge requires exactly 2; Export requires ≥ 1 selection
        /// and a non-null <see cref="currentProject"/>; Clear requires at least one buffered item.
        /// </summary>
        private void UpdateButtons()
        {
            int selectedCount = lstBuffer.SelectedIndices.Count;
            btnRemove.Enabled = selectedCount > 0;
            btnMerge.Enabled = selectedCount == 2;
            btnExport.Enabled = selectedCount > 0 && currentProject != null;
            btnClear.Enabled = bufferData.Rows.Count > 0;
        }

        /// <summary>
        /// Updates button states and renders a preview of the selected icon in <c>picPreview</c>.
        /// SVG files are rasterised at 256 × 256 px; all other formats are loaded as raster images.
        /// </summary>
        private void lstBuffer_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();

            // Show preview of selected icon
            if (lstBuffer.SelectedIndex >= 0 && lstBuffer.SelectedIndex < bufferData.Rows.Count)
            {
                try
                {
                    DataRow row = bufferData.Rows[lstBuffer.SelectedIndex];
                    string extension = row["Extension"].ToString();
                    byte[] imageData = (byte[])row["BinData"];

                    // Check if this is an SVG file
                    if (extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals("svg", StringComparison.OrdinalIgnoreCase))
                    {
                        // Load and render SVG
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.LoadXml(Encoding.UTF8.GetString(imageData));
                        SvgDocument svgDoc = SvgDocument.Open(xdoc);

                        // Resize to fit preview box (256x256)
                        svgDoc.Width = new SvgUnit(SvgUnitType.Pixel, 256);
                        svgDoc.Height = new SvgUnit(SvgUnitType.Pixel, 256);

                        picPreview.Image = svgDoc.Draw();
                    }
                    else
                    {
                        // Load raster image
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            Image img = Image.FromStream(ms);
                            picPreview.Image = new Bitmap(img);
                        }
                    }
                }
                catch (Exception ex)
                {
                    picPreview.Image = null;
                    lblPreview.Text = $"Preview error: {ex.Message}";
                }
            }
            else
            {
                picPreview.Image = null;
            }
        }

        /// <summary>Prompts for confirmation then deletes selected buffer entries from <c>IconBuffer</c> and reloads the list.</summary>
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstBuffer.SelectedIndices.Count == 0)
                return;

            DialogResult result = MessageBoxDialog.Show(
                $"Remove {lstBuffer.SelectedIndices.Count} item(s) from buffer?",
                "Icon Buffer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                theme);

            if (result == DialogResult.Yes)
            {
                List<int> selectedIndices = new List<int>();
                foreach (int index in lstBuffer.SelectedIndices)
                {
                    selectedIndices.Add(index);
                }

                foreach (int index in selectedIndices.OrderByDescending(i => i))
                {
                    DataRow row = bufferData.Rows[index];
                    int bufferId = Convert.ToInt32(row["BufferId"]);

                    string sql = $"DELETE FROM IconBuffer WHERE Id = {bufferId}";
                    Conx.ExecuteNonQuery(sql);
                }

                LoadBuffer();
            }
        }

        /// <summary>Prompts for confirmation then truncates the entire <c>IconBuffer</c> table and reloads the list.</summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBoxDialog.Show(
                "Clear all items from buffer?",
                "Icon Buffer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                theme);

            if (result == DialogResult.Yes)
            {
                Conx.ExecuteNonQuery("DELETE FROM IconBuffer");
                LoadBuffer();
            }
        }

        /// <summary>
        /// Requires exactly 2 selected icons. Automatically assigns the larger icon (by <c>Size</c>)
        /// as the base and the smaller as the overlay, then opens <see cref="MergeForm"/>.
        /// Reloads the buffer if the merge completes successfully.
        /// </summary>
        private void btnMerge_Click(object sender, EventArgs e)
        {
            if (lstBuffer.SelectedIndices.Count != 2)
            {
                MessageBoxDialog.Show("Please select exactly 2 icons to merge.", "Icon Buffer",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                return;
            }

            try
            {
                // Get the two selected items
                List<int> indices = new List<int>();
                foreach (int index in lstBuffer.SelectedIndices)
                {
                    indices.Add(index);
                }

                if (bufferData == null || bufferData.Rows.Count == 0)
                {
                    MessageBoxDialog.Show("Buffer data is not loaded. Please refresh and try again.", "Icon Buffer",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    return;
                }

                DataRow row1 = bufferData.Rows[indices[0]];
                DataRow row2 = bufferData.Rows[indices[1]];

                // Determine which is bigger (by Size field)
                int size1 = row1["Size"] == DBNull.Value ? 0 : Convert.ToInt32(row1["Size"]);
                int size2 = row2["Size"] == DBNull.Value ? 0 : Convert.ToInt32(row2["Size"]);

                if (size1 == 0 && size2 == 0)
                {
                    MessageBoxDialog.Show("Cannot determine icon sizes. Both icons have size = 0.", "Icon Buffer",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                DataRow bigRow = size1 >= size2 ? row1 : row2;
                DataRow smallRow = size1 >= size2 ? row2 : row1;

                int bigIconFileId = Convert.ToInt32(bigRow["IconFile"]);
                int smallIconFileId = Convert.ToInt32(smallRow["IconFile"]);

                string bigName = bigRow["FileName"].ToString();
                string smallName = smallRow["FileName"].ToString();

                if (bigRow["BinData"] == DBNull.Value || smallRow["BinData"] == DBNull.Value)
                {
                    MessageBoxDialog.Show("One or both icons have no binary data. Cannot merge.", "Icon Buffer",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    return;
                }

                byte[] bigData = (byte[])bigRow["BinData"];
                byte[] smallData = (byte[])smallRow["BinData"];

                // Open merge dialog
                MergeForm mergeForm = new MergeForm(connectionString, theme,
                    bigIconFileId, smallIconFileId,
                    bigName, smallName,
                    bigData, smallData);

                DialogResult result = mergeForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    MessageBoxDialog.Show("Icons merged successfully!", "Icon Buffer",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    LoadBuffer();
                }
                else if (result == DialogResult.Cancel)
                {
                    // User cancelled, no message needed
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error merging icons:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Icon Buffer",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        /// <summary>Reloads the buffer list from the database.</summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBuffer();
        }

        /// <summary>Closes this dialog.</summary>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Exports selected buffer icons to <see cref="currentProject"/> via <see cref="ExportManager"/>.
        /// Shows a confirmation summary before proceeding and a detailed results message afterwards,
        /// including file counts, resource additions, project file updates, and any warnings.
        /// </summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (lstBuffer.SelectedIndices.Count == 0)
            {
                MessageBoxDialog.Show("Please select one or more icons to export.", "Icon Buffer",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                return;
            }

            if (currentProject == null)
            {
                MessageBoxDialog.Show("No project is currently selected.\n\nPlease create or open a project first from the main menu:\nProject → Create Project... or Project → Open Project...",
                    "Icon Buffer", MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                return;
            }

            try
            {
                // Collect selected buffer rows
                List<DataRow> selectedRows = new List<DataRow>();
                foreach (int index in lstBuffer.SelectedIndices)
                {
                    if (index >= 0 && index < bufferData.Rows.Count)
                    {
                        selectedRows.Add(bufferData.Rows[index]);
                    }
                }

                if (selectedRows.Count == 0)
                {
                    MessageBoxDialog.Show("No valid icons selected.", "Icon Buffer",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                    return;
                }

                // Show confirmation
                string message = $"Export {selectedRows.Count} icon(s) to project:\n\n" +
                                $"Project: {currentProject.Name}\n" +
                                $"Type: {currentProject.Type}\n" +
                                $"Path: {currentProject.Path}\n\n" +
                                $"Continue?";

                DialogResult confirmResult = MessageBoxDialog.Show(message, "Export Icons",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, theme);

                if (confirmResult != DialogResult.Yes)
                    return;

                // Perform export
                ExportManager exportManager = new ExportManager(Conx);
                ExportResult result = exportManager.ExportSelectedIcons(currentProject, selectedRows);

                // Show results
                if (result.HasErrors)
                {
                    string errors = string.Join("\n", result.Errors);
                    MessageBoxDialog.Show($"Export failed with errors:\n\n{errors}",
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
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
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);

                    // Optionally refresh the buffer
                    LoadBuffer();
                }
                else
                {
                    MessageBoxDialog.Show("No icons were exported.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                }
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error during export:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }
    }
}
