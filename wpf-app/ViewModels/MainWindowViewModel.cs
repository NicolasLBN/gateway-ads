using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly AdsService _adsService;
    private readonly MachineService _machineService;
    private readonly AppStateService _appStateService;
    private readonly PlcPollingService _plcPollingService;
    private readonly PythonReportService _reportService;
    private readonly ILogger<MainWindowViewModel> _logger;

    private string _statusMessage = "Not Connected";
    private bool _isConnected;
    private Machine? _selectedMachine;
    private Recipe _currentRecipe;

    public MainWindowViewModel(
        AdsService adsService,
        MachineService machineService,
        AppStateService appStateService,
        PlcPollingService plcPollingService,
        PythonReportService reportService,
        ILogger<MainWindowViewModel> logger)
    {
        _adsService = adsService;
        _machineService = machineService;
        _appStateService = appStateService;
        _plcPollingService = plcPollingService;
        _reportService = reportService;
        _logger = logger;

        _currentRecipe = new Recipe();

        // Initialize commands
        ConnectCommand = new RelayCommand(async _ => await ConnectAsync(), _ => SelectedMachine != null && !IsConnected);
        DisconnectCommand = new RelayCommand(_ => Disconnect(), _ => IsConnected);
        SendRecipeCommand = new RelayCommand(async _ => await SendRecipeAsync(), _ => IsConnected);
        StartProcessCommand = new RelayCommand(async _ => await StartProcessAsync(), _ => IsConnected);
        ResetProcessCommand = new RelayCommand(async _ => await ResetProcessAsync(), _ => IsConnected);

        // Load machines
        Machines = new ObservableCollection<Machine>(_machineService.GetMachines());
        if (Machines.Count > 0)
        {
            SelectedMachine = Machines[0];
        }

        // Subscribe to state changes
        _appStateService.PropertyChanged += OnAppStateChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Machine> Machines { get; }

    public Machine? SelectedMachine
    {
        get => _selectedMachine;
        set
        {
            if (_selectedMachine != value)
            {
                _selectedMachine = value;
                _appStateService.SelectedMachine = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                OnPropertyChanged();
                StatusMessage = value ? $"Connected to {SelectedMachine?.Name}" : "Not Connected";
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public Recipe CurrentRecipe
    {
        get => _currentRecipe;
        set
        {
            if (_currentRecipe != value)
            {
                _currentRecipe = value;
                _appStateService.CurrentRecipe = value;
                OnPropertyChanged();
            }
        }
    }

    public MachineStatus? MachineStatus => _appStateService.MachineStatus;
    public ProcessStatus? ProcessStatus => _appStateService.ProcessStatus;

    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand SendRecipeCommand { get; }
    public ICommand StartProcessCommand { get; }
    public ICommand ResetProcessCommand { get; }

    private async Task ConnectAsync()
    {
        if (SelectedMachine == null)
            return;

        try
        {
            var connected = await _adsService.ConnectAsync(SelectedMachine.AmsNetId, SelectedMachine.AmsPort);
            IsConnected = connected;
            _appStateService.IsConnected = connected;

            if (connected)
            {
                _plcPollingService.StartPolling();
                _logger.LogInformation($"Connected to {SelectedMachine.Name}");
            }
            else
            {
                StatusMessage = "Connection failed";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to PLC");
            StatusMessage = "Connection error";
        }
    }

    private void Disconnect()
    {
        try
        {
            _plcPollingService.StopPolling();
            _adsService.Disconnect();
            IsConnected = false;
            _appStateService.IsConnected = false;
            _logger.LogInformation("Disconnected from PLC");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from PLC");
        }
    }

    private async Task SendRecipeAsync()
    {
        try
        {
            var success = await _adsService.SendRecipeAsync(CurrentRecipe);
            if (success)
            {
                _logger.LogInformation($"Recipe '{CurrentRecipe.Name}' sent to PLC");
                StatusMessage = $"Recipe '{CurrentRecipe.Name}' sent successfully";
            }
            else
            {
                StatusMessage = "Failed to send recipe";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending recipe");
            StatusMessage = "Error sending recipe";
        }
    }

    private async Task StartProcessAsync()
    {
        try
        {
            var success = await _adsService.StartProcessAsync();
            if (success)
            {
                _logger.LogInformation("Process started");
                StatusMessage = "Process started";
            }
            else
            {
                StatusMessage = "Failed to start process";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting process");
            StatusMessage = "Error starting process";
        }
    }

    private async Task ResetProcessAsync()
    {
        try
        {
            var success = await _adsService.ResetProcessAsync();
            if (success)
            {
                _logger.LogInformation("Process reset");
                StatusMessage = "Process reset";
            }
            else
            {
                StatusMessage = "Failed to reset process";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting process");
            StatusMessage = "Error resetting process";
        }
    }

    private void OnAppStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppStateService.MachineStatus))
        {
            OnPropertyChanged(nameof(MachineStatus));
        }
        else if (e.PropertyName == nameof(AppStateService.ProcessStatus))
        {
            OnPropertyChanged(nameof(ProcessStatus));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
