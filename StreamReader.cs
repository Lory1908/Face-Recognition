using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace StreamManager
{
    public class Stream{
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

        public enum StreamType
        {
            rgb,
            infra,
            depth,
            stop
        }

        public Stream()
        {
            InitializeKinectSensor();
        }

        private void InitializeKinectSensor()
        {
            //chooses the first kinect available
            sensor = KinectSensor.KinectSensors[0];

            if (sensor != null && sensor.Status == KinectStatus.Connected)
            {

                //formatting bitmaps
                colorBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null); //rgb
                depthBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null); //grays
                infraBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Gray16, null);

                //defining initial dimension of the buffers
                colorPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                depthPixels = new short[sensor.DepthStream.FramePixelDataLength];
                infraPixels = new byte[sensor.ColorStream.FramePixelDataLength];

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

                if (colorFrame != null) //se avvio il flusso troppo veloce il kinect potrebbe non essere pronto
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
                else MessageBox.Show("Stoppare prima il flusso RGB");
            }
        }

        private void Sensor_InfraFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null) //se avvio il flusso troppo veloce il kinect potrebbe non essere pronto
                {
                    if (infraPixels == null || infraPixels.Length != colorFrame.PixelDataLength)
                    {
                        infraPixels = new byte[sensor.ColorStream.FramePixelDataLength];
                    }

                    colorFrame.CopyPixelDataTo(infraPixels);

                    infraBitmap.WritePixels(new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                                            infraPixels, colorFrame.Width * colorFrame.BytesPerPixel, 0); 
                }
                else MessageBox.Show("Stoppare prima il flusso infrarossi");
            }
        }

        //handles depth display
        private void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null) //se avvio il flusso troppo veloce il kinect potrebbe non essere pronto
                {
                    if (depthPixels == null || depthPixels.Length != depthFrame.PixelDataLength)
                    {
                        depthPixels = new short[depthFrame.PixelDataLength];
                    }

                    depthFrame.CopyPixelDataTo(depthPixels);

                    depthBitmap.WritePixels(new Int32Rect(0, 0, depthFrame.Width, depthFrame.Height),
                                            depthPixels, depthFrame.Width * depthFrame.BytesPerPixel, 0); 
                }
                else MessageBox.Show("Stoppare prima il flusso di profondità");
            }
        }
        
        public WriteableBitmap GetStream(StreamType stream)
        {
            // Disabilita tutti gli eventi precedenti prima di abilitare il nuovo flusso
            sensor.ColorFrameReady -= Sensor_ColorFrameReady;
            sensor.ColorFrameReady -= Sensor_InfraFrameReady;
            sensor.DepthFrameReady -= Sensor_DepthFrameReady;

            // Disabilita tutti i flussi attivi prima di abilitarne uno nuovo
            sensor.ColorStream.Disable();
            sensor.DepthStream.Disable();

            //what to do on every click
            switch (stream)
            {
                //you need to switch streams cause rgb and infra are on the same Stream
                case StreamType.rgb:
                    //resets the stream to rgb
                    sensor.ColorFrameReady += Sensor_ColorFrameReady;
                    sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);               
                    return colorBitmap;

                case StreamType.infra:
                    //resets stream to infrared
                    sensor.ColorFrameReady += Sensor_InfraFrameReady;
                    sensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);
                    return infraBitmap;

                case StreamType.depth:
                    sensor.DepthFrameReady += Sensor_DepthFrameReady;
                    sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    return depthBitmap;

                case StreamType.stop:
                    sensor.ColorStream.Disable();
                    sensor.DepthStream.Disable();
                    return null;

                default:
                    return null;
            }
        }

        public void StopSensor()
        {
            if (sensor != null)
            {
                sensor.Stop();
            }
        }
    }
}
