namespace Sunstealer.FunctionApp1.Services;

internal class UpdateService
{
    private readonly IApplicationService? _application;

    public UpdateService(IApplicationService application)
    {
        _application = application ?? throw new Exception("application");
    }
}
