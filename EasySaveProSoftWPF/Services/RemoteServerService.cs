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
        private readonly List<TcpClient> _clients = new();


        public RemoteServerService(BackupJobsViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private void BroadcastJobStatus(object? state)
        {
            foreach (var client in _clients.ToList())
            {
                try
                {
                    using var stream = client.GetStream();
                    using var writer = new StreamWriter(stream) { AutoFlush = true };

                    if (_hasSentInitialList == false)
                    {
                        foreach (var job in _viewModel.BackupJobs)
                        {
                            writer.WriteLine($"[Job] {job.Name} - {job.Type}");
                        }
                        _hasSentInitialList = true;
                    }

                    // Then, only send meaningful progress updates:
                    if (_viewModel.ProgressValue > _lastProgress)
                    {
                        writer.WriteLine($"[Progress] {Math.Round(_viewModel.ProgressValue)}%");
                        _lastProgress = _viewModel.ProgressValue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[!] Broadcast error: {ex.Message}");
                    _clients.Remove(client);
                }
            }
        }

        private bool _hasSentInitialList = false;
        private double _lastProgress = 0;

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

        //private async Task HandleClient(TcpClient client)
        //{
        //    using var stream = client.GetStream();
        //    using var reader = new StreamReader(stream, Encoding.UTF8);
        //    using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        //    while (client.Connected && !_cts.Token.IsCancellationRequested)
        //    {
        //        // Send status (optional: fill in progress info)
        //        string jobsJson = JsonConvert.SerializeObject(_viewModel.BackupJobs.Select(j => new
        //        {
        //            j.Name,
        //            j.Type,
        //            j.LastBackupDate
        //        }));
        //        await writer.WriteLineAsync(jobsJson);

        //        if (stream.DataAvailable)
        //        {
        //            string commandLine = await reader.ReadLineAsync();
        //            _viewModel.HandleRemoteCommand(commandLine);
        //        }

        //        await Task.Delay(1000);
        //    }

        //    client.Close();
        //}
        private async Task HandleClient(TcpClient client)
        {
            _clients.Add(client); // Track this client

            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Send job list only once on connect
            string jobsJson = JsonConvert.SerializeObject(_viewModel.BackupJobs.Select(j => new
            {
                j.Name,
                j.Type,
                j.LastBackupDate
            }));
            await writer.WriteLineAsync(jobsJson);

            // Loop to listen for remote commands
            try
            {
                while (client.Connected && !_cts.Token.IsCancellationRequested)
                {
                    if (stream.DataAvailable)
                    {
                        string commandLine = await reader.ReadLineAsync();
                        if (!string.IsNullOrWhiteSpace(commandLine))
                        {
                            Console.WriteLine($"[Remote] Received: {commandLine}");
                            _viewModel.HandleRemoteCommand(commandLine);
                        }
                    }

                    await Task.Delay(200); // Small delay to prevent CPU hogging
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Remote] Client error: {ex.Message}");
            }
            finally
            {
                client.Close();
                _clients.Remove(client);
            }
        }




        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
        }
    }
}
