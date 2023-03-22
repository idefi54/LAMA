using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class ItemSelectionItemViewModel : BaseViewModel
    {
        public InventoryItem Item { get; private set; }

        public string Name => Item.name;
        public string Count => Item.free + "/" + (Item.taken + Item.free);

        public ItemSelectionItemViewModel(InventoryItem item)
        {
            Item = item;
        }
    }
}
