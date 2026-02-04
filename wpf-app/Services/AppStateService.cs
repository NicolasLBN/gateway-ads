using System.ComponentModel;
using WpfApp.Models;

namespace WpfApp.Services;

public class AppStateService : INotifyPropertyChanged
{
    private Machine? _selectedMachine;
    private bool _isConnected;
    private MachineStatus? _machineStatus;
    private ProcessStatus? _processStatus;
    private Recipe? _currentRecipe;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? StateChanged;

    public Machine? SelectedMachine
    {
        get => _selectedMachine;
        set
        {
            if (_selectedMachine != value)
            {
                _selectedMachine = value;
                OnPropertyChanged(nameof(SelectedMachine));
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
                OnPropertyChanged(nameof(IsConnected));
            }
        }
    }

    public MachineStatus? MachineStatus
    {
        get => _machineStatus;
        set
        {
            if (_machineStatus != value)
            {
                _machineStatus = value;
                OnPropertyChanged(nameof(MachineStatus));
            }
        }
    }

    public ProcessStatus? ProcessStatus
    {
        get => _processStatus;
        set
        {
            if (_processStatus != value)
            {
                _processStatus = value;
                OnPropertyChanged(nameof(ProcessStatus));
            }
        }
    }

    public Recipe? CurrentRecipe
    {
        get => _currentRecipe;
        set
        {
            if (_currentRecipe != value)
            {
                _currentRecipe = value;
                OnPropertyChanged(nameof(CurrentRecipe));
            }
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
