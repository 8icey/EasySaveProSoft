using System;
using System.IO;
using Newtonsoft.Json;
using EasySaveProSoft.Models;

namespace EasySaveProSoft.Services
{
    // Logger class handles writing logs to disk:
    // - Daily logs for successful file transfers
    // - Error logs for exceptions during backup operations
    public class Logger
    {
        // Path for the main log file (successful transfers)
        private readonly string _logFilePath = "DailyLog.json";

        // Path for error log file (exceptions and failures)
        private readonly string _errorLogFilePath = "ErrorLog.json"; // Stored in same directory as DailyLog

        // 🔧 Format config
        private readonly string _formatPath = "logformat.txt";

        private string GetLogFormat()
        {
            if (!File.Exists(_formatPath))
                return "json"; // default

            string format = File.ReadAllText(_formatPath).Trim().ToLower();
            return (format == "xml" || format == "json") ? format : "json";
        }

        public void SetLogFormat(string format)
        {
            if (format == "json" || format == "xml")
                File.WriteAllText(_formatPath, format);
        }

        // Logs details of each file copied during backup
        public void LogFileTransfer(FileItem fileItem)
        {
            try
            {
                var format = GetLogFormat();

                // Construct a structured log entry with all relevant data
                var logEntry = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    SourcePath = fileItem.SourcePath,
                    DestinationPath = fileItem.DestinationPath,
                    Size = fileItem.Size,
                    TransferTime = fileItem.TransferTime.TotalMilliseconds,
                    IsSuccess = fileItem.IsSuccess,
                    EncryptionTime = fileItem.EncryptionTimeMs 
                };


                if (format == "xml")
                {
                    using (var writer = new StreamWriter("DailyLog.xml", true))
                    {
                        writer.WriteLine("<Log>");
                        writer.WriteLine($"  <Timestamp>{logEntry.Timestamp}</Timestamp>");
                        writer.WriteLine($"  <Source>{logEntry.SourcePath}</Source>");
                        writer.WriteLine($"  <Destination>{logEntry.DestinationPath}</Destination>");
                        writer.WriteLine($"  <Size>{logEntry.Size}</Size>");
                        writer.WriteLine($"  <TransferTime>{logEntry.TransferTime}</TransferTime>");
                        writer.WriteLine($"  <IsSuccess>{logEntry.IsSuccess}</IsSuccess>");
                        writer.WriteLine("</Log>");
                    }
                }
                else
                {
                    string json = JsonConvert.SerializeObject(logEntry, Formatting.Indented);
                    File.AppendAllText(_logFilePath, json + ",\n");
                }

                Console.WriteLine($"[LOG] File transfer recorded in {format.ToUpper()} format.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to log file transfer: {ex.Message}");
            }
        }

        // Logs exceptions to the error log
        public void LogError(Exception ex)
        {
            try
            {
                var errorLog = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                };

                string json = JsonConvert.SerializeObject(errorLog, Formatting.Indented);
                File.AppendAllText(_errorLogFilePath, json + ",\n");

                Console.WriteLine($"[ERROR LOGGED] {ex.Message}");
            }
            catch (Exception loggingEx)
            {
                Console.WriteLine($"[LOGGER FAILURE] Could not write to error log: {loggingEx.Message}");
            }
        }

        // ✅ NEW: Logs job execution status in a separate file per job
        public void LogJobStatus(BackupJob job, bool executed)
        {
            try
            {
                var statusEntry = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    JobName = job.Name,
                    SourcePath = job.SourcePath,
                    DestinationPath = job.TargetPath,
                    Status = executed ? "Executed" : "Not Executed"
                };

                var format = GetLogFormat();
                string filename = $"JobStatus_{job.Name.Replace(" ", "_")}";

                if (format == "xml")
                {
                    filename += ".xml";
                    using (var writer = new StreamWriter(filename, false))
                    {
                        writer.WriteLine("<JobStatus>");
                        writer.WriteLine($"  <Timestamp>{statusEntry.Timestamp}</Timestamp>");
                        writer.WriteLine($"  <JobName>{statusEntry.JobName}</JobName>");
                        writer.WriteLine($"  <SourcePath>{statusEntry.SourcePath}</SourcePath>");
                        writer.WriteLine($"  <DestinationPath>{statusEntry.DestinationPath}</DestinationPath>");
                        writer.WriteLine($"  <Status>{statusEntry.Status}</Status>");
                        writer.WriteLine("</JobStatus>");
                    }
                }
                else
                {
                    filename += ".json";
                    string json = JsonConvert.SerializeObject(statusEntry, Formatting.Indented);
                    File.WriteAllText(filename, json);
                }

                Console.WriteLine($"[✓] Job status logged to {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to log job status: {ex.Message}");
            }
        }
    }
}
