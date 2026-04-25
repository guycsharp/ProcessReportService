public class ProcessInfo
{
    public required string Name { get; set; }
    public int PID { get; set; }
    public long MemoryMB { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Uptime { get; set; }
}
