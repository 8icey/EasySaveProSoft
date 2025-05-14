using EasySaveProSoft.Models;
using EasySaveProSoft.WPF.Views;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using EasySaveProSoft.ViewModels;

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

        private void OpenCreateBackupJobView()
        {
            var createView = new CreateBackupJobView(this);
            createView.ShowDialog();
        }

        private void ExecuteBackupJob()
        {
            if (SelectedBackupJob != null)
            {
                // 🔥 Subscribe to the event before running the job
                SelectedBackupJob.OnProgressUpdated += UpdateProgress;

                // Run the job
                _backupManager.RunJob(SelectedBackupJob.Name);

                // 🔥 Unsubscribe after completion
                SelectedBackupJob.OnProgressUpdated -= UpdateProgress;

                // Refresh the UI list
                BackupJobs.Clear();
                foreach (var job in _backupManager.Jobs)
                {
                    BackupJobs.Add(job);
                }
            }
        }

        // 🔥 Method to update progress in WPF
        private void UpdateProgress(double progress)
        {
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                MessageBox.Show($"Progress: {progress}%");
            });
        }

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
        private void DeleteBackupJob()
        {
            if (SelectedBackupJob != null)
            {
                _backupManager.DeleteJob(SelectedBackupJob.Name);
                BackupJobs.Remove(SelectedBackupJob);
            }
        }

        private bool CanExecuteOrDelete()
        {
            return SelectedBackupJob != null;
        }
    }
}
