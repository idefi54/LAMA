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
    public partial class CreateEncyclopediaCategoryPage : ContentPage
    {
        public CreateEncyclopediaCategoryPage(EncyclopedyCategory category )
        {
            InitializeComponent();
            BindingContext = new EncyclopedyCategoryViewModel(null, Navigation, category);
        }
    }
}