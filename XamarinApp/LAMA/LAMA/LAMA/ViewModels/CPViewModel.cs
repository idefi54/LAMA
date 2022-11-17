using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.ViewModels
{
    internal class CPViewModel:BaseViewModel
    {
        public CP cp { get; private set; }
        
        public string Name { get { return cp.name; } }
        public string Nick { get { return cp.nick; } }
        public string Roles { get { return cp.roles.ToString(); } }

        public CPViewModel(CP cp)
        {
            this.cp = cp;
        }
    }
}
