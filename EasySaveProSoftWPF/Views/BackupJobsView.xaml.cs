using System.Windows.Controls;
using EasySaveProSoft.Services;
using EasySaveProSoft.WPF.ViewModels;

namespace EasySaveProSoft.WPF.Views
{
    public partial class BackupJobsView : UserControl
    {
        public LocalizationViewModel Loc { get; set; } = new LocalizationViewModel();
        private readonly BackupJobsViewModel _viewModel;

        public BackupJobsView()
        {
            InitializeComponent();
            _viewModel = new BackupJobsViewModel();
            DataContext = _viewModel;

            // Start the remote server
            RemoteServerService server = new RemoteServerService(_viewModel);
            server.Start(9000);
        }
    }

}
