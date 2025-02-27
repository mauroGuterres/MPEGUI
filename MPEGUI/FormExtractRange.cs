﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPEGUI
{
    public partial class FormExtractRange : Form
    {
        private CancellationTokenSource _cts;

        public FormExtractRange()
        {
            InitializeComponent();
            // Initialize the range trackbar's event to update the text boxes.
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
                    txtExtractStart.Text = TimeSpan.FromSeconds(rangeTrackBar.LowerValue).ToString(@"hh\:mm\:ss");
                    txtExtractEnd.Text = TimeSpan.FromSeconds(rangeTrackBar.UpperValue).ToString(@"hh\:mm\:ss");
                }
                else
                {
                    MessageBox.Show("Please drop a valid video file.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void RangeTrackBar_RangeChanged(object sender, EventArgs e)
        {
            txtExtractStart.Text = TimeSpan.FromSeconds(rangeTrackBar.LowerValue).ToString(@"hh\:mm\:ss");
            txtExtractEnd.Text = TimeSpan.FromSeconds(rangeTrackBar.UpperValue).ToString(@"hh\:mm\:ss");
        }


        private async void btnSelectFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                openFile.Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    lblSelectedFile.Text = openFile.FileName;
                    // Get video duration and update the trackbar.
                    TimeSpan duration = await FFmpegHelper.GetVideoDuration(openFile.FileName);
                    rangeTrackBar.Minimum = 0;
                    rangeTrackBar.Maximum = (int)duration.TotalSeconds;
                    // Set default values (entire duration)
                    rangeTrackBar.LowerValue = 0;
                    rangeTrackBar.UpperValue = (int)duration.TotalSeconds;
                    // Also update the text boxes.
                    txtExtractStart.Text = TimeSpan.FromSeconds(rangeTrackBar.LowerValue).ToString(@"hh\:mm\:ss");
                    txtExtractEnd.Text = TimeSpan.FromSeconds(rangeTrackBar.UpperValue).ToString(@"hh\:mm\:ss");
                }
            }
        }

        private async void btnExtractRange_Click(object sender, EventArgs e)
        {
            string inputFile = lblSelectedFile.Text;
            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                MessageBox.Show("Please select a valid video file.");
                return;
            }

            // Parse times from the text boxes.
            if (!TimeSpan.TryParse(txtExtractStart.Text, out TimeSpan extractStart))
            {
                MessageBox.Show("Invalid extract start time. Use HH:MM:SS format.");
                return;
            }
            if (!TimeSpan.TryParse(txtExtractEnd.Text, out TimeSpan extractEnd))
            {
                MessageBox.Show("Invalid extract end time. Use HH:MM:SS format.");
                return;
            }

            // Build output file name (e.g., originalname_extracted.ext)
            string directory = Path.GetDirectoryName(inputFile);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(inputFile);
            string extension = Path.GetExtension(inputFile);
            string outputFile = Path.Combine(directory, fileNameNoExt + "_extracted" + extension);

            // Prepare UI
            btnExtractRange.Enabled = false;
            progressBar.Value = 0;
            _cts = new CancellationTokenSource();

            try
            {
                await FFmpegHelper.ExtractVideoRange(inputFile, outputFile, extractStart, extractEnd, progressBar, _cts.Token);
                MessageBox.Show("Extraction completed successfully!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Extraction was canceled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Extraction Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExtractRange.Enabled = true;
                progressBar.Value = 0;
                _cts = null;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }
    }
}
