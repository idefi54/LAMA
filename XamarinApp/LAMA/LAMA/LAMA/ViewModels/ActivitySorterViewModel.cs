using LAMA.Models;
using LAMA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;
using System.Linq;

namespace LAMA.ViewModels
{
    /// <summary>
    /// Part of <see cref="ActivityListViewModel"/> that takes care of sorting of ListView items.
    /// </summary>
    public class ActivitySorterViewModel : BaseViewModel
    {
        private const string NAME_STRING = "Jméno";
        private const string START_STRING = "Začátek";
        private const string PEOPLE_STRING = "Počet CP";
        private const string FREE_SPOTS_STRING = "Volné role";

        private string _sortNameString;
        public string SortNameString { get { return _sortNameString; } set { SetProperty(ref _sortNameString, value); } }

        private string _sortStartString;
        public string SortStartString { get { return _sortStartString; } set { SetProperty(ref _sortStartString, value); } }

        private string _sortPeopleString;
        public string SortPeopleString { get { return _sortPeopleString; } set { SetProperty(ref _sortPeopleString, value); } }

        private string _sortFreeSpotsString;
        public string SortFreeSpotsString { get { return _sortFreeSpotsString; } set { SetProperty(ref _sortFreeSpotsString, value); } }

        /// <summary>
        /// Adds a new comparer for names to the CompositeComparer.
        /// </summary>
        public Command SortNameCommand { get; }
        /// <summary>
        /// Adds a new comparer for starting time to the CompositeComparer.
        /// </summary>
        public Command SortStartCommand { get; }
        /// <summary>
        /// Adds a new comparer for the number of signed up people to the CompositeComparer.
        /// </summary>
        public Command SortPeopleCommand { get; }
        /// <summary>
        /// Adds a new comparer for the number of free spots still available in the activity to the CompositeComparer.
        /// </summary>
        public Command SortFreeSpotsCommand { get; }

        private TrulyObservableCollection<ActivityListItemViewModel> _activityList;

        private CompositeComparer<ActivityListItemViewModel> _comparator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityList">Collection whose content will be sorted.</param>
        public ActivitySorterViewModel(TrulyObservableCollection<ActivityListItemViewModel> activityList)
        {
            _activityList = activityList;

            SortNameString = NAME_STRING;
            SortStartString = START_STRING;
            SortPeopleString = PEOPLE_STRING;
            SortFreeSpotsString = FREE_SPOTS_STRING;

            SortNameCommand = new Command(OnSortName);
            SortStartCommand = new Command(OnSortStart);
            SortPeopleCommand = new Command(OnSortPeople);
            SortFreeSpotsCommand = new Command(OnSortFreeSpots);

            _comparator = new CompositeComparer<ActivityListItemViewModel>();
        }

        public void ApplySort()
        {
            SortHelper.BubbleSort(_activityList, _comparator);
        }

        bool _nameAscending;
        private void OnSortName()
        {
            _nameAscending = !_nameAscending;
            SortNameString = DrawArrow(_nameAscending) + NAME_STRING;
            _comparator.AddComparer(new ActivityNameComparator(_nameAscending));
            ApplySort();
        }

        bool _startAscending;
        private void OnSortStart()
        {
            _startAscending = !_startAscending;
            SortStartString = DrawArrow(_startAscending) + START_STRING;
            _comparator.AddComparer(new ActivityStartComparator(_startAscending));
            ApplySort();
        }

        bool _peopleAscending;
        private void OnSortPeople()
        {
            _peopleAscending = !_peopleAscending;
            SortPeopleString = DrawArrow(_peopleAscending) + PEOPLE_STRING;
            _comparator.AddComparer(new ActivityPeopleComparer(_peopleAscending));
            ApplySort();
        }

        bool _freeSpotsAscending;
        private void OnSortFreeSpots()
        {
            _freeSpotsAscending = !_freeSpotsAscending;
            SortFreeSpotsString = DrawArrow(_freeSpotsAscending) + FREE_SPOTS_STRING;
            _comparator.AddComparer(new ActivityFreeSpotComparer(_freeSpotsAscending));
            ApplySort();
        }

        private string DrawArrow(bool up)
        {
            return up ? "/\\ " : "\\/ ";
        }

        class ActivityNameComparator : IComparer<ActivityListItemViewModel>
        {
            int _ascendingModifier;

            public ActivityNameComparator(bool ascending)
            {
                _ascendingModifier = ascending ? 1 : -1;
            }            

            public int Compare(ActivityListItemViewModel x, ActivityListItemViewModel y)
            {
                return x.LarpActivity.name.CompareTo(y.LarpActivity.name) * _ascendingModifier;
            }
        }

        class ActivityStartComparator : IComparer<ActivityListItemViewModel>
        {
            int _ascendingModifier;

            public ActivityStartComparator(bool ascending)
            {
                _ascendingModifier = ascending ? 1 : -1;
            }

            public int Compare(ActivityListItemViewModel x, ActivityListItemViewModel y)
            {
                return x.LarpActivity.start.CompareTo(y.LarpActivity.start) * _ascendingModifier;
            }
        }

        class ActivityPeopleComparer : IComparer<ActivityListItemViewModel>
        {
            int _ascendingModifier;

            public ActivityPeopleComparer(bool ascending)
            {
                _ascendingModifier= ascending ? 1 : -1;
            }

            public int Compare(ActivityListItemViewModel x, ActivityListItemViewModel y)
            {
                return _ascendingModifier *
                    x.LarpActivity.registrationByRole.Count.CompareTo(y.LarpActivity.registrationByRole.Count);
            }
        }

        class ActivityFreeSpotComparer : IComparer<ActivityListItemViewModel>
        {
            int _ascendingModifier;

            public ActivityFreeSpotComparer(bool ascending)
            {
                _ascendingModifier = ascending? 1 : -1;
            }

            public int Compare(ActivityListItemViewModel x, ActivityListItemViewModel y)
            {
                int xfree = x.LarpActivity.roles.Sum(z => z.second)
                    - x.LarpActivity.registrationByRole.Count;
                int yfree = y.LarpActivity.roles.Sum(z => z.second)
                    - y.LarpActivity.registrationByRole.Count;
                return _ascendingModifier * xfree.CompareTo(yfree);
            }
        }
    }
}
