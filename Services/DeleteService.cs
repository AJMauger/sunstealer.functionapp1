namespace Sunstealer.FunctionApp1.Services;

internal class DeleteService
{
    private readonly IApplicationService? _application;

    public DeleteService(IApplicationService application)
    {
        _application = application ?? throw new Exception("application");
    }
}