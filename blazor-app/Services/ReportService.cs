using BlazorApp.Models;

namespace BlazorApp.Services;

public class ReportService
{
    private readonly List<Report> _reports = new();
    private readonly ILogger<ReportService> _logger;
    private readonly string _historyPath;
    private readonly object _lock = new();

    public ReportService(ILogger<ReportService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        var dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        _historyPath = Path.Combine(dataDir, "report-history.json");
        Load();
    }

    public List<Report> GetReports()
    {
        lock (_lock)
        {
            return _reports.OrderByDescending(r => r.Date).ToList();
        }
    }

    public Report? GetReport(string id)
    {
        lock (_lock)
        {
            return _reports.FirstOrDefault(r => r.Id == id);
        }
    }

    public void AddReport(Report report)
    {
        lock (_lock)
        {
            _reports.Add(report);
            Persist();
        }

        _logger.LogInformation("Report added: {Name}", report.RecipeName);
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_historyPath))
                return;

            var json = File.ReadAllText(_historyPath);
            var loaded = System.Text.Json.JsonSerializer.Deserialize<List<Report>>(json);
            if (loaded != null)
                _reports.AddRange(loaded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report history");
        }
    }

    private void Persist()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_reports, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_historyPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error persisting report history");
        }
    }
}
