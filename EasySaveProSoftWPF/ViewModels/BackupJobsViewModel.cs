using EasySaveProSoft.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using EasySaveProSoft.Services;
using EasySaveProSoft.WPF.Services;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using EasySaveProSoft.WPF.Views;

namespace EasySaveProSoft.WPF.ViewModels
{
    

    public class BackupJobsViewModel : INotifyPropertyChanged
    {
        public LocalizationViewModel Loc { get; } = new LocalizationViewModel();

        public BackupType[] BackupTypes => (BackupType[])Enum.GetValues(typeof(BackupType));
       

        public BackupManager Manager { get; private set; } = new BackupManager();
        public ObservableCollection<BackupJob> BackupJobs { get; private set; }

        private Logger _logger = new Logger();
       
        private CancellationTokenSource _cts = new();
        private bool _isPaused;
        private readonly ManualResetEventSlim _pauseEvent = new(true);
        private BackupJob _selectedBackupJob;
        
        public BackupJob SelectedBackupJob
        {
            get => _selectedBackupJob;
            set
            {
                _selectedBackupJob = value;
                OnPropertyChanged();
                ((RelayCommand)ExecuteBackupJobCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteBackupJobCommand).RaiseCanExecuteChanged();
              

            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set { _progressValue = value; OnPropertyChanged(); }
        }

        private string _fileSizeText;
        public string FileSizeText
        {
            get => _fileSizeText;
            set { _fileSizeText = value; OnPropertyChanged(); }
        }

        private string _estimatedTimeText;
        public string EstimatedTimeText
        {
            get => _estimatedTimeText;
            set { _estimatedTimeText = value; OnPropertyChanged(); }
        }

        // Champs de validation
        private bool _isSourceValid;
        public bool IsSourceValid
        {
            get => _isSourceValid;
            set { _isSourceValid = value; OnPropertyChanged(); }
        }

        private bool _isDestinationValid;
        public bool IsDestinationValid
        {
            get => _isDestinationValid;
            set { _isDestinationValid = value; OnPropertyChanged(); }
        }

        // Champs de création
        private string _newJobName;
        public string NewJobName
        {
            get => _newJobName;
            set { _newJobName = value; OnPropertyChanged(); }
        }

        private string _newJobSource;
        public string NewJobSource
        {
            get => _newJobSource;
            set
            {
                _newJobSource = value;
                OnPropertyChanged();
                IsSourceValid = Directory.Exists(value);
            }
        }

        private string _newJobTarget;
        public string NewJobTarget
        {
            get => _newJobTarget;
            set
            {
                _newJobTarget = value;
                OnPropertyChanged();
                IsDestinationValid = Directory.Exists(value);
            }
        }

        private BackupType _newJobType = BackupType.Full;
        public BackupType NewJobType
        {
            get => _newJobType;
            set { _newJobType = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand CreateBackupJobCommand { get; }
        public ICommand ExecuteBackupJobCommand { get; }
        public ICommand DeleteBackupJobCommand { get; }
        public ICommand RunAllBackupsCommand { get; }
        public ICommand BrowseSourceCommand { get; }
        public ICommand BrowseDestinationCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ExecuteSelectedJobsCommand { get; }

        


        public BackupJobsViewModel()

        {
            BackupJobs = new ObservableCollection<BackupJob>(Manager.Jobs);
            PauseCommand = new RelayCommand(_ => Pause(), _ => !_isPaused);
            ResumeCommand = new RelayCommand(_ => Resume(), _ => _isPaused);
            StopCommand = new RelayCommand(_ => Stop(), _ => _cts != null);

            CreateBackupJobCommand = new RelayCommand(_ => CreateBackupJob());
            ExecuteBackupJobCommand = new RelayCommand(_ => ExecuteBackupJob(), _ => SelectedBackupJob != null);
            DeleteBackupJobCommand = new RelayCommand(_ => DeleteBackupJob(), _ => SelectedBackupJob != null);
            RunAllBackupsCommand = new RelayCommand(async _ => await RunAllBackups());

            BrowseSourceCommand = new RelayCommand(_ => BrowseSource());
            BrowseDestinationCommand = new RelayCommand(_ => BrowseDestination());
            ExecuteSelectedJobsCommand = new RelayCommand(async _ => await ExecuteSelectedJobs());
        }

        private async Task RunAllBackups()
        {
            if (SoftwareDetector.IsBlockedSoftwareRunning())
            {
                string running = SoftwareDetector.GetFirstBlockedProcess();
                MessageBox.Show(
                    string.Format(WpfLanguageService.Instance.Translate("msg_blocked_software"), running),
                    "Blocked Software Running",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (BackupJobs.Count == 0)
            {
                MessageBox.Show(WpfLanguageService.Instance.Translate("msg_no_jobs"));
                return;
            }

            var pauseEvent = new ManualResetEventSlim(true);
            var token = _cts.Token;

            var progressVM = new AllBackupsProgressViewModel();
            var progressWindow = new AllBackupsProgressWindow(progressVM);
            progressWindow.Show();

            foreach (var job in BackupJobs)
            {
                var jobVM = new BackupProgressViewModel
                {
                    JobName = job.Name,
                    ProgressValue = 0,
                    SizeText = "0 B",
                    EstimatedTime = ""
                };

                progressVM.AddJob(jobVM);

                job.OnProgressUpdated += (progress, size, eta) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        jobVM.ProgressValue = progress;
                        jobVM.SizeText = size;
                        jobVM.EstimatedTime = eta;
                    });
                };

                var thread = new Thread(() => job.Execute(pauseEvent, token).Wait());
                thread.Start();

            }
        }
        public void CreateBackupJob()
        {
            if (string.IsNullOrWhiteSpace(NewJobName) || string.IsNullOrWhiteSpace(NewJobSource) || string.IsNullOrWhiteSpace(NewJobTarget))
            {
                MessageBox.Show(WpfLanguageService.Instance.Translate("msg_fill_fields"));
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
                _logger.LogJobStatus(job, false); // 🔹 Log de statut "Not Executed"
                MessageBox.Show($"Backup job '{NewJobName}' created!");
                ClearNewJobFields();
            }
            else
            {
                MessageBox.Show(WpfLanguageService.Instance.Translate("msg_invalid_paths"));
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

        //public void ExecuteBackupJob()
        //{




        //    if (SelectedBackupJob == null)
        //        return;

        //    Task.Run(() =>
        //    {
        //        SelectedBackupJob.OnProgressUpdated += UpdateProgress;
        //        SelectedBackupJob.Execute();

        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            _logger.LogJobStatus(SelectedBackupJob, true); // 🔹 Log de statut "Executed"
        //            MessageBox.Show($"Backup job '{SelectedBackupJob.Name}' executed.");
        //        });
        //    });
        //}

        public async void ExecuteBackupJob()
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var selectedJobs = BackupJobs.Where(j => j.IsSelected).ToList();

            // Fallback to single selected job if no checkboxes selected
            if (selectedJobs.Count == 0 && SelectedBackupJob != null)
                selectedJobs.Add(SelectedBackupJob);

            if (selectedJobs.Count == 0)
            {
                MessageBox.Show("Please select at least one job to execute.", "No Job Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Filter out invalid jobs
            var invalidJobs = selectedJobs.Where(j => !Directory.Exists(j.SourcePath) || !Directory.Exists(j.TargetPath)).ToList();
            if (invalidJobs.Any())
            {
                string names = string.Join(", ", invalidJobs.Select(j => j.Name));
                MessageBox.Show($"The following job(s) have invalid source or target paths:\n{names}", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Blocked software check
            if (SoftwareDetector.IsBlockedSoftwareRunning())
            {
                string running = SoftwareDetector.GetFirstBlockedProcess();
                MessageBox.Show(
                    string.Format(WpfLanguageService.Instance.Translate("msg_blocked_software"), running),
                    "Blocked Software Running",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            var pauseEvent = _pauseEvent;

            List<Task> tasks = new();

            foreach (var job in selectedJobs)
            {
                job.OnProgressUpdated += UpdateProgress;

                var task = Task.Run(async () =>
                {
                    bool success = await job.Execute(pauseEvent, token);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _logger.LogJobStatus(job, success);
                        if (success)
                        {
                            MessageBox.Show(string.Format(WpfLanguageService.Instance.Translate("msg_job_executed"), job.Name), "Backup Completed");
                        }
                        else
                        {
                            MessageBox.Show($"Backup job '{job.Name}' failed or was canceled.", "Backup Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Progress bar reset
            await Task.Delay(2000);
            ProgressValue = 0;
            FileSizeText = "";
            EstimatedTimeText = "";
        }






        public void DeleteBackupJob()
        {
            if (SelectedBackupJob == null)
                return;

            var name = SelectedBackupJob.Name;
            Manager.DeleteJob(name);
            BackupJobs.Remove(SelectedBackupJob);
            SelectedBackupJob = null;
            MessageBox.Show(string.Format(WpfLanguageService.Instance.Translate("msg_job_deleted"), name));

        }

        

        private async Task ExecuteSelectedJobs()
        {
            var selected = BackupJobs.Where(j => j.IsSelected).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("No jobs selected.");
                return;
            }

            foreach (var job in selected)
            {
                await Task.Run(() => job.Execute(_pauseEvent, _cts.Token));
                _logger.LogJobStatus(job, true);
            }

            ResetProgressBarAfterDelay();
        }
        private async void ResetProgressBarAfterDelay()
        {
            await Task.Delay(2000); // wait 2 seconds
            ProgressValue = 0;
            FileSizeText = "";
            EstimatedTimeText = "";
        }

        private void BrowseSource()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                var path = Path.GetDirectoryName(dialog.FileName);
                if (Directory.Exists(path))
                {
                    NewJobSource = path;
                }
            }
        }

        private void BrowseDestination()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                FileName = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                var path = Path.GetDirectoryName(dialog.FileName);
                if (Directory.Exists(path))
                {
                    NewJobTarget = path;
                }
            }
        }

        private void UpdateProgress(double progress, string sizeText, string eta)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressValue = progress;
                FileSizeText = sizeText;
                EstimatedTimeText = eta;
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }


        private void Pause()
        {
            _isPaused = true;
            _pauseEvent.Reset();
            UpdateCommandStates();
        }

        private void Resume()
        {
            _isPaused = false;
            _pauseEvent.Set();
            UpdateCommandStates();
        }

        private void Stop()
        {
            _cts.Cancel();
            _pauseEvent.Set(); // In case it's paused
            MessageBox.Show("Backup stopped.");
            ResetProgressBarAfterDelay();
        }

        private void UpdateCommandStates()
        {
            ((RelayCommand)PauseCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ResumeCommand).RaiseCanExecuteChanged();
            ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }

        //public void HandleRemoteCommand(string commandLine)
        //{
        //    if (string.IsNullOrWhiteSpace(commandLine))
        //        return;

        //    string[] parts = commandLine.Split(' ', 2);
        //    if (parts.Length < 2) return;

        //    string command = parts[0].ToLower();
        //    string jobName = parts[1].Trim();

        //    var job = BackupJobs.FirstOrDefault(j => j.Name.Equals(jobName, StringComparison.OrdinalIgnoreCase));
        //    if (job == null)
        //    {
        //        Console.WriteLine($"[Remote] No job found named '{jobName}'");
        //        return;
        //    }

        //    if (SelectedBackupJob != job)
        //        SelectedBackupJob = job;

        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        switch (command)
        //        {
        //            case "pause":
        //                Pause();
        //                break;
        //            case "resume":
        //                Resume();
        //                break;
        //            case "stop":
        //                Stop();
        //                break;
        //        }
        //    });
        //}

        public void HandleRemoteCommand(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                return;

            var command = commandLine.Trim().ToLower();

            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (command)
                {
                    case "pause":
                        if (!_isPaused)
                            Pause();
                        break;
                    case "resume":
                        if (_isPaused)
                            Resume();
                        break;
                    case "stop":
                        Stop();
                        break;
                    default:
                        Console.WriteLine($"[Remote] Unknown command: {command}");
                        break;
                }
            });
        }

    }
}

