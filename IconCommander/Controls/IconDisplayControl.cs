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

            // Add controls
            borderPanel.Controls.Add(pictureBox);
            this.Controls.Add(borderPanel);
            this.Controls.Add(lblFileName);

            this.ResumeLayout(false);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (pictureBox != null && pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
