using Azure.Identity;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sunstealer.FunctionApp1;
using Sunstealer.FunctionApp1.Models;;

var builder = WebApplication.CreateBuilder(args);
var appConfig = builder.Configuration.GetConnectionString("AppConfig");
var myValue = builder.Configuration.GetValue<string>("MyKey");

var properties = new Dictionary<string, string>()
    {
        { "LogLevel", "Trace" }
    };

TelemetryService? ts = null;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<ContextMiddleware>();
    })
    .ConfigureServices(services =>
    {
        var credential = new DefaultAzureCredential();
        var provider = new SqlColumnEncryptionAzureKeyVaultProvider(credential);

        Logger.Instance.LogInformation($"Functions.AlwaysEncrypted().", $"SqlColumnEncryptionAzureKeyVaultProvider.ProviderName: {SqlColumnEncryptionAzureKeyVaultProvider.ProviderName}");
        SqlConnection.RegisterColumnEncryptionKeyStoreProviders(
            new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>()
            {
                { SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, provider }
            });

        services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            var databaseConnectionString = builder.Configuration.GetConnectionString("DatabaseConnectionString");
            options.UseSqlServer(databaseConnectionString);
        })
        .AddApplicationInsightsTelemetryWorkerService(options => {
                var appInsightsConnectionString = builder.Configuration.GetConnectionString("AppInsightsConnectionString");
            options.ConnectionString = appInsightsConnectionString;
            }
        )
        .AddSingleton<IApplicationService, ApplicationService>()
        .AddHostedService<ApplicationService>()
        .AddSingleton<ITelemetryService, TelemetryService>(options =>
        {
            if (ts == null)
            {
                var appInsightsConnectionString = builder.Configuration.GetConnectionString("AppInsightsConnectionString");
                ts = new TelemetryService(appInsightsConnectionString, properties);
            }
            return ts;
        })
        .AddSingleton<ILoggerService, LoggerService>(options =>
        {
            if (ts == null)
            {
                var appInsightsConnectionString = builder.Configuration.GetConnectionString("AppInsightsConnectionString");
                ts = new TelemetryService(appInsightsConnectionString, properties);
            }
            return new LoggerService(ts);
        });
    })
    .ConfigureLogging(options =>
    {
        options.AddApplicationInsights()
            .AddConsole()
            .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
    })
   .ConfigureAppConfiguration(options =>
    {
        try
        {
            Logger.Instance.LogInformation("ConfigureAppConfiguration()", "ConfigureAppConfiguration()");
            if (ts == null)
            {
                var appInsightsConnectionString = builder.Configuration.GetConnectionString("AppInsightsConnectionString");
                ts = new TelemetryService(appInsightsConnectionString, properties);
            }

            // ajm: CUSTOMCONNSTR_ not ConnectionStrings: O_o
            var connectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AppConfig", EnvironmentVariableTarget.Process);
            Logger.Instance.LogInformation("ConfigureAppConfiguration().AddAzureAppConfiguration()", $"AppConfig ConnectionString: {connectionString} connecting ...");
            ts?.TrackTrace($"ConfigureAppConfiguration().AddAzureAppConfiguration() AppConfig ConnectionString: {connectionString} connecting ...", SeverityLevel.Verbose);

            if (ConfigurationModel.env != Environments.Development)
            {
                options.AddAzureAppConfiguration(options =>
                {
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
                        ts?.TrackTrace($"AppConfig ConnectionString: {connectionString} connected.", SeverityLevel.Verbose);
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
