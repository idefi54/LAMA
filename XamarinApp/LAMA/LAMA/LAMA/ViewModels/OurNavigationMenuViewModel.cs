using LAMA.Themes;
using LAMA.Communicator;
using LAMA.Singletons;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class OurNavigationMenuViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command GoToMap { get; }
        public Xamarin.Forms.Command GoToActivities { get; }
        public Xamarin.Forms.Command GoToTestPage { get; }
        public Xamarin.Forms.Command GoToChat { get; }
        public Xamarin.Forms.Command GoToActivityGraph { get; }
        public Xamarin.Forms.Command GoToEncyclopedy { get; }
        public Xamarin.Forms.Command GoToCP { get; }
        public Xamarin.Forms.Command GoToInventory { get; }
        public Xamarin.Forms.Command GoToLarpEvent { get; }
        public Xamarin.Forms.Command GoToPOI { get; }
        public Xamarin.Forms.Command LogOut { get; }

        public OurNavigationMenuViewModel()
        {
            GoToMap = new Xamarin.Forms.Command(OnGoToMap);
            GoToActivities = new Xamarin.Forms.Command(OnGoToActivities);
            GoToTestPage = new Xamarin.Forms.Command(OnGoToTestPage);
            GoToChat = new Xamarin.Forms.Command(OnGoToChat);
            GoToActivityGraph = new Xamarin.Forms.Command(OnGoToActivityGraph);
            GoToEncyclopedy = new Xamarin.Forms.Command(OnGoToEncyclopedy);
            GoToCP = new Xamarin.Forms.Command(OnGoToCP);
            GoToInventory = new Xamarin.Forms.Command(OnGoToInventory);
            GoToLarpEvent = new Xamarin.Forms.Command(OnGoToLarpEvent);
            GoToPOI = new Xamarin.Forms.Command(OnGoToPOI);
            LogOut = new Xamarin.Forms.Command(OnLogOut);
        }

        private void OnGoToMap(object obj)
        {
            App.Current.MainPage = new NavigationPage(new MapPage())
            {
                    BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                    BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new MapPage());
        }

        private void OnGoToActivities(object obj)
        {
            App.Current.MainPage = new NavigationPage(new ActivityListPage())
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new ActivityListPage());
        }

        //Remove later (after testing is done
        private void OnGoToTestPage(object obj)
        {
            App.Current.MainPage = new NavigationPage(new TestPage())
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new TestPage());
        }

        private void OnGoToChat(object obj)
        {
            App.Current.MainPage = new NavigationPage(new ChatChannels())
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new ChatChannels());
        }

        private void OnGoToActivityGraph(object obj)
        {
            App.Current.MainPage = new NavigationPage(new ActivityGraphPage())
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new ActivityGraphPage());
        }

        private void OnGoToEncyclopedy(object obj)
        {
            App.Current.MainPage = new NavigationPage(new EncyclopedyCategoryView(null))
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new EncyclopedyCategoryView(null));
        }

        private void OnGoToCP(object obj)
        {
            App.Current.MainPage = new NavigationPage(new CPListView())
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new CPListView());
        }
        private void OnGoToInventory(object obj)
        {
            App.Current.MainPage = new NavigationPage(new InventoryView())
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new InventoryView());
        }
        private void OnGoToLarpEvent(object obj)
        {
            App.Current.MainPage = new NavigationPage(new LarpEventView())
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //throw new Exception("not implemented, it's in another branch");
            //await App.Current.MainPage.Navigation.PushAsync(new LarpEventView());
        }
        private void OnGoToPOI(object obj)
        {
            App.Current.MainPage = new NavigationPage(new POIListView())
            {
                BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                BarBackgroundColor = ColorPalette.PrimaryColor
            };
            //await App.Current.MainPage.Navigation.PushAsync(new POIListView());
        }

        private async void OnLogOut(object obj)
        {
            bool result = await Application.Current.MainPage.DisplayAlert("Odhlášení", "Opravdu se chcete odhlásit?", "Ano", "Ne");
            if (result)
            {
                CommunicationInfo.Instance.Communicator = null;
                CommunicationInfo.Instance.ServerName = "";
                if (Device.RuntimePlatform == Device.WPF)
                {

                    App.Current.MainPage = new NavigationPage(new ChooseClientServerPage())
                    {
                        BarBackground = new SolidColorBrush(ColorPalette.PrimaryColor),
                        BarBackgroundColor = ColorPalette.PrimaryColor
                    };
                    //await App.Current.MainPage.Navigation.PushAsync(new ChooseClientServerPage());
                }
                else
                {
                    await Shell.Current.GoToAsync("//ClientChooseServerPage");
                }
            }
        }
    }
}
