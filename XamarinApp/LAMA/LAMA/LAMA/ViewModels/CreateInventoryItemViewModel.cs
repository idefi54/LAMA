using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace LAMA.ViewModels
{
    internal class CreateInventoryItemViewModel:BaseViewModel
    {
        //Name, Decription,Free, CancelCommand, CreateCommand
        string _name;
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
        string _description;
        public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
        int _free = 0;
        public string Free { get { return _free.ToString(); } set { SetProperty(ref _free, Helpers.readInt(value)); } }

        public Xamarin.Forms.Command CancelCommand { get; }
        public Xamarin.Forms.Command CreateCommand { get; }

        INavigation _navigation;
        public CreateInventoryItemViewModel(INavigation navigation)
        {
            _navigation = navigation;

            CancelCommand = new Xamarin.Forms.Command(OnCancel);
            CreateCommand = new Xamarin.Forms.Command(OnCreate);

        }

        private async void OnCancel()
        {
            // This will pop the current page off the navigation stack
            if (Device.RuntimePlatform == Device.WPF)
            {
                await App.Current.MainPage.Navigation.PopAsync();
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        private async void OnCreate()
        {
            bool itemNameValid = InputChecking.CheckInput(Name, "Jméno Předmetu", 50);
            if (!itemNameValid) return;
            bool itemCountValid = _free > 0;
            if (!itemCountValid)
            {
                await App.Current.MainPage.DisplayAlert("Špatný Počet Předmětů", "Špatný počet předmětů na skladě (musí existovat alespoň 1 předmět)", "OK");
                return;
            }
            bool descriptionValid = InputChecking.CheckInput(Description, "Popis Předmětu", 1000, true);
            if (!descriptionValid) return;
            var list = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList;
            InventoryItem item = new InventoryItem(list.nextID(), Name, Description, 0, _free);
            list.add(item);
            // This will pop the current page off the navigation stack
            if (Device.RuntimePlatform == Device.WPF)
            {
                await App.Current.MainPage.Navigation.PopAsync();
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}
