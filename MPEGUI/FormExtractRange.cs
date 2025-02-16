using System;
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
        }

        private void RangeTrackBar_RangeChanged(object sender, EventArgs e)
        {
            // Update text boxes based on trackbar values (assumed in seconds)
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
