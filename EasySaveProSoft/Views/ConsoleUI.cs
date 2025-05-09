using EasySaveProSoft.ViewModels;
using EasySaveProSoft.Models;
using EasySaveProSoft.Services;
using System;

namespace EasySaveProSoft.Views
{
    // Console-based UI for interacting with the backup system
    public class ConsoleUI
    {
        // ViewModel that connects to the logic and data layers
        private readonly BackupManagerViewModel _viewModel = new BackupManagerViewModel();

        // Provides support for multiple languages (English/French)
        private readonly LanguageService _languageService = new LanguageService();

        // Constructor: asks the user to choose a language on startup
        public ConsoleUI()
        {
            Console.WriteLine("Choose your language (en/fr): ");
            string lang = Console.ReadLine().ToLower();
            _languageService.SetLanguage(lang);
        }

        // Displays the main menu and handles user navigation
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

                // Validate numeric input
                bool parsed = int.TryParse(Console.ReadLine(), out choice);

                if (!parsed)
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }

                // Handle menu choices
                switch (choice)
                {
                    case 1:
                        CreateJob(); // Create new backup job
                        break;
                    case 2:
                        RunOneJob(); // Run a specific backup
                        break;
                    case 3:
                        _viewModel.RunAllBackups(); // Run all backups
                        Console.WriteLine(_languageService.Translate("all_jobs_executed"));
                        break;
                }

                Console.WriteLine("\n" + _languageService.Translate("press_enter"));
                Console.ReadLine();

            } while (choice != 4); // Exit condition
        }

        // Handles the creation of a new backup job via user input
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

            // Determine the backup type based on user input
            BackupType backupType = (type.ToLower() == "full" || type.ToLower() == "complet")
                ? BackupType.Full
                : BackupType.Differential;

            // Create the job
            BackupJob job = new BackupJob
            {
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = backupType
            };

            // Validate job paths before adding
            if (job.IsValid())
            {
                _viewModel.CreateBackupJob(name, source, target, backupType);
            }
            else
            {
                string msg = $"Invalid paths during job creation: Source = {source}, Target = {target}";
                Console.WriteLine($"[ERROR] {msg}");

                // Log invalid job creation attempt
                new Logger().LogError(new DirectoryNotFoundException(msg));
            }
        }

        // Prompts the user to run one backup job by name
        private void RunOneJob()
        {
            Console.Write(_languageService.Translate("enter_name"));
            string name = Console.ReadLine();
            _viewModel.RunBackup(name);
        }
    }
}
