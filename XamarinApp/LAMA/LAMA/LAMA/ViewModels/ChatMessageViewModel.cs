﻿using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class ChatMessageViewModel : BaseViewModel
    {
        ChatMessage chatMessage;

        public ChatMessage ChatMessage => chatMessage;

        public string Name => chatMessage == null ? "" : chatMessage.from;

        public string Text => chatMessage == null ? "" : chatMessage.message;

        public string Time => chatMessage == null ? "" : TimeFormat(DateTimeOffset.FromUnixTimeMilliseconds(chatMessage.sentAt).LocalDateTime);

        public bool ReceivedByServer => chatMessage == null ? false : chatMessage.receivedByServer;
        public Xamarin.Forms.Color HeaderColor => chatMessage.from == LocalStorage.clientName ? Xamarin.Forms.Color.LightSkyBlue : Xamarin.Forms.Color.LightGray;

        public Thickness Margin => chatMessage.from == LocalStorage.clientName ? new Thickness(100, 10, 20, 10) : new Thickness(20, 10, 100, 10);
       
        public ChatMessageViewModel(ChatMessage chatMessage)
        {
            this.chatMessage = chatMessage;
            Debug.WriteLine(chatMessage.from == LocalStorage.clientName);
        }

        string TimeFormat(DateTimeOffset time)
        {
            string result = $"{time.Hour}:{time.Minute}:{time.Second} / {time.Day}.{time.Month} / {time.Year}";
            return result;
        }

        internal void UpdateMessage(ChatMessage message)
        {
            SetProperty(ref chatMessage, message);
        }
    }
}
