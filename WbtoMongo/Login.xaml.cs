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
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WbtoMongo
{
    public partial class Login : PhoneApplicationPage
    {

        ProgressIndicator prog;

        public Login()
        {
            InitializeComponent();

            SystemTray.SetIsVisible(this, false);
            //SystemTray.SetOpacity(this, 0.5);
            SystemTray.SetBackgroundColor(this, Colors.White);
            SystemTray.SetForegroundColor(this, Colors.Black);

            prog = new ProgressIndicator();
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            prog.Text = "正在登录...";

            SystemTray.SetProgressIndicator(this, prog);

            // initialize login components
            var settings = IsolatedStorageSettings.ApplicationSettings;
            // username
            try
            {
                tbUsername.Text = settings[Constants.KEY_USERNAME].ToString();
            }
            catch (KeyNotFoundException) { }
            // save password
            try
            {
                cbSavePassword.IsChecked = (bool)settings[Constants.KEY_SAVE_PASSWORD];
            }
            catch (KeyNotFoundException) { }
            // password
            if ((bool)cbSavePassword.IsChecked)
            {
                try
                {
                    tbPassword.Password = settings[Constants.KEY_PASSWORD].ToString();
                }
                catch (KeyNotFoundException) { }
            }
        }


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.RemoveBackEntry();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            var result = MessageBox.Show("确定要退出应用吗？",
                "", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
            {
                e.Cancel = true;
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            doLogin();
        }
         
        private void doLogin()
        {
            String u = tbUsername.Text.Trim();
            String p = tbPassword.Password.Trim();
            if (u == string.Empty)
            {
                MessageBox.Show("empty username!");
                return;
            }
            if (p == string.Empty)
            {
                MessageBox.Show("empty password!");
                return;
            }

            WbtoApi.login(u, p, (response) =>
            {
                SystemTray.SetIsVisible(this, false);

                String json = response.Content;
                JObject ret = JObject.Parse(json);
                JObject user = (JObject)ret["user"];
                if (user == null)
                {
                    MessageBox.Show((string)ret["msg"]);
                    return;
                }
                //settings.Add(Constants.KEY_LOGIN_USER, user);

                // add to session
                Session.username = u;
                Session.password = p;

                // save status to storage
                var settings = IsolatedStorageSettings.ApplicationSettings;
                try
                {
                    settings[Constants.KEY_USERNAME] = u;
                }
                catch (KeyNotFoundException)
                {
                    settings.Add(Constants.KEY_USERNAME, u);
                }
                if ((bool)cbSavePassword.IsChecked)
                {
                    try
                    {
                        settings[Constants.KEY_PASSWORD] = p;
                    }
                    catch (KeyNotFoundException)
                    {
                        settings.Add(Constants.KEY_PASSWORD, p);
                    }
                }
                else
                {
                    try
                    {
                        settings[Constants.KEY_PASSWORD] = "";
                    }
                    catch (KeyNotFoundException)
                    {
                        settings.Add(Constants.KEY_PASSWORD, "");
                    }
                }
                try
                {
                    settings[Constants.KEY_SAVE_PASSWORD] = cbSavePassword.IsChecked;
                }
                catch (KeyNotFoundException)
                {
                    settings.Add(Constants.KEY_SAVE_PASSWORD, cbSavePassword.IsChecked);
                }

                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
                // NavigationService.GoBack();
            });

            SystemTray.SetIsVisible(this, true);
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove(Constants.KEY_USERNAME);
            settings.Remove(Constants.KEY_PASSWORD);
            settings.Remove(Constants.KEY_SAVE_PASSWORD);

            tbUsername.Text = "";
            tbPassword.Password = "";
            cbSavePassword.IsChecked = false;
        }

    }

}