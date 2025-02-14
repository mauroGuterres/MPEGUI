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

            bool useGpu = chkUseGpu.Checked;

            btnSplit.Invoke((Action)(() => btnSplit.Enabled = false));
            btnCancel.Invoke((Action)(() => btnCancel.Enabled = true));
            progressBar.Invoke((Action)(() => progressBar.Value = 0));

            CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                await Task.Run(async () =>
                {
                    await FFmpegHelper.SplitVideo(lblSelectedFile.Text, parts, progressBar, cts.Token);
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
            FFmpegHelper.CancelOperation();
        }


        private async Task<TimeSpan> GetVideoDuration(string inputFile)
        {
            TimeSpan duration = TimeSpan.Zero;

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{inputFile}\"",
                    RedirectStandardError = true, // FFmpeg outputs duration info to stderr
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            Match match = Regex.Match(output, @"Duration:\s(\d+):(\d+):(\d+.\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                int hours = int.Parse(match.Groups[1].Value);
                int minutes = int.Parse(match.Groups[2].Value);
                double seconds = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

                duration = new TimeSpan(0, hours, minutes, (int)seconds);
            }

            return duration;
        }

        private async Task SplitVideo(string inputFile, double partDuration, int parts)
        {
            string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Splits");
            Directory.CreateDirectory(outputFolder);

            List<Task> splitTasks = new List<Task>();

            for (int i = 0; i < parts; i++)
            {
                double startTime = i * partDuration;
                string outputFile = Path.Combine(outputFolder, $"part_{i + 1}{Path.GetExtension(inputFile)}");

                string ffmpegArgs = $"-i \"{inputFile}\" -ss {startTime.ToString(CultureInfo.InvariantCulture)} -t {partDuration.ToString(CultureInfo.InvariantCulture)} -c:v libx264 -preset ultrafast -crf 23 -c:a copy \"{outputFile}\"";



                splitTasks.Add(RunFFmpegAsync(ffmpegArgs));
            }

            await Task.WhenAll(splitTasks);
        }

        private async Task RunFFmpegAsync(string arguments)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
        }
    }

}
