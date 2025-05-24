using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class AllBackupsProgressViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BackupProgressViewModel> JobsProgress { get; set; }

        public AllBackupsProgressViewModel()
        {
            JobsProgress = new ObservableCollection<BackupProgressViewModel>();
        }

        public void AddJob(BackupProgressViewModel job)
        {
            JobsProgress.Add(job);
            OnPropertyChanged(nameof(JobsProgress));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

