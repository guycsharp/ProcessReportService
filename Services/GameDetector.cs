using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

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

            // Roblox special-case detection
            var robloxGame = GetRobloxGame();
            if (robloxGame != null)
            {
                Log($"Roblox detected: {robloxGame}");
                games.Add(robloxGame);
            }

            games = games
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Log($"Final REAL games: {string.Join(", ", games)}");
            return games;
        }

        // ---------------------------------------------------------
        // ROBLOX DETECTION (LOG-BASED placeId)
        // ---------------------------------------------------------
        private static string? GetRobloxGame()
        {
            try
            {
                var proc = Process.GetProcessesByName("RobloxPlayerBeta").FirstOrDefault();
                if (proc == null)
                    return null;

                // First try log-based detection (most reliable)
                string? placeId = GetRobloxPlaceIdFromLogs();
                if (!string.IsNullOrWhiteSpace(placeId))
                {
                    Log("Roblox placeId from logs: " + placeId);
                    return $"Roblox (Place {placeId})";
                }

                // Fallback: Roblox is open but no game is running
                return "Roblox";
            }
            catch (Exception ex)
            {
                Log("Roblox detection error: " + ex.Message);
                return null;
            }
        }

        private static string? GetRobloxPlaceIdFromLogs()
        {
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Roblox", "logs");

                if (!Directory.Exists(logDir))
                    return null;

                // Get newest log file
                string newestLog = Directory.GetFiles(logDir, "*.log")
                    .OrderByDescending(File.GetLastWriteTime)
                    .FirstOrDefault();

                if (newestLog == null)
                    return null;

                // Read log lines
                foreach (var line in File.ReadLines(newestLog))
                {
                    int idx = line.IndexOf("placeId=", StringComparison.OrdinalIgnoreCase);
                    if (idx == -1)
                        continue;

                    idx += "placeId=".Length;

                    string digits = new string(line.Skip(idx).TakeWhile(char.IsDigit).ToArray());
                    if (!string.IsNullOrWhiteSpace(digits))
                        return digits;
                }
            }
            catch (Exception ex)
            {
                Log("Roblox log scan error: " + ex.Message);
            }

            return null;
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

            // Game install folder markers (REAL games)
            var gameInstallMarkers = new[]
            {
                "steamapps\\common",
                "epic games\\",
                "gog galaxy\\games",
                "ubisoft game launcher\\games",
                "ea games\\",
                "origin games\\",
                "riot games\\",
                "battle.net\\",
                "blizzard\\"
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
                bool isRealGame = gameInstallMarkers.Any(marker => normalized.Contains(marker));

                if (!isRealGame)
                    continue;

                Log($"REAL GAME detected: {p.ProcessName} at {exePath}");
                results.Add(p.ProcessName);
            }

            return results;
        }
    }
}
