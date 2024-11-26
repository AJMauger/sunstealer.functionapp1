using System.ComponentModel.DataAnnotations;

namespace FunctionApp1;

public class Table1
{
    [Key]
    public int UUID { get; set; }

    public int Number1 { get; set; }

    public string Text1 { get; set; } = string.Empty;
}
