using System;
using System.Collections.Generic;
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

        public void Execute()
        {
            Console.WriteLine($"[+] Executing {Type} Backup for {Name}...");

            if (!Directory.Exists(SourcePath) || !Directory.Exists(TargetPath))
            {
                Console.WriteLine("[!] Source or target path not found.");
                return;
            }

            var files = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories);
            Console.WriteLine($"[+] Found {files.Length} files to process...");

            foreach (var file in files)
            {
                string relativePath = Path.GetRelativePath(SourcePath, file);
                string destinationFile = Path.Combine(TargetPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile) ?? string.Empty);

                if (Type == BackupType.Full || (Type == BackupType.Differential && IsNewer(file, destinationFile)))
                {
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    File.Copy(file, destinationFile, true);
                    timer.Stop();

                    var fileItem = new FileItem
                    {
                        SourcePath = file,
                        DestinationPath = destinationFile,
                        Size = new FileInfo(file).Length,
                        TransferTime = timer.Elapsed,
                        IsSuccess = true
                    };

                    _logger.LogFileTransfer(fileItem);
                    Console.WriteLine($"[✓] Copied: {file} → {destinationFile}");
                }
            }

            LastBackupDate = DateTime.Now;
            Console.WriteLine($"[+] Backup completed at {LastBackupDate}.");
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
    }
}
