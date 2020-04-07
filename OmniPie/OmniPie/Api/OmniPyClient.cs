using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Renci.SshNet;

namespace OmniPie.Api
{
    public class OmniPyClient
    {
        private string Host;
        private string Password;

        private ISubject<bool> ClientCanConnectSubject = new BehaviorSubject<bool>(false);

        public IObservable<bool> WhenClientCanConnectChanged() => ClientCanConnectSubject.AsObservable();

        private OmniPyClient()
        {
        }

        private static OmniPyClient Instance = new OmniPyClient();
        public static OmniPyClient Get()
        {
            return Instance;
        }
        public void SetConnectionInfo(string host, string password)
        {
            Host = host;
            Password = password;
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(password))
                ClientCanConnectSubject.OnNext(false);
            else
                ClientCanConnectSubject.OnNext(true);
        }

        private Task<OmniPySshClient> GetSshClient()
        {
            return OmniPySshClient.Connect(Host, Password, ClientCanConnectSubject);
        }

        private Task<OmniPySftpClient> GetSftpClient()
        {
            return OmniPySftpClient.Connect(Host, Password, ClientCanConnectSubject);
        }

        public async Task<string> VerifyConnection()
        {
            try
            {
                using (var _ = await GetSshClient())
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
            using (var client = await GetSshClient())
            {
                return await client.RunCommandAsync("cd ~/omnipy && ./omni.py status");
            }
        }
        public async Task<string> SetTempBasal(decimal rate, decimal durationHours)
        {
            using (var client = await GetSshClient())
            {
                return await client.RunCommandAsync($"cd ~/omnipy && ./omni.py tempbasal {rate.ToString(CultureInfo.InvariantCulture)} {durationHours.ToString(CultureInfo.InvariantCulture)}");
            }
        }

        public async Task<string> CancelTempBasal()
        {
            using (var client = await GetSshClient())
            {
                return await client.RunCommandAsync($"cd ~/omnipy && ./omni.py canceltempbasal");
            }
        }

        private string DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "omnipy.db");

        public async Task<string> DownloadHistory()
        {
            using (var client = await GetSftpClient())
            {
                await client.DownloadAsync("/home/pi/omnipy/data/pod.db", DbPath);
                var fi = new FileInfo(DbPath);
                return $"Downloaded {fi.Length} bytes.";
            }
        }

        public async Task<IEnumerable<OmniPyHistoryEntry>> ReadHistory()
        {
            var entries = new List<OmniPyHistoryEntry>();
            if (File.Exists(DbPath))
            {
                using (var conn = new SqliteConnection() {ConnectionString = $"Data Source={DbPath}"})
                {
                    await conn.OpenAsync();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"SELECT timestamp, pod_state, pod_minutes, pod_last_command,
                    insulin_delivered, insulin_canceled, insulin_reservoir FROM pod_history ORDER BY timestamp DESC";
                    var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        entries.Add(new OmniPyHistoryEntry
                        {
                            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long) reader.GetInt64(0)),
                            Progress = reader.GetInt32(1),
                            Minutes = reader.GetInt32(2),
                            Command = reader.GetString(3),
                            Delivered = reader.GetDecimal(4),
                            Canceled = reader.GetDecimal(5),
                            Reservoir = reader.GetDecimal(6)
                        });
                    }
                }
            }
            return entries;
        }
    }
}
