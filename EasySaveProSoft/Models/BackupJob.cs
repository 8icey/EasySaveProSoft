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

            Console.WriteLine($"[+] Found {totalFiles} files to process ({FormatSize(totalSize)})...");

            Stopwatch globalTimer = Stopwatch.StartNew();
            int currentFile = 0;

            // 👇 Save the current cursor position
            int progressBarLine = Console.CursorTop;
            Console.WriteLine(); // Create an empty line for the progress bar

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

                        // ✅ Real-time update in the same line
                        currentFile++;
                        DisplayProgress(currentFile, totalFiles, transferredSize, totalSize, globalTimer.Elapsed, progressBarLine);

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

        private void DisplayProgress(int current, int total, long transferred, long totalSize, TimeSpan elapsed, int progressBarLine)
        {
            // Calculate progress
            double percent = (double)current / total * 100;

            // Calculate estimated time remaining
            double avgTimePerFile = elapsed.TotalSeconds / current;
            double estimatedRemainingTime = avgTimePerFile * (total - current);

            // Draw progress bar
            int barWidth = 30;
            int progressBlocks = (int)((percent / 100) * barWidth);

            // Prepare the progress line
            string progressLine = $"[{new string('=', progressBlocks)}{new string(' ', barWidth - progressBlocks)}] " +
                                  $"{percent:F1}% | {FormatSize(transferred)}/{FormatSize(totalSize)} | ETA: {TimeSpan.FromSeconds(estimatedRemainingTime):mm\\:ss}";

            // **Move cursor back up to the progress bar line**
            Console.SetCursorPosition(0, progressBarLine);
            Console.Write(progressLine.PadRight(Console.WindowWidth - 1));
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

        // Validates both paths exist
        public bool IsValid()
        {
            return Directory.Exists(SourcePath) && Directory.Exists(TargetPath);
        } 

    }
}


