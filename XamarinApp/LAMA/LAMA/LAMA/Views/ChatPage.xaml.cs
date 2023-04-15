using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        public ChatPage(string channelName)
        {
            InitializeComponent();

            BindingContext = new ChatViewModel(Navigation, channelName, this);
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

    }
}
