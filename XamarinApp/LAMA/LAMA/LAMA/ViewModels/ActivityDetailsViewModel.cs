﻿using LAMA.Extensions;
using LAMA.Database;
using LAMA.Models;
using LAMA.Models.DTO;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using LAMA.Services;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms.Internals;
using LAMA.Singletons;
using System.Data;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using LAMA.Themes;

namespace LAMA.ViewModels
{
	/// <summary>
	/// Read only viewmodel for activities
	/// </summary>
    public class ActivityDetailsViewModel : BaseViewModel
    {
		#region registering roles

		public delegate void RoleRequestedDelegate(long activityID, string roleName);
		public static event RoleRequestedDelegate roleRequested;
        public delegate void RoleReceivedDelegate(long activityID, string roleName, bool wasGivenRole);
        public static event RoleReceivedDelegate roleReceived;

        public delegate void RoleRemovedDelegate(long activityID);
        public static event RoleRemovedDelegate roleRemoved;
        public delegate void RoleRemovedResultDelegate(long activityID, bool wasRemoved);
        public static event RoleRemovedResultDelegate roleRemovedResult;

        public static void InvokeRoleRemoved(long activityID)
        {
            roleRemoved?.Invoke(activityID);
        }

        public static void InvokeRoleRemovedResult(long activityID, bool wasRemoved)
        {
            roleRemovedResult?.Invoke(activityID, wasRemoved);
        }

        public static void InvokeRoleRequested(long activityID, string roleName)
		{
			roleRequested?.Invoke(activityID, roleName);
		}

        public static void InvokeRoleReceived(long activityID, string roleName, bool wasGivenRole)
        {
			Debug.WriteLine("Receive Role Invoked");
            roleReceived?.Invoke(activityID, roleName, wasGivenRole);
        }

		public static bool TryGetRole(long activityID, string selectedRole, long cpID)
		{
			if (CommunicationInfo.Instance.IsServer)
			{
				LarpActivity _activity = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(activityID);
				if (_activity == null) return false;
				int alreadyRegisteredCount = _activity.registrationByRole.Where(x => x.second == selectedRole).Count();

				if (_activity.roles.Where(x => x.first == selectedRole).First().second > alreadyRegisteredCount)
				{
					_activity.registrationByRole.Add(new Pair<long, string>(cpID, selectedRole));
					return true;
				}
			}
			return false;
        }

		public static bool TryRemoveRoles(long activityID, long cpID)
		{
            LarpActivity _activity = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(activityID);
            if (_activity == null) return false;
            string[] roles = _activity.registrationByRole.Where(x => x.first == cpID).Select(x => x.second.Trim()).ToArray();
            _activity.registrationByRole.RemoveAll(x => x.first == cpID);
			return true;
        }

		#endregion

		private string _activityName;
        public string ActivityName { get { return _activityName; } set { SetProperty(ref _activityName, value); } }


		private string _name;
		private string _description;
		private string _type;
		private string _status;
		private string _dayIndex;
		private TrulyObservableCollection<RoleItemViewModel> _roles;
		private TrulyObservableCollection<ItemItemViewModel> _items;
		private ObservableCollection<string> _equipment;
		private string _preparations;
		private string _location;

		private string _start;
		private string _end;
		private string _duration;

        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
		public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
		public string Type { get { return _type; } set { SetProperty(ref _type, value); } }
		public string Status { get { return _status; } set { SetProperty(ref _status, value); } }
		public string DayIndex { get { return _dayIndex; } set { SetProperty(ref _dayIndex, value); } }
		/// <summary>
		/// Collection of RoleItemViewModel representing the list of possible roles.
		/// </summary>
		public TrulyObservableCollection<RoleItemViewModel> Roles { get { return _roles; } set { SetProperty(ref _roles, value); } }
		/// <summary>
		/// Collection of ItemItemViewModel representing the list of possible roles.
		/// </summary>
		public TrulyObservableCollection<ItemItemViewModel> Items { get { return _items; } set { SetProperty(ref _items, value); } }
		public ObservableCollection<string> Equipment { get { return _equipment; } set { SetProperty(ref _equipment, value); } }
		public string Preparations { get { return _preparations; } set { SetProperty(ref _preparations, value); } }
		public string Location { get { return _location; } set { SetProperty(ref _location, value); } }



		public string Start { get { return _start; } set { SetProperty(ref _start, value); } }
		public string End { get { return _end; } set { SetProperty(ref _end, value); } }
		public string Duration { get { return _duration; } set { SetProperty(ref _duration, value); } }

		#region Icons

		private string[] _icons;
		private int _currentIconIndex;
		private int CurrentIconIndex
		{
			get => _currentIconIndex;
			set
			{
				_currentIconIndex = value;
				CurrentIcon = IconLibrary.GetImageSourceFromResourcePath(_icons[value]);
			}
		}
		private ImageSource _currentIcon;
		public ImageSource CurrentIcon
		{
			get
			{
				return _currentIcon;
			}
			set
			{
				SetProperty(ref _currentIcon, value);
			}
		}

		#endregion

		/// <summary>
		/// Collection of LarpActivityShortItemViewModel representing the list of possible roles.
		/// </summary>
		public TrulyObservableCollection<ActivityShortItemViewModel> Dependencies { get; }


		private bool _isRegistered;
		private bool isRegistered { get { return _isRegistered; } set { _isRegistered = value; OnPropertyChanged(nameof(SignUpText)); } }

		/// <summary>
		/// Display text for signing up or signing out, depending on whether the user is or isn’t registered to one of the roles.
		/// </summary>
		public string SignUpText
		{
			get
			{
				return isRegistered ? "Odhlásit se" : "Přihlásit se";
			}
		}

		public bool CanChangeActivity => LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeActivity);

        public bool NotBusy => !IsBusy;

        public string StatusCommandText => "Změnit stav";


		LarpActivity _activity;

		/// <summary>
		/// Displays menu containing the other commands.
		/// </summary>
		public Command OptionsCommand { get; }
		/// <summary>
		/// Signs user to the activity. If more than one role is present, he is given a dialogue window to choose one of them.
		/// </summary>
		public Command SignUpCommand { get; }
		/// <summary>
		/// Navigates user to the ActivityGraphPage to where the current activity is located.
		/// </summary>
		public Command ShowOnGraphCommand { get; }
		/// <summary>
		/// Navigates to the NewActivityPage for editing 
		/// </summary>
		public Command EditCommand { get; }
		/// <summary>
		/// Gives the user a dialogue window for changing the current Status of the activity.
		/// </summary>
		public Command StatusCommand { get; }

		public Command IconChange { get; set; }


		private INavigation Navigation;

		private IMessageService _messageService;

		public ActivityDetailsViewModel(INavigation navigation, LarpActivity activity)
        {
			_messageService = DependencyService.Get<Services.IMessageService>();
			Navigation = navigation;

			Dependencies = new TrulyObservableCollection<ActivityShortItemViewModel>();
			_roles = new TrulyObservableCollection<RoleItemViewModel>();
			_items = new TrulyObservableCollection<ItemItemViewModel>();

			// Icons need to be assigned before assigning icon index
			_icons = IconLibrary.GetIconsByClass<LarpActivity>();
			CurrentIconIndex = 0;

			Initialize(activity);

			isRegistered = IsRegistered();

			OptionsCommand = new Command(OnOptions);
			SignUpCommand = new Command(OnSignUp);
			ShowOnGraphCommand = new Command(OnShowOnGraph);
			EditCommand = new Command(OnEdit);
			StatusCommand = new Command(OnStatusAsync);
			IconChange = new Command(OnIconChange);
		}

		private void Initialize(LarpActivity activity)
		{
			Dependencies.Clear();
			_roles.Clear();
			_items.Clear();

			_activity = activity;
			ActivityName = _activity.name;

			Name = _activity.name;
			Description = _activity.description;
			Type = _activity.eventType.ToFriendlyString();
			Status = _activity.status.ToFriendlyString();
			CurrentIconIndex = _activity.IconIndex;

			UpdateDisplayedTime();

			DayIndex = (_activity.day + 1) + ".";
			Preparations = _activity.preparationNeeded;
			Location = _activity.place.ToString();

			foreach (var id in _activity.prerequisiteIDs)
			{
				LarpActivity larpActivity = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(id);
				Dependencies.Add(new ActivityShortItemViewModel(larpActivity));
			}

			foreach (Pair<string, int> item in _activity.roles)
			{
				int registered = _activity.registrationByRole
					.Where(x => x.second.Trim() == item.first)
					.Count();
				_roles.Add(new RoleItemViewModel(item.first, item.second, registered, false));
			}

			foreach (var item in _activity.requiredItems)
			{
				InventoryItem invItem = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.getByID(item.first);
				_items.Add(new ItemItemViewModel(invItem, item.second));
			}
		}

		#region Database Propagate Events

		public void OnAppearing()
        {
			SQLEvents.dataChanged += PropagateChanged;
			SQLEvents.dataDeleted += PropagateDeleted;

			if(_activity != null)
				CurrentIconIndex = _activity.IconIndex;
		}

		public void OnDisappearing()
        {
			SQLEvents.dataChanged -= PropagateChanged;
			SQLEvents.dataDeleted -= PropagateDeleted;

			if(_activity != null)
				CurrentIconIndex = _activity.IconIndex;
		}

		private void PropagateChanged(Serializable changed, int changedAttributeIndex)
		{
			if (changed == null || changed.GetType() != typeof(LarpActivity) || ((LarpActivity)changed).getID() != _activity.getID())
				return;

			LarpActivity activity = (LarpActivity)changed;

			Initialize(activity);
		}

		private void PropagateDeleted(Serializable deleted)
		{
			if (deleted == null || deleted.GetType() != typeof(LarpActivity) || ((LarpActivity)deleted).getID() != _activity.getID())
				return;

			_messageService.ShowAlertAsync("Právě zobrazovaná aktivita byla smazána. Budete vráceni zpět do předchozí obrazovky.", "Activita byla smazána");
			Navigation.PopAsync();
		}

		#endregion


		private void UpdateDisplayedTime()
		{

			DateTime startDateTime = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(_activity.start).ToLocalTime();
			Start = startDateTime.ToString(CultureInfo.GetCultureInfo("cs-CZ"));

			DateTime endDateTime = DateTimeExtension.UnixTimeStampMillisecondsToDateTime(_activity.start + _activity.duration).ToLocalTime();
			End = endDateTime.ToString(CultureInfo.GetCultureInfo("cs-CZ"));

			TimeSpan durationTimeSpan = endDateTime - startDateTime;
			Duration = (int)durationTimeSpan.TotalHours + "h " + durationTimeSpan.Minutes + "m";
		}

        private async void OnStatusAsync()
        {
			List<(int, string)> options = new List<(int, string)>();

			foreach (LarpActivity.Status item in Enum.GetValues(typeof(LarpActivity.Status)))
			{
				if(item != LarpActivity.Status.inProgress && item != _activity.status)
					options.Add(((int)item,item.ToFriendlyString()));
			}

			var stringOptions = options.Select(x => x.Item2).ToList();

			int? result = await _messageService.ShowSelectionAsync("Změnit status na:", stringOptions);

			if (result.HasValue)
            {
				var oldStatus = _activity.status;
				_activity.UpdateStatus((LarpActivity.Status)options[result.Value].Item1);
				if (_activity.status == oldStatus)
					await _messageService.ShowAlertAsync("Změna neproběhla, protože byla anulována automatickým nastavením stavu.");
				Status = _activity.status.ToFriendlyString();
            }
		}

		private async void OnOptions()
		{
			const string ShowOnGraph = "Zobrazit na Grafu";
			const string ChangeStatus = "Změnit stav";
			const string Edit = "Upravit";

			var options = new List<string>();
            options.Add(ShowOnGraph);
			options.Add(SignUpText);
			if (CanChangeActivity) options.Add(ChangeStatus);
			if (CanChangeActivity) options.Add(Edit);

            int? selectionResult = await _messageService.ShowSelectionAsync("Možnosti:", options);
			if (!selectionResult.HasValue) return;

			string selectedString = options[selectionResult.Value];
			if (selectedString == ShowOnGraph) OnShowOnGraph();
			if (selectedString == SignUpText) OnSignUp();
			if (selectedString == ChangeStatus) OnStatusAsync();
			if (selectedString == Edit) OnEdit();
        }

		private async void OnEdit()
		{
			await Navigation.PushAsync(new ActivityEditPage(UpdateActivity,_activity));
		}

		private async void OnIconChange()
		{
			CurrentIconIndex = await IconSelectionPage.ShowIconSelectionPage(Navigation, _icons);
		}

		private async void OnSignUp()
		{
			IsBusy = true;
            if (isRegistered)
            {
				UnregisterAsync();
                return;
			}

			bool activityDeleted = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(_activity.ID) == default(LarpActivity);

			if (activityDeleted)
			{
				await _messageService.ShowAlertAsync("Vypadá to, že se snažíte přihlásit na aktivitu, která mezitím byla smazána. Nyní budete navráceni zpět do seznamu aktivit.", "Aktivita neexistuje");
				await Navigation.PopAsync();
                IsBusy = false;
                return;
			}

			if (_activity.roles.Count == 0)
            {
				await _messageService.ShowAlertAsync("Aktivita neobsahuje žádné role ke kterým by se šlo přihlásit.");
                IsBusy = false;
                return;
            }

			string[] roles = _activity.roles.Select(x => x.first).ToArray();
			int index = 0;
			if(_activity.roles.Count > 1)
            {
				int? selectionResult = await _messageService.ShowSelectionAsync("Vyberte roli ke které se chcete přihlásit.", roles);
				if (!selectionResult.HasValue)
				{
                    IsBusy = false;
                    return;
				}
				index = selectionResult.Value;
            }

			string selectedRole = roles[index];

			int alreadyRegisteredCount = _activity.registrationByRole.Where(x => x.second == selectedRole).Count();

			if (CommunicationInfo.Instance.IsServer)
			{
				if (_activity.roles.Where(x => x.first == selectedRole).First().second > alreadyRegisteredCount)
				{
					_activity.registrationByRole.Add(new Pair<long, string>(LocalStorage.cpID, selectedRole));
					var editedRole = Roles.Where(role => role.Name == selectedRole).FirstOrDefault();
					editedRole.CurrentCount = _activity.registrationByRole.Where(x => x.second == selectedRole).Count();

					var Toast = DependencyService.Get<ToastInterface>();
					if (Toast != null)
						Toast.DoTheThing("Zaregistrován jako " + selectedRole + " do " + ActivityName + ".");
                    else await _messageService.ShowAlertAsync("Zaregistrován jako " + selectedRole + " do " + ActivityName + ".");
                }
				else
				{
					await _messageService.ShowAlertAsync("Daná role má již zaplněnou kapacitu. Pokud se chcete stále účastnit této aktivity, přihlašte se na jinou roli nebo požádejte o úpravu kapacity.");
				}
			}
			else
			{
				InvokeRoleRequested(_activity.ID, selectedRole);
				AutoResetEvent waitHandle = new AutoResetEvent(false);
				bool success = false;
				RoleReceivedDelegate ourDelegate = delegate (long activityID, string roleName, bool wasGivenRole)
				{
					Debug.WriteLine("Our Delegate");
					success = wasGivenRole;
					waitHandle.Set();  // signal that the finished event was raised
				};

				roleReceived += ourDelegate;
				bool receivedSignal = await Task.Run(() => waitHandle.WaitOne(5000));
				roleReceived -= ourDelegate;
				if (!receivedSignal) await _messageService.ShowAlertAsync("Server na požadavek registrace neodpověděl, zkontrolujte prosím své připojení.");
				else if (!success) await _messageService.ShowAlertAsync("Daná role má již zaplněnou kapacitu. Pokud se chcete stále účastnit této aktivity, přihlašte se na jinou roli nebo požádejte o úpravu kapacity.");
				else
				{
					var Toast = DependencyService.Get<ToastInterface>();
					if (Toast != null)
						Toast.DoTheThing("Zaregistrován jako " + selectedRole + " do " + ActivityName + ".");
					else await _messageService.ShowAlertAsync("Zaregistrován jako " + selectedRole + " do " + ActivityName + ".");
                }
			}
			isRegistered = IsRegistered();
            if (Device.WPF == Device.RuntimePlatform)
            {
                NavigationPage.SetHasNavigationBar(App.Current.MainPage, true);
            }
            IsBusy = false;
        }

		private async void OnShowOnGraph()
        {
			await Navigation.PushAsync(new ActivityGraphPage(_activity));
		}

		private async void UnregisterAsync()
		{
			IsBusy = true;

            bool activityDeleted = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(_activity.ID) == default(LarpActivity);

			if (activityDeleted)
			{
				await _messageService.ShowAlertAsync("Vypadá to, že se snažíte odhlásit z aktivity, která mezitím byla smazána. Nyní budete navráceni zpět do seznamu aktivit.", "Aktivita neexistuje");
				await Navigation.PopAsync();
                IsBusy = false;
				return;
			}

			long cpID = LocalStorage.cpID;
			string[] roles = _activity.registrationByRole.Where(x => x.first == cpID).Select(x => x.second.Trim()).ToArray();

			if (roles.Length == 0)
            {
				await _messageService.ShowAlertAsync("Žádná role na odebrání");
            }
			else
            {
				bool result = await _messageService.ShowConfirmationAsync($"Opravdu se chcete odregistrovat z role \"{roles[0]}\"");

				if (result)
				{
					if (CommunicationInfo.Instance.IsServer)
					{
						_activity.registrationByRole.RemoveAll(x => x.first == cpID);
						var editedRoles = Roles.Where(role => roles.Contains(role.Name));
						foreach (var editedRole in editedRoles)
						{
							editedRole.CurrentCount = _activity.registrationByRole.Where(x => x.second == editedRole.Name).Count();
						}
					}
					else
					{
                        InvokeRoleRemoved(_activity.ID);
                        AutoResetEvent waitHandle = new AutoResetEvent(false);
                        bool success = false;
                        RoleRemovedResultDelegate ourDelegate = delegate (long activityID, bool removedSuccessfully)
                        {
                            Debug.WriteLine("Our Delegate");
                            success = removedSuccessfully;
                            waitHandle.Set();  // signal that the finished event was raised
                        };

                        roleRemovedResult += ourDelegate;
                        bool receivedSignal = await Task.Run(() => waitHandle.WaitOne(5000));
						roleRemovedResult -= ourDelegate;
						if (!receivedSignal) await _messageService.ShowAlertAsync("Server na požadavek odhlášení z aktivity neodpověděl, zkontrolujte prosím své připojení.");
						else if (!success)
						{
							await _messageService.ShowAlertAsync("Vypadá to, že se snažíte odhlásit z aktivity, která mezitím byla smazána. Nyní budete navráceni zpět do seznamu aktivit.", "Aktivita neexistuje");
							await Navigation.PopAsync();
						}
						else
						{
							var Toast = DependencyService.Get<ToastInterface>();
							if (Toast != null)
								Toast.DoTheThing("Odhlášení z " + ActivityName + " proběhlo úspěšně.");
							else await _messageService.ShowAlertAsync("Odhlášení z " + ActivityName + " proběhlo úspěšně.");
						}
                    }
				}
            }
			isRegistered = IsRegistered();
            IsBusy = false;
        }

        private async void UpdateActivity(LarpActivityDTO larpActivityDTO)
		{
			bool activityDeleted = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(_activity.ID) == default(LarpActivity);

			if(activityDeleted)
			{
				await _messageService.ShowAlertAsync("Vypadá to, že se snažíte upravit aktivitu, která mezitím byla smazána. Nyní budete navráceni zpět do seznamu aktivit.","Activita neexistuje");
				await Navigation.PopAsync();
				return;
			}


			_activity.UpdateWhole(
				larpActivityDTO.name,
				larpActivityDTO.description,
				larpActivityDTO.preparationNeeded,
				larpActivityDTO.eventType,
				larpActivityDTO.duration,
				larpActivityDTO.day,
				larpActivityDTO.start,
				larpActivityDTO.place,
				larpActivityDTO.status,
				larpActivityDTO.iconIndex);

			_activity.UpdateRoles(larpActivityDTO.roles);
			_activity.UpdateItems(larpActivityDTO.requiredItems);
			_activity.UpdatePrerequisiteIDs(larpActivityDTO.prerequisiteIDs);

			Initialize(_activity);
		}

		private bool IsRegistered()
        {
			return _activity.registrationByRole.FindIndex(p => p.first == LocalStorage.cpID) >= 0;
		}
	}
}
