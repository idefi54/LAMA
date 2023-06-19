using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
	/// <summary>
	/// ViewModel for inventory item as used in activity creation/detail
	/// </summary>
    public class ItemItemViewModel : BaseViewModel
	{

		public long ItemID { get; private set; }

		private string _name;
		/// <summary>
		/// Name of the InventoryItem.
		/// </summary>
		public string Name => _name.ToString();

		private int _count;
		/// <summary>
		/// Number of the needed amount.
		/// </summary>
		public string Count
		{
			get { return _count.ToString(); }
			set
			{
				if (int.TryParse(value, out int number))
				{
					if (number < 1)
						number = 1;
					if (number > _maxCount)
						number = _maxCount;
					SetProperty(ref _count, number);
				}
				else
				{
					SetProperty(ref _count, _count);
				}
			}
		}

		private int _maxCount;
		/// <summary>
		/// The maximum number of this item in the inventory.
		/// </summary>
		public string MaxCount => "/" + _maxCount.ToString();

		public ItemItemViewModel(long id, string name, int max, int current = 1)
		{
			ItemID = id;
			_name = name;
			_count = current;
			_maxCount = max;
		}

        public ItemItemViewModel(InventoryItem item, int current = 1) : this(item.ID, item.name, item.taken + item.free, current)
        {

        }

		/// <summary>
		/// Returns <see cref="Pair"/> of <see cref="long"/> and <see cref="int"/> of the item for saving purposes.
		/// </summary>
		/// <returns></returns>
		public Pair<long, int> ToPair()
		{
			return new Pair<long, int>(ItemID, _count);
		}
	}
}
