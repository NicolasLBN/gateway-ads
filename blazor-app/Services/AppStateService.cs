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
