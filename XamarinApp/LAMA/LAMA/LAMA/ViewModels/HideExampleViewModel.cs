using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
	public class HideExampleViewModel : BaseViewModel
	{

		public Command TestHideCommand { get; }
		private bool _testHideBool = false;
		public bool TestHideBool { get { return _testHideBool; } set { SetProperty(ref _testHideBool, value); } }

		private void OnTestHide(object obj)
		{
			TestHideBool = !TestHideBool;
		}

		public HideExampleViewModel()
		{
			TestHideCommand = new Command(OnTestHide);
		}
	}
}
