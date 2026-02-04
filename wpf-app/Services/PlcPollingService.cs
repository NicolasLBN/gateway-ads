using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace WpfApp.Services;

public class PlcPollingService : IDisposable
{
    private readonly AdsService _adsService;
    private readonly AppStateService _appStateService;
    private readonly ILogger<PlcPollingService> _logger;
    private readonly DispatcherTimer _timer;
    private bool _isPolling;

    public PlcPollingService(
        AdsService adsService,
        AppStateService appStateService,
        ILogger<PlcPollingService> logger)
    {
        _adsService = adsService;
        _appStateService = appStateService;
        _logger = logger;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _timer.Tick += OnTimerTick;
    }

    public void StartPolling()
    {
        if (!_isPolling)
        {
            _isPolling = true;
            _timer.Start();
            _logger.LogInformation("Started PLC polling");
        }
    }

    public void StopPolling()
    {
        if (_isPolling)
        {
            _isPolling = false;
            _timer.Stop();
            _logger.LogInformation("Stopped PLC polling");
        }
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        if (!_adsService.IsConnected)
            return;

        try
        {
            // Read machine status
            var machineStatus = await _adsService.ReadMachineStatusAsync();
            if (machineStatus != null)
            {
                _appStateService.MachineStatus = machineStatus;
            }

            // Read process status
            var processStatus = await _adsService.ReadProcessStatusAsync();
            if (processStatus != null)
            {
                _appStateService.ProcessStatus = processStatus;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PLC polling");
        }
    }

    public void Dispose()
    {
        StopPolling();
        _timer.Tick -= OnTimerTick;
    }
}
