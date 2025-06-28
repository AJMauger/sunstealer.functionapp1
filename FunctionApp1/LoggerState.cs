namespace Sunstealer.FunctionApp1;

public class LoggerState
{
    public int LineNumber { get; set; }
    public string FilePath { get; set; }
    public string Message { get; set; }

    // ajm: ---------------------------------------------------------------------------------------
    public LoggerState(string message, string filePath, int lineNumber)
    {
        FilePath = filePath;
        LineNumber = lineNumber;
        Message = message;
    }

    // ajm: ---------------------------------------------------------------------------------------
    public override string ToString()
    {
        return $"{Message}";
    }
}
