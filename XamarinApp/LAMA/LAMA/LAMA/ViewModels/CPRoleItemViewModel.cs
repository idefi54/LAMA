using LAMA.Models;
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

        private bool _canEditDetails = false;
        public bool CanEditDetails { get => _canEditDetails; set => SetProperty(ref _canEditDetails, value); }

        public CPRoleItemViewModel(string roleName, Action<CPRoleItemViewModel> remove, long cpID = -1)
		{
			CanEditDetails = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) || cpID == LocalStorage.cp.ID || cpID == -1;
            _roleName = roleName;
			RemoveRole = new Command(() => remove(this));
		}
	}
}
