using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LAMA.Services
{
    public interface IMessageService
    {
        Task ShowAlertAsync(string message, string title = "LAMA");
        Task<bool> ShowConfirmationAsync(string message, string title = "LAMA");
        Task<int?> ShowSelectionAsync(string message, string[] options);
        Task<int?> ShowSelectionAsync(string message, List<string> options);
        Task<string> DisplayPromptAsync(string message, string title = "LAMA", string okText = "OK", string cancleText = "Zrušit");

    }

    /// <summary>
    /// Serves as a shortcut system for Xamarin display methods, so they could be used without needing ViewModel to access pages.
    /// </summary>
    public class MessageService : IMessageService
    {
        public async Task ShowAlertAsync(string message, string title = "LAMA")
        {
            await App.Current.MainPage.DisplayAlert(title, message, "Ok");
        }

        public async Task<bool> ShowConfirmationAsync(string message, string title = "LAMA")
        {
            Task<bool> task = App.Current.MainPage.DisplayAlert(title, message, "Yes", "No");
            await task;
            return task.Result;
        }
        public async Task<int?> ShowSelectionAsync(string message, List<string> options)
        {
            return await ShowSelectionAsync(message, options.ToArray());
        }

        public async Task<int?> ShowSelectionAsync(string message, string[] options)
        {
            string cancleString = "Zrušit";
            Task<string> task = App.Current.MainPage.DisplayActionSheet(message, cancleString, null, options);
            string result = await task;
            if(result == cancleString)
            {
                return null;
            }

            for (int i = 0; i < options.Length; i++)
            {
                if(options[i] == result)
                    return i;
            }

            return null;
        }

        public async Task<string> DisplayPromptAsync(string message, string title = "LAMA", string okText = "OK", string cancleText = "Zrušit")
        {
            if(Device.RuntimePlatform == Device.Android)
            {
                Task<string> task = App.Current.MainPage.DisplayPromptAsync(title, message, okText, cancleText);

                return await task;
            }
            else
            {
                return null;
            }
        }
    }
}
