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
using System.Windows.Shapes;


namespace HappyFeet
{
    /// <summary>
    /// Interaction logic for Video.xaml
    /// </summary>
    public partial class Video : Window
    {
        MediaElement VideoControl;
        public Video()
        {
            InitializeComponent();
        }

        public void playbackInit(string fileNumber)
        {
            VideoControl = new MediaElement();
            VideoControl.LoadedBehavior = MediaState.Manual;
            VideoControl.UnloadedBehavior = MediaState.Manual;
            if (!MainWindow.isInHomeSpace)
                VideoControl.Source = new Uri("C:/Users/testUser/Downloads/" + fileNumber + ".mp4");
            else
                VideoControl.Source = new Uri("C:/Users/Interactions Lab/Documents/Visual Studio 2013/Projects/TrackAndPlayack/HappyFeet/" + fileNumber + ".mp4");
            
            videoContainer.Children.Add(VideoControl);            
        }

        public void start()
        {
            VideoControl.Play();
        }

        public void stopPlayback()
        {
            //VideoControl.Stop();
            VideoControl.Close();
            VideoControl.UpdateDefaultStyle();
            videoContainer.Children.Remove(VideoControl);
            VideoControl = null;
        }

    }
}
