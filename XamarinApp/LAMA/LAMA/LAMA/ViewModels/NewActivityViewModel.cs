using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using LAMA.Models;
using LAMA.Services;

namespace LAMA.ViewModels
{
	public class NewActivityViewModel : BaseViewModel
	{
		private string _id;
		private string _name;
		private string _description;
		private string _type;
		private int _typeIndex;
		//private DateTime _startTime;
		//private DateTime _endTime;
		private DateTime _duration;
		private DateTime _start;
		private int _day;
		private List<string> _personale;
		private List<string> _equipment;
		private string _preparations;
		private string _location;
		private List<string> _typeList;

		public string Id { get { return _id; } set { SetProperty(ref _id , value); } }
		public string Name { get { return _name; } set { SetProperty(ref _name , value); } }
		public string Description { get { return _description; } set { SetProperty(ref _description , value); } }
		public string Type { get { return _type; } set { SetProperty(ref _type, value); } }
		public int TypeIndex { get { return _typeIndex; } set { SetProperty(ref _typeIndex, value); } }
		//public DateTime StartTime { get { return _startTime; } set { SetProperty(ref _startTime , value); } }
		//public DateTime EndTime { get { return _endTime; } set { SetProperty(ref _endTime , value); } }
		public DateTime Duration { get { return _duration; } set { SetProperty(ref _duration , value); } }
		public DateTime Start { get { return _start; } set { SetProperty(ref _start , value);} }
		public int Day { get { return _day; } set { SetProperty(ref _day , value); } }
		public List<string> Personale { get { return _personale; } set { SetProperty(ref _personale , value); } }
		public List<string> Equipment { get { return _equipment; } set { SetProperty(ref _equipment, value); } }
		public string Preparations { get { return _preparations; } set { SetProperty(ref _preparations , value); } }
		public string Location { get { return _location; } set { SetProperty(ref _location, value); } }

		public List<string> TypeList { get { return _typeList; } set { SetProperty(ref _typeList , value); } }


		//public NewActivityViewModel()
		//{
		//          SaveCommand = new Command(OnSave, ValidateSave);
		//          CancelCommand = new Command(OnCancel);
		//          this.PropertyChanged +=
		//              (_, __) => SaveCommand.ChangeCanExecute();
		//      }

		INavigation _navigation;
		Action<LarpActivity> _createNewActivity;

        public NewActivityViewModel(INavigation navigation, Action<LarpActivity> createNewActivity, LarpActivity activity = null)
        {
			if(activity != null)
            {
				Id = activity.ID.ToString();
				Name = activity.name;
				Description = activity.description;
				Type = activity.eventType.ToString();
				TypeIndex = (int)activity.eventType;
				Duration = new DateTime(2000,1,1,  activity.duration.hours,activity.duration.minutes,0);
				Start = new DateTime(2000, 1, 1, activity.start.hours, activity.start.minutes, 0);
				Day = activity.day;
				Preparations = activity.preparationNeeded;
				
			}

			_navigation = navigation;
			_createNewActivity = createNewActivity;
			SaveCommand = new Xamarin.Forms.Command(OnSave);
			TypeList = new List<string>();
			foreach (LarpActivity.EventType type in Enum.GetValues(typeof(LarpActivity.EventType)))
			{
				TypeList.Add(type.ToString());
			}
			TypeIndex = 0;
			Type = ((LarpActivity.EventType)TypeIndex).ToString();
		}

		private bool ValidateSave()
		{
			return !String.IsNullOrWhiteSpace(_name)
				&& !String.IsNullOrWhiteSpace(_description);
		}

		public Xamarin.Forms.Command SaveCommand { get; }
		public ICommand CancelCommand { get; }

		private async void OnCancel()
		{
			// This will pop the current page off the navigation stack
			await Shell.Current.GoToAsync("..");
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
			(double lon, double lat) = MapHandler.Instance.GetTemporaryPin();
            
			LarpActivity larpActivity = new LarpActivity(10, Name, Description, Preparations,
				(LarpActivity.EventType)Enum.Parse(typeof(LarpActivity.EventType), Type),
				new EventList<int>(),
				new Time(60 * Duration.Hour + Duration.Minute), Day, new Time(60 * Start.Hour + Start.Minute),
				new Pair<double, double>(lon, lat), LarpActivity.Status.readyToLaunch,
				new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());

			_createNewActivity(larpActivity);

			// This will pop the current page off the navigation stack
			await Shell.Current.GoToAsync("..");
        }
    }
}
