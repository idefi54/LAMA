﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LAMA.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateCPPage : ContentPage
    {
        public CreateCPPage()
        {
            InitializeComponent();
            BindingContext = new ViewModels.CreateCPViewModel(Navigation);
        }
    }
}