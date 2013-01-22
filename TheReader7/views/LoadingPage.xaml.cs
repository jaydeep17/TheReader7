using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Hawaii;
using Microsoft.Hawaii.Ocr.Client;
using System.Text;


namespace TheReader7.views
{
    public partial class LoadingPage : PhoneApplicationPage
    {
        private BitmapImage bmp;
        public LoadingPage()
        {
            InitializeComponent();
            bmp = new BitmapImage();
            bmp = (BitmapImage) PhoneApplicationService.Current.State["image"];
            //bmp = new BitmapImage(new Uri("/images/helloworld.jpg", UriKind.Relative));
            bg.ImageSource = bmp;
            statusText.Text = "loading . . .";
            
        }

        private void startOCR()
        {
            if (bmp.PixelHeight > 640 || bmp.PixelWidth > 640)
                resizeImage();
            //TODO: Check if the Image is of the correct size and dimension
            byte[] photoBuffer = imageToByte(bmp);
            OcrService.RecognizeImageAsync(Globals.HawaiiApplicationId, photoBuffer, (output) => { 
                Dispatcher.BeginInvoke(() => onOCRComplete(output));
            });
            
        }

        private static byte[] StreamToByteArray(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];

            long seekPosition = stream.Seek(0, SeekOrigin.Begin);
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            seekPosition = stream.Seek(0, SeekOrigin.Begin);

            return buffer;
        }

        private void onOCRComplete(OcrServiceResult result)
        {
            if (result.Status == Status.Success)
            {
                int wordCount = 0;
                StringBuilder sb = new StringBuilder();
                foreach (OcrText item in result.OcrResult.OcrTexts)
                {
                    wordCount += item.Words.Count;
                    sb.AppendLine(item.Text);
                }
                MessageBox.Show(sb.ToString());
                PhoneApplicationService.Current.State["text"] = sb.ToString();
                NavigationService.Navigate(new Uri("/views/OutputPage.xaml", UriKind.Relative));
                // TODO: fix navigation
            }
            else
            {
                statusText.Text = "[OCR conversion failed]\n" + result.Exception.Message;
            }
        }

        private byte[] imageToByte(BitmapImage img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriteableBitmap btmap = new WriteableBitmap(img);
                btmap.SaveJpeg(ms, btmap.PixelWidth, btmap.PixelHeight, 0, 100);
                byte[] buffer = new byte[ms.Length];

                long seekPosition = ms.Seek(0, SeekOrigin.Begin);
                int bytesRead = ms.Read(buffer, 0, buffer.Length);
                seekPosition = ms.Seek(0, SeekOrigin.Begin);

                return buffer;
            }
        }

        private void resizeImage() 
        {
            WriteableBitmap wb = new WriteableBitmap(bmp);
            // TODO: memory management 
            // we have 2 options
            // i) use "using" statement
            // ii) dispose of object "ms" before the method finishes (**check bmp as ms is set as it's source )
            MemoryStream ms = new MemoryStream();
            int h, w;
            if (wb.PixelWidth > wb.PixelHeight)
            {
                double aspRatio = wb.PixelWidth /(double) wb.PixelHeight;
                double hh, ww;
                hh = (640.0 / aspRatio);
                ww = hh * aspRatio;
                h = (int)hh;
                w = (int)ww;
            }
            else
            {
                double aspRatio = wb.PixelHeight /(double) wb.PixelWidth;
                double hh, ww;
                hh = (480.0 / aspRatio);
                ww = hh * aspRatio;
                h = (int)hh;
                w = (int)ww;
            }
            wb.SaveJpeg(ms, w, h, 0, 100);
            bmp.SetSource(ms);
        }

        private void splitImages() { }

        private void pageLoaded(object sender, RoutedEventArgs e)
        {
            startOCR();     // Initiates the OCR process
        }


    }
}