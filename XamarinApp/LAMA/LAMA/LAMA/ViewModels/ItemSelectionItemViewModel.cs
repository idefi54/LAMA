using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class ItemSelectionItemViewModel : BaseViewModel
    {
        public InventoryItem Item { get; private set; }

        /// <summary>
        /// Unformated name of the item.
        /// </summary>
        public string Name => Item.name;
        /// <summary>
        /// Formatted number of available items and maximum possible number of items.
        /// </summary>
        public string Count => Item.free + "/" + (Item.taken + Item.free);

        public ItemSelectionItemViewModel(InventoryItem item)
        {
            Item = item;
        }
    }
}
