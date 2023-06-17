using LAMA.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LarpEventPage : ContentPage
    {
        public LarpEventPage()
        {
            InitializeComponent();
            BindingContext = new LarpEventViewModel(Navigation);
            SaveButton.BackgroundColor = Color.DarkGray;
        }

        private void Changed(object sender, EventArgs e)
        {
            SaveButton.BackgroundColor = Color.Green;
        }

        private void Saved(object sender, EventArgs e)
        {
            SaveButton.BackgroundColor = Color.DarkGray;
        }
    }
}