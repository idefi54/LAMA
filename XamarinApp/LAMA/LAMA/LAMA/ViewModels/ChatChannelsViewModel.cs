using BruTile;
using LAMA.Communicator;
using LAMA.Models;
using LAMA.Singletons;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class ChatChannelsViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command ChannelCreatedCommand { get; }
        public Command<object> ChatChannelTapped { get; private set; }
        private string _channelName;
        public string ChannelName
        {
            get { return _channelName; }
            set { SetProperty(ref _channelName, value); }
        }

        public bool CanCreateChannels { get; set; }
        public TrulyObservableCollection<ChatChannelsItemViewModel> Channels { get; }

        INavigation Navigation;
        public event PropertyChangedEventHandler PropertyChanged;


        private void OnChannelCreated()
        {
            bool inputValid = InputChecking.CheckInput(ChannelName, "Jméno kanálu", 50);
            if (inputValid)
            {
                LarpEvent.ChatChannels.Add(ChannelName);
                ChannelName = "";
                OnPropertyChanged("ChannelName");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ChatChannelsViewModel(INavigation navigation)
        {
            ChannelCreatedCommand = new Xamarin.Forms.Command(OnChannelCreated);
            ChatChannelTapped = new Command<object>(DisplayChannel);

            Navigation = navigation;

            Channels = new TrulyObservableCollection<ChatChannelsItemViewModel>();

            for (int i = 0; i < LarpEvent.ChatChannels.Count; i++)
            {
                Channels.Add(new ChatChannelsItemViewModel(LarpEvent.ChatChannels[i]));
            }

            if (LocalStorage.cpID == 0) CanCreateChannels = true;
            else CanCreateChannels = false;
            SQLEvents.dataChanged += PropagateChanged;
        }

        private void PropagateChanged(Serializable changed, int changedAttributeIndex)
        {
            Debug.WriteLine("Propagate Changed");
            Debug.WriteLine($"{changedAttributeIndex}: {changed.GetType().Name}");
            if (changed == null || changed.GetType() != typeof(LarpEvent) || changedAttributeIndex != 3)
                return;

            Debug.WriteLine("Propagate Changed passed");
            for (int i = Channels.Count - 1; i >= 0; i--)
            {
                Channels.Remove(Channels[i]);
            }
            for (int i = 0; i < LarpEvent.ChatChannels.Count; i++)
            {
                Channels.Add(new ChatChannelsItemViewModel(LarpEvent.ChatChannels[i]));
            }

            Channels.Refresh();
        }

        private async void DisplayChannel(object obj)
        {
            if (obj.GetType() != typeof(ChatChannelsItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ChatChannelsItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            string channelName = ((ChatChannelsItemViewModel)obj).ChannelName;
            await Navigation.PushAsync(new ChatPage(channelName));
        }
    }
}