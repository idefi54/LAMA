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
    public class ChatChannelsViewModel : INotifyPropertyChanged
    {
        public Xamarin.Forms.Command ChannelCreatedCommand { get; }
        public Command<object> ChatChannelTapped { get; private set; }
        public string ChannelName { get; set; }

        public bool CanCreateChannels { get; set; }
        public ObservableCollection<String> Channels { get; set; }

        INavigation Navigation;

        public event PropertyChangedEventHandler PropertyChanged;


        private void OnChannelCreated()
        {
            if (ChannelName != "")
            {
                LarpEvent.ChatChannels.Add(ChannelName);
            }
            Debug.WriteLine($"ChatCannels: {LarpEvent.ChatChannels.Count}");
            ChannelName = "";
            OnPropertyChanged("ChannelName");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ChatChannelsViewModel(INavigation navigation)
        {
            LarpEvent.ChatChannels.dataChanged += PropagateChanged;
            ChannelCreatedCommand = new Xamarin.Forms.Command(OnChannelCreated);
            ChatChannelTapped = new Command<object>(DisplayChannel);

            Navigation = navigation;

            Channels = new ObservableCollection<string>();

            for (int i = 0; i < LarpEvent.ChatChannels.Count; i++)
            {
                Channels.Add(LarpEvent.ChatChannels[i]);
            }

            if (LocalStorage.cpID == 0) CanCreateChannels = true;
            else CanCreateChannels = false;
        }

        private void PropagateChanged()
        {
            Debug.WriteLine("PropagateChanged");
            Channels.Clear();

            for (int i = 0; i < LarpEvent.ChatChannels.Count; i++)
            {
                Channels.Add(LarpEvent.ChatChannels[i]);
            }
        }

        private async void DisplayChannel(object obj)
        {
            if (obj.GetType() != typeof(string))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(string).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            string channelName = (string)obj;
            await Navigation.PushAsync(new ChatPage(channelName));
        }
    }
}