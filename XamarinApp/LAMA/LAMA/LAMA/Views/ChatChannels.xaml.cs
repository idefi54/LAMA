using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatChannels : ContentPage
    {
        private ChatChannelsViewModel viewModel;
        public ChatChannels()
        {
            InitializeComponent();
            viewModel = new ChatChannelsViewModel(Navigation);
            BindingContext = viewModel;
        }

        public void DialogBackground_Focused(object sender, FocusEventArgs e)
        {
            viewModel.DisplayRenameDialog = false;
        }

        public void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel.ApplyFilter(e.NewTextValue);
        }
    }
}