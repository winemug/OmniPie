using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace OmniPie.Api
{
    public class OmniPySftpClient : SftpClient
    {

        private ISubject<bool> CanConnectSubject;
        public static async Task<OmniPySftpClient> Connect(string host, string password, ISubject<bool> canConnectSubject)
        {
            var client = new OmniPySftpClient(host, "pi", password)
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
        private OmniPySftpClient(string host, string username, string password) : base(host, username, password)
        {
        }

        public async Task DownloadAsync(string sourcePath, string targetPath)
        {
            await Task.Run(() =>
            {
                using (var fs = new FileStream(targetPath, FileMode.Create))
                {
                    DownloadFile(sourcePath, fs);
                }
            });
        }
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CanConnectSubject.OnNext(true);
        }
    }
}
