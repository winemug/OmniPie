using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OmniPie.Annotations;
using OmniPie.Api;
using Xamarin.Forms;

namespace OmniPie.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public bool ClientCanConnect { get; set; }
        public string DebugOut { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected readonly OmniPyClient Client;

        public BaseViewModel(Page page)
        {
            Client = OmniPyClient.Get();
            Client.WhenClientCanConnectChanged().Subscribe(canConnect => { ClientCanConnect = canConnect; });
            page.BindingContext = this;
            page.Appearing += PageOnAppearing;
        }

        private void PageOnAppearing(object sender, EventArgs e)
        {
            OnPageAppearing();
        }

        protected virtual void OnPageAppearing()
        {
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}