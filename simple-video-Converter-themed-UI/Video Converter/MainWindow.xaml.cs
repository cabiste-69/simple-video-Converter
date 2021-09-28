using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Path = System.IO.Path;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Text.RegularExpressions;

namespace Video_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9]+");
        string path,output;
        private int duration;
        string[] knownSupportedFormats = { ".mp4", ".mkv", ".flv", ".avi", ".webm", ".m4v", ".wmv" };
        string ffmpegPath = @"C:\ffmpeg\ffmpeg.exe";

        public MainWindow()
        {
            InitializeComponent();
            
            selectFormat.ItemsSource = knownSupportedFormats;
            selectFormat.SelectedIndex = 0;
            dataUnit.ItemsSource = new string[] { "Kb", "Mb", "Gb" };
            

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
        }

        void PreviewMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            var totalDurationTime = video.NaturalDuration.TimeSpan.TotalMilliseconds;
            
            duration = (int)video.NaturalDuration.TimeSpan.TotalMilliseconds;
        }

        private void VideoPlayClick(object sender, RoutedEventArgs e)
        {
            if (video.Position.TotalMilliseconds == duration)
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
            if (output == "")
            {
                MessageBox.Show("No ouput path selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(Path.GetDirectoryName(output)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(output));
            }
            LaunchCMD(path, output);
            
            Process.Start(@Path.GetDirectoryName(output));
            
            
        }

        private void LaunchCMD(string input, string output)
        {            
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-y -i {input} libx264 -b:v 0.5M -minrate 0.5M -maxrate 0.5M -bufsize 1M{output}",
                WorkingDirectory = Path.GetDirectoryName(ffmpegPath),
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
            }

        }

        private void outputFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void fileSizeYN_Checked(object sender, RoutedEventArgs e)
        {
            throw new NullReferenceException();
        }

        private void fileSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void fileSizeYN_Checked_1(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void outputPathButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                //outputPath.Text = dialog.FileName + @"\" + outputFileName.Text.Trim() + selectFormat.Text;
            }
        }

        //private void SetTime(TimeSpan x)
        //{
        //    endTimeValue.Value = x;
        //    startTimeValue.Value = new TimeSpan(0, 0, 0, 0,1);
        //}
    }
}
