using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Face_Recognition
{
    public partial class MainWindow : Window
    {
        //initialize kinect
        private KinectSensor sensor;

        //creating bitmap for the frames
        private WriteableBitmap colorBitmap;
        private WriteableBitmap depthBitmap;
        private WriteableBitmap infraBitmap;

        //creating array for every pixel(?)
        private byte[] colorPixels;
        private short[] depthPixels;
        private byte[] infraPixels;

        //variable to store how many clicks i get
        private int flusso = 0;

        public MainWindow()
        {
            InitializeComponent();
            InitializeKinectSensor();
        }

        private void InitializeKinectSensor()
        {
            //chooses the first kinect available
            sensor = KinectSensor.KinectSensors[0];

            if (sensor != null && sensor.Status == KinectStatus.Connected)
            {
                //enables depth and rgb stream
                sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                //associates event to action (every time I get a frame, there is an event)
                sensor.ColorFrameReady += Sensor_ColorFrameReady;
                sensor.DepthFrameReady += Sensor_DepthFrameReady;

                //formatting bitmaps
                colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null); //rgb
                depthBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null); //grays
                infraBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);

                //defining initial dimension of the buffers
                colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                depthPixels = new short[sensor.DepthStream.FramePixelDataLength];
                infraPixels = new byte[sensor.ColorStream.FramePixelDataLength];

                //default sorce
                VideoDisplay.Source = colorBitmap;

                try
                {
                    sensor.Start();
                }
                catch (IOException)
                {
                    sensor = null;
                }
            }
            else
            {
                MessageBox.Show("Nessun sensore Kinect trovato o disponibile.");
            }
        }

        //manages the event: frame available on colorstream
        private void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null && flusso != 2)
                {
                    
                    if (flusso == 0) //0 for rgb, 1 for infrared
                    {                
                        //if the buffer dimension is wrong, corrects it
                        if (colorPixels == null || colorPixels.Length != colorFrame.PixelDataLength)
                        {
                            colorPixels = new byte[colorFrame.PixelDataLength];
                        }

                        //stores data in the buffer
                        colorFrame.CopyPixelDataTo(colorPixels);

                        //composes the bitmap(frame)
                        colorBitmap.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                                                colorPixels, colorFrame.Width * colorFrame.BytesPerPixel, 0);
                    }
                    else if (flusso == 1)
                    {

                        if (infraPixels == null || infraPixels.Length != colorFrame.PixelDataLength)
                        {
                            infraPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                        }

                        colorFrame.CopyPixelDataTo(infraPixels);

                        infraBitmap.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                                                infraPixels, colorFrame.Width * colorFrame.BytesPerPixel, 0);
                    }
                }
            }
        }

        //handles depth display
        private void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null && flusso == 2)
                {
                    if (depthPixels == null || depthPixels.Length != depthFrame.PixelDataLength)
                    {
                        depthPixels = new short[depthFrame.PixelDataLength];
                    }

                    depthFrame.CopyPixelDataTo(depthPixels);

                    depthBitmap.WritePixels(new Int32Rect(0, 0, depthFrame.Width, depthFrame.Height),
                                            depthPixels, depthFrame.Width * depthFrame.BytesPerPixel, 0);
                }
            }
        }

        //handles button clicking
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            //counts number of clicks, at 2 it resets
            if (flusso == 2)
            {
                flusso = 0;
            }
            else
            {
                flusso++;
            }

            //what to do on every click
            switch (flusso)
            {
                //you need to switch streams cause rgb and infra are on the same Stream
                case 0:
                    //resets the stream to rgb
                    sensor.ColorStream.Disable();
                    sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    //displays stream and updates button text
                    VideoDisplay.Source = colorBitmap;
                    StreamTypeText.Text = "Flusso RGB";
                    break;
                case 1:
                    //resets stream to infrared
                    sensor.ColorStream.Disable();
                    sensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);
                    //display stream and updates button text
                    VideoDisplay.Source = infraBitmap;
                    StreamTypeText.Text = "Flusso Infrarossi";
                    break;
                case 2:
                    VideoDisplay.Source = depthBitmap;
                    StreamTypeText.Text = "Flusso Profondità";
                    break;
            }
        }       

        //what happens when I close the window
        private void Window_Closed(object sender, EventArgs e)
        {
            if (sensor != null)
            {
                sensor.Stop();
            }
        }
    }
}
