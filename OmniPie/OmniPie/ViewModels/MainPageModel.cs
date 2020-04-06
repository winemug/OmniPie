using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniPie.Annotations;
using OmniPie.Api;
using OmniPie.Definitions;
using OmniPie.Interfaces;
using Xamarin.Forms;

namespace OmniPie.ViewModels
{
    public class MainPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ConnectCommand { get; set; }
        public ICommand CancelConnectCommand { get; set; }

        public string ConnectionResult { get; set; }

        public MainPageModel()
        {
            ConnectCommand = new Command(async () => await Connect(), () => true);
            CancelConnectCommand = new Command(() => { }, () => false);
        }
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task Connect()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            try
            {
                CancelConnectCommand = new Command(() => { cts?.Cancel(); }, () => true);

                var key = OmniPyKey.Load();
                var connection = new OmniPyConnection(key);
                var result = await connection.VerifyConnection(cts.Token);
                switch (result)
                {
                    case OmniPyConnectionResult.DiscoveryFailed:
                        ConnectionResult = "Couldn't find OmniPy on the network.";
                        break;
                    case OmniPyConnectionResult.ConnectFailed:
                        ConnectionResult = $"Failed to connect to omnipy located at {connection.Address}";
                        break;
                    case OmniPyConnectionResult.IncorrectPassword:
                        ConnectionResult = $"Password is incorrect.";
                        break;
                    case OmniPyConnectionResult.OK:
                        ConnectionResult = $"Connection OK.";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                CancelConnectCommand = new Command(() => { }, () => false);
                cts.Dispose();
                cts = null;
            }
        }
    }
}
