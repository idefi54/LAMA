using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
	public class CPRoleItemViewModel : BaseViewModel
	{
		private string _roleName;
		public string RoleName { get { return _roleName; } set { SetProperty(ref _roleName, value); } }

		public Command RemoveRole { get; }

		public CPRoleItemViewModel(string roleName, Action<CPRoleItemViewModel> remove)
		{
			_roleName = roleName;
			RemoveRole = new Command(() => remove(this));
		}
	}
}
