namespace MPEGUI
{
    partial class FormCutVideoRange
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Existing controls
        private System.Windows.Forms.Label lblSelectedFile;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Label lblCutStart;
        private System.Windows.Forms.TextBox txtCutStart;
        private System.Windows.Forms.Label lblCutEnd;
        private System.Windows.Forms.TextBox txtCutEnd;
        private System.Windows.Forms.Button btnCutVideo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressBar;

        // New custom control for range selection
        private MPEGUI.Components.RangeTrackBar rangeTrackBar;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support – do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblSelectedFile = new Label();
            btnSelectFile = new Button();
            lblCutStart = new Label();
            txtCutStart = new TextBox();
            lblCutEnd = new Label();
            txtCutEnd = new TextBox();
            btnCutVideo = new Button();
            btnCancel = new Button();
            progressBar = new ProgressBar();
            rangeTrackBar = new Components.RangeTrackBar();
            btnAddRange = new Button();
            listBoxRanges = new ListBox();
            SuspendLayout();
            // 
            // lblSelectedFile
            // 
            lblSelectedFile.AutoSize = true;
            lblSelectedFile.Location = new Point(29, 33);
            lblSelectedFile.Margin = new Padding(4, 0, 4, 0);
            lblSelectedFile.Name = "lblSelectedFile";
            lblSelectedFile.Size = new Size(113, 25);
            lblSelectedFile.TabIndex = 0;
            lblSelectedFile.Text = "Selected File:";
            // 
            // btnSelectFile
            // 
            btnSelectFile.Location = new Point(29, 75);
            btnSelectFile.Margin = new Padding(4, 5, 4, 5);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(143, 38);
            btnSelectFile.TabIndex = 1;
            btnSelectFile.Text = "Select File";
            btnSelectFile.UseVisualStyleBackColor = true;
            btnSelectFile.Click += btnSelectFile_Click;
            // 
            // lblCutStart
            // 
            lblCutStart.AutoSize = true;
            lblCutStart.Location = new Point(29, 142);
            lblCutStart.Margin = new Padding(4, 0, 4, 0);
            lblCutStart.Name = "lblCutStart";
            lblCutStart.Size = new Size(185, 25);
            lblCutStart.TabIndex = 2;
            lblCutStart.Text = "Cut Start (HH:MM:SS):";
            // 
            // txtCutStart
            // 
            txtCutStart.Location = new Point(29, 175);
            txtCutStart.Margin = new Padding(4, 5, 4, 5);
            txtCutStart.Name = "txtCutStart";
            txtCutStart.Size = new Size(141, 31);
            txtCutStart.TabIndex = 3;
            // 
            // lblCutEnd
            // 
            lblCutEnd.AutoSize = true;
            lblCutEnd.Location = new Point(29, 233);
            lblCutEnd.Margin = new Padding(4, 0, 4, 0);
            lblCutEnd.Name = "lblCutEnd";
            lblCutEnd.Size = new Size(179, 25);
            lblCutEnd.TabIndex = 4;
            lblCutEnd.Text = "Cut End (HH:MM:SS):";
            // 
            // txtCutEnd
            // 
            txtCutEnd.Location = new Point(29, 267);
            txtCutEnd.Margin = new Padding(4, 5, 4, 5);
            txtCutEnd.Name = "txtCutEnd";
            txtCutEnd.Size = new Size(141, 31);
            txtCutEnd.TabIndex = 5;
            // 
            // btnCutVideo
            // 
            btnCutVideo.Location = new Point(29, 333);
            btnCutVideo.Margin = new Padding(4, 5, 4, 5);
            btnCutVideo.Name = "btnCutVideo";
            btnCutVideo.Size = new Size(143, 38);
            btnCutVideo.TabIndex = 6;
            btnCutVideo.Text = "Cut Video";
            btnCutVideo.UseVisualStyleBackColor = true;
            btnCutVideo.Click += btnCutVideo_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(200, 333);
            btnCancel.Margin = new Padding(4, 5, 4, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(143, 38);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(29, 400);
            progressBar.Margin = new Padding(4, 5, 4, 5);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(318, 38);
            progressBar.TabIndex = 8;
            // 
            // rangeTrackBar
            // 
            rangeTrackBar.Location = new Point(214, 142);
            rangeTrackBar.LowerValue = 0;
            rangeTrackBar.Margin = new Padding(4, 5, 4, 5);
            rangeTrackBar.Maximum = 100;
            rangeTrackBar.Minimum = 0;
            rangeTrackBar.MinimumSize = new Size(143, 50);
            rangeTrackBar.Name = "rangeTrackBar";
            rangeTrackBar.Size = new Size(429, 50);
            rangeTrackBar.TabIndex = 9;
            rangeTrackBar.UpperValue = 100;
            // 
            // btnAddRange
            // 
            btnAddRange.Location = new Point(200, 75);
            btnAddRange.Margin = new Padding(4, 5, 4, 5);
            btnAddRange.Name = "btnAddRange";
            btnAddRange.Size = new Size(143, 38);
            btnAddRange.TabIndex = 10;
            btnAddRange.Text = "Add Range";
            btnAddRange.UseVisualStyleBackColor = true;
            btnAddRange.Click += btnAddRange_Click;
            // 
            // listBoxRanges
            // 
            listBoxRanges.FormattingEnabled = true;
            listBoxRanges.ItemHeight = 25;
            listBoxRanges.Location = new Point(374, 249);
            listBoxRanges.Name = "listBoxRanges";
            listBoxRanges.Size = new Size(300, 329);
            listBoxRanges.TabIndex = 11;
            // 
            // FormCutVideoRange
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(686, 594);
            Controls.Add(listBoxRanges);
            Controls.Add(btnAddRange);
            Controls.Add(rangeTrackBar);
            Controls.Add(progressBar);
            Controls.Add(btnCancel);
            Controls.Add(btnCutVideo);
            Controls.Add(txtCutEnd);
            Controls.Add(lblCutEnd);
            Controls.Add(txtCutStart);
            Controls.Add(lblCutStart);
            Controls.Add(btnSelectFile);
            Controls.Add(lblSelectedFile);
            Margin = new Padding(4, 5, 4, 5);
            Name = "FormCutVideoRange";
            Text = "Cut Video Range";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnAddRange;
        private ListBox listBoxRanges;
    }
}
