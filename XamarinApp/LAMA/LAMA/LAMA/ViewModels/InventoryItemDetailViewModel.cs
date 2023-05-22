﻿using LAMA.Models;
using LAMA.Services;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
namespace LAMA.ViewModels
{
    internal class InventoryItemDetailViewModel : BaseViewModel
    {

        InventoryItem _item;

        string _name;
        string _description;
        int _numBorrowed;
        int _numFree;
        string _borrowedBy;

        int _howManyChange;
        string _CPName;

        public string Name { get { return _name; } private set { SetProperty(ref _name, value); } }
        public string Description { get { return _description; } private set { SetProperty(ref _description, value); } }
        public string NumBorrowed { get { return _numBorrowed.ToString(); } private set { SetProperty(ref _numBorrowed, Helpers.readInt( value)); } }
        public string NumFree { get { return _numFree.ToString(); } private set { SetProperty(ref _numFree, Helpers.readInt(value)); } }
        public string BorrowedBy { get { return _borrowedBy; } private set { SetProperty(ref _borrowedBy, value); } }
        public bool ManageInventory { get { return LocalStorage.cp.permissions.Contains(CP.PermissionType.ManageInventory); } set { } }
        public Command DetailedBorrowCommand { get; private set; }
        public Command DetailedReturnCommand { get; private set; }
        public Command SelectCP { get; private set; }
        public Command DeleteCommand { get; private set; }
        public string CPName { get { return _CPName; } set { SetProperty(ref _CPName, value); } }
        private CP selectedCP;

        public string HowManyChange { get { return _howManyChange.ToString(); } set { SetProperty(ref _howManyChange, Helpers.readInt(value)); } }
        

        INavigation _navigation;
        IMessageService _messageService;
        public InventoryItemDetailViewModel(INavigation navigation, InventoryItem item)
        {
            _item = item;
            _navigation = navigation;
            _messageService = DependencyService.Get<IMessageService>();

            _name = item.name;
            _description = item.description;
            _numBorrowed = item.taken;
            _numFree = item.free;

            writeBorrowedBy();
            
            DetailedBorrowCommand = new Command(OnBorrow);
            DetailedReturnCommand = new Command(OnReturn);
            SelectCP = new Command(OnSelectCP);
            DeleteCommand = new Command(onDelete);

            SetProperty(ref _howManyChange, 0);
            selectedCP = DatabaseHolder<CP, CPStorage>.Instance.rememberedList[0];
            CPName = selectedCP.name;
        }

        private async void OnSelectCP(object obj)
        {
            CPSelectionPage page = new CPSelectionPage(_displayName);
            await _navigation.PushAsync(page);


            void _displayName(CP cp)
            {
                if (cp != null)
                {
                    CPName = cp.name;
                }
            }
        }

        public async void OnBorrow()
        {
            if (!CheckExistence().Result)
                return;

            if (_howManyChange < 1)
                return;

            if (selectedCP == null)
            {
                await _messageService.ShowAlertAsync("Nebylo vybráno žádné CP.", "Nečekaná chyba");
                return;
            }

            _item.Borrow(_howManyChange, selectedCP.ID);
            NumBorrowed = _item.taken.ToString();
            NumFree = _item.free.ToString();

            HowManyChange = "0";
            writeBorrowedBy();
        }

        public async void OnReturn()
        {
            if (!CheckExistence().Result)
                return;

            if (_howManyChange < 1)
                return;

            if (selectedCP == null)
            {
                await _messageService.ShowAlertAsync("Nebylo vybráno žádné CP.", "Nečekaná chyba");
                return;
            }

            _item.Return(_howManyChange, selectedCP.ID);
            NumBorrowed = _item.taken.ToString();
            NumFree = _item.free.ToString();

            HowManyChange = "0";
            writeBorrowedBy();
        }

        private void writeBorrowedBy()
        {
            SetProperty(ref _numBorrowed, _item.taken);
            SetProperty(ref _numFree, _item.free);

            var CPList = DatabaseHolder<CP, CPStorage>.Instance.rememberedList;
            StringBuilder output = new StringBuilder();
            foreach(var a in this._item.takenBy)
            {
                output.Append(CPList.getByID(a.first).nick);
                output.Append(": ");
                output.Append(a.second.ToString());
                output.Append(", ");
            }

            BorrowedBy = output.ToString();
        }
        async void onDelete()
        {
            if (!CheckExistence().Result)
                return;

            DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.removeByID(_item.ID);
            await _navigation.PopAsync();
        }

        private async Task<bool> CheckExistence()
        {
            bool itemDeleted = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.getByID(_item.ID) == default(InventoryItem);

            if (itemDeleted)
            {
                await DependencyService.Get<Services.IMessageService>()
                    .ShowAlertAsync(
                        "Vypadá to, že se snažíte pracovat s předmětem, který mezitím byl smazán. Nyní budete navráceni zpět do seznamu předmětů.",
                        "Předmět neexistuje");
                await _navigation.PopAsync();
                IsBusy = false;
                return false;
            }
            return true;
        }
    }
}
