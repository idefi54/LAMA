using System;
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
		private DateTime _startTime;
		private DateTime _endTime;
		private List<string> _personale;
		private List<string> _equipment;
		private string _preparations;
		private string _location;

		public string Id { get { return _id; } set { SetProperty(ref _id , value); } }
		public string Name { get { return _name; } set { SetProperty(ref _name , value); } }
		public string Description { get { return _description; } set { SetProperty(ref _description , value); } }
		public string Type { get { return _type; } set { SetProperty(ref _type , value); } }
		public DateTime StartTime { get { return _startTime; } set { SetProperty(ref _startTime , value); } }
		public DateTime EndTime { get { return _endTime; } set { SetProperty(ref _endTime , value); } }
		public List<string> Personale { get { return _personale; } set { SetProperty(ref _personale , value); } }
		public List<string> Equipment { get { return _equipment; } set { SetProperty(ref _equipment, value); } }
		public string Preparations { get { return _preparations; } set { SetProperty(ref _preparations , value); } }
		public string Location { get { return _location; } set { SetProperty(ref _location, value); } }


		public NewActivityViewModel()
		{
			//SaveCommand = new Command(OnSave, ValidateSave);
			//CancelCommand = new Command(OnCancel);
			//this.PropertyChanged +=
			//	(_, __) => SaveCommand.ChangeCanExecute();
		}

		private bool ValidateSave()
		{
			return !String.IsNullOrWhiteSpace(_name)
				&& !String.IsNullOrWhiteSpace(_description);
		}

		public Command SaveCommand { get; }
		public Command CancelCommand { get; }

		private async void OnCancel()
		{
			// This will pop the current page off the navigation stack
			await Shell.Current.GoToAsync("..");
		}

		//private async void OnSave()
		//{
		//	Activity newActivity = new Activity()
		//	{
		//		Id = Guid.NewGuid().ToString(),
		//		Name = Name,
		//		Description = Description
		//	};

		//	await ActivityDataStore.AddItemAsync(newActivity);

		//	// This will pop the current page off the navigation stack
		//	await Shell.Current.GoToAsync("..");
		//}
	}
}
