# MPEGUI

MPEGUI is a Windows Forms application built with .NET 8 that provides an easy-to-use graphical interface for video conversion and splitting using **FFmpeg**.

## Features
- Convert videos to multiple formats including **MP4, WebM, MKV, AVI, and MOV**.
- GPU acceleration support for NVIDIA (NVENC), Intel (QSV), and AMD (AMF).
- **CRF-based encoding** for optimal quality and file size.
- **Split videos** into multiple parts without re-encoding.
- **Progress tracking** for conversions and splits.
- **Automatic detection** of system-installed FFmpeg or fallback to an embedded version.

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
1. **Select a video file**
2. **Choose a codec** (e.g., H.264, H.265, VP9, etc.)
3. **Set CRF or Bitrate**
4. **Click Convert**

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

