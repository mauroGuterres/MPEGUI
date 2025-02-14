using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MPEGUI
{
    public partial class FormSplitVideo : Form
    {
        private CancellationTokenSource _cts;
        public FormSplitVideo()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov;*.flv",
                Title = "Select a Video File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                lblSelectedFile.Text = openFileDialog.FileName;
            }
        }

        private async void btnSplit_Click(object sender, EventArgs e)
        {
            if (lblSelectedFile.Text == "No file selected" || !File.Exists(lblSelectedFile.Text))
            {
                MessageBox.Show("Please select a valid video file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int parts = (int)numParts.Value;
            if (parts < 2)
            {
                MessageBox.Show("Please enter at least 2 parts!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }            

            btnSplit.Invoke((Action)(() => btnSplit.Enabled = false));
            btnCancel.Invoke((Action)(() => btnCancel.Enabled = true));
            progressBar.Invoke((Action)(() => progressBar.Value = 0));

            _cts = new CancellationTokenSource();

            try
            {
                await Task.Run(async () =>
                {
                    await FFmpegHelper.SplitVideo(lblSelectedFile.Text, parts, progressBar, _cts.Token);
                });

                MessageBox.Show("Video split successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Splitting was canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnSplit.Invoke((Action)(() => btnSplit.Enabled = true));
                btnCancel.Invoke((Action)(() => btnCancel.Enabled = false));
                progressBar.Invoke((Action)(() => progressBar.Value = 0));
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }
       

               
    }

}
