using LAMA.Singletons;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace LAMA.ViewModels
{
    internal class LarpEventViewModel:BaseViewModel
    {

        public string Name { get; set; }
        public string ChatChannels { get; set; }
        public string NewChannel { get; set; }
        public bool CanChangeLarpEvent { get { return LocalStorage.cp.permissions.Contains(Models.CP.PermissionType.ManageEvent); } }
        public bool CanNotChangeLarpEvent { get { return !CanChangeLarpEvent; } }
        Command SaveCommand { get; set; }
        Command AddChannelCommand { get; set; }


        INavigation navigation;
        public LarpEventViewModel(INavigation navigation)
        {
            this.navigation = navigation;

            Name = LarpEvent.Name;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < LarpEvent.ChatChannels.Count; ++i)
            {
                if(i !=0)
                    builder.Append(Environment.NewLine);
                builder.Append(ChatChannels[i]);
            }
            SaveCommand = new Command(OnSave);
            AddChannelCommand = new Command(OnAddChannel);
        }
        void OnSave()
        {
            LarpEvent.Name = Name;
        }
        void OnAddChannel()
        {
            LarpEvent.ChatChannels.Add(NewChannel);
            NewChannel = string.Empty;
        }

        //TODO add days
    }
}
