using EasySaveProSoft.Models;
using EasySaveProSoft.ViewModels;
using EasySaveProSoft.WPF.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class BackupJobsViewModel
    {
        public ObservableCollection<BackupJob> BackupJobs { get; private set; }
        public BackupJob SelectedBackupJob { get; set; }

        public ICommand CreateBackupJobCommand { get; }
        public ICommand ExecuteBackupJobCommand { get; }
        public ICommand DeleteBackupJobCommand { get; }

        private readonly BackupManager _backupManager;

        public BackupJobsViewModel()
        {
            _backupManager = new BackupManager();
            BackupJobs = new ObservableCollection<BackupJob>(_backupManager.Jobs);

            CreateBackupJobCommand = new RelayCommand(OpenCreateBackupJobView);
            ExecuteBackupJobCommand = new RelayCommand(ExecuteBackupJob, CanExecuteOrDelete);
            DeleteBackupJobCommand = new RelayCommand(DeleteBackupJob, CanExecuteOrDelete);
        }

        // 🔥 Opens the window to create a new job
        private void OpenCreateBackupJobView()
        {
            var createView = new CreateBackupJobView(this);
            createView.ShowDialog(); // Modal dialog
        }

        // 🔥 Called from the popup window
        public void CreateBackupJob(string name, string source, string target, BackupType type)
        {
            var newJob = new BackupJob
            {
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = type
            };

            _backupManager.AddJob(newJob);
            BackupJobs.Add(newJob);
        }

        // Execute the selected job
        private void ExecuteBackupJob()
        {
            if (SelectedBackupJob != null)
            {
                _backupManager.RunJob(SelectedBackupJob.Name);
            }
        }

        // Delete the selected job
        private void DeleteBackupJob()
        {
            if (SelectedBackupJob != null)
            {
                _backupManager.DeleteJob(SelectedBackupJob.Name);
                BackupJobs.Remove(SelectedBackupJob);
            }
        }

        // Validation for buttons
        private bool CanExecuteOrDelete()
        {
            return SelectedBackupJob != null;
        }
    }
}
