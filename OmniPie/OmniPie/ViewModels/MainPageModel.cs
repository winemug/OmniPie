using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniPie.Annotations;
using OmniPie.Api;
using OmniPie.Definitions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OmniPie.ViewModels
{
    public class MainPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Host { get; set; }
        public string Password { get; set; }

        public ICommand LocateCommand { get; set; }
        public bool ClientCanConnect { get; set; }

        public decimal TempBasalRate { get; set; } = 0m;

        public decimal TempBasalDuration { get; set; } = 2m;

        public ICommand VerifyConnectionCommand { get; set; }
        public ICommand StatusCommand { get; set; }
        public ICommand SetTempBasalCommand { get; set;}
        public ICommand CancelTempBasalCommand { get; set; }

        public string DebugOut { get; set; }

        private readonly OmniPyClient Client;

        public MainPageModel()
        {
            LocateCommand = new Command(async () => await Locate(), () => true);

            Client = new OmniPyClient();
            Client.WhenClientCanConnectChanged().Subscribe(canConnect => { ClientCanConnect = canConnect; });

            PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(Host):
                    case nameof(Password):
                        Preferences.Set(ConfigurationConstants.OmniPyAddress, Host);
                        Preferences.Set(ConfigurationConstants.OmniPyPassword, Password);
                        Client.SetConnectionInfo(Host, Password);
                        break;
                    //case nameof(TempBasalDuration):
                    //    if (TempBasalDuration < 0.5m)
                    //        TempBasalDuration = 0.5m;
                    //    if (TempBasalDuration > 12m)
                    //        TempBasalDuration = 12m;

                    //    TempBasalDuration = Math.Round(TempBasalDuration * 2) / 2m;
                    //    break;
                    //case nameof(TempBasalRate):
                    //    if (TempBasalRate < 0m)
                    //        TempBasalRate = 0m;
                    //    if (TempBasalRate > 30m)
                    //        TempBasalRate = 30m;
                    //    TempBasalRate = Math.Round(TempBasalDuration * 20) / 20m;
                    //    break;
                }
            };

            Host = Preferences.Get(ConfigurationConstants.OmniPyAddress, "");
            Password = Preferences.Get(ConfigurationConstants.OmniPyPassword, "omnipy");
            Client.SetConnectionInfo(Host, Password);

            VerifyConnectionCommand = new Command(async() => DebugOut = await Client.VerifyConnection());
            StatusCommand = new Command(async () => DebugOut = await Client.UpdateStatus(), () => true);
            SetTempBasalCommand = new Command(async () => DebugOut = await Client.SetTempBasal(TempBasalRate, TempBasalDuration), () => true);
            CancelTempBasalCommand = new Command(async () => DebugOut = await Client.CancelTempBasal(), () => true);

        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task Locate()
        {
            ClientCanConnect = false;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            try
            {
                var result = await Discover(cts.Token);
                if (result != null)
                {
                    Host = result;
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                cts.Dispose();
                ClientCanConnect = await Client.WhenClientCanConnectChanged().FirstAsync();
            }
        }

        private async Task<string> Discover(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => tcs.SetCanceled());

            var server = new UdpClient(6665, AddressFamily.InterNetwork)
            {
                EnableBroadcast = true
            };
            var receiveTask = server.ReceiveAsync();

            var broadcastTargets =
                NetworkInterface.GetAllNetworkInterfaces()
                    .Where(i => i.Description == "bt-pan" || i.Description == "wlan0")
                    .Select(i => i.GetIPProperties())
                    .SelectMany(ipp => ipp.UnicastAddresses)
                    .Select(uc => new { Address = uc.Address, Mask =uc.IPv4Mask })
                    .Where(uce => uce.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(uce => new { AdressBytes = uce.Address.GetAddressBytes(), MaskBytes= uce.Mask.GetAddressBytes() })
                    .Select(ucip => new IPAddress(new byte[]
                    {
                        (byte) (ucip.AdressBytes[0] | ~ucip.MaskBytes[0]),
                        (byte) (ucip.AdressBytes[1] | ~ucip.MaskBytes[1]),
                        (byte) (ucip.AdressBytes[2] | ~ucip.MaskBytes[2]),
                        (byte) (ucip.AdressBytes[3] | ~ucip.MaskBytes[3])
                    })); // because that's how I like it

            while (true)
            {
                var client = new UdpClient()
                {
                    EnableBroadcast = true
                };

                var data= Encoding.ASCII.GetBytes("Oh dear.");
                foreach(var broadcastTarget in broadcastTargets)
                    await client.SendAsync(data, data.Length, new IPEndPoint(broadcastTarget, 6664));

                var which = await Task.WhenAny(Task.Delay(5000), receiveTask, tcs.Task);
                if (which == receiveTask)
                {
                    var receiveResult = await receiveTask;
                    return receiveResult.RemoteEndPoint.Address.ToString();
                }
                if (which == tcs.Task)
                {
                    await tcs.Task;
                }
            }
        }
    }
}
