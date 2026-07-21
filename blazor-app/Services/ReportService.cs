using BlazorApp.Models;

namespace BlazorApp.Services;

public class ReportService
{
    private readonly List<Report> _reports = new();
    private readonly ILogger<ReportService> _logger;

    public ReportService(ILogger<ReportService> logger)
    {
        _logger = logger;
        
        // Add some sample reports
        AddSampleReports();
    }

    public List<Report> GetReports()
    {
        return _reports.OrderByDescending(r => r.Date).ToList();
    }

    public Report? GetReport(string id)
    {
        return _reports.FirstOrDefault(r => r.Id == id);
    }

    public void AddReport(Report report)
    {
        _reports.Add(report);
        _logger.LogInformation($"Report added: {report.RecipeName}");
    }

    private void AddSampleReports()
    {
        _reports.Add(new Report
        {
            Id = "1",
            RecipeName = "Chocolate Chip Cookies",
            MachineName = "Mixing Unit A",
            Date = new DateTime(2025, 12, 7, 10, 30, 0),
            Products = new List<Ingredient>
            {
                new() { Name = "Flour", Quantity = 500, Volume = 100, MolarMass = 0 },
                new() { Name = "Sugar", Quantity = 200, Volume = 50, MolarMass = 0 },
                new() { Name = "Butter", Quantity = 150, Volume = 30, MolarMass = 0 },
                new() { Name = "Chocolate Chips", Quantity = 100, Volume = 20, MolarMass = 0 },
                new() { Name = "Eggs", Quantity = 100, Volume = 20, MolarMass = 0 }
            },
            Steps = BuildSampleSteps(new DateTime(2025, 12, 7, 10, 30, 0), "Dosage", "Melange", "Extraction", "Cuisson")
        });

        _reports.Add(new Report
        {
            Id = "2",
            RecipeName = "Vanilla Cupcakes",
            MachineName = "Mixing Unit B",
            Date = new DateTime(2025, 12, 6, 14, 20, 0),
            Products = new List<Ingredient>
            {
                new() { Name = "Flour", Quantity = 400, Volume = 80, MolarMass = 0 },
                new() { Name = "Sugar", Quantity = 250, Volume = 60, MolarMass = 0 },
                new() { Name = "Vanilla Extract", Quantity = 10, Volume = 2, MolarMass = 0 },
                new() { Name = "Milk", Quantity = 200, Volume = 50, MolarMass = 0 },
                new() { Name = "Eggs", Quantity = 120, Volume = 25, MolarMass = 0 }
            },
            Steps = BuildSampleSteps(new DateTime(2025, 12, 6, 14, 20, 0), "Dosage", "Melange", "Cuisson")
        });
    }

    private static List<ProcessStep> BuildSampleSteps(DateTime start, params string[] stepNames)
    {
        var steps = new List<ProcessStep>();
        var current = start;

        foreach (var name in stepNames)
        {
            var end = current.AddSeconds(15);
            steps.Add(new ProcessStep
            {
                Name = name,
                StartTime = current,
                EndTime = end,
                DurationSeconds = 15
            });
            current = end;
        }

        return steps;
    }
}
