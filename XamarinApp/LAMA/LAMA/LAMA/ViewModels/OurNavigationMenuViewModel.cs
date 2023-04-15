using LAMA.Communicator;
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

        private async void OnGoToMap(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new MapPage());
        }

        private async void OnGoToActivities(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new ActivityListPage());
        }

        private async void OnGoToTestPage(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new TestPage());
        }

        private async void OnGoToChat(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new ChatChannels());
        }

        private async void OnGoToActivityGraph(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new ActivityGraphPage());
        }

        private async void OnGoToEncyclopedy(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new EncyclopedyCategoryView(null));
        }

        private async void OnGoToCP(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new CPListView());
        }
        private async void OnGoToInventory(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new InventoryView());
        }
        private async void OnGoToLarpEvent(object obj)
        {
            throw new Exception("not implemented, it's in another branch");
            //await App.Current.MainPage.Navigation.PushAsync(new LarpEventView());
        }
       private async void OnGoToPOI(object obj)
        {
            await App.Current.MainPage.Navigation.PushAsync(new POIListView());
        }

        private async void OnLogOut(object obj)
        {
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
