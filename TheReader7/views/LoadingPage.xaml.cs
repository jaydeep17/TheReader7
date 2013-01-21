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
            bg.ImageSource = bmp;
            statusText.Text = "loading . . .";

            startOCR();     // Initiates the OCR process
        }

        private void startOCR()
        {
            //TODO: Check if the Image is of the correct size and dimension
            byte[] photoBuffer = imageToByte(bmp);
            OcrService.RecognizeImageAsync(Globals.HawaiiApplicationId, photoBuffer, (output) => { 
                Dispatcher.BeginInvoke(() => onOCRComplete(output));
            });
            
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
                WriteableBitmap btmMap = new WriteableBitmap
                    (img.PixelWidth, img.PixelHeight);

                // write an image into the stream
                Extensions.SaveJpeg(btmMap, ms,
                    img.PixelWidth, img.PixelHeight, 0, 100);

                return ms.ToArray();
            }
        }

        private void splitImages() { }

    }
}