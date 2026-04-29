using Microsoft.Extensions.Logging;
using Sunstealer.FunctionApp1.Monitor;

namespace Sunstealer.FunctionApp1.Services;

internal class DeleteService
{
    private readonly IApplicationService? _application;
    private readonly ILogger? _logger;

    public DeleteService(IApplicationService application, ILogger<DeleteService> logger)
    {
        _application = application ?? throw new Exception("application");
        _logger = logger ?? throw new Exception("logger");

        _logger.LogInformation($"DeleteService.DeleteService().", string.Empty);
    }
}