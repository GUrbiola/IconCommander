using IconCommander.DataAccess;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    public partial class MergeForm : Form
    {
        private string connectionString;
        private ZidThemes theme;
        private IIconCommanderDb Conx;

        private byte[] bigIconData;
        private byte[] smallIconData;
        private int bigIconFileId;
        private int smallIconFileId;
        private string bigIconName;
        private string smallIconName;

        private Image bigIconImage;
        private Image smallIconImage;
        private Image previewImage;

        public MergeForm(string dbConnectionString, ZidThemes currentTheme,
            int bigIconId, int smallIconId, string bigName, string smallName,
            byte[] bigData, byte[] smallData)
        {
            InitializeComponent();
            connectionString = dbConnectionString;
            theme = currentTheme;

            if (Properties.Settings.Default.IsSqlite)
                Conx = new SqliteConnector();
            else
                Conx = new SqlConnector();

            Conx.Initialize(connectionString);

            bigIconFileId = bigIconId;
            smallIconFileId = smallIconId;
            bigIconName = bigName;
            smallIconName = smallName;
            bigIconData = bigData;
            smallIconData = smallData;

            // Load images from binary data
            LoadImages();
        }

        private void LoadImages()
        {
            try
            {
                if (bigIconData == null || bigIconData.Length == 0)
                {
                    throw new Exception("Big icon data is null or empty");
                }

                if (smallIconData == null || smallIconData.Length == 0)
                {
                    throw new Exception("Small icon data is null or empty");
                }

                using (MemoryStream ms = new MemoryStream(bigIconData))
                {
                    bigIconImage = Image.FromStream(ms);
                    picBigIcon.Image = new Bitmap(bigIconImage);
                }

                using (MemoryStream ms = new MemoryStream(smallIconData))
                {
                    smallIconImage = Image.FromStream(ms);
                    picSmallIcon.Image = new Bitmap(smallIconImage);
                }

                lblBigIconInfo.Text = $"{bigIconName} ({bigIconImage.Width}x{bigIconImage.Height})";
                lblSmallIconInfo.Text = $"{smallIconName} ({smallIconImage.Width}x{smallIconImage.Height})";

                // Generate initial preview
                UpdatePreview();
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error loading images:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Merge Icons",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                this.Close();
            }
        }

        private void MergeForm_Load(object sender, EventArgs e)
        {
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            // Set default position to TopRight
            rbTopRight.Checked = true;

            this.CenterToScreen();
        }

        private string GetSelectedPosition()
        {
            if (rbTopLeft.Checked) return "TopLeft";
            if (rbTop.Checked) return "Top";
            if (rbTopRight.Checked) return "TopRight";
            if (rbLeft.Checked) return "Left";
            if (rbCenter.Checked) return "Center";
            if (rbRight.Checked) return "Right";
            if (rbBottomLeft.Checked) return "BottomLeft";
            if (rbBottom.Checked) return "Bottom";
            if (rbBottomRight.Checked) return "BottomRight";
            if (rbCustom.Checked)
            {
                int x = 0, y = 0;
                int.TryParse(customX.Value.ToString(), out x);
                int.TryParse(customY.Value.ToString(), out y);
                return $"Custom:{x},{y}";
            }
            return "TopRight";
        }

        private Point GetSmallIconPosition(string position, int bigWidth, int bigHeight, int smallWidth, int smallHeight)
        {
            // Check if custom position
            if (position.StartsWith("Custom:"))
            {
                string coords = position.Substring(7); // Remove "Custom:" prefix
                string[] parts = coords.Split(',');
                if (parts.Length == 2)
                {
                    int x = 0, y = 0;
                    if (int.TryParse(parts[0], out x) && int.TryParse(parts[1], out y))
                    {
                        // Allow negative values for overlap effect
                        // Clamp to reasonable bounds
                        x = Math.Max(-smallWidth + 10, Math.Min(x, bigWidth - 10));
                        y = Math.Max(-smallHeight + 10, Math.Min(y, bigHeight - 10));
                        return new Point(x, y);
                    }
                }
            }

            // For corner/edge positions, position flush to the edge (no margin)
            // This creates the typical "badge" or "overlay" effect
            switch (position)
            {
                case "TopLeft":
                    // Flush to top-left corner
                    return new Point(0, 0);

                case "Top":
                    // Centered horizontally, flush to top
                    return new Point((bigWidth - smallWidth) / 2, 0);

                case "TopRight":
                    // Flush to top-right corner
                    return new Point(bigWidth - smallWidth, 0);

                case "Left":
                    // Flush to left edge, centered vertically
                    return new Point(0, (bigHeight - smallHeight) / 2);

                case "Center":
                    // Centered both horizontally and vertically
                    return new Point((bigWidth - smallWidth) / 2, (bigHeight - smallHeight) / 2);

                case "Right":
                    // Flush to right edge, centered vertically
                    return new Point(bigWidth - smallWidth, (bigHeight - smallHeight) / 2);

                case "BottomLeft":
                    // Flush to bottom-left corner
                    return new Point(0, bigHeight - smallHeight);

                case "Bottom":
                    // Centered horizontally, flush to bottom
                    return new Point((bigWidth - smallWidth) / 2, bigHeight - smallHeight);

                case "BottomRight":
                    // Flush to bottom-right corner
                    return new Point(bigWidth - smallWidth, bigHeight - smallHeight);

                default:
                    // Default to top-right (common for badges/notifications)
                    return new Point(bigWidth - smallWidth, 0);
            }
        }

        private void UpdatePreview()
        {
            try
            {
                string position = GetSelectedPosition();
                previewImage = CreateMergedImage(position);
                picPreview.Image = previewImage;
                lblPreviewInfo.Text = $"Preview ({previewImage.Width}x{previewImage.Height})";
            }
            catch (Exception ex)
            {
                lblPreviewInfo.Text = $"Preview error: {ex.Message}";
            }
        }

        private Image CreateMergedImage(string position)
        {
            // Create a new bitmap with the size of the big icon
            Bitmap mergedImage = new Bitmap(bigIconImage.Width, bigIconImage.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(mergedImage))
            {
                // Set high quality rendering
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Draw the big icon as base
                g.DrawImage(bigIconImage, 0, 0, bigIconImage.Width, bigIconImage.Height);

                // Calculate position for small icon
                Point smallIconPos = GetSmallIconPosition(position,
                    bigIconImage.Width, bigIconImage.Height,
                    smallIconImage.Width, smallIconImage.Height);

                // Draw the small icon on top
                g.DrawImage(smallIconImage, smallIconPos.X, smallIconPos.Y,
                    smallIconImage.Width, smallIconImage.Height);
            }

            return mergedImage;
        }

        private void PositionRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                UpdatePreview();
            }
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            try
            {
                // Ensure "Merged Icons" collection exists
                int mergedCollectionId = EnsureMergedIconsCollection();
                if (mergedCollectionId == -1)
                {
                    MessageBoxDialog.Show("Failed to create or find 'Merged Icons' collection.", "Merge Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    return;
                }

                // Ensure "Merged Icons" vein exists
                int mergedVeinId = EnsureMergedIconsVein(mergedCollectionId);
                if (mergedVeinId == -1)
                {
                    MessageBoxDialog.Show("Failed to create or find 'Merged Icons' vein.", "Merge Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    return;
                }

                // Create the merged image
                string position = GetSelectedPosition();
                Image merged = CreateMergedImage(position);

                // Generate merged icon name
                string mergedIconName = $"{bigIconName}_{smallIconName}_merged";

                // Save merged icon to database
                int mergedIconId = SaveMergedIcon(mergedVeinId, mergedIconName, merged);
                if (mergedIconId == -1)
                {
                    MessageBoxDialog.Show("Failed to save merged icon.", "Merge Icons",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                    return;
                }

                // Get the IconFile ID of the merged icon
                int mergedIconFileId = GetMergedIconFileId(mergedIconId);

                // Save merge recipe
                SaveMergeRecipe(bigIconFileId, smallIconFileId, position, mergedIconFileId);

                // Add merged icon to buffer zone
                string insertBuffer = $@"INSERT INTO BufferZone (IconFile, CreationDate)
                                        VALUES ({mergedIconFileId}, '{DateTime.Now:yyyy-MM-dd HH:mm:ss}')";
                Conx.ExecuteNonQuery(insertBuffer);

                MessageBoxDialog.Show($"Icons merged successfully!\nMerged icon: {mergedIconName}",
                    "Merge Icons", MessageBoxButtons.OK, MessageBoxIcon.Information, theme);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error merging icons: {ex.Message}", "Merge Icons",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private int EnsureMergedIconsCollection()
        {
            // Check if collection exists
            var response = Conx.ExecuteScalar("SELECT Id FROM Collections WHERE Name = 'Merged Icons'");
            if (response.IsOK && response.Result != null && response.Result != DBNull.Value)
            {
                return Convert.ToInt32(response.Result);
            }

            // Create collection
            string sql = "INSERT INTO Collections (Name, Description) VALUES ('Merged Icons', 'Collection for merged/composite icons')";
            if (!Properties.Settings.Default.IsSqlite)
                sql += "; SELECT SCOPE_IDENTITY()";
            else
                sql += "; SELECT last_insert_rowid()";

            var insertResponse = Conx.ExecuteScalar(sql);
            if (insertResponse.IsOK && insertResponse.Result != null)
            {
                return Convert.ToInt32(insertResponse.Result);
            }

            return -1;
        }

        private int EnsureMergedIconsVein(int collectionId)
        {
            // Check if vein exists
            var response = Conx.ExecuteScalar($"SELECT Id FROM Veins WHERE Name = 'Merged Icons' AND Collection = {collectionId}");
            if (response.IsOK && response.Result != null && response.Result != DBNull.Value)
            {
                return Convert.ToInt32(response.Result);
            }

            // Create vein
            string sql = $@"INSERT INTO Veins (Collection, Name, Description, Path, IsIcon, IsImage, IsSvg)
                           VALUES ({collectionId}, 'Merged Icons', 'Vein for merged/composite icons', '', 0, 1, 0)";
            if (!Properties.Settings.Default.IsSqlite)
                sql += "; SELECT SCOPE_IDENTITY()";
            else
                sql += "; SELECT last_insert_rowid()";

            var insertResponse = Conx.ExecuteScalar(sql);
            if (insertResponse.IsOK && insertResponse.Result != null)
            {
                return Convert.ToInt32(insertResponse.Result);
            }

            return -1;
        }

        private int SaveMergedIcon(int veinId, string iconName, Image mergedImage)
        {
            // Register the icon
            Conx.RegisterIcon(iconName, veinId, 1, 0, 0, null);
            int iconId = Conx.InsertedId;

            if (iconId <= 0)
                return -1;

            // Convert image to PNG byte array
            byte[] imageData;
            using (MemoryStream ms = new MemoryStream())
            {
                mergedImage.Save(ms, ImageFormat.Png);
                imageData = ms.ToArray();
            }

            // Calculate hash
            string hash;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(imageData);
                hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            // Save icon file
            Conx.SaveIconFile(iconId, iconName, "png", "image/png",
                mergedImage.Width * mergedImage.Height, "", imageData, 1, hash, null);

            return iconId;
        }

        private int GetMergedIconFileId(int iconId)
        {
            var response = Conx.ExecuteScalar($"SELECT Id FROM IconFiles WHERE Icon = {iconId} ORDER BY Id DESC");
            if (response.IsOK && response.Result != null && response.Result != DBNull.Value)
            {
                return Convert.ToInt32(response.Result);
            }
            return -1;
        }

        private void SaveMergeRecipe(int bigIconId, int smallIconId, string position, int resultIconId)
        {
            string sql = $@"INSERT INTO MergeRecipes (BigIcon, SmallIcon, SmallIconPosition, IconResult)
                           VALUES ({bigIconId}, {smallIconId}, '{position}', {resultIconId})";
            Conx.ExecuteNonQuery(sql);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void CustomPosition_Changed(object sender, EventArgs e)
        {
            // Only update preview if custom radio button is checked
            if (rbCustom.Checked)
            {
                UpdatePreview();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                bigIconImage?.Dispose();
                smallIconImage?.Dispose();
                previewImage?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
