using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Runtime.CompilerServices;
using Sunstealer.FunctionApp1.Models;

namespace Sunstealer.FunctionApp1.Monitor;

public static class LoggerExtensions
{
    public static void LogDebug(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[db] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        ConfigurationModel.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [db] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Debug, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    public static void LogInformation(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[in] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        ConfigurationModel.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [in] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Information, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    public static void LogWarning(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[wn] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        ConfigurationModel.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [wn] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Warning, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    public static void LogError(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[er] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        ConfigurationModel.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [er] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Error, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    public static void LogError(this ILogger logger, Exception exception, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[ex] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        ConfigurationModel.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [ex] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["exception"] = exception?.Message ?? "null",
            ["stacktrace"] = exception?.StackTrace ?? "null",
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Critical, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    public static void LogTrace(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[tr] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        ConfigurationModel.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [tr] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Trace, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }
}