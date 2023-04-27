using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xamarin.Forms.Platform.WPF;
using Xamarin.Forms;
using LAMA.Services;
using System.Diagnostics;

namespace LAMA.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FormsApplicationPage
    {
        private long _touchId = 0;
        public MainWindow()
        {
            InitializeComponent();
            Xamarin.Forms.Forms.Init();
            MapHandler.UseGPU = false;
            LoadApplication(new LAMA.App());
            

            MouseDown += MainWindow_MouseDown;
            MouseMove += MainWindow_MouseMove;
            MouseUp += MainWindow_MouseUp;

            if (Device.RuntimePlatform == Device.WPF)
            {
                string primaryHex = Themes.ColorPalette.PrimaryColor.ToHex();
                System.Windows.Media.Color primaryColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(primaryHex);
                //App.Current.Resources["WindowBackgroundColor"] = new System.Windows.Media.SolidColorBrush(primaryColor);
                App.Current.Resources["CommandBarBackgroundColor"] = new System.Windows.Media.SolidColorBrush(primaryColor);
                App.Current.Resources["DefaultTitleBarBackgroundColor"] = new System.Windows.Media.SolidColorBrush(primaryColor);
                App.Current.Resources["DefaultTabbedBarBackgroundColor"] = new System.Windows.Media.SolidColorBrush(primaryColor);
                App.Current.Resources["AccentColor"] = new System.Windows.Media.SolidColorBrush(primaryColor);
            }
        }

        private void SendTouchEvent(long touchId, TouchTracking.TouchActionType type, float x, float y)
        {
            LAMA.Views.ActivityGraphPage currPage = null;

            // Very dirty fix - not ideal
            float correctionX = (this.WindowState == WindowState.Maximized) ? -7.2f : 0;
            float correctionY = (this.WindowState == WindowState.Maximized) ? 6.4f : 0;

            if (Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count > 0)
            {
                int index = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count - 1;

                currPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack[index] as LAMA.Views.ActivityGraphPage;
            }

            if (currPage != null)
            {
                float difference = (float)(ActualHeight - currPage.Height);
                TouchTracking.TouchActionEventArgs args = new TouchTracking.TouchActionEventArgs(
                    id: touchId,
                    type: type,
                    location: new TouchTracking.TouchTrackingPoint(x + correctionX, y - difference + correctionY),
                    isInContact: true);

                currPage.OnTouchEffectAction(this, args);
            }
        }

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            System.Windows.Point position = e.GetPosition(this);
            SendTouchEvent(_touchId, TouchTracking.TouchActionType.Released, (float)position.X, (float)position.Y);
            _touchId++;
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(this);
            SendTouchEvent(_touchId, TouchTracking.TouchActionType.Moved, (float)position.X, (float)position.Y);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            System.Windows.Point position = e.GetPosition(this);
            SendTouchEvent(_touchId, TouchTracking.TouchActionType.Pressed, (float)position.X, (float)position.Y);
        }


    }
}