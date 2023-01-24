using LAMA.Models;
using LAMA.Singletons;
using LAMA.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Text;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class InventoryViewModel :BaseViewModel
    {
        TrulyObservableCollection<InventoryItemViewModel> _ItemList = new TrulyObservableCollection<InventoryItemViewModel>();
        public TrulyObservableCollection<InventoryItemViewModel> ItemList { get { return _ItemList; } private set { SetProperty(ref _ItemList, value); } }
        Dictionary<long, InventoryItemViewModel> IDToViewModel = new Dictionary<long, InventoryItemViewModel>();

        public string FilterText { get; set; }

        public Xamarin.Forms.Command AddItemCommand { get; }
        
        //
        public Command<object> BorrowItem { get; private set; }

        public Command<object> ReturnItem { get; private set; }
        public Command<object> OpenDetailCommand { get; private set; }

        public bool CanChangeItems { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageInventory);} set { } }

        INavigation Navigation;

        long maxId = 0;
        

        public InventoryViewModel(INavigation navigation)
        {
            Navigation = navigation;
            ItemList = new TrulyObservableCollection<InventoryItemViewModel>();

            var inventoryItems = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList;
            for (int i = 0; i < inventoryItems.Count; ++i) 
            {
                ItemList.Add(new InventoryItemViewModel(inventoryItems[i]));
                maxId = Math.Max(maxId, inventoryItems[i].ID);
                IDToViewModel.Add(inventoryItems[i].ID, ItemList[i]);    
            }
            SQLEvents.created += OnCreated;
            SQLEvents.dataDeleted += OnDeleted;
            SQLEvents.dataChanged += OnChange;

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
        }
        private async void OnCreateItem()
        {
            await Navigation.PushAsync(new CreateInventoryItemView());
        }







        TrulyObservableCollection<InventoryItemViewModel> rememberForFilter = null;

        void OnFilter()
        {
            OnCancelFilter();

            rememberForFilter = _ItemList;

            TrulyObservableCollection<InventoryItemViewModel> newList = new TrulyObservableCollection<InventoryItemViewModel>();

            foreach (var itemView in ItemList)
            {
                if (itemView.Name.ToLower().Contains(FilterText.ToLower()))
                    newList.Add(itemView);
            }

            SetProperty(ref _ItemList, newList);

        }
        void OnCancelFilter()
        {
            if (rememberForFilter != null)
                SetProperty(ref _ItemList, rememberForFilter);
            rememberForFilter = null;
        }



        bool nameDescended = false;
        void OnOrderByName()
        {
            nameDescended = !nameDescended;
            order(nameDescended, new CompareByName());
        }
        
        void order(bool ascending, IComparer<InventoryItemViewModel> comparer)
        {
            // just bubble sort because i wanna do it super simply and in place
            // and i am too lazy to do merge sort in place
            bool changed = true;
            while (changed)
            {
                changed = false;
                // one pass
                for (int i = 0; i < ItemList.Count - 1; ++i)
                {
                    if ((ascending && comparer.Compare(ItemList[i], ItemList[i + 1]) < 0) ||
                        (!ascending && comparer.Compare(ItemList[i], ItemList[i + 1]) > 0))
                    {
                        //swap 
                        var temp = ItemList[i];
                        ItemList[i] = ItemList[i + 1];
                        ItemList[i + 1] = temp;
                        if (!changed)
                            changed = true;
                    }
                }
            }
        }

        class CompareByName : IComparer<InventoryItemViewModel>
        {
            public int Compare(InventoryItemViewModel x, InventoryItemViewModel y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }
        






    }
}
