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

            var games = DetectRealGames();

            games = games
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Log($"Final REAL games: {string.Join(", ", games)}");
            return games;
        }

        // ---------------------------------------------------------
        // REAL GAME DETECTION
        // ---------------------------------------------------------
        private static List<string> DetectRealGames()
        {
            var results = new List<string>();

            // Launcher root folders (NOT games)
            var launcherRoots = new[]
            {
                "steam", "steamapps", "epic games", "gog galaxy",
                "ubisoft game launcher", "ea", "origin", "riot games",
                "battle.net", "blizzard"
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

                string normalized = dir.ToLowerInvariant();

                // Skip launcher folders
                if (launcherRoots.Any(root => normalized.EndsWith(root)))
                    continue;

                // REAL GAME RULE:
                // A real game EXE lives inside a game folder, not the launcher folder.
                bool isRealGame =
                    normalized.Contains("steamapps\\common") ||
                    normalized.Contains("epic games\\") ||
                    normalized.Contains("gog galaxy\\games") ||
                    normalized.Contains("ubisoft game launcher\\games") ||
                    normalized.Contains("ea games\\") ||
                    normalized.Contains("origin games\\") ||
                    normalized.Contains("riot games\\") ||
                    normalized.Contains("battle.net\\") ||
                    normalized.Contains("blizzard\\");

                if (!isRealGame)
                    continue;

                Log($"REAL GAME detected: {p.ProcessName} at {exePath}");
                results.Add(p.ProcessName);
            }

            return results;
        }
    }
}
