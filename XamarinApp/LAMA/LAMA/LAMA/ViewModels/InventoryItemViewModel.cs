using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class InventoryItemViewModel : BaseViewModel
    {
        InventoryItem _item;

        public InventoryItem Item { get { return _item; } }

        public string Name { get{ return _item != null ? Item.name : ""; } } 

        public string Detail { get { return _item != null ? Item.description : ""; } }
        public string Borrowed { get { return _item != null ? Item.taken.ToString() : ""; } }
        public string Free { get { return _item != null ? (Item.free - Item.taken).ToString() : ""; } }



        public bool ShowReturnButton { get { return Item.taken > 0; } }
        public bool ShowBorrowButton { get { return Item.free > 0; } }

        /*private bool _showDeleteButton;
        public bool ShowDeleteButton { get { return _showDeleteButton; } set { SetProperty(ref _showDeleteButton, value, nameof(ShowDeleteButton)); } }
        */

        public InventoryItemViewModel(InventoryItem item)
        {
            _item = item;

        }
        
    }
}
