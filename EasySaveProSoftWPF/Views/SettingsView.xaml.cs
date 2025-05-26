using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using EasySaveProSoft.Services;
using EasySaveProSoft.WPF.Services;
using Newtonsoft.Json;
using EasySaveProSoft.WPF.ViewModels;

namespace EasySaveProSoft.WPF.Views
{
    public partial class SettingsView : UserControl
    {
        public LocalizationViewModel Loc { get; set; } = new LocalizationViewModel();

        private const string ConfigFile = "EncryptionExtensions.json";
        private const string BlockedSoftwareFile = "BlockedProcesses.json";
        private List<string> _extensions = new();
        private List<string> _blockedSoftware = new();
        private readonly LanguageService _languageService;
        //private void ChangetoFr()
        //{
        //    MainContentFrame.Content = new SettingsView();
        //}
        public SettingsView()
        {
            InitializeComponent();
            LoadExtensions();
            LoadBlockedSoftware();

            _languageService = new LanguageService();
            //LanguageComboBox.SelectedIndex = _languageService.CurrentLanguage == "en" ? 0 : 1;

            string currentFormat = LoadCurrentFormat();
            foreach (ComboBoxItem item in LogFormatComboBox.Items)
            {
                if (item.Tag.ToString() == currentFormat)
                {
                    LogFormatComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        

        // 🔹 Extension logic
        private void AddExtension_Click(object sender, RoutedEventArgs e)
        {
            string extension = ExtensionTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(extension) && !_extensions.Contains(extension))
            {
                _extensions.Add(extension);
                ExtensionsListBox.Items.Add(extension);
                ExtensionTextBox.Clear();
            }
        }

        private void RemoveExtension_Click(object sender, RoutedEventArgs e)
        {
            if (ExtensionsListBox.SelectedItem != null)
            {
                string selected = ExtensionsListBox.SelectedItem.ToString();
                _extensions.Remove(selected);
                ExtensionsListBox.Items.Remove(selected);
            }
        }

        private void SaveExtensions_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(_extensions));
            MessageBox.Show(WpfLanguageService.Instance.Translate("msg_extensions_saved"));
        }

        private void LoadExtensions()
        {
            if (File.Exists(ConfigFile))
            {
                _extensions = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(ConfigFile));
                foreach (var ext in _extensions)
                {
                    ExtensionsListBox.Items.Add(ext);
                }
            }
        }

        // 🔹 Language
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string langCode = selectedItem.Tag.ToString();
                WpfLanguageService.Instance.SetLanguage(langCode);
                AppConfig.Set("Language", langCode);
                AppConfig.Save();
            }
        }

        // 🔹 Log Format
        private void ApplyLogFormat_Click(object sender, RoutedEventArgs e)
        {
            if (LogFormatComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string format = selectedItem.Tag.ToString();
                new Logger().SetLogFormat(format);
                AppConfig.Set("LogFormat", format);
                AppConfig.Save();
                MessageBox.Show(
    string.Format(WpfLanguageService.Instance.Translate("msg_log_format_set"), format.ToUpper())
);
            }
        }

        private string LoadCurrentFormat()
        {
            const string path = "logformat.txt";
            if (!File.Exists(path)) return "json";
            string format = File.ReadAllText(path).Trim().ToLower();
            return (format == "json" || format == "xml") ? format : "json";
        }

        // 🔹 Blocked Software Logic
        private void LoadBlockedSoftware()
        {
            if (File.Exists(BlockedSoftwareFile))
            {
                _blockedSoftware = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(BlockedSoftwareFile));
                foreach (var exe in _blockedSoftware)
                    BlockedSoftwareListBox.Items.Add(exe);
            }
        }

        private void SaveBlockedSoftware_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(BlockedSoftwareFile, JsonConvert.SerializeObject(_blockedSoftware));
            MessageBox.Show(WpfLanguageService.Instance.Translate("msg_blocked_saved"));

        }

        private void AddBlockedSoftware_Click(object sender, RoutedEventArgs e)
        {
            string exe = BlockedSoftwareTextBox.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(exe) && !_blockedSoftware.Contains(exe))
            {
                _blockedSoftware.Add(exe);
                BlockedSoftwareListBox.Items.Add(exe);
                BlockedSoftwareTextBox.Clear();
            }
        }

        private void RemoveBlockedSoftware_Click(object sender, RoutedEventArgs e)
        {
            if (BlockedSoftwareListBox.SelectedItem is string selected)
            {
                _blockedSoftware.Remove(selected);
                BlockedSoftwareListBox.Items.Remove(selected);
            }
        }

        private void BrowseBlockedSoftware_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe"
            };
            if (dialog.ShowDialog() == true)
            {
                string processName = Path.GetFileName(dialog.FileName).ToLower();
                BlockedSoftwareTextBox.Text = processName;
            }
        }

        private void LogFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


        }
    }
}
