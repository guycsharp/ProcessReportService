using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ProcessReportService.Services
{
    public static class GameDetector
    {
        private static readonly string LogPath = @"C:\NETCore\ProcessReportService\detector.log";

        private static void Log(string msg)
        {
            try
            {
                File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss} {msg}\n");
            }
            catch { }
        }

        // ---------------------------------------------------------
        // PUBLIC API
        // ---------------------------------------------------------
        public static List<string> GetRunningGames()
        {
            Log("=== GetRunningGames() called ===");

            var games = new List<string>();

            // Folder-based detection only
            games.AddRange(DetectGamesByProcessFolders());

            // Remove duplicates
            games = games
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Log($"Final detected games: {string.Join(", ", games)}");
            return games;
        }

        // ---------------------------------------------------------
        // FOLDER-BASED DETECTION (MAIN LOGIC)
        // ---------------------------------------------------------
        private static List<string> DetectGamesByProcessFolders()
        {
            var results = new List<string>();

            // Keywords that identify game launcher folders
            var keywords = new[]
            {
                "steam", "epic", "gog", "ubisoft", "ea", "origin",
                "riot", "battle", "blizzard"
            };

            foreach (var p in Process.GetProcesses())
            {
                string exePath;
                try
                {
                    exePath = p.MainModule.FileName;
                }
                catch
                {
                    continue;
                }

                string dir = Path.GetDirectoryName(exePath) ?? "";
                if (string.IsNullOrWhiteSpace(dir))
                    continue;

                // Walk up 3 folder levels
                var folders = new List<string>();
                string current = dir;

                for (int i = 0; i < 3; i++)
                {
                    folders.Add(current);
                    current = Directory.GetParent(current)?.FullName ?? "";
                    if (string.IsNullOrWhiteSpace(current))
                        break;
                }

                // Check if any folder contains launcher keywords
                if (folders.Any(f => keywords.Any(k =>
                    f.Contains(k, StringComparison.OrdinalIgnoreCase))))
                {
                    Log($"Folder-based match: {p.ProcessName} at {exePath}");
                    results.Add(p.ProcessName);
                }
            }

            return results;
        }
    }
}
