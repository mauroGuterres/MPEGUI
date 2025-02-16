using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPEGUI
{
    public partial class FormCutVideoRange : Form
    {
        private CancellationTokenSource _cts;

        public FormCutVideoRange()
        {
            InitializeComponent();

            // Basic setup
            /*progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;*/
            // When a file is selected, set the rangeTrackBar's Minimum/Maximum (in seconds)
            // For example, if video duration is known:
            // rangeTrackBar.Minimum = 0;
            // rangeTrackBar.Maximum = (int)videoDuration.TotalSeconds;
            // Optionally, set default lower/upper values.
            rangeTrackBar.RangeChanged += RangeTrackBar_RangeChanged;

            // Enable drag and drop on the form.
            this.AllowDrop = true;

            // Register drag and drop events.
            this.DragEnter += new DragEventHandler(Form_DragEnter);
            this.DragDrop += new DragEventHandler(Form_DragDrop);

        }

        private void Form_DragEnter(object sender, DragEventArgs e)
        {
            // Check if the data is a file.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form_DragDrop(object sender, DragEventArgs e)
        {
            // Get the file list.
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                // Assume the first file is the video file.
                string videoFile = files[0];

                // Optionally, you can filter based on file extension if needed.
                string ext = Path.GetExtension(videoFile).ToLower();
                if (ext == ".mp4" || ext == ".mkv" || ext == ".avi" || ext == ".mov" || ext == ".webm")
                {
                    // Set the label to the file path (or update your UI accordingly).
                    lblSelectedFile.Text = videoFile;

                    // You can also trigger additional logic here, like updating the range trackbar
                    // by reading the video's duration.
                }
                else
                {
                    MessageBox.Show("Please drop a valid video file.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void RangeTrackBar_RangeChanged(object sender, EventArgs e)
        {
            // Update the text boxes based on the range trackbar values (assumed in seconds)
            txtCutStart.Text = TimeSpan.FromSeconds(rangeTrackBar.LowerValue).ToString(@"hh\:mm\:ss");
            txtCutEnd.Text = TimeSpan.FromSeconds(rangeTrackBar.UpperValue).ToString(@"hh\:mm\:ss");
        }

        private async void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    lblSelectedFile.Text = openFile.FileName;
                    // Get video duration (assuming GetVideoDuration returns a Task<TimeSpan>)
                    TimeSpan duration = await FFmpegHelper.GetVideoDuration(openFile.FileName);

                    // Update the RangeTrackBar to reflect the video's total duration in seconds
                    rangeTrackBar.Minimum = 0;
                    rangeTrackBar.Maximum = (int)duration.TotalSeconds;
                    rangeTrackBar.LowerValue = 0;
                    rangeTrackBar.UpperValue = (int)duration.TotalSeconds;
                }
            }
        }


        private async void btnCutVideo_Click(object sender, EventArgs e)
        {
            string inputFile = lblSelectedFile.Text;
            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                MessageBox.Show("Please select a valid video file.");
                return;
            }

            // Parse user times (HH:MM:SS)
            if (!TimeSpan.TryParse(txtCutStart.Text, out TimeSpan cutStart))
            {
                MessageBox.Show("Invalid Cut Start time. Use HH:MM:SS format.");
                return;
            }
            if (!TimeSpan.TryParse(txtCutEnd.Text, out TimeSpan cutEnd))
            {
                MessageBox.Show("Invalid Cut End time. Use HH:MM:SS format.");
                return;
            }

            // Build output file path (e.g., add "_cut" to the name)
            string directory = Path.GetDirectoryName(inputFile);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(inputFile);
            string extension = Path.GetExtension(inputFile);
            string outputFile = Path.Combine(directory, fileNameNoExt + "_cut" + extension);

            // Prepare for progress + cancellation
            btnCutVideo.Enabled = false;
            progressBar.Value = 0;
            _cts = new CancellationTokenSource();

            try
            {
                await FFmpegHelper.CutVideoRange(
                    inputFile,
                    outputFile,
                    cutStart,                    
                    cutEnd,
                    progressBar,
                    _cts.Token
                );

                MessageBox.Show("Video cut successfully!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Cut operation was canceled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Cut Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCutVideo.Enabled = true;
                progressBar.Value = 0;
                _cts = null;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Cancel the operation
            _cts?.Cancel();
        }
    }

}
