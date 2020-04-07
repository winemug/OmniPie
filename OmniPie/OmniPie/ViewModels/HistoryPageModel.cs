using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using OmniPie.Annotations;
using OmniPie.Api;

namespace OmniPie.ViewModels
{
    public class HistoryPageModel : INotifyPropertyChanged
    {
        private IEnumerable<OmniPyHistoryEntry> Entries { get; }

        public HistoryPageModel(IEnumerable<OmniPyHistoryEntry> entries)
        {
            Entries = entries;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
