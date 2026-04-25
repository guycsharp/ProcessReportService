using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;

namespace ProcessReportService.Services
{
    public static class GameDetector
    {
        // ---------------------------------------------------------
        // PUBLIC API
        // ---------------------------------------------------------
        public static List<string> GetRunningGames()
        {
            var games = new List<string>();

            games.AddRange(GetRunningSteamGames());
            games.AddRange(GetRunningEpicGames());
            if (IsRobloxRunning()) games.Add("Roblox");

            return games.Distinct().ToList();
        }

        // ---------------------------------------------------------
        // STEAM DETECTION
        // ---------------------------------------------------------
        private static List<string> GetRunningSteamGames()
        {
            var results = new List<string>();

            foreach (string steamApps in GetAllSteamLibraryFolders())
            {
                foreach (var file in Directory.GetFiles(steamApps, "appmanifest_*.acf"))
                {
                    try
                    {
                        string text = File.ReadAllText(file);

                        string name = ExtractAcfValue(text, "name");
                        string installDir = ExtractAcfValue(text, "installdir");

                        if (string.IsNullOrWhiteSpace(name)) continue;

                        if (IsProcessRunningByFolder(installDir))
                            results.Add(name);
                    }
                    catch { }
                }
            }

            return results;
        }

        private static IEnumerable<string> GetAllSteamLibraryFolders()
        {
            var folders = new List<string>();

            string steamPath = GetSteamInstallPath();
            if (steamPath != null)
            {
                string defaultApps = Path.Combine(steamPath, "steamapps");
                if (Directory.Exists(defaultApps))
                    folders.Add(defaultApps);
            }

            // Parse libraryfolders.vdf for additional drives
            string libraryFile = Path.Combine(steamPath ?? "", "steamapps", "libraryfolders.vdf");
            if (File.Exists(libraryFile))
            {
                string[] lines = File.ReadAllLines(libraryFile);

                foreach (string line in lines)
                {
                    if (line.Contains("\"path\""))
                    {
                        int start = line.IndexOf("\"", line.IndexOf("path") + 5) + 1;
                        int end = line.IndexOf("\"", start);
                        string path = line.Substring(start, end - start);

                        string apps = Path.Combine(path, "steamapps");
                        if (Directory.Exists(apps))
                            folders.Add(apps);
                    }
                }
            }

            return folders.Distinct();
        }

        private static string GetSteamInstallPath()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
                return key?.GetValue("InstallPath")?.ToString();
            }
            catch { return null; }
        }

        private static string ExtractAcfValue(string text, string key)
        {
            int idx = text.IndexOf($"\"{key}\"", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            int start = text.IndexOf("\"", idx + key.Length + 2);
            int end = text.IndexOf("\"", start + 1);

            if (start < 0 || end < 0) return null;

            return text.Substring(start + 1, end - start - 1);
        }

        // ---------------------------------------------------------
        // EPIC GAMES DETECTION
        // ---------------------------------------------------------
        private static List<string> GetRunningEpicGames()
        {
            var results = new List<string>();

            foreach (string manifestDir in GetEpicManifestFolders())
            {
                foreach (var file in Directory.GetFiles(manifestDir, "*.item"))
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var data = JsonSerializer.Deserialize<EpicManifest>(json);

                        if (data == null || string.IsNullOrWhiteSpace(data.DisplayName))
                            continue;

                        if (IsProcessRunningByFolder(data.InstallLocation))
                            results.Add(data.DisplayName);
                    }
                    catch { }
                }
            }

            return results;
        }

        private static IEnumerable<string> GetEpicManifestFolders()
        {
            var folders = new List<string>();

            // Default location
            string defaultDir = @"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests";
            if (Directory.Exists(defaultDir))
                folders.Add(defaultDir);

            // Registry lookup for custom install
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Epic Games\EpicGamesLauncher");
                string path = key?.GetValue("AppDataPath")?.ToString();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    string manifestDir = Path.Combine(path, "Data", "Manifests");
                    if (Directory.Exists(manifestDir))
                        folders.Add(manifestDir);
                }
            }
            catch { }

            return folders.Distinct();
        }

        private class EpicManifest
        {
            public string DisplayName { get; set; }
            public string InstallLocation { get; set; }
        }

        // ---------------------------------------------------------
        // ROBLOX DETECTION
        // ---------------------------------------------------------
        private static bool IsRobloxRunning()
        {
            return Process.GetProcessesByName("RobloxPlayerBeta").Any();
        }

        // ---------------------------------------------------------
        // HELPER: detect running EXE by folder name
        // ---------------------------------------------------------
        private static bool IsProcessRunningByFolder(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName)) return false;

            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    string path = p.MainModule.FileName;
                    if (path.Contains(folderName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch { }
            }

            return false;
        }
    }
}
