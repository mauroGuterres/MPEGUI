namespace MPEGUI
{
    partial class FormExtractRange
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Controls on the form
        private System.Windows.Forms.Label lblSelectedFile;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Label lblExtractStart;
        private System.Windows.Forms.TextBox txtExtractStart;
        private System.Windows.Forms.Label lblExtractEnd;
        private System.Windows.Forms.TextBox txtExtractEnd;
        private System.Windows.Forms.Button btnExtractRange;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressBar;
        // Our custom dual-thumb range trackbar
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

        private void InitializeComponent()
        {
            lblSelectedFile = new Label();
            btnSelectFile = new Button();
            lblExtractStart = new Label();
            txtExtractStart = new TextBox();
            lblExtractEnd = new Label();
            txtExtractEnd = new TextBox();
            btnExtractRange = new Button();
            btnCancel = new Button();
            progressBar = new ProgressBar();
            rangeTrackBar = new Components.RangeTrackBar();
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
            // lblExtractStart
            // 
            lblExtractStart.AutoSize = true;
            lblExtractStart.Location = new Point(29, 142);
            lblExtractStart.Margin = new Padding(4, 0, 4, 0);
            lblExtractStart.Name = "lblExtractStart";
            lblExtractStart.Size = new Size(210, 25);
            lblExtractStart.TabIndex = 2;
            lblExtractStart.Text = "Extract Start (HH:MM:SS):";
            // 
            // txtExtractStart
            // 
            txtExtractStart.Location = new Point(29, 175);
            txtExtractStart.Margin = new Padding(4, 5, 4, 5);
            txtExtractStart.Name = "txtExtractStart";
            txtExtractStart.Size = new Size(141, 31);
            txtExtractStart.TabIndex = 3;
            // 
            // lblExtractEnd
            // 
            lblExtractEnd.AutoSize = true;
            lblExtractEnd.Location = new Point(29, 233);
            lblExtractEnd.Margin = new Padding(4, 0, 4, 0);
            lblExtractEnd.Name = "lblExtractEnd";
            lblExtractEnd.Size = new Size(204, 25);
            lblExtractEnd.TabIndex = 4;
            lblExtractEnd.Text = "Extract End (HH:MM:SS):";
            // 
            // txtExtractEnd
            // 
            txtExtractEnd.Location = new Point(29, 267);
            txtExtractEnd.Margin = new Padding(4, 5, 4, 5);
            txtExtractEnd.Name = "txtExtractEnd";
            txtExtractEnd.Size = new Size(141, 31);
            txtExtractEnd.TabIndex = 5;
            // 
            // btnExtractRange
            // 
            btnExtractRange.Location = new Point(29, 333);
            btnExtractRange.Margin = new Padding(4, 5, 4, 5);
            btnExtractRange.Name = "btnExtractRange";
            btnExtractRange.Size = new Size(143, 38);
            btnExtractRange.TabIndex = 6;
            btnExtractRange.Text = "Extract Range";
            btnExtractRange.UseVisualStyleBackColor = true;
            btnExtractRange.Click += btnExtractRange_Click;
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
            progressBar.Size = new Size(571, 38);
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
            // FormExtractRange
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(686, 500);
            Controls.Add(rangeTrackBar);
            Controls.Add(progressBar);
            Controls.Add(btnCancel);
            Controls.Add(btnExtractRange);
            Controls.Add(txtExtractEnd);
            Controls.Add(lblExtractEnd);
            Controls.Add(txtExtractStart);
            Controls.Add(lblExtractStart);
            Controls.Add(btnSelectFile);
            Controls.Add(lblSelectedFile);
            Margin = new Padding(4, 5, 4, 5);
            Name = "FormExtractRange";
            Text = "Extract Video Range";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
