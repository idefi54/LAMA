﻿using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
		private List<string> _typeList;


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
		





		LarpActivity _activity;

        public DisplayActivityViewModel(LarpActivity activity)
        {
            _activity = activity;
            ActivityName = _activity.name;

			Name = _activity.name;
			Description = _activity.description;
			Type = _activity.eventType.ToString();
			Duration = _activity.duration.hours + "h " + _activity.duration.minutes + "m";
			Start = _activity.start.hours + ":" + _activity.start.minutes;
			DayIndex = _activity.day + "#";
			Preparations = _activity.preparationNeeded;
			Location = _activity.place.ToString();
        }
    }
}
