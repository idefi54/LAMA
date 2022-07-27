using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class NewActivityRoleViewModel : BaseViewModel
    {
        private string _name;
        private int _count;
        
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
        public int Count { get { return _count; } set { SetProperty(ref _count, value); } }


        public NewActivityRoleViewModel(string name = "", int count = 0)
        {
            Name = name;
            Count = count;
        }
    }
}
