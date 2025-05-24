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
            if (SelectedBackupJob == null)
                return;

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

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            await Task.Run(async () =>
            {
                SelectedBackupJob.OnProgressUpdated += UpdateProgress;
                await SelectedBackupJob.Execute(_pauseEvent, token);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _logger.LogJobStatus(SelectedBackupJob, true);
                    MessageBox.Show(string.Format(WpfLanguageService.Instance.Translate("msg_job_executed"), SelectedBackupJob.Name));
                });
            });
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

        //     private async Task RunAllBackups()
        //     {
        //         ProgressValue = 0;
        //         // ✅ DO THIS BEFORE THE TASK
        //         if (SoftwareDetector.IsBlockedSoftwareRunning())
        //         {
        //             string running = SoftwareDetector.GetFirstBlockedProcess();
        //             MessageBox.Show(
        //                 string.Format(WpfLanguageService.Instance.Translate("msg_blocked_software"), running),
        //                  "Blocked Software Running",
        //               MessageBoxButton.OK,
        //                 MessageBoxImage.Warning
        //);
        //             return;
        //         }

        //         if (BackupJobs.Count == 0)
        //         {
        //             MessageBox.Show(WpfLanguageService.Instance.Translate("msg_no_jobs"));
        //             return;
        //         }

        //         double step = 100.0 / BackupJobs.Count;

        //         foreach (var job in BackupJobs)
        //         {

        //             await Task.Run(() => job.Execute());
        //             _logger.LogJobStatus(job, true); // 🔹 Log après chaque exécution
        //             ProgressValue += step;
        //         }

        //         MessageBox.Show(WpfLanguageService.Instance.Translate("msg_all_executed"));
        //     }

        private async Task RunAllBackups()
        {
            ProgressValue = 0;

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

            double step = 100.0 / BackupJobs.Count;

            var cts = new CancellationTokenSource(); // create a token source
            var pauseEvent = new ManualResetEventSlim(true); // allow execution to continue immediately

            foreach (var job in BackupJobs)
            {
                await Task.Run(() => job.Execute(pauseEvent, cts.Token));
                _logger.LogJobStatus(job, true);
                ProgressValue += step;
            }

            MessageBox.Show(WpfLanguageService.Instance.Translate("msg_all_executed"));
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
        }

        private void UpdateCommandStates()
        {
            ((RelayCommand)PauseCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ResumeCommand).RaiseCanExecuteChanged();
            ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }


    }
}
