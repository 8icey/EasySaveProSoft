using System;
using System.IO;


namespace EasySaveProSoft.Services
{
    public class CryptoService
    {
        private const byte XOR_KEY = 0xAA;

        public void EncryptFile(string sourceFile, string destinationFile)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(sourceFile);

                for (int i = 0; i < fileBytes.Length; i++)
                {
                    fileBytes[i] ^= XOR_KEY;
                }

                File.WriteAllBytes(destinationFile, fileBytes);
                Console.WriteLine($"[+] File encrypted and saved to {destinationFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Encryption failed: {ex.Message}");
            }
        }
    }
}
