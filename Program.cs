using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using FunctionApp1;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using FunctionApp1.Models;

var builder = WebApplication.CreateBuilder(args);
var appConfig = builder.Configuration.GetConnectionString("AppConfig");
var myValue = builder.Configuration.GetValue<string>("MyKey");

var hostBuilder = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker => worker.UseNewtonsoftJson())
    .ConfigureServices(services =>
    {

        services.AddDbContextFactory<ApplicationDbContext>(options => {
            var connection = builder.Configuration.GetConnectionString("Database");
            options.UseSqlServer(connection);
        });

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton<IApplicationService, ApplicationService>();        
        services.AddHostedService<ApplicationService>();
    });

    Logger.Instance.LogInformation("ConfigureAppConfiguration()", "ConfigureAppConfiguration() external");

    try {
        hostBuilder.ConfigureAppConfiguration(builder =>
        {
            try {
                Logger.Instance.LogInformation("ConfigureAppConfiguration()", "ConfigureAppConfiguration() internal");

                if (ConfigurationModel.env != Environments.Development)
                {
                    builder.AddAzureAppConfiguration(options =>
                    {
                        // ajm: CUSTOMCONNSTR_ not ConnectionStrings: O_o
                        var connectionString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_AppConfig", EnvironmentVariableTarget.Process);
                        Logger.Instance.LogInformation("ConfigureAppConfiguration().AddAzureAppConfiguration()", $"AppConfig ConnectionString: {connectionString} connecting...");
                        options.Connect(new Uri(connectionString), new ManagedIdentityCredential())
                        /* .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        })*/
                        .Select("Sunstealer::*", "Sunstealer")
                        .ConfigureRefresh(refreshOptions => refreshOptions.Register("Sunstealer:Sentinel", refreshAll: true));
                    Logger.Instance.LogInformation("ConfigureAppConfiguration().AddAzureAppConfiguration()", $"AppConfig ConnectionString: {connectionString} connected.");
                    });
                }
            }
            catch(Exception e) {
                Logger.Instance.LogException(e, "ConfigureAppConfiguration() internal", "ConfigureAppConfiguration()");
            }
        });
    }
    catch(Exception e) {
        Logger.Instance.LogException(e, "ConfigureAppConfiguration() external", "ConfigureAppConfiguration()");
    }

    var host = hostBuilder.Build();

host.Run();
