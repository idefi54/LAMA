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
		public int CurrentCount { get { return _currentCount; } set { SetProperty(ref _currentCount, value); OnPropertyChanged(Count); } }
		public string Count => $"{_currentCount}/{_maxCount}";

		private int _maxCount;
		public string MaxCount {
			get { return _maxCount.ToString(); }
			set 
			{
				if(int.TryParse(value, out int number))
				{
					SetProperty(ref _maxCount, number);
					OnPropertyChanged(Count);
				}
			}
		}

		private bool _editable;
		public bool Editable { get { return _editable; } set { SetProperty(ref _editable, value); } }

		public RoleItemViewModel(string name, int max, int current, bool isEditable)
		{
			_name = name;
			_currentCount = current;
			_maxCount = max;
			Editable = isEditable;
		}

		public Pair<string,int> ToPair()
		{
			return new Pair<string, int>(_name, _maxCount);
		}
	}
}
