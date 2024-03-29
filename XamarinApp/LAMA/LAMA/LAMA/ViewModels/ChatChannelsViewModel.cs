﻿using BruTile;
using LAMA.Communicator;
using LAMA.Models;
using LAMA.Services;
using LAMA.Singletons;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using static LAMA.ViewModels.ActivityDetailsViewModel;

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

        public bool CanManageChat { get; set; }
        private int selectedChannelID;

        public delegate void ChannelModifiedDelegate(int channelID, string name);
        public static event ChannelModifiedDelegate channelModified;
        public delegate void ChannelModifiedResultDelegate(bool wasModified);
        public static event ChannelModifiedResultDelegate channelModifiedResult;

        public delegate void ChannelCreatedDelegate(string name);
        public static event ChannelCreatedDelegate channelCreated;
        public delegate void ChannelCreatedResultDelegate(bool wasCreated);
        public static event ChannelCreatedResultDelegate channelCreatedResult;

        public static void InvokeChannelModified(int channelID, string name)
        {
            channelModified?.Invoke(channelID, name);
        }

        public static void InvokeChannelModifiedResult(bool wasModified)
        {
            channelModifiedResult?.Invoke(wasModified);
        }

        public static void InvokeChannelCreated(string name)
        {
            channelCreated?.Invoke(name);
        }

        public static void InvokeChannelCreatedResult(bool wasCreated)
        {
            channelCreatedResult?.Invoke(wasCreated);
        }

        public static bool TryAddChannel(string channelName)
        {
            if (CommunicationInfo.Instance.IsServer)
            {
                LarpEvent.ChatChannels.Add(channelName);
                return true;
            }
            return false;
        }

        public static bool TryModifyChannel(int id, string name)
        {
            if (CommunicationInfo.Instance.IsServer && LarpEvent.ChatChannels.Count > id && id >= 0)
            {
                LarpEvent.ChatChannels[id] = name;
                return true;
            }
            return false;
        }

        public TrulyObservableCollection<ChatChannelsItemViewModel> Channels { get; }
        public TrulyObservableCollection<ChatChannelsItemViewModel> ArchivedChannels { get; }

        INavigation Navigation;
        public event PropertyChangedEventHandler PropertyChanged;


        private async void OnChannelCreated()
        {
            bool inputValid = InputChecking.CheckInput(ChannelName, "Jméno kanálu", 50);
            if (inputValid)
            {
                if (CommunicationInfo.Instance.IsServer)
                {
                    LarpEvent.ChatChannels.Add(ChannelName);
                }
                else
                {
                    string name = ChannelName;
                    InvokeChannelCreated(name);
                    AutoResetEvent waitHandle = new AutoResetEvent(false);
                    IsBusy = true;
                    bool success = false;
                    ChannelCreatedResultDelegate ourDelegate = delegate (bool modifiedSuccessfully)
                    {
                        success = modifiedSuccessfully;
                        waitHandle.Set();  // signal that the finished event was raised
                    };

                    channelCreatedResult += ourDelegate;
                    bool receivedSignal = await Task.Run(() => waitHandle.WaitOne(5000));
                    channelCreatedResult -= ourDelegate;
                    if (!receivedSignal)
                    {
                        IsBusy = false;
                        await App.Current.MainPage.DisplayAlert("Chyba Připojení", "Server na požadavek vytvoření kanálu neodpověděl, zkontrolujte prosím své připojení.", "OK");
                        return;
                    }
                    else if (!success)
                    {
                        IsBusy = false;
                        await App.Current.MainPage.DisplayAlert("Tvorba Zamítnuta", "Server odmítl vytvoření kanálu", "OK");
                        return;
                    }
                }
                var Toast = DependencyService.Get<ToastInterface>();
                IsBusy = false;
                if (Toast != null)
                    Toast.DoTheThing("Tvorba kanálu " + ChannelName + " proběhla úspěšně.");
                else await App.Current.MainPage.DisplayAlert("Tvorba Kanálu", "Tvorba kanálu " + ChannelName + " proběhla úspěšně.", "OK");
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

        private IMessageService messageService;

        public ChatChannelsViewModel(INavigation navigation)
        {
            messageService = DependencyService.Get<IMessageService>();

            ChannelCreatedCommand = new Xamarin.Forms.Command(OnChannelCreated);
            ChatChannelTapped = new Command<object>(DisplayChannel);
            ArchiveChannelCommand = new Command<object>(ArchiveChannel);
            RestoreChannelCommand = new Command<object>(RestoreChannel);
            RenameChannelCommand = new Command<object>(RenameChannel);
            ChannelSetNewNameCommand = new Command<object>(SetNewChannelName);
            HideRenameDialogCommand = new Command<object>(HideRenameDialog);

            Navigation = navigation;

            Channels = new TrulyObservableCollection<ChatChannelsItemViewModel>();
            ArchivedChannels = new TrulyObservableCollection<ChatChannelsItemViewModel>();

            for (int i = 0; i < LarpEvent.ChatChannels.Count; i++)
            {
                if (LarpEvent.ChatChannels[i][0] == SpecialCharacters.archivedChannelIndicator)
                {
                    ArchivedChannels.Add(new ChatChannelsItemViewModel(LarpEvent.ChatChannels[i]));
                }
                else
                {
                    Channels.Add(new ChatChannelsItemViewModel(LarpEvent.ChatChannels[i]));
                }
            }

            CanManageChat = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat);
            SQLEvents.dataChanged += PropagateChanged;
        }

        private void HideRenameDialog(object obj)
        {
            DisplayRenameDialog = false;
        }

        private void PropagateChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed is CP cp && cp.ID == LocalStorage.cpID && changedAttributeIndex == 9)
                CanManageChat = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat);

            if (changed == null || changed.GetType() != typeof(LarpEvent) || changedAttributeIndex != 3)
                return;

            for (int i = Channels.Count - 1; i >= 0; i--)
            {
                Channels.Remove(Channels[i]);
            }
            for (int i = ArchivedChannels.Count - 1; i >= 0; i--)
            {
                ArchivedChannels.Remove(ArchivedChannels[i]);
            }

            for (int i = 0; i < LarpEvent.ChatChannels.Count; i++)
            {
                if (LarpEvent.ChatChannels[i][0] == SpecialCharacters.archivedChannelIndicator)
                {
                    ArchivedChannels.Add(new ChatChannelsItemViewModel(LarpEvent.ChatChannels[i]));
                }
                else
                {
                    Channels.Add(new ChatChannelsItemViewModel(LarpEvent.ChatChannels[i]));
                }
            }

            Channels.Refresh();
            ArchivedChannels.Refresh();
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

            bool result = await messageService.ShowConfirmationAsync("Opravdu chcete archivovat tento kanál? Nikdo kromě lidí s pravomocí archivovat nebude schopen kanál vidět.", "Opravdu archivovat?");
            if (!result)
                return;
            
            ChatChannelsItemViewModel channel = ((ChatChannelsItemViewModel)obj);
            string channelName = channel.ChannelName;
            int index = LarpEvent.ChatChannels.IndexOf(channelName);
            ChannelModified(SpecialCharacters.archivedChannelIndicator + channelName, index);
        }

        public async void ChannelModified(string channelName, int index)
        {
            string originalName = LarpEvent.ChatChannels[index];
            if (originalName[0] == SpecialCharacters.archivedChannelIndicator) originalName = originalName.Substring(1);
            if (CommunicationInfo.Instance.IsServer)
            {
                LarpEvent.ChatChannels[index] = channelName;
            }
            else
            {
                InvokeChannelModified(index, channelName);
                AutoResetEvent waitHandle = new AutoResetEvent(false);
                IsBusy = true;
                bool success = false;
                ChannelModifiedResultDelegate ourDelegate = delegate (bool modifiedSuccessfully)
                {
                    success = modifiedSuccessfully;
                    waitHandle.Set();  // signal that the finished event was raised
                };

                channelModifiedResult += ourDelegate;
                bool receivedSignal = await Task.Run(() => waitHandle.WaitOne(5000));
                channelModifiedResult -= ourDelegate;
                if (!receivedSignal)
                {
                    IsBusy = false;
                    await App.Current.MainPage.DisplayAlert("Chyba Připojení", "Server na požadavek změny kanálu neodpověděl, zkontrolujte prosím své připojení.", "OK");
                    return;
                }
                else if (!success)
                {
                    IsBusy = false;
                    await App.Current.MainPage.DisplayAlert("Změna Zamítnuta", "Server odmítl změnu kanálu", "OK");
                    return;
                }
            }
            var Toast = DependencyService.Get<ToastInterface>();
            IsBusy = false;
            if (Toast != null)
                Toast.DoTheThing("Změna kanálu " + originalName + " proběhla úspěšně.");
            else await App.Current.MainPage.DisplayAlert("Změna Kanálu", "Změna kanálu " + originalName + " proběhla úspěšně.", "OK");
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

            ChatChannelsItemViewModel channel = ((ChatChannelsItemViewModel)obj);
            string channelName = channel.ChannelName;
            int index = LarpEvent.ChatChannels.IndexOf(SpecialCharacters.archivedChannelIndicator + channelName);
            ChannelModified(channelName, index);
        }

        private async void RenameChannel(object obj)
        {
            if (obj.GetType() != typeof(ChatChannelsItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ChatChannelsItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            ChatChannelsItemViewModel channel = (ChatChannelsItemViewModel)obj;
            string channelName = channel.ChannelName;
            selectedChannelID = LarpEvent.ChatChannels.IndexOf((channel.archived ? SpecialCharacters.archivedChannelIndicator.ToString() : "") + channelName);
            PreviousChannelName = channel.ChannelName;
            DisplayRenameDialog = true;
        }

        private void SetNewChannelName(object obj)
        {
            if (InputChecking.CheckInput(ChannelNewName, "Nové Jméno", 50))
            {
                string channelName = LarpEvent.ChatChannels[selectedChannelID];
                DisplayRenameDialog = false;
                if (channelName[0] == SpecialCharacters.archivedChannelIndicator)
                    ChannelModified(SpecialCharacters.archivedChannelIndicator + ChannelNewName, selectedChannelID);
                else
                    ChannelModified(ChannelNewName, selectedChannelID);
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

            foreach (ChatChannelsItemViewModel channel in ArchivedChannels)
            {
                if (filterCheck(channel) && !channel.IsVisible && channel.CanBeVisible())
                    channel.IsVisible = true;
                else if (!filterCheck(channel) && channel.IsVisible)
                    channel.IsVisible = false;
            }
        }
    }
}