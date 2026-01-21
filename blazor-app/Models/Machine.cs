namespace BlazorApp.Models;

public class Machine
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AmsNetId { get; set; } = string.Empty;
    public int AmsPort { get; set; } = 851;
    public string Description { get; set; } = string.Empty;
}
