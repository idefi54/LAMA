using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
	/// <summary>
	/// ViewModel for CP roles
	/// </summary>
	public class CPRoleItemViewModel : BaseViewModel
	{
		private string _roleName;
		/// <summary>
		/// Simple display of the role name.
		/// </summary>
		public string RoleName { get { return _roleName; } set { SetProperty(ref _roleName, value); } }

		/// <summary>
		/// Calls the specified action received in the constructor.
		/// </summary>
		public Command RemoveRole { get; }

        private bool _canEditDetails = false;
		/// <summary>
		/// If the user has permission to edit the CPs. This is present here so the page doesn’t have to specify context each time.
		/// </summary>
		public bool CanEditDetails { get => _canEditDetails; set => SetProperty(ref _canEditDetails, value); }

        public CPRoleItemViewModel(string roleName, Action<CPRoleItemViewModel> remove, long cpID = -1)
		{
			CanEditDetails = LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeCP) || cpID == LocalStorage.cp.ID || cpID == -1;
            _roleName = roleName;
			RemoveRole = new Command(() => remove(this));
		}
	}
}
