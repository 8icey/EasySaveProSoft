using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySaveProSoft.Services
{
    public static class AppConfig
    {
        private static readonly string ConfigFilePath = "config.ini";
        private static readonly Dictionary<string, string> _config = new();

        // Load config on first use
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

        // ✅ Read int with fallback
        public static int GetInt(string key, int defaultValue)
        {
            if (_config.TryGetValue(key, out var value) && int.TryParse(value, out var result))
                return result;
            return defaultValue;
        }

        // ✅ Large file threshold (in bytes)
        public static long GetLargeFileThresholdBytes()
        {
            int mb = GetInt("LargeFileThresholdMB", 1000); // default: 1000 MB
            return mb * 1024L * 1024L;
        }

        // ✅ Priority file extensions (comma-separated)
        public static List<string> GetPriorityOrder()
        {
            string raw = Get("PriorityExtensions", "");
            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(ext => ext.Trim().ToLower())
                      .Where(ext => !string.IsNullOrWhiteSpace(ext))
                      .Select(ext => ext.StartsWith(".") ? ext : "." + ext)
                      .ToList();
        }

        public static void SetPriorityOrder(List<string> extensions)
        {
            string joined = string.Join(",", extensions);
            Set("PriorityExtensions", joined);
            Save();
        }

        public static void SetLargeFileThresholdMB(int mb)
        {
            Set("LargeFileThresholdMB", mb.ToString());
            Save(); // Save to config.ini immediately
        }

        public static int GetNetworkThreshold()
        {
            if (_config.TryGetValue("networkThreshold", out string value) && int.TryParse(value, out int result))
                return result;

            return 1000; // défaut : 1000 KB/s
        }

        public static int GetMaxParallelJobsOnHighLoad()
        {
            if (_config.TryGetValue("maxParallelJobsOnHighLoad", out string value) && int.TryParse(value, out int result))
                return result;

            return 1; // défaut : 1 job si surcharge
        }


    }
}
