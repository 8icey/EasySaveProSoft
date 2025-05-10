using System;
using System.Collections.Generic;
using EasySaveProSoft.Models;
using EasySaveProSoft.Services;

namespace EasySaveProSoft.Models
{
    public class BackupManager 
    {
        public List<BackupJob> Jobs { get; private set; }
        private readonly Logger _logger = new Logger();
        private readonly JsonHandler _jsonHandler = new JsonHandler();
        private const int MaxJobs = 5;

        // Load jobs from persistent storage (JSON file)
        public BackupManager()
        {
            Jobs = _jsonHandler.LoadJobs();
        }

        // Add a new backup job and save to JSON
        public void AddJob(BackupJob job)
        {
            if (Jobs.Count >= MaxJobs)
            {
                Console.WriteLine("[!] Maximum number of backup jobs reached (5). Delete a job to add a new one.");
                return;
            }

            Jobs.Add(job);
            _jsonHandler.SaveJobs(Jobs);
            Console.WriteLine($"[+] Backup Job '{job.Name}' added successfully!");
        }

        // Find and execute a backup job by name, then delete it
        public void RunJob(string name)
        {
            var job = Jobs.Find(j => j.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (job != null)
            {
                job.Execute();
                _jsonHandler.DeleteJob(name, Jobs);  // Delete after execution
            }
            else
            {
                string msg = $"No backup job found with the name: {name}";
                Console.WriteLine($"[ERROR] {msg}");
                _logger.LogError(new KeyNotFoundException(msg));
            }
        }

        // Execute all backup jobs and delete each one after completion
        public void RunAllJobs()
        {
            if (Jobs.Count == 0)
            {
                Console.WriteLine("[!] No backup jobs found.");
            }
            else
            {
                var jobsToDelete = new List<string>();
                foreach (var job in Jobs)
                {
                    job.Execute();
                    jobsToDelete.Add(job.Name);
                }

                // Delete all jobs from JSON after execution
                foreach (var jobName in jobsToDelete)
                {
                    _jsonHandler.DeleteJob(jobName, Jobs);
                }
            }
        }

        // ✅ **NEW** — Display all backup jobs in the console
        public void DisplayJobs()
        {
            if (Jobs.Count == 0)
            {
                Console.WriteLine("[!] No backup jobs available.");
            }
            else
            {
                Console.WriteLine("=== List of Backup Jobs ===");
                foreach (var job in Jobs)
                {
                    Console.WriteLine($"- Name: {job.Name}");
                    Console.WriteLine($"  Source: {job.SourcePath}");
                    Console.WriteLine($"  Target: {job.TargetPath}");
                    Console.WriteLine($"  Type: {job.Type}\n");
                }
            }
        }
    }
}
