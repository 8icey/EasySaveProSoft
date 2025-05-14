using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveProSoft.Models;
using EasySaveProSoft.Services;
using EasySaveProSoft.ViewModels;

namespace EasySaveProSoft.WPF.ViewModels
{
    public class SettingsViewModel
    {
        public string CurrentLanguage { get; set; } = "en";

        public void ChangeLanguage(string lang)
        {
            CurrentLanguage = lang;
            // Update the language service if needed
        }
    }
}

