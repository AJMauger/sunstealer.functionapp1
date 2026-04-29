using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry.Metrics;
using Sunstealer.FunctionApp1.Models;
using Sunstealer.FunctionApp1.Monitor;
using Sunstealer.FunctionApp1.Services;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Web.Http;

namespace Sunstealer.FunctionApp2.Functions;

public class Functions
{
    private readonly ActivitySource _dependency;
    private readonly HttpClient httpClient = new HttpClient();
    private readonly Histogram<double> _duration;
    private readonly Counter<int> _counter;
    private readonly IApplicationService? _application;
    private readonly IConfiguration? _configuration;
    private readonly ILogger? _logger;
    private readonly MeterProvider _meterProvider;

    // ajm: ---------------------------------------------------------------------------------------
    public Functions(IApplicationService application, IConfiguration configuration, ILogger<Functions> logger, IMeterFactory meterFactory, MeterProvider meterProvider)
    {
        _application = application ?? throw new Exception("application");
        _configuration = configuration ?? throw new Exception("configuration");
        _logger = logger ?? throw new Exception("logger");

        _logger.LogInformation($"Functions.Functions().");

        _meterProvider = meterProvider ?? throw new Exception("meterProvider");
        var meter = meterFactory.Create("Sunstealer.FunctionApp1.Worker");
        _counter = meter.CreateCounter<int>("Counter", "count") ?? throw new Exception("counter");
        _duration = meter.CreateHistogram<double>("Duration", "ms") ?? throw new Exception("duration");
        _dependency = new ActivitySource("Sunstealer.FunctionApp1.Worker") ?? throw new Exception("dependency");

        _logger.LogDebug("ILogger Debug Test.");
        _logger.LogInformation("ILogger Information Test.");
        _logger.LogWarning("ILogger Warning Test.");
        _logger.LogError("ILogger Error Test.");
        _logger.LogError(new Exception("AdamException"), "ILogger Exception Test.");
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("GetConfiguration")]
    [OpenApiOperation(operationId: "GetConfiguration", Description = "GetConfiguration")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult GetConfiguration([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            string[] filter = new[]
            {
                "certificate", "clientid", "clientsecret", "connection", "key", "password", "private", "secret", "token", "username"
            };

            _logger?.LogInformation("IConfiguration.AsEnumerable() start");
            var data = new Dictionary<string, string>();
            foreach (var i in _configuration?.AsEnumerable() ?? [])
            {
                string value = i.Value ?? "";
                if (!filter.Any(s => i.Key.Contains(s, StringComparison.OrdinalIgnoreCase)) && !filter.Any(s => value.Contains(s, StringComparison.OrdinalIgnoreCase)))
                {
                    value = value.Replace("4657646b-6198-40ad-9d41-acc207fa9cf1", "<subscription-id>");
                    _logger?.LogInformation($"{i.Key} = {i.Value}.");
                    data.Add(i.Key, value ?? "null");
                }
                else
                {
                    _logger?.LogInformation($"{i.Key} = [REDACTED].");
                    data.Add(i.Key, "[REDACTED]");
                }
            }
            _logger?.LogInformation("IConfiguration.AsEnumerable() end");

            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Function1.GetConfiguration()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("Log")]
    [OpenApiOperation(operationId: "Log", Description = "Log")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult Log([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            _logger?.LogInformation($"Functions.Log().");
            return new OkObjectResult(ConfigurationModel.log);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Function1.Log()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("TrackDependency")]
    [OpenApiOperation(operationId: "TrackDependency", Description = "TrackDependency")]
    public async Task<IActionResult> TrackDependency([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            using (var activity = _dependency.StartActivity("ExternalServiceName", ActivityKind.Client))
            {
                activity?.SetTag("peer.service", "RemoteApiSystem");

                int ms = Random.Shared.Next(1, 1000);
                await Task.Delay(ms);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            return new OkObjectResult("TrackDependency");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.TrackDependency().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
    
    // ajm: ---------------------------------------------------------------------------------------
    [Function("TrackEvent")]
    [OpenApiOperation(operationId: "TrackEvent", Description = "TrackEvent")]
    public async Task<IActionResult> TrackEvent([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            var activity = Activity.Current;
            activity?.AddEvent(new ActivityEvent("CustomEvent", tags: new ActivityTagsCollection
            {
                { "customProperty", "value" }
            }));
            return new OkObjectResult("TrackEvent");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.TrackEvent().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("TrackMetric")]
    [OpenApiOperation(operationId: "TrackMetric", Description = "TrackMetric")]
    public async Task<IActionResult> TrackMetric([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            int ms = Random.Shared.Next(1, 1000);
            await Task.Delay(ms);
            _duration.Record(ms, new TagList { { "CustomDimension", "Duration" } });
            _counter.Add(1, new TagList { { "CustomDimension", "Counter" } });
            _meterProvider.ForceFlush(5000);
            return new OkObjectResult("TrackMetric");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.TrackMetric().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("Delete")]
    [OpenApiOperation(operationId: "Delete", Description = "Delete")]
    [OpenApiRequestBody("application/json", typeof(DeleteRequestModel), Description = "Delete Request Model")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DeleteResponseModel))]
    public async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        try
        {
            if (_application == null)
            {
                _logger?.LogError("ApplicationService is null in Function.Delete().");
                return new InternalServerErrorResult();
            }

            var model = new DeleteRequestModel()
            {
            };
            await _application.Delete(model);

            return new OkObjectResult(null);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.Delete().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("Insert")]
    [OpenApiOperation(operationId: "Insert", Description = "Insert")]
    [OpenApiRequestBody("application/json", typeof(InsertRequestModel), Description = "Insert Request Model")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(InsertResponseModel))]
    public async Task<IActionResult> Insert([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        try
        {
            if (_application == null)
            {
                _logger?.LogError("ApplicationService is null in Function.Insert().");
                return new InternalServerErrorResult();
            }

            var model = new InsertRequestModel()
            {
            };

            var results = await _application.Insert(model);
            return new OkObjectResult(results);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.Insert().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("Select")]
    [OpenApiOperation(operationId: "Select", Description = "Select")]
    [OpenApiRequestBody("application/json", typeof(SelectRequestModel))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SelectResponseModel))]
    public async Task<IActionResult> Select([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        try
        {
            if (_application == null)
            {
                _logger?.LogError("ApplicationService is null in Function.Select().");
                return new InternalServerErrorResult();
            }

            var model = new SelectRequestModel()
            {
            };
            var results = await _application.Select(model);
            return new OkObjectResult(results);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.Seatch().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("Update")]
    [OpenApiOperation(operationId: "Update", Description = "Update")]
    [OpenApiRequestBody("application/json", typeof(UpdateRequestModel))]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UpdateResponseModel))]
    public async Task<IActionResult> Update([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        try
        {
            if (_application == null)
            {
                _logger?.LogError("ApplicationService is null in Function.Update().");
                return new InternalServerErrorResult();
            }

            var model = new UpdateRequestModel()
            {
            };
            await _application.Update(model);
            return new OkObjectResult(null);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.Update().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("Container1")]
    [OpenApiOperation(operationId: "Container1", Description = "Container1")]
    public async Task<IActionResult> Container1([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            var connectionString = _configuration?.GetConnectionString("CosmosDbConnectionString") ?? Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING", EnvironmentVariableTarget.Process);
            var client = new CosmosClient(connectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct
            });
            var database = client.GetDatabase("database1");
            var container = database.GetContainer("container1");
            var properties = (await container.ReadContainerAsync()).Resource;
            var json = JsonConvert.SerializeObject(properties, Formatting.Indented);
            _logger?.LogInformation($"properties: {json}");

            return new OkObjectResult(properties);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.Update().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    [Function("Test")]
    [OpenApiOperation(operationId: "Test", Description = "Container1")]
    public async Task<IActionResult> Test([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            return new OkObjectResult("Test");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.Test().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
