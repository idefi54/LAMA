using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class DisplayActivityViewModel : BaseViewModel
    {
        private string _activityName;
        public string ActivityName { get { return _activityName; } set { SetProperty(ref _activityName, value); } }

        LarpActivity _activity;

        public DisplayActivityViewModel(LarpActivity activity)
        {
            _activity = activity;
            ActivityName = _activity.name;
        }
    }
}
