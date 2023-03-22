using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class ItemItemViewModel : BaseViewModel
	{

		public long ItemID { get; private set; }

		private string _name;
		public string Name => _name.ToString();//{ get { return _name.ToString(); } set { SetProperty(ref _name, value); } }

		private int _count;
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

		public Pair<long, int> ToPair()
		{
			return new Pair<long, int>(ItemID, _count);
		}
	}
}
