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
        Dictionary<long, InventoryItemViewModel> IDToViewModel = new Dictionary<long, InventoryItemViewModel>();

        public Xamarin.Forms.Command AddItemCommand { get; }

        //
        public Command<object> BorrowItem { get; private set; }

        public Command<object> ReturnItem { get; private set; }
        public Command<object> OpenDetailCommand { get; private set; }

        INavigation Navigation;

        long maxId = 0;


        public InventoryViewModel(INavigation navigation)
        {
            Navigation = navigation;
            ItemList = new ObservableCollection<InventoryItemViewModel>();

            var inventoryItems = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList;
            for (int i = 0; i < inventoryItems.Count; ++i) 
            {
                ItemList.Add(new InventoryItemViewModel(inventoryItems[i]));
                maxId = Math.Max(maxId, inventoryItems[i].ID);
                IDToViewModel.Add(inventoryItems[i].ID, ItemList[i]);
                SQLEvents.dataChanged += OnChange;
            }
            SQLEvents.created += OnCreated;
            SQLEvents.dataDeleted += OnDeleted;

            AddItemCommand = new Xamarin.Forms.Command(OnCreateItem);
            BorrowItem = new Command<object>(OnBorrowItem);
            ReturnItem = new Command<object>(OnReturnItem);
            OpenDetailCommand = new Command<object>(OnOpenDetail);
        }
        private async void OnOpenDetail(object obj)
        {
            if (obj.GetType() != typeof(InventoryItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(InventoryItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }
            var viewModel = (InventoryItemViewModel)obj;
            await Navigation.PushAsync(new InventoryItemDetail(viewModel.Item));
        }
        private void OnChange(Serializable changed, int index)
        {
            if(changed.GetType() != typeof(InventoryItem))
            {
                return;
            }
            //REFRESH DATA
            
        }
        private void OnCreated(Serializable made)
        {
            if (made.GetType() != typeof(InventoryItem))
            {
                return;
            }
            var item = (InventoryItem)made;
            ItemList.Add(new InventoryItemViewModel(item));
            IDToViewModel.Add(item.ID, ItemList[ItemList.Count - 1]);

        }
        private void OnDeleted(Serializable deleted)
        {
            if (deleted.GetType() != typeof(InventoryItem))
            {
                return;
            }
            var item = (InventoryItem)deleted;

            ItemList.Remove(IDToViewModel[item.ID]);
            IDToViewModel.Remove(item.ID);
        }
        

        private void OnChange(Serializable changed, int index)
        {
            if(changed.GetType() != typeof(InventoryItem))
            {
                return;
            }
            //REFRESH DATA
        }
        private void OnCreated(Serializable made)
        {
            if (made.GetType() != typeof(InventoryItem))
            {
                return;
            }
            var item = (InventoryItem)made;
            ItemList.Add(new InventoryItemViewModel(item));
            IDToViewModel.Add(item.ID, ItemList[ItemList.Count - 1]);

        }
        private void OnDeleted(Serializable deleted)
        {
            if (deleted.GetType() != typeof(InventoryItem))
            {
                return;
            }
            var item = (InventoryItem)deleted;

            ItemList.Remove(IDToViewModel[item.ID]);
            IDToViewModel.Remove(item.ID);
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
            //UPDATE DISPLAY
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
            //UPDATE DISPLAY
        }
        private async void OnCreateItem()
        {
            await Navigation.PushAsync(new CreateInventoryItemView());
        }
        
            
    }
}
