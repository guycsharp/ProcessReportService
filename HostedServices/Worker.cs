using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using ProcessReportService.Services;   

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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var games = GameDetector.GetRunningGames();

                if (games.Any())
                {
                    string json = ProcessReporter.GenerateJsonReport();
                    _storage.Save(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during detection");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}

