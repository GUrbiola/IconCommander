using IconCommander.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    public partial class IconBufferForm : Form
    {
        private string connectionString;
        private ZidThemes theme;
        private IIconCommanderDb Conx;
        private DataTable bufferData;

        public IconBufferForm(string dbConnectionString, ZidThemes currentTheme)
        {
            InitializeComponent();
            connectionString = dbConnectionString;
            theme = currentTheme;

            if (Properties.Settings.Default.IsSqlite)
                Conx = new SqliteConnector();
            else
                Conx = new SqlConnector();

            Conx.Initialize(connectionString);
        }

        private void IconBufferForm_Load(object sender, EventArgs e)
        {
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            LoadBuffer();
        }

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

        private void UpdateButtons()
        {
            int selectedCount = lstBuffer.SelectedIndices.Count;
            btnRemove.Enabled = selectedCount > 0;
            btnMerge.Enabled = selectedCount == 2;
            btnClear.Enabled = bufferData.Rows.Count > 0;
        }

        private void lstBuffer_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();

            // Show preview of selected icon
            if (lstBuffer.SelectedIndex >= 0 && lstBuffer.SelectedIndex < bufferData.Rows.Count)
            {
                try
                {
                    DataRow row = bufferData.Rows[lstBuffer.SelectedIndex];
                    byte[] imageData = (byte[])row["BinData"];

                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        Image img = Image.FromStream(ms);
                        picPreview.Image = new Bitmap(img);
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBuffer();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
