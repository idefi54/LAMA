using BruTile;
using LAMA.Models;
using Mapsui.Widgets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class ChatViewModel : INotifyPropertyChanged
    {
        //public Xamarin.Forms.Command AddMessageCommand { get; }

        public Xamarin.Forms.Command MessageSentCommand { get; }
        public string MessageText { get; set; }
        public ObservableCollection<ChatMessageViewModel> ChatMessageListItems { get; }

        INavigation Navigation;

        long maxId = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnMessageSent()
        {
            Debug.WriteLine("OnMessageSent");
            if (MessageText != "")
            {
                ChatLogic.Instance.SendMessage(0, MessageText);
            }
            Debug.WriteLine(MessageText);
            MessageText = "";
            OnPropertyChanged("MessageText");
        }

        public ChatViewModel(INavigation navigation)
        {
            Navigation = navigation;
            MessageSentCommand = new Xamarin.Forms.Command(OnMessageSent);

            ChatMessageListItems = new ObservableCollection<ChatMessageViewModel>();

            List<ChatMessage> messages = ChatLogic.Instance.GetMessages(0, 0, Int32.MaxValue);

            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessageListItems.Add(new ChatMessageViewModel(messages[i]));
            }

            foreach (ChatMessageViewModel item in ChatMessageListItems)
            {
                if (item.ChatMessage.getID() > maxId)
                    maxId = item.ChatMessage.getID();
            }
            /*
            #region population of the test database
            if (DatabaseHolder<ChatMessage, ChatMessageStorage>.Instance.rememberedList.Count == 0)
            {
                ChatMessage message = new ChatMessage("testCp", 0, "testing testing testing testing testing testing testing testing testing", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000000);
                DatabaseHolder<ChatMessage, ChatMessageStorage>.Instance.rememberedList.add(message);
                ChatMessageListItems.Add(new ChatMessageViewModel(message));

                message = new ChatMessage("testCp2", 0, "testing2 testing2 testing2 testing2 testing2 testing2 testing2 testing2 testing2", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000000);
                DatabaseHolder<ChatMessage, ChatMessageStorage>.Instance.rememberedList.add(message);
                ChatMessageListItems.Add(new ChatMessageViewModel(message));

                message = new ChatMessage("testCp3", 0, "testing3 testing3 testing3 testing3 testing3 testing3 testing3 testing3 testing3", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000000);
                DatabaseHolder<ChatMessage, ChatMessageStorage>.Instance.rememberedList.add(message);
                ChatMessageListItems.Add(new ChatMessageViewModel(message));
            }

            #endregion
            */
            //AddMessageCommand = new Xamarin.Forms.Command(OnAddChatMessage);
        }
    }
}
