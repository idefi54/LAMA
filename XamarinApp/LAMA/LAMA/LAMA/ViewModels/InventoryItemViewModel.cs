using LAMA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LAMA.ViewModels
{
    public class InventoryItemViewModel : BaseViewModel, INotifyPropertyChanged
    {
        InventoryItem _item;

        public InventoryItem Item { get { return _item; } }

        public string Name { get{ return _item != null ? Item.name : ""; } } 

        public string Detail { get { return _item != null ? Item.description : ""; } }
        public string Borrowed { get { return _item != null ? Item.taken.ToString() : ""; } }
        public string Free { get { return _item != null ? Item.free.ToString() : ""; } }

        private bool _canChangeItems;
        public bool CanChangeItems { get => _canChangeItems; set => SetProperty(ref _canChangeItems, value); }


        public bool ShowReturnButton { get { return Item.taken > 0; } }
        public bool ShowBorrowButton { get { return Item.free > 0; } }

        /*private bool _showDeleteButton;
        public bool ShowDeleteButton { get { return _showDeleteButton; } set { SetProperty(ref _showDeleteButton, value, nameof(ShowDeleteButton)); } }
        */

        public InventoryItemViewModel(InventoryItem item)
        {
            _item = item;
            item.IGotUpdated += onChange;
            CanChangeItems = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageInventory);
            SQLEvents.dataChanged += (Serializable changed, int changedAttributeIndex) =>
            {
                if (changed is CP cp && cp.ID == LocalStorage.cpID && changedAttributeIndex == 9)
                    CanChangeItems = LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageInventory);
            };

        }
        public event PropertyChangedEventHandler PropertyChanged;
        void onChange(object sender, int index)
        {
            string propName = "";

            switch(index)
            {
                case 1: 
                    propName = nameof(Name);
                    break;
                case 2:
                    propName = nameof(Detail);
                    break;
                case 3:
                    propName = nameof(Borrowed);
                    break;
                case 4: propName = nameof(Free);
                    break;
                default:
                    return;
            }
            
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        
    }
}
