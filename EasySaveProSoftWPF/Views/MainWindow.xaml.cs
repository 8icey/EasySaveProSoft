using System.Windows;
using System.Windows.Controls;
using EasySaveProSoft.WPF.ViewModels;
using EasySaveProSoft.WPF.Views;

namespace EasySaveProSoft.WPF.Views
{
    public partial class MainWindow : Window
    {
        //public LocalizationViewModel Loc { get; set; } = new LocalizationViewModel();

        public MainWindow()
        {
            InitializeComponent();
            MainContentFrame.Content = new BackupJobsView(); // Default page

        }

        private void NavigateToBackupJobs(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Content = new BackupJobsView();
        }

        private void NavigateToSettings(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Content = new SettingsView();
        }


    }
}
