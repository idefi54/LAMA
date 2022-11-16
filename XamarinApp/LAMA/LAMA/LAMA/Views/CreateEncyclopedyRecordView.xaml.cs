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
    public partial class CreateEncyclopedyRecordView : ContentPage
    {
        public CreateEncyclopedyRecordView()
        {
            InitializeComponent();
            BindingContext = new EncyclopedyRecordViewModel(null, Navigation);
        }
    }
}