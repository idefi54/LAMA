using LAMA.Extensions;
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

namespace LAMA.ViewModels
{
    public class DisplayActivityViewModel : BaseViewModel
    {
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
		public TrulyObservableCollection<RoleItemViewModel> Roles { get { return _roles; } set { SetProperty(ref _roles, value); } }
		public TrulyObservableCollection<ItemItemViewModel> Items { get { return _items; } set { SetProperty(ref _items, value); } }
		public ObservableCollection<string> Equipment { get { return _equipment; } set { SetProperty(ref _equipment, value); } }
		public string Preparations { get { return _preparations; } set { SetProperty(ref _preparations, value); } }
		public string Location { get { return _location; } set { SetProperty(ref _location, value); } }



		public string Start { get { return _start; } set { SetProperty(ref _start, value); } }
		public string End { get { return _end; } set { SetProperty(ref _end, value); } }
		public string Duration { get { return _duration; } set { SetProperty(ref _duration, value); } }


		public TrulyObservableCollection<LarpActivityShortItemViewModel> Dependencies { get; }


		private bool _isRegistered;
		private bool isRegistered { get { return _isRegistered; } set { _isRegistered = value; OnPropertyChanged(nameof(SignUpText)); } }
		

		public string SignUpText
		{
			get
			{
				return isRegistered ? "Odhlásit se" : "Přihlásit se";
			}
		}

		public bool CanChangeActivity => LocalStorage.cp.permissions.Contains(CP.PermissionType.ChangeActivity);


		public string StatusCommandText => "Změnit stav";
		public Command StatusCommand { get; }


		LarpActivity _activity;


		public Command SignUpCommand { get; }
		public Command ShowOnGraphCommand { get; }
		public Command EditCommand { get; }


		private INavigation Navigation;

		private IMessageService _messageService;

		public DisplayActivityViewModel(INavigation navigation, LarpActivity activity)
        {
			_messageService = DependencyService.Get<Services.IMessageService>();
			Navigation = navigation;

			Dependencies = new TrulyObservableCollection<LarpActivityShortItemViewModel>();

			Initialize(activity);

			isRegistered = IsRegistered();

			SignUpCommand = new Xamarin.Forms.Command(OnSignUp);
			ShowOnGraphCommand = new Xamarin.Forms.Command(OnShowOnGraph);
			EditCommand = new Xamarin.Forms.Command(OnEdit);
			StatusCommand = new Xamarin.Forms.Command(OnStatusAsync);

			SQLEvents.dataChanged += PropagateChanged;
			SQLEvents.dataDeleted += PropagateDeleted;
		}

		private void Initialize(LarpActivity activity)
		{
			Dependencies.Clear();

			_activity = activity;
			ActivityName = _activity.name;

			Name = _activity.name;
			Description = _activity.description;
			Type = _activity.eventType.ToFriendlyString();
			Status = _activity.status.ToFriendlyString();

			UpdateDisplayedTime();

			DayIndex = (_activity.day + 1) + ".";
			Preparations = _activity.preparationNeeded;
			Location = _activity.place.ToString();

			foreach (var id in _activity.prerequisiteIDs)
			{
				LarpActivity larpActivity = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(id);
				Dependencies.Add(new LarpActivityShortItemViewModel(larpActivity));
			}
			_roles = new TrulyObservableCollection<RoleItemViewModel>();
			foreach (Pair<string, int> item in _activity.roles)
			{
				int registered = _activity.registrationByRole
					.Where(x => x.second.Trim() == item.first)
					.Count();
				_roles.Add(new RoleItemViewModel(item.first, item.second, registered, false));
			}

			_items = new TrulyObservableCollection<ItemItemViewModel>();
			foreach (var item in _activity.requiredItems)
			{
				InventoryItem invItem = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.getByID(item.first);
				_items.Add(new ItemItemViewModel(invItem, item.second));
			}
		}

		#region Database Propagate Events

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
				_activity.status = (LarpActivity.Status)options[result.Value].Item1;
				Status = _activity.status.ToFriendlyString();
            }
		}

		private async void OnEdit()
		{
			await Navigation.PushAsync(new NewActivityPage(UpdateActivity,_activity));
		}

		private async void OnSignUp()
		{
			if (isRegistered)
            {
				UnregisterAsync();
				return;
			}

			bool activityDeleted = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(_activity.ID) == default(LarpActivity);

			if (activityDeleted)
			{
				await _messageService.ShowAlertAsync("Vypadá to, že se snažíte přihlásit na aktivitu, která mezitím byla smazána. Nyní budete navráceni zpět do seznamu aktivit.", "Activita neexistuje");
				await Navigation.PopAsync();
				return;
			}

			if (_activity.roles.Count == 0)
            {
				await _messageService.ShowAlertAsync("Aktivita neobsahuje žádné role ke kterým by se šlo přihlásit.");
				return;
            }

			string[] roles = _activity.roles.Select(x => x.first).ToArray();
			int index = 0;
			if(_activity.roles.Count > 1)
            {
				int? selectionResult = await _messageService.ShowSelectionAsync("Vyberte roli ke které se chcete přihlásit.", roles);
				if (!selectionResult.HasValue)
					return;
				index = selectionResult.Value;
            }

			string selectedRole = roles[index];

			int alreadyRegisteredCount = _activity.registrationByRole.Where(x => x.second == selectedRole).Count();

			if(_activity.roles.Where(x=>x.first == selectedRole).First().second > alreadyRegisteredCount)
            {
				_activity.registrationByRole.Add(new Pair<long, string>(LocalStorage.cpID,selectedRole));
				var editedRole = Roles.Where(role => role.Name == selectedRole).FirstOrDefault();
				editedRole.CurrentCount = _activity.registrationByRole.Where(x => x.second == selectedRole).Count();

				var Toast = DependencyService.Get<ToastInterface>();
				if (Toast != null)
					Toast.DoTheThing("Zaregistrován jako " + selectedRole + " do " + ActivityName + ".");


			}
			else
            {
				await _messageService.ShowAlertAsync("Daná role má již zaplněnou kapacitu. Pokud chcete stále se účastnit této aktivity, přihlašte se na jinou roli nebo požádejte o úpravu kapacity.");
            }

			isRegistered = IsRegistered();
		}

		private async void OnShowOnGraph()
        {
			await Navigation.PushAsync(new ActivityGraphPage(_activity));
        }

        private async void UnregisterAsync()
		{
			bool activityDeleted = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(_activity.ID) == default(LarpActivity);

			if (activityDeleted)
			{
				await _messageService.ShowAlertAsync("Vypadá to, že se snažíte odhlásit z aktivity, která mezitím byla smazána. Nyní budete navráceni zpět do seznamu aktivit.", "Activita neexistuje");
				await Navigation.PopAsync();
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
					_activity.registrationByRole.RemoveAll(x => x.first == cpID);
					var editedRoles = Roles.Where(role => roles.Contains(role.Name));
					foreach (var editedRole in editedRoles)
                    {
						editedRole.CurrentCount = _activity.registrationByRole.Where(x => x.second == editedRole.Name).Count();
					}
				}
            }
			isRegistered = IsRegistered();
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
				larpActivityDTO.status);

			Name = larpActivityDTO.name;
			Description = larpActivityDTO.description;

			Roles.Clear();
			_activity.UpdateRoles(larpActivityDTO.roles);
			foreach(var role in larpActivityDTO.roles)
			{
				int registered = _activity.registrationByRole
					.Where(x => x.second.Trim() == role.first)
					.Count();
				Roles.Add(new RoleItemViewModel(role.first, role.second, registered, false));
			}

			Items.Clear();
			_activity.UpdateItems(larpActivityDTO.requiredItems);
			foreach(var item in larpActivityDTO.requiredItems)
            {
				InventoryItem invItem = DatabaseHolder<InventoryItem,InventoryItemStorage>.Instance.rememberedList.getByID(item.first);
				Items.Add(new ItemItemViewModel(invItem, item.second));
            }

			Dependencies.Clear();
			_activity.UpdatePrerequisiteIDs(larpActivityDTO.prerequisiteIDs);
			foreach (int id in _activity.prerequisiteIDs)
			{
				LarpActivity la = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(id);
				Dependencies.Add(new LarpActivityShortItemViewModel(la));
			}

			UpdateDisplayedTime();

			DayIndex = (larpActivityDTO.day + 1) + ".";
			Preparations = larpActivityDTO.preparationNeeded;
			Location = larpActivityDTO.place.ToString();
		}

		private bool IsRegistered()
        {
			return _activity.registrationByRole.FindIndex(p => p.first == LocalStorage.cpID) >= 0;
		}
	}
}
