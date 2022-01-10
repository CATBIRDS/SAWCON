using System.Windows.Forms;
using System;

public class Form1 : Form
{
    // Class variables
    private string imageFilePath = "";
    private string audioFilePath = "";
    private double audioFileSize = 0;
    private string audioTitle = "UNKNOWN";
    private string audioArtist = "UNKNOWN";
    private string audioAlbum = "UNKNOWN";
    private int audioDuration = 0;  
    private string outputDirectory = "";
    private bool readyToCreateWebm = false;

    // Closing method override to ensure cleanup of any stray temp files
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (this.Controls.Find("imageDisplay", true).Length > 0)
        {
            PictureBox imageDisplay = (PictureBox)this.Controls.Find("imageDisplay", true)[0];
            if (imageDisplay.Image != null)
            {
                imageDisplay.Image.Dispose();
            }
            string tempPath = System.IO.Path.GetTempPath();
            string albumArtPath = tempPath + "SAWCON-ExtractedCover.png";
            if (System.IO.File.Exists(albumArtPath))
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.IO.File.Delete(albumArtPath);
            }
        }
        base.OnFormClosing(e);
    }

    public void ErrorLayout()
    {
        // Main Window
        this.Name = "SAWCON";
        this.Text = "SAWCON Setup";
        this.Size = new System.Drawing.Size(280, 170);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        // add 5px padding to top and left
        this.Padding = new Padding(5, 5, 5, 5);

        // Using TableLayoutPanel
        TableLayoutPanel errorGridLayout = new TableLayoutPanel();
        errorGridLayout.Name = "errorGridLayout";
        errorGridLayout.ColumnCount = 2;
        errorGridLayout.RowCount = 2;
        errorGridLayout.Dock = DockStyle.Fill;
        errorGridLayout.AutoSize = true;
        errorGridLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.Controls.Add(errorGridLayout);

        // Warning Label
        Label warningLabel = new Label();
        warningLabel.AutoSize = true;
        warningLabel.Text = "FFmpeg is not part of your system's PATH.\n" +
                            "This is not an optional component.\n" +
                            "If you don't know what this means, FFmpeg \nis probably not installed on your system.\n" +
                            "Please install FFmpeg, or add it to your PATH.\n";
        errorGridLayout.Controls.Add(warningLabel, 0, 0);
        errorGridLayout.SetColumnSpan(warningLabel, 2);

        // Help Button
        Button helpButton = new Button();
        helpButton.Text = "View Documentation and Exit";
        // open url then close form
        helpButton.Click += (sender, e) => {
            System.Diagnostics.Process.Start("explorer", "https://github.com/CATBIRDS/SAWCON");
            this.Close();
        };
        // make button fill entire row
        helpButton.AutoSize = true;
        helpButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        errorGridLayout.Controls.Add(helpButton, 0, 1);

        // Exit Button
        Button exitButton = new Button();
        exitButton.Text = "Exit";
        exitButton.AutoSize = true;
        exitButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        exitButton.Click += (sender, e) => this.Close();
        errorGridLayout.Controls.Add(exitButton, 1, 1);
    }

    public void FormLayout()
    {
        this.BackColor = System.Drawing.SystemColors.Control;

        // Main Window
        this.Name = "SAWCON";
        this.Text = "Static Audio Webm Creator: Optimized for Nerds";
        this.Size = new System.Drawing.Size(450, 330);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;

        // Using TableLayoutPanel
        TableLayoutPanel mainGridLayout = new TableLayoutPanel();
        mainGridLayout.Name = "mainGridLayout";
        mainGridLayout.ColumnCount = 2;
        mainGridLayout.RowCount = 3;
        mainGridLayout.Dock = DockStyle.Fill;
        mainGridLayout.AutoSize = true;
        mainGridLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.Controls.Add(mainGridLayout);

        // Image Display
        PictureBox imageDisplay = new PictureBox();
        imageDisplay.Name = "imageDisplay";
        imageDisplay.SizeMode = PictureBoxSizeMode.Zoom;
        imageDisplay.Size = new System.Drawing.Size(200, 200);
        imageDisplay.BorderStyle = BorderStyle.FixedSingle;
        mainGridLayout.Controls.Add(imageDisplay, 0, 1);

        // Container for audioInfoDisplay, fileSizePreset, inputField, and outputButton
        TableLayoutPanel audioContainer = new TableLayoutPanel();
        audioContainer.Name = "audioContainer";
        audioContainer.ColumnCount = 1;
        audioContainer.RowCount = 4;
        audioContainer.Dock = DockStyle.Fill;
        audioContainer.AutoSize = true;
        audioContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        audioContainer.Padding = new Padding(0);
        mainGridLayout.Controls.Add(audioContainer, 1, 1);

        // Audio Info Display
        TextBox audioInfoDisplay = new TextBox();
        audioInfoDisplay.Name = "audioInfoDisplay";
        audioInfoDisplay.Size = new System.Drawing.Size(215, 100);
        audioInfoDisplay.BorderStyle = BorderStyle.FixedSingle;
        audioInfoDisplay.ReadOnly = true;
        audioInfoDisplay.Multiline = true;
        audioInfoDisplay.Enabled = false;

        // Placeholder rows for audio info
        string audioTitle = "Title: ";
        string audioArtist = "Artist: ";
        string audioAlbum = "Album: ";
        string audioDuration = "Duration: ";
        string audioTrack = "Track Number: ";
        string audioSize = "Size: ";
        // add to first row of audioContainer
        audioContainer.Controls.Add(audioInfoDisplay, 0, 0);
        this.updateTextBox(audioTitle, audioArtist, audioAlbum, audioDuration, audioTrack, audioSize);

        // File Size Preset Checkbox
        CheckBox fileSizePreset = new CheckBox();
        fileSizePreset.Name = "fileSizePreset";
        fileSizePreset.Text = "Use /wsg/ File Limit (6MB)";
        fileSizePreset.AutoSize = true;
        fileSizePreset.Checked = true;
        fileSizePreset.CheckedChanged += new System.EventHandler(this.fileSizePreset_CheckedChanged);
        audioContainer.Controls.Add(fileSizePreset, 0, 1);

        // Input Field
        TextBox inputField = new TextBox();
        // only allow numbers and periods and backspace
        inputField.KeyPress += new KeyPressEventHandler(this.inputField_KeyPress);
        inputField.Name = "inputField";
        inputField.Text = "Enter File Size (MB)";
        inputField.Enabled = false;
        inputField.Size = new System.Drawing.Size(215, 20);
        inputField.GotFocus += new System.EventHandler(this.inputField_GotFocus);
        inputField.LostFocus += new System.EventHandler(this.inputField_LostFocus);
        inputField.BorderStyle = BorderStyle.FixedSingle;
        audioContainer.Controls.Add(inputField, 0, 2);

        // Image Button
        Button imageButton = new Button();
        imageButton.Name = "imageButton";
        imageButton.Text = "Select an Image File";
        mainGridLayout.Controls.Add(imageButton, 0, 0);
        imageButton.AutoSize = true;
        imageButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        imageButton.Dock = DockStyle.Fill;
        imageButton.Click += new EventHandler(imageButton_Click);

        // Audio Button
        Button audioButton = new Button();
        audioButton.Name = "audioButton";
        audioButton.Text = "Select an Audio File";
        mainGridLayout.Controls.Add(audioButton, 1, 0);
        audioButton.AutoSize = true;
        audioButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        audioButton.Dock = DockStyle.Fill;
        audioButton.Click += new EventHandler(audioButton_Click);

        // Output Button
        Button outputButton = new Button();
        outputButton.Name = "outputButton";
        outputButton.Text = "Select Output Directory (Optional)";
        outputButton.Size = new System.Drawing.Size(215, 30);
        audioContainer.Controls.Add(outputButton, 0, 3);
        outputButton.Click += new EventHandler(outputButton_Click);

        // Progress Bar
        ProgressBar progressBar = new ProgressBar();
        progressBar.Name = "progressBar";
        progressBar.Size = new System.Drawing.Size(215, 30);
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        progressBar.Value = 0;
        progressBar.Step = 1;
        progressBar.Style = ProgressBarStyle.Continuous;
        progressBar.Dock = DockStyle.Fill;
        progressBar.Visible = false;
        mainGridLayout.Controls.Add(progressBar, 1, 2);

        // Webm Creation Button
        Button webmButton = new Button();
        webmButton.Name = "webmButton";
        webmButton.Text = "Create Webm";
        webmButton.Enabled = false;
        webmButton.AutoSize = true;
        webmButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        webmButton.Dock = DockStyle.Fill;
        mainGridLayout.Controls.Add(webmButton, 0, 2);
        webmButton.Click += new EventHandler(webmButton_Click);
    }

    public bool ExistsOnPath(string fileName)
    {
        return GetFullPath(fileName) != null;
    }

    public string GetFullPath(string fileName)
    {
        if (System.IO.File.Exists(fileName))
            return System.IO.Path.GetFullPath(fileName);

        var values = Environment.GetEnvironmentVariable("PATH");
        foreach (var path in values.Split(System.IO.Path.PathSeparator))
        {
            var fullPath = System.IO.Path.Combine(path, fileName);
            if (System.IO.File.Exists(fullPath))
                return fullPath;
        }
        return null;
    }

    // imageButton event handler
    private void imageButton_Click(object sender, EventArgs e)
    {
        // Open dialog in current directory to select image file
        OpenFileDialog openImageDialog = new OpenFileDialog();
        openImageDialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
        openImageDialog.Filter = "Image Files|*.BMP;*.JPG;*.JPEG;*.PNG;*.GIF;*.APNG;*.TIFF;*.TIF;*.WEBP;*.JPEG2000;*.TGA"; //|All Files|*.*
        openImageDialog.FilterIndex = 1;
        openImageDialog.RestoreDirectory = true;

        // Show openImageDialog
        if (openImageDialog.ShowDialog() == DialogResult.OK)
        {
            string imageFilePath = openImageDialog.FileName;
            this.updateImage(imageFilePath);
        }
    }

    // audioButton event handler
    private void audioButton_Click(object sender, EventArgs e)
    {
        // Open dialog in current directory to select audio file
        OpenFileDialog openAudioDialog = new OpenFileDialog();
        openAudioDialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory();
        openAudioDialog.Filter = "Audio Files|*.MP3;*.FLAC;*.AAC;*.APE;*.WAV;*.WMA;*.OGG;*.M4A;*.AIFF;*.MP2;*.MP1;*.DSF"; //|All Files|*.*
        openAudioDialog.FilterIndex = 1;
        openAudioDialog.RestoreDirectory = true;

        // Show openAudioDialog
        if (openAudioDialog.ShowDialog() == DialogResult.OK)
        {
            string audioFilePath = openAudioDialog.FileName;
            this.updateAudio(audioFilePath);
        }
    }

    // update imageDisplay
    private void updateImage(string imageFilePath)
    {
        PictureBox imageDisplay = (PictureBox)this.Controls.Find("imageDisplay", true)[0];
        imageDisplay.Image = System.Drawing.Image.FromFile(imageFilePath);
        this.imageFilePath = imageFilePath;
        if (this.audioFilePath != "")
        {
            Button webmButton = (Button)this.Controls.Find("webmButton", true)[0];
            webmButton.Enabled = true;
            this.readyToCreateWebm = true;
        }
    }

    // update audioInfoDisplay
    private void updateAudio(string audioFilePath)
    {
        TextBox audioInfoDisplay = (TextBox)this.Controls.Find("audioInfoDisplay", true)[0];
        audioInfoDisplay.Text = "";
        this.audioFilePath = audioFilePath;
        if (this.imageFilePath != "")
        {
            Button webmButton = (Button)this.Controls.Find("webmButton", true)[0];
            webmButton.Enabled = true;
            this.readyToCreateWebm = true;
        }
        // run ffmpeg from command line to get audio info
        string ffmpegPath = "ffmpeg";
        string ffmpegCommand = "-hide_banner -i " + "\"" + audioFilePath + "\"" + " -f ffmetadata ";
        string ffmpegOutput = "";
        System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
        ffmpeg.StartInfo.CreateNoWindow = true;
        ffmpeg.StartInfo.UseShellExecute = false;
        ffmpeg.StartInfo.RedirectStandardOutput = true;
        // Because there's no output file specified, we redirect and read the ERROR output, not the standard output
        // While technically an error as far as ffmpeg is concerned, this is our intended workflow
        ffmpeg.StartInfo.RedirectStandardError = true;
        ffmpeg.StartInfo.FileName = ffmpegPath;
        ffmpeg.StartInfo.Arguments = ffmpegCommand;
        ffmpeg.Start();
        ffmpegOutput = ffmpeg.StandardError.ReadToEnd();
        ffmpeg.WaitForExit();
        // Set default values
        string audioTitle = "Title: UNKNOWN";
        string audioArtist = "Artist: UNKNOWN";
        string audioAlbum = "Album: UNKNOWN";
        string audioDuration = "Duration: UNKNOWN";
        string audioTrack = "Track Number: UNKNOWN";
        string audioSize = "Size: UNKNOWN";
        // Parse ffmpeg output
        string[] ffmpegOutputLines = ffmpegOutput.Split('\n');
        foreach (string line in ffmpegOutputLines)
        {
            if (line.Contains("TITLE"))
            {
                this.audioTitle = line.Substring(line.IndexOf(":") + 1);
                audioTitle = "Title: " + line.Substring(line.IndexOf(':') + 1).Trim();
            }
            else if (line.Contains("ARTIST"))
            {
                this.audioArtist = line.Substring(line.IndexOf(":") + 1);
                audioArtist = "Artist: " + line.Substring(line.IndexOf(':') + 1).Trim();
            }
            else if (line.Contains("ALBUM"))
            {
                this.audioAlbum = line.Substring(line.IndexOf(":") + 1);
                audioAlbum = "Album: " + line.Substring(line.IndexOf(':') + 1).Trim();
            }
            else if (line.Contains("track"))
            {
                audioTrack = "Track Number: " + line.Substring(line.IndexOf(':') + 1).Trim();
            }
            else if (line.Contains("Duration"))
            {
                string audioDurationLine = line.Substring(line.IndexOf(':') + 1);
                string[] audioDurationSplit = audioDurationLine.Split(':');
                int audioDurationHours = int.Parse(audioDurationSplit[0]);
                int audioDurationMinutes = int.Parse(audioDurationSplit[1]);
                int audioDurationSeconds = int.Parse(audioDurationSplit[2].Substring(0, audioDurationSplit[2].IndexOf('.')));
                audioDuration = "Duration: " + string.Format("{0:00}", audioDurationHours) + ":" + string.Format("{0:00}", audioDurationMinutes) + ":" + string.Format("{0:00}", audioDurationSeconds);
                this.audioDuration = audioDurationHours * 3600 + audioDurationMinutes * 60 + audioDurationSeconds;
            }
        }

        // Attempt to get album art from audio file, if we don't have one already
        if (this.imageFilePath == "")
        {
            string tempPath = System.IO.Path.GetTempPath();
            string albumArtPath = tempPath + "SAWCON-ExtractedCover.png";
            string coverCommand = "-y -hide_banner -loglevel error -i " + "\"" + audioFilePath + "\" " + albumArtPath;
            System.Diagnostics.Process cover = new System.Diagnostics.Process();
            cover.StartInfo.CreateNoWindow = true;
            cover.StartInfo.UseShellExecute = false;
            cover.StartInfo.RedirectStandardOutput = true;
            cover.StartInfo.FileName = ffmpegPath;
            cover.StartInfo.Arguments = coverCommand;
            cover.Start();
            cover.WaitForExit();
            if (System.IO.File.Exists(albumArtPath))
            {
                this.imageFilePath = albumArtPath;
                this.updateImage(albumArtPath);
            }
        }

        // Get file size
        long length = new System.IO.FileInfo(audioFilePath).Length;
        double size = length / (1024.0 * 1024.0);
        this.audioFileSize = size;
        audioSize = "Size: " + size.ToString("0.00") + " MB";
        this.updateTextBox(audioTitle, audioArtist, audioAlbum, audioDuration, audioTrack, audioSize);
    }

    // checkbox event handler
    private void fileSizePreset_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox checkbox = (CheckBox)sender;
        if (checkbox.Checked)
        {
            TextBox inputField = (TextBox)this.Controls.Find("inputField", true)[0];
            inputField.Enabled = false;
        }
        else
        {
            TextBox inputField = (TextBox)this.Controls.Find("inputField", true)[0];
            inputField.Enabled = true;
        }
    }

    // input gotFocus event handler
    private void inputField_GotFocus(object sender, EventArgs e)
    {
        // Clear input text if it's the default value

        TextBox input = (TextBox)sender;
        if (input.Text == "Enter File Size (MB)")
        {
            input.Text = "";
        }
    }

    // input lostFocus event handler
    private void inputField_LostFocus(object sender, EventArgs e)
    {
        // Set default input text if empty
        TextBox input = (TextBox)sender;
        if (input.Text == "")
        {
            input.Text = "Enter File Size (MB)";
        }
    }

    // inputKeyDown KeyPressEventHandler handler

    private void inputField_KeyPress(object sender, KeyPressEventArgs e)
    {
        // only allow numbers, numpad numbers, period, backspace, delete, and enter
        TextBox input = (TextBox)sender;
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
        {
            e.Handled = true;
        }
        // only allow one period
        if (e.KeyChar == '.' && input.Text.IndexOf('.') > -1)
        {
            e.Handled = true;
        }
        // if enter is pressed and readyToCreateWebm is true, run createWebm()
        if (e.KeyChar == 13 && this.readyToCreateWebm)
        {
            this.createWebm();
        }
    }

    // outputDirectory button event handler
    private void outputButton_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        folderBrowser.ShowNewFolderButton = true;
        folderBrowser.Description = "Select Output Directory";
        folderBrowser.SelectedPath = System.IO.Directory.GetCurrentDirectory();
        if (folderBrowser.ShowDialog() == DialogResult.OK)
        {
            Button outputButton = (Button)this.Controls.Find("outputButton", true)[0];
            // set text to final directory in path
            outputButton.Text = "Output: " + new System.IO.DirectoryInfo(folderBrowser.SelectedPath).Name;
            this.outputDirectory = folderBrowser.SelectedPath;
        }
    }

    // webmButton click event handler
    private void webmButton_Click(object sender, EventArgs e)
    {
        this.createWebm();
    }

    // update audioInfoDisplay
    private void updateTextBox(string audioTitle, string audioArtist, string audioAlbum, string audioDuration, string audioTrack, string audioSize)
    {
        TextBox audioInfoDisplay = (TextBox)this.Controls.Find("audioInfoDisplay", true)[0];
        audioInfoDisplay.AppendText(audioTitle);
        audioInfoDisplay.AppendText(Environment.NewLine);
        audioInfoDisplay.AppendText(audioArtist);
        audioInfoDisplay.AppendText(Environment.NewLine);
        audioInfoDisplay.AppendText(audioAlbum);
        audioInfoDisplay.AppendText(Environment.NewLine);
        audioInfoDisplay.AppendText(audioTrack);
        audioInfoDisplay.AppendText(Environment.NewLine);
        audioInfoDisplay.AppendText(audioDuration);
        audioInfoDisplay.AppendText(Environment.NewLine);
        audioInfoDisplay.AppendText(audioSize);
    }

    // createWebm
    private void createWebm()
    {
        // Show progress bar
        ProgressBar progress = (ProgressBar)this.Controls.Find("progressBar", true)[0];
        progress.Visible = true;
        progress.Value = 0;
        // Calculate quality options
        int audioQuality = calculateOggQuality(); // Progress bar value upon completion is between 11 and 20
        string ffmpegPath = "ffmpeg";
        string shill = "Created using SAWCON.exe - https://GitHub.com/CATBIRDS/SAWCON";
        // if audioTitle and audioArtist are not "UNKNOWN", set output file name to audioTitle + " - " + audioArtist
        string outputFileName = audioFilePath.Substring(audioFilePath.LastIndexOf('\\') + 1);
        if (this.audioTitle != "UNKNOWN" && this.audioArtist != "UNKNOWN")
        {
            progress.Value += 5;
            outputFileName = this.audioArtist.Trim() + " - " + this.audioTitle.Trim();
        }
        if (this.outputDirectory == "")
        {
            progress.Value += 5;
            this.outputDirectory = System.IO.Directory.GetCurrentDirectory();
        }
        string outputPath = this.outputDirectory + "\\" + outputFileName + ".webm";
        // "ffmpeg", "-framerate", "1", "-y", "-i", f"{os.path.split(image_file)[1]}", "-i", f"{os.path.split(audio_file)[1]}", "-c:v", "libvpx", "-b:v", "2M", "-c:a", "libvorbis", "-q:a", f"{quality.index(desired_quality)}", "-g", "10000", "-force_key_frames", "0", "-metadata", f"title={metadata}", "-metadata", f"comment={shill}",  "-vf", f"scale={dimensions[0]}:{dimensions[1]}", f"{os.path.split(audio_file)[1]}.webm"])
        string ffmpegCommand = "-framerate 1 -y" +
                                " -i \"" + this.imageFilePath + "\"" +
                                " -i \"" + this.audioFilePath + "\"" +
                                " -c:v libvpx -b:v 2M" +
                                " -c:a libvorbis -q:a \"" + audioQuality.ToString() + "\"" +
                                " -g 10000 -force_key_frames 0" +
                                " -metadata title=\"" + this.audioTitle.Trim() + "\"" +
                                " -metadata subtitle=\"" + this.audioArtist.Trim() + " - " + this.audioAlbum.Trim() + "\"" +
                                " -metadata comment=\"" + shill + "\"" +
                                " \"" + outputPath + "\"";
        System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
        ffmpeg.StartInfo.CreateNoWindow = true;
        ffmpeg.StartInfo.UseShellExecute = false;
        ffmpeg.StartInfo.FileName = ffmpegPath;
        ffmpeg.StartInfo.Arguments = ffmpegCommand;
        progress.Value = 50;
        ffmpeg.Start();
        // each time ffmpeg writes to the console, update progress bar
        ffmpeg.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                // update progress bar as long as the value is less than 90
                if (progress.Value < 90)
                {
                    progress.Value += 1;
                }
            }
        };
        ffmpeg.WaitForExit();
        // if duration is longer than 5 minutes, run postProcess()
        if (this.audioDuration > 300)
        {
            // if progress bar is below 90, set to 90
            if (progress.Value < 90)
            {
                progress.Value = 90;
            }
            this.postProcess(outputPath);
        }
        progress.Value = 100;
    }

    // byte editing function
    private void postProcess(string filename)
    {
        // find offset of hex 0x448988
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

    // calculate ogg quality parameter
    private int calculateOggQuality()
    {
        // Progress bar
        ProgressBar progress = (ProgressBar)this.Controls.Find("progressBar", true)[0];
        // Array of quality options
        int[] qualityOptions = { 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 500 };
        TextBox inputField = (TextBox)this.Controls.Find("inputField", true)[0];
        // get file size of image file
        long imageFileSize = new System.IO.FileInfo(this.imageFilePath).Length;
        double desiredFileSize = 6000000 - imageFileSize;
        if (inputField.Text != "Enter File Size (MB)")
        {
            progress.Value = 5;
            desiredFileSize = (double.Parse(inputField.Text) * 1000000) - imageFileSize;
        }
        double audioDuration = this.audioDuration;
        // bitrate is in kb/s
        double bitrate = (desiredFileSize * 8 / 1000) / audioDuration;
        progress.Value = 10;
        for (int i = 0; i < qualityOptions.Length; i++)
        {
            progress.Value += i;
            if (qualityOptions[i] >= bitrate)
            {
                if (i == 0)
                {
                    return i;
                }
                else
                {
                    return i - 1;
                }
            }
        }
        return 10;
    }

}