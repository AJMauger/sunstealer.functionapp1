using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace sunstealer.azure.functions;

public class DocumentFilter : IDocumentFilter
{
    // ajm: ---------------------------------------------------------------------------------------
    public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
    {
        var schemas = document.Components.Schemas;
        foreach (var schema in schemas)
        {
            if (schema.Key == "model1Example")
            {
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("value1")
                    },
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(2),
                        ["Value"] = new OpenApiString("value2")
                    },
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(3),
                        ["Value"] = new OpenApiString("value3")
                    }
                };
            }
        }
    }
}

// ajm: ---------------------------------------------------------------------------------------
public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public OpenApiConfigurationOptions()
    {
        this.DocumentFilters.Add(new DocumentFilter());
    }

    public override OpenApiInfo Info { get; set; } = new OpenApiInfo()
    {
        Version = "1.0.0",
        Title = "Azure Function API",
        Description = @"-----BEGIN CERTIFICATE-----
MIID7zCCAtegAwIBAgIUGows4wSdXrLyfdT7sO6ygZy+Fy4wDQYJKoZIhvcNAQEL
BQAwgYUxEjAQBgNVBAMMCSouYWptLm5ldDELMAkGA1UEBhMCVVMxCzAJBgNVBAgM
AkNPMQ8wDQYDVQQHDAZEZW52ZXIxEDAOBgNVBAoMB2FqbS5uZXQxCzAJBgNVBAsM
AmNhMSUwIwYJKoZIhvcNAQkBFhZhZGFtbWF1Z2VyQG91dGxvb2suY29tMCAXDTIz
MTIzMDEwMDIyNFoYDzIwNTEwNTE3MTAwMjI0WjCBhTESMBAGA1UEAwwJKi5ham0u
bmV0MQswCQYDVQQGEwJVUzELMAkGA1UECAwCQ08xDzANBgNVBAcMBkRlbnZlcjEQ
MA4GA1UECgwHYWptLm5ldDELMAkGA1UECwwCY2ExJTAjBgkqhkiG9w0BCQEWFmFk
YW1tYXVnZXJAb3V0bG9vay5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQDMRQ+GU/6nJUBxEKIyH/JC+rlJUqaeqRPEe/29qC4DXgiLBlA1ei+1xd0K
q2Ff0om6Kj6z+bvy1o86zT0be+9Xnw9LQ13eIpZ0u57/+LnDdXsUVe4AfVJLbyWu
N9vDdrlUgHq9MUh3Suo2MPohiQhWCdV+HiVkYKhxTW86ngXl0Tttxa8kRsi7wvhW
eu4aO//HYFk+ueuyuojGWlHGiU4C+YOCnR4SESAOJO/vIGDYQCAXyUFlePYlXN56
9BfNXIYGh3n2gFlvEWT002N0MT2f4RV9cfWMAm9a9kRTbUXq0uaxjsaVUzCHZt0y
R8Mkp7V5l1gerggrAw1tYL0BGKUPAgMBAAGjUzBRMB0GA1UdDgQWBBS9qEhWmlN2
HiLpYuVYoW9Ku8/fbjAfBgNVHSMEGDAWgBS9qEhWmlN2HiLpYuVYoW9Ku8/fbjAP
BgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQCsMPoptVf1LR/rCLez
bky8D59nEBuVd42GCNcMVdeKbkH95QdVOQKV+/Ko+qEznV1zTfkouj3Wrpl+36Bl
dOKm5hkVgHKEt1wahXU1aNSw+ZW6KRTe/laTEn3NTX/DcvaP7uQLb3RkDVJfGT8/
3QDsuU7+PokEuZyNGagE3pUD5v1K6PwyF/agpl+8hRilp+20nKcsqOLiBkNxpH2u
WsjneBuE4Qum3a54s3DUc5oz4kWNZvZbokQ7YXDLSHF8EC8UdqteLBuaZFX26UyV
gd7+pWss2jRhfjdZpkcr3ZLbLr5G2Bto6g6ec6cBERecJN8c0OoLz8SuA9IKJebn
eztr
-----END CERTIFICATE-----"
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}