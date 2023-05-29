using LAMA.Communicator;
using LAMA.Services;
using LAMA.Singletons;
using LAMA.ViewModels;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LAMA
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        const string STOP = "Stop Background Location Tracking";
        const string START = "Start Background Location Tracking";

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DisplayActivityPage), typeof(DisplayActivityPage));
            LocationTracking.Text =
                GetLocationService.IsRunning
                ? START
                : STOP;

            MessagingCenter.Subscribe(this, "ServiceStarted", (StartServiceMessage message) => LocationTracking.Text = STOP);
            MessagingCenter.Subscribe(this, "ServiceStopped", (StopServiceMessage message) => LocationTracking.Text = START);
        }

        private void OnLocationTrackingClicked(object sender, EventArgs e)
        {

            var item = sender as MenuItem;

            if (Device.RuntimePlatform == Device.Android && item.Text == START)
            {
                StartService();
                item.Text = STOP;
                Debug.WriteLine("START");
            } else if (Device.RuntimePlatform == Device.Android)
            {
                StopService();
                item.Text = START;
                Debug.WriteLine("STOP");
            }
        }

        private void StartService()
        {
            var startServiceMessage = new StartServiceMessage();
            MessagingCenter.Send(startServiceMessage, "ServiceStarted");
        }

        private void StopService()
        {
            var stopServiceMessage = new StopServiceMessage();
            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Odhlášení", "Opravdu se chcete odhlásit?", "Ano", "Ne");
            if (result)
            {
                CommunicationInfo.Instance.Communicator = null;
                CommunicationInfo.Instance.ServerName = "";
                if (Device.RuntimePlatform == Device.WPF)
                {
                    await App.Current.MainPage.Navigation.PushAsync(new ChooseClientServerPage());
                }
                else
                {
                    await Shell.Current.GoToAsync("//ClientChooseServerPage");
                }
            }
        }
    }
}
