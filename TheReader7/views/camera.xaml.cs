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

namespace TheReader7.views
{
    public partial class camera : PhoneApplicationPage
    {
        private PhotoCamera cam;
        private MediaLibrary mediaLib;
        public camera()
        {
            InitializeComponent();
            mediaLib = new MediaLibrary();
            cam = new PhotoCamera(CameraType.Primary);
            cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(captureCompleted);
            cam.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(captureImageAvailable);
            viewfinderBrush.SetSource(cam);
        }

        private void captureImageAvailable(object sender, ContentReadyEventArgs e)
        {
            string filename = "" + ".jpg";
            Dispatcher.BeginInvoke(delegate()
            {
                txtmsg.Text = "Image available";
            });
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
    }
}