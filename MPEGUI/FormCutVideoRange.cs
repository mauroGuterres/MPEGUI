using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        private List<(TimeSpan Start, TimeSpan End)> removalRanges = new List<(TimeSpan, TimeSpan)>();

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
            // If the data being dragged is a file, allow copy effect.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private async void Form_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the list of dropped files.
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                // Assume the first file is our video.
                string file = files[0];
                string ext = Path.GetExtension(file).ToLowerInvariant();

                // Validate that the file is a supported video type.
                if (ext == ".mp4" || ext == ".mkv" || ext == ".avi" || ext == ".mov" || ext == ".webm")
                {
                    // Update the UI with the file path.
                    lblSelectedFile.Text = file;

                    // Get the video duration using ffprobe (or your preferred method).
                    TimeSpan duration = await FFmpegHelper.GetVideoDuration(file);
                    Debug.WriteLine($"Video duration: {duration}");

                    // Update the trackbar: minimum = 0, maximum = total seconds.
                    rangeTrackBar.Minimum = 0;
                    rangeTrackBar.Maximum = (int)duration.TotalSeconds;
                    rangeTrackBar.LowerValue = 0;
                    rangeTrackBar.UpperValue = (int)duration.TotalSeconds;

                    // Update text boxes with formatted times.
                    txtCutStart.Text = TimeSpan.FromSeconds(rangeTrackBar.LowerValue).ToString(@"hh\:mm\:ss");
                    txtCutEnd.Text = TimeSpan.FromSeconds(rangeTrackBar.UpperValue).ToString(@"hh\:mm\:ss");
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

        private void btnAddRange_Click(object sender, EventArgs e)
        {
            // Parse start and end from textboxes (or get from a range trackbar)
            if (TimeSpan.TryParse(txtCutStart.Text, out TimeSpan start) && TimeSpan.TryParse(txtCutEnd.Text, out TimeSpan end))
            {
                if (start < end)
                {
                    removalRanges.Add((start, end));
                    // Update a ListBox or other UI element to display the added range.
                    listBoxRanges.Items.Add($"{start:hh\\:mm\\:ss} - {end:hh\\:mm\\:ss}");
                }
                else
                {
                    MessageBox.Show("Cut start must be before cut end.");
                }
            }
            else
            {
                MessageBox.Show("Invalid time format. Use HH:MM:SS.");
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

            // Build output file name (e.g., inputfilename_cut.mp4)
            string directory = Path.GetDirectoryName(inputFile);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(inputFile);
            string extension = Path.GetExtension(inputFile);
            string outputFile = Path.Combine(directory, fileNameNoExt + "_cut" + extension);

            btnCutVideo.Enabled = false;
            progressBar.Value = 0;
            CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                await FFmpegHelper.CutVideoMultipleRanges(inputFile, outputFile, removalRanges, progressBar, cts.Token);
                MessageBox.Show("Video processed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCutVideo.Enabled = true;
                progressBar.Value = 0;
                removalRanges.Clear(); // Reset the list for next use.
                listBoxRanges.Items.Clear();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Cancel the operation
            _cts?.Cancel();
        }
    }

}
