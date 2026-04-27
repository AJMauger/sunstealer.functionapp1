using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Sunstealer.FunctionApp2.OpenApi;

public class DocumentFilter : IDocumentFilter
{
    // ajm: ---------------------------------------------------------------------------------------
    public void Apply(IHttpRequestDataObject req, OpenApiDocument document)
    {
        var schemas = document.Components.Schemas;
        foreach (var schema in schemas)
        {
            if (schema.Key == "DeleteRequestModel")
            {
                var transactionId = new OpenApiString(Guid.NewGuid().ToString());
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("InsertRequestModel")
                    }
                };
            }
            if (schema.Key == "DeleteResponseModel")
            {
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("InsertRequestModel")
                    }
                };
            }
            if (schema.Key == "InsertRequestModel")
            {
                var transactionId = new OpenApiString(Guid.NewGuid().ToString());
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("InsertRequestModel")
                    }
                };
            }
            if (schema.Key == "InsertResponseModel")
            {
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("InsertResponseModel")
                    }
                }; 
            }
            if (schema.Key == "UpdateRequestModel")
            {
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("UpdateRequestModel")
                    }
                };
            }
            if (schema.Key == "UpdateResponseModel")
            {
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("UpdateResponseModel")
                    }
                };
            }
            if (schema.Key == "SelectRequestModel")
            {
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("SelectRequestModel")
                    }
                };
            }
            if (schema.Key == "serachResponseModel")
            {
                schema.Value.Example = new OpenApiArray() {
                    new OpenApiObject()
                    {
                        ["Key"] = new OpenApiInteger(1),
                        ["Value"] = new OpenApiString("SelectRequestModel")
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
        DocumentFilters.Add(new DocumentFilter());
    }

    public override OpenApiInfo Info { get; set; } = new OpenApiInfo()
    {
        Version = "1.0.0",
        Title = "Azure Function API",
        Description = @""
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}