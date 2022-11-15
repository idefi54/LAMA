using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
	public class RoleItemViewModel : BaseViewModel
	{
		private string _name;
		public string Name { get { return _name.ToString(); } set { SetProperty(ref _name, value); } }

		private int _currentCount;
		private int _maxCount;
		public string Count => $"{_currentCount}/{_maxCount}";
		public string MaxCount {
			get { return _maxCount.ToString(); }
			set 
			{
				if(int.TryParse(value, out int number))
				{
					SetProperty(ref _maxCount, number);
				}
			}
		}

		public RoleItemViewModel(string name, int max, int current)
		{
			_name = name;
			_currentCount = current;
			_maxCount = max;
		}

		public Pair<string,int> ToPair()
		{
			return new Pair<string, int>(_name, _maxCount);
		}
	}
}
