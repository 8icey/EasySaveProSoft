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
                    {"lbl_blocked", "Blocked Software Packages:"},
                    {"btn_save_blocked", "Save Blocked Software's"},
                    { "msg_fill_fields", "Please fill in all fields to create a backup job." },
                    { "msg_job_created", "Backup job '{0}' created!" },
                    { "msg_invalid_paths", "Invalid source or target paths." },
                    { "msg_blocked_software", "Execution blocked. Please close {0} to proceed." },
                    { "msg_job_executed", "Backup job '{0}' executed." },
                    { "msg_all_executed", "All backups executed." },
                    { "msg_no_jobs", "No backup jobs found." },
                    { "msg_job_deleted", "Backup job '{0}' deleted." },
                    { "msg_extensions_saved", "Extensions saved successfully!" },
{ "msg_log_format_set", "Log format set to: {0}" },
{ "msg_blocked_saved", "Blocked software list saved!" },
                    { "btn_apply_format", "Apply Format" },
                    { "btn_pause", "Pause" },
                    { "btn_resume", "Resume" },
                    { "btn_stop", "Stop" },
                    { "lbl_priority", "Priority Extensions :" },
                    { "btn_moveup", "Go Up" },
                    { "btn_movedown", "Go Down" },
                    { "btn_add_priority", "Add" },
                    { "btn_remove_priority", "Remove" },
                     { "LblLargeFileThreshold", "Large File Threshold : " },


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
                    {"lbl_blocked", "Packages logiciels bloqués :"},
                     {"btn_save_blocked", "Enregistrer les logiciels bloqués"},
                     { "msg_fill_fields", "Veuillez remplir tous les champs pour créer un travail de sauvegarde." },
                    { "msg_job_created", "Travail de sauvegarde '{0}' créé !" },
                      { "msg_invalid_paths", "Chemins source ou cible invalides." },
                    { "msg_blocked_software", "Exécution bloquée. Veuillez fermer {0} pour continuer." },
                     { "msg_job_executed", "Travail de sauvegarde '{0}' exécuté." },
                       { "msg_all_executed", "Tous les travaux de sauvegarde ont été exécutés." },
                    { "msg_no_jobs", "Aucun travail de sauvegarde trouvé." },
                    { "msg_job_deleted", "Travail de sauvegarde '{0}' supprimé." },
                    { "msg_extensions_saved", "Extensions enregistrées avec succès !" },
{ "msg_log_format_set", "Format de journal défini sur : {0}" },
{ "msg_blocked_saved", "Liste des logiciels bloqués enregistrée !" },
 { "btn_apply_format", "Appliquer le format" },
 { "btn_pause", "Pause" },
 { "btn_resume", "Continue" },
 { "btn_stop", "Arrêt" },
 { "lbl_priority", "Extensions prioritaires :" },
 { "btn_moveup", "Monter" },
                    { "btn_movedown", "Descendre" },
                    { "btn_add_priority", "Ajouter" },
                    { "btn_remove_priority", "Retirer" },
                     { "LblLargeFileThreshold", "Seuil de gros fichier : " },



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