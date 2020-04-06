using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniPie.Definitions;
using OmniPie.Interfaces;
using Xamarin.Forms;

namespace OmniPie.Api
{
    public class OmniPyConnection
    {
        public string Address { get; private set; }
        private OmniPyKey Key;
        public OmniPyConnection(OmniPyKey key)
        {
            Key = key;
            Address = DependencyService.Get<IConfiguration>()
                .GetString(ConfigurationConstants.OmniPyAddress);
        }

        public async Task<OmniPyConnectionResult> VerifyConnection(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(Address))
            {
                try
                {
                    Address = await Discover(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    return OmniPyConnectionResult.DiscoveryFailed;
                }
            }

            return OmniPyConnectionResult.OK;
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

            while (true)
            {
                var client = new UdpClient(AddressFamily.InterNetwork)
                {
                    EnableBroadcast = true
                };

                var data= Encoding.ASCII.GetBytes("Oh dear.");
                await client.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Any, 0));

                var which = await Task.WhenAny(Task.Delay(5000), receiveTask, tcs.Task);
                if (which == receiveTask)
                {
                    var receiveResult = await receiveTask;
                    return receiveResult.RemoteEndPoint.ToString();
                }
                if (which == tcs.Task)
                {
                    await tcs.Task;
                }
            }
        }
    }
}
