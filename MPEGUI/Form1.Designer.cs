namespace MPEGUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSelectFile = new Button();
            lblSelectedFile = new Label();
            btnConvert = new Button();
            cmbCodec = new ComboBox();
            txtBitrate = new TextBox();
            trkCRF = new TrackBar();
            btnCancel = new Button();
            lblCRFValue = new Label();
            label1 = new Label();
            progressBar = new ProgressBar();
            menuStrip1 = new MenuStrip();
            ((System.ComponentModel.ISupportInitialize)trkCRF).BeginInit();
            SuspendLayout();
            // 
            // btnSelectFile
            // 
            btnSelectFile.Location = new Point(12, 27);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(141, 34);
            btnSelectFile.TabIndex = 0;
            btnSelectFile.Text = "Select video";
            btnSelectFile.UseVisualStyleBackColor = true;
            btnSelectFile.Click += btnSelectFile_Click;
            // 
            // lblSelectedFile
            // 
            lblSelectedFile.AutoSize = true;
            lblSelectedFile.Location = new Point(24, 77);
            lblSelectedFile.Name = "lblSelectedFile";
            lblSelectedFile.Size = new Size(0, 25);
            lblSelectedFile.TabIndex = 1;
            // 
            // btnConvert
            // 
            btnConvert.Location = new Point(347, 29);
            btnConvert.Name = "btnConvert";
            btnConvert.Size = new Size(141, 34);
            btnConvert.TabIndex = 2;
            btnConvert.Text = "Convert";
            btnConvert.UseVisualStyleBackColor = true;
            btnConvert.Click += btnConvert_Click;
            // 
            // cmbCodec
            // 
            cmbCodec.FormattingEnabled = true;
            cmbCodec.Location = new Point(159, 29);
            cmbCodec.Name = "cmbCodec";
            cmbCodec.Size = new Size(182, 33);
            cmbCodec.TabIndex = 3;
            // 
            // txtBitrate
            // 
            txtBitrate.Location = new Point(415, 77);
            txtBitrate.Name = "txtBitrate";
            txtBitrate.Size = new Size(150, 31);
            txtBitrate.TabIndex = 4;
            // 
            // trkCRF
            // 
            trkCRF.Location = new Point(347, 124);
            trkCRF.Maximum = 28;
            trkCRF.Minimum = 18;
            trkCRF.Name = "trkCRF";
            trkCRF.Size = new Size(318, 69);
            trkCRF.TabIndex = 5;
            trkCRF.Value = 18;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(494, 29);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(141, 34);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblCRFValue
            // 
            lblCRFValue.AutoSize = true;
            lblCRFValue.Location = new Point(478, 168);
            lblCRFValue.Name = "lblCRFValue";
            lblCRFValue.Size = new Size(43, 25);
            lblCRFValue.TabIndex = 7;
            lblCRFValue.Text = "CRF";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(347, 77);
            label1.Name = "label1";
            label1.Size = new Size(62, 25);
            label1.TabIndex = 8;
            label1.Text = "Bitrate";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 238);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(674, 34);
            progressBar.TabIndex = 9;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(698, 24);
            menuStrip1.TabIndex = 10;
            menuStrip1.Text = "menuStrip1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(698, 302);
            Controls.Add(progressBar);
            Controls.Add(label1);
            Controls.Add(lblCRFValue);
            Controls.Add(btnCancel);
            Controls.Add(trkCRF);
            Controls.Add(txtBitrate);
            Controls.Add(cmbCodec);
            Controls.Add(btnConvert);
            Controls.Add(lblSelectedFile);
            Controls.Add(btnSelectFile);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "MPEGUI";
            FormClosing += Form1_FormClosing;
            ((System.ComponentModel.ISupportInitialize)trkCRF).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelectFile;
        private Label lblSelectedFile;
        private Button btnConvert;
        private ComboBox cmbCodec;
        private TextBox txtBitrate;
        private TrackBar trkCRF;
        private Button btnCancel;
        private Label lblCRFValue;
        private Label label1;
        private ProgressBar progressBar;
        private MenuStrip menuStrip1;
    }
}
