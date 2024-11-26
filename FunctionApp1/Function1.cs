using Asp.Versioning;
using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;

namespace FunctionApp1;

public class Functions
{
    private readonly IApplicationService _application;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger _logger;

    public Functions(IApplicationService application, IConfiguration configuration, IDbContextFactory<ApplicationDbContext> dbContextFactory, ILoggerFactory loggerFactory)
    {
        try
        {
            _application = application;
            _configuration = configuration;
            _dbContextFactory = dbContextFactory;
            _logger = loggerFactory.CreateLogger<Functions>();

            Logger.Instance.LogInformation($"Functions.Functions() _configuration: {_configuration}.", string.Empty);
        }
        catch(Exception e)
        {
            _logger?.LogError(e, $"Function.Function().");
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

            var uri = new Uri(connection);
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

            using (var db = this._dbContextFactory.CreateDbContext())
            {
                var view = "View2";
                var sql = $"drop TABLE [dbo].[{view}];";
                try
                {
                    db.Database.ExecuteSql(FormattableStringFactory.Create(sql));
                } 
                catch(Exception) { }

                System.IO.FileInfo fi = new($"{view}.txt");
                System.IO.StreamReader sr = fi.OpenText();
                string? header = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    sql = $"CREATE TABLE[dbo].[{view}] (";
                    var fields = header.Split(" ", StringSplitOptions.TrimEntries);
                    foreach(var field in fields) {
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
                    while((line = sr.ReadLine()) != null)
                    {
                        line= line.Trim();
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
                        db.Database.ExecuteSql(FormattableStringFactory.Create(sql));
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

    [Function("Test1")]
    [OpenApiOperation(operationId: "Test1", Description = "Test1")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult Test1([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.Test1().", string.Empty);
            return new OkObjectResult(Logger.Instance.list);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.Tets1()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    [Function("Test2")]
    [OpenApiOperation(operationId: "Test2", Description = "Test2")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult Test2([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.Test2().", string.Empty);

            var list = this._configuration.AsEnumerable();
            foreach (var i in list) {
                Logger.Instance.LogInformation("Function1.Test2()", $"{i.Key} = {i.Value}.");
            }

            var data = new Dictionary<string, string>();            
            try
            {
                data.Add("secret", this._configuration.GetValue<string>("Sunstealer::Secret") ?? string.Empty);
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e, "Function1.Test2()", string.Empty);
            }

            try
            {
                data.Add("nonSecret", this._configuration.GetValue<string>("Sunstealer::NonSecret") ?? string.Empty);
            }
            catch (Exception e)
            {
                Logger.Instance.LogException(e, "Function1.Test2()", string.Empty);
            }

            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.Test2()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    [Function("Test3")]
    [OpenApiOperation(operationId: "Test3", Description = "Test3")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult Test3([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.Test3().", string.Empty);

            List<Table1> data = new List<Table1>();
            using (var db = this._dbContextFactory.CreateDbContext())
            {
                var now = DateTime.Now;
                data = db.table1.Where(f => f.UUID > 0).ToList();
                data.ForEach(f => f.Number1++);
                db.SaveChanges();
                Logger.Instance.LogInformation($"Duration: {DateTime.Now - now}", string.Empty);
            }
            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.Test3()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }
}
