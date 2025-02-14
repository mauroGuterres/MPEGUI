using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MPEGUI
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public static class FFmpegHelper
    {
        private static Process _ffmpegProcess;
        private static CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Run an FFmpeg command asynchronously with progress tracking.
        /// </summary>
        public static async Task RunFFmpegAsync(
      string arguments,
      ProgressBar progressBar,
      TimeSpan partDuration,
      TimeSpan totalDuration,
      CancellationToken cancellationToken,
      Action<int> updateProgress,
      bool isSplitProcess)
        {
            using (Process ffmpegProcess = new Process()) // ✅ Each FFmpeg instance is isolated
            {
                ffmpegProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = GetFFmpegPath(),
                    Arguments = arguments + " -progress pipe:1",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                ffmpegProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data) && e.Data.StartsWith("out_time_ms="))
                    {
                        string timeMsStr = e.Data.Split('=')[1].Trim();
                        if (long.TryParse(timeMsStr, out long timeMs))
                        {
                            double elapsedSeconds = timeMs / 1_000_000.0;
                            double progressPercent = (elapsedSeconds / partDuration.TotalSeconds) * 100;

                            if (progressPercent <= 100)
                            {
                                if (!isSplitProcess)
                                {
                                    progressBar.Invoke((Action)(() => progressBar.Value = (int)progressPercent));
                                }
                                else
                                {
                                    updateProgress?.Invoke((int)progressPercent);
                                }
                            }
                        }
                    }
                };

                // ✅ Start the process
                ffmpegProcess.Start();
                ffmpegProcess.BeginOutputReadLine();
                ffmpegProcess.BeginErrorReadLine();

                await ffmpegProcess.WaitForExitAsync();

                if (cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        ffmpegProcess.Kill();
                    }
                    catch { /* Ignore process kill errors */ }
                    throw new OperationCanceledException();
                }
            }
        }


        // ✅ Overload function now also uses GetFFmpegPath()
        public static async Task RunFFmpegAsync(string arguments, ProgressBar progressBar, TimeSpan videoDuration, CancellationToken cancellationToken)
        {
            await RunFFmpegAsync(arguments, progressBar, videoDuration, videoDuration, cancellationToken, null, false);
        }

        /// <summary>
        /// Convert a video file while tracking progress.
        /// </summary>
        public static async Task ConvertVideo(string inputFile, string outputFile, string codec, string bitrate, int crf, ProgressBar progressBar, CancellationToken cancellationToken)
        {
            TimeSpan videoDuration = await GetVideoDuration(inputFile);
            if (videoDuration == TimeSpan.Zero) throw new Exception("Failed to get video duration.");

            progressBar.Invoke((Action)(() =>
            {
                progressBar.Maximum = 100;
                progressBar.Value = 0;
            }));

            // ✅ Determine correct output extension based on the codec
            string newExtension = GetOutputExtension(codec);
            outputFile = Path.ChangeExtension(outputFile, newExtension);

            // ✅ Select correct audio codec for the chosen format
            string audioCodec = newExtension == ".webm" ? "libopus" : "aac";

            // ✅ Base FFmpeg command
            string ffmpegArgs = $"-i \"{inputFile}\" -c:v {codec} -preset fast";

            // ✅ Handle CRF correctly based on the codec
            if (SupportsCRF(codec))
            {
                ffmpegArgs += $" -crf {crf}"; // ✅ Standard CRF for supported encoders
            }
            else if (codec.Contains("nvenc"))
            {
                ffmpegArgs += $" -cq {crf}"; // ✅ CQ mode for NVIDIA NVENC
                ffmpegArgs += $" -b:v 0 -maxrate {GetNVENCBitrate(crf)}k -bufsize {GetNVENCBitrate(crf) * 2}k"; // ✅ Limit excessive file sizes
            }
            else if (codec.Contains("qsv"))
            {
                ffmpegArgs += $" -global_quality {crf}"; // ✅ ICQ mode for Intel Quick Sync
            }
            else if (codec.Contains("amf"))
            {
                ffmpegArgs += $" -qp {crf}"; // ✅ QP mode for AMD AMF
            }
            else if (RequiresBitrate(codec) && !string.IsNullOrEmpty(bitrate))
            {
                ffmpegArgs += $" -b:v {bitrate}"; // ✅ For codecs requiring a fixed bitrate
            }

            // ✅ Set audio codec
            ffmpegArgs += $" -c:a {audioCodec} -b:a 192k";

            // ✅ Ensure correct metadata handling
            ffmpegArgs += " -map_metadata 0 -movflags +faststart -fflags +genpts";

            // ✅ Output file
            ffmpegArgs += $" \"{outputFile}\"";

            // ✅ Run FFmpeg conversion
            await RunFFmpegAsync(ffmpegArgs, progressBar, videoDuration, cancellationToken);
        }

        private static bool SupportsCRF(string codec)
        {
            return codec.Contains("libx264") || codec.Contains("libx265") ||
                   codec.Contains("libvpx") || codec.Contains("libvpx-vp9") ||
                   codec.Contains("libaom");
        }


        private static bool RequiresBitrate(string codec)
        {
            return codec.Contains("mpeg4") || codec.Contains("msmpeg4") || codec.Contains("libxvid") ||
                   codec.Contains("prores") || codec.Contains("dnxhd") || codec.Contains("flv") ||
                   codec.Contains("mpeg1video") || codec.Contains("mpeg2video");
        }

        private static string GetOutputExtension(string codec)
        {
            if (codec.Contains("libvpx") || codec.Contains("libaom")) return ".webm";
            if (codec.Contains("libx264") || codec.Contains("libx265") || codec.Contains("h264") || codec.Contains("hevc")) return ".mp4";
            if (codec.Contains("ffv1") || codec.Contains("vp9") || codec.Contains("libaom")) return ".mkv";
            if (codec.Contains("mpeg4") || codec.Contains("msmpeg4") || codec.Contains("libxvid")) return ".avi";
            if (codec.Contains("prores") || codec.Contains("dnxhd")) return ".mov";
            if (codec.Contains("flv")) return ".flv";
            if (codec.Contains("mpeg1video") || codec.Contains("mpeg2video")) return ".mpg";

            return ".mp4"; // Default to MP4 if unknown
        }

        private static int GetNVENCBitrate(int crf)
        {
            if (crf <= 18)
                return 20000; // Max 20 Mbps (prevent huge files)
            else if (crf <= 22)
                return 15000; // Max 15 Mbps
            else if (crf <= 26)
                return 10000; // Max 10 Mbps
            else
                return 5000; // Max 5 Mbps
        }

        
        /// <summary>
        /// Get the duration of a video file using FFmpeg.
        /// </summary>
        public static async Task<TimeSpan> GetVideoDuration(string inputFile)
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

        /// <summary>
        /// Splits a video into equal parts using FFmpeg with progress tracking.
        /// </summary>
        public static async Task SplitVideo(string inputFile, int parts, ProgressBar progressBar, CancellationToken cancellationToken)
        {
            string inputDirectory = Path.GetDirectoryName(inputFile);
            string outputFolder = Path.Combine(inputDirectory, $"{Path.GetFileNameWithoutExtension(inputFile)}_split");

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            TimeSpan videoDuration = await GetVideoDuration(inputFile);
            if (videoDuration == TimeSpan.Zero) throw new Exception("Failed to get video duration.");

            progressBar.Invoke((Action)(() =>
            {
                progressBar.Maximum = 100;
                progressBar.Value = 0;
            }));

            TimeSpan partDuration = TimeSpan.FromSeconds(videoDuration.TotalSeconds / parts);
            List<Task> splitTasks = new List<Task>();

            int completedParts = 0;
            object progressLock = new object(); // 🔒 Prevents race conditions

            for (int i = 0; i < parts; i++)
            {
                string outputFile = Path.Combine(outputFolder, $"{Path.GetFileNameWithoutExtension(inputFile)}_part{i + 1}{Path.GetExtension(inputFile)}");
                string formattedStartTime = TimeSpan.FromSeconds(i * partDuration.TotalSeconds).ToString(@"hh\:mm\:ss\.fff");
                string formattedPartDuration = partDuration.ToString(@"hh\:mm\:ss\.fff");

                string ffmpegArgs = $"-i \"{inputFile}\" -c copy -y -ss {formattedStartTime} -t {formattedPartDuration} \"{outputFile}\"";

                Debug.WriteLine($"Running FFmpeg Split Command: {ffmpegArgs}"); // ✅ Debugging output

                Task splitTask = RunFFmpegAsync(ffmpegArgs, progressBar, partDuration, videoDuration, cancellationToken, (partProgress) =>
                {
                    lock (progressLock)
                    {
                        // ✅ More accurate progress tracking
                        double elapsedSeconds = (completedParts + (partProgress / 100.0)) * partDuration.TotalSeconds;
                        int newProgress = (int)((elapsedSeconds / videoDuration.TotalSeconds) * 100);

                        progressBar.Invoke((Action)(() =>
                        {
                            progressBar.Value = Math.Min(100, Math.Max(0, newProgress)); // ✅ Ensures 0-100% range
                        }));
                    }
                }, true);

                splitTasks.Add(splitTask);
            }

            await Task.WhenAll(splitTasks);

            // ✅ Ensure the progress bar reaches exactly 100% at the end
            progressBar.Invoke((Action)(() => progressBar.Value = 100));
        }






        private static string GetFFmpegPath()
        {
            try
            {
                // ✅ Check if FFmpeg is available in system PATH
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "ffmpeg";
                    process.StartInfo.Arguments = "-version";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        return "ffmpeg"; // ✅ Use system FFmpeg
                    }
                }
            }
            catch
            {
                // ❌ FFmpeg not found in PATH, fallback to embedded FFmpeg
            }

            // ✅ Fallback: Use FFmpeg stored in the app directory
            string localFFmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "ffmpeg.exe");

            if (File.Exists(localFFmpeg))
            {
                return localFFmpeg;
            }

            throw new Exception("FFmpeg not found. Please install FFmpeg or ensure 'ffmpeg.exe' is in the application folder.");
        }






        /// <summary>
        /// Cancels the current FFmpeg operation.
        /// </summary>
        public static void CancelOperation()
        {
            _cancellationTokenSource?.Cancel();
            _ffmpegProcess?.Kill();
        }
    }

}


