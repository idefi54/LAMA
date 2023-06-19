using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
	/// <summary>
	/// ViewModel for <see cref="Views.CPSelectionPage"/>
	/// </summary>
	public class CPSelectionViewModel : BaseViewModel
	{
		private CPSelectItemViewModel _selecedCP;
		/// <summary>
		/// The current selected CPSelectItemViewModel in the ListView. In the setter the callback function is run and the page is exited.
		/// </summary>
		public CPSelectItemViewModel SelectedCP
		{
			get
			{
				return _selecedCP;
			}
			set
			{
				_selecedCP = value;
				if (_selecedCP != null && _selecedCP.cp != null)
				{
					_callback(_selecedCP.cp);
					Close();
				}
			}
		}


		private TrulyObservableCollection<CPSelectItemViewModel> sourceCPList;
		/// <summary>
		/// Collection of filtered CPSelectItemViewModels. It is a subset of internal sourceCPList that contains all the CPs.
		/// </summary>
		public TrulyObservableCollection<CPSelectItemViewModel> FilteredCPList { get; }

		private Action<CP> _callback;

		/// <summary>
		/// CPSelectorFilterViewModel that takes care of searching for specific CPs.
		/// </summary>
		public CPSelectorFilterViewModel FilterViewModel { get; }

		public CPSelectionViewModel(Action<CP> callback)
		{
			_callback = callback;

			sourceCPList = new TrulyObservableCollection<CPSelectItemViewModel>();
			FilteredCPList = new TrulyObservableCollection<CPSelectItemViewModel>();

			FilterViewModel = new CPSelectorFilterViewModel(sourceCPList, FilteredCPList);

			for (int i = 0; i < DatabaseHolder<CP, CPStorage>.Instance.rememberedList.Count; i++)
			{
				var item = new CPSelectItemViewModel(DatabaseHolder<CP, CPStorage>.Instance.rememberedList[i]);
				sourceCPList.Add(item);
				FilteredCPList.Add(item);
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
