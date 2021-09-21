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

namespace Video_Converter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path,output;
        private int duration;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void ImportClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "video files (*.mp4;*.flv;*.mkv;*.avi;*.3gp;*.wmv)|*.mp4;*.flv;*.mkv;*.avi;*.3gp;*.wmv";
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                test.Text = path;
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
            SetTime(video.NaturalDuration.TimeSpan);
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
            if(endTimeValue.Value == new TimeSpan(0))
            {
                MessageBox.Show("the end time must be greater than 0", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(startTimeValue.Value > endTimeValue.Value)
            {
                MessageBox.Show("the end time must be greater than the start time FFS", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //Error handeling Done

            //MessageBox.Show($"you're going to convert {Path.GetFileName(path)} {video.NaturalDuration.TimeSpan} \n to \n  ");

            output = "D:\\" + outputFileName.Text + ".mp4";
            LaunchCMD(path, output);
            
            Process.Start(@Path.GetDirectoryName(output));
            
        }

        private void LaunchCMD(string input, string output)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = false;
            processStartInfo.UseShellExecute = false;
            processStartInfo.FileName = "ffmpeg.exe";
            processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            processStartInfo.Arguments = $"-y -i {input} {output}";
            MessageBox.Show($"-y -i {input} {output}");
            using(Process exeProcess = Process.Start(processStartInfo))
            {
                exeProcess.WaitForExit();
            }
        }

        private void openExplorer_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@Path.GetDirectoryName(path));
        }

        private void SetTime(TimeSpan x)
        {
            endTimeValue.Value = x;
            startTimeValue.Value = new TimeSpan(0, 0, 0, 0,1);
        }
    }
}
