using EasySaveProSoft.ViewModels;
using EasySaveProSoft.Models;
using EasySaveProSoft.Services;
using System;

namespace EasySaveProSoft.Views
{
    public class ConsoleUI
    {
        private readonly BackupManagerViewModel _viewModel = new BackupManagerViewModel();
        private readonly LanguageService _languageService = new LanguageService();

        public ConsoleUI()
        {
            Console.WriteLine("Choose your language (en/fr): ");
            string lang = Console.ReadLine().ToLower();
            _languageService.SetLanguage(lang);
        }

        public void DisplayMenu()
        {
            int choice;
            do
            {
                Console.Clear();
                Console.WriteLine(_languageService.Translate("menu_title"));
                Console.WriteLine(_languageService.Translate("menu_create"));
                Console.WriteLine(_languageService.Translate("menu_run_one"));
                Console.WriteLine(_languageService.Translate("menu_run_all"));
                Console.WriteLine(_languageService.Translate("menu_exit"));
                Console.Write(_languageService.Translate("menu_choice"));
                choice = int.Parse(Console.ReadLine() ?? "0");

                switch (choice)
                {
                    case 1:
                        CreateJob();
                        break;
                    case 2:
                        RunOneJob();
                        break;
                    case 3:
                        _viewModel.RunAllBackups();
                        Console.WriteLine(_languageService.Translate("all_jobs_executed"));
                        break;
                }

                Console.WriteLine("\n" + _languageService.Translate("press_enter"));
                Console.ReadLine();
            } while (choice != 4);
        }

        private void CreateJob()
        {
            Console.Write(_languageService.Translate("enter_name"));
            string name = Console.ReadLine();
            Console.Write(_languageService.Translate("enter_source"));
            string source = Console.ReadLine();
            Console.Write(_languageService.Translate("enter_target"));
            string target = Console.ReadLine();
            Console.Write(_languageService.Translate("enter_type"));
            string type = Console.ReadLine();
            BackupType backupType = (type.ToLower() == "full" || type.ToLower() == "complet") ? BackupType.Full : BackupType.Differential;

            _viewModel.CreateBackupJob(name, source, target, backupType);
        }

        private void RunOneJob()
        {
            Console.Write(_languageService.Translate("enter_name"));
            string name = Console.ReadLine();
            _viewModel.RunBackup(name);
        }
    }
}
