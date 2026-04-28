using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

Host.CreateDefaultBuilder(args)
#if DEBUG
    .UseConsoleLifetime()   // <-- Worker runs when pressing F5
#else
    .UseWindowsService()    // <-- Worker runs when installed as a service
#endif
    .ConfigureServices(services =>
    {
        services.AddSingleton<IReportGenerator, JsonReportGenerator>();
        services.AddSingleton<IReportStorage>(new FileReportStorage(
            @"C:\ProgramData\ProcessReport\last_report.json"));
        services.AddSingleton<IReportSender, EmailReportSender>();

        services.AddHostedService<Worker>();
        services.AddHostedService<ShutdownHandler>();
    })
    .Build()
    .Run();
