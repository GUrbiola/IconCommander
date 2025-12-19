namespace IconCommander.Forms
{
    partial class MergeForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.themeManager1 = new ZidUtilities.CommonCode.Win.Controls.ThemeManager(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblBigIconInfo = new System.Windows.Forms.Label();
            this.picBigIcon = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lblSmallIconInfo = new System.Windows.Forms.Label();
            this.picSmallIcon = new System.Windows.Forms.PictureBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rbCustom = new System.Windows.Forms.RadioButton();
            this.rbBottomRight = new System.Windows.Forms.RadioButton();
            this.rbBottom = new System.Windows.Forms.RadioButton();
            this.rbBottomLeft = new System.Windows.Forms.RadioButton();
            this.rbRight = new System.Windows.Forms.RadioButton();
            this.rbCenter = new System.Windows.Forms.RadioButton();
            this.rbLeft = new System.Windows.Forms.RadioButton();
            this.rbTopRight = new System.Windows.Forms.RadioButton();
            this.rbTop = new System.Windows.Forms.RadioButton();
            this.rbTopLeft = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lblPreviewInfo = new System.Windows.Forms.Label();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.btnMerge = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.customY = new System.Windows.Forms.NumericUpDown();
            this.customX = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBigIcon)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSmallIcon)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.customY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.customX)).BeginInit();
            this.SuspendLayout();
            // 
            // themeManager1
            // 
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ExcludedControlTypes.Add("ICSharpCode.TextEditor.TextEditorControl");
            this.themeManager1.ExcludedControlTypes.Add("ZidUtilities.CommonCode.ICSharpTextEditor.ExtendedEditor");
            this.themeManager1.ParentForm = this;
            this.themeManager1.Theme = ZidUtilities.CommonCode.Win.ZidThemes.Default;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblBigIconInfo);
            this.groupBox1.Controls.Add(this.picBigIcon);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 220);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Base Icon";
            // 
            // lblBigIconInfo
            // 
            this.lblBigIconInfo.AutoSize = true;
            this.lblBigIconInfo.Location = new System.Drawing.Point(6, 197);
            this.lblBigIconInfo.Name = "lblBigIconInfo";
            this.lblBigIconInfo.Size = new System.Drawing.Size(49, 13);
            this.lblBigIconInfo.TabIndex = 1;
            this.lblBigIconInfo.Text = "Icon Info";
            // 
            // picBigIcon
            // 
            this.picBigIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picBigIcon.Location = new System.Drawing.Point(9, 19);
            this.picBigIcon.Name = "picBigIcon";
            this.picBigIcon.Size = new System.Drawing.Size(180, 170);
            this.picBigIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picBigIcon.TabIndex = 0;
            this.picBigIcon.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lblSmallIconInfo);
            this.groupBox2.Controls.Add(this.picSmallIcon);
            this.groupBox2.Location = new System.Drawing.Point(218, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 220);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Overlay Icon";
            // 
            // lblSmallIconInfo
            // 
            this.lblSmallIconInfo.AutoSize = true;
            this.lblSmallIconInfo.Location = new System.Drawing.Point(6, 197);
            this.lblSmallIconInfo.Name = "lblSmallIconInfo";
            this.lblSmallIconInfo.Size = new System.Drawing.Size(49, 13);
            this.lblSmallIconInfo.TabIndex = 2;
            this.lblSmallIconInfo.Text = "Icon Info";
            // 
            // picSmallIcon
            // 
            this.picSmallIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picSmallIcon.Location = new System.Drawing.Point(9, 19);
            this.picSmallIcon.Name = "picSmallIcon";
            this.picSmallIcon.Size = new System.Drawing.Size(180, 170);
            this.picSmallIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picSmallIcon.TabIndex = 1;
            this.picSmallIcon.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.customX);
            this.groupBox3.Controls.Add(this.customY);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.rbCustom);
            this.groupBox3.Controls.Add(this.rbBottomRight);
            this.groupBox3.Controls.Add(this.rbBottom);
            this.groupBox3.Controls.Add(this.rbBottomLeft);
            this.groupBox3.Controls.Add(this.rbRight);
            this.groupBox3.Controls.Add(this.rbCenter);
            this.groupBox3.Controls.Add(this.rbLeft);
            this.groupBox3.Controls.Add(this.rbTopRight);
            this.groupBox3.Controls.Add(this.rbTop);
            this.groupBox3.Controls.Add(this.rbTopLeft);
            this.groupBox3.Location = new System.Drawing.Point(12, 238);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(380, 150);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Overlay Position";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(245, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Top (Y):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(245, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Left (X): ";
            // 
            // rbCustom
            // 
            this.rbCustom.AutoSize = true;
            this.rbCustom.Location = new System.Drawing.Point(240, 25);
            this.rbCustom.Name = "rbCustom";
            this.rbCustom.Size = new System.Drawing.Size(121, 17);
            this.rbCustom.TabIndex = 9;
            this.rbCustom.Text = "Custom (X, Y pixels):";
            this.rbCustom.UseVisualStyleBackColor = true;
            this.rbCustom.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbBottomRight
            // 
            this.rbBottomRight.AutoSize = true;
            this.rbBottomRight.Location = new System.Drawing.Point(135, 115);
            this.rbBottomRight.Name = "rbBottomRight";
            this.rbBottomRight.Size = new System.Drawing.Size(86, 17);
            this.rbBottomRight.TabIndex = 8;
            this.rbBottomRight.Text = "Bottom Right";
            this.rbBottomRight.UseVisualStyleBackColor = true;
            this.rbBottomRight.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbBottom
            // 
            this.rbBottom.AutoSize = true;
            this.rbBottom.Location = new System.Drawing.Point(72, 115);
            this.rbBottom.Name = "rbBottom";
            this.rbBottom.Size = new System.Drawing.Size(58, 17);
            this.rbBottom.TabIndex = 7;
            this.rbBottom.Text = "Bottom";
            this.rbBottom.UseVisualStyleBackColor = true;
            this.rbBottom.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbBottomLeft
            // 
            this.rbBottomLeft.AutoSize = true;
            this.rbBottomLeft.Location = new System.Drawing.Point(9, 115);
            this.rbBottomLeft.Name = "rbBottomLeft";
            this.rbBottomLeft.Size = new System.Drawing.Size(79, 17);
            this.rbBottomLeft.TabIndex = 6;
            this.rbBottomLeft.Text = "Bottom Left";
            this.rbBottomLeft.UseVisualStyleBackColor = true;
            this.rbBottomLeft.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbRight
            // 
            this.rbRight.AutoSize = true;
            this.rbRight.Location = new System.Drawing.Point(135, 70);
            this.rbRight.Name = "rbRight";
            this.rbRight.Size = new System.Drawing.Size(50, 17);
            this.rbRight.TabIndex = 5;
            this.rbRight.Text = "Right";
            this.rbRight.UseVisualStyleBackColor = true;
            this.rbRight.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbCenter
            // 
            this.rbCenter.AutoSize = true;
            this.rbCenter.Location = new System.Drawing.Point(72, 70);
            this.rbCenter.Name = "rbCenter";
            this.rbCenter.Size = new System.Drawing.Size(56, 17);
            this.rbCenter.TabIndex = 4;
            this.rbCenter.Text = "Center";
            this.rbCenter.UseVisualStyleBackColor = true;
            this.rbCenter.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbLeft
            // 
            this.rbLeft.AutoSize = true;
            this.rbLeft.Location = new System.Drawing.Point(9, 70);
            this.rbLeft.Name = "rbLeft";
            this.rbLeft.Size = new System.Drawing.Size(43, 17);
            this.rbLeft.TabIndex = 3;
            this.rbLeft.Text = "Left";
            this.rbLeft.UseVisualStyleBackColor = true;
            this.rbLeft.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbTopRight
            // 
            this.rbTopRight.AutoSize = true;
            this.rbTopRight.Location = new System.Drawing.Point(135, 25);
            this.rbTopRight.Name = "rbTopRight";
            this.rbTopRight.Size = new System.Drawing.Size(72, 17);
            this.rbTopRight.TabIndex = 2;
            this.rbTopRight.Text = "Top Right";
            this.rbTopRight.UseVisualStyleBackColor = true;
            this.rbTopRight.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbTop
            // 
            this.rbTop.AutoSize = true;
            this.rbTop.Location = new System.Drawing.Point(72, 25);
            this.rbTop.Name = "rbTop";
            this.rbTop.Size = new System.Drawing.Size(44, 17);
            this.rbTop.TabIndex = 1;
            this.rbTop.Text = "Top";
            this.rbTop.UseVisualStyleBackColor = true;
            this.rbTop.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // rbTopLeft
            // 
            this.rbTopLeft.AutoSize = true;
            this.rbTopLeft.Location = new System.Drawing.Point(9, 25);
            this.rbTopLeft.Name = "rbTopLeft";
            this.rbTopLeft.Size = new System.Drawing.Size(65, 17);
            this.rbTopLeft.TabIndex = 0;
            this.rbTopLeft.Text = "Top Left";
            this.rbTopLeft.UseVisualStyleBackColor = true;
            this.rbTopLeft.CheckedChanged += new System.EventHandler(this.PositionRadioButton_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lblPreviewInfo);
            this.groupBox4.Controls.Add(this.picPreview);
            this.groupBox4.Location = new System.Drawing.Point(424, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(280, 300);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Preview";
            // 
            // lblPreviewInfo
            // 
            this.lblPreviewInfo.AutoSize = true;
            this.lblPreviewInfo.Location = new System.Drawing.Point(6, 277);
            this.lblPreviewInfo.Name = "lblPreviewInfo";
            this.lblPreviewInfo.Size = new System.Drawing.Size(66, 13);
            this.lblPreviewInfo.TabIndex = 3;
            this.lblPreviewInfo.Text = "Preview Info";
            // 
            // picPreview
            // 
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Location = new System.Drawing.Point(9, 19);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(260, 250);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picPreview.TabIndex = 2;
            this.picPreview.TabStop = false;
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(548, 318);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(75, 30);
            this.btnMerge.TabIndex = 4;
            this.btnMerge.Text = "Merge";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(629, 318);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // customY
            // 
            this.customY.Location = new System.Drawing.Point(296, 83);
            this.customY.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.customY.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.customY.Name = "customY";
            this.customY.Size = new System.Drawing.Size(77, 20);
            this.customY.TabIndex = 14;
            this.customY.ValueChanged += new System.EventHandler(this.CustomPosition_Changed);
            // 
            // customX
            // 
            this.customX.Location = new System.Drawing.Point(296, 53);
            this.customX.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.customX.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            this.customX.Name = "customX";
            this.customX.Size = new System.Drawing.Size(77, 20);
            this.customX.TabIndex = 15;
            this.customX.ValueChanged += new System.EventHandler(this.CustomPosition_Changed);
            // 
            // MergeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(716, 400);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnMerge);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MergeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Merge Icons";
            this.Load += new System.EventHandler(this.MergeForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBigIcon)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSmallIcon)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.customY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.customX)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ZidUtilities.CommonCode.Win.Controls.ThemeManager themeManager1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox picBigIcon;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox picSmallIcon;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton rbBottomRight;
        private System.Windows.Forms.RadioButton rbBottom;
        private System.Windows.Forms.RadioButton rbBottomLeft;
        private System.Windows.Forms.RadioButton rbRight;
        private System.Windows.Forms.RadioButton rbCenter;
        private System.Windows.Forms.RadioButton rbLeft;
        private System.Windows.Forms.RadioButton rbTopRight;
        private System.Windows.Forms.RadioButton rbTop;
        private System.Windows.Forms.RadioButton rbTopLeft;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblBigIconInfo;
        private System.Windows.Forms.Label lblSmallIconInfo;
        private System.Windows.Forms.Label lblPreviewInfo;
        private System.Windows.Forms.RadioButton rbCustom;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown customX;
        private System.Windows.Forms.NumericUpDown customY;
    }
}
