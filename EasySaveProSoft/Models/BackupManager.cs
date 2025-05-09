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

        public BackupManager()
        {
            // Load jobs on startup
            Jobs = _jsonHandler.LoadJobs();
        }

        public void AddJob(BackupJob job)
        {
            Jobs.Add(job);
            _jsonHandler.SaveJobs(Jobs);
            Console.WriteLine($"[+] Backup Job '{job.Name}' added successfully!");
        }

        public void RunJob(string name)
        {
            var job = Jobs.Find(j => j.Name == name);
            if (job != null)
            {
                job.Execute();
                _jsonHandler.SaveJobs(Jobs);
            }
            else
            {
                Console.WriteLine($"[!] No job found with name: {name}");
            }
        }

        public void RunAllJobs()
        {
            if (Jobs.Count == 0)
            {
                Console.WriteLine("[!] No backup jobs found.");
            }
            else
            {
                foreach (var job in Jobs)
                {
                    job.Execute();
                }
                _jsonHandler.SaveJobs(Jobs);
            }
        }
    }
}
