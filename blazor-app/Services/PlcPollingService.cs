namespace BlazorApp.Services;

public class PlcPollingService : BackgroundService
{
    private readonly AdsService _adsService;
    private readonly ILogger<PlcPollingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(500);

    public PlcPollingService(AdsService adsService, ILogger<PlcPollingService> logger)
    {
        _adsService = adsService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC Polling Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_adsService.IsConnected)
                {
                    await _adsService.ReadProcessStatusAsync();
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PLC polling");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("PLC Polling Service stopped");
    }
}
