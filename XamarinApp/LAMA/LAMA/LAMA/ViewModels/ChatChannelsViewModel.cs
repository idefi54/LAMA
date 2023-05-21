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
        public Xamarin.Forms.Command ArchiveChannelCommand { get; }
        public Xamarin.Forms.Command RestoreChannelCommand { get; }
        public Xamarin.Forms.Command RenameChannelCommand { get; }
        public Xamarin.Forms.Command ChannelSetNewNameCommand { get; }
        public Xamarin.Forms.Command HideRenameDialogCommand { get; }

        private bool _displayRenameDialog = false;
        public bool DisplayRenameDialog 
        { 
            get { return _displayRenameDialog; }
            set { SetProperty(ref _displayRenameDialog, value); }
        }

        private string _channelNewName;
        public string ChannelNewName
        {
            get { return _channelNewName; }
            set { SetProperty(ref _channelNewName, value); }
        }

        private string _previousChannelName;
        public string PreviousChannelName
        {
            get { return _previousChannelName; }
            set { SetProperty(ref _previousChannelName, value); }
        }

        public Command<object> ChatChannelTapped { get; private set; }
        private string _channelName;
        public string ChannelName
        {
            get { return _channelName; }
            set { SetProperty(ref _channelName, value); }
        }

        public bool CanCreateChannels { get; set; }
        private int selectedChannelID;

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
            ArchiveChannelCommand = new Command<object>(ArchiveChannel);
            RestoreChannelCommand = new Command<object>(RestoreChannel);
            RenameChannelCommand = new Command<object>(RenameChannel);
            ChannelSetNewNameCommand = new Command<object>(SetNewChannelName);
            HideRenameDialogCommand = new Command<object>(HideRenameDialog);

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

        private void HideRenameDialog(object obj)
        {
            DisplayRenameDialog = false;
        }

        private void PropagateChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed == null || changed.GetType() != typeof(LarpEvent) || changedAttributeIndex != 3)
                return;

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

        private async void ArchiveChannel(object obj)
        {
            if (obj.GetType() != typeof(ChatChannelsItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ChatChannelsItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            string channelName = ((ChatChannelsItemViewModel)obj).ChannelName;
            int index = Channels.IndexOf(((ChatChannelsItemViewModel)obj));
            LarpEvent.ChatChannels[index] = SpecialCharacters.archivedChannelIndicator + channelName;
            LarpEvent.ChatChannels.InvokeDataChanged();
        }

        private async void RestoreChannel(object obj)
        {
            if (obj.GetType() != typeof(ChatChannelsItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ChatChannelsItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            string channelName = ((ChatChannelsItemViewModel)obj).ChannelName;
            int index = Channels.IndexOf(((ChatChannelsItemViewModel)obj));
            LarpEvent.ChatChannels[index] = channelName;
            LarpEvent.ChatChannels.InvokeDataChanged();
        }

        private async void RenameChannel(object obj)
        {
            if (obj.GetType() != typeof(ChatChannelsItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ChatChannelsItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            string result = "testName";
            ChatChannelsItemViewModel chatChannel = (ChatChannelsItemViewModel)obj;
            selectedChannelID = Channels.IndexOf(chatChannel);
            PreviousChannelName = chatChannel.ChannelName;
            if (Device.RuntimePlatform == Device.WPF)
            {
                DisplayRenameDialog = true;
            }
            else
            {
                result = await App.Current.MainPage.DisplayPromptAsync("Nové Jméno", $"Jaké má být nové jméno kanálu (předchozí jméno: {PreviousChannelName})?");
                if (InputChecking.CheckInput(result, "Nové Jméno", 50))
                {
                    string channelName = ((ChatChannelsItemViewModel)obj).ChannelName;
                    int index = Channels.IndexOf(((ChatChannelsItemViewModel)obj));
                    if (channelName[0] == SpecialCharacters.archivedChannelIndicator)
                    {
                        LarpEvent.ChatChannels[index] = SpecialCharacters.archivedChannelIndicator + result;
                    }
                    else
                    {
                        LarpEvent.ChatChannels[index] = result;
                    }
                    LarpEvent.ChatChannels.InvokeDataChanged();
                }
            }
        }

        private void SetNewChannelName(object obj)
        {
            if (InputChecking.CheckInput(ChannelNewName, "Nové Jméno", 50))
            {
                string channelName = LarpEvent.ChatChannels[selectedChannelID];
                if (channelName[0] == SpecialCharacters.archivedChannelIndicator)
                {
                    LarpEvent.ChatChannels[selectedChannelID] = SpecialCharacters.archivedChannelIndicator + ChannelNewName;
                }
                else
                {
                    LarpEvent.ChatChannels[selectedChannelID] = ChannelNewName;
                }
                LarpEvent.ChatChannels.InvokeDataChanged();
                DisplayRenameDialog = false;
            }
        }

        public void ApplyFilter(string filter)
        {
            bool IsFiltered = !String.IsNullOrWhiteSpace(filter);

            Func<ChatChannelsItemViewModel, bool> filterCheck = (channel) =>
            {
                if (IsFiltered && !channel.ChannelName.ToLower().Contains(filter))
                    return false;
                return true;
            };

            TrulyObservableCollection<ChatChannelsItemViewModel> _filteredList = new TrulyObservableCollection<ChatChannelsItemViewModel>();
            foreach (ChatChannelsItemViewModel channel in Channels)
            {
                if (filterCheck(channel) && !channel.IsVisible && channel.CanBeVisible())
                    channel.IsVisible = true;
                else if (!filterCheck(channel) && channel.IsVisible)
                    channel.IsVisible = false;
            }
        }
    }
}