namespace Sunstealer.FunctionApp1;

public class Logger
{
    public List<string> list = new();

    public static Logger Instance
    {
        get
        {
            return instance;
        }
    }

    private static Logger instance { get; set; } = new();

    public void LogDebug(string function, string message)
    {
        Console.WriteLine($"[db] {function}: {message}");
        list.Add($"[db] {function}: {message}");
    }

    public void LogError(string function, string message)
    {
        Console.WriteLine($"[er] {function}: {message}");
        list.Add($"[er] {function}: {message}");
    }

    public void LogException(Exception e, string function, string message)
    {
        Console.WriteLine($"[ex] {function}: {message} {e}");
        list.Add($"[ex] {function}: {message} {e}");
    }

    public void LogInformation(string function, string message)
    {
        Console.WriteLine($"[in] {function}: {message}");
        list.Add($"[in] {function}: {message}");
    }

    public void LogWarning(string function, string message)
    {
        Console.WriteLine($"[wn] {function}: {message}");
        list.Add($"[wn] {function}: {message}");
    }
}
