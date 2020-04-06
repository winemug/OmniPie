using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OmniPie.Annotations;
using OmniPie.Api;
using OmniPie.Definitions;
using OmniPie.Interfaces;
using Renci.SshNet;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace OmniPie.ViewModels
{
    public class MainPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Host { get; set; } = "";
        public string Password { get; set; }= "omnipy";
        public ICommand LocateCommand { get; set; }
        public bool LocateEnabled { get; set; }

        public decimal TempBasalRate { get; set; } = 0m;

        public decimal TempBasalDuration { get; set; } = 2m;

        public ICommand StatusCommand { get; set; }
        public ICommand SetTempBasalCommand { get; set;}
        public ICommand CancelTempBasalCommand { get; set; }

        public string DebugOut { get; set; }

        public MainPageModel()
        {
            LocateCommand = new Command(async () => await Locate(), () => true);
            LocateEnabled = true;
            StatusCommand = new Command(async () => await UpdateStatus(), () => true);
            SetTempBasalCommand = new Command(async () => await SetTempBasal(TempBasalRate, TempBasalDuration), () => true);
            CancelTempBasalCommand = new Command(async () => await CancelTempBasal(), () => true);
        }
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task Locate()
        {
            LocateEnabled = false;
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
                LocateEnabled = true;
            }
        }

        public async Task UpdateStatus()
        {
            using (var client = new SshClient(Host, "pi", Password))
            {
                client.HostKeyReceived += (sender, e) =>
                {
                    e.CanTrust = true;
                };
                client.Connect();
                DebugOut = client.RunCommand("cd ~/omnipy && ./omni.py status").Result;
            }
        }
        public async Task SetTempBasal(decimal rate, decimal durationHours)
        {
            using (var client = new SshClient(Host, "pi", Password))
            {
                client.HostKeyReceived += (sender, e) =>
                {
                    e.CanTrust = true;
                };
                client.Connect();
                DebugOut = client.RunCommand($"cd ~/omnipy && ./omni.py tempbasal {rate.ToString(CultureInfo.InvariantCulture)} {durationHours.ToString(CultureInfo.InvariantCulture)}").Result;
            }
        }

        public async Task CancelTempBasal()
        {
            using (var client = new SshClient(Host, "pi", Password))
            {
                client.HostKeyReceived += (sender, e) =>
                {
                    e.CanTrust = true;
                };
                client.Connect();
                DebugOut = client.RunCommand("cd ~/omnipy && ./omni.py canceltempbasal").Result;
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
