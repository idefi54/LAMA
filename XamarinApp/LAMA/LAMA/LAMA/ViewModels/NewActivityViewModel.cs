using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using LAMA.Models;
using LAMA.Services;
using LAMA.Models.DTO;
using LAMA.Extensions;
using LAMA.Views;
using static LAMA.Models.LarpActivity;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Globalization;
using LAMA.Singletons;

namespace LAMA.ViewModels
{
	public class NewActivityViewModel : BaseViewModel
	{
		private string _id;
		private string _name;
		private string _description;
		private string _type;
		private int _typeIndex;

		private ObservableCollection<RoleItemViewModel> _roles;
		private ObservableCollection<ItemItemViewModel> _items;
		private List<string> _equipment;
		private string _preparations;
		private string _location;
		private List<string> _typeList;

		public string Id { get { return _id; } set { SetProperty(ref _id, value); } }
		public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
		public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
		public string Type { get { return _type; } set { SetProperty(ref _type, value); } }
		public int TypeIndex { get { return _typeIndex; } set { SetProperty(ref _typeIndex, value); } }


		public ObservableCollection<RoleItemViewModel> Roles { get { return _roles; } set { SetProperty(ref _roles, value); } }
		public ObservableCollection<ItemItemViewModel> Items { get { return _items; } set { SetProperty(ref _items, value); } }
		public List<string> Equipment { get { return _equipment; } set { SetProperty(ref _equipment, value); } }
		public string Preparations { get { return _preparations; } set { SetProperty(ref _preparations, value); } }
		public string Location { get { return _location; } set { SetProperty(ref _location, value); } }

		public List<string> TypeList { get { return _typeList; } set { SetProperty(ref _typeList, value); } }

		public TrulyObservableCollection<LarpActivityShortItemViewModel> Dependencies { get; }

		#region Time

		private TimeSpan _startTime;
		public TimeSpan StartTime { get { return _startTime; } set { SetProperty(ref _startTime, value); } }

		private TimeSpan _endTime;
		public TimeSpan EndTime { get { return _endTime; } set { SetProperty(ref _endTime, value); } }

		private DateTime _startDate;
		public DateTime StartDate
		{
			get
			{
				return _startDate;
			}
			set
			{
				SetProperty(ref _startDate, value);
				StartDateString = StartDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
			}
		}

		private DateTime _endDate;
		public DateTime EndDate
		{
			get
			{
				return _endDate;
			}
			set
			{
				SetProperty(ref _endDate, value);
				EndDateString = EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
			}
		}

		private string _startDateString;
		public string StartDateString { get { return _startDateString; } set { SetProperty(ref _startDateString, value); } }
		private string _endDateString;
		public string EndDateString { get { return _endDateString; } set { SetProperty(ref _endDateString, value);} }


		public ObservableCollection<int> TimeStringHourOptions { get; }
		public ObservableCollection<int> TimeStringMinuteOptions { get; }


		private int _startTimeStringHourSelected;
		public int StartTimeStringHourSelected
		{ 
			get { return _startTimeStringHourSelected; }
			set 
			{
				StartTime = new TimeSpan(value, StartTime.Minutes,0);
				SetProperty(ref _startTimeStringHourSelected, value);
			}
		}

		private int _startTimeStringMinuteSelected;
		public int StartTimeStringMinuteSelected
		{
			get { return _startTimeStringMinuteSelected; }
			set
			{
				StartTime = new TimeSpan(StartTime.Hours, value, 0);
				SetProperty(ref _startTimeStringMinuteSelected, value); 
			}
		}


		private int _endTimeStringHourSelected;
		public int EndTimeStringHourSelected
		{
			get { return _endTimeStringHourSelected; }
			set
			{
				EndTime = new TimeSpan(value, EndTime.Minutes, 0);
				SetProperty(ref _endTimeStringHourSelected, value);
			}
		}

		private int _endTimeStringMinuteSelected;
		public int EndTimeStringMinuteSelected
		{
			get { return _endTimeStringMinuteSelected; }
			set
			{
				EndTime = new TimeSpan(EndTime.Hours, value, 0);
				SetProperty(ref _endTimeStringMinuteSelected, value);
			}
		}

		#endregion Time


		INavigation _navigation;
		Action<LarpActivityDTO> _createNewActivity;

		private LarpActivity larpActivity;

		private IMessageService _messageService;

		public Command SetStartTimeDateCommand { get; }
		public Command SetEndTimeDateCommand { get; }

		public Xamarin.Forms.Command SaveCommand { get; }
		public ICommand CancelCommand { get; }
		public Command AddDependencyCommand { get; }
		public Command<LarpActivityShortItemViewModel> RemoveDependencyCommand { get; }

		public Command AddNewRole { get; }
		public Command<RoleItemViewModel> RemoveRole { get; }

		public Command AddNewItem { get; }
		public Command<ItemItemViewModel> RemoveItem { get; }

		public NewActivityViewModel(INavigation navigation, Action<LarpActivityDTO> createNewActivity, LarpActivity activity = null)
		{
			_messageService = DependencyService.Get<IMessageService>();

			Dependencies = new TrulyObservableCollection<LarpActivityShortItemViewModel>();
			larpActivity = activity;
			MapHandler.Instance.SetSelectionPin(0, 0);

			Roles = new ObservableCollection<RoleItemViewModel>();
			Items = new ObservableCollection<ItemItemViewModel>();

			TimeStringHourOptions = new ObservableCollection<int>();
			TimeStringMinuteOptions = new ObservableCollection<int>();

			for (int i = 0; i < 24; i++)
			{
				TimeStringHourOptions.Add(i);
			}
			for (int i = 0; i < 60; i += 5)
			{
				TimeStringMinuteOptions.Add(i);
			}

			if (larpActivity != null)
            {
				Title = "Upravit Aktivitu";
				Id = larpActivity.ID.ToString();
				Name = larpActivity.name;
				Description = larpActivity.description;
				Type = larpActivity.eventType.ToString();
				TypeIndex = (int)larpActivity.eventType;

				StartDate = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(activity.start).ToLocalTime();
				EndDate = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(activity.start + activity.duration).ToLocalTime();

				Preparations = larpActivity.preparationNeeded;
				MapHandler.Instance.SetSelectionPin(larpActivity.place.first, larpActivity.place.second);
				foreach(Pair<string, int> role in activity.roles)
				{
					Roles.Add(new RoleItemViewModel(role.first, role.second, 0, false));
				}
				foreach(Pair<long, int> item in activity.requiredItems)
				{
					InventoryItem invItem = DatabaseHolder<InventoryItem,InventoryItemStorage>.Instance.rememberedList.getByID(item.first);
					Items.Add(new ItemItemViewModel(invItem, item.second));
				}
				foreach(int id in larpActivity.prerequisiteIDs)
				{
					Dependencies.Add(new LarpActivityShortItemViewModel(DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(id)));
				}
			}
			else
			{
				Title = "Nová Aktivita";
				Name = "";
				Description = "";
				Type = EventType.normal.ToString();
				TypeIndex = (int)EventType.normal;

				DateTime now = DateTime.Now;
				now = now.AddSeconds(-now.Second);

				StartDate = now.AddMinutes(30);
				EndDate = StartDate.AddHours(1);

				Preparations = "";
			}

			StartTime = new TimeSpan(StartDate.Hour, StartDate.Minute, 0);
			EndTime = new TimeSpan(EndDate.Hour, EndDate.Minute, 0);

			StartTimeStringHourSelected = StartTime.Hours;
			StartTimeStringMinuteSelected = StartTime.Minutes - StartTime.Minutes%5;

			EndTimeStringHourSelected = EndTime.Hours;
			EndTimeStringMinuteSelected = EndTime.Minutes - EndTime.Minutes%5;

			_navigation = navigation;
			_createNewActivity = createNewActivity;
			SaveCommand = new Command(OnSave);
			TypeList = new List<string>();
			foreach (LarpActivity.EventType type in Enum.GetValues(typeof(LarpActivity.EventType)))
			{
				TypeList.Add(type.ToString());
			}
			TypeIndex = 0;
			Type = ((LarpActivity.EventType)TypeIndex).ToString();

			AddDependencyCommand = new Command(OnAddDependency);
			RemoveDependencyCommand = new Command<LarpActivityShortItemViewModel>(OnRemoveDependency);
			AddNewRole = new Command(OnAddNewRole);
			RemoveRole = new Command<RoleItemViewModel>(OnRemoveRole);
			AddNewItem = new Command(OnAddNewItem);
			RemoveItem = new Command<ItemItemViewModel>(OnRemoveItem);

			SetStartTimeDateCommand = new Command(OnSetStartTimeDate);
			SetEndTimeDateCommand = new Command(OnSetEndTimeDate);
		}

		public async void OnSetStartTimeDate()
		{
			DateTime date = await CalendarPage.ShowCalendarPage(_navigation, StartDate);
			if (date != null)
			{
				StartDate = new DateTime(date.Year, date.Month, date.Day,0,0,0, DateTimeKind.Utc);
			}
		}

		public async void OnSetEndTimeDate()
		{
			DateTime date = await CalendarPage.ShowCalendarPage(_navigation, EndDate);
			if (date != null)
			{
				EndDate = new DateTime(date.Year, date.Month, date.Day,0,0,0, DateTimeKind.Utc);
			}
		}

		private void OnRemoveRole(RoleItemViewModel role)
		{
			Roles.Remove(role);
		}

		private void OnAddNewRole()
		{
			Roles.Add(new RoleItemViewModel("role", 1, 0, true));
		}

		private void OnRemoveItem(ItemItemViewModel item)
		{
			Items.Remove(item);
		}

		private void OnAddNewItem()
		{

			HashSet<long> items = new HashSet<long>();
			foreach (var item in Items)
			{
				items.Add(item.ItemID);
			}

			_navigation.PushAsync(new ItemSelectionPage(
				SaveItem,
				(InventoryItem item) =>
				{
					return !items.Contains(item.ID);
				}
				));
		}

		public void SaveItem(InventoryItem item)
		{
			Items.Add(new ItemItemViewModel(item));
		}

		private bool ValidateSave()
		{
			StringBuilder messageBuilder = new StringBuilder();

			if (String.IsNullOrWhiteSpace(_name)
				|| String.IsNullOrWhiteSpace(_description))
            {
				messageBuilder.AppendLine("Název aktivity a popis aktivity musí být vyplněny.");
            }

			bool duplicates = false;
			foreach(var role in Roles)
			{
				foreach(var role2 in Roles)
				{
					if (role.Name == role2.Name && role != role2)
						duplicates = true;
				}
			}
			if (duplicates)
            {
				messageBuilder.AppendLine("Nemohou být dvě role se stejným názvem.");
            }

			DateTime start = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTime.Hours, StartTime.Minutes, 0);
			DateTime end = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, EndTime.Hours, EndTime.Minutes, 0);

			if(start >= end)
			{
				messageBuilder.AppendLine("Čas začátku musí být před časem konce.");
			}

			string message = messageBuilder.ToString();

			if(message != "")
            {
				_messageService.ShowAlertAsync(message, "Nevalidní aktivita!");
            }
			return message == "";
		}

		public void OnAddDependency()
		{
			HashSet<long> dependencies = new HashSet<long>();
			foreach(var item in Dependencies)
			{
				dependencies.Add(item.LarpActivity.ID);
			}

			_navigation.PushAsync(new ActivitySelectionPage(
				SaveDependency,
				(LarpActivity la) => 
				{
					return la != larpActivity && !dependencies.Contains(la.ID); 
				}
				));
		}

		private void OnRemoveDependency(LarpActivityShortItemViewModel obj)
		{
			if (obj == null)
				return;
			Dependencies.Remove(obj);
		}

		public void SaveDependency(LarpActivity activity)
		{
			Dependencies.Add(new LarpActivityShortItemViewModel(activity));
		}

		private async void OnCancel()
		{
			// This will pop the current page off the navigation stack
			if (Device.RuntimePlatform == Device.WPF)
			{
				await App.Current.MainPage.Navigation.PopAsync();
			}
			else
			{
				await Shell.Current.GoToAsync("..");
			}
		}

        private async void OnSave()
        {
			//Activity newActivity = new Activity()
			//{
			//    Id = Guid.NewGuid().ToString(),
			//    Name = Name,
			//    Description = Description
			//};

			//await ActivityDataStore.AddItemAsync(newActivity);

			if(!ValidateSave())
            {


				return;
            }
			(double lon, double lat) = MapHandler.Instance.GetSelectionPin();
			
			EventList<Pair<string, int>> roles = new EventList<Pair<string, int>>();

			foreach(var role in _roles)
			{
				roles.Add(role.ToPair());
			}
			
			var dependencies = new EventList<long>();
			
			foreach(LarpActivityShortItemViewModel item in Dependencies)
			{
				dependencies.Add(item.LarpActivity.ID);
			}

			EventList<Pair<long, int>> items = new EventList<Pair<long, int>>();

			foreach(var item in _items)
            {
				items.Add(item.ToPair());
            }

			//is not used at saving, no dataloss here
			EventList<Pair<long, string>> registered = new EventList<Pair<long, string>>();

			int tmp_day = 0;

			DateTime finalStart = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTime.Hours, StartTime.Minutes, 0, 0, DateTimeKind.Local);
			DateTime finalEnd = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, EndTime.Hours, EndTime.Minutes, 0, 0, DateTimeKind.Local);


			long start = finalStart.ToUnixTimeMilliseconds();
			long duration = finalEnd.ToUnixTimeMilliseconds() - start;

			LarpActivity larpActivity = new LarpActivity(
				10,
				Name, 
				Description is null ? "" : Description,
				Preparations is null ? "" : Preparations,
				(LarpActivity.EventType)Enum.Parse(typeof(LarpActivity.EventType), Type),
				dependencies,
				duration,
				tmp_day,
				start,
				new Pair<double, double>(lon, lat), 
				LarpActivity.Status.readyToLaunch,
				items, 
				roles, 
				registered);

			// GraphY is not on the page, so needs to be transfered like this.
			if (this.larpActivity != null)
				larpActivity.GraphY = this.larpActivity.GraphY;

			var activityDto = new LarpActivityDTO(larpActivity);
			_createNewActivity(activityDto);

			// This will pop the current page off the navigation stack
			if (Device.RuntimePlatform == Device.WPF)
			{
				await App.Current.MainPage.Navigation.PopAsync();
			}
			else
			{
				await Shell.Current.GoToAsync("..");
			}
        }
    }
}
