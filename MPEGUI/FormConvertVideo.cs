using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MPEGUI
{
    public partial class FormConvertVideo : Form
    {
        private CancellationTokenSource _cts;
        Dictionary<string, string> codecMap;
        private Process _ffmpegProcess;
        private bool _isConversionRunning = false;
        private TimeSpan _totalDuration = TimeSpan.Zero; // Store total duration for progress tracking

        public FormConvertVideo()
        {
            InitializeComponent();
            LoadFFmpegCodecs();


            // Populate codec selection
            /* cmbCodec.Items.Add("libx264 (H.264)");
             cmbCodec.Items.Add("libx265 (H.265)");
             cmbCodec.Items.Add("vp9 (VP9)");
             cmbCodec.Items.Add("av1 (AV1)");
             cmbCodec.Items.Add("copy (No Re-encoding)");
             cmbCodec.SelectedIndex = 0; // Default to H.264*/

            // Setup Progress Bar
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            trkCRF.Minimum = 18; // Best quality limit
            trkCRF.Maximum = 28; // Best compression limit
            trkCRF.Value = 23; // Default CRF
            lblCRFValue.Text = $"CRF: {trkCRF.Value}"; // Show initial CRF
            trkCRF.Scroll += TrkCRF_Scroll; // Event to update label

            codecMap = new Dictionary<string, string>
            {
                { "libx264", "H.264 (libx264)" },
                { "libx265", "H.265 / HEVC (libx265)" },
                { "vp8", "VP8" },
                { "libvpx", "VP8 (libvpx)" },
                { "vp9", "VP9" },
                { "libvpx-vp9", "VP9 (libvpx-vp9)" },
                { "mpeg4", "MPEG-4 Part 2" },
                { "libxvid", "MPEG-4 (Xvid)" },
                { "h264_nvenc", "H.264 (NVIDIA NVENC)" },
                { "hevc_nvenc", "H.265 (NVIDIA NVENC)" },
                { "h264_qsv", "H.264 (Intel QuickSync)" },
                { "hevc_qsv", "H.265 (Intel QuickSync)" },
                { "prores", "Apple ProRes" },
                { "flv", "Flash Video (FLV)" },
                { "mjpeg", "Motion JPEG" },
                { "libtheora", "Theora" },
                { "gif", "GIF Animation" },
                { "libaom-av1", "AV1 (libaom)" },
                { "libsvtav1", "AV1 (SVT-AV1)" }
            };

            MenuStrip menuStrip = new MenuStrip();

            // Create "File" Menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");

            // Create "Split Video" Menu Item
            ToolStripMenuItem splitVideoMenu = new ToolStripMenuItem("Split Video");
            splitVideoMenu.Click += SplitVideoMenu_Click;


            // Create new "Cut Video Range" Menu Item
            ToolStripMenuItem cutVideoMenu = new ToolStripMenuItem("Cut Video Range");
            cutVideoMenu.Click += CutVideoMenu_Click;
            fileMenu.DropDownItems.Add(cutVideoMenu);


            // Add to Menu
            fileMenu.DropDownItems.Add(splitVideoMenu);
            menuStrip.Items.Add(fileMenu);

            // Add MenuStrip to Form
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

        }

        // Open "Split Video" Form
        private void SplitVideoMenu_Click(object sender, EventArgs e)
        {
            FormSplitVideo splitForm = new FormSplitVideo();
            splitForm.Show();
        }

        private void CutVideoMenu_Click(object sender, EventArgs e)
        {
            FormCutVideoRange formCutVideoRange = new FormCutVideoRange();
            formCutVideoRange.Show();
        }


        private void TrkCRF_Scroll(object sender, EventArgs e)
        {
            lblCRFValue.Text = $"CRF: {trkCRF.Value}"; // Show CRF value while adjusting
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.mkv;*.avi;*.mov",
                Title = "Select a Video File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                lblSelectedFile.Text = openFileDialog.FileName;
                _totalDuration =  GetVideoDuration(openFileDialog.FileName); // Get total duration
            }
        }

        private async void btnConvert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblSelectedFile.Text) || !File.Exists(lblSelectedFile.Text))
            {
                MessageBox.Show("Please select a valid video file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string inputFile = lblSelectedFile.Text;
            string outputFile = Path.ChangeExtension(inputFile, ".mp4");

            string selectedFriendlyName = cmbCodec.SelectedItem.ToString();
            string selectedCodec = codecMap.FirstOrDefault(x => x.Value == selectedFriendlyName).Key ?? "libx264";

            string bitrate = "";
            int crf = trkCRF.Value;

            btnConvert.Invoke((Action)(() => btnConvert.Enabled = false));
            btnCancel.Invoke((Action)(() => btnCancel.Enabled = true));
            progressBar.Invoke((Action)(() => progressBar.Value = 0));

            _isConversionRunning = true;

            // Use the class-level CancellationTokenSource
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Run(async () =>
                {
                    await FFmpegHelper.ConvertVideo(inputFile, outputFile, selectedCodec, bitrate, crf, progressBar, _cts.Token);
                });

                MessageBox.Show("Conversion completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Conversion was canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "FFmpeg Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnConvert.Invoke((Action)(() => btnConvert.Enabled = true));
                btnCancel.Invoke((Action)(() => btnCancel.Enabled = false));
                progressBar.Invoke((Action)(() => progressBar.Value = 0));

                // Reset the token source once done
                _cts = null;
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Cancel the conversion process via the class-level cancellation token source.
            _cts?.Cancel();
        }                

        private TimeSpan GetVideoDuration(string filePath)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{filePath}\"",
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // Extract duration from FFmpeg output (e.g., "Duration: 00:05:30.45")
                Match match = Regex.Match(output, @"Duration: (\d+):(\d+):(\d+)");
                if (match.Success)
                {
                    int hours = int.Parse(match.Groups[1].Value);
                    int minutes = int.Parse(match.Groups[2].Value);
                    int seconds = int.Parse(match.Groups[3].Value);
                    return new TimeSpan(hours, minutes, seconds);
                }
            }
            catch { }

            return TimeSpan.Zero;
        }

        private async void LoadFFmpegCodecs()
        {
            await Task.Run(() =>
            {
                try
                {
                    Process process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "ffmpeg",
                            Arguments = "-codecs",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Debug: Save FFmpeg full output
                    System.IO.File.WriteAllText("ffmpeg_codecs_debug.txt", output);

                    List<string> extractedEncoders = new List<string>();

                    // New Regex: Extract encoders listed inside "(encoders: ... )"
                    MatchCollection matches = Regex.Matches(output, @"\(encoders:\s*([a-z0-9_ ]+)\)", RegexOptions.Multiline);

                    foreach (Match match in matches)
                    {
                        string[] encoders = match.Groups[1].Value.Split(' ');
                        extractedEncoders.AddRange(encoders);
                    }

                    // Debug: Save extracted video encoders before filtering
                    System.IO.File.WriteAllText("extracted_encoders_debug.txt", string.Join("\n", extractedEncoders));

                    // List of common, widely used encoders
                    

                    List<KeyValuePair<string, string>> codecs = new List<KeyValuePair<string, string>>();

                    foreach (string encoderName in extractedEncoders)
                    {
                        if (codecMap.ContainsKey(encoderName)) // Only add known/common encoders
                        {
                            string friendlyName = codecMap[encoderName];
                            codecs.Add(new KeyValuePair<string, string>(encoderName, friendlyName));
                        }
                    }

                    // Debug: Save filtered encoders to check if anything matched
                    System.IO.File.WriteAllText("filtered_encoders_debug.txt", string.Join("\n", codecs.Select(c => c.Value)));

                    // Update UI on the main thread
                    Invoke(new Action(() =>
                    {
                        cmbCodec.Items.Clear();
                        cmbCodec.Items.Add("YouTube Full HD (1080p)");
                        foreach (var codec in codecs)
                        {
                            cmbCodec.Items.Add(codec.Value);
                        }
                        if (cmbCodec.Items.Count > 0)
                        {
                            cmbCodec.SelectedIndex = 0; // Select first codec as default
                        }
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading FFmpeg codecs: " + ex.Message);
                }
            });
        }        

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                _ffmpegProcess.Kill();
                _ffmpegProcess.Dispose();
            }
        }
    }
}
