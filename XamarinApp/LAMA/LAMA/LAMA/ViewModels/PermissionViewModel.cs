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
        public bool CanChange { get; set; }

        public PermissionViewModel(string name, bool cchecked, bool canchange, CP.PermissionType type)
        {
            Name = name;
            Checked = cchecked;
            CanChange= canchange;
            this.type = type;
        }

        public CP.PermissionType type;
    }
}
