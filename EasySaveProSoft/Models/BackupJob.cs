using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using EasySaveProSoft.Services;
//using EasySaveProSoft.WPF.Services;
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
        //private SoftwareDetector = new SoftwareDetector();
        public event Action<double, string, string> OnProgressUpdated;

        public BackupJob()
        {
            LoadEncryptedExtensions();
        }

        public bool Execute()
        {
            // ✅ Check for blocked software

            if (SoftwareDetector.IsBlockedSoftwareRunning())
            {
                string blocked = SoftwareDetector.GetFirstBlockedProcess();
                Console.WriteLine($"[BLOCKED] Cannot execute while '{blocked}' is running.");
                return false;
            }



            if (!Directory.Exists(SourcePath) || !Directory.Exists(TargetPath))
                return true;

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
                    double encryptionTimeMs = 0;
                    bool success = true;

                    try
                    {
                        if (ShouldEncrypt(file))
                        {
                            success = _cryptoService.EncryptFile(file, destinationFile, out encryptionTimeMs);
                        }
                        else
                        {
                            File.Copy(file, destinationFile, true);
                            encryptionTimeMs = 0;
                        }

                        timer.Stop();

                        long fileSize = new FileInfo(file).Length;
                        transferredSize += fileSize;

                        _logger.LogFileTransfer(new FileItem
                        {
                            SourcePath = file,
                            DestinationPath = destinationFile,
                            Size = fileSize,
                            TransferTime = timer.Elapsed,
                            IsSuccess = success,
                            EncryptionTimeMs = encryptionTimeMs
                        });

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
            return true;
        }

        private bool ShouldEncrypt(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return _extensionsToEncrypt.Contains(extension);
        }

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
