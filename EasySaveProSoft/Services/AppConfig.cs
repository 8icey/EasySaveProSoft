using System;
using System.Collections.Generic;
using System.IO;

namespace EasySaveProSoft.Services
{
    public static class AppConfig
    {
        private static readonly string ConfigFilePath = "config.ini";
        private static readonly Dictionary<string, string> _config = new();

        // Charger au démarrage
        static AppConfig()
        {
            Load();
        }

        public static void Load()
        {
            _config.Clear();
            if (!File.Exists(ConfigFilePath)) return;

            foreach (var line in File.ReadAllLines(ConfigFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.Contains("=")) continue;
                var parts = line.Split('=', 2);
                _config[parts[0].Trim()] = parts[1].Trim();
            }
        }

        public static void Save()
        {
            using StreamWriter writer = new(ConfigFilePath);
            foreach (var entry in _config)
            {
                writer.WriteLine($"{entry.Key}={entry.Value}");
            }
        }

        public static string Get(string key, string defaultValue = "")
        {
            return _config.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static void Set(string key, string value)
        {
            _config[key] = value;
        }

        public static List<string> GetPriorityExtensions()
        {
            string raw = Get("PriorityExtensions", "");
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(raw))
            {
                foreach (var ext in raw.Split(','))
                {
                    var clean = ext.Trim().ToLower();
                    if (!string.IsNullOrEmpty(clean))
                        list.Add(clean.StartsWith(".") ? clean : "." + clean);
                }
            }
            return list;
        }

        public static void SetPriorityExtensions(List<string> extensions)
        {
            string joined = string.Join(",", extensions);
            Set("PriorityExtensions", joined);
            Save();
        }
    }
}
