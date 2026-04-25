using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using ProcessReportService.Services; // Make sure this namespace matches your project

public static class ProcessReporter
{
    public static string GenerateJsonReport()
    {
        var report = new ProcessReport
        {
            Timestamp = DateTime.Now,
            Processes = new List<ProcessInfo>()
        };

        // Ask GameDetector which games are running
        List<string> runningGames = GameDetector.GetRunningGames();

        // If no games are running, return empty report
        if (runningGames.Count == 0)
            return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });

        foreach (var p in Process.GetProcesses())
        {
            try
            {
                // Skip protected processes
                try { _ = p.StartTime; }
                catch { continue; }

                // Check if this process belongs to a detected game
                bool isGameProcess = runningGames.Any(game =>
                    p.ProcessName.Contains(game, StringComparison.OrdinalIgnoreCase));

                if (!isGameProcess)
                    continue;

                DateTime st = p.StartTime;
                DateTime now = DateTime.Now;

                report.Processes.Add(new ProcessInfo
                {
                    Name = p.ProcessName,
                    PID = p.Id,
                    MemoryMB = p.WorkingSet64 / 1024 / 1024,
                    StartTime = st.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndTime = now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Uptime = (now - st).ToString(@"hh\:mm\:ss")
                });
            }
            catch
            {
                // Skip inaccessible processes
            }
        }

        return JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
