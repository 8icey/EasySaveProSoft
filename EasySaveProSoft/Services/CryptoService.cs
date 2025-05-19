using System;
using System.Diagnostics;
using System.IO;

namespace EasySaveProSoft.Services
{
    public class CryptoService
    {
        private const byte XOR_KEY = 0xAA;

        // ✅ NEW: returns encryption duration in milliseconds
        public bool EncryptFile(string sourceFile, string destinationFile, out double encryptionTimeMs)
        {
            encryptionTimeMs = 0;

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                byte[] fileBytes = File.ReadAllBytes(sourceFile);

                for (int i = 0; i < fileBytes.Length; i++)
                {
                    fileBytes[i] ^= XOR_KEY;
                }

                File.WriteAllBytes(destinationFile, fileBytes);

                stopwatch.Stop();
                encryptionTimeMs = stopwatch.Elapsed.TotalMilliseconds;

                Console.WriteLine($"[+] File encrypted and saved to {destinationFile}");
                return true;
            }
            catch (Exception ex)
            {
                encryptionTimeMs = -1; // ❌ Indicates encryption error
                Console.WriteLine($"[ERROR] Encryption failed: {ex.Message}");
                return false;
            }
        }
    }
}


