
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sunstealer.FunctionApp1.Models;

namespace Sunstealer.FunctionApp1;

public interface IApplicationService
{
    public ConfigurationModel Configuration { get; }
}

public class ApplicationService: IApplicationService, IHostedService
{
    private readonly IConfiguration confliguration;
    private readonly ILogger<Functions> logger;

    public static LogLevel _logLevel = LogLevel.Trace;

    public ConfigurationModel Configuration { get; set; } = new ConfigurationModel();

    public ApplicationService(IConfiguration confliguration, ILogger<Functions> logger) {
        this.confliguration = confliguration;
        this.logger = logger;

        Logger.Instance.LogInformation("ApplicationService.StartAsync()", "ApplicationService.Async()");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try {
            Logger.Instance.LogInformation("ApplicationService.StartAsync()", "ApplicationService.StartAsync()");
            var myValue1 = this.confliguration.GetValue<string>("MyKey");
            var myValue2 = Environment.GetEnvironmentVariable("MyKey");

            this.logger.LogInformation($"ApplicationService.StartAsync() MyKey = {myValue1} {myValue2}");
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "ApplicationService.StartAsync()", "ApplicationService.StartAsync()");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try {
            Logger.Instance.LogInformation("ApplicationService.StopAsync()", "ApplicationService.StopAsync()");
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "ApplicationService.StopAsync()", "ApplicationService.StopAsync()");
        }

        return Task.CompletedTask;
    }
}
