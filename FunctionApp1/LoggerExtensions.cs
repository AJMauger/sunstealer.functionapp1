using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Sunstealer.FunctionApp1;

public static class LoggerExtensions
{
    public static void LogDebug(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var loggerState = new LoggerState(message, filePath, lineNumber);
        logger.Log(LogLevel.Debug, new EventId(0, "ajm: TODO"), loggerState, null, (state, exception) => loggerState.ToString());
    }

    public static void LogInformation(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var loggerState = new LoggerState(message, filePath, lineNumber);
        logger.Log(LogLevel.Information, new EventId(0), loggerState, null, (state, exception) => loggerState.ToString());
    }

    public static void LogWarning(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var loggerState = new LoggerState(message, filePath, lineNumber);
        logger.Log(LogLevel.Warning, new EventId(0), loggerState, null, (state, exception) => loggerState.ToString());
    }

    public static void LogError(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var loggerState = new LoggerState(message, filePath, lineNumber);
        logger.Log(LogLevel.Error, new EventId(0), loggerState, null, (state, exception) => loggerState.ToString());
    }

    public static void LogError(this ILogger logger, Exception exception, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var loggerState = new LoggerState(message, filePath, lineNumber);
        logger.Log(LogLevel.Critical, new EventId(0), loggerState, exception, (state, exception) => loggerState.ToString());
    }

    public static void LogTrace(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var loggerState = new LoggerState(message, filePath, lineNumber);
        logger.Log(LogLevel.Trace, new EventId(0), loggerState, null, (state, exception) => message);
    }
}