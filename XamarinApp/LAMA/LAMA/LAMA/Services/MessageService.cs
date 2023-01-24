using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LAMA.Services
{
    public interface IMessageService
    {
        Task ShowAsync(string message, string title = "LAMA");
        Task<bool> ShowConfirmationAsync(string message, string title = "LAMA");
        Task<int?> ShowSelectionAsync(string message, string[] options, string title = "LAMA");
    }

    public class MessageService : IMessageService
    {
        public async Task ShowAsync(string message, string title = "LAMA")
        {
            await App.Current.MainPage.DisplayAlert(title, message, "Ok");
        }

        public async Task<bool> ShowConfirmationAsync(string message, string title = "LAMA")
        {
            Task<bool> task = App.Current.MainPage.DisplayAlert(title, message, "Yes", "No");
            await task;
            return task.Result;
        }

        public async Task<int?> ShowSelectionAsync(string message, string[] options, string title = "LAMA")
        {
            string cancleString = "Zrušit";
            Task<string> task = App.Current.MainPage.DisplayActionSheet(title, cancleString, null, options);
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
    }
}
