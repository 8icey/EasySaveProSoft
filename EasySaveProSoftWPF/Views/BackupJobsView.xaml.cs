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
            var vm = new BackupJobsViewModel(); // or reuse the same one
            DataContext = vm;
        }
    }

}
