using BlazorApp.Models;

namespace BlazorApp.Services;

public class AppStateService
{
    private readonly AdsService _adsService;
    private Machine? _selectedMachine;
    private Recipe? _currentRecipe;
    private readonly List<(DateTime Time, MachineStatus Status)> _machineHistory = new();
    private readonly List<(DateTime Time, ProcessStatus Status)> _processHistory = new();

    public event Action? StateChanged;

    public Machine? SelectedMachine => _selectedMachine;
    public bool IsConnected => _adsService.IsConnected;
    public Recipe? CurrentRecipe => _currentRecipe;
    public MachineStatus? LatestMachineStatus { get; private set; }
    public ProcessStatus? LatestProcessStatus { get; private set; }
    public IReadOnlyList<(DateTime Time, MachineStatus Status)> MachineHistory => _machineHistory.AsReadOnly();
    public IReadOnlyList<(DateTime Time, ProcessStatus Status)> ProcessHistory => _processHistory.AsReadOnly();

    public AppStateService(AdsService adsService)
    {
        _adsService = adsService;
        _adsService.MachineStatusUpdated += OnMachineStatusUpdated;
        _adsService.ProcessStatusUpdated += OnProcessStatusUpdated;
    }

    public void SetSelectedMachine(Machine machine)
    {
        _selectedMachine = machine;
        NotifyStateChanged();
    }

    public void SetCurrentRecipe(Recipe recipe)
    {
        _currentRecipe = recipe;
        NotifyStateChanged();
    }

    public void ClearProcessHistory()
    {
        _processHistory.Clear();
        _machineHistory.Clear();
        NotifyStateChanged();
    }

    public List<ProcessStep> GetProcessStepsFromHistory()
    {
        var steps = new List<ProcessStep>();
        
        if (_processHistory.Count == 0 || _machineHistory.Count == 0)
            return steps;

        // Group history by step
        var stepGroups = _processHistory
            .Where(p => p.Status.CurrentStep > 0 && p.Status.CurrentStep < 7)
            .GroupBy(p => p.Status.CurrentStep)
            .OrderBy(g => g.Key);

        foreach (var stepGroup in stepGroups)
        {
            var stepData = stepGroup.ToList();
            if (stepData.Count == 0) continue;

            var firstInStep = stepData.First();
            var lastInStep = stepData.Last();
            
            // Find corresponding machine data for this step
            var machineDataForStep = _machineHistory
                .Where(m => m.Time >= firstInStep.Time && m.Time <= lastInStep.Time)
                .ToList();

            if (machineDataForStep.Count > 0)
            {
                // Calculate average values for the step
                var avgTemp = machineDataForStep.Average(m => m.Status.MotorTemperature);
                var avgPressure = machineDataForStep.Average(m => m.Status.OilPressure);
                var avgSpeed = machineDataForStep.Average(m => m.Status.MotorSpeed);
                
                var stepDuration = (int)(lastInStep.Time - firstInStep.Time).TotalSeconds;
                
                steps.Add(new ProcessStep
                {
                    Name = firstInStep.Status.StepName,
                    Time = stepDuration,
                    Temp = Math.Round(avgTemp, 1),
                    Pressure = Math.Round(avgPressure, 2),
                    Speed = Math.Round(avgSpeed, 0),
                    Remark = firstInStep.Status.ErrorCode != 0 ? firstInStep.Status.ErrorText : "OK"
                });
            }
        }

        return steps;
    }

    private void OnMachineStatusUpdated(object? sender, MachineStatus status)
    {
        LatestMachineStatus = status;
        
        // Keep only last 100 entries
        if (_machineHistory.Count > 100)
            _machineHistory.RemoveAt(0);
        
        _machineHistory.Add((DateTime.Now, status));
        NotifyStateChanged();
    }

    private void OnProcessStatusUpdated(object? sender, ProcessStatus status)
    {
        LatestProcessStatus = status;
        
        // Keep only last 100 entries
        if (_processHistory.Count > 100)
            _processHistory.RemoveAt(0);
        
        _processHistory.Add((DateTime.Now, status));
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
