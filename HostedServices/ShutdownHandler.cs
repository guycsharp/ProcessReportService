using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

public class ShutdownHandler : IHostedService
{
    private readonly IReportGenerator _generator;
    private readonly IReportStorage _storage;

    public ShutdownHandler(IReportGenerator generator, IReportStorage storage)
    {
        _generator = generator;
        _storage = storage;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            string json = _generator.Generate();
            _storage.Save(json);
        }
        catch
        {
            // swallow — shutdown must not block
        }

        return Task.CompletedTask;
    }
}
