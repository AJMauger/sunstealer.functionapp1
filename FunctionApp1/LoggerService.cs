using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace Sunstealer.FunctionApp1;

public interface ILoggerService: ILogger;

public class LoggerService : ILoggerService
{
    public TelemetryService _telemetryService;

    public LoggerService(TelemetryService telemetryService)
    {
        _telemetryService = telemetryService;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LoggerState? loggerState = state as LoggerState;
        if (exception == null)
        {
            _telemetryService.TrackTrace(loggerState?.Message ?? "", GetSeverity(logLevel), null, loggerState?.FilePath ?? "", loggerState?.LineNumber ?? 0);
        }
        else
        {
            _telemetryService.TrackException(exception, loggerState?.Message ?? "");
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    public SeverityLevel GetSeverity(LogLevel logLevel)
    {
        return SeverityLevel.Verbose;
    }
}
