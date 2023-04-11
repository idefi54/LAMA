using LAMA.Colors;
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

            if (Device.RuntimePlatform == Device.WPF)
            {
                //MainPage = new NavigationPage(new LoginPage());
                MainPage = new NavigationPage(new ChooseClientServerPage())
                {
                    BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                    BarBackgroundColor = ColorPalette.PrimaryColor
                };
            }
            else
            {
                MainPage = new AppShell();
            }
            InitializeComponent();
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
