using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;

namespace OmniPie.Api
{
    public class OmniPySshClient : SshClient
    {
        private ISubject<bool> CanConnectSubject;
        public static async Task<OmniPySshClient> Connect(string host, string password, ISubject<bool> canConnectSubject)
        {
            var client = new OmniPySshClient(host, "pi", password)
            {
                CanConnectSubject = canConnectSubject
            };

            canConnectSubject.OnNext(false);
            try
            {
                var connectTask = Task.Run(() => { client.Connect(); });
                var which = await Task.WhenAny(connectTask, Task.Delay(15000));
                if (which == connectTask)
                    return client;
                throw new OperationCanceledException();
            }
            catch (Exception)
            {
                canConnectSubject.OnNext(true);
                client.Dispose();
                throw;
            }
        }

        private OmniPySshClient(string host, string username, string password) : base(host, username, password)
        {
        }

        public async Task<string> RunCommandAsync(string commandText)
        {
            return await Task.Run(() =>
            {
                var command = RunCommand(commandText);
                if (string.IsNullOrEmpty(command.Error))
                    return command.Result;
                throw new Exception($"Error executing command: {command.Error}");
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CanConnectSubject.OnNext(true);
        }
    }
}
