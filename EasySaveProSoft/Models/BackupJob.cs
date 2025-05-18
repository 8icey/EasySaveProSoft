using System;
using System.Diagnostics;
using System.IO;
using EasySaveProSoft.Services;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EasySaveProSoft.Models
{
    public class BackupJob
    {
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public BackupType Type { get; set; }
        public DateTime LastBackupDate { get; set; }

        private readonly Logger _logger = new Logger();
        private readonly CryptoService _cryptoService = new CryptoService();
        private List<string> _extensionsToEncrypt = new List<string>();
        
        public event Action<double, string, string> OnProgressUpdated;

        // ✅ New Constructor to load encryption extensions
        public BackupJob()
        {
            LoadEncryptedExtensions();
        }

        // 🔄 **Main Execution Logic**
        public void Execute()
        {
            if (!Directory.Exists(SourcePath) || !Directory.Exists(TargetPath))
                return;

            var files = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories);
            int totalFiles = files.Length;
            long totalSize = 0;
            long transferredSize = 0;

            foreach (var file in files)
            {
                totalSize += new FileInfo(file).Length;
            }

            Stopwatch globalTimer = Stopwatch.StartNew();
            int currentFile = 0;

            foreach (var file in files)
            {
                string relativePath = Path.GetRelativePath(SourcePath, file);
                string destinationFile = Path.Combine(TargetPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile) ?? string.Empty);

                if (Type == BackupType.Full || (Type == BackupType.Differential && IsNewer(file, destinationFile)))
                {
                    var timer = Stopwatch.StartNew();
                    try
                    {
                        if (ShouldEncrypt(file))
                        {
                            _cryptoService.EncryptFile(file, destinationFile);
                        }
                        else
                        {
                            File.Copy(file, destinationFile, true);
                        }

                        timer.Stop();

                        long fileSize = new FileInfo(file).Length;
                        transferredSize += fileSize;

                        currentFile++;
                        double progress = (double)currentFile / totalFiles * 100;

                        string sizeText = $"{FormatSize(transferredSize)} / {FormatSize(totalSize)}";
                        string eta = TimeSpan.FromSeconds((globalTimer.Elapsed.TotalSeconds / currentFile) * (totalFiles - currentFile)).ToString(@"hh\:mm\:ss");

                        OnProgressUpdated?.Invoke(progress, sizeText, eta);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Could not copy file: {file}");
                    }
                }
            }

            globalTimer.Stop();
            LastBackupDate = DateTime.Now;
        }

        // ✅ **Check if the file should be encrypted**
        private bool ShouldEncrypt(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return _extensionsToEncrypt.Contains(extension);
        }

        // ✅ **Load the extensions from config**
        private void LoadEncryptedExtensions()
        {
            try
            {
                if (File.Exists("EncryptionExtensions.json"))
                {
                    _extensionsToEncrypt = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("EncryptionExtensions.json"));
                    Console.WriteLine("[+] Loaded encrypted extensions from configuration.");
                }
                else
                {
                    Console.WriteLine("[!] No configuration found. Using defaults.");
                    _extensionsToEncrypt = new List<string> { ".txt", ".pdf", ".docx" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Could not load encryption extensions: {ex.Message}");
                _extensionsToEncrypt = new List<string> { ".txt", ".pdf", ".docx" };
            }
        }

        // ✅ **Check if the file is newer for Differential Backup**
        private bool IsNewer(string sourceFile, string destinationFile)
        {
            if (!File.Exists(destinationFile))
                return true;

            DateTime sourceModified = File.GetLastWriteTime(sourceFile);
            DateTime targetModified = File.GetLastWriteTime(destinationFile);

            return sourceModified > targetModified;
        }
        public bool IsValid()
        {
            return Directory.Exists(SourcePath) && Directory.Exists(TargetPath);
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }


    }


}
