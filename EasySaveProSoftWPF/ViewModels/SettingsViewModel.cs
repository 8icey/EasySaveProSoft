using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySaveProSoft.Services;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public string CurrentLanguage { get; set; } = "en";

        public ObservableCollection<string> PriorityOrder { get; set; }
        


        private string _newExtension;
        public string NewExtension
        {
            get => _newExtension;
            set
            {
                _newExtension = value;
                OnPropertyChanged();
                _addExtensionCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _selectedExtension;
        public string SelectedExtension
        {
            get => _selectedExtension;
            set
            {
                _selectedExtension = value;
                OnPropertyChanged();
                _moveUpCommand?.RaiseCanExecuteChanged();
                _moveDownCommand?.RaiseCanExecuteChanged();
            }
        }

        private RelayCommand _addExtensionCommand;
        private RelayCommand _moveUpCommand;
        private RelayCommand _moveDownCommand;

        public ICommand AddExtensionCommand => _addExtensionCommand;
        public ICommand RemoveExtensionCommand { get; }
        public ICommand MoveUpCommand => _moveUpCommand;
        public ICommand MoveDownCommand => _moveDownCommand;

        public SettingsViewModel()
        {
            PriorityOrder = new ObservableCollection<string>(AppConfig.GetPriorityOrder());

            _addExtensionCommand = new RelayCommand(
                _ => AddExtension(),
                _ => !string.IsNullOrWhiteSpace(NewExtension)
            );

            RemoveExtensionCommand = new RelayCommand(p => RemoveExtension(p as string));

            _moveUpCommand = new RelayCommand(_ => Move(-1), _ => CanMove(-1));
            _moveDownCommand = new RelayCommand(_ => Move(1), _ => CanMove(1));
        }

        private void AddExtension()
        {
            string ext = NewExtension.Trim().ToLower();
            if (!ext.StartsWith(".")) ext = "." + ext;

            if (!PriorityOrder.Contains(ext))
            {
                PriorityOrder.Add(ext);
                AppConfig.SetPriorityOrder(PriorityOrder.ToList());
            }

            NewExtension = string.Empty;
        }

        private void RemoveExtension(string ext)
        {
            if (PriorityOrder.Contains(ext))
            {
                PriorityOrder.Remove(ext);
                AppConfig.SetPriorityOrder(PriorityOrder.ToList());
            }
        }

        private void Move(int direction)
        {
            int index = PriorityOrder.IndexOf(SelectedExtension);
            if (index < 0 || index + direction < 0 || index + direction >= PriorityOrder.Count) return;

            PriorityOrder.Move(index, index + direction);
            AppConfig.SetPriorityOrder(PriorityOrder.ToList());
            OnPropertyChanged(nameof(PriorityOrder));
        }

        private bool CanMove(int direction)
        {
            int index = PriorityOrder.IndexOf(SelectedExtension);
            return index >= 0 && index + direction >= 0 && index + direction < PriorityOrder.Count;
        }

        public void ChangeLanguage(string lang)
        {
            CurrentLanguage = lang;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
