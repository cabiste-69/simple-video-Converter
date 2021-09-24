using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;
using System.Threading.Tasks;
using Path = System.IO.Path;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;
using DataSizeUnits;
using NReco.VideoInfo;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;


namespace Video_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9]+");
        string path,fullOutput, outputWOEx;
        private int duration;
        
        string[] knownSupportedFormats = { ".mp4", ".mkv", ".flv", ".avi", ".webm", ".m4v", ".wmv" };
        public string ffmpegPath = @"C:\ffmpeg\ffmpeg.exe";


        public MainWindow()
        {
            InitializeComponent();
            
            selectFormat.ItemsSource = knownSupportedFormats;
            selectFormat.SelectedIndex = 0;
            dataUnit.ItemsSource = new string[] { "Kb", "Mb", "Gb" };
            CheckForFFmpegOrDownload checkForFFmpeg = new CheckForFFmpegOrDownload();

            checkForFFmpeg.FFmpegIsHere();
            outputPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg");

        }

        private void ImportClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "video files (*.mp4;*.flv;*.mkv;*.avi;*.3gp;*.wmv)|*.mp4;*.flv;*.mkv;*.avi;*.3gp;*.wmv";
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                
                video.Source = new Uri(path);
                video.Position = TimeSpan.FromSeconds(0);
                video.MediaOpened += PreviewMedia_MediaOpened;
                video.Play();
                video.Pause();
                
            }
            double aaa = new FileInfo(path).Length;

            
            
            
            //converts it to kilobit because apparently that's what windows uses
            DataSize sizeInMegabytes = new DataSize(aaa, Unit.Byte).ConvertToUnit(Unit.Megabyte);
            fileSize.Text = Convert.ToString(Math.Round(sizeInMegabytes.Quantity,2));

        }

        void PreviewMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            var totalDurationTime = video.NaturalDuration.TimeSpan.TotalMilliseconds;
            duration = (int)video.NaturalDuration.TimeSpan.TotalSeconds;
            
        }

        private void VideoPlayClick(object sender, RoutedEventArgs e)
        {
            if (video.Position.TotalSeconds == duration)
            {
                video.Position = new TimeSpan(0);
            }
            video.Play();
        }

        private void VideoPauseClick(object sender, RoutedEventArgs e)
        {
            video.Pause();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
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
            int s = Convert.ToInt32(CalculateBitRate());

            //MessageBox.Show($"path: {path} \n output: {output} \n bitrate: {s}");
            LaunchCMD(path, outputWOEx, s);
            
            Process.Start(@Path.GetDirectoryName(fullOutput));
            
            
        }

        private void LaunchCMD(string input, string output, int bitrate)
        {


            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                //Arguments = $"-y -i {input} -c:v libx264 -b:v {bitrate.ToString()}M -minrate {bitrate.ToString()}M -maxrate {bitrate.ToString()}M -bufsize 1M {output}",
                //Arguments = $"-y -i {input} -c:v libx264 {output}{selectFormat.Text} -i {input} -c:v libx264 {output}1{selectFormat.Text}",
                Arguments = $"-i {input} -c:v libx264 -b:v {bitrate}K -pass 1 -vsync cfr -f null /dev/null -i {input} -c:v libx264 -b:v {bitrate}K -pass 2 -c:a aac -b:a 128k {output}{selectFormat.Text}",

                //WorkingDirectory = Path.GetDirectoryName(ffmpegPath),
                //CreateNoWindow = true,
                UseShellExecute = false
            };
            
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
                
            }
            outputPath.Text = startInfo.Arguments;

        }

        private double CalculateBitRate()
        {
            double x = Convert.ToDouble(fileSize.Text);
            DataSize sizeInMegabytes = new DataSize(x, Unit.Megabyte).ConvertToUnit(Unit.Kilobyte);
            return sizeInMegabytes.Quantity / duration;
        }

        private void outputFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(outputFileName.Text.Trim() != "")
            {
                outputPath.IsEnabled = true;
                outputPathButton.IsEnabled = true;
            }else
            {
                outputPath.IsEnabled = false;
                outputPath.Text = "";
                outputPathButton.IsEnabled = false;
            }
        }

        private void fileSizeY_Checked(object sender, RoutedEventArgs e)
        {
            
                fileSize.IsEnabled = true;
                dataUnit.IsEnabled = true;
                dataUnit.SelectedIndex = 1;
            
        }
        private void fileSizeY_Unchecked(object sender, RoutedEventArgs e)
        {
            fileSize.IsEnabled = false;
            fileSize.Text = "";
            dataUnit.IsEnabled = false;
            dataUnit.SelectedIndex = -1;

        }

        private void fileSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
                
        private void outputPathButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                outputPath.Text = dialog.FileName + @"\" + outputFileName.Text.Trim() + selectFormat.Text;
                outputWOEx = dialog.FileName + @"\" + outputFileName.Text.Trim();
            }
        }

        //private void SetTime(TimeSpan x)
        //{
        //    endTimeValue.Value = x;
        //    startTimeValue.Value = new TimeSpan(0, 0, 0, 0,1);
        //}
    }
}
