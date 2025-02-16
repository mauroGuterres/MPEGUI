using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Management;


namespace MPEGUI
{
    public static class FFmpegHelper
    {
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
            using (Process ffmpegProcess = new Process())
            {
                ffmpegProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = GetFFmpegPath(),
                    Arguments = arguments + " -progress pipe:1",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true, // for sending 'q' on cancel
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                ffmpegProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data) && e.Data.StartsWith("out_time_ms="))
                    {
                        // Extract time in ms
                        string timeMsStr = e.Data.Split('=')[1].Trim();
                        if (long.TryParse(timeMsStr, out long timeMs))
                        {
                            double elapsedSeconds = timeMs / 1_000_000.0;
                            double progressPercent = (elapsedSeconds / partDuration.TotalSeconds) * 100;

                            if (progressPercent <= 100)
                            {
                                // Update in-app progress bar
                                progressBar.Invoke((Action)(() =>
                                {
                                    int progressVal = (int)progressPercent;
                                    progressBar.Value = progressVal;

                                    // Update Windows taskbar progress
                                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                                    TaskbarManager.Instance.SetProgressValue(progressVal, 100);
                                }));
                            }
                        }
                    }
                };

                ffmpegProcess.Start();
                ffmpegProcess.BeginOutputReadLine();
                ffmpegProcess.BeginErrorReadLine();

                // Graceful cancellation approach (send 'q' to finalize the file)
                using (cancellationToken.Register(() =>
                {
                    if (!ffmpegProcess.HasExited)
                    {
                        try
                        {
                            ffmpegProcess.StandardInput.Write("q");
                            ffmpegProcess.StandardInput.Close();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error sending 'q' to FFmpeg: " + ex.Message);
                        }
                    }
                }))
                {
                    await ffmpegProcess.WaitForExitAsync(cancellationToken);
                }

                // Reset taskbar progress when the operation completes or is canceled
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);

                // If canceled, throw
                cancellationToken.ThrowIfCancellationRequested();
            }
        }




        // Overload for when you only need to supply a single duration
        public static async Task RunFFmpegAsync(string arguments, ProgressBar progressBar, TimeSpan videoDuration, CancellationToken cancellationToken)
        {
            await RunFFmpegAsync(arguments, progressBar, videoDuration, videoDuration, cancellationToken, null, false);
        }

        /// <summary>
        /// Convert a video file while tracking progress.
        /// </summary>
        public static async Task ConvertVideo(
    string inputFile,
    string outputFile,
    string codec,
    string bitrate,
    int crf,
    ProgressBar progressBar,
    CancellationToken cancellationToken)
        {
            TimeSpan videoDuration = await GetVideoDuration(inputFile);
            if (videoDuration == TimeSpan.Zero)
                throw new Exception("Failed to get video duration.");

            progressBar.Invoke((Action)(() =>
            {
                progressBar.Maximum = 100;
                progressBar.Value = 0;
            }));

            // Determine correct output extension based on the codec
            string newExtension = GetOutputExtension(codec);
            // Use the final output file directly (no temporary file)
            outputFile = Path.ChangeExtension(outputFile, newExtension);

            // Select the correct audio codec
            string audioCodec = newExtension == ".webm" ? "libopus" : "aac";

            // Build the FFmpeg command
            string ffmpegArgs = $"-i \"{inputFile}\"";

            if (codec == "YouTube Full HD (1080p)")
            {
                ffmpegArgs += $" -c:v libx264 -preset slow -crf {crf} -b:v 0 -maxrate 10000k -bufsize 16000k";
                ffmpegArgs += $" -vf \"scale=1920:1080,fps=60\"";
            }
            else
            {
                ffmpegArgs += $" -c:v {codec} -preset fast";
                if (SupportsCRF(codec))
                {
                    ffmpegArgs += $" -crf {crf}";
                }
                else if (codec.Contains("nvenc"))
                {
                    ffmpegArgs += $" -cq {crf}";
                    ffmpegArgs += $" -b:v 0 -maxrate {GetNVENCBitrate(crf)}k -bufsize {GetNVENCBitrate(crf) * 2}k";
                }
                else if (codec.Contains("qsv"))
                {
                    ffmpegArgs += $" -global_quality {crf}";
                }
                else if (codec.Contains("amf"))
                {
                    ffmpegArgs += $" -qp {crf}";
                }
                else if (RequiresBitrate(codec) && !string.IsNullOrEmpty(bitrate))
                {
                    ffmpegArgs += $" -b:v {bitrate}";
                }
            }

            ffmpegArgs += $" -c:a {audioCodec} -b:a 192k";
            ffmpegArgs += " -map_metadata 0 -movflags +faststart -fflags +genpts";
            // Write directly to the final output file
            ffmpegArgs += $" \"{outputFile}\"";

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
            if (videoDuration == TimeSpan.Zero)
                throw new Exception("Failed to get video duration.");

            progressBar.Invoke((Action)(() =>
            {
                progressBar.Maximum = 100;
                progressBar.Value = 0;
            }));

            TimeSpan partDuration = TimeSpan.FromSeconds(videoDuration.TotalSeconds / parts);

            // Array to hold progress for each part (0 to 100)
            double[] partProgressValues = new double[parts];

            List<Task> splitTasks = new List<Task>();

            for (int i = 0; i < parts; i++)
            {
                // Capture the current part index for the lambda
                int partIndex = i;

                string outputFile = Path.Combine(outputFolder, $"{Path.GetFileNameWithoutExtension(inputFile)}_part{partIndex + 1}{Path.GetExtension(inputFile)}");
                string formattedStartTime = TimeSpan.FromSeconds(partIndex * partDuration.TotalSeconds).ToString(@"hh\:mm\:ss\.fff");
                string formattedPartDuration = partDuration.ToString(@"hh\:mm\:ss\.fff");

                string ffmpegArgs = $"-i \"{inputFile}\" -c copy -y -ss {formattedStartTime} -t {formattedPartDuration} \"{outputFile}\"";

                Debug.WriteLine($"Running FFmpeg Split Command: {ffmpegArgs}");

                Task splitTask = RunFFmpegAsync(ffmpegArgs, progressBar, partDuration, videoDuration, cancellationToken, (currentPartProgress) =>
                {
                    // Update this part's progress value
                    lock (partProgressValues)
                    {
                        partProgressValues[partIndex] = currentPartProgress;
                        // Compute overall progress as the average of all part progress values.
                        double overallProgress = partProgressValues.Average();
                        progressBar.Invoke((Action)(() =>
                        {
                            progressBar.Value = Math.Min(100, (int)overallProgress);
                        }));
                    }
                }, true);

                splitTasks.Add(splitTask);
            }

            await Task.WhenAll(splitTasks);

            // Ensure the progress bar reaches 100% at the end.
            progressBar.Invoke((Action)(() => progressBar.Value = 100));
        }

        private static string GetFFmpegPath()
        {
            try
            {
                // Check if FFmpeg is available in system PATH
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
                        return "ffmpeg";
                    }
                }
            }
            catch
            {
                // FFmpeg not found in PATH, fallback to embedded FFmpeg
            }

            string localFFmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "ffmpeg.exe");
            if (File.Exists(localFFmpeg))
            {
                return localFFmpeg;
            }

            throw new Exception("FFmpeg not found. Please install FFmpeg or ensure 'ffmpeg.exe' is in the application folder.");
        }

        

public static (string encoder, string options) GetHardwareEncoderOptions()
    {
        // Default to CPU encoder (lossless libx264)
        string encoder = "libx264";
        string options = "-preset veryslow -crf 0";

        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject mo in searcher.Get())
                {
                    string name = mo["Name"]?.ToString().ToLower() ?? "";
                    if (name.Contains("nvidia"))
                    {
                        encoder = "h264_nvenc";
                        options = "-preset llhq -qp 0";
                        break;
                    }
                    else if (name.Contains("amd") || name.Contains("radeon"))
                    {
                        encoder = "h264_amf";
                        options = "-qp 0";
                        break;
                    }
                    else if (name.Contains("intel"))
                    {
                        encoder = "h264_qsv";
                        options = "-global_quality 1"; // near lossless mode
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error if necessary; keep fallback encoder.
        }

        return (encoder, options);
    }


        /// <summary>
        /// Cuts out a specified portion (cutStart to cutEnd) from the video
        /// and concatenates the remaining parts into a single output file.
        /// </summary>
        public static async Task CutVideoRange(
      string inputFile,
      string outputFile,
      TimeSpan cutStart,
      TimeSpan cutEnd,
      ProgressBar progressBar,
      CancellationToken cancellationToken)
        {
            // 1. Get total duration of the input video.
            TimeSpan totalDuration = await GetVideoDuration(inputFile);
            if (totalDuration == TimeSpan.Zero)
                throw new Exception("Failed to get video duration.");

            // Validate cut times.
            if (cutStart < TimeSpan.Zero || cutEnd > totalDuration || cutStart >= cutEnd)
                throw new Exception("Invalid cut range.");

            // 2. Adjust cut times to the nearest keyframes.
            TimeSpan adjustedCutStart = await GetNearestKeyframeTime(inputFile, cutStart, searchBackward: true, cancellationToken: cancellationToken);
            TimeSpan adjustedCutEnd = await GetNearestKeyframeTime(inputFile, cutEnd, searchBackward: false, cancellationToken: cancellationToken);

            // 3. Determine container-specific flags based on the output file extension.
            string fileExt = Path.GetExtension(outputFile).ToLowerInvariant();
            string extractionFlags;
            string concatFlags;

            switch (fileExt)
            {
                case ".mp4":
                    extractionFlags = "-avoid_negative_ts make_zero -reset_timestamps 1";
                    // For MP4, move the moov atom to the beginning.
                    concatFlags = "-reset_timestamps 1 -movflags +faststart";
                    break;
                case ".mkv":
                    extractionFlags = "-avoid_negative_ts make_zero -reset_timestamps 1 -fflags +genpts";
                    // MKV doesn't need faststart.
                    concatFlags = "-reset_timestamps 1 -copyts -start_at_zero";
                    break;
                default:
                    // Default flags for other formats.
                    extractionFlags = "-avoid_negative_ts make_zero -reset_timestamps 1";
                    concatFlags = "-reset_timestamps 1 -copyts -start_at_zero";
                    break;
            }

            // 4. Prepare intermediate file paths.
            string tempDir = Path.GetDirectoryName(outputFile) ?? AppDomain.CurrentDomain.BaseDirectory;
            string part1File = Path.Combine(tempDir, "part1_cut" + fileExt);
            string part2File = Path.Combine(tempDir, "part2_cut" + fileExt);

            // 5. Extract Part 1 (from 0 to adjustedCutStart) using stream copy.
            if (adjustedCutStart > TimeSpan.Zero)
            {
                string part1Args = $"-i \"{inputFile}\" -ss 0 -to {adjustedCutStart} -c copy {extractionFlags} \"{part1File}\"";
                await RunFFmpegAsync(part1Args, progressBar, adjustedCutStart, cancellationToken);
            }

            // 6. Extract Part 2 (from adjustedCutEnd to totalDuration) using stream copy.
            if (adjustedCutEnd < totalDuration)
            {
                string part2Args = $"-i \"{inputFile}\" -ss {adjustedCutEnd} -to {totalDuration} -c copy {extractionFlags} \"{part2File}\"";
                TimeSpan part2Duration = totalDuration - adjustedCutEnd;
                await RunFFmpegAsync(part2Args, progressBar, part2Duration, cancellationToken);
            }

            // 7. Concatenate the parts.
            bool part1Exists = File.Exists(part1File);
            bool part2Exists = File.Exists(part2File);
            string concatOutput = outputFile; // temporary output for concatenation step

            if (!part1Exists && !part2Exists)
            {
                throw new Exception("Cut range removed the entire video!");
            }
            else if (part1Exists && !part2Exists)
            {
                File.Move(part1File, concatOutput, overwrite: true);
            }
            else if (!part1Exists && part2Exists)
            {
                File.Move(part2File, concatOutput, overwrite: true);
            }
            else
            {
                // Create a concat list file.
                string concatListPath = Path.Combine(tempDir, "concat_list.txt");
                File.WriteAllLines(concatListPath, new[]
                {
            $"file '{part1File}'",
            $"file '{part2File}'"
        });

                string concatArgs = $"-f concat -safe 0 -i \"{concatListPath}\" -c copy {concatFlags} \"{concatOutput}\"";
                TimeSpan combinedDuration = adjustedCutStart + (totalDuration - adjustedCutEnd);
                await RunFFmpegAsync(concatArgs, progressBar, combinedDuration, cancellationToken);

                // Cleanup intermediate files.
                File.Delete(concatListPath);
                File.Delete(part1File);
                File.Delete(part2File);
            }

            // 8. Remux the concatenated file to rebuild indexes and timestamps.
            // Use a separate temporary file for the remux output.
            string remuxOutput = Path.Combine(tempDir, "remuxed" + fileExt);
            string remuxArgs = $"-i \"{concatOutput}\" -c copy -reset_timestamps 1 {(fileExt == ".mp4" ? "-movflags +faststart" : "")} \"{remuxOutput}\"";
            // Here we assume remuxing takes a very short time; adjust the progress duration if needed.
            await RunFFmpegAsync(remuxArgs, progressBar, TimeSpan.FromSeconds(5), cancellationToken);

            // Replace the concatenated file with the remuxed file.
            File.Delete(concatOutput);
            File.Move(remuxOutput, outputFile, overwrite: true);

            progressBar.Invoke((Action)(() => progressBar.Value = 100));
        }

        public static async Task ExtractVideoRange(
    string inputFile,
    string outputFile,
    TimeSpan extractStart,
    TimeSpan extractEnd,
    ProgressBar progressBar,
    CancellationToken cancellationToken)
        {
            // 1. Get total duration.
            TimeSpan totalDuration = await GetVideoDuration(inputFile);
            if (totalDuration == TimeSpan.Zero)
                throw new Exception("Failed to get video duration.");

            // Clamp and validate times.
            if (extractStart < TimeSpan.Zero)
                extractStart = TimeSpan.Zero;
            if (extractEnd > totalDuration)
                extractEnd = totalDuration;
            if (extractStart >= extractEnd)
                throw new Exception("Invalid extract range.");

            // 2. Adjust the start time to the nearest keyframe (searching backward).
            TimeSpan adjustedStart = await GetNearestKeyframeTime(inputFile, extractStart, searchBackward: true, cancellationToken: cancellationToken);
            TimeSpan durationToExtract = extractEnd - adjustedStart;

            // 3. Build the FFmpeg command.
            // We use -ss before -i for fast keyframe–aligned seeking and -t for duration.
            string args = $"-ss {adjustedStart} -i \"{inputFile}\" -t {durationToExtract} -c copy \"{outputFile}\"";

            // 4. Run the command.
            await RunFFmpegAsync(args, progressBar, durationToExtract, cancellationToken);
            progressBar.Invoke((Action)(() => progressBar.Value = 100));
        }




        public static async Task<TimeSpan> GetNearestKeyframeTime(string inputFile, TimeSpan targetTime, bool searchBackward, CancellationToken cancellationToken = default)
        {
            // Use ffprobe to list keyframe timestamps (in seconds)
            // This command lists only keyframes (-skip_frame nokey)
            string arguments = $"-select_streams v -skip_frame nokey -show_frames -show_entries frame=pkt_pts_time -of csv=p=0 \"{inputFile}\"";
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "ffprobe";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                // Parse each line as a double (seconds)
                List<double> keyframeTimes = new List<double>();
                using (StringReader reader = new StringReader(output))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (double.TryParse(line, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double seconds))
                        {
                            keyframeTimes.Add(seconds);
                        }
                    }
                }

                if (keyframeTimes.Count == 0)
                {
                    // Fallback: if no keyframe info is available, return the target time.
                    return targetTime;
                }

                double targetSeconds = targetTime.TotalSeconds;
                if (searchBackward)
                {
                    // Find the largest keyframe time that is <= targetSeconds.
                    double nearest = keyframeTimes.Where(t => t <= targetSeconds).DefaultIfEmpty(keyframeTimes.First()).Max();
                    return TimeSpan.FromSeconds(nearest);
                }
                else
                {
                    // Find the smallest keyframe time that is >= targetSeconds.
                    double nearest = keyframeTimes.Where(t => t >= targetSeconds).DefaultIfEmpty(keyframeTimes.Last()).Min();
                    return TimeSpan.FromSeconds(nearest);
                }
            }
        }


    }
}
