namespace Sunstealer.FunctionApp1.Services;

internal class SelectService
{
    private readonly IApplicationService? _application;

    public SelectService(IApplicationService application)
    {
        _application = application ?? throw new Exception("application");
    }
}
