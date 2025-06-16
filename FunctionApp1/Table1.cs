using System.ComponentModel.DataAnnotations;

namespace Sunstealer.FunctionApp1;

public class Table1
{
    [Key]
    public int UUID { get; set; }
    public string Encrypted1 { get; set; } = string.Empty;
    public DateTime? Date1 { get; set; }
    public int Number1 { get; set; }
    public string Text1 { get; set; } = string.Empty;
}
