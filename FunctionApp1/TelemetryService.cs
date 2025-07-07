using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Runtime.CompilerServices;

namespace Sunstealer.FunctionApp1;

public interface ITelemetryService
{
    public void TrackEvent(string eventName, Dictionary<string, double>? metrics = null, Dictionary<string, string>? properties = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
    public void TrackException(Exception e, string message, Dictionary<string, double>? metrics = null, Dictionary<string, string>? properties = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
    public void TrackTrace(string message, SeverityLevel severity, Dictionary<string, string>? properties = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);
}

public class TelemetryService: ITelemetryService
{
    public TelemetryClient _client;
    private readonly Dictionary<string, string> _globalProperties;

    // ajm: ---------------------------------------------------------------------------------------
    public TelemetryService(string connectionString, Dictionary<string, string> globalProperties)
    {
        var config = TelemetryConfiguration.CreateDefault();
        config.ConnectionString = connectionString;
        _client = new TelemetryClient(config);
        _globalProperties = globalProperties;
    }

    // ajm: ---------------------------------------------------------------------------------------
    public void TrackDepemdency(string dependencyName, Dictionary<string, double>? metrics = null, Dictionary<string, string>? properties = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var dt = new DependencyTelemetry()
        {
            Duration = TimeSpan.Zero,
            Name = dependencyName,
            Timestamp = DateTimeOffset.UtcNow
        };
        foreach (var m in metrics ?? [])
        {
            dt.Metrics.Add(m.Key, m.Value);
        }
        var stackFrame = StackFrame.GetStackFrame(filePath, lineNumber);
        foreach (var sf in stackFrame ?? [])
        {
            dt.Properties.Add(sf.Key, sf.Value);
        }
        foreach (var p in properties ?? [])
        {
            dt.Properties.Add(p.Key, p.Value);
        }
        _client.TrackDependency(dt);
        _client.Flush();
    }

    // ajm: ---------------------------------------------------------------------------------------
    public void TrackEvent(string eventName, Dictionary<string, double>? metrics = null, Dictionary<string, string>? properties = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var et = new EventTelemetry()
        {
            Name = eventName,
        };
        foreach (var m in metrics ?? [])
        {
            et.Metrics.Add(m.Key, m.Value);
        }
        var stackFrame = StackFrame.GetStackFrame(filePath, lineNumber);
        foreach (var sf in stackFrame ?? [])
        {
            et.Properties.Add(sf.Key, sf.Value);
        }
        foreach (var p in properties ?? [])
        {
            et.Properties.Add(p.Key, p.Value);
        }
        _client.TrackEvent(et);
        _client.Flush();
    }

    // ajm: ---------------------------------------------------------------------------------------
    public void TrackException(Exception e, string message, Dictionary<string, double>? metrics = null, Dictionary<string, string>? properties = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var et = new ExceptionTelemetry()
        {
            Exception = e,
            Message = message
        };
        foreach (var m in metrics ?? [])
        {
            et.Metrics.Add(m.Key, m.Value);
        }
        foreach (var p in properties ?? [])
        {
            et.Properties.Add(p.Key, p.Value);
        }
        _client.TrackException(et);
        _client.Flush();
    }

    // ajm: ---------------------------------------------------------------------------------------
    public void TrackRequest(string requestName, Uri uri, Dictionary<string, double>? metrics = null, Dictionary<string, string>? properties = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var rt = new RequestTelemetry()
        {
            Duration = TimeSpan.Zero,
            Name = requestName,
            Timestamp = DateTimeOffset.UtcNow,
            Url = uri
        };
        foreach (var m in metrics ?? [])
        {
            rt.Metrics.Add(m.Key, m.Value);
        }
        var stackFrame = StackFrame.GetStackFrame(filePath, lineNumber);
        foreach (var sf in stackFrame ?? [])
        {
            rt.Properties.Add(sf.Key, sf.Value);
        }
        foreach (var p in properties ?? [])
        {
            rt.Properties.Add(p.Key, p.Value);
        }
        _client.TrackRequest(rt);
        _client.Flush();
    }

    // ajm: ---------------------------------------------------------------------------------------
    public void TrackTrace(string message, SeverityLevel severity, Dictionary<string, string>? properties = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        var tt = new TraceTelemetry()
        {
            Message = message,
            SeverityLevel = severity
        };
        tt.Properties.Add("FlowId", CallContext.GetData(Flow.FlowIdName)?.ToString());
        tt.Properties.Add("SpanId", CallContext.GetData(Flow.SpanIdName)?.ToString());
        var stackFrame = StackFrame.GetStackFrame(filePath, lineNumber);
        foreach (var sf in stackFrame ?? [])
        {
            tt.Properties.Add(sf.Key, sf.Value);
        }
        foreach (var p in properties ?? [])
        {
            tt.Properties.Add(p.Key, p.Value);
        }
        _client.TrackTrace(tt);
        _client.Flush();
    }
}
