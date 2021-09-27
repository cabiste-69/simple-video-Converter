using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Video_Converter
{
    class CheckForFFmpegOrDownload
    {
        public void FFmpegIsHere()
        {
           
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg")))
            {
                var result = MessageBox.Show("do you want to install FFmpeg?", "FFmpeg doesn't exist", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg"));
                }
                else
                {
                    MessageBox.Show("whyyyy");
                }
            }
        }
    }
}
