﻿

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
using System;
using System.Linq;
using System.Threading;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class BackupJobsViewModel : INotifyPropertyChanged
    {

        public LocalizationViewModel Loc { get; } = new LocalizationViewModel();

        public ObservableCollection<string> PriorityOrder { get; }

        public BackupType[] BackupTypes => (BackupType[])Enum.GetValues(typeof(BackupType));
        public BackupManager Manager { get; } = new BackupManager();
        public ObservableCollection<BackupJob> BackupJobs { get; }

        private readonly Logger _logger = new Logger();
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

        private string _newExtension;
        public string NewExtension
        {
            get => _newExtension;
            set
            {
                _newExtension = value;
                OnPropertyChanged();
                _addExtensionCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _selectedExtension;
        public string SelectedExtension
        {
            get => _selectedExtension;
            set
            {
                _selectedExtension = value;
                OnPropertyChanged();
                _moveUpCommand?.RaiseCanExecuteChanged();
                _moveDownCommand?.RaiseCanExecuteChanged();
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

        // New job creation
        public string NewJobName { get; set; }
        public string NewJobSource { get; set; }
        public string NewJobTarget { get; set; }
        public BackupType NewJobType { get; set; } = BackupType.Full;

        public bool IsSourceValid => Directory.Exists(NewJobSource);
        public bool IsDestinationValid => Directory.Exists(NewJobTarget);

        // Commands
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

        public ICommand AddExtensionCommand => _addExtensionCommand;
        public ICommand RemoveExtensionCommand { get; }
        public ICommand MoveUpCommand => _moveUpCommand;
        public ICommand MoveDownCommand => _moveDownCommand;

        private readonly RelayCommand _addExtensionCommand;
        private readonly RelayCommand _moveUpCommand;
        private readonly RelayCommand _moveDownCommand;

        public BackupJobsViewModel()
        {
            BackupJobs = new ObservableCollection<BackupJob>(Manager.Jobs);
            PriorityOrder = new ObservableCollection<string>(AppConfig.GetPriorityOrder());

            CreateBackupJobCommand = new RelayCommand(_ => CreateBackupJob());
            ExecuteBackupJobCommand = new RelayCommand(_ => ExecuteBackupJob(), _ => true);
            DeleteBackupJobCommand = new RelayCommand(_ => DeleteBackupJob(), _ => SelectedBackupJob != null);
            RunAllBackupsCommand = new RelayCommand(async _ => await RunAllBackups());

            BrowseSourceCommand = new RelayCommand(_ => BrowseSource());
            BrowseDestinationCommand = new RelayCommand(_ => BrowseDestination());
            ExecuteSelectedJobsCommand = new RelayCommand(async _ => await ExecuteSelectedJobs());

            PauseCommand = new RelayCommand(_ => Pause(), _ => !_isPaused);
            ResumeCommand = new RelayCommand(_ => Resume(), _ => _isPaused);
            StopCommand = new RelayCommand(_ => Stop(), _ => true);

            _addExtensionCommand = new RelayCommand(_ => AddExtension(), _ => !string.IsNullOrWhiteSpace(NewExtension));
            RemoveExtensionCommand = new RelayCommand(p => RemoveExtension(p as string));
            _moveUpCommand = new RelayCommand(_ => Move(-1), _ => CanMove(-1));
            _moveDownCommand = new RelayCommand(_ => Move(1), _ => CanMove(1));
        }

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
                MessageBox.Show(WpfLanguageService.Instance.Translate("msg_no_jobs"),
                                "No Job Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Filter out invalid jobs
            var invalidJobs = selectedJobs
                .Where(j => !Directory.Exists(j.SourcePath) || !Directory.Exists(j.TargetPath))
                .ToList();

            if (invalidJobs.Any())
            {
                string names = string.Join(", ", invalidJobs.Select(j => j.Name));
                MessageBox.Show(
                    string.Format(WpfLanguageService.Instance.Translate("msg_invalid_paths"), names),
                    "Invalid Path",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
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
                            MessageBox.Show(
                                string.Format(WpfLanguageService.Instance.Translate("msg_job_executed"), job.Name),
                                "Backup Completed"
                            );
                        }
                        else
                        {
                            MessageBox.Show(
                                string.Format(WpfLanguageService.Instance.Translate("msg_job_failed"), job.Name),
                                "Backup Failed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                            );
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

            NetworkMonitorService.StartMonitoring();

            var pauseEvent = new ManualResetEventSlim(true);
            var token = _cts.Token;
            var progressVM = new AllBackupsProgressViewModel();
            var progressWindow = new AllBackupsProgressWindow(progressVM);
            progressWindow.Show();

            int maxParallel = int.MaxValue; // Illimité
            var tasks = new List<Task>();

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

                var task = Task.Run(async () =>
                {
                    int currentMax = NetworkMonitorService.IsNetworkOverloaded()
                        ? AppConfig.GetMaxParallelJobsOnHighLoad()
                        : int.MaxValue;

                    using (var semaphore = new SemaphoreSlim(currentMax))
                    {
                        await semaphore.WaitAsync();

                        try
                        {
                            await job.Execute(pauseEvent, token);
                            Application.Current.Dispatcher.Invoke(() => _logger.LogJobStatus(job, true));
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            NetworkMonitorService.StopMonitoring();

            MessageBox.Show(WpfLanguageService.Instance.Translate("msg_all_executed"));
        }


        private async Task ExecuteSelectedJobs()
        {
            foreach (var job in BackupJobs.Where(j => j.IsSelected))
            {
                await Task.Run(() => job.Execute(_pauseEvent, _cts.Token));
                _logger.LogJobStatus(job, true);
            }

            ResetProgress();
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

            if (!job.IsValid())
            {
                MessageBox.Show(WpfLanguageService.Instance.Translate("msg_invalid_paths"));
                return;
            }

            Manager.AddJob(job);
            BackupJobs.Add(job);
            _logger.LogJobStatus(job, false);
            MessageBox.Show($"Backup job '{NewJobName}' created!");
            ClearNewJobFields();
        }

        //public void DeleteBackupJob()
        //{
        //    if (SelectedBackupJob == null)
        //    {
        //        MessageBox.Show("No backup job selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }


        //    Manager.DeleteJob(SelectedBackupJob.Name);
        //    BackupJobs.Remove(SelectedBackupJob);
        //    MessageBox.Show(string.Format(WpfLanguageService.Instance.Translate("msg_job_deleted"), SelectedBackupJob.Name));
        //    SelectedBackupJob = null;
        //}


        public void DeleteBackupJob()
        {
            // Get all jobs with checkboxes selected
            var selectedJobs = BackupJobs.Where(j => j.IsSelected).ToList();

            // If none checked, fall back to the highlighted one
            if (selectedJobs.Count == 0 && SelectedBackupJob != null)
                selectedJobs.Add(SelectedBackupJob);

            if (selectedJobs.Count == 0)
            {
                MessageBox.Show("No backup job selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var job in selectedJobs.ToList()) // toList to avoid modifying collection during iteration
            {
                Manager.DeleteJob(job.Name);
                BackupJobs.Remove(job);
            }

            SelectedBackupJob = null;

            MessageBox.Show($"{selectedJobs.Count} job(s) deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ClearNewJobFields()
        {
            NewJobName = string.Empty;
            NewJobSource = string.Empty;
            NewJobTarget = string.Empty;
            NewJobType = BackupType.Full;
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
                    OnPropertyChanged(nameof(NewJobSource)); // ✅ Force UI update
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
                    OnPropertyChanged(nameof(NewJobTarget)); // ✅ Force UI update
                }
            }
        }


        private void UpdateProgress(double progress, string size, string eta)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressValue = progress;
                FileSizeText = size;
                EstimatedTimeText = eta;
            });
        }

        private async void ResetProgress()
        {
            await Task.Delay(2000);
            ProgressValue = 0;
            FileSizeText = "";
            EstimatedTimeText = "";
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
            _pauseEvent.Set();
            MessageBox.Show("Backup stopped.");
            ResetProgress();
        }

        private void UpdateCommandStates()
        {
            ((RelayCommand)PauseCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ResumeCommand).RaiseCanExecuteChanged();
            ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }

        private void AddExtension()
        {
            string ext = NewExtension.Trim().ToLower();
            if (!ext.StartsWith(".")) ext = "." + ext;
            if (!PriorityOrder.Contains(ext))
            {
                PriorityOrder.Add(ext);
                AppConfig.SetPriorityOrder(PriorityOrder.ToList());
            }

            NewExtension = string.Empty;
        }

        private void RemoveExtension(string ext)
        {
            if (PriorityOrder.Contains(ext))
            {
                PriorityOrder.Remove(ext);
                AppConfig.SetPriorityOrder(PriorityOrder.ToList());
            }
        }

        private void Move(int direction)
        {
            int index = PriorityOrder.IndexOf(SelectedExtension);
            if (index < 0 || index + direction < 0 || index + direction >= PriorityOrder.Count) return;

            PriorityOrder.Move(index, index + direction);
            AppConfig.SetPriorityOrder(PriorityOrder.ToList());
        }

        private bool CanMove(int direction)
        {
            int index = PriorityOrder.IndexOf(SelectedExtension);
            return index >= 0 && index + direction >= 0 && index + direction < PriorityOrder.Count;
        }

        public void HandleRemoteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;

            command = command.Trim().ToLower();

            Application.Current.Dispatcher.Invoke(() =>
            {
                switch (command)
                {
                    case "pause":
                        if (!_isPaused) Pause();
                        break;
                    case "resume":
                        if (_isPaused) Resume();
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private int _largeFileThresholdMB = AppConfig.GetInt("LargeFileThresholdMB", 1000);
        public int LargeFileThresholdMB
        {
            get => _largeFileThresholdMB;
            set
            {
                _largeFileThresholdMB = value;
                OnPropertyChanged();
                AppConfig.SetLargeFileThresholdMB(value);
            }
        }
    }


}
