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
            // Convert the list of jobs to JSON format with indentation for readability
            string json = JsonConvert.SerializeObject(jobs, Newtonsoft.Json.Formatting.Indented);

            // Overwrite the file with the latest job list
            File.WriteAllText(_filePath, json);

            Console.WriteLine("[✓] Backup jobs saved to backupWorks.json");
        }

        // Loads the list of backup jobs from the JSON file
        public List<BackupJob> LoadJobs()
        {
            // If the file does not exist, return an empty list
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("[!] No previous backup jobs found.");
                return new List<BackupJob>();
            }

            // Read and deserialize the JSON content into a list of BackupJob objects
            string json = File.ReadAllText(_filePath);
            var jobs = JsonConvert.DeserializeObject<List<BackupJob>>(json);

            Console.WriteLine("[✓] Loaded previous backup jobs from backupWorks.json");

            // If deserialization fails and returns null, return an empty list
            return jobs ?? new List<BackupJob>();
        }
    }
}
