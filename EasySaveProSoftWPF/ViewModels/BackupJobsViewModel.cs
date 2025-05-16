using EasySaveProSoft.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class BackupJobsViewModel : INotifyPropertyChanged
    {
        public BackupType[] BackupTypes => (BackupType[])Enum.GetValues(typeof(BackupType));

        public BackupManager Manager { get; private set; } = new BackupManager();
        public ObservableCollection<BackupJob> BackupJobs { get; private set; }

        private BackupJob _selectedBackupJob;
        public BackupJob SelectedBackupJob
        {
            get => _selectedBackupJob;
            set
            {
                _selectedBackupJob = value;
                OnPropertyChanged();
                // Raise CanExecuteChanged for commands depending on selection
                ((RelayCommand)ExecuteBackupJobCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteBackupJobCommand).RaiseCanExecuteChanged();
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        // Properties for binding create job inputs (bind these in your Create Job form)
        public string NewJobName { get; set; }
        public string NewJobSource { get; set; }
        public string NewJobTarget { get; set; }
        public BackupType NewJobType { get; set; } = BackupType.Full;

        // Commands
        public ICommand CreateBackupJobCommand { get; }
        public ICommand ExecuteBackupJobCommand { get; }
        public ICommand DeleteBackupJobCommand { get; }
        public ICommand RunAllBackupsCommand { get; }

        public BackupJobsViewModel()
        {
            BackupJobs = new ObservableCollection<BackupJob>(Manager.Jobs);

            CreateBackupJobCommand = new RelayCommand(_ => CreateBackupJob());
            ExecuteBackupJobCommand = new RelayCommand(_ => ExecuteBackupJob(), _ => SelectedBackupJob != null);
            DeleteBackupJobCommand = new RelayCommand(_ => DeleteBackupJob(), _ => SelectedBackupJob != null);
            RunAllBackupsCommand = new RelayCommand(async _ => await RunAllBackups());
        }

        // Create a job using bound properties
        public void CreateBackupJob()
        {
            if (string.IsNullOrWhiteSpace(NewJobName) || string.IsNullOrWhiteSpace(NewJobSource) || string.IsNullOrWhiteSpace(NewJobTarget))
            {
                MessageBox.Show("Please fill in all fields to create a backup job.");
                return;
            }

            var job = new BackupJob
            {
                Name = NewJobName,
                SourcePath = NewJobSource,
                TargetPath = NewJobTarget,
                Type = NewJobType
            };

            if (job.IsValid())
            {
                Manager.AddJob(job);
                BackupJobs.Add(job);
                MessageBox.Show($"Backup job '{NewJobName}' created!");
                ClearNewJobFields();
            }
            else
            {
                MessageBox.Show("Invalid source or target paths.");
            }
        }

        private void ClearNewJobFields()
        {
            NewJobName = string.Empty;
            NewJobSource = string.Empty;
            NewJobTarget = string.Empty;
            NewJobType = BackupType.Full;
            OnPropertyChanged(nameof(NewJobName));
            OnPropertyChanged(nameof(NewJobSource));
            OnPropertyChanged(nameof(NewJobTarget));
            OnPropertyChanged(nameof(NewJobType));
        }

        // Execute selected job
        public void ExecuteBackupJob()
        {
            if (SelectedBackupJob == null)
                return;

            Task.Run(() =>
            {
                SelectedBackupJob.Execute();
                MessageBox.Show($"Backup job '{SelectedBackupJob.Name}' executed.");
            });
        }

        // Delete selected job
        public void DeleteBackupJob()
        {
            if (SelectedBackupJob == null)
                return;

            var name = SelectedBackupJob.Name;
            Manager.DeleteJob(name);
            BackupJobs.Remove(SelectedBackupJob);
            SelectedBackupJob = null;
            MessageBox.Show($"Backup job '{name}' deleted.");
        }

        // Run all jobs with progress update
        private async Task RunAllBackups()
        {
            ProgressValue = 0;

            if (BackupJobs.Count == 0)
            {
                MessageBox.Show("No backup jobs found.");
                return;
            }

            double step = 100.0 / BackupJobs.Count;

            foreach (var job in BackupJobs)
            {
                await Task.Run(() => job.Execute());
                ProgressValue += step;
            }

            MessageBox.Show("All backups executed.");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
