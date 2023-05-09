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
using System.Linq;
using LAMA.Singletons;
using LAMA.Views;
using System.Xml.Linq;

namespace LAMA.ViewModels
{
    internal class ChatViewModel : BaseViewModel
    {
        //public Xamarin.Forms.Command AddMessageCommand { get; }

        public Xamarin.Forms.Command MessageSentCommand { get; }
        private string _messageText;
        public string MessageText { get { return _messageText; } set { SetProperty(ref _messageText, value); } }
        public ObservableCollection<ChatMessageViewModel> ChatMessageListItems { get; set; }

        INavigation Navigation;

        long maxId = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private int channelID;

        private ChatPage page;

        private void SortMessagesInPlace(ObservableCollection<ChatMessageViewModel> collection)
        {
            List<ChatMessageViewModel> sorted;
            sorted = new List<ChatMessageViewModel>(collection.OrderBy(p => p.ChatMessage.sentAt));
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnEntryComplete(object sender, EventArgs e)
        {
            OnMessageSent();
        }

        private void OnMessageSent()
        {
            if (!String.IsNullOrWhiteSpace(MessageText))
            {
                ChatLogic.Instance.SendMessage(channelID, MessageText);
                MessageText = "";
                OnPropertyChanged("MessageText");
            }
        }

        public ChatViewModel(INavigation navigation, string channelName, ChatPage page)
        {
            this.page = page;
            SQLEvents.created += PropagateCreated;
            SQLEvents.dataChanged += PropagateChanged;
            SQLEvents.dataDeleted += PropagateDeleted;

            channelID = LarpEvent.ChatChannels.FindIndex(x => x == channelName);

            Navigation = navigation;
            MessageSentCommand = new Xamarin.Forms.Command(OnMessageSent);

            ChatMessageListItems = new ObservableCollection<ChatMessageViewModel>();

            List<ChatMessage> messages = ChatLogic.Instance.GetMessages(channelID, 0, Int32.MaxValue);
        
            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessageListItems.Add(new ChatMessageViewModel(messages[i]));
            }

            foreach (ChatMessageViewModel item in ChatMessageListItems)
            {
                if (item.ChatMessage.getID() > maxId)
                    maxId = item.ChatMessage.getID();
            }

            SortMessagesInPlace(ChatMessageListItems);
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

        private void PropagateCreated(Serializable created)
        {
            if (created == null || created.GetType() != typeof(ChatMessage))
                return;

            ChatMessage message = (ChatMessage)created;

            ChatMessageViewModel item = ChatMessageListItems.Where(x => x.ChatMessage.ID == message.ID).FirstOrDefault();

            if (item != null || message.channel != channelID)
                return;

            item = new ChatMessageViewModel(message);
            ChatMessageListItems.Add(item);
            SortMessagesInPlace(ChatMessageListItems);
            page.ScrollToBottom();
        }

        private void PropagateChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed == null || changed.GetType() != typeof(ChatMessage))
                return;

            ChatMessage message = (ChatMessage)changed;

            ChatMessageViewModel item = ChatMessageListItems.Where(x => x.ChatMessage.ID == message.ID).FirstOrDefault();

            if (item == null || message.channel != channelID)
                return;

            item.UpdateMessage(message);
            SortMessagesInPlace(ChatMessageListItems);
        }

        private void PropagateDeleted(Serializable deleted)
        {
            if (deleted == null || deleted.GetType() != typeof(ChatMessage))
                return;

            ChatMessage message = (ChatMessage)deleted;

            ChatMessageViewModel item = ChatMessageListItems.Where(x => x.ChatMessage.ID == message.ID).FirstOrDefault();

            if (item != null && message.channel == channelID)
            {
                ChatMessageListItems.Remove(item);
            }
            SortMessagesInPlace(ChatMessageListItems);
        }
    }
}
