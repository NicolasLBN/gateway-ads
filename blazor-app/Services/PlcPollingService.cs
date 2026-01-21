namespace BlazorApp.Services;

public class PlcPollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlcPollingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(500);

    public PlcPollingService(
        IServiceProvider serviceProvider,
        ILogger<PlcPollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PLC Polling Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var adsService = scope.ServiceProvider.GetRequiredService<AdsService>();

                if (adsService.IsConnected)
                {
                    await adsService.ReadMachineStatusAsync();
                    await adsService.ReadProcessStatusAsync();
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
