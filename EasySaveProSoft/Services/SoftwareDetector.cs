using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace EasySaveProSoft.Services
{
    public class SoftwareDetector
    {
        private static readonly string BlockedSoftwareFile = "BlockedProcesses.json";

        // Load blocked software list from JSON
        public static List<string> LoadBlockedSoftware()
        {
            if (!File.Exists(BlockedSoftwareFile))
            {
                Console.WriteLine("[ERROR] BlockedSoftware.json not found.");
                return new List<string>();

            }


//

            try
            {
                var json = File.ReadAllText(BlockedSoftwareFile);
                var list = JsonConvert.DeserializeObject<List<string>>(json);
                var lowerList = list?.Select(p => p.Trim().ToLower()).ToList() ?? new List<string>();

                Console.WriteLine("[LOADED BLOCKED SOFTWARE]:");
                foreach (var name in lowerList)
                    Console.WriteLine($"- {name}");

                return lowerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load blocked software list: {ex.Message}");
                return new List<string>();
            }
        }


        // Check if any blocked process is running
        public static bool IsBlockedSoftwareRunning()
        {
            var blockedList = LoadBlockedSoftware();

            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    string runningProcess = process.ProcessName.ToLower().Trim() + ".exe";
                    Console.WriteLine($"[CHECKING] {runningProcess}");

                    if (blockedList.Any(blocked => runningProcess.Equals(blocked, StringComparison.OrdinalIgnoreCase)))

                    {
                        Console.WriteLine($"[BLOCKED] {runningProcess} is running and blocked.");
                        return true;
                    }
                }
                catch
                {
                    // Ignore
                }
            }

            Console.WriteLine("[OK] No blocked software is currently running.");
            return false;
        }




        // Return the first blocked process found (for UI display)
        public static string GetFirstBlockedProcess()
        {
            var blockedList = LoadBlockedSoftware();
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                try
                {
                    string processName = process.ProcessName.ToLower() + ".exe";
                    if (blockedList.Contains(processName))
                        return processName;
                }
                catch { }
            }

            return null;
        }
    }
}
