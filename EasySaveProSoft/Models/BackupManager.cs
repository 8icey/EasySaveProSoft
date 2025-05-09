using System;
using System.Collections.Generic;
using EasySaveProSoft.Models;
using EasySaveProSoft.Services;

namespace EasySaveProSoft.Models
{
    // This class manages a list of backup jobs: adding, executing individually or all at once.
    public class BackupManager
    {
        // Stores the list of all backup jobs
        public List<BackupJob> Jobs { get; private set; }

        // Logger instance for error and file transfer logging
        private readonly Logger _logger = new Logger();

        // Handles saving/loading jobs to/from a JSON file
        private readonly JsonHandler _jsonHandler = new JsonHandler();

        // Constructor: loads jobs from persistent storage (JSON file) when the app starts
        public BackupManager()
        {
            Jobs = _jsonHandler.LoadJobs();
        }

        // Adds a new backup job and saves the updated list to disk
        public void AddJob(BackupJob job)
        {
            Jobs.Add(job);
            _jsonHandler.SaveJobs(Jobs);
            Console.WriteLine($"[+] Backup Job '{job.Name}' added successfully!");
        }

        // Finds and executes a backup job by name
        public void RunJob(string name)
        {
            // Case-insensitive search for the job by its name
            var job = Jobs.Find(j => j.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (job != null)
            {
                // If found, execute the job and save updated state (like LastBackupDate)
                job.Execute();
                _jsonHandler.SaveJobs(Jobs);
            }
            else
            {
                // If not found, notify user and log the error
                string msg = $"No backup job found with the name: {name}";
                Console.WriteLine($"[ERROR] {msg}");
                _logger.LogError(new KeyNotFoundException(msg));
            }
        }

        // Executes all backup jobs in the list
        public void RunAllJobs()
        {
            if (Jobs.Count == 0)
            {
                Console.WriteLine("[!] No backup jobs found.");
            }
            else
            {
                // Execute each job one by one
                foreach (var job in Jobs)
                {
                    job.Execute();
                }

                // Save the updated state after all jobs are done
                _jsonHandler.SaveJobs(Jobs);
            }
        }
    }
}
