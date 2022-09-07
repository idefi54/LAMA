using BruTile;
using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class ChatViewModel
    {
        //public Xamarin.Forms.Command AddMessageCommand { get; }

        public Xamarin.Forms.Command MessageSentCommand { get; }
        public string MessageText { get; set; }
        public ObservableCollection<ChatMessageViewModel> ChatMessageListItems { get; }
        INavigation Navigation;

        long maxId = 0;

        private void OnMessageSent()
        {
            Debug.WriteLine("OnMessageSent");
            ChatLogic.Instance.SendMessage(0, MessageText);
            Debug.WriteLine(MessageText);
            MessageText = "";
        }

        public ChatViewModel(INavigation navigation)
        {
            Navigation = navigation;
            MessageSentCommand = new Xamarin.Forms.Command(OnMessageSent);

            ChatMessageListItems = new ObservableCollection<ChatMessageViewModel>();

            for (int i = 0; i < DatabaseHolder<ChatMessage, ChatMessageStorage>.Instance.rememberedList.Count; i++)
            {
                ChatMessageListItems.Add(new ChatMessageViewModel(DatabaseHolder<ChatMessage, ChatMessageStorage>.Instance.rememberedList[i]));
            }

            foreach (ChatMessageViewModel item in ChatMessageListItems)
            {
                if (item.ChatMessage.getID() > maxId)
                    maxId = item.ChatMessage.getID();
            }

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

            //AddMessageCommand = new Xamarin.Forms.Command(OnAddChatMessage);
        }
    }
}
