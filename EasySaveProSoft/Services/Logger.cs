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

        // Logs details of each file copied during backup
        public void LogFileTransfer(FileItem fileItem)
        {
            try
            {
                // Construct a structured log entry with all relevant data
                var logEntry = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    SourcePath = fileItem.SourcePath,
                    DestinationPath = fileItem.DestinationPath,
                    Size = fileItem.Size,
                    TransferTime = fileItem.TransferTime.TotalMilliseconds,
                    IsSuccess = fileItem.IsSuccess
                };

                // Serialize the log entry to JSON
                string json = JsonConvert.SerializeObject(logEntry, Formatting.Indented);

                // Append the entry to the log file (with a trailing comma for line separation)
                File.AppendAllText(_logFilePath, json + ",\n");

                Console.WriteLine($"[LOG] File transfer recorded: {fileItem.SourcePath} → {fileItem.DestinationPath}");
            }
            catch (Exception ex)
            {
                // If logging fails, print the error to console (fail silently otherwise)
                Console.WriteLine($"[ERROR] Failed to log file transfer: {ex.Message}");
            }
        }

        // Logs exceptions to the error log
        public void LogError(Exception ex)
        {
            try
            {
                // Prepare an error object with timestamp, message, and stack trace
                var errorLog = new
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                };

                // Serialize and append to error log file
                string json = JsonConvert.SerializeObject(errorLog, Formatting.Indented);
                File.AppendAllText(_errorLogFilePath, json + ",\n");

                Console.WriteLine($"[ERROR LOGGED] {ex.Message}");
            }
            catch (Exception loggingEx)
            {
                // If error logging itself fails, show a last-resort message
                Console.WriteLine($"[LOGGER FAILURE] Could not write to error log: {loggingEx.Message}");
            }
        }
    }
}
