﻿using LAMA.ViewModels;
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
    public partial class CreateInventoryItemView : ContentPage
    {
        public CreateInventoryItemView()
        {
            InitializeComponent();
            BindingContext = new CreateInventoryItemViewModel(Navigation);
        }
    }
}