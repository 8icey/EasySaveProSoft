using EasySaveProSoft.Services;
using EasySaveProSoft.ViewModels;
using EasySaveProSoft.WPF.Services;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;
using EasySaveProSoft.WPF.ViewModels;

namespace EasySaveProSoft.WPF
{
    /// </summary>
    public partial class App : Application
    {
        private readonly BackupJobsViewModel _viewModel;
        private static Mutex mutex;
        public static BackupJobsViewModel SharedViewModel { get; private set; }

        public static RemoteServerService RemoteServer { get; private set; }
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern bool SetForegroundWindow(IntPtr hWnd);

        //[DllImport("user32.dll", SetLastError = true)]
        //private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        // Constants for ShowWindow
        private const int SW_RESTORE = 9;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            const string mutexName = "EASY_SAVE_PROSOFT_WPF_SINGLE_INSTANCE";

            mutex = new Mutex(true, mutexName, out createdNew);

            //if (!createdNew)
            //{
            //    // Try to find the existing window
            //    IntPtr hWnd = FindWindow(null, "EasySave"); // Use your main window title here

            //    if (hWnd != IntPtr.Zero)
            //    {
            //        SetForegroundWindow(hWnd);
            //    }

            //    Shutdown();
            //    return;
            //}


            if (!createdNew)
            {
                IntPtr hWnd = FindWindow(null, "EasySave 2.0"); // Replace with your exact window title

                if (hWnd != IntPtr.Zero)
                {
                    // If the window is minimized, restore it
                    if (IsIconic(hWnd))
                    {
                        ShowWindow(hWnd, SW_RESTORE);
                    }

                    // Bring to front
                    SetForegroundWindow(hWnd);
                }

                Shutdown();
                return;
            }

            base.OnStartup(e);



            SharedViewModel = new BackupJobsViewModel();
            RemoteServer = new RemoteServerService(SharedViewModel);
            RemoteServer.Start(9000);



            //var mainViewModel = new BackupJobsViewModel();
            //RemoteServer = new RemoteServerService(mainViewModel);
            //RemoteServer.Start(9000);
            // Charger config
            AppConfig.Load();

            string lang = AppConfig.Get("Language", "en");
            WpfLanguageService.Instance.SetLanguage(lang);

            string format = AppConfig.Get("LogFormat", "json");
            new Logger().SetLogFormat(format);
        }

    }



}





