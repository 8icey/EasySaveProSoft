using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveProSoft.Services
{
    // Provides simple multi-language translation for menu texts and prompts
    public class LanguageService
    {
        // Holds the currently selected language dictionary (e.g. "en", "fr")
        private Dictionary<string, string> _currentDictionary;

        // Stores the language code currently in use
        public string CurrentLanguage { get; private set; }


        // All supported translations are defined in this dictionary
        // Each language maps to its own string-key/value dictionary
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "en", new Dictionary<string, string>()
                {
                    {"menu_title", "===== EasySave ProSoft v1.0 ====="},
                    {"menu_create", "1. Create Backup Job"},
                    {"menu_run_one", "2. Run One Backup Job"},
                    {"menu_run_all", "3. Run All Backup Jobs"},
                    {"menu_exit", "4. Exit"},
                    {"menu_choice", "Choose an option: "},
                    {"enter_name", "Enter job name: "},
                    {"enter_source", "Enter source path: "},
                    {"enter_target", "Enter target path: "},
                    {"enter_type", "Enter backup type (Full/Differential): "},
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
                    {"menu_create", "1. Créer un travail de sauvegarde"},
                    {"menu_run_one", "2. Exécuter un travail de sauvegarde"},
                    {"menu_run_all", "3. Exécuter tous les travaux de sauvegarde"},
                    {"menu_exit", "4. Quitter"},
                    {"menu_choice", "Choisissez une option : "},
                    {"enter_name", "Entrez le nom du travail : "},
                    {"enter_source", "Entrez le chemin source : "},
                    {"enter_target", "Entrez le chemin cible : "},
                    {"enter_type", "Entrez le type de sauvegarde (Complet/Différentiel) : "},
                    {"job_added", "Travail de sauvegarde ajouté avec succès !"},
                    {"job_not_found", "Aucun travail trouvé avec ce nom."},
                    {"all_jobs_executed", "Tous les travaux de sauvegarde ont été exécutés."},
                    {"press_enter", "Appuyez sur Entrée pour continuer..."}
                }
            }
        };

        // Selects a language by code (e.g., "en", "fr")
        // Falls back to English if the language is unsupported
        public void SetLanguage(string langCode)
        {
            if (_translations.ContainsKey(langCode))
            {
                _currentDictionary = _translations[langCode];
                CurrentLanguage = langCode;
            }
            else
            {
                Console.WriteLine("Language not supported. Defaulting to English.");
                _currentDictionary = _translations["en"];
                CurrentLanguage = "en";
            }
        }

        // Retrieves the translated string based on a given key
        // If the key is not found, the key itself is returned as fallback
        public string Translate(string key)
        {
            return _currentDictionary.ContainsKey(key) ? _currentDictionary[key] : key;
        }
    }
}
