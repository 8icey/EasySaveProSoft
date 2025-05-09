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
            string lang;
            do
            {
                Console.WriteLine("Choose your language (en/fr): ");
                lang = Console.ReadLine().ToLower();

                if (lang != "en" && lang != "fr")
                {
                    Console.WriteLine("[!] Invalid choice. Please type 'en' for English or 'fr' for French.");
                }
            } while (lang != "en" && lang != "fr");

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
                Console.WriteLine("4. Display Backup Jobs");  // New option
                Console.WriteLine("5. Exit");                  // Moved to 5
                Console.Write(_languageService.Translate("menu_choice"));

                bool parsed = int.TryParse(Console.ReadLine(), out choice);

                if (!parsed)
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        if (_viewModel.Manager.Jobs.Count >= 5)
                        {
                            Console.WriteLine("[!] Maximum number of backup jobs reached (5). Please delete one before adding.");
                        }
                        else
                        {
                            CreateJob();
                        }
                        break;
                    case 2:
                        RunOneJob();
                        break;
                    case 3:
                        _viewModel.RunAllBackups();
                        break;
                    case 4:
                        _viewModel.DisplayJobs(); // Show the list
                        break;
                }

                Console.WriteLine("\n" + _languageService.Translate("press_enter"));
                Console.ReadLine();
            } while (choice != 5); // Exit
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

            BackupType backupType = (type.ToLower() == "full" || type.ToLower() == "complet")
                ? BackupType.Full
                : BackupType.Differential;

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
