using LAMA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LAMA.ViewModels
{
	public enum ActivitySearchType
	{
		Any = 0,
		Normal = 1,
		Preparation = 2,
	}

	public enum ActivitySearchRegistration
	{
		Any = 0,
		Registered = 1,
		Unregistered = 2,
	}

	public enum ActivitySearchStatus
    {
		Any = 0,
		AwaitingPrerequisites = 1,
		ReadyToLaunch = 2,
		Launched = 3,
		InProgress = 4,
		Completed = 5,
	}

	public static class ActivityFilterExtensions
	{
		public static string ToFriendlyString(this ActivitySearchType searchType)
		{
			switch (searchType)
			{
				case ActivitySearchType.Any:
					return "Jakákoliv";
				case ActivitySearchType.Normal:
					return "Klasická aktivita";
				case ActivitySearchType.Preparation:
					return "Příprava";
			}
			return searchType.ToString();
		}
		public static string ToFriendlyString(this ActivitySearchRegistration searchType)
		{
			switch (searchType)
			{
				case ActivitySearchRegistration.Any:
					return "Jakákoliv";
				case ActivitySearchRegistration.Registered:
					return "Jsem přihlášen";
				case ActivitySearchRegistration.Unregistered:
					return "Nejsem přihlášen";
			}
			return searchType.ToString();
		}
		public static string ToFriendlyString(this ActivitySearchStatus searchStatus)
		{
            switch (searchStatus)
            {
                case ActivitySearchStatus.Any:
					return "Jakákoliv";
                case ActivitySearchStatus.AwaitingPrerequisites:
                    return "Čeká na splnění předpokladů";
                case ActivitySearchStatus.ReadyToLaunch:
                    return "Připravena ke spuštění";
                case ActivitySearchStatus.Launched:
                    return "Spuštěna";
                case ActivitySearchStatus.InProgress:
                    return "Právě běží";
                case ActivitySearchStatus.Completed:
                    return "Dokončena";
            }
            return searchStatus.ToString();
        }
        public static ActivitySearchType ToSearchEnum(this LarpActivity.EventType type)
		{
			switch (type)
			{
				case LarpActivity.EventType.normal:
					return ActivitySearchType.Normal;
				case LarpActivity.EventType.preparation:
					return ActivitySearchType.Preparation;
			}
			return ActivitySearchType.Any;
		}
		public static ActivitySearchStatus ToSearchEnum(this LarpActivity.Status status)
		{
            switch (status)
            {
                case LarpActivity.Status.awaitingPrerequisites:
                    return ActivitySearchStatus.AwaitingPrerequisites;
                case LarpActivity.Status.readyToLaunch:
                    return ActivitySearchStatus.ReadyToLaunch;
                case LarpActivity.Status.launched:
                    return ActivitySearchStatus.Launched;
                case LarpActivity.Status.inProgress:
                    return ActivitySearchStatus.InProgress;
                case LarpActivity.Status.completed:
                    return ActivitySearchStatus.Completed;
            }
            return ActivitySearchStatus.Any;
        }
    }

    public class ActivityFilterViewModel : BaseViewModel
	{
		private bool _isFiltered = false;
		public bool IsFiltered { get { return _isFiltered; } set { SetProperty(ref _isFiltered, value); } }

		#region SearchValues
		
		private string _searchName;
		public string SearchName { get { return _searchName; } set { SetProperty(ref _searchName, value); ApplyFilter(); } }

		private ActivitySearchType _searchType;
		public int SearchTypeIndex
		{
			get { return (int)_searchType; }
			set { SetProperty(ref _searchType, (ActivitySearchType)value); ApplyFilter(); }
		}

		private ActivitySearchRegistration _searchRegistration;
		public int SearchRegistrationIndex
		{
			get { return (int)_searchRegistration; }
			set { SetProperty(ref _searchRegistration, (ActivitySearchRegistration)value); ApplyFilter(); }
		}

		private ActivitySearchStatus _searchStatus;
		public int SearchStatusIndex
        {
			get { return (int)_searchStatus; }
			set { SetProperty(ref _searchStatus, (ActivitySearchStatus)value); ApplyFilter(); }
        }

		#endregion SearchValues

		#region XamarinValues

		private List<string> _searchTypeList;
		public List<string> SearchTypeList { get { return _searchTypeList; } set { SetProperty(ref _searchTypeList, value);} }

		private List<string> _searchRegistrationList;
		public List<string> SearchRegistrationList { get { return _searchRegistrationList; } set { SetProperty(ref _searchRegistrationList, value); } }

		private List<string> _searchStatusList;
		public List<string> SearchStatusList { get { return _searchStatusList; } set { SetProperty(ref _searchStatusList, value);} }

		#endregion XamarinValues


		private Action _applySort;
		private TrulyObservableCollection<ActivityListItemViewModel> _sourceList;
		private TrulyObservableCollection<ActivityListItemViewModel> _filteredList;

		public ActivityFilterViewModel(TrulyObservableCollection<ActivityListItemViewModel> sourceList,
									   TrulyObservableCollection<ActivityListItemViewModel> filteredList,
									   Action applySort)
		{
			_sourceList = sourceList;
			_filteredList = filteredList;
			_applySort = applySort;

			SearchTypeList = new List<string>();
			foreach(ActivitySearchType item in Enum.GetValues(typeof(ActivitySearchType)))
			{
				SearchTypeList.Add(item.ToFriendlyString());
			}
			_searchType = ActivitySearchType.Any;

			SearchRegistrationList = new List<string>();
			foreach(ActivitySearchRegistration item in Enum.GetValues(typeof(ActivitySearchRegistration)))
			{
				SearchRegistrationList.Add(item.ToFriendlyString());
			}
			_searchRegistration = ActivitySearchRegistration.Any;

			SearchStatusList = new List<string>();
			foreach (ActivitySearchStatus item in Enum.GetValues(typeof(ActivitySearchStatus)))
			{
				SearchStatusList.Add(item.ToFriendlyString());
			}
			_searchStatus = ActivitySearchStatus.Any;
		}

		public void ApplyFilter()
		{
			string searchNameLower = String.IsNullOrEmpty(_searchName) ? null : _searchName.ToLower();

			IsFiltered = searchNameLower != null || _searchType != ActivitySearchType.Any || _searchRegistration != ActivitySearchRegistration.Any;

			Func<ActivityListItemViewModel, bool> filterCheck = (activity) =>
			{
				if (_searchType != ActivitySearchType.Any)
				{
					if (_searchType != activity.LarpActivity.eventType.ToSearchEnum())
						return false;
				}

				if (_searchRegistration != ActivitySearchRegistration.Any)
				{
					long id = LocalStorage.cpID;
					bool present = false;
					foreach(var item in activity.LarpActivity.registrationByRole)
					{
						if(item.first == id)
							present = true;
					}
					if ((present && _searchRegistration == ActivitySearchRegistration.Unregistered) ||
						(!present && _searchRegistration == ActivitySearchRegistration.Registered))
						return false;
				}

				if (_searchStatus != ActivitySearchStatus.Any)
				{
					if (_searchStatus != activity.LarpActivity.status.ToSearchEnum())
						return false;
				}

				if(searchNameLower != null)
				{
					if (!activity.LarpActivity.name.ToLower().Contains(searchNameLower))
						return false;
				}

				return true;
			};

			_filteredList.Clear();
			foreach(var item in _sourceList)
			{
				if(filterCheck(item))
					_filteredList.Add(item);
			}

			_applySort();
		}
	}
}
