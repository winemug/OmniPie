﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniPie.ViewModels;
using Xamarin.Forms;

namespace OmniPie
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class OmniPyPage : ContentPage
    {
        public OmniPyPage()
        {
            InitializeComponent();
            new OmniPyViewModel(this);
        }
    }
}
