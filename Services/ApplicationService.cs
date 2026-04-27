
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sunstealer.FunctionApp1.Models;
using Sunstealer.FunctionApp1.Monitor;
using System.Text.Json;

namespace Sunstealer.FunctionApp1.Services;

// ajm --------------------------------------------------------------------------------------------
public interface IApplicationService
{
    public ConfigurationModel Configuration { get; }
    public Task Delete(DeleteRequestModel model);
    public Task<ItemResponse<InsertRequestModel>> Insert(InsertRequestModel model);
    public Task<List<Dictionary<string, string>>> Select(SelectRequestModel model);
    public Task Update(UpdateRequestModel model);
}

// ajm --------------------------------------------------------------------------------------------
public class ApplicationService: IApplicationService, IHostedService
{
    private readonly CosmosClient _cosmosClient;

    private readonly IConfiguration? _configuration;
    private readonly ILogger? _logger;

    public static LogLevel _logLevel = LogLevel.Trace;

    public ConfigurationModel Configuration { get; set; } = new ConfigurationModel();

    // ajm ----------------------------------------------------------------------------------------
    public ApplicationService(CosmosClient cosmosClient, IConfiguration configuration, ILogger<ApplicationService> logger)
    {
        _cosmosClient = cosmosClient ?? throw new Exception("cosmosClient");
        _configuration = configuration ?? throw new Exception("configuration");
        _logger = logger ?? throw new Exception("logger");

        _logger.LogInformation($"ApplicationService.ApplicationService().", string.Empty);
    }

    // ajm ----------------------------------------------------------------------------------------
    public async Task Delete(DeleteRequestModel model)
    {
        await Task.Delay(1);
    }

    // ajm ----------------------------------------------------------------------------------------
    public async Task<ItemResponse<InsertRequestModel>> Insert(InsertRequestModel model)
    {
        var container = _cosmosClient.GetContainer("database1", "container1");

        var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
        _logger?.LogDebug($"model: {json}");

        ItemResponse<InsertRequestModel> response = await container.CreateItemAsync(model);
        return response;
    }

    // ajm ----------------------------------------------------------------------------------------
    public async Task<List<Dictionary<string, string>>> Select(SelectRequestModel model)
    {
        var container = _cosmosClient.GetContainer("database1", "container1");
        var query = container.GetItemQueryIterator<Dictionary<string, string>>(new QueryDefinition("SELECT * FROM c"));
        var results = new List<Dictionary<string, string>>();

        while (query.HasMoreResults)
        {
            foreach (var item in await query.ReadNextAsync())
            {
                results.Add(item);
            }
        }

        return results;
    }

    // ajm ----------------------------------------------------------------------------------------
    public async Task Update(UpdateRequestModel model)
    {
        await Task.Delay(1);
    }

    // ajm ----------------------------------------------------------------------------------------
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try {
            _logger?.LogInformation("ApplicationService.StartAsync()");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "ApplicationService.StartAsync()");
        }
        return Task.CompletedTask;
    }

    // ajm ----------------------------------------------------------------------------------------
    public Task StopAsync(CancellationToken cancellationToken)
    {
        try {
            _logger?.LogInformation("ApplicationService.StopAsync()");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "ApplicationService.StopAsync()");
        }

        return Task.CompletedTask;
    }


}
