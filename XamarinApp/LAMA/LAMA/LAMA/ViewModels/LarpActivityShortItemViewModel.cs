using LAMA.Extensions;
using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    /// <summary>
    /// Lightweight version of <see cref="ActivityListItemViewModel"/>
    /// </summary>
    public class LarpActivityShortItemViewModel : BaseViewModel
    {

        LarpActivity _larpActivity;

        public LarpActivity LarpActivity => _larpActivity;

        public string Name => _larpActivity == null ? "" : _larpActivity.name + " " + _larpActivity.eventType.ToShortString();

        public LarpActivityShortItemViewModel(LarpActivity activity)
        {
            _larpActivity = activity;
        }

        internal void UpdateActivity(LarpActivity activity)
        {
            SetProperty(ref _larpActivity, activity);

            OnPropertyChanged(nameof(Name));
        }
    }
}
