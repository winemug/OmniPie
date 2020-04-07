using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniPie.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniPie
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistoryPage : ContentPage
    {
        public HistoryPage(HistoryPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}