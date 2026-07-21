namespace BlazorApp.Models;

public class ProcessStatus
{
    public PackMLState State { get; set; } = PackMLState.Clearing;
    public string StateName { get; set; } = string.Empty;

    public int CurrentStepIndex { get; set; }
    public string CurrentStepName { get; set; } = string.Empty;
    public int TotalSteps { get; set; }

    public int StepTimeElapsed { get; set; }
    public int StepTimeRemaining { get; set; }
    public double Progress { get; set; }

    public bool ProcessDone { get; set; }
    public bool IsHeld { get; set; }

    public int ErrorCode { get; set; }
    public string ErrorText { get; set; } = string.Empty;
}
