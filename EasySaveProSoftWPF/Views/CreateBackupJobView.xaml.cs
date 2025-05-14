using System.Windows;
using EasySaveProSoft.Models;
using EasySaveProSoft.ViewModels;
using EasySaveProSoft.WPF.ViewModels;

namespace EasySaveProSoft.WPF.Views
{
    public partial class CreateBackupJobView : Window
    {
        private readonly BackupJobsViewModel _viewModel;

        public CreateBackupJobView(BackupJobsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
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
            _viewModel.CreateBackupJob(NameTextBox.Text, SourceTextBox.Text, TargetTextBox.Text, type);

            MessageBox.Show("Backup Job created successfully!");
            this.Close(); // Close the window after creation
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
