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
    public int Time { get; set; }
    public double Temp { get; set; }
    public double Pressure { get; set; }
    public double Speed { get; set; }
    public string Remark { get; set; } = string.Empty;
}
