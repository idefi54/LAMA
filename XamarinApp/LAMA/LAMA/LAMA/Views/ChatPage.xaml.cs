﻿using LAMA.Services;
using LAMA.ViewModels;
using Mapsui.Widgets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        private IKeyboardService _keyboard;
        private ChatViewModel chatViewModel;
        private bool _shiftHeld;
        public ChatPage(string channelName)
        {
            _shiftHeld = false;
            _keyboard = DependencyService.Get<IKeyboardService>();
            _keyboard.OnKeyDown += OnKeyDown;
            _keyboard.OnKeyUp += OnKeyUp;
            InitializeComponent();

            chatViewModel = new ChatViewModel(Navigation, channelName, this);
            BindingContext = chatViewModel;
        }

        public async void ScrollToBottom(bool animated = false)
        {
            await Task.Delay(100);
            await ScrollViewMessages.ScrollToAsync(0, ScrollViewMessages.Content.Height, animated);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ScrollToBottom();
        }

        protected void OnEntryComplete(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform != Device.WPF)
            {
                chatViewModel.OnEntryComplete(sender, e);
            }
        }

        protected void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.OldTextValue != null && !_shiftHeld && Device.RuntimePlatform == Device.WPF)
            {
                string newText = e.NewTextValue;
                string oldText = e.OldTextValue;
                if (newText.Length == oldText.Length+2)
                {
                    int i;
                    for (i = 0; i < oldText.Length; i++) {
                        if (newText[i] != oldText[i]) break;
                    }
                    Debug.WriteLine(newText[i]);
                    if (newText[i] == '\r' && newText[i+1]== '\n')
                    {
                        chatViewModel.MessageText = oldText;
                        chatViewModel.OnEntryComplete(sender, null);
                    }
                }
            }
        }


        private void OnKeyDown(string key)
        {
            if (key == "LeftShift" || key == "RightShift")
                _shiftHeld = true;
        }

        private void OnKeyUp(string key)
        {            
            if (key == "LeftShift" || key == "RightShift")
                _shiftHeld = false;

            if (!TextBox.IsFocused || _shiftHeld) return;
            /*
            if (key == "Return")
                chatViewModel.OnEntryComplete(TextBox, null);
            */
        }
    }
}
