using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
	/// <summary>
	/// ViewModel for activity roles
	/// </summary>
	public class RoleItemViewModel : BaseViewModel
	{
		private string _name;
		/// <summary>
		/// Name of the role.
		/// </summary>
		public string Name { get { return _name.ToString(); } set { SetProperty(ref _name, value); } }

		private int _currentCount;
		/// <summary>
		/// Current number of signed up people.
		/// </summary>
		public int CurrentCount { get { return _currentCount; } set { SetProperty(ref _currentCount, value); OnPropertyChanged(Count); } }
		/// <summary>
		/// Formatted display of both current number of signed up people and the maximum possible.
		/// </summary>
		public string Count => $"{_currentCount}/{_maxCount}";

		private int _maxCount;
		/// <summary>
		/// The max number of people needed for this role.
		/// </summary>
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
		/// <summary>
		/// If the role is editable. Is set to true only if adding new roles to prevent editing the already existing where CPs might already be subscribed to them.
		/// </summary>
		public bool Editable { get { return _editable; } set { SetProperty(ref _editable, value); } }

		public RoleItemViewModel(string name, int max, int current, bool isEditable)
		{
			_name = name;
			_currentCount = current;
			_maxCount = max;
			Editable = isEditable;
		}

		/// <summary>
		/// Get saveable format of role
		/// </summary>
		/// <returns></returns>
		public Pair<string,int> ToPair()
		{
			return new Pair<string, int>(_name, _maxCount);
		}
	}
}
