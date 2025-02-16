# MPEGUI

MPEGUI is a Windows Forms application built with .NET 8 that provides an easy-to-use graphical interface for video conversion and splitting using **FFmpeg**.

## Features
- **Video Conversion:**  
  Convert videos to multiple formats including **MP4, WebM, MKV, AVI, and MOV** with support for CRF-based encoding for optimal quality and file size.
  
- **GPU Acceleration:**  
  Leverage hardware acceleration for video encoding with support for NVIDIA (NVENC), Intel (QSV), and AMD (AMF).

- **Video Splitting:**  
  Split videos into multiple parts without re-encoding for fast processing.

- **Cut Video Range:**  
  Remove an unwanted segment from a video. The application aligns cuts to keyframes and applies minimal re-encoding at the boundaries (if needed) to ensure smooth playback with minimal quality loss.

- **Extract Video Range:**  
  Select a range from a video (using an intuitive dual-thumb range trackbar) and extract that portion into a new file using stream copy (no re-encoding), for fast and lossless output.

- **Dual-Thumb Range Trackbar:**  
  Easily select time ranges with a custom range trackbar integrated into the UI.

- **Progress Tracking:**  
  Real-time progress updates for conversions, splits, cuts, and extractions, including integration with the Windows taskbar.

- **Advanced Timestamp Handling:**  
  Smart use of FFmpeg flags and keyframe alignment minimizes playback freezes and ensures smooth transitions between segments.

## Requirements
- **.NET 8 SDK** ([Download Here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- **FFmpeg** ([Download Here](https://ffmpeg.org/download.html))

## Installation
1. **Clone this repository**
   ```sh
   git clone https://github.com/mauroGuterres/MPEGUI.git
   cd MPEGUI
   ```
2. **Download and place FFmpeg**
   - Visit [FFmpeg Download Page](https://ffmpeg.org/download.html)
   - Download **FFmpeg static build** for Windows.
   - Extract it and place `ffmpeg.exe` inside `MPEGUI/ffmpeg/`
   
   **Folder structure should look like this:**
   ```
   MPEGUI/
   ├── bin/
   ├── ffmpeg/
   │   ├── ffmpeg.exe
   ├── obj/
   ├── MPEGUI.sln
   ├── MPEGUI.csproj
   ├── Program.cs
   ├── FFmpegHelper.cs
   ├── Form1.cs
   ├── FormSplitVideo.cs
   ```
3. **Build and run**
   ```sh
   dotnet build
   dotnet run
   ```

## Usage

### Convert Video
1. **Select a video file.**
2. **Choose a codec** (e.g., H.264, H.265, VP9, etc.).
3. **Set the CRF value.**
4. **Click Convert.**

### Split Video
1. **Select a video file.**
2. **Enter the number of parts.**
3. **Click Split.**

### Cut Video Range
1. **Select a video file.**
2. **Enter the time range to remove.**
3. **Click Cut Video Range.**  
   The application removes the specified segment and concatenates the remaining parts using keyframe alignment and minimal re-encoding at the boundaries to ensure smooth playback.

### Extract Video Range
1. **Select a video file.**
2. **Use the dual-thumb range trackbar to select the desired time range.**  
   The trackbar automatically updates the start and end times displayed in the text boxes.
3. **Click Extract Range.**  
   The output video will contain only the selected portion, extracted using stream copy for fast and lossless processing.


### Split Video
1. **Select a video file**
2. **Enter number of parts**
3. **Click Split**

## Contribution
Feel free to open an **issue** or submit a **pull request** if you find bugs or have feature requests.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits
- **FFmpeg** - [https://ffmpeg.org](https://ffmpeg.org)
- **.NET 8** - [https://dotnet.microsoft.com](https://dotnet.microsoft.com)

