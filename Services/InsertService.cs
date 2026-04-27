namespace Sunstealer.FunctionApp1.Services;

internal class InsertService
{
    private readonly IApplicationService? _application;

    public InsertService(IApplicationService application)
    {
        _application = application ?? throw new Exception("application");
    }
}