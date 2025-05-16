using System.Collections.ObjectModel;
using System.Windows;
using EasySaveProSoft.Models;
using EasySaveProSoft.ViewModels;
using EasySaveProSoft.WPF.ViewModels;

namespace EasySaveProSoft.WPF.Views
{
    public partial class CreateBackupJobView : Window
    {
        public BackupManager Manager { get; private set; } = new BackupManager();
        public ObservableCollection<BackupJob> BackupJobs { get; private set; }
        private readonly BackupJobsViewModel _viewModel;

        public CreateBackupJobView(BackupJobsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
        }
        public void CreateBackupJob(string name, string source, string target, BackupType type)
        {
            // ✅ Create the job with provided parameters
            var job = new BackupJob
            {
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = type
            };

            // ✅ Validate paths before adding
            if (job.IsValid())
            {
                Manager.AddJob(job);
                BackupJobs.Add(job); // 🔄 Refreshes the ListBox in WPF
                MessageBox.Show($"Backup Job '{name}' added successfully!");
            }
            else
            {
                MessageBox.Show("Invalid paths. Please check your source and target directories.");
            }
        }


        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(NameTextBox.Text) ||
                string.IsNullOrEmpty(SourceTextBox.Text) ||
                string.IsNullOrEmpty(TargetTextBox.Text) ||
                TypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please fill all the fields.");
                return;
            }

            // Determine backup type
            BackupType type = (TypeComboBox.Text == "Full") ? BackupType.Full : BackupType.Differential;

            // Create new job
            //_viewModel.CreateBackupJob(NameTextBox.Text, SourceTextBox.Text, TargetTextBox.Text, type);

            MessageBox.Show("Backup Job created successfully!");
            this.Close(); // Close the window after creation
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
