using IconCommander.DataAccess;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZidUtilities.CommonCode.Win;
using ZidUtilities.CommonCode.Win.Controls;
using ZidUtilities.CommonCode.Win.Forms;

namespace IconCommander.Forms
{
    public partial class TagEditForm : Form
    {
        private IIconCommanderDb connector;
        private int iconFileId;
        private int iconId;
        private string iconName;
        private ZidThemes theme;
        private List<string> allAvailableTags;
        private HashSet<string> originalTags;  // Tags when form loaded
        private HashSet<string> currentTags;    // Current state of tags
        private HashSet<string> tagsToAdd;
        private HashSet<string> tagsToRemove;

        public TagEditForm(IIconCommanderDb conx, int iconFileId, string iconName, Image iconImage, ZidThemes currentTheme)
        {
            InitializeComponent();

            this.connector = conx;
            this.iconFileId = iconFileId;
            this.iconName = iconName;
            this.theme = currentTheme;

            originalTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            currentTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            tagsToAdd = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            tagsToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            pictIconImage.BackgroundImage = iconImage;

            if(iconImage.Size.Height > 64)
            {
                int growth = iconImage.Size.Height - 64;
                this.Size = new Size(this.Size.Width, this.Size.Height + growth);
                pictIconImage.Size = new Size(pictIconImage.Size.Width, pictIconImage.Size.Height + growth);
            }
        }

        private void TagEditForm_Load(object sender, EventArgs e)
        {
            themeManager1.Theme = theme;
            themeManager1.ApplyTheme();

            this.Text = $"Edit Tags - {iconName}";

            // Get Icon ID from IconFile ID
            iconId = connector.GetIconIdFromIconFileId(iconFileId);

            if (iconId == 0)
            {
                MessageBoxDialog.Show("Could not find icon information.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
                this.Close();
                return;
            }

            LoadTags();
        }

        private void LoadTags()
        {
            try
            {
                // Load current tags for this icon
                List<string> tags = connector.GetTagsForIconFileId(iconFileId);
                if (tags != null)
                {
                    originalTags = new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase);
                    currentTags = new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase);
                }

                // Load all available tags from database
                allAvailableTags = connector.GetAllTags();
                if (allAvailableTags == null)
                {
                    allAvailableTags = new List<string>();
                }

                // Combine current tags with all available tags
                var allTagsSet = new HashSet<string>(allAvailableTags, StringComparer.OrdinalIgnoreCase);
                foreach (string tag in currentTags)
                {
                    allTagsSet.Add(tag);
                }

                PopulateTokenSelect(allTagsSet, currentTags);
                UpdateAddButton();
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error loading tags:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void PopulateTokenSelect(HashSet<string> allTags, HashSet<string> selectedTags)
        {
            // Create dictionary for TokenSelect (display text -> value)
            var tagsDict = new Dictionary<string, object>();
            foreach (string tag in allTags.OrderBy(t => t))
            {
                tagsDict.Add(tag, tag);
            }

            tokenSelectCurrentTags.SetDataSource(tagsDict);

            foreach (string tag in selectedTags.OrderBy(x => x))
                tokenSelectCurrentTags.AddToken(tag, tag);

            // Note: TokenSelect will allow user to select/deselect tags
            // Current tags are pre-selected by the control automatically if they're in the data source

            UpdateTagCount();
        }

        private void UpdateTagCount()
        {
            var selectedTags = tokenSelectCurrentTags.SelectedValues.Cast<object>().Select(v => v.ToString()).ToList();
            lblCurrentCount.Text = $"({selectedTags.Count} tags selected)";
        }

        private void UpdateAddButton()
        {
            string newTag = txtNewTag.Text.Trim();
            // Check if tag exists in TokenSelect data source
            bool tagExists = tokenSelectCurrentTags.SelectedValues.Cast<object>()
                .Select(v => v.ToString())
                .Any(t => t.Equals(newTag, StringComparison.OrdinalIgnoreCase));

            btnAdd.Enabled = !string.IsNullOrWhiteSpace(newTag) && !tagExists;
        }

        private void lstAvailableTags_DoubleClick(object sender, EventArgs e)
        {
            // Double-click to add tag to TokenSelect
            // Note: User should click on the tag in TokenSelect to select it
            // This method is kept for reference but TokenSelect handles tag selection
        }

        private void lstAvailableTags_KeyDown(object sender, KeyEventArgs e)
        {
            // Press Enter to add tag to TokenSelect
            // Note: User should click on the tag in TokenSelect to select it
            // This method is kept for reference but TokenSelect handles tag selection
        }

        private void txtNewTag_TextChanged(object sender, EventArgs e)
        {
            UpdateAddButton();
        }

        private void txtNewTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnAdd.Enabled)
            {
                btnAdd_Click(sender, e);
                e.Handled = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string newTag = txtNewTag.Text.Trim();

            if (string.IsNullOrWhiteSpace(newTag))
                return;

            // Check if tag already exists in TokenSelect
            var currentTagsList = tokenSelectCurrentTags.SelectedValues.Cast<object>()
                .Select(v => v.ToString())
                .ToList();

            if (currentTagsList.Any(t => t.Equals(newTag, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBoxDialog.Show($"Tag '{newTag}' already exists.", "Duplicate Tag",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                return;
            }

            tokenSelectCurrentTags.AddToken(newTag, newTag);

            //// Add new tag to all available tags and reload TokenSelect
            //if (!allAvailableTags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
            //{
            //    allAvailableTags.Add(newTag);
            //}

            //// Repopulate TokenSelect with new tag included
            //var allTagsSet = new HashSet<string>(allAvailableTags, StringComparer.OrdinalIgnoreCase);
            //PopulateTokenSelect(allTagsSet, currentTags);

            txtNewTag.Clear();
            txtNewTag.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Get currently selected tags from TokenSelect
                var selectedTags = tokenSelectCurrentTags.SelectedValues.Cast<object>()
                    .Select(v => v.ToString())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Determine what to add and remove
                tagsToAdd = selectedTags.Except(originalTags).ToHashSet(StringComparer.OrdinalIgnoreCase);
                tagsToRemove = originalTags.Except(selectedTags).ToHashSet(StringComparer.OrdinalIgnoreCase);

                bool hasChanges = tagsToAdd.Count > 0 || tagsToRemove.Count > 0;

                if (!hasChanges)
                {
                    MessageBoxDialog.Show("No changes to save.", "Save Tags",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, theme);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                    return;
                }

                // Remove tags
                foreach (string tag in tagsToRemove)
                {
                    connector.RemoveTagFromIcon(iconId, tag);
                }

                // Add tags
                foreach (string tag in tagsToAdd)
                {
                    connector.RegisterIconTag(iconId, tag);
                }

                MessageBoxDialog.Show($"Tags updated successfully!\n\nAdded: {tagsToAdd.Count}\nRemoved: {tagsToRemove.Count}",
                    "Save Tags", MessageBoxButtons.OK, MessageBoxIcon.Information, theme);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBoxDialog.Show($"Error saving tags:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, theme);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // Check if there are unsaved changes
            var selectedTags = tokenSelectCurrentTags.SelectedValues.Cast<object>()
                .Select(v => v.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool hasChanges = !originalTags.SetEquals(selectedTags);

            if (hasChanges)
            {
                DialogResult result = MessageBoxDialog.Show(
                    "You have unsaved changes. Are you sure you want to close?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    theme);

                if (result != DialogResult.Yes)
                    return;
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
