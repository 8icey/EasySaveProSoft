using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySaveProSoft.Services;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public string CurrentLanguage { get; set; } = "en";

        public ObservableCollection<string> PriorityExtensions { get; set; }

        private string _newExtension;
        public string NewExtension
        {
            get => _newExtension;
            set
            {
                _newExtension = value;
                OnPropertyChanged();
                _addExtensionCommand?.RaiseCanExecuteChanged(); // 🔧 Active/désactive dynamiquement le bouton
            }
        }

        private string _selectedExtension;
        public string SelectedExtension
        {
            get => _selectedExtension;
            set { _selectedExtension = value; OnPropertyChanged(); }
        }

        // 🔧 On utilise une référence privée pour pouvoir appeler RaiseCanExecuteChanged
        private RelayCommand _addExtensionCommand;
        public ICommand AddExtensionCommand => _addExtensionCommand;
        public ICommand RemoveExtensionCommand { get; }

        public SettingsViewModel()
        {
            PriorityExtensions = new ObservableCollection<string>(AppConfig.GetPriorityExtensions());

            _addExtensionCommand = new RelayCommand(
                _ => AddExtension(),
                _ => !string.IsNullOrWhiteSpace(NewExtension)
            );

            RemoveExtensionCommand = new RelayCommand(p => RemoveExtension(p as string));
        }

        public void ChangeLanguage(string lang)
        {
            CurrentLanguage = lang;
            // Update the language service if needed
        }

        private void AddExtension()
        {
            string ext = NewExtension.Trim().ToLower();
            if (!ext.StartsWith(".")) ext = "." + ext;

            if (!PriorityExtensions.Contains(ext))
            {
                PriorityExtensions.Add(ext);
                AppConfig.SetPriorityExtensions(new List<string>(PriorityExtensions));
            }

            NewExtension = string.Empty;
        }

        private void RemoveExtension(string ext)
        {
            if (PriorityExtensions.Contains(ext))
            {
                PriorityExtensions.Remove(ext);
                AppConfig.SetPriorityExtensions(new List<string>(PriorityExtensions));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
