namespace BlazorApp.Models;

// Captures the exact wall-clock start/end of each recipe step as observed by the
// Blazor client, used to build the PDF report's execution timeline.
public class StepExecution
{
    public int StepIndex { get; set; }
    public string StepName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public double DurationSeconds => ((EndTime ?? DateTime.Now) - StartTime).TotalSeconds;
}
