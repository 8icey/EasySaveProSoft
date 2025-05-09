using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;  // Make sure you install Newtonsoft.Json from NuGet
using EasySaveProSoft.Models;
using System.Xml;

namespace EasySaveProSoft.Services
{
    public class Logger
    {
        private readonly string _logFilePath = "DailyLog.json";

        public void LogFileTransfer(FileItem fileItem)
        {
            var logEntry = new
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SourcePath = fileItem.SourcePath,
                DestinationPath = fileItem.DestinationPath,
                Size = fileItem.Size,
                TransferTime = fileItem.TransferTime.TotalMilliseconds,
                IsSuccess = fileItem.IsSuccess
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(logEntry, Newtonsoft.Json.Formatting.Indented);


            // Append to the JSON log file
            File.AppendAllText(_logFilePath, json + ",\n");

            Console.WriteLine($"[LOG] File transfer recorded: {fileItem.SourcePath} → {fileItem.DestinationPath}");
        }
    }
}
