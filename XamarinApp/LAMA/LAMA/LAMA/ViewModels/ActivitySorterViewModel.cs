using LAMA.Models;
using LAMA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class ActivitySorterViewModel : BaseViewModel
    {
        private string _sortNameString;
        public string SortNameString { get { return _sortNameString; } set { SetProperty(ref _sortNameString, value); } }

        private string _sortStartString;
        public string SortStartString { get { return _sortStartString; } set { SetProperty(ref _sortStartString, value); } }

        private string _sortPeopleString;
        public string SortPeopleString { get { return _sortPeopleString; } set { SetProperty(ref _sortPeopleString, value); } }


        public Command SortNameCommand { get; }
        public Command SortStartCommand { get; }
        public Command SortPeopleCommand { get; }

        private TrulyObservableCollection<ActivityListItemViewModel> _activityList;

        private CompositeComparer<ActivityListItemViewModel> _comparator;

        public ActivitySorterViewModel(TrulyObservableCollection<ActivityListItemViewModel> activityList)
        {
            _activityList = activityList;

            SortNameString = "Název";
            SortStartString = "Začátek";
            SortPeopleString = "Zaplnění";

            SortNameCommand = new Command(OnSortName);
            SortStartCommand = new Command(OnSortStart);
            SortPeopleCommand = new Command(OnSortPeople);

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
            SortNameString = _nameAscending ? "/\\ Název" : "\\/ Název";
            _comparator.AddComparer(new ActivityNameComparator(_nameAscending));
            ApplySort();
        }

        private void OnSortStart()
        {

        }

        private void OnSortPeople()
        {

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
    }
}
