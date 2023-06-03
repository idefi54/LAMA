using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    public class PermissionViewModel:BaseViewModel
    {

        public string Name { get; set; }
        public bool Checked { get; set; }
        private bool _canChange;
        public bool CanChange { get => _canChange; set => SetProperty(ref _canChange, value); }

        public PermissionViewModel(string name, bool isChecked, Func<bool> canChange, CP.PermissionType type)
        {
            Name = name;
            Checked = isChecked;
            CanChange = canChange();
            this.type = type;

            SQLEvents.dataChanged += (Serializable changed, int changedAttributeIndex) =>
            {
                if (changed is CP cp && cp.ID == LocalStorage.cpID && changedAttributeIndex == 9)
                    CanChange = canChange();
            };
        }

        public CP.PermissionType type;
    }
}
