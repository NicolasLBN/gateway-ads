namespace BlazorApp.Models;

public class ProcessStatus
{
    public int CurrentStep { get; set; }
    public string StepName { get; set; } = string.Empty;
    public double Progress { get; set; }
    public double StepProgress { get; set; }
    public uint StepTime { get; set; }
    public uint TotalTime { get; set; }
    public int ErrorCode { get; set; }
    public string ErrorText { get; set; } = string.Empty;
    public bool ProcessDone { get; set; }
}
