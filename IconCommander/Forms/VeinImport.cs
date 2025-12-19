using DocumentFormat.OpenXml.VariantTypes;
using IconCommander.DataAccess;
using IconCommander.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using ZidUtilities.CommonCode;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.Controls.AddressBar;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    public partial class VeinImport : Form
    {
        private string connectionString;
        private ZidThemes theme;
        private int selectedRowIndex = -1;
        private FileSystemNode rootNode;
        private bool processCanceled;

        private IIconCommanderDb Conx;
        private DataTable Veins;

        public DataRow SelectedVein { get; set; } = null;

        public VeinImport(string dbConnectionString, ZidThemes currentTheme)
        {
            InitializeComponent();
            connectionString = dbConnectionString;
            theme = currentTheme;

            if (Properties.Settings.Default.IsSqlite)
                Conx = new SqliteConnector();
            else
                Conx = new SqlConnector();

            Conx.Initialize(connectionString);

            string sql = @"
SELECT
	v.Id,
	v.Collection,
	c.Name AS CollectionName,
	v.Name,
	v.Description,
	v.Path,
	v.IsIcon,
	v.IsImage,
	v.IsSvg,
	(SELECT COUNT(1) from IconFiles As Icf WHERE Icf.Icon in ( SELECT I.Id FROM Icons I WHERE I.Vein = v.Id )) AS FileCount,
	(SELECT COUNT(1) FROM Icons Ic WHERE Ic.Vein = v.Id ) AS IconCount,
	(SELECT COUNT(1) from IconTags As Ict WHERE Ict.Icon in ( SELECT Ico.Id FROM Icons Ico WHERE Ico.Vein = v.Id )) AS TagCount
FROM 
	Veins v	LEFT JOIN Collections c ON v.Collection = c.Id
ORDER BY 
	v.Name
";
            var result = Conx.ExecuteTable(sql);
            if (result.IsOK)
            {
                Veins = result.Result;
                LoadVeins();
            }

            rootNode = new FileSystemNode();
            breadCrumbVeinPath.RootNode = rootNode;

        }

        private void LoadVeins()
        {
            cmbVeins.Items.Clear();
            for (int i = 0; i < Veins.Rows.Count; i++)
            {
                ComboboxItem item = new ComboboxItem();
                item.Text = Veins.Rows[i]["Name"].ToString();
                item.Value = Veins.Rows[i];
                cmbVeins.Items.Add(item);
            }
        }

        private void VeinImport_Load(object sender, EventArgs e)
        {
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            this.CenterToScreen();
        }

        private void cmbVeins_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadVeinData(cmbVeins.SelectedItem as ComboboxItem);
        }

        private void LoadVeinData(ComboboxItem item)
        {
            if (item == null)
                return;

            DataRow selectedItem = (DataRow)item.Value;

            var vein = new
            {
                Vein = int.Parse(selectedItem["Id"].ToString()),
                VeinName = selectedItem["Name"].ToString(),
                VeinDescription = selectedItem["Description"].ToString(),
                Path = selectedItem["Path"].ToString(),
                Collection = selectedItem["Collection"].ToString(),
                CollectionName = selectedItem["CollectionName"].ToString(),
                FileCount = int.Parse(selectedItem["FileCount"].ToString()),
                IconCount = int.Parse(selectedItem["IconCount"].ToString()),
                TagCount = int.Parse(selectedItem["TagCount"].ToString()),
                IsIcon = int.Parse(selectedItem["IsIcon"].ToString()),
                IsImage = int.Parse(selectedItem["IsImage"].ToString()),
                IsSvg = int.Parse(selectedItem["IsSvg"].ToString())
            };

            SelectedVein = selectedItem;

            txtCollection.Text = vein.CollectionName;
            txtVeinPath.Text = vein.Path;
            LoadBreadCrumb(vein.Path);
            txtTagsFile.Text = "";
            LoadTreeView(vein.Path);
            logs.Items.Clear();
            logs.Refresh();

            chkSvg.Checked = vein.IsSvg == 1;
            chkImage.Checked = vein.IsImage == 1;
            chkIcon.Checked = vein.IsIcon == 1;

            labFileCount.Text = vein.FileCount.ToString();
            labIconCount.Text = vein.IconCount.ToString();
            labTagCount.Text = vein.TagCount.ToString();


        }

        private void LoadBreadCrumb(string path)
        {
            Stack<string> folders = new Stack<string>();

            while (!path.IsEmpty())
            {
                string fileName = Path.GetFileName(path);
                folders.Push(fileName.IsEmpty() ? path : fileName);
                var dirInfo = new DirectoryInfo(path);
                if (dirInfo.Parent != null)
                    path = dirInfo.Parent.FullName;
                else
                    break;
            }

            breadCrumbVeinPath.CurrentNode = rootNode;

            bool isDrive = true;
            while (folders.Count > 0)
            {
                string childText = folders.Pop();
                if (isDrive)
                    childText = childText.TruncateLastChar();
                foreach (var child in breadCrumbVeinPath.CurrentNode.Children)
                {
                    if (isDrive)
                    {
                        if (child.DisplayName.Contains(childText, StringComparison.OrdinalIgnoreCase))
                        {
                            breadCrumbVeinPath.CurrentNode = child;
                            isDrive = false;
                            break;
                        }
                    }
                    else
                    {
                        if (child.DisplayName.Equals(childText, StringComparison.OrdinalIgnoreCase))
                        {
                            breadCrumbVeinPath.CurrentNode = child;
                            break;
                        }
                    }
                }
            }

        }

        private void LoadTreeView(string path)
        {
            TreeNode root = new TreeNode();
            root.Text = Path.GetFileName(path);

            List<TreeNode> nodes = GetNodesFor(path);
            if (nodes != null && nodes.Count > 0)
                root.ImageIndex = 1;
            else
                root.ImageIndex = 0;

            foreach (TreeNode node in nodes)
                root.Nodes.Add(node);

            treeVeinRoot.Nodes.Clear();
            treeVeinRoot.Nodes.Add(root);
        }

        private List<TreeNode> GetNodesFor(string path)
        {
            List<TreeNode> back = new List<TreeNode>();

            string[] dirs = Directory.GetDirectories(path);
            Array.Sort(dirs);
            string[] files = Directory.GetFiles(path);
            Array.Sort(files);

            foreach (string dir in dirs)
            {
                TreeNode newDir = new TreeNode();
                newDir.Text = Path.GetFileName(dir);
                string[] x = Directory.GetFileSystemEntries(dir);
                if (x != null && x.Length > 0)
                    newDir.ImageIndex = 1;
                else
                    newDir.ImageIndex = 0;

                foreach (TreeNode node in GetNodesFor(dir))
                    newDir.Nodes.Add(node);

                back.Add(newDir);
            }

            foreach (string file in files)
            {
                TreeNode newFile = new TreeNode();
                newFile.Text = Path.GetFileName(file);

                string extension = Path.GetExtension(file);
                switch (extension.ToLower())
                {
                    case ".txt":
                        newFile.ImageIndex = 2;
                        break;
                    case ".png":
                        newFile.ImageIndex = 3;
                        break;
                    case ".jpg":
                    case ".jpeg":
                        newFile.ImageIndex = 4;
                        break;
                    case ".bmp":
                        newFile.ImageIndex = 5;
                        break;
                    case ".ico":
                        newFile.ImageIndex = 6;
                        break;
                    default:
                        newFile.ImageIndex = 7;
                        break;
                }

                back.Add((newFile));
            }

            return back;
        }

        private void btnTagsFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Any File (*.*)|*.*";
            openDialog.Multiselect = false;
            openDialog.Title = "Select Js File with Tags/Keywords";
            openDialog.CheckFileExists = true;
            openDialog.CheckPathExists = true;

            if (openDialog.ShowDialog() == DialogResult.OK)
                txtTagsFile.Text = openDialog.FileName;

        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            ComboboxItem item = cmbVeins.SelectedItem as ComboboxItem;

            if (item == null)
                return;
            DataRow selectedItem = (DataRow)item.Value;

            int iconCount = int.Parse(selectedItem["IconCount"].ToString());
            int vein = int.Parse(selectedItem["Id"].ToString());

            if (iconCount > 0)
            {
                if (MessageBoxDialog.Show("There are already icons in this vein, do you want to delete them and proceed with the import ?", "Import Vein",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, theme) == DialogResult.Yes)
                {
                    if (!Conx.DeleteIconsFromVein(vein))
                    {
                        MessageBoxDialog.Show($"An error occurred while trying to clean icon tables: {Conx.LastMessage}", "Import Vein", MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }



            cmbVeins.Enabled = false;
            btnImport.Enabled = false;
            btnCancel.Enabled = true;

            processCanceled = false;
            bgWorker.RunWorkerAsync();
        }

        public void CleanTempTables()
        {
            if (Properties.Settings.Default.IsSqlite)
            {
                Conx.ExecuteNonQuery("DELETE FROM KeywordBuffer");
                Conx.ExecuteNonQuery("DELETE FROM IconBuffer");
                Conx.ExecuteNonQuery("DELETE FROM IconKeywordBuffer");
            }
            else
            {
                Conx.ExecuteNonQuery("TRUNCATE TABLE KeywordBuffer");
                Conx.ExecuteNonQuery("TRUNCATE TABLE IconBuffer");
                Conx.ExecuteNonQuery("TRUNCATE TABLE IconKeywordBuffer");
            }
        }

        private void WriteLog(string logText)
        {
            logs.Items.Add(logText);
            logs.SelectedIndex = logs.Items.Count - 1;
            logs.TopIndex = logs.Items.Count - 1;
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int IsIcon, IsImage, IsSvg, Collection, Vein;

            if (SelectedVein == null)
                return;
            bool tagsFile = txtTagsFile.Text.IsEmpty();

            if(chkCleanJsFile.Checked)
                CleanTempTables();

            if (tagsFile)
            {
                if (MessageBoxDialog.Show("No Js File with Keywords and Icons Association was Specifyed, continue to import vein ?", "Import Vein", MessageBoxButtons.YesNo, MessageBoxIcon.Question, theme) != DialogResult.Yes)
                {
                    return;
                }
            }
            else if(chkReloadJsFile.Checked)
            {
                bgWorker.ReportProgress(0, "Reading and processing js file for keyword association.");

                IconexMapper KeyWordsToIconIds = new IconexMapper("Keywords", "IconIds");
                IconexMapper IconsToKeyWordIds = new IconexMapper("Icons", "Keywords");
                IconexMapper KeyWordsToKeywordIds = new IconexMapper("Keywords", "KeywordIds");
                string fileName = txtTagsFile.Text;// @"D:\Icons\iconex_v5\v_collection\_iconex_system\_scripts\icon_keyword_table2.js";
                string fileText = File.ReadAllText(fileName);
                bgWorker.ReportProgress(0, "********STARTED PROCESS TO REGISTER ICON NAMES AND KEYWORDS TO DB*******");

                JObject obj = JsonConvert.DeserializeObject<JObject>(fileText);

                if (obj != null)
                {
                    foreach (JToken tok in obj["keywordsToIconNameIds"].Children())
                    {
                        Mapping x = new Mapping();
                        x.Text = tok[0].ToString();
                        JArray refs = tok[1] as JArray;
                        foreach (JToken r in refs)
                            x.References.Add(int.Parse(r.ToString()));
                        KeyWordsToIconIds.Mappings.Add(x);
                    }

                    foreach (JToken tok in obj["iconNamesToKeywordIds"].Children())
                    {
                        Mapping x = new Mapping();
                        x.Text = $"{tok[0].ToString()}";
                        JArray refs = tok[1] as JArray;
                        foreach (JToken r in refs)
                            x.References.Add(int.Parse(r.ToString()));
                        IconsToKeyWordIds.Mappings.Add(x);
                    }

                    foreach (JToken tok in obj["searchKeywordsToKeywordIds"].Children())
                    {
                        Mapping x = new Mapping();
                        x.Text = tok[0].ToString();
                        JArray refs = tok[1] as JArray;
                        foreach (JToken r in refs)
                            x.References.Add(int.Parse(r.ToString()));
                        KeyWordsToKeywordIds.Mappings.Add(x);
                    }

                    Dictionary<string, List<int>> ItoK = new Dictionary<string, List<int>>();
                    foreach (Mapping m in IconsToKeyWordIds.Mappings)
                        ItoK.Add(m.Text, m.References);

                    //register icons
                    int iconIndex = 0;
                    foreach (var ItK in ItoK)
                    {
                        Conx.Icons_Insert(iconIndex, ItK.Key);
                        if (iconIndex % 100 == 0)
                            bgWorker.ReportProgress(0, $"Registering Icon to Db {iconIndex} of {ItoK.Count}");
                        iconIndex++;

                        if (bgWorker.CancellationPending)
                        {
                            bgWorker.ReportProgress(0, "Task Cancelled!");
                            processCanceled = true;
                            return;
                        }
                    }
                    bgWorker.ReportProgress(0, $"Registered All Icons to Db ({ItoK.Count})");

                    //register keywords and their references
                    int keywordIndex = 0;
                    HashSet<string> ktis = new HashSet<string>();
                    foreach (Mapping m in KeyWordsToIconIds.Mappings)
                    {
                        Conx.Keywords_Insert(keywordIndex, m.Text);

                        foreach (int r in m.References)
                        {
                            if (!ktis.Contains($"{r}-{keywordIndex}"))
                            {
                                ktis.Add($"{r}-{keywordIndex}");
                                Conx.RegisterIconRelationShip(r, keywordIndex);
                                if (Conx.Error)
                                    MessageBoxDialog.Show(Conx.LastMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }
                        }

                        if (keywordIndex % 100 == 0)
                            bgWorker.ReportProgress(0, $"Registering Keyword to Db {keywordIndex} of {KeyWordsToKeywordIds.Mappings.Count}");
                        keywordIndex++;

                        if (bgWorker.CancellationPending)
                        {
                            bgWorker.ReportProgress(0, "Task Cancelled!");
                            processCanceled = true;
                            return;
                        }
                    }
                    bgWorker.ReportProgress(0, $"Registered All Keywords to Db ({KeyWordsToKeywordIds.Mappings.Count})");

                }




                bgWorker.ReportProgress(0, "Js file has been processed!");
            }

            IsIcon = int.Parse(SelectedVein["IsIcon"].ToString());
            IsImage = int.Parse(SelectedVein["IsImage"].ToString());
            IsSvg = int.Parse(SelectedVein["IsSvg"].ToString());
            Collection = int.Parse(SelectedVein["Collection"].ToString());
            Vein = int.Parse(SelectedVein["Id"].ToString());

            List<string> allFiles = Directory.GetFiles(txtVeinPath.Text, "*", SearchOption.AllDirectories).ToList();
            Dictionary<string, int> filesWithId = new Dictionary<string, int>();
            int fileIndex = 0, perc = 0;

            // **CRITICAL: Accumulate tags for bulk insert - commit in chunks to avoid losing work**
            List<(int iconId, string tag)> allTagsToInsert = new List<(int iconId, string tag)>();

            // **CHECKPOINT SETTINGS - Commit every N files to avoid losing hours of work!**
            const int CHECKPOINT_INTERVAL = 1000; // Commit every 1000 files
            int filesProcessedSinceCheckpoint = 0;
            int totalFilesCommitted = 0;
            HashSet<(int, string)> uniqueTagIcon = new HashSet<(int, string)>();

            // **START TRANSACTION FOR ENTIRE IMPORT - MASSIVE PERFORMANCE BOOST**
            System.Data.IDbTransaction transaction = null;

            bool optimizationsEnabled = false;
            bool indexesDropped = false;
            Exception importError = null;

            try
            {
                // **Drop indexes first (before opening connection for transaction)**
                try
                {
                    bgWorker.ReportProgress(0, "Dropping search indexes for faster import...");
                    Conx.DropIconTagsIndexes();
                    Conx.DropIconFilesIndexes();
                    indexesDropped = true;
                }
                catch (Exception ex)
                {
                    bgWorker.ReportProgress(0, $"WARNING: Failed to drop indexes: {ex.Message}");
                    bgWorker.ReportProgress(0, "Continuing import with indexes (will be slower)...");
                    indexesDropped = false;
                }

                // **CRITICAL: Begin transaction with PRAGMA optimizations**
                // This opens the connection, applies PRAGMA settings, then starts the transaction
                // All on the SAME connection so PRAGMA settings are active!
                bgWorker.ReportProgress(0, "Starting transaction with performance optimizations...");

                if (Properties.Settings.Default.IsSqlite)
                {
                    transaction = Conx.BeginBulkTransaction(applyOptimizations: true);
                    optimizationsEnabled = true;
                    bgWorker.ReportProgress(0, "Transaction started with PRAGMA optimizations active!");
                }
                else
                {
                    transaction = null;
                    optimizationsEnabled = false;
                }
                bgWorker.ReportProgress(0, $"NOTE: Work will be saved every {CHECKPOINT_INTERVAL} files to prevent data loss");

                foreach (string file in allFiles)
                {
                    if (bgWorker.CancellationPending)
                    {
                        bgWorker.ReportProgress(0, "Task Cancelled! Rolling back transaction...");
                        processCanceled = true;

                        try
                        {
                            if (transaction != null)
                                Conx.RollbackBulkTransaction(transaction);
                        }
                        catch (Exception ex)
                        {
                            bgWorker.ReportProgress(0, $"Error rolling back: {ex.Message}");
                        }

                        // CRITICAL: Always restore indexes and settings
                        RestoreDatabaseState(indexesDropped, optimizationsEnabled, bgWorker);
                        return;
                    }


                    FileInfo fi = new FileInfo(file);

                    string crudeFileName = fi.Name.Substring(0, fi.Name.LastIndexOf("."));
                    string extension = file.Substring(file.LastIndexOf(".") + 1);
                    string type;
                    List<string> validExtensions = new List<string>()
                {
                    "bmp", "gif", "jpg", "jpeg", "png", "tiff", "tif", "ico", "wmf", "emf", "svg"
                };

                    if (!validExtensions.InsensitiveContains(extension))
                    {
                        bgWorker.ReportProgress(0, $"File ({crudeFileName}.{extension}) does not have a valid extension ({extension}), and it will be skipped from the import task.");
                        continue;
                    }


                    if (!filesWithId.ContainsKey(crudeFileName))
                    {//register icon in data base
                        int newId;

                        // Pass transaction to RegisterIcon for performance
                        // CRITICAL FIX: Use InsertedId property instead of GetIconId
                        // to avoid transaction isolation issues
                        if (Properties.Settings.Default.IsSqlite)
                        {
                            Conx.RegisterIcon(crudeFileName, Vein, IsImage, IsIcon, IsSvg, transaction);
                            newId = Conx.InsertedId;
                        }
                        else
                        {
                            Conx.RegisterIcon(crudeFileName, Vein, IsImage, IsIcon, IsSvg, null);
                            newId = Conx.InsertedId;
                        }

                        filesWithId.Add(crudeFileName, newId);
                    }
                    int iconId = filesWithId[crudeFileName];

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

                    int width;
                    if (type == "svg")
                    {
                        XDocument doc = XDocument.Load(file);
                        XElement svgElement = doc.Root;

                        if (svgElement == null || svgElement.Name.LocalName != "svg")
                        {
                            Console.WriteLine("Invalid SVG file.");
                            return;
                        }

                        // Try to get width and height attributes
                        string widthAttr = (string)svgElement.Attribute("width");
                        string heightAttr = (string)svgElement.Attribute("height");
                        if (widthAttr.Contains("px", StringComparison.OrdinalIgnoreCase))
                            widthAttr = widthAttr.Substring(0, widthAttr.IndexOf("px", StringComparison.OrdinalIgnoreCase));
                        width = Convert.ToInt32(widthAttr);
                    }
                    else
                    {
                        using (Stream stream = File.OpenRead(file))
                        {
                            using (Image sourceImage = Image.FromStream(stream, false, false))
                            {
                                width = sourceImage.Width;
                                //Console.WriteLine(sourceImage.Height);
                            }
                        }
                    }


                    string originalPath = file;

                    // **READ FILE ONLY ONCE** - huge performance improvement!
                    byte[] binData = File.ReadAllBytes(file);

                    // Calculate hash from the byte array (don't read file again!)
                    string fileHash;
                    using (var sha256 = System.Security.Cryptography.SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(binData);
                        fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }

                    int isMerged = 0;

                    // Pass transaction to SaveIconFile for performance
                    int IconFileId;
                    if (Properties.Settings.Default.IsSqlite)
                    {
                        Conx.SaveIconFile(iconId, crudeFileName, extension, type, width, originalPath, binData, isMerged, fileHash, transaction);
                        IconFileId = Conx.GetIconFileId(fileHash);
                    }
                    else
                    {
                        Conx.SaveIconFile(iconId, crudeFileName, extension, type, width, originalPath, binData, isMerged, fileHash, null);
                        if(!Conx.Error)
                            IconFileId = Conx.InsertedId;
                        else
                            MessageBoxDialog.Show(Conx.LastMessage, "Error Inserting File",  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    // **ACCUMULATE TAGS - Don't insert yet, collect all for bulk insert**
                    List<string> tags = new List<string>();

                    if (tagsFile)
                    {//Calculate tags from filename
                        string[] tagArray = crudeFileName.Split(new char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tagArray != null && tagArray.Length > 0)
                            tags.AddRange(tagArray);
                    }
                    else
                    {//Calculate tags from temporary tables
                        tags = Conx.GetTagsFromTempTablesFor(crudeFileName);
                    }

                    // Add all tags to accumulator for bulk insert later
                    if (tags != null && tags.Count > 0)
                    {
                        foreach (string tag in tags)
                        {
                            if (!string.IsNullOrWhiteSpace(tag) && !tag.Equals("-") && tag.Length >= 2)
                            {
                                if (!uniqueTagIcon.Contains((iconId, tag)) )
                                {
                                    uniqueTagIcon.Add((iconId, tag));
                                    allTagsToInsert.Add((iconId, tag));
                                }
                            }
                        }
                    }

                    fileIndex++;
                    filesProcessedSinceCheckpoint++;

                    // **CHECKPOINT - Commit every CHECKPOINT_INTERVAL files to save work!**
                    if (filesProcessedSinceCheckpoint >= CHECKPOINT_INTERVAL)
                    {
                        try
                        {
                            bgWorker.ReportProgress(0, $"");
                            bgWorker.ReportProgress(0, $"╔══════════════════════════════════════════════════════════╗");
                            bgWorker.ReportProgress(0, $"║  CHECKPOINT: Saving progress ({filesProcessedSinceCheckpoint} files)");
                            bgWorker.ReportProgress(0, $"╚══════════════════════════════════════════════════════════╝");

                            // Insert accumulated tags
                            if (allTagsToInsert.Count > 0)
                            {
                                bgWorker.ReportProgress(0, $"Inserting {allTagsToInsert.Count} tags...");
                                if (Properties.Settings.Default.IsSqlite)
                                    Conx.BulkInsertAllTags(allTagsToInsert, transaction, bgWorker);
                                else
                                    Conx.BulkInsertAllTags(allTagsToInsert, null, bgWorker);
                                allTagsToInsert.Clear(); // Clear accumulator
                            }

                            // Commit current transaction
                            bgWorker.ReportProgress(0, $"Committing transaction...");
                            if (Properties.Settings.Default.IsSqlite)
                                Conx.CommitBulkTransaction(transaction);
                            transaction = null; // Mark as committed
                            totalFilesCommitted += filesProcessedSinceCheckpoint;
                            bgWorker.ReportProgress(0, $"✓ SAVED! {totalFilesCommitted}/{allFiles.Count} files committed to database");

                            // Start new transaction for next chunk
                            // IMPORTANT: Don't re-apply optimizations - connection is already optimized!
                            bgWorker.ReportProgress(0, $"Starting new transaction for next batch...");

                            if (Properties.Settings.Default.IsSqlite)
                                transaction = Conx.BeginBulkTransaction(applyOptimizations: false);

                            filesProcessedSinceCheckpoint = 0;
                            bgWorker.ReportProgress(0, $"✓ Ready to continue import");
                            bgWorker.ReportProgress(0, $"");
                        }
                        catch (Exception checkpointEx)
                        {
                            bgWorker.ReportProgress(0, $"");
                            bgWorker.ReportProgress(0, $"╔══════════════════════════════════════════════════════════╗");
                            bgWorker.ReportProgress(0, $"║  CHECKPOINT ERROR                                        ║");
                            bgWorker.ReportProgress(0, $"╚══════════════════════════════════════════════════════════╝");
                            bgWorker.ReportProgress(0, $"Error during checkpoint: {checkpointEx.Message}");
                            bgWorker.ReportProgress(0, $"Stack trace: {checkpointEx.StackTrace}");
                            bgWorker.ReportProgress(0, $"");
                            bgWorker.ReportProgress(0, $"✓ {totalFilesCommitted} files were already saved before this checkpoint");
                            bgWorker.ReportProgress(0, $"Import will terminate to prevent further issues.");
                            throw; // Re-throw to trigger main error handler
                        }
                    }

                    // ALWAYS report percentage progress
                    int currentPerc = (fileIndex * 100) / allFiles.Count;
                    if (currentPerc > perc)
                    {
                        perc = currentPerc;
                        bgWorker.ReportProgress(0, $"Processing files: {fileIndex}/{allFiles.Count} ({perc}%) - {allTagsToInsert.Count} tags accumulated");
                    }

                    // Detailed progress every 10 files to keep UI responsive
                    if (fileIndex % 10 == 0)
                    {
                        bgWorker.ReportProgress(0, $"Processed: {crudeFileName}.{extension} - {allTagsToInsert.Count} tags so far");
                    }
                }

                // **FINAL BATCH - Insert remaining tags from last chunk**
                if (allTagsToInsert.Count > 0)
                {
                    bgWorker.ReportProgress(0, $"");
                    bgWorker.ReportProgress(0, $"Inserting final {allTagsToInsert.Count} tags...");
                    if (Properties.Settings.Default.IsSqlite)
                        Conx.BulkInsertAllTags(allTagsToInsert, transaction, bgWorker);
                    else
                        Conx.BulkInsertAllTags(allTagsToInsert, null, bgWorker);
                    bgWorker.ReportProgress(0, "✓ All tags inserted successfully!");
                }

                // **COMMIT FINAL TRANSACTION**
                try
                {
                    if (Properties.Settings.Default.IsSqlite)
                        Conx.CommitBulkTransaction(transaction);
                    totalFilesCommitted += filesProcessedSinceCheckpoint;
                    bgWorker.ReportProgress(0, $"");
                    bgWorker.ReportProgress(0, $"╔══════════════════════════════════════════════════════════╗");
                    bgWorker.ReportProgress(0, $"║  FINAL COMMIT: All {totalFilesCommitted} files saved!   ");
                    bgWorker.ReportProgress(0, $"╚══════════════════════════════════════════════════════════╝");
                }
                catch (Exception commitEx)
                {
                    bgWorker.ReportProgress(0, $"ERROR: Failed to commit final transaction: {commitEx.Message}");
                    bgWorker.ReportProgress(0, $"NOTE: {totalFilesCommitted - filesProcessedSinceCheckpoint} files were already saved at previous checkpoints");
                    importError = commitEx;
                }

                // **ALWAYS RESTORE INDEXES AND NORMAL MODE - CRITICAL!**
                RestoreDatabaseState(indexesDropped, optimizationsEnabled, bgWorker);

                if (importError == null)
                    bgWorker.ReportProgress(0, "Import complete! All optimizations restored.");
                else
                    throw importError;
            }
            catch (Exception ex)
            {
                // **ROLLBACK CURRENT TRANSACTION ON ERROR**
                bgWorker.ReportProgress(0, $"");
                bgWorker.ReportProgress(0, $"╔══════════════════════════════════════════════════════════╗");
                bgWorker.ReportProgress(0, $"║  CRITICAL ERROR during import                            ║");
                bgWorker.ReportProgress(0, $"╚══════════════════════════════════════════════════════════╝");
                bgWorker.ReportProgress(0, $"Error: {ex.Message}");

                try
                {
                    if (transaction != null)
                    {
                        Conx.RollbackBulkTransaction(transaction);
                        bgWorker.ReportProgress(0, "Current transaction rolled back due to error.");
                    }
                }
                catch (Exception rollbackEx)
                {
                    bgWorker.ReportProgress(0, $"ERROR: Failed to rollback: {rollbackEx.Message}");
                }

                // **REPORT PARTIAL SUCCESS**
                bgWorker.ReportProgress(0, $"");
                if (totalFilesCommitted > 0)
                {
                    bgWorker.ReportProgress(0, $"╔══════════════════════════════════════════════════════════╗");
                    bgWorker.ReportProgress(0, $"║  PARTIAL SUCCESS                                         ║");
                    bgWorker.ReportProgress(0, $"╚══════════════════════════════════════════════════════════╝");
                    bgWorker.ReportProgress(0, $"✓ {totalFilesCommitted} files were saved at previous checkpoints");
                    bgWorker.ReportProgress(0, $"✗ {filesProcessedSinceCheckpoint} files from current batch were lost");
                    bgWorker.ReportProgress(0, $"");
                    bgWorker.ReportProgress(0, $"To continue: Re-import the vein. Already imported files will be skipped.");
                }
                else
                {
                    bgWorker.ReportProgress(0, $"No files were saved (error occurred before first checkpoint)");
                }

                // **CRITICAL: ALWAYS RESTORE INDEXES AND SETTINGS NO MATTER WHAT**
                RestoreDatabaseState(indexesDropped, optimizationsEnabled, bgWorker);

                // Store error to rethrow after cleanup
                importError = ex;
            }
            finally
            {
                // **FINAL SAFETY NET - ENSURE CLEANUP HAPPENED**
                try
                {
                    Conx.CloseBulkConnection();
                }
                catch
                {
                    // Connection close failure is not critical
                }

                // Rethrow the import error if one occurred
                if (importError != null)
                {
                    if (totalFilesCommitted > 0)
                        bgWorker.ReportProgress(0, $"Import partially completed. {totalFilesCommitted} files saved. Database restored to normal state.");
                    else
                        bgWorker.ReportProgress(0, $"Import failed. Database has been restored to normal state.");
                    throw importError;
                }
            }
        }

        /// <summary>
        /// CRITICAL METHOD: Restores database to normal state after bulk import.
        /// This MUST succeed or the database will be left in a broken state.
        /// </summary>
        private void RestoreDatabaseState(bool indexesDropped, bool optimizationsEnabled, BackgroundWorker worker)
        {
            List<string> errors = new List<string>();

            // **RESTORE INDEXES - CRITICAL**
            if (indexesDropped)
            {
                worker.ReportProgress(0, "CRITICAL: Recreating indexes (this may take a moment)...");

                try
                {
                    Conx.RecreateIconTagsIndexes();
                    worker.ReportProgress(0, "✓ IconTags indexes recreated successfully");
                }
                catch (Exception ex)
                {
                    string error = $"CRITICAL ERROR: Failed to recreate IconTags indexes: {ex.Message}";
                    worker.ReportProgress(0, error);
                    errors.Add(error);
                }

                try
                {
                    Conx.RecreateIconFilesIndexes();
                    worker.ReportProgress(0, "✓ IconFiles indexes recreated successfully");
                }
                catch (Exception ex)
                {
                    string error = $"CRITICAL ERROR: Failed to recreate IconFiles indexes: {ex.Message}";
                    worker.ReportProgress(0, error);
                    errors.Add(error);
                }
            }

            // **RESTORE PRAGMA SETTINGS - CRITICAL**
            if (optimizationsEnabled)
            {
                worker.ReportProgress(0, "Restoring normal database settings...");

                try
                {
                    Conx.DisableBulkInsertMode();
                    worker.ReportProgress(0, "✓ Database settings restored to normal");
                }
                catch (Exception ex)
                {
                    string error = $"ERROR: Failed to restore PRAGMA settings: {ex.Message}";
                    worker.ReportProgress(0, error);
                    errors.Add(error);
                }
            }

            // **REPORT RESULTS**
            if (errors.Count > 0)
            {
                worker.ReportProgress(0, "");
                worker.ReportProgress(0, "╔══════════════════════════════════════════════════════════╗");
                worker.ReportProgress(0, "║  CRITICAL: DATABASE CLEANUP FAILED                       ║");
                worker.ReportProgress(0, "╚══════════════════════════════════════════════════════════╝");
                worker.ReportProgress(0, "");
                worker.ReportProgress(0, "You MUST manually run this SQL script to fix your database:");
                worker.ReportProgress(0, "");
                worker.ReportProgress(0, "-- RESTORE PRAGMA SETTINGS");
                worker.ReportProgress(0, "PRAGMA synchronous = NORMAL;");
                worker.ReportProgress(0, "PRAGMA journal_mode = DELETE;");
                worker.ReportProgress(0, "PRAGMA temp_store = DEFAULT;");
                worker.ReportProgress(0, "PRAGMA locking_mode = NORMAL;");
                worker.ReportProgress(0, "");
                worker.ReportProgress(0, "-- RECREATE INDEXES");
                worker.ReportProgress(0, "CREATE INDEX IF NOT EXISTS idx_IconTags ON IconTags(Tag);");
                worker.ReportProgress(0, "CREATE INDEX IF NOT EXISTS idx_IconTags2 ON IconTags(Icon);");
                worker.ReportProgress(0, "CREATE INDEX IF NOT EXISTS idx_IconTags3 ON IconTags(Icon, Tag);");
                worker.ReportProgress(0, "CREATE INDEX IF NOT EXISTS idx_IconFiles ON IconFiles(Icon);");
                worker.ReportProgress(0, "");

                foreach (string error in errors)
                {
                    worker.ReportProgress(0, $"  • {error}");
                }
            }
            else
            {
                worker.ReportProgress(0, "✓ Database state fully restored");
            }
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            WriteLog(e.UserState.ToString());
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            cmbVeins.Enabled = true;
            btnImport.Enabled = true;
            btnCancel.Enabled = false;


            if (processCanceled)
            {
                int Vein = int.Parse(SelectedVein["Id"].ToString());
                Conx.DeleteIconsFromVein(Vein);
            }
            else
            {
                MessageBoxDialog.Show("Vein Import Completed !!", "Vein Import", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, theme);

                string sql = @"
SELECT
	v.Id,
	v.Collection,
	c.Name AS CollectionName,
	v.Name,
	v.Description,
	v.Path,
	v.IsIcon,
	v.IsImage,
	v.IsSvg,
	(SELECT COUNT(1) from IconFiles As Icf WHERE Icf.Icon in ( SELECT I.Id FROM Icons I WHERE I.Vein = v.Id )) AS FileCount,
	(SELECT COUNT(1) FROM Icons Ic WHERE Ic.Vein = v.Id ) AS IconCount,
	(SELECT COUNT(1) from IconTags As Ict WHERE Ict.Icon in ( SELECT Ico.Id FROM Icons Ico WHERE Ico.Vein = v.Id )) AS TagCount
FROM 
	Veins v	LEFT JOIN Collections c ON v.Collection = c.Id
ORDER BY 
	v.Name
";
                try
                {
                    var result = Conx.ExecuteTable(sql);
                    if (result.IsOK)
                    {
                        Veins = result.Result;
                        LoadVeins();
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxDialog.Show($"Error occurred after import, {ex.Message}", "Vein Import", MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            bgWorker.CancelAsync();
        }

        private void VeinImport_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bgWorker.IsBusy)
            {
                if (MessageBoxDialog.Show("There is a task being processed in the background, do you want to cancel this task ?", "Work in the Background",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, theme) == DialogResult.Yes)
                {
                    bgWorker.CancelAsync();
                }
                e.Cancel = true;
            }
        }


    }
}
