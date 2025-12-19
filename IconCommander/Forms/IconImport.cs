using IconCommander.DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using ZidUtilities.CommonCode;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    public partial class IconImport : Form
    {
        private string connectionString;
        private ZidThemes theme;
        private IIconCommanderDb Conx;
        private DataTable Collections;
        private DataTable Veins;
        private List<string> selectedFiles = new List<string>();
        private bool processCanceled;

        public IconImport(string dbConnectionString, ZidThemes currentTheme)
        {
            InitializeComponent();
            connectionString = dbConnectionString;
            theme = currentTheme;

            if (Properties.Settings.Default.IsSqlite)
                Conx = new SqliteConnector();
            else
                Conx = new SqlConnector();

            Conx.Initialize(connectionString);

            LoadCollections();
        }

        private void IconImport_Load(object sender, EventArgs e)
        {
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            this.CenterToScreen();
        }

        private void LoadCollections()
        {
            string sql = "SELECT Id, Name FROM Collections ORDER BY Name";
            var result = Conx.ExecuteTable(sql);
            if (result.IsOK)
            {
                Collections = result.Result;
                cmbCollections.Items.Clear();
                foreach (DataRow row in Collections.Rows)
                {
                    ComboboxItem item = new ComboboxItem
                    {
                        Text = row["Name"].ToString(),
                        Value = row
                    };
                    cmbCollections.Items.Add(item);
                }
            }
        }

        private void LoadVeinsForCollection(int collectionId)
        {
            string sql = $@"
SELECT
    v.Id,
    v.Name,
    v.Path,
    v.IsIcon,
    v.IsImage,
    v.IsSvg
FROM Veins v
WHERE v.Collection = {collectionId}
ORDER BY v.Name";

            var result = Conx.ExecuteTable(sql);
            if (result.IsOK)
            {
                Veins = result.Result;
                cmbVeins.Items.Clear();
                foreach (DataRow row in Veins.Rows)
                {
                    ComboboxItem item = new ComboboxItem
                    {
                        Text = row["Name"].ToString(),
                        Value = row
                    };
                    cmbVeins.Items.Add(item);
                }

                if (cmbVeins.Items.Count > 0)
                    cmbVeins.Enabled = true;
                else
                {
                    cmbVeins.Enabled = false;
                    MessageBoxDialog.Show("No veins found for this collection.", "Import Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                }
            }
        }

        private void cmbCollections_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboboxItem item = cmbCollections.SelectedItem as ComboboxItem;
            if (item != null)
            {
                DataRow collection = (DataRow)item.Value;
                int collectionId = Convert.ToInt32(collection["Id"]);
                txtCollection.Text = collection["Name"].ToString();
                LoadVeinsForCollection(collectionId);
            }
        }

        private void cmbVeins_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboboxItem item = cmbVeins.SelectedItem as ComboboxItem;
            if (item != null)
            {
                DataRow vein = (DataRow)item.Value;
                txtVeinName.Text = vein["Name"].ToString();
                txtVeinPath.Text = vein["Path"].ToString();

                btnAddFiles.Enabled = true;
            }
        }

        private void btnAddFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Icon Files|*.ico;*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.svg;*.tiff;*.tif;*.wmf;*.emf|All Files|*.*";
            openDialog.Multiselect = true;
            openDialog.Title = "Select Icon Files to Import";
            openDialog.CheckFileExists = true;
            openDialog.CheckPathExists = true;

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in openDialog.FileNames)
                {
                    if (!selectedFiles.Contains(file))
                    {
                        selectedFiles.Add(file);
                        lstFiles.Items.Add(Path.GetFileName(file));
                    }
                }

                lblFileCount.Text = selectedFiles.Count.ToString();
                btnImport.Enabled = selectedFiles.Count > 0;
            }
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedIndices.Count > 0)
            {
                List<int> indicesToRemove = new List<int>();
                foreach (int index in lstFiles.SelectedIndices)
                {
                    indicesToRemove.Add(index);
                }

                // Remove in reverse order to maintain indices
                indicesToRemove.Sort();
                indicesToRemove.Reverse();

                foreach (int index in indicesToRemove)
                {
                    selectedFiles.RemoveAt(index);
                    lstFiles.Items.RemoveAt(index);
                }

                lblFileCount.Text = selectedFiles.Count.ToString();
                btnImport.Enabled = selectedFiles.Count > 0;
            }
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            selectedFiles.Clear();
            lstFiles.Items.Clear();
            lblFileCount.Text = "0";
            btnImport.Enabled = false;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (cmbVeins.SelectedItem == null)
            {
                MessageBoxDialog.Show("Please select a vein before importing.", "Import Icons",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                return;
            }

            if (selectedFiles.Count == 0)
            {
                MessageBoxDialog.Show("Please add at least one file to import.", "Import Icons",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, theme);
                return;
            }

            cmbCollections.Enabled = false;
            cmbVeins.Enabled = false;
            btnAddFiles.Enabled = false;
            btnRemoveSelected.Enabled = false;
            btnClearAll.Enabled = false;
            btnImport.Enabled = false;
            btnCancel.Enabled = true;

            processCanceled = false;
            bgWorker.RunWorkerAsync();
        }

        private void WriteLog(string logText)
        {
            logs.Items.Add(logText);
            logs.SelectedIndex = logs.Items.Count - 1;
            logs.TopIndex = logs.Items.Count - 1;
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ComboboxItem veinItem = null;
            this.Invoke((MethodInvoker)delegate
            {
                veinItem = cmbVeins.SelectedItem as ComboboxItem;
            });

            if (veinItem == null)
                return;

            DataRow selectedVein = (DataRow)veinItem.Value;
            int veinId = Convert.ToInt32(selectedVein["Id"]);
            int isIcon = Convert.ToInt32(selectedVein["IsIcon"]);
            int isImage = Convert.ToInt32(selectedVein["IsImage"]);
            int isSvg = Convert.ToInt32(selectedVein["IsSvg"]);

            bgWorker.ReportProgress(0, $"Starting import of {selectedFiles.Count} files...");

            Dictionary<string, int> filesWithId = new Dictionary<string, int>();
            int fileIndex = 0;

            // Start transaction for better performance
            System.Data.IDbTransaction transaction = null;
            try
            {
                bgWorker.ReportProgress(0, "Starting transaction...");
                if (Properties.Settings.Default.IsSqlite)
                    transaction = Conx.BeginBulkTransaction(applyOptimizations: false);

                foreach (string file in selectedFiles)
                {
                    if (bgWorker.CancellationPending)
                    {
                        bgWorker.ReportProgress(0, "Task Cancelled!");
                        processCanceled = true;
                        if (transaction != null)
                            Conx.RollbackBulkTransaction(transaction);
                        return;
                    }

                    try
                    {
                        FileInfo fi = new FileInfo(file);
                        string crudeFileName = fi.Name.Substring(0, fi.Name.LastIndexOf("."));
                        string extension = file.Substring(file.LastIndexOf(".") + 1);
                        string type;

                        // Determine type
                        switch (extension.ToLower())
                        {
                            case "icon":
                            case "ico":
                                type = "icon";
                                break;
                            case "svg":
                                type = "svg";
                                break;
                            default:
                                type = $"image/{extension}";
                                break;
                        }

                        // Register icon if not exists
                        if (!filesWithId.ContainsKey(crudeFileName))
                        {
                            int newId;

                            if (Properties.Settings.Default.IsSqlite)
                            {
                                Conx.RegisterIcon(crudeFileName, veinId, isImage, isIcon, isSvg, transaction);
                                newId = Conx.InsertedId;
                            }
                            else
                            {
                                Conx.RegisterIcon(crudeFileName, veinId, isImage, isIcon, isSvg, null);
                                newId = Conx.InsertedId;
                            }

                            filesWithId.Add(crudeFileName, newId);
                        }

                        int iconId = filesWithId[crudeFileName];

                        // Get file dimensions
                        int width;
                        if (type == "svg")
                        {
                            try
                            {
                                XDocument doc = XDocument.Load(file);
                                XElement svgElement = doc.Root;

                                if (svgElement != null && svgElement.Name.LocalName == "svg")
                                {
                                    string widthAttr = (string)svgElement.Attribute("width");
                                    if (widthAttr != null && widthAttr.Contains("px", StringComparison.OrdinalIgnoreCase))
                                        widthAttr = widthAttr.Substring(0, widthAttr.IndexOf("px", StringComparison.OrdinalIgnoreCase));
                                    width = string.IsNullOrEmpty(widthAttr) ? 24 : Convert.ToInt32(widthAttr);
                                }
                                else
                                {
                                    width = 24; // Default for SVG
                                }
                            }
                            catch
                            {
                                width = 24; // Default on error
                            }
                        }
                        else
                        {
                            try
                            {
                                using (Stream stream = File.OpenRead(file))
                                {
                                    using (Image sourceImage = Image.FromStream(stream, false, false))
                                    {
                                        width = sourceImage.Width;
                                    }
                                }
                            }
                            catch
                            {
                                width = 16; // Default on error
                            }
                        }

                        // Read file binary data
                        byte[] binData = File.ReadAllBytes(file);

                        // Calculate hash
                        string fileHash;
                        using (var sha256 = System.Security.Cryptography.SHA256.Create())
                        {
                            byte[] hashBytes = sha256.ComputeHash(binData);
                            fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                        }

                        int isMerged = 0;

                        // Save icon file
                        if (Properties.Settings.Default.IsSqlite)
                        {
                            Conx.SaveIconFile(iconId, crudeFileName, extension, type, width, file, binData, isMerged, fileHash, transaction);
                        }
                        else
                        {
                            Conx.SaveIconFile(iconId, crudeFileName, extension, type, width, file, binData, isMerged, fileHash, null);
                        }

                        // Extract tags from filename
                        string[] tagArray = crudeFileName.Split(new char[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string tag in tagArray)
                        {
                            if (!string.IsNullOrWhiteSpace(tag) && !tag.Equals("-") && tag.Length >= 2)
                            {
                                if (Properties.Settings.Default.IsSqlite)
                                {
                                    // For SQLite, we need to use the transaction connection
                                    Conx.RegisterIconTag(iconId, tag);
                                }
                                else
                                {
                                    Conx.RegisterIconTag(iconId, tag);
                                }
                            }
                        }

                        fileIndex++;
                        int percent = (fileIndex * 100) / selectedFiles.Count;
                        bgWorker.ReportProgress(percent, $"Imported {fileIndex}/{selectedFiles.Count}: {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        bgWorker.ReportProgress(0, $"ERROR importing {Path.GetFileName(file)}: {ex.Message}");
                    }
                }

                // Commit transaction
                if (Properties.Settings.Default.IsSqlite && transaction != null)
                {
                    bgWorker.ReportProgress(0, "Committing transaction...");
                    Conx.CommitBulkTransaction(transaction);
                }

                bgWorker.ReportProgress(100, $"Import completed! {fileIndex} files imported successfully.");
            }
            catch (Exception ex)
            {
                bgWorker.ReportProgress(0, $"CRITICAL ERROR: {ex.Message}");
                if (transaction != null)
                {
                    try
                    {
                        Conx.RollbackBulkTransaction(transaction);
                        bgWorker.ReportProgress(0, "Transaction rolled back due to error.");
                    }
                    catch { }
                }
            }
            finally
            {
                try
                {
                    Conx.CloseBulkConnection();
                }
                catch { }
            }
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WriteLog(e.UserState.ToString());
            progressBar1.Value = e.ProgressPercentage;
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            cmbCollections.Enabled = true;
            cmbVeins.Enabled = true;
            btnAddFiles.Enabled = true;
            btnRemoveSelected.Enabled = true;
            btnClearAll.Enabled = true;
            btnImport.Enabled = selectedFiles.Count > 0;
            btnCancel.Enabled = false;

            if (!processCanceled)
            {
                MessageBoxDialog.Show("Icon import completed successfully!", "Import Icons",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            bgWorker.CancelAsync();
        }
    }
}
