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

        // ✅ **New Constructor** to select the language at the beginning
        public ConsoleUI()
        {
            string lang;
            do
            {
                Console.WriteLine("Choose your language (en/fr): ");
                lang = Console.ReadLine()?.ToLower().Trim();

                if (string.IsNullOrEmpty(lang))
                {
                    Console.WriteLine("[!] Language cannot be empty. Please type 'en' for English or 'fr' for French.");
                    continue;
                }

                if (lang != "en" && lang != "fr")
                {
                    Console.WriteLine("[!] Language not supported. Defaulting to English.");
                    lang = "en";
                }

                _languageService.SetLanguage(lang);
            } while (lang != "en" && lang != "fr");
        }

        // ✅ **Display Menu** — Main Loop
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
                Console.WriteLine("4. Display Backup Jobs");
                Console.WriteLine("5. Delete Backup Job");
                Console.WriteLine("6. Exit");
                Console.Write(_languageService.Translate("menu_choice"));

                bool parsed = int.TryParse(Console.ReadLine(), out choice);

                if (!parsed)
                {
                    Console.WriteLine("[!] Invalid input. Please enter a number.");
                    continue;
                }

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
                        break;
                    case 4:
                        _viewModel.DisplayJobs();
                        break;
                    case 5:
                        DeleteJob();
                        break;
                    case 6:
                        Console.WriteLine("[✓] Exiting application.");
                        break;
                    default:
                        Console.WriteLine("[!] Invalid option. Please choose between 1 and 6.");
                        break;
                }

                Console.WriteLine("\n" + _languageService.Translate("press_enter"));
                Console.ReadLine();
            } while (choice != 6);
        }

        // ✅ **Create a new Backup Job**
        //private void CreateJob()
        //{
        //    Console.Write(_languageService.Translate("enter_name"));
        //    string name = Console.ReadLine();
        //    while (string.IsNullOrEmpty(name))
        //    {
        //        Console.WriteLine("[!] Job name cannot be empty. Please enter a valid name:");
        //        name = Console.ReadLine();
        //    }

        //    Console.Write(_languageService.Translate("enter_source"));
        //    string source = Console.ReadLine();
        //    while (string.IsNullOrEmpty(source))
        //    {
        //        Console.WriteLine("[!] Source path cannot be empty. Please enter a valid path:");
        //        source = Console.ReadLine();
        //    }

        //    Console.Write(_languageService.Translate("enter_target"));
        //    string target = Console.ReadLine();
        //    while (string.IsNullOrEmpty(target))
        //    {
        //        Console.WriteLine("[!] Target path cannot be empty. Please enter a valid path:");
        //        target = Console.ReadLine();
        //    }

        //    Console.Write(_languageService.Translate("enter_type"));
        //    string type = Console.ReadLine();
        //    BackupType backupType = (type.ToLower() == "full" || type.ToLower() == "complet") ? BackupType.Full : BackupType.Differential;

        //    _viewModel.CreateBackupJob(name, source, target, backupType);
        //}

        private void CreateJob()
        {
            //Console.WriteLine("\n[+] Scanning for external drives...");
            //ExternalDriveService.DisplayExternalDrives();

            Console.Write(_languageService.Translate("enter_name"));
            string name = Console.ReadLine();
            while (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("[!] Job name cannot be empty. Please enter a valid name:");
                name = Console.ReadLine();
            }

            Console.Write(_languageService.Translate("enter_source"));
            string source = Console.ReadLine();
            while (string.IsNullOrEmpty(source) || !Directory.Exists(source))
            {
                Console.WriteLine("[!] Invalid source path or path not found. Please enter a valid path:");
                source = Console.ReadLine();
            }

            Console.Write(_languageService.Translate("enter_target"));
            string target = Console.ReadLine();
            while (string.IsNullOrEmpty(target) || !Directory.Exists(target))
            {
                Console.WriteLine("[!] Invalid target path or path not found. Please enter a valid path:");
                target = Console.ReadLine();
            }

            Console.Write(_languageService.Translate("enter_type"));
            string type = Console.ReadLine();
            BackupType backupType = (type.ToLower() == "full" || type.ToLower() == "complet") ? BackupType.Full : BackupType.Differential;

            _viewModel.CreateBackupJob(name, source, target, backupType);
        }

        // ✅ **Execute One Backup Job**
        private void RunOneJob()
        {
            Console.Write(_languageService.Translate("enter_name"));
            string name = Console.ReadLine();
            while (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("[!] Job name cannot be empty. Please enter a valid name:");
                name = Console.ReadLine();
            }

            _viewModel.RunBackup(name);
        }

        // ✅ **Delete One Backup Job**
        private void DeleteJob()
        {
            Console.Write("Enter the name of the job you want to delete: ");
            string name = Console.ReadLine();
            while (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("[!] Job name cannot be empty. Please enter a valid name:");
                name = Console.ReadLine();
            }

            _viewModel.DeleteBackup(name);
        }
    }
}
