## Static Audio Webm Creator: Optimized for Nerds
SAWCON is a hybrid GUI/CLI webm tool for the imageboard user of tomorrow. It is a rewrite of the original SAWC project in C#, with the aim of converting it from an afternoon project made on a whim into a fully serviceable, portable executable binary that's usable by an average user without any major setup or hassle.

Put simply, SAWCON combines image and audio data to create a webm video.

### Features:
- Combines audio and image files as specified by the user, to create a webm video file featuring the static image with the audio playing in the background
- Customizable filesize limits and output location
- Quality maximization for both image and audio components, within a specified filesize
- Automatic detection and utilization of embedded image data from audio files, where applicable
- Automatic metadata parsing and inclusion upon webm creation
- Automatic keyframe generation and byte editing for videos longer than 5 minutes, allowing for simple bypassing of standard imageboard duration limitations
- All major features accessible via CLI or drag-and-drop for faster usage
___

### Before we begin:
SAWCON _requires_ FFmpeg to be installed on your system and added to its PATH. If you don't have it, [get it](https://www.ffmpeg.org/download.html).

Additionally,  SAWCON makes use of the .NET Framework v4.6 - by default, all Windows 10 devices should have this installed and you should not have to do any additional work to run the program, but if you're using a different operating system you may need to install this. This program _will not_ do this for you, but you can get it [direct from Microsoft](https://www.microsoft.com/en-us/download/details.aspx?id=48130) if you need it.

___

### Usage:
SAWCON can be used in one of two ways: directly running the .exe, or dragging files onto it (you can optionally run it from a terminal, which will operate functionally identical to the latter option).

#### GUI Usage:
When run directly, you will be presented with the following window:

`(Visual Guide Coming Soon™)`

#### Drag-and-Drop/CLI Usage:

When files are dragged and dropped onto the executable, the program will automatically create a webm using the standard filesize limit of 6MB. Alternatively, these files can be supplied to the program as arguments in a terminal, if you're into that sort of thing. The standard way to do this is to select your desired audio file, your desired artwork file, and drag both onto the .exe file. This will create a webm file combining the two under 6MB, with as high quality for each as will fit.

Worth noting is that the program will check for embedded artwork in audio files - if your chosen audio already contains an embedded album cover, you do not need to supply any other files. However, you can still override this embedded data by supplying an image file.
