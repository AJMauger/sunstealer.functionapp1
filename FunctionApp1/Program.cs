using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using FunctionApp1;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using FunctionApp1.Models;

var builder = WebApplication.CreateBuilder(args);
var appConfig = builder.Configuration.GetConnectionString("AppConfig");
var myValue = builder.Configuration.GetValue<string>("MyKey");

var host = new HostBuilder()
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
    })
    .ConfigureAppConfiguration(builder =>
    {
        try {
            Logger.Instance.LogInformation("ConfigureAppConfiguration()", "ConfigureAppConfiguration()");

            if (ConfigurationModel.env != Environments.Development)
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    var connectionString = Environment.GetEnvironmentVariable("AppConfig", EnvironmentVariableTarget.Process);
                    Logger.Instance.LogInformation("ConfigureAppConfiguration().AddAzureAppConfiguration()", $"AppConfig ConnectionString: {connectionString}");
                    options.Connect(new Uri(connectionString), new ManagedIdentityCredential())
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(new DefaultAzureCredential());
                    })
                    .Select("Sunstealer:*", "Sunstealer")
                    .ConfigureRefresh(refreshOptions => refreshOptions.Register("Sunstealer:Sentinel", refreshAll: true));
                });
            }
        }
        catch(Exception e) {
            Logger.Instance.LogException(e, "ConfigureAppConfiguration()", "ConfigureAppConfiguration()");
        }
    })
    .Build();

host.Run();
