using EasySaveProSoft.Services;
using EasySaveProSoft.ViewModels;
using EasySaveProSoft.WPF.Services;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;

namespace EasySaveProSoft.WPF
{
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;
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
                IntPtr hWnd = FindWindow(null, "EasySave"); // Replace with your exact window title

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
           
            // Charger config
            AppConfig.Load();

            string lang = AppConfig.Get("Language", "en");
            WpfLanguageService.Instance.SetLanguage(lang);

            string format = AppConfig.Get("LogFormat", "json");
            new Logger().SetLogFormat(format);
        }

    }



}





