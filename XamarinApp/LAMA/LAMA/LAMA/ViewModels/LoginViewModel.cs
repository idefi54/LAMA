using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Xamarin.Forms.Command LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Xamarin.Forms.Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
        }
    }
}
