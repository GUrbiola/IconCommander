using IconCommander.DataAccess;
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
using ZidUtilities.CommonCode.Win.Controls;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    public partial class CleanupUtilityForm : Form
    {
        private IIconCommanderDb connector;
        private ZidThemes theme;
        private DataTable corruptedFiles;

        public CleanupUtilityForm(IIconCommanderDb conx, ZidThemes currentTheme)
        {
            connector = conx;
            theme = currentTheme;
            InitializeComponent();
        }

        private void CleanupUtilityForm_Load(object sender, EventArgs e)
        {
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            ScanForCorruptedFiles();
        }

        private void ScanForCorruptedFiles()
        {
            lstLog.Items.Clear();
            corruptedFiles = new DataTable();
            corruptedFiles.Columns.Add("IconFileId", typeof(int));
            corruptedFiles.Columns.Add("IconId", typeof(int));
            corruptedFiles.Columns.Add("FileName", typeof(string));
            corruptedFiles.Columns.Add("Extension", typeof(string));
            corruptedFiles.Columns.Add("Size", typeof(int));
            corruptedFiles.Columns.Add("OriginalPath", typeof(string));
            corruptedFiles.Columns.Add("Reason", typeof(string));

            btnScan.Enabled = false;
            btnDelete.Enabled = false;
            btnRepair.Enabled = false;
            progressBar.Value = 0;

            try
            {
                lstLog.Items.Add("Starting scan...");
                Application.DoEvents();

                // Get all icon files
                string sql = @"SELECT f.Id, f.Icon, f.FileName, f.Extension, f.Size, f.OriginalPath, f.BinData, i.Name as IconName
                              FROM IconFiles f
                              LEFT JOIN Icons i ON f.Icon = i.Id
                              ORDER BY f.Id";

                var response = connector.ExecuteTable(sql);

                if (!response.IsOK)
                {
                    lstLog.Items.Add($"Error querying database: {response.Errors[0].Message}");
                    return;
                }

                int totalFiles = response.Result.Rows.Count;
                int validCount = 0;
                int corruptedCount = 0;
                int emptyCount = 0;

                lstLog.Items.Add($"Found {totalFiles} icon files to check...");
                progressBar.Maximum = totalFiles;
                Application.DoEvents();

                foreach (DataRow row in response.Result.Rows)
                {
                    int iconFileId = Convert.ToInt32(row["Id"]);
                    int iconId = row["Icon"] != DBNull.Value ? Convert.ToInt32(row["Icon"]) : 0;
                    string fileName = row["FileName"]?.ToString() ?? "Unknown";
                    string extension = row["Extension"]?.ToString() ?? "";
                    int size = row["Size"] != DBNull.Value ? Convert.ToInt32(row["Size"]) : 0;
                    string originalPath = row["OriginalPath"]?.ToString() ?? "";
                    byte[] binData = row["BinData"] as byte[];
                    string iconName = row["IconName"]?.ToString() ?? "Unknown";

                    progressBar.Value++;
                    if (progressBar.Value % 100 == 0)
                    {
                        lblProgress.Text = $"Scanning: {progressBar.Value} / {totalFiles}";
                        Application.DoEvents();
                    }

                    // Check if data is null or empty
                    if (binData == null || binData.Length == 0)
                    {
                        emptyCount++;
                        corruptedFiles.Rows.Add(iconFileId, iconId, iconName, extension, size, originalPath, "Empty or null binary data");
                        continue;
                    }

                    // Check if this is an SVG file - validate differently
                    if (extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) ||
                        extension.Equals("svg", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            // Validate SVG by parsing as XML and loading with Svg.NET
                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(Encoding.UTF8.GetString(binData));
                            SvgDocument svgDoc = SvgDocument.Open(xdoc);

                            // Successfully loaded SVG
                            validCount++;
                        }
                        catch (Exception ex)
                        {
                            // SVG is corrupted
                            corruptedCount++;
                            string reason = ex.Message.Length > 50 ? ex.Message.Substring(0, 50) + "..." : ex.Message;
                            corruptedFiles.Rows.Add(iconFileId, iconId, iconName, extension, size, originalPath, $"Invalid SVG: {reason}");
                        }
                    }
                    else
                    {
                        // Try to load as raster image (PNG, JPG, BMP, ICO, etc.)
                        try
                        {
                            using (MemoryStream ms = new MemoryStream(binData))
                            {
                                using (Image img = Image.FromStream(ms))
                                {
                                    // Image loaded successfully
                                    validCount++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            corruptedCount++;
                            string reason = ex.Message.Length > 50 ? ex.Message.Substring(0, 50) + "..." : ex.Message;
                            corruptedFiles.Rows.Add(iconFileId, iconId, iconName, extension, size, originalPath, reason);
                        }
                    }
                }

                progressBar.Value = totalFiles;
                lblProgress.Text = $"Scan complete";

                lstLog.Items.Add($"");
                lstLog.Items.Add($"=== SCAN RESULTS ===");
                lstLog.Items.Add($"Total files scanned: {totalFiles}");
                lstLog.Items.Add($"Valid files: {validCount}");
                lstLog.Items.Add($"Empty files: {emptyCount}");
                lstLog.Items.Add($"Corrupted files: {corruptedCount}");
                lstLog.Items.Add($"Total corrupted/empty: {corruptedFiles.Rows.Count}");

                if (corruptedFiles.Rows.Count > 0)
                {
                    lstLog.Items.Add($"");
                    lstLog.Items.Add($"Corrupted files details:");

                    int repairableCount = 0;
                    foreach (DataRow row in corruptedFiles.Rows)
                    {
                        string origPath = row["OriginalPath"]?.ToString() ?? "";
                        bool hasOriginal = !string.IsNullOrEmpty(origPath) && File.Exists(origPath);
                        if (hasOriginal) repairableCount++;

                        string marker = hasOriginal ? "[REPAIRABLE]" : "[NO ORIGINAL]";
                        lstLog.Items.Add($"  {marker} ID {row["IconFileId"]}: {row["FileName"]}{row["Extension"]} - {row["Reason"]}");
                    }

                    lstLog.Items.Add($"");
                    lstLog.Items.Add($"Repairable files (have OriginalPath): {repairableCount}");

                    btnDelete.Enabled = true;
                    btnRepair.Enabled = repairableCount > 0;
                }
            }
            catch (Exception ex)
            {
                lstLog.Items.Add($"Error during scan: {ex.Message}");
            }
            finally
            {
                btnScan.Enabled = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (corruptedFiles == null || corruptedFiles.Rows.Count == 0)
            {
                MessageBoxDialog.Show("No corrupted files to delete.", "Cleanup",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                return;
            }

            DialogResult confirm = MessageBoxDialog.Show(
                $"Are you sure you want to delete {corruptedFiles.Rows.Count} corrupted/empty icon files?\n\n" +
                "This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                theme);

            if (confirm != DialogResult.Yes)
                return;

            btnDelete.Enabled = false;
            btnScan.Enabled = false;
            progressBar.Value = 0;
            progressBar.Maximum = corruptedFiles.Rows.Count;

            try
            {
                lstLog.Items.Add("");
                lstLog.Items.Add("Starting deletion...");

                int deletedCount = 0;
                int failedCount = 0;

                foreach (DataRow row in corruptedFiles.Rows)
                {
                    int iconFileId = Convert.ToInt32(row["IconFileId"]);

                    try
                    {
                        string deleteSql = $"DELETE FROM IconFiles WHERE Id = {iconFileId}";
                        var deleteResponse = connector.ExecuteNonQuery(deleteSql);

                        if (deleteResponse.IsOK)
                        {
                            deletedCount++;
                        }
                        else
                        {
                            failedCount++;
                            lstLog.Items.Add($"Failed to delete ID {iconFileId}: {deleteResponse.Errors[0].Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        lstLog.Items.Add($"Error deleting ID {iconFileId}: {ex.Message}");
                    }

                    progressBar.Value++;
                    if (progressBar.Value % 10 == 0)
                    {
                        lblProgress.Text = $"Deleting: {progressBar.Value} / {corruptedFiles.Rows.Count}";
                        Application.DoEvents();
                    }
                }

                lblProgress.Text = "Deletion complete";
                lstLog.Items.Add("");
                lstLog.Items.Add("=== DELETION RESULTS ===");
                lstLog.Items.Add($"Successfully deleted: {deletedCount}");
                lstLog.Items.Add($"Failed: {failedCount}");

                MessageBoxDialog.Show(
                    $"Cleanup complete!\n\nDeleted: {deletedCount}\nFailed: {failedCount}",
                    "Cleanup Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    theme);

                // Clear corrupted files list
                corruptedFiles.Clear();
                btnDelete.Enabled = false;
            }
            catch (Exception ex)
            {
                lstLog.Items.Add($"Error during deletion: {ex.Message}");
            }
            finally
            {
                btnScan.Enabled = true;
            }
        }

        private void btnRepair_Click(object sender, EventArgs e)
        {
            if (corruptedFiles == null || corruptedFiles.Rows.Count == 0)
            {
                MessageBoxDialog.Show("No corrupted files to repair.", "Repair",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                return;
            }

            // Count repairable files
            int repairableCount = 0;
            foreach (DataRow row in corruptedFiles.Rows)
            {
                string origPath = row["OriginalPath"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(origPath) && File.Exists(origPath))
                    repairableCount++;
            }

            if (repairableCount == 0)
            {
                MessageBoxDialog.Show("No repairable files found (missing OriginalPath or files don't exist).", "Repair",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                return;
            }

            DialogResult confirm = MessageBoxDialog.Show(
                $"Found {repairableCount} repairable file(s) with valid OriginalPath.\n\n" +
                "This will read data from the original files and update the database.\n\n" +
                "Continue with repair?",
                "Confirm Repair",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                theme);

            if (confirm != DialogResult.Yes)
                return;

            btnRepair.Enabled = false;
            btnDelete.Enabled = false;
            btnScan.Enabled = false;
            progressBar.Value = 0;
            progressBar.Maximum = repairableCount;

            try
            {
                lstLog.Items.Add("");
                lstLog.Items.Add("Starting repair...");

                int repairedCount = 0;
                int failedCount = 0;
                int skippedCount = 0;

                foreach (DataRow row in corruptedFiles.Rows)
                {
                    int iconFileId = Convert.ToInt32(row["IconFileId"]);
                    string fileName = row["FileName"]?.ToString() ?? "Unknown";
                    string extension = row["Extension"]?.ToString() ?? "";
                    string originalPath = row["OriginalPath"]?.ToString() ?? "";

                    // Skip if no original path or file doesn't exist
                    if (string.IsNullOrEmpty(originalPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    if (!File.Exists(originalPath))
                    {
                        skippedCount++;
                        lstLog.Items.Add($"Skipped ID {iconFileId}: Original file not found at {originalPath}");
                        continue;
                    }

                    try
                    {
                        // Read file from original path
                        byte[] fileData = File.ReadAllBytes(originalPath);

                        // Validate based on file type
                        bool isValid = false;

                        if (extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) ||
                            extension.Equals("svg", StringComparison.OrdinalIgnoreCase))
                        {
                            // Validate SVG
                            try
                            {
                                XmlDocument xdoc = new XmlDocument();
                                xdoc.LoadXml(Encoding.UTF8.GetString(fileData));
                                SvgDocument svgDoc = SvgDocument.Open(xdoc);
                                isValid = true;
                            }
                            catch
                            {
                                isValid = false;
                            }
                        }
                        else
                        {
                            // Validate raster image
                            try
                            {
                                using (MemoryStream ms = new MemoryStream(fileData))
                                {
                                    using (Image testImg = Image.FromStream(ms))
                                    {
                                        isValid = true;
                                    }
                                }
                            }
                            catch
                            {
                                isValid = false;
                            }
                        }

                        if (isValid)
                        {
                            // File is valid, calculate hash
                            string hash = CalculateHash(fileData);

                            // Update database using direct SQL command for binary data
                            bool updateSuccess = UpdateIconFileBinary(iconFileId, fileData, hash);

                            if (updateSuccess)
                            {
                                repairedCount++;
                            }
                            else
                            {
                                failedCount++;
                                lstLog.Items.Add($"Failed to update ID {iconFileId}");
                            }
                        }
                        else
                        {
                            failedCount++;
                            lstLog.Items.Add($"Failed to validate ID {iconFileId}: File at {originalPath} is not a valid image/SVG");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        lstLog.Items.Add($"Error repairing ID {iconFileId}: {ex.Message}");
                    }

                    progressBar.Value = repairedCount + failedCount;
                    if ((repairedCount + failedCount) % 10 == 0)
                    {
                        lblProgress.Text = $"Repairing: {repairedCount + failedCount} / {repairableCount}";
                        Application.DoEvents();
                    }
                }

                lblProgress.Text = "Repair complete";
                lstLog.Items.Add("");
                lstLog.Items.Add("=== REPAIR RESULTS ===");
                lstLog.Items.Add($"Successfully repaired: {repairedCount}");
                lstLog.Items.Add($"Failed: {failedCount}");
                lstLog.Items.Add($"Skipped (no original): {skippedCount}");

                MessageBoxDialog.Show(
                    $"Repair complete!\n\nRepaired: {repairedCount}\nFailed: {failedCount}\nSkipped: {skippedCount}",
                    "Repair Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    theme);

                // Clear corrupted files list and disable buttons
                corruptedFiles.Clear();
                btnDelete.Enabled = false;
                btnRepair.Enabled = false;
            }
            catch (Exception ex)
            {
                lstLog.Items.Add($"Error during repair: {ex.Message}");
            }
            finally
            {
                btnScan.Enabled = true;
            }
        }

        private bool UpdateIconFileBinary(int iconFileId, byte[] binData, string hash)
        {
            try
            {
                // Use SqliteConnector if available, otherwise generic interface
                if (connector is SqliteConnector sqliteConn)
                {
                    using (var conn = new System.Data.SQLite.SQLiteConnection(sqliteConn.ConnectionString))
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "UPDATE IconFiles SET BinData = @binData, Hash = @hash WHERE Id = @id";
                            cmd.Parameters.Add("@binData", System.Data.DbType.Binary).Value = binData;
                            cmd.Parameters.AddWithValue("@hash", hash);
                            cmd.Parameters.AddWithValue("@id", iconFileId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return true;
                }
                else
                {
                    // Fallback for non-SQLite databases - use hex encoding
                    string hexData = BitConverter.ToString(binData).Replace("-", "");
                    var ps = new Dictionary<string, string>
                    {
                        { "@hash", hash }
                    };
                    string sql = $"UPDATE IconFiles SET BinData = 0x{hexData}, Hash = @hash WHERE Id = {iconFileId}";
                    var result = connector.ExecuteNonQuery(sql, ps);
                    return result.IsOK;
                }
            }
            catch
            {
                return false;
            }
        }

        private string CalculateHash(byte[] data)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
