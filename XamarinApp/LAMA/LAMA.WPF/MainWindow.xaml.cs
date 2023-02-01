﻿using System;
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
            LoadApplication(new LAMA.App());

            MouseDown += MainWindow_MouseDown;
            MouseMove += MainWindow_MouseMove;
            MouseUp += MainWindow_MouseUp;
        }

        private void SendTouchEvent(long touchId, TouchTracking.TouchActionType type, float x, float y)
        {
            LAMA.Views.ActivityGraphPage currPage = null;

            if (Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count > 0)
            {
                int index = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.Count - 1;

                currPage = Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack[index] as LAMA.Views.ActivityGraphPage;
            }

            if (currPage != null)
            {
                TouchTracking.TouchActionEventArgs args = new TouchTracking.TouchActionEventArgs(
                    id: touchId,
                    type: type,
                    location: new TouchTracking.TouchTrackingPoint(x, y),
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