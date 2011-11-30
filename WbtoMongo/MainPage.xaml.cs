using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using RestSharp;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.IO.IsolatedStorage;
using Newtonsoft.Json.Linq;

namespace WbtoMongo
{
    public partial class MainPage : PhoneApplicationPage
    {
        ProgressIndicator prog;
        PhotoChooserTask selectphoto;
        CameraCaptureTask cameraCapture;

        String imageFileName;
        Stream imageStream;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            SystemTray.SetIsVisible(this, false);
            //SystemTray.SetOpacity(this, 0.5);
            SystemTray.SetBackgroundColor(this, Colors.White);
            SystemTray.SetForegroundColor(this, Colors.Black);

            prog = new ProgressIndicator();
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            prog.Text = "正在发送...";

            SystemTray.SetProgressIndicator(this, prog);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.RemoveBackEntry();

            if (!Session.hasUsernameAndPassword())
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;
                try
                {
                    String u = settings[Constants.KEY_USERNAME].ToString();
                    String p = settings[Constants.KEY_PASSWORD].ToString();
                    Session.username = u;
                    Session.password = p;
                    if (u == string.Empty || p == string.Empty)
                    {
                        navigateToLoginPage();
                    }
                }
                catch (KeyNotFoundException)
                {
                    navigateToLoginPage();
                }
            }

        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            String text = tbText.Text;
            //text = text + "  |  " + DateTime.Now.ToString();

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                BinaryReader reader = new BinaryReader(imageStream);
                byte[] bytes = reader.ReadBytes((int)imageStream.Length);
                WbtoApi.upload(text, imageFileName, bytes, submitCallback);
            }
            else
            {
                WbtoApi.update(text, submitCallback);
            }

            SystemTray.SetIsVisible(this, true);
        }

        private void submitCallback(RestResponse response)
        {
            String caption = "发送微博";
            SystemTray.SetIsVisible(this, false);

            String json = response.Content;
            JObject ret = JObject.Parse(json);
            string retcode = (string)ret["retcode"];
            if (retcode == "0")
            {
                // clear UI and data
                clearPhoto();
                tbText.Text = "";
            }
            else
            {
                caption = "发送失败";
            }
            MessageBox.Show((string)ret["msg"], caption, MessageBoxButton.OK);
        }

        private void btnPickPic_Click(object sender, EventArgs e)
        {
            imageFileName = string.Empty;
            imageStream = null;

            selectphoto = new PhotoChooserTask();
            selectphoto.ShowCamera = true;
            selectphoto.Completed += new EventHandler<PhotoResult>(selectphoto_Completed);
            selectphoto.Show();

        }

        void selectphoto_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {
                btnClearPic.Visibility = System.Windows.Visibility.Visible;

                imageStream = e.ChosenPhoto;
                imageFileName = e.OriginalFileName;
                
                image1.Source = new BitmapImage(new Uri(imageFileName));
            }
        }

        private void btnCapPhoto_Click(object sender, EventArgs e)
        {
            imageFileName = string.Empty;
            imageStream = null;

            cameraCapture = new CameraCaptureTask();
            cameraCapture.Completed += new EventHandler<PhotoResult>(cameracapture_Completed);
            cameraCapture.Show();
        }

        void cameracapture_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK && e.ChosenPhoto != null)
            {
                btnClearPic.Visibility = System.Windows.Visibility.Visible;

                imageStream = e.ChosenPhoto;
                imageFileName = e.OriginalFileName;

                image1.Source = new BitmapImage(new Uri(imageFileName));
            }
        }

        #region private methods        
        private void navigateToLoginPage()
        {
            NavigationService.Navigate(new Uri("/Login.xaml", UriKind.RelativeOrAbsolute));
        }
        private void clearPhoto()
        {
            imageFileName = null;
            imageStream = null;
            image1.Source = null;

            btnClearPic.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            var result = MessageBox.Show("确定要退出应用吗？", "", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
            {
                e.Cancel = true;
            }
        }

        private void ApplicationBarMenuItem_SwitchAccount_Click(object sender, EventArgs e)
        {
            navigateToLoginPage();
        }

        private void btnClearPic_Click(object sender, RoutedEventArgs e)
        {
            clearPhoto();
        }

    }
}