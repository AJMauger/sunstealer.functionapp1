using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sunstealer.FunctionApp1;
using System.Reflection;

namespace Sunslealer.FunctionApp1.Tests;

public interface ITelemetryClient
{
    void TrackEvent(EventTelemetry telemetry);
}

public class TelemetryProcessor : ITelemetryProcessor
{
    public List<ITelemetry> Items = new List<ITelemetry>();
    public ITelemetryProcessor Next { get; set; }

    public TelemetryProcessor(ITelemetryProcessor next)
    {
        Next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public void Process(ITelemetry item)
    {
        this.Items.Add(item);
    }
}

public class UnitTest1
{
    private LoggerService? _loggerService;
    private TelemetryProcessor _telemetryProcessor;
    private TelemetryClient _telemetryClient;
    private TelemetryService _telemetryService;

    public UnitTest1() 
    {
        var connectionString = "InstrumentationKey=Fake;IngestionEndpoint=https://fake.com/;LiveEndpoint=https://fake.com/;ApplicationId=Fake";
        var properties = new Dictionary<string, string> {
            { "Prop1", "Value1" }
        };

        var services = new ServiceCollection();
        services.AddSingleton(new TelemetryConfiguration {
            ConnectionString = connectionString,
            TelemetryChannel = new InMemoryChannel()
        });
        services.AddSingleton<ITelemetryProcessor, TelemetryProcessor>();
        services.AddLogging();
        services.AddSingleton(_ =>
        {
            TelemetryService ts = new TelemetryService(connectionString, properties);
            return new LoggerService(ts);
        });

        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<TelemetryConfiguration>();
        _telemetryClient = new TelemetryClient(configuration);

        configuration.TelemetryProcessorChainBuilder
            .Use((next) => _telemetryProcessor = new TelemetryProcessor(next))
            .Build();

        _telemetryService = new TelemetryService(connectionString, properties);

        FieldInfo? fieldInfo = typeof(TelemetryService).GetField("_client", BindingFlags.Public | BindingFlags.Instance);
        fieldInfo?.SetValue(_telemetryService, _telemetryClient);
        Assert.True(_telemetryService._client == _telemetryClient);

        _loggerService = serviceProvider.GetRequiredService<LoggerService>();
        // _loggerService = new LoggerService(_telemetryService);

        fieldInfo = typeof(LoggerService).GetField("_telemetryService", BindingFlags.Public | BindingFlags.Instance);
        fieldInfo?.SetValue(_loggerService, _telemetryService);
        Assert.True(_loggerService?._telemetryService == _telemetryService);
    }

    [Fact]
    public void Test1()
    {
        _telemetryService.TrackEvent("AdamEvent", new Dictionary<string, double>(), new Dictionary<string, string>());
        var telemetry = _telemetryProcessor?.Items[0] as EventTelemetry;
        Assert.True(telemetry?.Name == "AdamEvent");
    }

    [Fact]
    public void Test2()
    {
        _telemetryService.TrackTrace("AdamTrace", SeverityLevel.Information, new Dictionary<string, string>());
        var telemetry = _telemetryProcessor?.Items[0] as TraceTelemetry;
        Assert.True(telemetry?.Message == "AdamTrace");
    }

    [Fact]
    public void Test3()
    {
        _loggerService?.LogInformation("AdamInformation");
        var telemetry = _telemetryProcessor?.Items[0] as TraceTelemetry;
        Assert.True(telemetry?.Message == "AdamInformation");
    }
}
