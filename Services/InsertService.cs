using Microsoft.Extensions.Logging;
using Sunstealer.FunctionApp1.Monitor;

namespace Sunstealer.FunctionApp1.Services;

internal class InsertService
{
    private readonly IApplicationService? _application;
    private readonly ILogger? _logger;

    public InsertService(IApplicationService application, ILogger<InsertService> logger)
    {
        _application = application ?? throw new Exception("application");
        _logger = logger ?? throw new Exception("logger");

        _logger.LogInformation($"InsertService.InsertService().", string.Empty);
    }
}