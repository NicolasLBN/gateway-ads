using BlazorApp.Models;

namespace BlazorApp.Services;

public class AppStateService
{
    private readonly AdsService _adsService;
    private Machine? _selectedMachine;
    private Recipe? _currentRecipe;
    private readonly List<StepExecution> _stepTimeline = new();

    public event Action? StateChanged;

    public Machine? SelectedMachine => _selectedMachine;
    public bool IsConnected => _adsService.IsConnected;
    public Recipe? CurrentRecipe => _currentRecipe;
    public ProcessStatus? LatestProcessStatus { get; private set; }
    public IReadOnlyList<StepExecution> CurrentStepTimeline => _stepTimeline.AsReadOnly();

    public AppStateService(AdsService adsService)
    {
        _adsService = adsService;
        _adsService.ProcessStatusUpdated += OnProcessStatusUpdated;
        _adsService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    private void OnConnectionStateChanged(object? sender, EventArgs e)
    {
        NotifyStateChanged();
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
        _stepTimeline.Clear();
        NotifyStateChanged();
    }

    public async Task<bool> SendCommandAsync(PackMLCommand command)
    {
        var result = await _adsService.SendCommandAsync(command);

        if (result && (command == PackMLCommand.Reset || command == PackMLCommand.Clear))
        {
            _stepTimeline.Clear();
            NotifyStateChanged();
        }

        return result;
    }

    /// <summary>Bring PLC to Stopped (Home page). No-op if already stopped/clearing.</summary>
    public async Task EnsureStoppedAsync()
    {
        if (!IsConnected)
            return;

        var state = LatestProcessStatus?.State;
        if (state is PackMLState.Stopped or PackMLState.Clearing)
            return;

        await SendCommandAsync(PackMLCommand.Stop);
    }

    /// <summary>Bring PLC to Idle (Recipe details). Stops first if needed, then Reset.</summary>
    public async Task EnsureIdleAsync()
    {
        if (!IsConnected)
            return;

        var state = LatestProcessStatus?.State;
        if (state == PackMLState.Idle || state == PackMLState.Resetting)
            return;

        if (state is PackMLState.Execute or PackMLState.Starting or PackMLState.Completing)
        {
            await SendCommandAsync(PackMLCommand.Stop);
            await Task.Delay(400);
            state = LatestProcessStatus?.State;
        }

        if (state is PackMLState.Stopped or PackMLState.Complete or null)
        {
            await SendCommandAsync(PackMLCommand.Reset);
        }
    }

    private void OnProcessStatusUpdated(object? sender, ProcessStatus status)
    {
        var previous = LatestProcessStatus;
        LatestProcessStatus = status;

        UpdateStepTimeline(previous, status);

        NotifyStateChanged();
    }

    private void UpdateStepTimeline(ProcessStatus? previous, ProcessStatus status)
    {
        var now = DateTime.Now;

        // A fresh run is starting: clear the previous timeline
        if (status.State == PackMLState.Starting && previous?.State != PackMLState.Starting)
        {
            _stepTimeline.Clear();
        }

        var lastEntry = _stepTimeline.Count > 0 ? _stepTimeline[^1] : null;

        if (status.CurrentStepIndex > 0)
        {
            if (lastEntry == null || lastEntry.StepIndex != status.CurrentStepIndex)
            {
                if (lastEntry != null && lastEntry.EndTime == null)
                {
                    lastEntry.EndTime = now;
                }

                _stepTimeline.Add(new StepExecution
                {
                    StepIndex = status.CurrentStepIndex,
                    StepName = status.CurrentStepName,
                    StartTime = now
                });
            }
        }

        // Close out the last step once the recipe has finished or was stopped
        if (status.State is PackMLState.Completing or PackMLState.Complete or PackMLState.Stopped)
        {
            lastEntry = _stepTimeline.Count > 0 ? _stepTimeline[^1] : null;
            if (lastEntry != null && lastEntry.EndTime == null)
            {
                lastEntry.EndTime = now;
            }
        }
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
