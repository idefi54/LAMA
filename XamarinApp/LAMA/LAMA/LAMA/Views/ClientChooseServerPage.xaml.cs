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
    public partial class ClientChooseServerPage : ContentPage
    {
        public ClientChooseServerPage()
        {
            InitializeComponent();
            this.BindingContext = new ClientChooseServerViewModel();
        }

        public ClientChooseServerPage(string serverName)
        {
            InitializeComponent();
            this.BindingContext = new ClientChooseServerViewModel(serverName);
        }
    }
}