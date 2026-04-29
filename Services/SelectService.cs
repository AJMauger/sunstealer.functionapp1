using Microsoft.Extensions.Logging;
using Sunstealer.FunctionApp1.Monitor;

namespace Sunstealer.FunctionApp1.Services;

internal class SelectService
{
    private readonly IApplicationService? _application;
    private readonly ILogger? _logger;

    public SelectService(IApplicationService application, ILogger<SelectService> logger)
    {
        _application = application ?? throw new Exception("application");
        _logger = logger ?? throw new Exception("logger");

        _logger.LogInformation($"SelectService.SelectService().", string.Empty);
    }
}
