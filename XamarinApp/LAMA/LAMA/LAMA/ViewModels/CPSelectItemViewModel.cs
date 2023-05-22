using LAMA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LAMA.ViewModels
{
    public class CPSelectItemViewModel : BaseViewModel
    {
        public CP cp { get; private set; }
        
        public string Name => cp.name;
        public string Nick => cp.nick;
        public string FullIdentifier => $"{cp.nick} - {cp.name}";
        public string Roles => String.Join(", ", cp.roles);
        public CPSelectItemViewModel(CP cp)
        {
            this.cp = cp;
        }
    }
}
