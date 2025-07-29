namespace Sunstealer.FunctionApp1.Models;

public class ConfigurationModel
{
    public static string env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Error";

    public string AzureDbConnectionString { get; set; } = string.Empty;
}
