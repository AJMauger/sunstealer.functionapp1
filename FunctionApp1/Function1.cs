using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Web.Http;

namespace Sunstealer.FunctionApp1;

public class Functions
{
    private readonly HttpClient httpClient = new HttpClient();
    private readonly IApplicationService? _application;
    private readonly IConfiguration? _configuration;
    private readonly IDbContextFactory<ApplicationDbContext>? _dbContextFactory;
    private readonly ILoggerService? _logger;
    private readonly ITelemetryService? _telemetryService;


    public Functions(IApplicationService application, IConfiguration configuration, IDbContextFactory<ApplicationDbContext> dbContextFactory, 
        ILoggerFactory loggerFactory, ILoggerService loggerService, ITelemetryService telemetryService)
    {
        try
        {
            _application = application ?? throw new Exception("application");
            _configuration = configuration ?? throw new Exception("configuration");
            _dbContextFactory = dbContextFactory ?? throw new Exception("dbContextFactory");
            // _logger = loggerFactory?.CreateLogger<ILoggerService>();
            _logger = loggerService;
            _telemetryService = telemetryService;

            Logger.Instance.LogInformation($"Functions.Functions() _configuration: {_configuration}.", string.Empty);

            _logger.LogDebug("ILogger Debug Test.");
            _logger.LogInformation("ILogger Information Test.");
            _logger.LogWarning("ILogger Warning Test.");
            _logger.LogError("ILogger Error Test.");
            _logger.LogError(new Exception("AdamException"), "ILogger Exception Test.");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.Function().");
        }
    }

    [Function("SqlOutput")]
    [SqlOutput("[dbo].[table1]", "DatabaseConnectionString")]
    public async Task<Table1> SqlOutput(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
    {
        _logger?.LogInformation($"SqlColumnEncryptionAzureKeyVaultProvider.ProviderName: {SqlColumnEncryptionAzureKeyVaultProvider.ProviderName}.");

        var encryptedX = "EncryptedX";
        var row = await req.ReadFromJsonAsync<Table1>() ?? new Table1
        {
            Encrypted1 = encryptedX,
            Date1 = DateTime.Now,
            Number1 = 2,
            Text1 = "ClearTextX"
        };

        return row;
    }

    [Function("SqlTrigger")]
    public void SqlTrigger(
    [SqlTrigger("[dbo].[table1]", "DatabaseConnectionString")] IReadOnlyList<SqlChange<Table1>> changes, FunctionContext context)
    {
        Logger.Instance.LogInformation($"Functions.SqlTrigger().", string.Empty);

        foreach (var change in changes)
        {
            Logger.Instance.LogInformation($"Change Operation: {change.Operation}, UUID: {change.Item.UUID}, Encrypted1: {change.Item.Encrypted1}, Date1: {change.Item.Date1}, Number1: {change.Item.Number1}, Text1: {change.Item.Text1}", string.Empty);
            _logger?.LogInformation($"Change Operation: {change.Operation}, UUID: {change.Item.UUID}, Encrypted1: {change.Item.Encrypted1}, Date1: {change.Item.Date1}, Number1: {change.Item.Number1}, Text1: {change.Item.Text1}");
        }
    }

    [Function("AlwaysEncrypted")]
    [OpenApiOperation(operationId: "AlwaysEncrypted", Description = "AlwaysEncrypted")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public async Task<IActionResult> AlwaysEncrypted([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.AlwaysEncrypted().", string.Empty);

            // ajm: test SQLOutput
            /* ajm: var encryptedX = "EncryptedX";
            var requestData = new Table1
            {
                Encrypted1 = encryptedX,
                Date1 = DateTime.Now,
                Number1 = 2,
                Text1 = "ClearTextX"
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("http://localhost:7299/api/SqlOutput", content);

            if (response.IsSuccessStatusCode)
            {
                _logger?.LogInformation("SqlOutput: 200.");
            }
            else
            {
                _logger?.LogError($"SqlOutput: {response.StatusCode}");
            }*/

            var dbContext = _dbContextFactory?.CreateDbContext();

            List<Table1> data = new List<Table1>();
            var now = DateTime.Now;
            if (dbContext != null)
            {
                string encrypted01 = "Encrypted";
                data = dbContext.table1.Where(f => f.Encrypted1 == encrypted01).ToList();
                data.ForEach(f =>
                {
                    Console.WriteLine($"UUID: {f.UUID}, Encrypted1: {f.Encrypted1}, Date1: {f.Date1}, Number1: {f.Number1}, Text1: {f.Text1}"); 
                });
            }
            Logger.Instance.LogInformation($"Duration: {DateTime.Now - now}", string.Empty);
            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.EntityFrameworkLinq()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    [Function("EntityFrameworkLinq")]
    [OpenApiOperation(operationId: "EntityFrameworkLinq", Description = "EntityFrameworkLinq")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult EntityFrameworkLinq([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.EntityFrameworkLinq().", string.Empty);

            List<Table1> data = new List<Table1>();
            using (var db = _dbContextFactory?.CreateDbContext())
            {
                var now = DateTime.Now;
                if (db != null)
                {
                    data = db.table1.Where(f => f.UUID > 0).ToList();
                    data.ForEach(f => f.Number1++);
                    db.SaveChanges();
                }
                Logger.Instance.LogInformation($"Duration: {DateTime.Now - now}", string.Empty);
            }
            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.EntityFrameworkLinq()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    [Function("GetConfiguration")]
    [OpenApiOperation(operationId: "GetConfiguration", Description = "GetConfiguration")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult GetConfiguration([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.GetConfiguration().", string.Empty);

            var list = this._configuration?.AsEnumerable();
            foreach (var i in list ?? [])
            {
                Logger.Instance.LogInformation("Function1.GetConfiguration()", $"{i.Key} = {i.Value}.");
            }

            var data = new Dictionary<string, string>();
            try
            {
                data.Add("key1", this._configuration?.GetValue<string>("Sunstealer::key1") ?? string.Empty);
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e, "Function1.GetConfiguration()", "key1");
            }

            try
            {
                data.Add("secret1", this._configuration?.GetValue<string>("Sunstealer::secret1") ?? string.Empty);
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e, "Function1.GetConfiguration()", "scret1");
            }

            try
            {
                data.Add("key1", this._configuration?.GetValue<string>("Sunstealer::sentinel") ?? string.Empty);
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e, "Function1.GetConfiguration()", "sentinel");
            }

            Logger.Instance.LogInformation("Function1.GetConfiguration()", "IConfiguration.AsEnumerable() start");
            foreach (var kv in _configuration?.AsEnumerable() ?? [])
            {
                Logger.Instance.LogInformation("Function1.GetConfiguration()", "{kv.Key} = {kv.Value}");
            }
            Logger.Instance.LogInformation("Function1.GetConfiguration()", "IConfiguration.AsEnumerable() end");

            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.GetConfiguration()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    [Function("GetSecret")]
    [OpenApiOperation(operationId: "GetSecret", Description = "https://keyvaultsunstealer.vault.azure.net")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    [OpenApiParameter(name: "connection", In = ParameterLocation.Query, Required = true, Type = typeof(string))]
    [OpenApiParameter(name: "secret", In = ParameterLocation.Query, Required = true, Type = typeof(string))]
    public IActionResult GetSecret([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            var connection = req.Query["connection"];
            var secret = req.Query["secret"];
            Logger.Instance.LogInformation($"Functions.GetSecret(connection: {connection}, key: {secret}).", string.Empty);

            var uri = new Uri(connection.First() ?? throw new Exception("connection"));
            var keyVaultClient = new SecretClient(uri, new DefaultAzureCredential());
            var value = keyVaultClient.GetSecret(secret).Value.Value;
            Logger.Instance.LogInformation($"secret: \"{value}\"", string.Empty);

            return new OkObjectResult(value);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.GetSecret()", string.Empty);
            return new InternalServerErrorResult();
        }
    }

    [Function("ImportView1")]
    [OpenApiOperation(operationId: "ImportView1", Description = "ImportView1")]
    public IActionResult ImportView1([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.ImportView1().", string.Empty);

            using (var db = _dbContextFactory?.CreateDbContext() ?? throw new Exception("!_dbContextFactory"))
            {
                var view = "View2";
                var sql = $"drop TABLE [dbo].[{view}];";
                try
                {
                    db.Database.ExecuteSql(FormattableStringFactory.Create(sql));
                }
                catch (Exception) { }

                FileInfo fi = new($"{view}.txt");
                StreamReader sr = fi.OpenText();
                string? header = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    sql = $"CREATE TABLE[dbo].[{view}] (";
                    var fields = header.Split(" ", StringSplitOptions.TrimEntries);
                    foreach (var field in fields)
                    {
                        if (!string.IsNullOrWhiteSpace(field))
                        {
                            sql += $"[{field}] NVARCHAR(50) NOT NULL,";
                        }
                    }
                    sql = sql[..^1];
                    sql += ");";
                    Logger.Instance.LogInformation($"Functions.ImportView1().", $"sql: {sql}");
                    db.Database.ExecuteSql(FormattableStringFactory.Create(sql));

                    sr.ReadLine();

                    string? line = string.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line))
                        {
                            break;
                        }
                        sql = $"INSERT INTO database1..{view} (";
                        foreach (var field in fields)
                        {
                            if (!string.IsNullOrWhiteSpace(field))
                            {
                                sql += $"[{field}],";
                            }
                        }
                        sql = sql[..^1];

                        sql += ") VALUES (";
                        var values = line.Split(" ", StringSplitOptions.TrimEntries);
                        foreach (var value in values)
                        {
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                sql += $"'{value}',";
                            }
                        }
                        sql = sql[..^1];
                        sql += ")";
                        Logger.Instance.LogInformation($"Functions.ImportView1().", $"sql: {sql}");
                        // dbContext.Database.ExecuteSql(FormattableStringFactory.Create(sql));
                    }
                    sr.Close();
                }
            }
            return new OkObjectResult($"OK.");
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.InstallCertificate()", string.Empty);
            return new InternalServerErrorResult();
        }
    }

    [Function("InstallCertificateMy")]
    [OpenApiOperation(operationId: "InstallCertificateMy", Description = "InstallCertificateMy")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult InstallCertificateMy([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.InstallCertificate().", string.Empty);
            var response = req.CreateResponse(HttpStatusCode.OK);
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            byte[] x509 = System.Text.UTF8Encoding.UTF8.GetBytes("-----BEGIN CERTIFICATE-----\r\nMIID7zCCAtegAwIBAgIUGows4wSdXrLyfdT7sO6ygZy+Fy4wDQYJKoZIhvcNAQEL\r\nBQAwgYUxEjAQBgNVBAMMCSouYWptLm5ldDELMAkGA1UEBhMCVVMxCzAJBgNVBAgM\r\nAkNPMQ8wDQYDVQQHDAZEZW52ZXIxEDAOBgNVBAoMB2FqbS5uZXQxCzAJBgNVBAsM\r\nAmNhMSUwIwYJKoZIhvcNAQkBFhZhZGFtbWF1Z2VyQG91dGxvb2suY29tMCAXDTIz\r\nMTIzMDEwMDIyNFoYDzIwNTEwNTE3MTAwMjI0WjCBhTESMBAGA1UEAwwJKi5ham0u\r\nbmV0MQswCQYDVQQGEwJVUzELMAkGA1UECAwCQ08xDzANBgNVBAcMBkRlbnZlcjEQ\r\nMA4GA1UECgwHYWptLm5ldDELMAkGA1UECwwCY2ExJTAjBgkqhkiG9w0BCQEWFmFk\r\nYW1tYXVnZXJAb3V0bG9vay5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK\r\nAoIBAQDMRQ+GU/6nJUBxEKIyH/JC+rlJUqaeqRPEe/29qC4DXgiLBlA1ei+1xd0K\r\nq2Ff0om6Kj6z+bvy1o86zT0be+9Xnw9LQ13eIpZ0u57/+LnDdXsUVe4AfVJLbyWu\r\nN9vDdrlUgHq9MUh3Suo2MPohiQhWCdV+HiVkYKhxTW86ngXl0Tttxa8kRsi7wvhW\r\neu4aO//HYFk+ueuyuojGWlHGiU4C+YOCnR4SESAOJO/vIGDYQCAXyUFlePYlXN56\r\n9BfNXIYGh3n2gFlvEWT002N0MT2f4RV9cfWMAm9a9kRTbUXq0uaxjsaVUzCHZt0y\r\nR8Mkp7V5l1gerggrAw1tYL0BGKUPAgMBAAGjUzBRMB0GA1UdDgQWBBS9qEhWmlN2\r\nHiLpYuVYoW9Ku8/fbjAfBgNVHSMEGDAWgBS9qEhWmlN2HiLpYuVYoW9Ku8/fbjAP\r\nBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQCsMPoptVf1LR/rCLez\r\nbky8D59nEBuVd42GCNcMVdeKbkH95QdVOQKV+/Ko+qEznV1zTfkouj3Wrpl+36Bl\r\ndOKm5hkVgHKEt1wahXU1aNSw+ZW6KRTe/laTEn3NTX/DcvaP7uQLb3RkDVJfGT8/\r\n3QDsuU7+PokEuZyNGagE3pUD5v1K6PwyF/agpl+8hRilp+20nKcsqOLiBkNxpH2u\r\nWsjneBuE4Qum3a54s3DUc5oz4kWNZvZbokQ7YXDLSHF8EC8UdqteLBuaZFX26UyV\r\ngd7+pWss2jRhfjdZpkcr3ZLbLr5G2Bto6g6ec6cBERecJN8c0OoLz8SuA9IKJebn\r\neztr\r\n-----END CERTIFICATE-----");
            store.Add(new X509Certificate2(x509));
            store.Close();
            Logger.Instance.LogInformation($"Certificate installed.", string.Empty);
            return new OkObjectResult($"Certificate installed.");
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.InstallCertificate()", string.Empty);
            return new InternalServerErrorResult();
        }
    }

    [Function("InstallCertificateRoot")]
    [OpenApiOperation(operationId: "InstallCertificateRoot", Description = "InstallCertificateRoot")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult InstallCertificateRoot([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.InstallCertificate().", string.Empty);
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            byte[] x509 = System.Text.UTF8Encoding.UTF8.GetBytes("-----BEGIN CERTIFICATE-----\r\nMIID7zCCAtegAwIBAgIUGows4wSdXrLyfdT7sO6ygZy+Fy4wDQYJKoZIhvcNAQEL\r\nBQAwgYUxEjAQBgNVBAMMCSouYWptLm5ldDELMAkGA1UEBhMCVVMxCzAJBgNVBAgM\r\nAkNPMQ8wDQYDVQQHDAZEZW52ZXIxEDAOBgNVBAoMB2FqbS5uZXQxCzAJBgNVBAsM\r\nAmNhMSUwIwYJKoZIhvcNAQkBFhZhZGFtbWF1Z2VyQG91dGxvb2suY29tMCAXDTIz\r\nMTIzMDEwMDIyNFoYDzIwNTEwNTE3MTAwMjI0WjCBhTESMBAGA1UEAwwJKi5ham0u\r\nbmV0MQswCQYDVQQGEwJVUzELMAkGA1UECAwCQ08xDzANBgNVBAcMBkRlbnZlcjEQ\r\nMA4GA1UECgwHYWptLm5ldDELMAkGA1UECwwCY2ExJTAjBgkqhkiG9w0BCQEWFmFk\r\nYW1tYXVnZXJAb3V0bG9vay5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK\r\nAoIBAQDMRQ+GU/6nJUBxEKIyH/JC+rlJUqaeqRPEe/29qC4DXgiLBlA1ei+1xd0K\r\nq2Ff0om6Kj6z+bvy1o86zT0be+9Xnw9LQ13eIpZ0u57/+LnDdXsUVe4AfVJLbyWu\r\nN9vDdrlUgHq9MUh3Suo2MPohiQhWCdV+HiVkYKhxTW86ngXl0Tttxa8kRsi7wvhW\r\neu4aO//HYFk+ueuyuojGWlHGiU4C+YOCnR4SESAOJO/vIGDYQCAXyUFlePYlXN56\r\n9BfNXIYGh3n2gFlvEWT002N0MT2f4RV9cfWMAm9a9kRTbUXq0uaxjsaVUzCHZt0y\r\nR8Mkp7V5l1gerggrAw1tYL0BGKUPAgMBAAGjUzBRMB0GA1UdDgQWBBS9qEhWmlN2\r\nHiLpYuVYoW9Ku8/fbjAfBgNVHSMEGDAWgBS9qEhWmlN2HiLpYuVYoW9Ku8/fbjAP\r\nBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQCsMPoptVf1LR/rCLez\r\nbky8D59nEBuVd42GCNcMVdeKbkH95QdVOQKV+/Ko+qEznV1zTfkouj3Wrpl+36Bl\r\ndOKm5hkVgHKEt1wahXU1aNSw+ZW6KRTe/laTEn3NTX/DcvaP7uQLb3RkDVJfGT8/\r\n3QDsuU7+PokEuZyNGagE3pUD5v1K6PwyF/agpl+8hRilp+20nKcsqOLiBkNxpH2u\r\nWsjneBuE4Qum3a54s3DUc5oz4kWNZvZbokQ7YXDLSHF8EC8UdqteLBuaZFX26UyV\r\ngd7+pWss2jRhfjdZpkcr3ZLbLr5G2Bto6g6ec6cBERecJN8c0OoLz8SuA9IKJebn\r\neztr\r\n-----END CERTIFICATE-----");
            store.Add(new X509Certificate2(x509));
            store.Close();
            Logger.Instance.LogInformation($"Certificate installed.", string.Empty);
            return new OkObjectResult($"Certificate installed.");
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.InstallCertificate()", string.Empty);
            return new InternalServerErrorResult();
        }
    }

    [Function("Log")]
    [OpenApiOperation(operationId: "Log", Description = "Log")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult Log([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.Log().", "");
            return new OkObjectResult(Logger.Instance.list);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.Log()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    [Function("Telemetry")]
    [OpenApiOperation(operationId: "Telemetry", Description = "Telemetry")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult Telemetry([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            _telemetryService?.TrackEvent("AdamEvent",
                new Dictionary<string, double>()
                {
                    { "metric1", 1.0 }
                },
                new Dictionary<string, string>()
                {
                    { "property1", "value1" }
                });

            _telemetryService?.TrackTrace("AdamTrace", SeverityLevel.Verbose,
                new Dictionary<string, string>()
                {
                    { "property1", "value1" }
                });

            return new OkObjectResult($"OK.");
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.Tets1()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }
}
