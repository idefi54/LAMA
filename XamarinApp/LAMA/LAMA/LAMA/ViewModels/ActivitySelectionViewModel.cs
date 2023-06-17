using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    /// <summary>
    /// ViewModel for <see cref="LAMA.Views.ActivitySelectionPage"/>
    /// </summary>
    public class ActivitySelectionViewModel
    {
        /// <summary>
        /// Exposed selected activity.
        /// </summary>
        public LarpActivity Activity { get; private set; }

        private Action<LarpActivity> _callback;
        /// <summary>
        ///  List of activities displayed in ListView. It is populated in the constructor by activities that can pass the filter.
        /// </summary>
        public ObservableCollection<ActivityListItemViewModel> LarpActivityListItems { get; }

        private ActivityListItemViewModel _selectedItem;
        /// <summary>
        /// When selecting one of the activities this saves the selection and closes the page.
        /// </summary>
        public ActivityListItemViewModel SelectedItem { get { return _selectedItem; } set
            {
                if (value == null)
                    return;
                _selectedItem = value;
                Activity = _selectedItem.LarpActivity;
                _callback(Activity);
                Close();
            }
        }


        public ActivitySelectionViewModel(Action<LarpActivity> callback, Func<LarpActivity, bool> filter)
		{
            _callback = callback;
            Activity = null;
            LarpActivityListItems = new ObservableCollection<ActivityListItemViewModel>();

            for (int i = 0; i < DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.Count; i++)
            {
                LarpActivity la = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList[i];
                if(filter(la))
                    LarpActivityListItems.Add(new ActivityListItemViewModel(la));
            }
		}

		private async void Close()
        {
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
