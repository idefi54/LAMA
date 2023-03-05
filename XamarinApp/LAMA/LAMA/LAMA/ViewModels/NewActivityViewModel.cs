﻿using System;
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

namespace LAMA.ViewModels
{
	public class NewActivityViewModel : BaseViewModel
	{
		private string _id;
		private string _name;
		private string _description;
		private string _type;
		private int _typeIndex;

        private DateTime _startTime;
        private DateTime _endTime;
		private DateTime _startDate;
		private DateTime _endDate;

		private ObservableCollection<RoleItemViewModel> _roles;
		private List<string> _equipment;
		private string _preparations;
		private string _location;
		private List<string> _typeList;

		public string Id { get { return _id; } set { SetProperty(ref _id , value); } }
		public string Name { get { return _name; } set { SetProperty(ref _name , value); } }
		public string Description { get { return _description; } set { SetProperty(ref _description , value); } }
		public string Type { get { return _type; } set { SetProperty(ref _type, value); } }
		public int TypeIndex { get { return _typeIndex; } set { SetProperty(ref _typeIndex, value); } }

        public DateTime StartTime { get { return _startTime; } set { SetProperty(ref _startTime, value); } }
        public DateTime EndTime { get { return _endTime; } set { SetProperty(ref _endTime, value); } }
        public DateTime StartDate { get { return _startDate; } set { SetProperty(ref _startDate, value); } }
        public DateTime EndDate { get { return _endDate; } set { SetProperty(ref _endDate, value); } }

		public ObservableCollection<RoleItemViewModel> Roles { get { return _roles; } set { SetProperty(ref _roles , value); } }
		public List<string> Equipment { get { return _equipment; } set { SetProperty(ref _equipment, value); } }
		public string Preparations { get { return _preparations; } set { SetProperty(ref _preparations , value); } }
		public string Location { get { return _location; } set { SetProperty(ref _location, value); } }

		public List<string> TypeList { get { return _typeList; } set { SetProperty(ref _typeList , value); } }

        public TrulyObservableCollection<LarpActivityShortItemViewModel> Dependencies { get; }


		//public NewActivityViewModel()
		//{
		//          SaveCommand = new Command(OnSave, ValidateSave);
		//          CancelCommand = new Command(OnCancel);
		//          this.PropertyChanged +=
		//              (_, __) => SaveCommand.ChangeCanExecute();
		//      }

		INavigation _navigation;
		Action<LarpActivityDTO> _createNewActivity;

		private LarpActivity larpActivity;

		private IMessageService _messageService;

		public NewActivityViewModel(INavigation navigation, Action<LarpActivityDTO> createNewActivity, LarpActivity activity = null)
		{
			_messageService = DependencyService.Get<IMessageService>();

			Dependencies = new TrulyObservableCollection<LarpActivityShortItemViewModel>();
			larpActivity = activity;
			MapHandler.Instance.SetSelectionPin(0, 0);

			Roles = new ObservableCollection<RoleItemViewModel>();


			if (larpActivity != null)
            {
				Title = "Upravit Aktivitu";
				Id = larpActivity.ID.ToString();
				Name = larpActivity.name;
				Description = larpActivity.description;
				Type = larpActivity.eventType.ToString();
				TypeIndex = (int)larpActivity.eventType;

				StartTime = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(activity.start);
				EndTime = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(activity.start + activity.duration);

				Preparations = larpActivity.preparationNeeded;
				MapHandler.Instance.SetSelectionPin(larpActivity.place.first, larpActivity.place.second);
				foreach(Pair<string, int> role in activity.roles)
				{
					Roles.Add(new RoleItemViewModel(role.first, role.second, 0, false));
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

				StartTime = now.AddMinutes(30);
				EndTime = StartTime.AddHours(1);

				Preparations = "";
			}
			StartDate = StartTime;
			EndDate = EndTime;

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

			SetStartTimeDateCommand = new Command(OnSetStartTimeDate);
			SetEndTimeDateCommand = new Command(OnSetEndTimeDate);
		}

		public Command SetStartTimeDateCommand { get; }
		public Command SetEndTimeDateCommand { get; }

		public async void OnSetStartTimeDate()
		{
			DateTime date = await CalendarPage.ShowCalendarPage(_navigation);
			if (date != null)
			{
				StartDate = new DateTime(date.Year, date.Month, date.Day,0,0,0, DateTimeKind.Utc);
			}
		}

		public async void OnSetEndTimeDate()
		{
			DateTime date = await CalendarPage.ShowCalendarPage(_navigation);
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

			if(StartTime >= EndTime)
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

		public Xamarin.Forms.Command SaveCommand { get; }
		public ICommand CancelCommand { get; }
		public Command AddDependencyCommand { get; }
		public Command<LarpActivityShortItemViewModel> RemoveDependencyCommand { get; }

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

		public Command AddNewRole { get; }
		public Command<RoleItemViewModel> RemoveRole {get; }

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

			EventList<Pair<int, int>> items = new EventList<Pair<int, int>>();
			EventList<Pair<long, string>> registered = new EventList<Pair<long, string>>();

			int tmp_day = 0;

			DateTime finalStart = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTime.Hour, StartTime.Minute, 0, 0, DateTimeKind.Utc);
			DateTime finalEnd = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, EndTime.Hour, EndTime.Minute, 0, 0, DateTimeKind.Utc);


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

			_createNewActivity(new LarpActivityDTO(larpActivity));

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
