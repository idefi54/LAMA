using LAMA.Services;
using LAMA.Views;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA
{
    public partial class App : Application
    {

        public App()
        {
            DependencyService.Register<IMessageService, MessageService>();

            InitializeComponent();

            if (Device.RuntimePlatform == Device.WPF)
            {
                //MainPage = new NavigationPage(new LoginPage());
                MainPage = new NavigationPage(new ChooseClientServerPage())
                {
                    BarBackground = new SolidColorBrush(new Color((double)33 / 255, (double)144 / 255, (double)243 / 255)),
                    BarBackgroundColor = new Color(33, 144, 243)
                };
            }
            else
            {
                MainPage = new AppShell();
            }
        }


        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
