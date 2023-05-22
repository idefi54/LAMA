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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using static LAMA.ViewModels.DisplayActivityViewModel;

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
                        await App.Current.MainPage.DisplayAlert("Chyba Připojení", "Server na požadavek vytvo5en9 kanálu neodpověděl, zkontrolujte prosím své připojení.", "OK");
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

            if (LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageChat)) CanCreateChannels = true;
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
            ChannelModified(SpecialCharacters.archivedChannelIndicator + channelName, index);
        }

        public async void ChannelModified(string channelName, int index)
        {
            string originalName = LarpEvent.ChatChannels[index];
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

            string channelName = ((ChatChannelsItemViewModel)obj).ChannelName;
            int index = Channels.IndexOf(((ChatChannelsItemViewModel)obj));
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

            //string result = "testName";
            ChatChannelsItemViewModel chatChannel = (ChatChannelsItemViewModel)obj;
            selectedChannelID = Channels.IndexOf(chatChannel);
            PreviousChannelName = chatChannel.ChannelName;
            //if (Device.RuntimePlatform == Device.WPF)
            //{
                DisplayRenameDialog = true;
            /*}
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
            */
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
        }
    }
}