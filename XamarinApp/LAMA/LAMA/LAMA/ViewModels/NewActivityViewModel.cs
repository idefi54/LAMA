﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using LAMA.Models;

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


		//public NewActivityViewModel()
		//{
		//          SaveCommand = new Command(OnSave, ValidateSave);
		//          CancelCommand = new Command(OnCancel);
		//          this.PropertyChanged +=
		//              (_, __) => SaveCommand.ChangeCanExecute();
		//      }

		INavigation _navigation;
		Action<LarpActivity> _createNewActivity;

        public NewActivityViewModel(INavigation navigation, Action<LarpActivity> createNewActivity)
        {
			_navigation = navigation;
			_createNewActivity = createNewActivity;
			SaveCommand = new Xamarin.Forms.Command(OnSave);
        }

		private bool ValidateSave()
		{
			return true;
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


			LarpActivity larpActivity = new LarpActivity(10, Name, Description, Preparations,
				LarpActivity.EventType.preparation, new EventList<int>(),
				new Time(60 * Duration.Hour + Duration.Minute), Day, new Time(60 * Start.Hour + Start.Minute),
				new Pair<double, double>(0, 0), LarpActivity.Status.readyToLaunch,
				new EventList<Pair<int, int>>(), new EventList<Pair<string, int>>(), new EventList<Pair<int, string>>());

			_createNewActivity(larpActivity);

			// This will pop the current page off the navigation stack
			await Shell.Current.GoToAsync("..");
        }
    }
}
