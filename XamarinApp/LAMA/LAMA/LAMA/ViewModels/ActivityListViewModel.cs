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
    public class ActivityListViewModel : BaseViewModel
    {

        public Command TestChangeValues { get; }
        public Command TestRefresh { get; }
        public Command SortCommand { get; }
        public Command FilterCommand { get; }
        public Command AddActivityCommand { get; }
        public TrulyObservableCollection<ActivityListItemViewModel> LarpActivityListItems { get; }
        public TrulyObservableCollection<ActivityListItemViewModel> FilteredLarpActivityListItems { get; }

        public Command<object> LarpActivityTapped { get; private set; }

        public Command<object> RemoveLarpActivity { get; private set; }

        public Command<object> ShowRemoveButton { get; private set; }

        ActivityListItemViewModel lastInteracterActivity = null;

        INavigation Navigation;

        private bool _canChangeActivity;
        public bool CanChangeActivity { get => _canChangeActivity; set => SetProperty(ref _canChangeActivity, value); }

        private bool _showSortDropdown;
        public bool ShowSortDropdown { get { return _showSortDropdown; } set { SetProperty(ref _showSortDropdown, value); } }

        private bool _showFilterDropdown;
        public bool ShowFilterDropdown { get { return _showFilterDropdown; } set { SetProperty(ref _showFilterDropdown, value); } }

        //int maxId = 0;

        public ActivitySorterViewModel ActivitySorter { get; }
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

            //foreach(ActivityListItemViewModel item in LarpActivityListItems)
            //{
            //    if(item.LarpActivity.ID > maxId)
            //        maxId = item.LarpActivity.ID;
            //}

            #region population of test database
            //LarpActivity activity = new LarpActivity() { name = "Příprava přepadu", start = new Time(60 * 12 + 45), eventType = LarpActivity.EventType.preparation };

            //LarpActivity larpActivity = new LarpActivity(0, "Příprava přepadu", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 12 + 45), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //larpActivity = new LarpActivity(1, "Přepad karavanu", "", "", LarpActivity.EventType.normal, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 14 + 15), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //larpActivity = new LarpActivity(2, "Příprava záchrany", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 14), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //larpActivity = new LarpActivity(3, "Záchrana kupce", "", "", LarpActivity.EventType.normal, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 16 + 10), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //larpActivity = new LarpActivity(4, "Úklid mrtvol hráčů", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(60 + 30), 0, new Time(60 * 16 + 15), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //DatabaseHolder<LarpActivity>.Instance.rememberedList.add(larpActivity);
            //LarpActivityListItems.Add(new ActivityListItemViewModel(larpActivity));

            //LarpActivityListItems.Add(new LarpActivity() { name = "Příprava přepadu", start = new Time(60 * 12 + 45), eventType = LarpActivity.EventType.preparation });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Přepad karavanu", start = new Time(60 * 14 + 15), eventType = LarpActivity.EventType.normal });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Příprava záchrany", start = new Time(60 * 14), eventType = LarpActivity.EventType.preparation });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Záchrana kupce", start = new Time(60 * 16 + 10), eventType = LarpActivity.EventType.normal });
            //LarpActivityListItems.Add(new LarpActivity() { name = "Úklid mrtvol hráčů", start = new Time(60 * 16 + 15), eventType = LarpActivity.EventType.preparation });

            #endregion

            AddActivityCommand = new Command(OnAddActivityListItem);
            LarpActivityTapped = new Command<object>(DisplayActivity);
            RemoveLarpActivity = new Command<object>(RemoveActivity);
            ShowRemoveButton = new Command<object>(DisplayRemoveButton);

            TestChangeValues = new Command(TestChangeLastActivityValues);
            TestRefresh = new Command(RefreshCollection);

            SortCommand = new Command(ToggleSort);
            FilterCommand = new Command(ToggleFilter);


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

        private void TestChangeLastActivityValues(object obj)
		{
            if(LarpActivityListItems.Count > 0)
                LarpActivityListItems[0].SetName(LarpActivityListItems[0].LarpActivity.name + "x");
                //LarpActivityListItems[0].LarpActivity.name = LarpActivityListItems[0].LarpActivity.name + "x";
        }

		private void RefreshCollection(object obj)
		{
            //LarpActivityListItems.Refresh();
            
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

        private async void DisplayRemoveButton(object obj)
        {

            if (obj.GetType() != typeof(ActivityListItemViewModel))
            {
                await App.Current.MainPage.DisplayAlert("Message", "Object is of wrong type.\nExpected: " + typeof(ActivityListItemViewModel).Name
                    + "\nActual: " + obj.GetType().Name, "OK");
                return;
            }

            ActivityListItemViewModel activityViewModel = (ActivityListItemViewModel)obj;
            UpdateLastInteracter(activityViewModel);

            await App.Current.MainPage.DisplayAlert("Message", activityViewModel.ShowDeleteButton.ToString(), "OK");
            activityViewModel.ShowDeleteButton = true;
            await App.Current.MainPage.DisplayAlert("Message", activityViewModel.ShowDeleteButton.ToString(), "OK");
            
        }

        private async void OnAddActivityListItem(object obj)
        {
            //await Shell.Current.GoToAsync(nameof(NewActivityPage));

            await Navigation.PushAsync(new NewActivityPage(CreateNewActivity));
            
            //LarpActivity larpActivity = new LarpActivity(0, "New Activity", "", "", LarpActivity.EventType.preparation, new EventList<int>(),
            //    new Time(0), 0, new Time(0), new Pair<double, double>(0, 0), LarpActivity.Status.launched, new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());
            //LarpActivityListItems.Add(larpActivity);
        }

        private void CreateNewActivity(LarpActivityDTO larpActivity)
        {
            //larpActivity.ID = ++maxId;

            //LarpActivity activity = new LarpActivity(++maxId, larpActivity.name, larpActivity.description, larpActivity.preparationNeeded, larpActivity.eventType,
            //    larpActivity.prerequisiteIDs, larpActivity.duration, larpActivity.day, larpActivity.start, larpActivity.place, larpActivity.status,
            //    larpActivity.requiredItems, larpActivity.roles, larpActivity.registrationByRole);

            larpActivity.ID = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.nextID();
            LarpActivity activity = larpActivity.CreateLarpActivity();

            LarpActivityListItems.Add(new ActivityListItemViewModel(activity));
            DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.add(activity);

            ActivityFilter.ApplyFilter();
        }
    }
}
