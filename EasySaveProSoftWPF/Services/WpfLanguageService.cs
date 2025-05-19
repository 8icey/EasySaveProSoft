using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySaveProSoft.WPF.Services
{
    public class WpfLanguageService : INotifyPropertyChanged
    {
        // Singleton instance
        private static WpfLanguageService _instance;
        public static WpfLanguageService Instance => _instance ??= new WpfLanguageService();

        public event PropertyChangedEventHandler PropertyChanged;

        // Currently selected language
        public string CurrentLanguage { get; private set; } = "en";

        // The current dictionary of translations
        private Dictionary<string, string> _currentDictionary;


        //رلاىرك

        // Available translations
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "en", new Dictionary<string, string>()
                {
                    {"menu_backup_jobs", "Backup Jobs"},
                    {"menu_settings", "Settings"},
                    {"btn_add", "Add"},
                    {"btn_remove", "Remove"},
                    {"btn_save", "Save Extensions"},
                    {"lbl_manage_extensions", "Manage Encrypted Extensions:"},
                    {"lbl_language", "Language:"},
                    {"lbl_name", "Name:"},
                    {"lbl_source_path", "Source Path:"},
                    {"lbl_target_path", "Target Path:"},
                    {"lbl_type", "Type:"},
                    {"btn_create_job", "Create Job"},
                    {"btn_execute_job", "Execute Job"},
                    {"btn_execute_all", "Execute All"},
                    {"btn_delete_job", "Delete Job"},
                }
            },
            {
                "fr", new Dictionary<string, string>()
                {
                    {"menu_backup_jobs", "Travaux de Sauvegarde"},
                    {"menu_settings", "Paramètres"},
                    {"btn_add", "Ajouter"},
                    {"btn_remove", "Supprimer"},
                    {"btn_save", "Enregistrer les Extensions"},
                    {"lbl_manage_extensions", "Gérer les Extensions Chiffrées:"},
                    {"lbl_language", "Langue:"},
                    {"lbl_name", "Nom:"},
                    {"lbl_source_path", "Chemin Source:"},
                    {"lbl_target_path", "Chemin Cible:"},
                    {"lbl_type", "Type:"},
                    {"btn_create_job", "Créer un Travail"},
                    {"btn_execute_job", "Exécuter le Travail"},
                    {"btn_execute_all", "Exécuter Tout"},
                    {"btn_delete_job", "Supprimer le Travail"},
                }
            }
        };

        private WpfLanguageService()
        {
            SetLanguage("en");
        }

        public void SetLanguage(string langCode)
        {
            if (_translations.ContainsKey(langCode))
            {
                _currentDictionary = _translations[langCode];
                CurrentLanguage = langCode;
                OnPropertyChanged(null); // Update all bindings
            }
        }

        public string Translate(string key)
        {
            return _currentDictionary.ContainsKey(key) ? _currentDictionary[key] : key;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
