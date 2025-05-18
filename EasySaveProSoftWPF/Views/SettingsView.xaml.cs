using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using EasySaveProSoft.Services;
using EasySaveProSoft.WPF.Services;
using Newtonsoft.Json;

namespace EasySaveProSoft.WPF.Views
{
    public partial class SettingsView : UserControl
    {
        private const string ConfigFile = "EncryptionExtensions.json";
        private List<string> _extensions = new List<string>();
        private readonly LanguageService _languageService;

        public SettingsView()
        {
            InitializeComponent();
            LoadExtensions();

            // Initialize LanguageService
            _languageService = new LanguageService();

            // Set the ComboBox value based on the current language
            LanguageComboBox.SelectedIndex = _languageService.CurrentLanguage == "en" ? 0 : 1;
        }

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
            MessageBox.Show("Extensions saved successfully!");
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

        // ✅ NEW: Handle language change
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string langCode = selectedItem.Tag.ToString();
                WpfLanguageService.Instance.SetLanguage(langCode);
            }
        }

    }
}
