using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;
using System.Threading;

namespace TheReader7.views
{
    public partial class camera : PhoneApplicationPage
    {
        private PhotoCamera cam;
        private MediaLibrary mediaLib;
        private Thread imgProc;
        private bool process;

        public camera()
        {
            InitializeComponent();
            mediaLib = new MediaLibrary();
        }

        private void captureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            string filename = "" + ".jpg";
            Dispatcher.BeginInvoke(delegate()
            {
                txtmsg.Text = "Image available";
            });
            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(e.ImageStream);
            PhoneApplicationService.Current.State["image"] = bmp;
            NavigationService.Navigate(new Uri("/views/LoadingPage.xaml", UriKind.Relative));
            // TODO: fix onNavigatedTO and onNavigationFrom problems
        }

        private void captureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            
            Dispatcher.BeginInvoke(delegate()
            {
                txtmsg.Text = "Image captured";
            });
        }

        private void cameraCanvasTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (cam != null)
            {
                try
                {
                    cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(delegate()
                    {
                        txtmsg.Text = ex.Message;
                    });
                }
            }
        }

        private void detectText()
        {
            int[] ARGBPx = new int[(int)cam.PreviewResolution.Width * (int)cam.PreviewResolution.Height];
            try
            {
                PhotoCamera phCam = (PhotoCamera)cam;
                while (process)
                {
                    // TODO: detect layout/ columns/ bounding rect
                }
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(delegate()
                {
                    txtmsg.Text = ex.Message;
                });
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true))
            {
                cam = new PhotoCamera(CameraType.Primary);
                cam.Initialized += new EventHandler<CameraOperationCompletedEventArgs>(camInitialized);
                cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(captureCompleted);
                cam.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(captureImageAvailable);
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                // Phone has no camera
                // QUIT
            }
        }

        private void camInitialized(object sender, CameraOperationCompletedEventArgs e)
        {
            imgProc = new Thread(detectText);
            // TODO: start text detection thread
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                cam.Dispose();
                cam.Initialized -= camInitialized;
                cam.CaptureCompleted -= captureCompleted;
                cam.CaptureImageAvailable -= captureImageAvailable;
            }
        }
    }
}