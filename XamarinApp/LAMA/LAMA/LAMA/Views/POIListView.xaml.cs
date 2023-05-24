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
    public partial class POIListView : ContentPage
    {
        public POIListView()
        {
            InitializeComponent();
            BindingContext = new POIListViewModel(Navigation);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // This fixes a bug of disappearing icons.
            BindingContext = new POIListViewModel(Navigation);
        }
    }
}