using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class InventoryViewModel 
    {
        public ObservableCollection<InventoryItemViewModel> ItemList { get; }


        public Xamarin.Forms.Command AddItemCommand { get; }

        //
        public Command<object> BorrowItem { get; private set; }

        public Command<object> ReturnItem { get; private set; }

        INavigation Navigation;

        int maxId = 0;


        public InventoryViewModel(INavigation navigation)
        {
            Navigation = navigation;
            ItemList = new ObservableCollection<InventoryItemViewModel>();

            var inventoryItems = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList;
            for(int i =0; i< inventoryItems.Count; ++i)
            {
                ItemList.Add(new InventoryItemViewModel(inventoryItems[i]));
                maxId = Math.Max(maxId, inventoryItems[i].ID);
            }


            // AddItemCommand = new Xamarin.Forms.Command(OnCreateItem);
            BorrowItem = new Command<object>(OnBorrowItem);
            ReturnItem = new Command<object>(OnReturnItem);
        }

        private async void OnBorrowItem(object obj)
        {
            if(obj.GetType() != typeof (InventoryItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(InventoryItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }
            var itemViewModel = (InventoryItemViewModel)obj;
            
            itemViewModel.Item.Borrow(1);
            //UPDATE DATA
        }
        private async void OnReturnItem(object obj)
        {
            if (obj.GetType() != typeof(InventoryItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(InventoryItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }
            var itemViewModel = (InventoryItemViewModel)obj;

            itemViewModel.Item.Return(1);
            //UPDATE DATA
        }

        
    }
}
