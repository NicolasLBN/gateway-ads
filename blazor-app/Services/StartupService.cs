using BlazorApp.Models;

namespace BlazorApp.Services;

public class StartupService : IHostedService
{
    private readonly ILogger<StartupService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public StartupService(ILogger<StartupService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StartupService: Generating sample HTML reports...");
        
        try
        {
            var reportService = _serviceProvider.GetRequiredService<ReportService>();
            var htmlReportService = _serviceProvider.GetRequiredService<HtmlReportService>();
            
            var reports = reportService.GetReports();
            foreach (var report in reports)
            {
                try
                {
                    await Task.Run(() => htmlReportService.GenerateReport(report), cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation($"Generated HTML report for: {report.RecipeName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to generate HTML report for: {report.RecipeName}");
                }
            }
            
            _logger.LogInformation("StartupService: Sample reports generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartupService: Error generating sample reports");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
