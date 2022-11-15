using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
	public class RoleItemViewModel : BaseViewModel
	{
		private string _name;
		public string Name => _name;

		private int _currentCount;
		private int _maxCount;
		public string Count => $"{_currentCount}/{_maxCount}";

		public RoleItemViewModel(string name, int max, int current)
		{
			_name = name;
			_currentCount = current;
			_maxCount = max;
		}
	}
}
