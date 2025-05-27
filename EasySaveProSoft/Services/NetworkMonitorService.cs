using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace EasySaveProSoft.Services
{
    public class NetworkMonitorService
    {
        private static long _bytesReceivedPerSecond = 0;
        private static long _bytesSentPerSecond = 0;
        private static readonly int _checkIntervalMs = 1000;

        public static long BytesReceivedPerSec => _bytesReceivedPerSecond;
        public static long BytesSentPerSec => _bytesSentPerSecond;

        private static bool _monitoring = false;

        public static void StartMonitoring()
        {
            if (_monitoring) return;
            _monitoring = true;

            Task.Run(async () =>
            {
                while (_monitoring)
                {
                    try
                    {
                        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                            .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                         ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                        long receivedBefore = interfaces.Sum(ni => ni.GetIPv4Statistics().BytesReceived);
                        long sentBefore = interfaces.Sum(ni => ni.GetIPv4Statistics().BytesSent);

                        await Task.Delay(_checkIntervalMs);

                        long receivedAfter = interfaces.Sum(ni => ni.GetIPv4Statistics().BytesReceived);
                        long sentAfter = interfaces.Sum(ni => ni.GetIPv4Statistics().BytesSent);

                        _bytesReceivedPerSecond = (receivedAfter - receivedBefore);
                        _bytesSentPerSecond = (sentAfter - sentBefore);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[NetworkMonitor] Error: {ex.Message}");
                    }
                }
            });
        }

        public static void StopMonitoring()
        {
            _monitoring = false;
        }

        public static bool IsNetworkOverloaded()
        {
            // Lire les paramètres utilisateur (en KB/s)
            int thresholdKb = AppConfig.GetNetworkThreshold(); // ex. 1000 KB/s
            long totalKb = (_bytesReceivedPerSecond + _bytesSentPerSecond) / 1024;

            return totalKb >= thresholdKb;
        }

        public static string GetCurrentUsageText()
        {
            return $"{BytesReceivedPerSec / 1024} KB/s ↓ | {BytesSentPerSec / 1024} KB/s ↑";
        }
    }
}
