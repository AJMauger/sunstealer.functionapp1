using Microsoft.Extensions.Logging;

namespace Sunstealer.FunctionApp1.Services;

internal class UpdateService
{
    private readonly IApplicationService? _application;
    private readonly ILogger? _logger;

    public UpdateService(IApplicationService application, ILogger<UpdateService> logger)
    {
        _application = application ?? throw new Exception("application");
        _logger = logger ?? throw new Exception("logger");

        _logger.LogInformation($"UpdateService.UpdateService().", string.Empty);
    }
}
