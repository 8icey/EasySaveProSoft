using System.Windows.Controls;
using EasySaveProSoft.WPF.ViewModels;

namespace EasySaveProSoft.WPF.Views
{
    public partial class BackupJobsView : UserControl
    {
        public LocalizationViewModel Loc { get; set; } = new LocalizationViewModel();
        public BackupJobsView()
        {
            InitializeComponent();
            DataContext = new BackupJobsViewModel(); // 🔥 This binds the ViewModel to the View
        }

        
    }
}
