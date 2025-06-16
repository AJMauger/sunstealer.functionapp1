using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.DataContracts;
using Sunstealer.FunctionApp1;
using Sunstealer.FunctionApp1.Models;

var builder = WebApplication.CreateBuilder(args);
var appConfig = builder.Configuration.GetConnectionString("AppConfig");
var myValue = builder.Configuration.GetValue<string>("MyKey");

// ajm: APPLICATIONINSIGHTS_CONNECTION_STRING
string connectionString = "";

var properties = new Dictionary<string, string>()
    {
        { "LogLevel", "Trace" }
    };

TelemetryService? ts = new TelemetryService(connectionString, properties);

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<ContextMiddleware>();
    })
    .ConfigureServices(services =>
    {
        services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            var connection = builder.Configuration.GetConnectionString("DatabaseConnectionString");
            options.UseSqlServer(connection);
        })
        .AddApplicationInsightsTelemetryWorkerService(options =>
            options.ConnectionString = connectionString
        )
        .AddSingleton<IApplicationService, ApplicationService>()
        .AddHostedService<ApplicationService>()
        .AddSingleton<ITelemetryService, TelemetryService>(options =>
        {
            return ts;
        })
        .AddSingleton<ILoggerService, LoggerService>(options =>
        {
            return new LoggerService(ts);
        });
    })
    .ConfigureLogging(options =>
    {
        options.AddApplicationInsights();
    })
   .ConfigureAppConfiguration(builder =>
    {
        try
        {
            Logger.Instance.LogInformation("ConfigureAppConfiguration()", "ConfigureAppConfiguration()");

            if (ConfigurationModel.env != Environments.Development)
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    // ajm: CUSTOMCONNSTR_ not ConnectionStrings: O_o
                    var connectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AppConfig", EnvironmentVariableTarget.Process);
                    Logger.Instance.LogInformation("ConfigureAppConfiguration().AddAzureAppConfiguration()", $"AppConfig ConnectionString: {connectionString} connecting ...");
                    ts?.TrackTrace($"ConfigureAppConfiguration().AddAzureAppConfiguration() AppConfig ConnectionString: {connectionString} connecting ...", SeverityLevel.Verbose);
                    if (connectionString != null)
                    {
                        options.Connect(new Uri(connectionString), new ManagedIdentityCredential())
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        })
                        .Select("Sunstealer::*")
                        .ConfigureRefresh(refreshOptions => refreshOptions.Register("Sunstealer::sentinel", refreshAll: true));

                        Logger.Instance.LogInformation("ConfigureAppConfiguration().AddAzureAppConfiguration()", $"AppConfig ConnectionString: {connectionString} connected.");
                    }
                });
            }
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "ConfigureAppConfiguration()", "ConfigureAppConfiguration()");
        }
    })
    .Build();

host.Run();
