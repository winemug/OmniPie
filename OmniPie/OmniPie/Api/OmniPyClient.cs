using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace OmniPie.Api
{
    public class OmniPyClient
    {
        private string Host;
        private string Password;

        private ISubject<bool> ClientCanConnectSubject = new BehaviorSubject<bool>(false);

        public IObservable<bool> WhenClientCanConnectChanged() => ClientCanConnectSubject.AsObservable();

        public void SetConnectionInfo(string host, string password)
        {
            Host = host;
            Password = password;
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(password))
                ClientCanConnectSubject.OnNext(false);
            else
                ClientCanConnectSubject.OnNext(true);
        }

        private Task<OmniPySshClient> GetClient()
        {
            return OmniPySshClient.Connect(Host, Password, ClientCanConnectSubject);
        }

        public async Task<string> VerifyConnection()
        {
            try
            {
                using (var _ = await GetClient())
                {
                    return "Connection OK.";
                }
            }
            catch (Exception e)
            {
                return $"Connection failed with error: {e}";
            }
        }

        public async Task<string> UpdateStatus()
        {
            using (var client = await GetClient())
            {
                return await client.RunCommandAsync("cd ~/omnipy && ./omni.py status");
            }
        }
        public async Task<string> SetTempBasal(decimal rate, decimal durationHours)
        {
            using (var client = await GetClient())
            {
                return await client.RunCommandAsync($"cd ~/omnipy && ./omni.py tempbasal {rate.ToString(CultureInfo.InvariantCulture)} {durationHours.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        public async Task<string> CancelTempBasal()
        {
            using (var client = await GetClient())
            {
                return await client.RunCommandAsync($"cd ~/omnipy && ./omni.py canceltempbasal");
            }
        }
    }
}
