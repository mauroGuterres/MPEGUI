namespace MPEGUI
{
    partial class FormSplitVideo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblSelectedFile = new Label();
            numParts = new NumericUpDown();
            btnBrowse = new Button();
            lblSplitIn = new Label();
            lblParts = new Label();
            btnSplit = new Button();
            chkUseGpu = new CheckBox();
            progressBar = new ProgressBar();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numParts).BeginInit();
            SuspendLayout();
            // 
            // lblSelectedFile
            // 
            lblSelectedFile.AutoSize = true;
            lblSelectedFile.Location = new Point(33, 99);
            lblSelectedFile.Name = "lblSelectedFile";
            lblSelectedFile.Size = new Size(133, 25);
            lblSelectedFile.TabIndex = 0;
            lblSelectedFile.Text = "No file selected";
            // 
            // numParts
            // 
            numParts.Location = new Point(353, 42);
            numParts.Name = "numParts";
            numParts.Size = new Size(180, 31);
            numParts.TabIndex = 1;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(33, 35);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(112, 34);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Select";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // lblSplitIn
            // 
            lblSplitIn.AutoSize = true;
            lblSplitIn.Location = new Point(281, 44);
            lblSplitIn.Name = "lblSplitIn";
            lblSplitIn.Size = new Size(66, 25);
            lblSplitIn.TabIndex = 3;
            lblSplitIn.Text = "Split in";
            // 
            // lblParts
            // 
            lblParts.AutoSize = true;
            lblParts.Location = new Point(539, 44);
            lblParts.Name = "lblParts";
            lblParts.Size = new Size(52, 25);
            lblParts.TabIndex = 4;
            lblParts.Text = "parts";
            // 
            // btnSplit
            // 
            btnSplit.Location = new Point(151, 35);
            btnSplit.Name = "btnSplit";
            btnSplit.Size = new Size(112, 34);
            btnSplit.TabIndex = 5;
            btnSplit.Text = "Split";
            btnSplit.UseVisualStyleBackColor = true;
            btnSplit.Click += btnSplit_Click;
            // 
            // chkUseGpu
            // 
            chkUseGpu.AutoSize = true;
            chkUseGpu.Location = new Point(38, 161);
            chkUseGpu.Name = "chkUseGpu";
            chkUseGpu.Size = new Size(106, 29);
            chkUseGpu.TabIndex = 6;
            chkUseGpu.Text = "Use GPU";
            chkUseGpu.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(33, 234);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(742, 34);
            progressBar.TabIndex = 7;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(32, 316);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(112, 34);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // FormSplitVideo
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnCancel);
            Controls.Add(progressBar);
            Controls.Add(chkUseGpu);
            Controls.Add(btnSplit);
            Controls.Add(lblParts);
            Controls.Add(lblSplitIn);
            Controls.Add(btnBrowse);
            Controls.Add(numParts);
            Controls.Add(lblSelectedFile);
            Name = "FormSplitVideo";
            Text = "FormSplitVideo";
            ((System.ComponentModel.ISupportInitialize)numParts).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblSelectedFile;
        private NumericUpDown numParts;
        private Button btnBrowse;
        private Label lblSplitIn;
        private Label lblParts;
        private Button btnSplit;
        private CheckBox chkUseGpu;
        private ProgressBar progressBar;
        private Button btnCancel;
    }
}