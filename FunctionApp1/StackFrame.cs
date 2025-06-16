using Microsoft.Extensions.Logging;

namespace Sunstealer.FunctionApp1;

public class StackFrame
{
    // ajm: ---------------------------------------------------------------------------------------
    public static Dictionary<string, string> GetStackFrame(string filePath, int lineNumber)
    {
        var properties = new Dictionary<string, string>();

        System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(2);
        var method = sf.GetMethod();

        switch(ApplicationService._logLevel)
        {
            case LogLevel.Trace:
                goto case LogLevel.Debug;
            case LogLevel.Debug:
                properties.Add("filePath", filePath);
                properties.Add("lineNumber", lineNumber.ToString());
                goto case LogLevel.Information;
            case LogLevel.Information:
                properties.Add("method", $"{method?.DeclaringType?.FullName}.{method?.Name}");
                goto case LogLevel.Warning;
            case LogLevel.Warning:
                goto case LogLevel.Error;
            case LogLevel.Error:
                goto case LogLevel.Critical;
            case LogLevel.Critical:
                break;
        }
        return properties;
    }
}
