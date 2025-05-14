using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EasySaveProSoft.Models;
using EasySaveProSoft.Services;

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

        // 🔥 Event to update progress
        public event Action<double> OnProgressUpdated;

        public void Execute()
        {
            Console.WriteLine($"\n[+] Executing {Type} Backup for '{Name}'...");
            if (!Directory.Exists(SourcePath) || !Directory.Exists(TargetPath))
            {
                Console.WriteLine("[!] Source or target path not found.");
                return;
            }

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
                        File.Copy(file, destinationFile, true);
                        timer.Stop();

                        long fileSize = new FileInfo(file).Length;
                        transferredSize += fileSize;

                        var fileItem = new FileItem
                        {
                            SourcePath = file,
                            DestinationPath = destinationFile,
                            Size = fileSize,
                            TransferTime = timer.Elapsed,
                            IsSuccess = true
                        };

                        _logger.LogFileTransfer(fileItem);

                        // ✅ Real-time update for WPF
                        currentFile++;
                        double progress = (double)currentFile / totalFiles * 100;

                        // 🔥 Trigger the event if there is a subscriber
                        OnProgressUpdated?.Invoke(progress);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Could not copy file: {file}");
                        Console.WriteLine($"[ERROR] Reason: {ex.Message}");
                    }
                }
            }

            globalTimer.Stop();
            LastBackupDate = DateTime.Now;
            Console.WriteLine($"\n[✓] Backup completed at {LastBackupDate = DateTime.Now}.");
        }

        private bool IsNewer(string sourceFile, string destinationFile)
        {
            if (!File.Exists(destinationFile))
                return true;

            DateTime sourceModified = File.GetLastWriteTime(sourceFile);
            DateTime targetModified = File.GetLastWriteTime(destinationFile);

            return sourceModified > targetModified;
        }

        // Validates both paths exist
        public bool IsValid()
        {
            return Directory.Exists(SourcePath) && Directory.Exists(TargetPath);
        }
    }
}
