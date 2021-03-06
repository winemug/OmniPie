﻿using System;
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
    public class OmniPyViewModel : BaseViewModel
    {
        public string Host { get; set; }
        public string Password { get; set; }

        public ICommand LocateCommand { get; set; }
        public bool LocateEnabled { get; set; }
        public ICommand VerifyConnectionCommand { get; set; }
        public ICommand PingCommand { get; set; }
        public bool PingEnabled { get; set; }
        
        public ICommand VerifyRileyLinkCommand { get; set; }
        public ICommand RestartCommand { get; set; }
        public ICommand ShutdownCommand { get; set; }

        public OmniPyViewModel(Page page) : base(page)
        {
            LocateCommand = new Command(async () => await Locate(), () => true);
            LocateEnabled = true;
            PingEnabled = false;
            
            PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(Host):
                    case nameof(Password):
                        Preferences.Set(ConfigurationConstants.OmniPyAddress, Host);
                        Preferences.Set(ConfigurationConstants.OmniPyPassword, Password);
                        Client.SetConnectionInfo(Host, Password);
                        PingEnabled = !string.IsNullOrEmpty(Host);
                        break;
                }
            };

            Host = Preferences.Get(ConfigurationConstants.OmniPyAddress, "");
            Password = Preferences.Get(ConfigurationConstants.OmniPyPassword, "omnipy");
            Client.SetConnectionInfo(Host, Password);

            VerifyConnectionCommand = new Command(async() => DebugOut = await Client.VerifyConnection());
            PingCommand = new Command(async () => DebugOut = await Ping());
            ShutdownCommand = new Command(async () => DebugOut = await Client.Shutdown());
            RestartCommand = new Command(async () => DebugOut = await Client.Restart());
            VerifyRileyLinkCommand = new Command(async() => DebugOut = await Client.VerifyRileyLink());
        }

        private async Task Locate()
        {
            ClientCanConnect = false;
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
                ClientCanConnect = await Client.WhenClientCanConnectChanged().FirstAsync();
                LocateEnabled = true;
            }
        }

        private async Task<string> Ping()
        {
            PingEnabled = false;
            var ping = new Ping();
            try
            {
                var pr = await ping.SendPingAsync(Host, 3000);
                return $"Ping reply received in {pr.RoundtripTime} ms";
            }
            catch (Exception e)
            {
                return $"No ping reply received. Error: {e}";
            }
            finally
            {
                PingEnabled = true;
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
