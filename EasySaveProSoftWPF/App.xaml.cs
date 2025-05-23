using System.Configuration;
using System.Data;
using System.Windows;
using EasySaveProSoft.Models;
using EasySaveProSoft.Services;
using EasySaveProSoft.ViewModels;
using EasySaveProSoft.WPF.Services;

namespace EasySaveProSoft.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
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
