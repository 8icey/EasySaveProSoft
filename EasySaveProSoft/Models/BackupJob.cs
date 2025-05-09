using System;
using System.Collections.Generic;
using System.IO;
using EasySaveProSoft.Helpers;
using EasySaveProSoft.Models;
using EasySaveProSoft.Services;

namespace EasySaveProSoft.Models
{
    // Represents a backup job
    public class BackupJob
    {
        // Properties of the backup job
        public string Name { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public BackupType Type { get; set; }
        public DateTime LastBackupDate { get; set; }

        private readonly Logger _logger = new Logger(); // Logger instance

        // Runs the backup job
        public void Execute()
        {
            Console.WriteLine($"[DEBUG] Starting Execute() for job: {Name}");

            // Validate source path
            if (!Directory.Exists(SourcePath))
            {
                string msg = $"Source path does not exist: {SourcePath}";
                Console.WriteLine($"[ERROR] {msg}");
                try { _logger.LogError(new DirectoryNotFoundException(msg)); }
                catch (Exception ex) { Console.WriteLine($"[FATAL] Logging failed: {ex.Message}"); }
                return;
            }

            // Validate target path
            if (!Directory.Exists(TargetPath))
            {
                string msg = $"Target path does not exist: {TargetPath}";
                Console.WriteLine($"[ERROR] {msg}");
                try { _logger.LogError(new DirectoryNotFoundException(msg)); }
                catch (Exception ex) { Console.WriteLine($"[FATAL] Logging failed: {ex.Message}"); }
                return;
            }

            // Get all files recursively from source
            string[] files;
            try
            {
                files = Directory.GetFiles(SourcePath, "*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to get files: {ex.Message}");
                try { _logger.LogError(ex); }
                catch (Exception logEx) { Console.WriteLine($"[FATAL] Logging failed: {logEx.Message}"); }
                return;
            }

            Console.WriteLine($"[+] Found {files.Length} files to process...");

            // Process each file
            foreach (var file in files)
            {
                string relativePath = Path.GetRelativePath(SourcePath, file);
                string destinationFile = Path.Combine(TargetPath, relativePath);

                try
                {
                    // Ensure target directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile) ?? string.Empty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Could not create directory for {destinationFile}: {ex.Message}");
                    try { _logger.LogError(ex); }
                    catch (Exception logEx) { Console.WriteLine($"[FATAL] Logging failed: {logEx.Message}"); }
                    continue;
                }

                // Check whether to copy (full or newer in differential)
                if (Type == BackupType.Full || (Type == BackupType.Differential && IsNewer(file, destinationFile)))
                {
                    TimeSpan elapsed = TimeSpan.Zero;
                    bool success = false;

                    try
                    {
                        elapsed = TimerUtil.MeasureExecution(() =>
                        {
                            if (File.Exists(destinationFile))
                                File.SetAttributes(destinationFile, FileAttributes.Normal);

                            File.Copy(file, destinationFile, true);
                        });

                        success = true;
                        Console.WriteLine($"[✓] Copied: {file} → {destinationFile}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to copy {file}: {ex.Message}");
                        try { _logger.LogError(ex); }
                        catch (Exception logEx) { Console.WriteLine($"[FATAL] Logging failed: {logEx.Message}"); }
                    }

                    // Log transfer details
                    var fileItem = new FileItem
                    {
                        SourcePath = file,
                        DestinationPath = destinationFile,
                        Size = new FileInfo(file).Length,
                        TransferTime = elapsed,
                        IsSuccess = success
                    };

                    _logger.LogFileTransfer(fileItem);
                }
            }

            // Finalize
            LastBackupDate = DateTime.Now;
            Console.WriteLine($"[+] Backup completed at {LastBackupDate}.");
        }

        // Compares file modification timestamps to decide if file should be copied
        private bool IsNewer(string sourceFile, string destinationFile)
        {
            try
            {
                if (!File.Exists(destinationFile))
                    return true;

                DateTime sourceModified = File.GetLastWriteTime(sourceFile);
                DateTime targetModified = File.GetLastWriteTime(destinationFile);

                return sourceModified > targetModified;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Could not compare timestamps: {ex.Message}");
                try { _logger.LogError(ex); }
                catch (Exception logEx) { Console.WriteLine($"[FATAL] Logging failed: {logEx.Message}"); }
                return false;
            }
        }

        // Validates both paths exist
        public bool IsValid()
        {
            return Directory.Exists(SourcePath) && Directory.Exists(TargetPath);
        }
    }

}
