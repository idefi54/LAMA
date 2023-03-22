using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class ItemSelectionViewModel : BaseViewModel
    {
        public InventoryItem Item;

        private Action<InventoryItem> _callback;

        public ObservableCollection<ItemSelectionItemViewModel> ItemsListItems { get; }

        private ItemSelectionItemViewModel _selectedItem;
        public ItemSelectionItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == null)
                    return;
                _selectedItem = value;
                Item = _selectedItem.Item;
                _callback(Item);
                Close();
            }
        }


        public ItemSelectionViewModel(Action<InventoryItem> callback, Func<InventoryItem, bool> filter)
        {
            _callback = callback;
            Item = null;
            ItemsListItems = new ObservableCollection<ItemSelectionItemViewModel>();

            for (int i = 0; i < DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.Count; i++)
            {
                InventoryItem item = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList[i];
                if (filter(item))
                    ItemsListItems.Add(new ItemSelectionItemViewModel(item));
            }
        }

        private async void Close()
        {
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
