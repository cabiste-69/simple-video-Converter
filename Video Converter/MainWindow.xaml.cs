using DataSizeUnits;
using MediaInfo;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xabe.FFmpeg;
using Path = System.IO.Path;


namespace Video_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9]+");
        string path, fullOutput;
        private int duration;
        long vBitrate, aBitrate;
        bool useMT;
        ConversionPreset[] preset = { ConversionPreset.UltraFast, ConversionPreset.SuperFast, ConversionPreset.VeryFast, ConversionPreset.Faster, ConversionPreset.Fast, ConversionPreset.Medium, ConversionPreset.Slow, ConversionPreset.Slower, ConversionPreset.VerySlow };
        string[] knownSupportedFormats = { ".mp4", ".mkv", ".flv", ".avi", ".webm", ".m4v", ".wmv" };
        //public string ffmpegPath = @"C:\ffmpeg\ffmpeg.exe";


        public MainWindow()
        {
            InitializeComponent();

            CheckForFFmpegOrDownload checkForFFmpeg = new CheckForFFmpegOrDownload();
            checkForFFmpeg.FFmpegIsHere();


            selectFormat.ItemsSource = knownSupportedFormats;
            selectFormat.SelectedIndex = 0;
            dataUnit.ItemsSource = new string[] { "Kb", "Mb", "Gb" };
            //selectAudioBitrate.ItemsSource = new string[] { "64", "96", "128", "160", "192", "224", "256", "288", "320" };
            //selectAudioBitrate.SelectedIndex = 8;
            dataUnit.SelectedIndex = 1;
            //selectPreset.ItemsSource = preset;
            //selectPreset.SelectedIndex = 0;
            
            FFmpeg.SetExecutablesPath(@"C:\Users\comp\AppData\Local\FFmpeg", "ffmpeg", "ffprobe");
            
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "video files (*.mp4;*.flv;*.mkv;*.avi;*.3gp;*.wmv;*.webm)|*.mp4;*.flv;*.mkv;*.avi;*.3gp;*.wmv;*.webm";
            DataSize sizeInMegabytes = new DataSize();
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;

                video.Source = new Uri(path);
                video.Position = TimeSpan.FromSeconds(0);
                video.MediaOpened += PreviewMedia_MediaOpened;
                sizeInMegabytes = new DataSize(new FileInfo(path).Length, Unit.Byte).ConvertToUnit(Unit.Megabyte);
                inputFileName.Text = path;
            }
            fileSize.Text = Convert.ToString(Math.Round(sizeInMegabytes.Quantity, 2));
        }

        void PreviewMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            var totalDurationTime = video.NaturalDuration.TimeSpan.TotalMilliseconds;
            duration = (int)video.NaturalDuration.TimeSpan.TotalSeconds;

        }

        private void VideoPlay_Click(object sender, RoutedEventArgs e)
        {
            if (video.Position.TotalSeconds == duration)
            {
                video.Position = new TimeSpan(0);
            }
            video.Play();
        }

        private void VideoPause_Click(object sender, RoutedEventArgs e)
        {
            video.Pause();
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            //Error handeling 
            if (!video.HasVideo)
            {
                MessageBox.Show("you need to import a video before exporting!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            fullOutput = @outputPath.Text;
            if (fullOutput == "")
            {
                MessageBox.Show("No ouput path selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(fullOutput)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullOutput));
            }

            
            //SetVariables(selectAudioBitrate.SelectedItem.ToString(), multiT);
            CalculateBitRate();
            await RunConversion(path, fullOutput, vBitrate, aBitrate, useMT, preset[6]);
            //MessageBox.Show(vBitrate.ToString());


        }

        private async Task RunConversion(string input, string output, long vBitrate, long aBitrate, bool useMT, ConversionPreset preset)
        {
            var mediaInfo = await FFmpeg.GetMediaInfo(input);
            var videoStream = mediaInfo.VideoStreams.First();
            var audioStream = mediaInfo.AudioStreams.First();

            var conversion = FFmpeg.Conversions.New()
            //SetOverwriteOutput to overwrite files. It's useful when we already run application before [bool]
            .SetOverwriteOutput(true)
            //Add video stream to output file [IVideoStream]
            .AddStream(videoStream)
            //Add audio stream to output file [IAudioStream]
            .AddStream(audioStream)
            //Set conversion preset. You have to chose between file size and quality of video and duration of conversion [ConversionPreset]
            .SetPreset(preset)
            //sets the video's bitrate [long]
            .SetVideoBitrate(vBitrate)
            //sets the audio's bitrate [long]
            .SetAudioBitrate(aBitrate * 1000)
            //set the conversion to use multithreading or not [bool]
            .UseMultiThread(useMT)
            //Set output file path [string]
            .SetOutput(output);
            await conversion.Start();
            MessageBox.Show(conversion.Build());
        }


        private void CalculateBitRate()
        {
            double x = Convert.ToDouble(fileSize.Text);
            DataSize sizeInKilobytes = new DataSize(x, Unit.Megabyte).ConvertToUnit(Unit.Byte);
            vBitrate = Convert.ToInt64(sizeInKilobytes.Quantity / duration);
           // MessageBox.Show($"Wanted size: {x} \n Size in kilobytes {sizeInKilobytes.Quantity} \n video bitrate no audio: {sizeInKilobytes.Quantity / duration} \n video bitrate with audio {vBitrate}");
        }


        private void SetVariables(string audioBitrate, CheckBox multiT)
        {
            aBitrate = Convert.ToInt64(audioBitrate);
            useMT = multiT.IsChecked.Value;
            
        }


        private void fileSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }



        private void EnableForcedFileSize(object sender, RoutedEventArgs e)
        {
            if (fileSizeYN.IsChecked == true)
            {
                fileSize.IsEnabled = true;
            }
            else
            {
                fileSize.IsEnabled = false;
            }
        }



        private void outputPathButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "video files (*.mp4;*.flv;*.mkv;*.avi;*.3gp;*.wmv;*.webm)|*.mp4;*.flv;*.mkv;*.avi;*.3gp;*.wmv;*.webm";

            if (dialog.ShowDialog() == true)
            {
                outputPath.Text = dialog.FileName + selectFormat.Text;

            }
        }
    }

}
