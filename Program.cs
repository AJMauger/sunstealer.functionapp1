using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Sunstealer.FunctionApp1.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddOpenTelemetry()
    // ajm: .UseAzureMonitorExporter()
    .WithMetrics(options =>
    {
        options.AddMeter("Sunstealer.FunctionApp1.Worker");
        options.AddAzureMonitorMetricExporter();

        options.AddAspNetCoreInstrumentation();
        options.AddHttpClientInstrumentation();
        options.AddRuntimeInstrumentation();
        options.AddSqlClientInstrumentation();
    })
    .WithTracing(options =>
    {
        options.AddSource("Sunstealer.FunctionApp1.Worker");
        options.AddAzureMonitorTraceExporter();

        options.AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
        });
        // ajm: options.AddAzureSdkInstrumentation();
        // ajm: options.AddEntityFrameworkCoreInstrumentation();
        // ajm: options.AddGrpcClientInstrumentation();
        options.AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        });
        options.AddSqlClientInstrumentation(options =>
        {
            options.RecordException = true;
        });
    });

builder.Services.AddLogging(options =>
{
    options.AddOpenTelemetry(options =>
    {
        options.IncludeScopes = true;
        // ajm: options.IncludeFormattedMessage = true;
    });
});

builder.Services.AddSingleton(options =>
{
    var configuration = options.GetRequiredService<IConfiguration>();
    string connectionString = configuration.GetConnectionString("CosmosDbConnectionString") ?? Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING", EnvironmentVariableTarget.Process) ?? throw new Exception("Cosmos DB connection string.");
    return new CosmosClient(connectionString, new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct
    });
});

builder.Services.AddSingleton<IApplicationService, ApplicationService>();

builder.Logging.AddOpenTelemetry(options =>
{
    options.AddAzureMonitorLogExporter();
});

builder.Build().Run();
