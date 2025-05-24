using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveRemoteConsole.Services
{
    public class RemoteClientService
    {
        private TcpClient _client;

        private NetworkStream _stream;

        public event Action<string> OnMessageReceived;

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(host, port);
                _stream = _client.GetStream();

                // Start receiving messages
                _ = Task.Run(ReceiveLoop);
                return true;
            }
            catch (Exception ex)
            {
                OnMessageReceived?.Invoke($"[Error] {ex.Message}");
                return false;
            }
        }

        public async Task SendAsync(string message)
        {
            if (_client?.Connected != true) return;
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            await _stream.WriteAsync(data, 0, data.Length);
        }

        private async Task ReceiveLoop()
        {
            using StreamReader reader = new StreamReader(_stream, Encoding.UTF8);
            while (_client.Connected)
            {
                string line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    OnMessageReceived?.Invoke(line);
                }
            }
        }
    }
}
