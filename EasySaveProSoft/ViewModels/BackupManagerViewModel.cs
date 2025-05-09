using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveProSoft.Models;

namespace EasySaveProSoft.ViewModels
{
    public class BackupManagerViewModel
    {
        public BackupManager Manager { get; private set; } = new BackupManager();

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

        public void RunBackup(string name)
        {
            Manager.RunJob(name);
        }

        public void RunAllBackups()
        {
            Manager.RunAllJobs();
        }
    }
}

