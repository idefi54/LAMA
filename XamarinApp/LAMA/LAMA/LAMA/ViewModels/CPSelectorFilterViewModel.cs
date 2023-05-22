using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
	public class CPSelectorFilterViewModel : BaseViewModel
	{

		private string _searchText;
		public string SearchText { get { return _searchText; } set { SetProperty(ref _searchText, value); ApplyFilter(); } }


		private TrulyObservableCollection<CPSelectItemViewModel> _sourceList;
		private TrulyObservableCollection<CPSelectItemViewModel> _filteredList;

		public CPSelectorFilterViewModel(TrulyObservableCollection<CPSelectItemViewModel> sourceList,
									   TrulyObservableCollection<CPSelectItemViewModel> filteredList)
		{
			_sourceList = sourceList;
			_filteredList = filteredList;
		}


		public void ApplyFilter()
		{
			string searchTextLower = String.IsNullOrEmpty(_searchText) ? null : _searchText.ToLower();

			Func<CPSelectItemViewModel, bool> filterCheck = (cpvm) =>
			{
				if (searchTextLower == null)
					return true;

				if (cpvm.cp.name.ToLower().Contains(searchTextLower))
					return true;
				if (cpvm.cp.nick.ToLower().Contains(searchTextLower))
					return true;

				foreach (var role in cpvm.cp.roles)
				{
					if (role.ToLower() == searchTextLower)
						return true;
				}

				return false;
			};

			_filteredList.Clear();
			foreach (var item in _sourceList)
			{
				if (filterCheck(item))
					_filteredList.Add(item);
			}
		}
	}
}
