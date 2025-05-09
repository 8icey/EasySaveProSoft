using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using EasySaveProSoft.Models;
using System.Xml;

namespace EasySaveProSoft.Services
{
    // Responsible for saving and loading BackupJob data to/from a JSON file
    public class JsonHandler
    {
        // File path where jobs will be stored
        private readonly string _filePath = "backupWorks.json";

        // Serializes the list of backup jobs and saves it to a file
        public void SaveJobs(List<BackupJob> jobs)
        {
            string json = JsonConvert.SerializeObject(jobs, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_filePath, json);
            Console.WriteLine("[✓] Backup jobs saved to backupWorks.json");
        }

        // Loads the list of backup jobs from the JSON file
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

        //  **NEW** — Deletes the job from JSON after execution
        public void DeleteJob(string jobName, List<BackupJob> jobs)
        {
            jobs.RemoveAll(job => job.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase));
            SaveJobs(jobs); // Save changes back to the JSON file
            Console.WriteLine($"[✓] Backup job '{jobName}' has been deleted after execution.");
        }
    }
}
