namespace Sunstealer.FunctionApp1.Models;

// ajm: -------------------------------------------------------------------------------------------
public class ConfigurationModel
{
    public static string env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Error";

    public static List<string> log = new();
}
