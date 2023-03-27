using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();
            if (Device.RuntimePlatform != Device.WPF)
            {
                App.Current.Resources["CommandBarBackgroundColor"] = new SolidColorBrush(Colors.ColorPalette.PrimaryColor);
                App.Current.Resources["DefaultTitleBarBackgroundColor"] = new SolidColorBrush(Colors.ColorPalette.PrimaryColor);
                App.Current.Resources["DefaultTabbedBarBackgroundColor"] = new SolidColorBrush(Colors.ColorPalette.PrimaryColor);
                App.Current.Resources["AccentColor"] = new SolidColorBrush(Colors.ColorPalette.PrimaryColor);
            }
        }
    }
}