using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using EasySaveProSoft.Models;
using System.Xml;

namespace EasySaveProSoft.Services
{
    public class JsonHandler
    {
        private readonly string _filePath = "backupWorks.json";

        public void SaveJobs(List<BackupJob> jobs)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(jobs, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(_filePath, json);
            Console.WriteLine("[✓] Backup jobs saved to backupWorks.json");
        }

        public List<BackupJob> LoadJobs()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("[!] No previous backup jobs found.");
                return new List<BackupJob>();
            }

            string json = File.ReadAllText(_filePath);
            var jobs = JsonConvert.DeserializeObject<List<BackupJob>>(json);
            Console.WriteLine("[✓] Loaded previous backup jobs from backupWorks.json");
            return jobs ?? new List<BackupJob>();
        }
    }
}
