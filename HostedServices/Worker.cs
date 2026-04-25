using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IReportStorage _storage;
    private readonly IReportSender _sender;

    public Worker(ILogger<Worker> logger, IReportStorage storage, IReportSender sender)
    {
        _logger = logger;
        _storage = storage;
        _sender = sender;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_storage.Exists())
            return;

        try
        {
            string json = _storage.Load();
            await _sender.SendAsync(json);
            _storage.Delete();
        }
        catch
        {
            // keep file for next boot
        }
    }
}
