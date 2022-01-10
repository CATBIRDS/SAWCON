using System;
using System.Windows.Forms;

// SAWCON DEEZ NUTS, GOTEEM

public class Program
{
    public static Form1 form = new Form1();


    [STAThread]
    static void Main(string[] args)
    {
        string[] audioExtensions = { ".MP3", ".FLAC", ".AAC", ".APE", ".WAV", ".WMA", ".OGG", ".M4A", ".AIFF", ".MP2", ".MP1", ".DSF" };
        string[] imageExtensions = { ".BMP", ".JPG", ".JPEG", ".PNG", ".GIF", ".APNG", ".TIFF", ".TIF", ".WEBP", ".JPEG2000", ".TGA" };
        string audioFile = "";
        string imageFile = "";
        double fileSize = 6000000;
        string outputDirectory = System.IO.Directory.GetCurrentDirectory();
        // If a user opens the program by dragging files onto it, run in auto-mode
        if (args.Length > 0)
        {

            string tempPath = System.IO.Path.GetTempPath();
            string albumArtPath = tempPath + "SAWCON-ExtractedCover.png";
            if (System.IO.File.Exists(albumArtPath))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(albumArtPath);
            }
            foreach (string file in args)
            {
                string extension = System.IO.Path.GetExtension(file);
                extension = extension.ToUpper();
                if (audioFile == "" && Array.Exists(audioExtensions, element => element == extension))
                {
                    
                    audioFile = file;
                    if (imageFile == "")
                    {
                        string coverCommand = "-y -hide_banner -loglevel error -i " + "\"" + audioFile + "\" " + albumArtPath;
                        System.Diagnostics.Process cover = new System.Diagnostics.Process();
                        cover.StartInfo.UseShellExecute = false;
                        cover.StartInfo.RedirectStandardOutput = true;
                        cover.StartInfo.FileName = "ffmpeg";
                        cover.StartInfo.Arguments = coverCommand;
                        cover.Start();
                        cover.WaitForExit();
                        if (System.IO.File.Exists(albumArtPath))
                        {
                            imageFile = albumArtPath;
                            break;
                        }
                    }
                }
                else if (imageFile == "" && Array.Exists(imageExtensions, element => element == extension))
                {
                    Console.WriteLine(file);
                    imageFile = file;
                }
                // These don't actually function properly, but as-is they should be harmless, so they're left in for now
                else if (System.IO.Directory.Exists(file))
                {
                    outputDirectory = file;
                }
                else if (double.TryParse(file, out double temp))
                {
                    fileSize = temp * 1000000;
                }
            }
            if (audioFile == "" || imageFile == "")
            {
                return;
            }
            else
            {
                // We're gonna cram the entire functionality of the program's default parameters into this single else statement. Sue me.
                // Get Metadata
                string ffmpegPath = "ffmpeg";
                string metadataCommand = "-hide_banner -i " + "\"" + audioFile + "\"" + " -f ffmetadata ";
                string ffmpegOutput = "";
                string audioTitle = "";
                string audioArtist = "";
                string audioAlbum = "";
                double audioDuration = 0;
                System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
                ffmpeg.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.RedirectStandardError = true;
                ffmpeg.StartInfo.FileName = ffmpegPath;
                ffmpeg.StartInfo.Arguments = metadataCommand;
                ffmpeg.Start();
                ffmpegOutput = ffmpeg.StandardError.ReadToEnd();
                ffmpeg.WaitForExit();
                string[] ffmpegOutputLines = ffmpegOutput.Split('\n');
                foreach (string line in ffmpegOutputLines)
                {
                    if (audioTitle == "" && (line.Contains("TITLE") || line.Contains("title")))
                    {
                        audioTitle = line.Substring(line.IndexOf(":") + 1);
                    }
                    else if (audioArtist == "" && (line.Contains("ARTIST") || line.Contains("artist")))
                    {
                        audioArtist = line.Substring(line.IndexOf(":") + 1);
                    }
                    else if (audioAlbum == "" && (line.Contains("ALBUM") || line.Contains("album")))
                    {
                        audioAlbum = line.Substring(line.IndexOf(":") + 1);
                    }
                    else if (audioDuration == 0 && line.Contains("Duration"))
                    {
                        string audioDurationLine = line.Substring(line.IndexOf(':') + 1);
                        string[] audioDurationSplit = audioDurationLine.Split(':');
                        int audioDurationHours = int.Parse(audioDurationSplit[0]);
                        int audioDurationMinutes = int.Parse(audioDurationSplit[1]);
                        int audioDurationSeconds = int.Parse(audioDurationSplit[2].Substring(0, audioDurationSplit[2].IndexOf('.')));
                        audioDuration = audioDurationHours * 3600 + audioDurationMinutes * 60 + audioDurationSeconds;
                    }
                }
                // Determine Quality
                double bitrate = (fileSize * 8 / 1000) / audioDuration;
                int[] qualityOptions = { 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 500 };
                int quality = 10;
                for (int i = 0; i < qualityOptions.Length; i++)
                {
                    if (qualityOptions[i] >= bitrate)
                    {
                        if (i == 0)
                        {
                            quality = i;
                            break;
                        }
                        else
                        {
                            quality = i - 1;
                            break;
                        }
                    }
                }
                // Create Webm
                string shill = "Created using SAWCON.exe - https://GitHub.com/CATBIRDS/SAWCON";
                
                string filename = audioFile.Substring(audioFile.LastIndexOf('\\') + 1);
                string outputPath = "\"" + filename + ".webm\"";
                if (audioTitle != "")
                {
                    outputPath = outputDirectory + "\\" + audioTitle.Trim() + ".webm";
                }
                string webmCommand = "-framerate 1 -y" +
                                " -i \"" + imageFile + "\"" +
                                " -i \"" + audioFile + "\"" +
                                " -c:v libvpx -b:v 2M" +
                                " -c:a libvorbis -q:a \"" + quality.ToString() + "\"" +
                                " -g 10000 -force_key_frames 0" +
                                " -metadata title=\"" + audioTitle.Trim() + "\"" +
                                " -metadata subtitle=\"" + audioArtist.Trim() + " - " + audioAlbum.Trim() + "\"" +
                                " -metadata comment=\"" + shill + "\"" +
                                " \"" + outputPath + "\"";
                System.Diagnostics.Process webm = new System.Diagnostics.Process();
                webm.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                webm.StartInfo.RedirectStandardOutput = true;
                webm.StartInfo.UseShellExecute = false;
                webm.StartInfo.FileName = ffmpegPath;
                webm.StartInfo.Arguments = webmCommand;
                webm.Start();
                webm.WaitForExit();
                // Post-Processing as needed
                if (audioDuration > 300)
                {
                    long offset = 0;
                    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read)))
                    {
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            if (reader.ReadByte() == 0x44 && reader.ReadByte() == 0x89 && reader.ReadByte() == 0x88)
                            {
                                offset = reader.BaseStream.Position;
                                break;
                            }
                        }
                    }
                    // write 0x41124F80 after offset
                    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite)))
                    {
                        if (offset != 0)
                        {
                            byte[] bytehack = new byte[] { 0x41, 0x12, 0x4F, 0x80 }; // 0x41124F80
                            writer.Seek((int)offset, System.IO.SeekOrigin.Begin);
                            writer.Write(bytehack);
                        }
                    }
                }
                if (System.IO.File.Exists(albumArtPath))
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.IO.File.Delete(albumArtPath);
                }
            }
        }
        else
        {
            if (form.ExistsOnPath("ffmpeg.exe"))
            {
                form.FormLayout();
            }
            else
            {
                form.ErrorLayout();
            }
            Application.Run(form);
        }
    }
}