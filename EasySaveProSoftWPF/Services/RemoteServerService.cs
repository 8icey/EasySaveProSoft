using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySaveProSoft.WPF.ViewModels;
using Newtonsoft.Json;

namespace EasySaveProSoft.Services
{
    public class RemoteServerService
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts = new();
        private readonly BackupJobsViewModel _viewModel;

        public RemoteServerService(BackupJobsViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Start(int port = 9000)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            Console.WriteLine($"[RemoteServer] Listening on port {port}");

            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClient(client);
                }
            });
        }

        private async Task HandleClient(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            while (client.Connected && !_cts.Token.IsCancellationRequested)
            {
                // Send status (optional: fill in progress info)
                string jobsJson = JsonConvert.SerializeObject(_viewModel.BackupJobs.Select(j => new
                {
                    j.Name,
                    j.Type,
                    j.LastBackupDate
                }));
                await writer.WriteLineAsync(jobsJson);

                if (stream.DataAvailable)
                {
                    string commandLine = await reader.ReadLineAsync();
                    _viewModel.HandleRemoteCommand(commandLine);
                }

                await Task.Delay(1000);
            }

            client.Close();
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
        }
    }
}
