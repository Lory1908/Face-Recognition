using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using StreamManager;

namespace Display
{
    public partial class MainWindow : Window
    {
        private readonly StreamManager.Stream flusso;

        public MainWindow()
        {
            InitializeComponent();
            //StreamTypeText.Text = "Premere un pulsante";
            flusso = new StreamManager.Stream();
        }

        //handles button clicking
        private void RGBButton_Click(object sender, RoutedEventArgs e)
        {
            VideoDisplay.Source = flusso.GetStream(StreamManager.Stream.StreamType.rgb);
            StreamTypeText.Text = "Flusso RGB";
        }

        private void InfraButton_Click(object sender, RoutedEventArgs e)
        {
            VideoDisplay.Source = flusso.GetStream(StreamManager.Stream.StreamType.infra);
            StreamTypeText.Text = "Flusso Infra";
        }

        private void DepthButton_Click(object sender, RoutedEventArgs e)
        {
            VideoDisplay.Source = flusso.GetStream(StreamManager.Stream.StreamType.depth);
            StreamTypeText.Text = "Flusso Depth";
        }

        //what happens when I close the window
        private void Window_Closed(object sender, EventArgs e)
        {
            flusso.StopSensor();
        }
    }
}
