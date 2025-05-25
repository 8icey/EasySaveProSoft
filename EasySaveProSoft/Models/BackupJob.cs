using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EasySaveProSoft.Services;
using Newtonsoft.Json;

namespace EasySaveProSoft.Models
{
    public class BackupJob
    {
        public static SemaphoreSlim LargeFileLock = new SemaphoreSlim(1, 1);
        public static long LargeFileThresholdBytes = 1L * 1024 * 1024 * 1024;

        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public BackupType Type { get; set; }
        public DateTime LastBackupDate { get; set; }

        private readonly Logger _logger = new Logger();
        private readonly CryptoService _cryptoService = new CryptoService();
        private List<string> _extensionsToEncrypt = new List<string>();

        public event Action<double, string, string> OnProgressUpdated;

        public BackupJob()
        {
            LoadEncryptedExtensions();
        }

        public async Task<bool> Execute(ManualResetEventSlim pauseEvent, CancellationToken token)
        {
            try
            {
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
                    totalSize += new FileInfo(file).Length;

                // 🔄 Étape 1 : Enregistrement des fichiers prioritaires
                var priorityExtensions = AppConfig.GetPriorityExtensions();
                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    if (priorityExtensions.Contains(ext))
                        PriorityFileCoordinator.RegisterPriorityFile(file);
                }

                Stopwatch globalTimer = Stopwatch.StartNew();
                int currentFile = 0;

                foreach (var file in files)
                {
                    token.ThrowIfCancellationRequested();
                    pauseEvent.Wait();

                    await WaitWhileBlockedAsync();

                    string relativePath = Path.GetRelativePath(SourcePath, file);
                    string destinationFile = Path.Combine(TargetPath, relativePath);
                    long fileSize = new FileInfo(file).Length;
                    bool isLarge = fileSize >= LargeFileThresholdBytes;

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile) ?? string.Empty);

                    // 🔄 Étape 2 : Bloquer les non-prioritaires s’il en reste
                    string fileExt = Path.GetExtension(file).ToLower();
                    if (!priorityExtensions.Contains(fileExt))
                    {
                        PriorityFileCoordinator.WaitUntilNoPriorityLeft();
                    }

                    if (Type == BackupType.Full || (Type == BackupType.Differential && IsNewer(file, destinationFile)))
                    {
                        var timer = Stopwatch.StartNew();
                        double encryptionTimeMs = 0;
                        bool success = true;

                        try
                        {
                            if (isLarge)
                                await LargeFileLock.WaitAsync();

                            if (ShouldEncrypt(file))
                            {
                                success = _cryptoService.EncryptFile(file, destinationFile, out encryptionTimeMs);
                            }
                            else
                            {
                                File.Copy(file, destinationFile, true);
                                encryptionTimeMs = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] Could not copy file: {file}\n{ex.Message}");
                            success = false;
                        }
                        finally
                        {
                            if (isLarge)
                                LargeFileLock.Release();
                        }

                        timer.Stop();
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

                        // 🔄 Étape 3 : Marquer comme traité
                        if (priorityExtensions.Contains(fileExt))
                        {
                            PriorityFileCoordinator.MarkAsProcessed(file);
                        }

                        currentFile++;
                        double progress = (double)currentFile / totalFiles * 100;
                        string sizeText = $"{FormatSize(transferredSize)} / {FormatSize(totalSize)}";
                        string eta = TimeSpan.FromSeconds(
                            (globalTimer.Elapsed.TotalSeconds / currentFile) * (totalFiles - currentFile)
                        ).ToString(@"hh\:mm\:ss");

                        OnProgressUpdated?.Invoke(progress, sizeText, eta);
                    }
                }

                globalTimer.Stop();
                LastBackupDate = DateTime.Now;
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[CANCELED] Backup was canceled.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL ERROR] {ex.Message}");
                return false;
            }
        }

        private async Task WaitWhileBlockedAsync()
        {
            while (SoftwareDetector.IsBlockedSoftwareRunning())
            {
                string blocked = SoftwareDetector.GetFirstBlockedProcess();
                Console.WriteLine($"⏸️ Paused due to: {blocked}");
                await Task.Delay(1000);
            }
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
