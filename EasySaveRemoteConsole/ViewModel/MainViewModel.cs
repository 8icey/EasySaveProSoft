using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EasySaveRemoteConsole.Services;
using EasySaveProSoft.WPF.ViewModels;

namespace EasySaveRemoteConsole.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private string _host = "127.0.0.1";
        public string Host
        {
            get => _host;
            set { _host = value; OnPropertyChanged(); }
        }

        private int _port = 9000;
        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Messages { get; } = new();

        private RemoteClientService _client;

        public ICommand ConnectCommand { get; }
        public ICommand SendPauseCommand { get; }
        public ICommand SendResumeCommand { get; }
        public ICommand SendStopCommand { get; }

        public MainViewModel()
        {
            ConnectCommand = new RelayCommand(_ => Connect());
            SendPauseCommand = new RelayCommand(_ => SendCommand("PAUSE"));
            SendResumeCommand = new RelayCommand(_ => SendCommand("RESUME"));
            SendStopCommand = new RelayCommand(_ => SendCommand("STOP"));
        }

        private void Connect()
        {
            _client = new RemoteClientService();
            _client.OnMessageReceived += msg => App.Current.Dispatcher.Invoke(() => Messages.Add(msg));
            _client.ConnectAsync(Host, Port);
            Messages.Add($"[Client] Connected to {Host}:{Port}");
        }

        private void SendCommand(string command)
        {
            _client?.SendAsync(command);
            Messages.Add($"[Client] Sent command: {command}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
