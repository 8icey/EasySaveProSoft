using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using EasySaveProSoft.Services;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EasySaveProSoft.Models
{
    public class BackupJob
    {
        public static SemaphoreSlim LargeFileLock = new SemaphoreSlim(1, 1); // Max 1 large file at a time
        public static long LargeFileThresholdBytes = 1L * 1024 * 1024 * 1024; // 1 GB

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
                // ✅ Initial blocked software check
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

                Stopwatch globalTimer = Stopwatch.StartNew();
                int currentFile = 0;

                foreach (var file in files)
                {
                    // 🔹 Check for cancellation
                    token.ThrowIfCancellationRequested();

                    // ⏸ Pause support
                    pauseEvent.Wait();

                    // 🔄 Blocked software re-check during execution
                    await WaitWhileBlockedAsync();

                    string relativePath = Path.GetRelativePath(SourcePath, file);
                    string destinationFile = Path.Combine(TargetPath, relativePath);
                    long fileSize = new FileInfo(file).Length;
                    bool isLarge = fileSize >= LargeFileThresholdBytes;

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile) ?? string.Empty);

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
                //_logger.LogInfo("Backup was canceled by the user.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL ERROR] {ex.Message}");
                //_logger.LogError("Fatal error during backup: " + ex.Message);
                return false;
            }
        }


        // 🔄 Pauses file execution until all blocked software is closed
        private async Task WaitWhileBlockedAsync()
        {
            while (SoftwareDetector.IsBlockedSoftwareRunning())
            {
                string blocked = SoftwareDetector.GetFirstBlockedProcess();
                Console.WriteLine($"⏸️ Paused due to: {blocked}");
                await Task.Delay(1000); // Recheck every second
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
