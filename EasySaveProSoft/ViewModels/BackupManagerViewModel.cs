using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveProSoft.Models;

namespace EasySaveProSoft.ViewModels
{
    // Acts as a ViewModel layer between the UI (Console) and the BackupManager logic
    public class BackupManagerViewModel
    {
        // The underlying backup manager instance that handles job logic
        public BackupManager Manager { get; private set; } = new BackupManager();

        // Creates a new backup job and adds it if paths are valid
        public void CreateBackupJob(string name, string source, string target, BackupType type)
        {
            // Construct a backup job using input parameters
            BackupJob job = new BackupJob
            {
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = type
            };

            // Check if the job paths are valid before adding
            if (job.IsValid())
            {
                Manager.AddJob(job);
            }
            else
            {
                Console.WriteLine("Invalid paths. Please check your source and target directories.");
            }
        }

        // Executes a backup job by name
        public void RunBackup(string name)
        {
            Manager.RunJob(name);
        }

        // Executes all backup jobs
        public void RunAllBackups()
        {
            Manager.RunAllJobs();
        }
    }
}
