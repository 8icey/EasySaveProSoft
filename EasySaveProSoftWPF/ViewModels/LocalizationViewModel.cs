using EasySaveProSoft.WPF.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class LocalizationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private WpfLanguageService _lang => WpfLanguageService.Instance;

        public string BtnAdd => _lang.Translate("btn_add");
        public string BtnRemove => _lang.Translate("btn_remove");
        public string BtnSave => _lang.Translate("btn_save");
        public string LblManageExtensions => _lang.Translate("lbl_manage_extensions");
        public string LblLanguage => _lang.Translate("lbl_language");
        public string LblName => _lang.Translate("lbl_name");
        public string LblSourcePath => _lang.Translate("lbl_source_path");
        public string LblTargetPath => _lang.Translate("lbl_target_path");
        public string LblType => _lang.Translate("lbl_type");
        public string BtnCreateJob => _lang.Translate("btn_create_job");
        public string BtnExecuteJob => _lang.Translate("btn_execute_job");
        public string BtnExecuteAll => _lang.Translate("btn_execute_all");
        public string BtnDeleteJob => _lang.Translate("btn_delete_job");

        public string MenuBackupJobs => _lang.Translate("menu_backup_jobs");
        public string MenuSettings => _lang.Translate("menu_settings");
        public string BtnSaveBlocked => _lang.Translate("btn_save_blocked");
        public string LblBlocked => _lang.Translate("lbl_blocked");
        public string BtnApplyFormat => _lang.Translate("btn_apply_format");
        public string BtnPause => _lang.Translate("btn_pause");
        public string BtnResume => _lang.Translate("btn_resume");
        public string BtnStop => _lang.Translate("btn_stop");





        public string lbl_priority => _lang.Translate("lbl_priority");
        public string btn_moveup => _lang.Translate("btn_moveup");
        public string btn_movedown => _lang.Translate("btn_movedown");
        public string btn_add_priority => _lang.Translate("btn_add_priority");
        public string btn_remove_priority => _lang.Translate("btn_remove_priority");
        public string LblLargeFileThreshold => _lang.Translate("LblLargeFileThreshold");

        public string lbl_settres => _lang.Translate("lbl_settres");
        public string thresres => _lang.Translate("thresres");
        public string surchargemax => _lang.Translate("surchargemax");
        public string applyres => _lang.Translate("applyres");
        

        public LocalizationViewModel()
        {
            _lang.PropertyChanged += (_, _) => OnPropertyChanged(null); // Refresh all bindings when language changes
        }

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
