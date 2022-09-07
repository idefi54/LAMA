using LAMA.Models;
using LAMA.Singletons;
using Mapsui.Providers.ArcGIS.Dynamic;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    internal class ChatMessageViewModel
    {
        ChatMessage chatMessage;

        public ChatMessage ChatMessage => chatMessage;

        public string Name => chatMessage == null ? "" : chatMessage.from;

        public string Text => chatMessage == null ? "" : chatMessage.message;

        public string Time => chatMessage == null ? "" : TimeFormat(DateTimeOffset.FromUnixTimeMilliseconds(chatMessage.sentAt));
       
        public ChatMessageViewModel(ChatMessage chatMessage)
        {
            this.chatMessage = chatMessage;
        }

        string TimeFormat(DateTimeOffset time)
        {
            string result = $"{time.Hour}:{time.Minute}:{time.Second} / {time.Day}.{time.Month} / {time.Year}";
            return result;
        }
    }
}
