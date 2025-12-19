using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace IconCommander.Controls
{
    /// <summary>
    /// Custom control for displaying icons with proper stretching and sizing
    /// </summary>
    public class IconDisplayControl : UserControl
    {
        private PictureBox pictureBox;
        private Label lblFileName;
        private Panel borderPanel;
        private byte[] _imageData;
        private string _fileName;
        private int _iconFileId;
        private bool _isSelected;
        private ContextMenuStrip contextMenu;

        public event EventHandler IconClicked;
        public event EventHandler IconDoubleClicked;

        /// <summary>
        /// The IconFile ID from database
        /// </summary>
        public int IconFileId
        {
            get { return _iconFileId; }
            set { _iconFileId = value; }
        }

        /// <summary>
        /// The file name to display
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                if (lblFileName != null)
                    lblFileName.Text = value;
            }
        }

        /// <summary>
        /// Whether this icon is selected
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                UpdateBorder();
            }
        }

        /// <summary>
        /// The image data to display
        /// </summary>
        public byte[] ImageData
        {
            get { return _imageData; }
            set
            {
                _imageData = value;
                LoadImage();
            }
        }

        public IconDisplayControl()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.SuspendLayout();

            // Border panel for selection highlight
            borderPanel = new Panel();
            borderPanel.Dock = DockStyle.Fill;
            borderPanel.Padding = new Padding(2);
            borderPanel.BackColor = SystemColors.Control;

            // Picture box for the icon
            pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage; // Show at actual size, centered
            pictureBox.BackColor = Color.White;
            pictureBox.Click += PictureBox_Click;
            pictureBox.DoubleClick += PictureBox_DoubleClick;
            pictureBox.Cursor = Cursors.Hand;

            // Label for file name (hidden, only used for error messages)
            lblFileName = new Label();
            lblFileName.Dock = DockStyle.Bottom;
            lblFileName.Height = 20;
            lblFileName.TextAlign = ContentAlignment.MiddleCenter;
            lblFileName.Font = new Font(lblFileName.Font.FontFamily, 7);
            lblFileName.Text = "";
            lblFileName.Visible = false; // Hide the label

            // Create context menu
            InitializeContextMenu();

            // Attach context menu to picture box
            pictureBox.ContextMenuStrip = contextMenu;

            // Add controls
            borderPanel.Controls.Add(pictureBox);
            this.Controls.Add(borderPanel);
            this.Controls.Add(lblFileName);

            this.ResumeLayout(false);
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();

            // Save to Downloads
            ToolStripMenuItem menuSaveToDownloads = new ToolStripMenuItem("Save to Downloads");
            menuSaveToDownloads.Click += MenuSaveToDownloads_Click;
            contextMenu.Items.Add(menuSaveToDownloads);

            // Save to Desktop
            ToolStripMenuItem menuSaveToDesktop = new ToolStripMenuItem("Save to Desktop");
            menuSaveToDesktop.Click += MenuSaveToDesktop_Click;
            contextMenu.Items.Add(menuSaveToDesktop);

            // Save to Documents
            ToolStripMenuItem menuSaveToDocuments = new ToolStripMenuItem("Save to Documents");
            menuSaveToDocuments.Click += MenuSaveToDocuments_Click;
            contextMenu.Items.Add(menuSaveToDocuments);

            // Save to C:\Temp
            ToolStripMenuItem menuSaveToTemp = new ToolStripMenuItem("Save to C:\\Temp");
            menuSaveToTemp.Click += MenuSaveToTemp_Click;
            contextMenu.Items.Add(menuSaveToTemp);

            // Separator
            contextMenu.Items.Add(new ToolStripSeparator());

            // Save to Location...
            ToolStripMenuItem menuSaveToLocation = new ToolStripMenuItem("Save to Location...");
            menuSaveToLocation.Click += MenuSaveToLocation_Click;
            contextMenu.Items.Add(menuSaveToLocation);
        }

        private void LoadImage()
        {
            if (_imageData == null || _imageData.Length == 0)
            {
                pictureBox.Image = null;
                return;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(_imageData))
                {
                    // Create a copy of the image to avoid locking the stream
                    Image originalImage = Image.FromStream(ms);
                    pictureBox.Image = new Bitmap(originalImage);
                    originalImage.Dispose();
                }
            }
            catch (Exception ex)
            {
                // If it fails, show error message
                pictureBox.Image = null;
                lblFileName.Text = $"Error: {ex.Message}";
                lblFileName.Visible = true;
            }
        }

        private void UpdateBorder()
        {
            if (_isSelected)
            {
                borderPanel.BackColor = Color.DodgerBlue;
                borderPanel.Padding = new Padding(3);
            }
            else
            {
                borderPanel.BackColor = SystemColors.Control;
                borderPanel.Padding = new Padding(2);
            }
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            IconClicked?.Invoke(this, EventArgs.Empty);
        }

        private void PictureBox_DoubleClick(object sender, EventArgs e)
        {
            IconDoubleClicked?.Invoke(this, EventArgs.Empty);
        }

        private void MenuSaveToDownloads_Click(object sender, EventArgs e)
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            SaveToLocation(downloadsPath);
        }

        private void MenuSaveToDesktop_Click(object sender, EventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            SaveToLocation(desktopPath);
        }

        private void MenuSaveToDocuments_Click(object sender, EventArgs e)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            SaveToLocation(documentsPath);
        }

        private void MenuSaveToTemp_Click(object sender, EventArgs e)
        {
            string tempPath = @"C:\Temp";

            // Create C:\Temp if it doesn't exist
            if (!Directory.Exists(tempPath))
            {
                try
                {
                    Directory.CreateDirectory(tempPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create directory C:\\Temp:\n{ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            SaveToLocation(tempPath);
        }

        private void MenuSaveToLocation_Click(object sender, EventArgs e)
        {
            if (_imageData == null || _imageData.Length == 0)
            {
                MessageBox.Show("No image data to save.", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save Icon To Location";

            // Set initial file name based on current file name
            if (!string.IsNullOrEmpty(_fileName))
            {
                saveDialog.FileName = _fileName;
            }
            else
            {
                saveDialog.FileName = $"icon_{_iconFileId}";
            }

            // Determine file extension from image data
            string extension = GetImageExtension(_imageData);
            saveDialog.DefaultExt = extension;

            // Set filter based on detected format
            string filterName = extension.TrimStart('.').ToUpper();
            saveDialog.Filter = $"{filterName} Files|*{extension}|All Files|*.*";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(saveDialog.FileName, _imageData);
                    MessageBox.Show($"Icon saved successfully to:\n{saveDialog.FileName}",
                        "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save icon:\n{ex.Message}",
                        "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveToLocation(string folderPath)
        {
            if (_imageData == null || _imageData.Length == 0)
            {
                MessageBox.Show("No image data to save.", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show($"Directory does not exist:\n{folderPath}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Determine file extension from image data
                string extension = GetImageExtension(_imageData);

                // Create file name
                string baseFileName = !string.IsNullOrEmpty(_fileName)
                    ? _fileName
                    : $"icon_{_iconFileId}";

                // Ensure it has the correct extension
                if (!baseFileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    baseFileName += extension;
                }

                string filePath = Path.Combine(folderPath, baseFileName);

                // Check if file exists and create unique name if needed
                int counter = 1;
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
                while (File.Exists(filePath))
                {
                    string uniqueName = $"{fileNameWithoutExt}_{counter}{extension}";
                    filePath = Path.Combine(folderPath, uniqueName);
                    counter++;
                }

                // Save the file
                File.WriteAllBytes(filePath, _imageData);

                // Show confirmation
                MessageBox.Show($"Icon saved successfully to:\n{filePath}",
                    "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save icon:\n{ex.Message}",
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetImageExtension(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 4)
                return ".png";

            // Check PNG signature
            if (imageData[0] == 0x89 && imageData[1] == 0x50 && imageData[2] == 0x4E && imageData[3] == 0x47)
                return ".png";

            // Check JPEG signature
            if (imageData[0] == 0xFF && imageData[1] == 0xD8)
                return ".jpg";

            // Check GIF signature
            if (imageData[0] == 0x47 && imageData[1] == 0x49 && imageData[2] == 0x46)
                return ".gif";

            // Check BMP signature
            if (imageData[0] == 0x42 && imageData[1] == 0x4D)
                return ".bmp";

            // Check ICO signature
            if (imageData[0] == 0x00 && imageData[1] == 0x00 && imageData[2] == 0x01 && imageData[3] == 0x00)
                return ".ico";

            // Default to PNG
            return ".png";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (pictureBox != null && pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }

                if (contextMenu != null)
                {
                    contextMenu.Dispose();
                    contextMenu = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
