using LAMA.Extensions;
using LAMA.Models;
using LAMA.Models.DTO;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    public class DisplayActivityViewModel : BaseViewModel
    {
        private string _activityName;
        public string ActivityName { get { return _activityName; } set { SetProperty(ref _activityName, value); } }


		private string _name;
		private string _description;
		private string _type;
		private string _duration;
		private string _start;
		private string _dayIndex;
		private List<string> _personale;
		private List<string> _equipment;
		private string _preparations;
		private string _location;


		public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
		public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
		public string Type { get { return _type; } set { SetProperty(ref _type, value); } }
		public string Duration { get { return _duration; } set { SetProperty(ref _duration, value); } }
		public string Start { get { return _start; } set { SetProperty(ref _start, value); } }
		public string DayIndex { get { return _dayIndex; } set { SetProperty(ref _dayIndex, value); } }
		public List<string> Personale { get { return _personale; } set { SetProperty(ref _personale, value); } }
		public List<string> Equipment { get { return _equipment; } set { SetProperty(ref _equipment, value); } }
		public string Preparations { get { return _preparations; } set { SetProperty(ref _preparations, value); } }
		public string Location { get { return _location; } set { SetProperty(ref _location, value); } }
		


		public TrulyObservableCollection<LarpActivityShortItemViewModel> Dependencies { get; }





		LarpActivity _activity;


		public Command SignUpCommand { get; }
		public Command EditCommand { get; }


		INavigation Navigation;

		public DisplayActivityViewModel(INavigation navigation, LarpActivity activity)
        {
			Dependencies = new TrulyObservableCollection<LarpActivityShortItemViewModel>();

			Navigation = navigation;

            _activity = activity;
            ActivityName = _activity.name;

			Name = _activity.name;
			Description = _activity.description;
			Type = _activity.eventType.ToString();
			DateTime dt = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(_activity.duration);
			Duration = dt.Hour + "h " + dt.Minute + "m";
			dt = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(_activity.start);
			Start = dt.Hour + ":" + dt.Minute;
			DayIndex = (_activity.day + 1) + ".";
			Preparations = _activity.preparationNeeded;
			Location = _activity.place.ToString();

			foreach(var id in _activity.prerequisiteIDs)
			{
				LarpActivity larpActivity = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(id);
				Dependencies.Add(new LarpActivityShortItemViewModel(larpActivity));
			}



			SignUpCommand = new Xamarin.Forms.Command(OnSignUp);
			EditCommand = new Xamarin.Forms.Command(OnEdit);
		}

		private async void OnEdit()
		{
			await Navigation.PushAsync(new NewActivityPage(UpdateActivity,_activity));
		}

		private async void OnSignUp()
		{
			await Shell.Current.GoToAsync("..");
		}

		private void UpdateActivity(LarpActivityDTO larpActivityDTO)
		{
			_activity.UpdateWhole(
				larpActivityDTO.name,
				larpActivityDTO.description,
				larpActivityDTO.preparationNeeded,
				larpActivityDTO.eventType,
				larpActivityDTO.duration,
				larpActivityDTO.day,
				larpActivityDTO.start,
				larpActivityDTO.place,
				larpActivityDTO.status);

			Name = larpActivityDTO.name;
			Description = larpActivityDTO.description;

			Dependencies.Clear();
			_activity.UpdatePrerequisiteIDs(larpActivityDTO.prerequisiteIDs);
			foreach (int id in _activity.prerequisiteIDs)
			{
				LarpActivity la = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(id);
				Dependencies.Add(new LarpActivityShortItemViewModel(la));
			}

			DateTime dt = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(larpActivityDTO.duration);
			Duration = dt.Hour + "h " + dt.Minute + "m";
			dt = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(larpActivityDTO.start);
			Start = dt.Hour + ":" + dt.Minute;
			DayIndex = (larpActivityDTO.day + 1) + ".";
			Preparations = larpActivityDTO.preparationNeeded;
			Location = larpActivityDTO.place.ToString();
		}
	}
}
