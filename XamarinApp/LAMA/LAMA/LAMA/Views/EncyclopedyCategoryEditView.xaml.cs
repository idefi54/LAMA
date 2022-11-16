﻿using LAMA.Models;
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
    public partial class EncyclopedyCategoryEditView : ContentPage
    {
        public EncyclopedyCategoryEditView(EncyclopedyCategory whatToEdit)
        {
            InitializeComponent();
            BindingContext = new EncyclopedyCategoryViewModel(whatToEdit, Navigation);
        }
    }
}