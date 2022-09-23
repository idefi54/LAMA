using LAMA.Models;
using LAMA.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;
namespace LAMA.ViewModels
{
    internal class InventoryItemDetailViewModel:BaseViewModel
    {

        InventoryItem _item;

        string _name;
        string _description;
        int _numBorrowed;
        int _numFree;
        string _borrowedBy;

        string _borrowName;
        int _howManyBorrow;
        string _returnName;
        int _returnNum;


        public string Name { get { return _name; } }
        public string Description { get { return _description; } }
        public string NumBorrowed { get { return _numBorrowed.ToString(); } }
        public string NumFree { get { return _numFree.ToString(); } }
        public string BorrowedBy { get { return _borrowedBy; } }

        public Command DetailedBorrowCommand;
        public Command DetailedReturnCommand;

        public string BorrowerName { get { return _borrowName; } set { SetProperty(ref _borrowName, value); } }
        public string HowManyBorrow { get { return _howManyBorrow.ToString(); } set { SetProperty(ref _howManyBorrow, Helpers.readInt(value)); } }
        public string ReturnerName { get { return _returnName; } set { SetProperty(ref _returnName, value); } }
        public string HowManyReturn { get { return _returnNum.ToString(); } set { SetProperty(ref _returnNum, Helpers.readInt(value)); } }
        //Name, Description, NumBorrowed, NumFree, BorrowedBy

        //DetailedBorrowCommand, DetailedReturnCommand
        //BorrowerName, HowManyBorrow
        //ReturnerName, HowManyReturn

        INavigation Navigation;
        public InventoryItemDetailViewModel(INavigation navigation, InventoryItem item)
        {
            _item = item;
            Navigation = navigation;


            _name = item.name;
            _description = item.description;
            _numBorrowed = item.taken;
            _numFree = item.free;
            _borrowedBy = item.takenBy.ToString();

            DetailedBorrowCommand = new Command(OnBorrow);
            DetailedReturnCommand = new Command(OnReturn);
        }
        public async void OnBorrow()
        {
            //TADY DÁT TO VYJÍŽDĚCÍ OKNO
            if (_howManyBorrow < 1)
                return;
            
            _item.Borrow(_howManyBorrow, _borrowName);
            _howManyBorrow = 0;
            _borrowName = "";
        }
        public async void OnReturn()
        {
            _item.Return(_returnNum, _returnName);
            _returnNum = 0;
            _returnName = "";
        }

    }
}
