using LAMA.Models;
using LAMA.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class InventoryItemDetailEditViewModel:BaseViewModel
    {
        InventoryItem item;
        INavigation navigation;

        string _name;
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
        string _description;
        public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
        int _free = 0;
        public string Free { get { return _free.ToString(); } set { SetProperty(ref _free, Helpers.readInt(value)); } }

        public Xamarin.Forms.Command CancelCommand { get; }
        public Xamarin.Forms.Command SaveCommand { get; }
        


        IMessageService messageService;

        public InventoryItemDetailEditViewModel(INavigation navigation, InventoryItem item)
        {
            this.item = item;
            this.navigation = navigation;

            Name = item.name;
            Description = item.description;
            Free = item.free.ToString();
            SaveCommand = new Command(OnSave);
            CancelCommand = new Command(OnCancel);

            messageService = DependencyService.Get<IMessageService>();
        }

        private async void OnSave()
        {
            int free = Helpers.readInt(Free);
            if (free < item.taken)
            {
                messageService.ShowAlertAsync("Snažíte se zadat menší kapacitu, než kolik je zabráno");
                return;
            }
            else
            {
                item.name = Name;
                item.description = Description;
                item.SetCapacity(free);
                await navigation.PopAsync();
            }
        }
        private async void OnCancel()
        {
            await navigation.PopAsync();
        }
    }
}
