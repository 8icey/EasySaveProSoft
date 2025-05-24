using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class BackupProgressViewModel : INotifyPropertyChanged
    {
        private string _jobName;
        public string JobName
        {
            get => _jobName;
            set { _jobName = value; OnPropertyChanged(); }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set { _progressValue = value; OnPropertyChanged(); }
        }

        private string _sizeText;
        public string SizeText
        {
            get => _sizeText;
            set { _sizeText = value; OnPropertyChanged(); }
        }

        private string _estimatedTime;
        public string EstimatedTime
        {
            get => _estimatedTime;
            set { _estimatedTime = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

