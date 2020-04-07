using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OmniPie.Annotations;
using OmniPie.Api;

namespace OmniPie.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public bool ClientCanConnect { get; set; }
        public string DebugOut { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected readonly OmniPyClient Client;

        public BaseViewModel()
        {
            Client = OmniPyClient.Get();
            Client.WhenClientCanConnectChanged().Subscribe(canConnect => { ClientCanConnect = canConnect; });

        }
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}