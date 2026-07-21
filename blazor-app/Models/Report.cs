namespace BlazorApp.Models;

public class Report
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string RecipeName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public List<Ingredient> Products { get; set; } = new();
    public List<ProcessStep> Steps { get; set; } = new();
}

public class ProcessStep
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double DurationSeconds { get; set; }
}
