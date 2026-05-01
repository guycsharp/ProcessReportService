using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using ProcessReportService.Services;

public static class ProcessReporter
{
    public static string GenerateJsonReport()
    {
        var report = new ProcessReport
        {
            Timestamp = DateTime.Now,
            Processes = new List<ProcessInfo>()
        };

        // Get detected game processes (these are process names)
        List<string> runningGameProcesses = GameDetector.GetRunningGames();

        // If no games detected, return empty report
        if (runningGameProcesses.Count == 0)
            return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });

        foreach (var p in Process.GetProcesses())
        {
            try
            {
                // Skip protected processes
                try { _ = p.StartTime; }
                catch { continue; }

                // Match by exact process name (case-insensitive)
                bool isGameProcess = runningGameProcesses.Any(gameProc =>
                {
                    if (gameProc.Equals("Roblox", StringComparison.OrdinalIgnoreCase))
                        return p.ProcessName.StartsWith("RobloxPlayer", StringComparison.OrdinalIgnoreCase);

                    return p.ProcessName.Equals(gameProc, StringComparison.OrdinalIgnoreCase);
                });


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
