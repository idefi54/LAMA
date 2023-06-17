using LAMA.Models;
using LAMA.Models.DTO;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="ActivityListPage"/>.
    /// </summary>
    public class ActivityListViewModel : BaseViewModel
    {
        /// <summary>
        /// Displays and hides flyout for the sorting settings.
        /// </summary>
        public Command SortCommand { get; }
        /// <summary>
        /// Displays and hides flyout for the filtering settings.
        /// </summary>
        public Command FilterCommand { get; }
        /// <summary>
        /// Creates new NewActivityPage and gives it as a callback CreateNewActivity(LarpActivityDTO) method which saves the created activity to the database.
        /// </summary>
        public Command AddActivityCommand { get; }
        /// <summary>
        /// Navigates to the DisplayActivityPage.
        /// </summary>
        public Command<object> LarpActivityTapped { get; private set; }
        /// <summary>
        /// Removes LarpActivity from the database and the list.
        /// </summary>
        public Command<object> RemoveLarpActivity { get; private set; }


        /// <summary>
        /// TrulyObservableCollection of ActivityListItemViewModel containing all the existing Larp Activities.
        /// Currently it is not directly displayed in favor of the FilteredLarpActivityListItems collection.
        /// It is automatically updated to the current state of the database using PropagateCreated, PropagateChanged
        /// and PropagateDeleted methods.
        /// </summary>
        public TrulyObservableCollection<ActivityListItemViewModel> LarpActivityListItems { get; }
        /// <summary>
        /// TrulyObservableCollection of ActivityListItemViewModel containing filtered subset of activities determined
        /// by ActivityFilter and sorted using ActivitySorter. This list is shown in the ActivityListPage.
        /// </summary>
        public TrulyObservableCollection<ActivityListItemViewModel> FilteredLarpActivityListItems { get; }



        ActivityListItemViewModel lastInteracterActivity = null;

        INavigation Navigation;

        private bool _canChangeActivity;
        public bool CanChangeActivity { get => _canChangeActivity; set => SetProperty(ref _canChangeActivity, value); }

        private bool _showSortDropdown;
        public bool ShowSortDropdown { get { return _showSortDropdown; } set { SetProperty(ref _showSortDropdown, value); } }

        private bool _showFilterDropdown;
        public bool ShowFilterDropdown { get { return _showFilterDropdown; } set { SetProperty(ref _showFilterDropdown, value); } }

        /// <summary>
        ///  ActivitySorterViewModel that takes care of sorting of activities. It does so for multiple different parameters at the same time.
        /// </summary>
        public ActivitySorterViewModel ActivitySorter { get; }
        /// <summary>
        /// ActivityFilterViewModel that takes care of filing FilteredLarpActivityListItems with a subset of LarpActivityListItems.
        /// </summary>
        public ActivityFilterViewModel ActivityFilter { get; }

        public ActivityListViewModel(INavigation navigation)
        {
            Navigation = navigation;

            LarpActivityListItems = new TrulyObservableCollection<ActivityListItemViewModel>();
            FilteredLarpActivityListItems = new TrulyObservableCollection<ActivityListItemViewModel>();
            ActivitySorter = new ActivitySorterViewModel(FilteredLarpActivityListItems);
            ActivityFilter = new ActivityFilterViewModel(LarpActivityListItems, FilteredLarpActivityListItems, ActivitySorter.ApplySort);

            for (int i = 0; i < DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.Count; i++)
            {
                var item = new ActivityListItemViewModel(DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList[i]);
                LarpActivityListItems.Add(item);
                FilteredLarpActivityListItems.Add(item);
            }

            AddActivityCommand = new Command(OnAddActivityListItem);
            LarpActivityTapped = new Command<object>(DisplayActivity);
            RemoveLarpActivity = new Command<object>(RemoveActivity);

            SortCommand = new Command(ToggleSort);
            FilterCommand = new Command(ToggleFilter);

            CanChangeActivity = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeActivity);
            SQLEvents.created += PropagateCreated;
            SQLEvents.dataChanged += PropagateChanged;
            SQLEvents.dataDeleted += PropagateDeleted;
        }

        #region Database Propagate Events
        private void PropagateCreated(Serializable created)
        {
            if (created == null || created.GetType() != typeof(LarpActivity))
                return;

            LarpActivity activity = (LarpActivity)created;

            ActivityListItemViewModel item = LarpActivityListItems.Where(x => x.LarpActivity.ID == activity.ID).FirstOrDefault();

            if (item != null)
                return;

            item = new ActivityListItemViewModel(activity);
            LarpActivityListItems.Add(item);

            ActivityFilter.ApplyFilter();
        }

        private void PropagateChanged(Serializable changed, int changedAttributeIndex)
        {
            if (changed is CP cp && cp.ID == LocalStorage.cpID)
                CanChangeActivity = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeActivity);

            if (changed == null || changed.GetType() != typeof(LarpActivity))
                return;

            LarpActivity activity = (LarpActivity)changed;

            ActivityListItemViewModel item = LarpActivityListItems.Where(x => x.LarpActivity.ID == activity.ID).FirstOrDefault();

            // here can be problem if created and changed of same activity come in wrong order (the change is not registered until future update)
            // but creating it if it doesn't exist is bigger problem, because of changed and deleted combination in wrong order
            if (item == null)
                return;

            item.UpdateActivity(activity);
            LarpActivityListItems.RefreshItem(LarpActivityListItems.IndexOf(item)); //this should hopefuly update the single line... but I still advise using INotifyPropertyChange

            ActivityFilter.ApplyFilter();
        }

        private void PropagateDeleted(Serializable deleted)
        {
            if (deleted == null || deleted.GetType() != typeof(LarpActivity))
                return;

            LarpActivity activity = (LarpActivity)deleted;

            ActivityListItemViewModel item = LarpActivityListItems.Where(x => x.LarpActivity.ID == activity.ID).FirstOrDefault();

            if (item != null)
            {
                LarpActivityListItems.Remove(item);
            }

            ActivityFilter.ApplyFilter();
        }

        #endregion

        private void ToggleSort()
		{
            ShowSortDropdown = !ShowSortDropdown;
            ShowFilterDropdown = false;
		}

        private void ToggleFilter()
		{
            ShowFilterDropdown = !ShowFilterDropdown;
            ShowSortDropdown = false;
		}

		private void UpdateLastInteracter(ActivityListItemViewModel alivm)
        {
            if(lastInteracterActivity != null)
            {
                lastInteracterActivity.ResetDisplay();
            }
            lastInteracterActivity = alivm;
        }

        private async void DisplayActivity(object obj)
        {
            if (obj.GetType() != typeof(ActivityListItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ActivityListItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            ActivityListItemViewModel activityViewModel = (ActivityListItemViewModel)obj;
            UpdateLastInteracter(activityViewModel);

            LarpActivity activity = activityViewModel.LarpActivity;

            await Navigation.PushAsync(new DisplayActivityPage(activity));
        }

        private async void RemoveActivity(object obj)
        {
            if (obj.GetType() != typeof(ActivityListItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ActivityListItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            ActivityListItemViewModel activityViewModel = (ActivityListItemViewModel)obj;
            UpdateLastInteracter(activityViewModel);

            bool result = await App.Current.MainPage.DisplayAlert("Smazat aktivitu", "Opravdu chcete smazat aktivitu " + activityViewModel.LarpActivity.name + ".", "Smazat", "zrušit");

            if(result)
            {
                LarpActivityListItems.Remove(activityViewModel);
                RememberedList<LarpActivity, LarpActivityStorage> rememberedList = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList;
                for (int i = 0; i < rememberedList.Count; i++)
				{
                    if (rememberedList[i].prerequisiteIDs.Contains(activityViewModel.LarpActivity.ID))
					{
                        rememberedList[i].prerequisiteIDs.Remove(activityViewModel.LarpActivity.ID);
					}
				}
                DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.removeByID(activityViewModel.LarpActivity.getID());

                ActivityFilter.ApplyFilter();
            }
        }

        private async void OnAddActivityListItem(object obj)
        {
            await Navigation.PushAsync(new ActivityEditPage(CreateNewActivity));
        }

        private void CreateNewActivity(LarpActivityDTO larpActivity)
        {
            larpActivity.ID = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.nextID();
            LarpActivity activity = larpActivity.CreateLarpActivity();

            LarpActivityListItems.Add(new ActivityListItemViewModel(activity));
            DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.add(activity);

            ActivityFilter.ApplyFilter();
        }
    }
}
