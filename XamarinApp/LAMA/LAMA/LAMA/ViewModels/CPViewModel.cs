using LAMA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LAMA.ViewModels
{
    internal class CPViewModel:BaseViewModel, INotifyPropertyChanged
    {
        public CP cp { get; private set; }
        
        public string Name { get { return cp.name; } }
        public string Nick { get { return cp.nick; } }
        public string Roles { get { return cp.roles.ToReadableString(); } }

        public CPViewModel(CP cp)
        {
            this.cp = cp;
            cp.IGotUpdated += onUpdate;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void onUpdate(object sender, int index)
        {
            string propName = nameof(Name);
            switch(index)
            {
                case 1:
                    propName = nameof(Name);
                    break;
                case 2:
                    propName = nameof(Nick);
                    break;
                case 3: 
                    propName = nameof(Roles);
                    break;
                default: 
                    return;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            

        }
    }
}
