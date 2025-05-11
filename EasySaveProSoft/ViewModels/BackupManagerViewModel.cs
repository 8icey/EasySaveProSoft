using EasySaveProSoft.Models;

namespace EasySaveProSoft.ViewModels
{
    public class BackupManagerViewModel
    {
        // The underlying backup manager instance that handles job logic
        public BackupManager Manager { get; private set; } = new BackupManager();

        // Creates a new backup job and adds it if paths are valid
        public void CreateBackupJob(string name, string source, string target, BackupType type)
        {
            BackupJob job = new BackupJob
            {
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = type
            };

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

        // Displays all backup jobs
        public void DisplayJobs()
        {
            Manager.DisplayJobs();
        }

        // ✅ **NEW** — Delete a job by name
        public void DeleteBackup(string name)
        {
            Manager.DeleteJob(name);
        }
    }
}
