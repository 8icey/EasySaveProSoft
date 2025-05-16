using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySaveProSoft.Services
{
    public class LanguageService : INotifyPropertyChanged
    {
        // 🔥 Event to trigger when language changes
        public event PropertyChangedEventHandler PropertyChanged;
        public string CurrentLanguage { get; private set; } = "en";
        // Stores the current language dictionary
        //public static string MenuCreate => _currentDictionary?["menu_create"] ?? "Create Backup Job";
        //public static string MenuRunOne => _currentDictionary?["menu_run_one"] ?? "Run One Backup Job";
        //public static string MenuDelete => _currentDictionary?["menu_delete"] ?? "Delete Backup Job";
        //public static string MenuExit => _currentDictionary?["menu_exit"] ?? "Exit";

        private Dictionary<string, string> _currentDictionary;

        // Stores all available translations
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "en", new Dictionary<string, string>()
                {
                    {"menu_title", "===== EasySave ProSoft v1.0 ====="},
                    {"menu_create", "Create Backup Job"},
                    {"menu_run_one", "Run One Backup Job"},
                    {"menu_run_all", "Run All Backup Jobs"},
                    {"menu_exit", "Exit"},
                    {"menu_choice", "Choose an option:"},
                    {"enter_name", "Enter job name:"},
                    {"enter_source", "Enter source path:"},
                    {"enter_target", "Enter target path:"},
                    {"enter_type", "Enter backup type (Full/Differential):"},
                    {"job_added", "Backup Job added successfully!"},
                    {"job_not_found", "No job found with that name."},
                    {"all_jobs_executed", "All backups executed."},
                    {"press_enter", "Press Enter to continue..."}
                }
            },
            {
                "fr", new Dictionary<string, string>()
                {
                    {"menu_title", "===== EasySave ProSoft v1.0 ====="},
                    {"menu_create", "Créer un travail de sauvegarde"},
                    {"menu_run_one", "Exécuter un travail de sauvegarde"},
                    {"menu_run_all", "Exécuter tous les travaux de sauvegarde"},
                    {"menu_exit", "Quitter"},
                    {"menu_choice", "Choisissez une option:"},
                    {"enter_name", "Entrez le nom du travail:"},
                    {"enter_source", "Entrez le chemin source:"},
                    {"enter_target", "Entrez le chemin cible:"},
                    {"enter_type", "Entrez le type de sauvegarde (Complet/Différentiel):"},
                    {"job_added", "Travail de sauvegarde ajouté avec succès!"},
                    {"job_not_found", "Aucun travail trouvé avec ce nom."},
                    {"all_jobs_executed", "Tous les travaux de sauvegarde ont été exécutés."},
                    {"press_enter", "Appuyez sur Entrée pour continuer..."}
                }
            }
        };

        public LanguageService()
        {
            SetLanguage("en"); // Default to English
        }

        // 🔄 **Switch Language and Notify**
        public void SetLanguage(string langCode)
        {
            if (_translations.ContainsKey(langCode))
            {
                _currentDictionary = _translations[langCode];
                CurrentLanguage = langCode;
                OnPropertyChanged(null); // Notify all bindings
            }
        }



        // 🔎 **Get Translated Text**
        public string Translate(string key)
        {
            return _currentDictionary.ContainsKey(key) ? _currentDictionary[key] : key;
        }

        // 🔄 **Notify Property Changed**
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
