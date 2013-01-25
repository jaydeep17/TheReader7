using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.IO;

namespace TheReader7.views
{
    public partial class OutputPage : PhoneApplicationPage
    {
        string txt;
        public OutputPage()
        {
            InitializeComponent();
            txt = (string) PhoneApplicationService.Current.State["text"];
            opText.Text = txt;
            MessageBox.Show(txt);
        }

        // TODO: do the IO process using the new WP8 async APIs in actual app
        private void saveText(string filename) {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(filename))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(filename,FileMode.Append))
                    {
                        StreamWriter writer = new StreamWriter(rawStream);
                        writer.WriteLine(txt);
                        writer.Close();
                    }
                }
                else
                {
                    using (IsolatedStorageFileStream rawStream = isf.CreateFile(filename))
                    {
                        StreamWriter writer = new StreamWriter(rawStream);
                        writer.WriteLine(txt);
                        writer.Close();
                    }
                }
            }
        }

        private void loadText(string filename)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(filename))
                {
                    using (IsolatedStorageFileStream rawStream = isf.OpenFile(filename, FileMode.Open))
                    {
                        StreamReader reader = new StreamReader(rawStream);
                        txt = reader.ReadToEnd();
                        opText.Text = txt;
                    }
                }
                else
                {
                    // TODO: speak file doesn't exist
                }
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            // TODO: I guess this works only in WP7, **check it**
            e.Cancel = true;
            NavigationService.RemoveBackEntry();
            NavigationService.GoBack();
        }


    }
}