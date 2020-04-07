using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using OmniPie.Annotations;
using OmniPie.Api;
using Xamarin.Forms;

namespace OmniPie.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        public ICommand DownloadHistoryCommand { get; set; }
        public IEnumerable<OmniPyHistoryEntry> Entries { get; set; }

        public HistoryViewModel(Page page) : base(page)
        {
            DownloadHistoryCommand = new Command(async () =>
            {
                DebugOut = await Client.DownloadHistory();
                Entries = await Client.ReadHistory();
            });
        }

        protected override async void OnPageAppearing()
        {
            if (Entries == null)
            {
                Entries = await Client.ReadHistory();
            }
        }
    }
}
