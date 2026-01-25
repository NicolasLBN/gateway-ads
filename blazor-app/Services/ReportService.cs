using BlazorApp.Models;

namespace BlazorApp.Services;

public class ReportService
{
    private readonly List<Report> _reports = new();
    private readonly ILogger<ReportService> _logger;
    private readonly HtmlReportService? _htmlReportService;

    public ReportService(ILogger<ReportService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        
        // Add some sample reports
        AddSampleReports();
        
        // Try to get HtmlReportService and generate reports
        _htmlReportService = serviceProvider.GetService<HtmlReportService>();
        if (_htmlReportService != null)
        {
            GenerateSampleReports();
        }
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
            RecipeName = "Acetylsalicylic Acid Synthesis",
            MachineName = "Reactor Unit A",
            Date = new DateTime(2025, 12, 7, 10, 30, 0),
            Products = new List<Ingredient>
            {
                new() { Name = "Salicylic Acid", Quantity = 138.12, Volume = 100, MolarMass = 138.12 },
                new() { Name = "Acetic Anhydride", Quantity = 102.09, Volume = 50, MolarMass = 102.09 },
                new() { Name = "Phosphoric Acid", Quantity = 5.0, Volume = 3, MolarMass = 97.99 },
                new() { Name = "Ethanol", Quantity = 46.07, Volume = 25, MolarMass = 46.07 },
                new() { Name = "Water", Quantity = 18.02, Volume = 50, MolarMass = 18.02 }
            },
            Steps = new List<ProcessStep>
            {
                new() { Name = "Preparation", Time = 60, Temp = 25.0, Pressure = 1.0, Speed = 0, Remark = "OK" },
                new() { Name = "Dosing Salicylic Acid", Time = 120, Temp = 30.0, Pressure = 1.0, Speed = 150, Remark = "OK" },
                new() { Name = "Adding Acetic Anhydride", Time = 180, Temp = 45.0, Pressure = 1.2, Speed = 200, Remark = "OK" },
                new() { Name = "Heating & Mixing", Time = 300, Temp = 85.0, Pressure = 1.5, Speed = 350, Remark = "OK" },
                new() { Name = "Cooling", Time = 240, Temp = 40.0, Pressure = 1.2, Speed = 200, Remark = "OK" },
                new() { Name = "Quality Control", Time = 120, Temp = 25.0, Pressure = 1.0, Speed = 0, Remark = "OK" },
                new() { Name = "Finalization", Time = 60, Temp = 25.0, Pressure = 1.0, Speed = 0, Remark = "OK" }
            }
        });

        _reports.Add(new Report
        {
            Id = "2",
            RecipeName = "Ibuprofen Formulation",
            MachineName = "Mixing Unit B",
            Date = new DateTime(2025, 12, 6, 14, 20, 0),
            Products = new List<Ingredient>
            {
                new() { Name = "Ibuprofen API", Quantity = 206.28, Volume = 80, MolarMass = 206.28 },
                new() { Name = "Lactose Monohydrate", Quantity = 360.31, Volume = 120, MolarMass = 360.31 },
                new() { Name = "Microcrystalline Cellulose", Quantity = 100.0, Volume = 60, MolarMass = 162.14 },
                new() { Name = "Sodium Starch Glycolate", Quantity = 50.0, Volume = 30, MolarMass = 134.11 },
                new() { Name = "Magnesium Stearate", Quantity = 10.0, Volume = 5, MolarMass = 591.24 }
            },
            Steps = new List<ProcessStep>
            {
                new() { Name = "Preparation", Time = 60, Temp = 22.0, Pressure = 1.0, Speed = 0, Remark = "OK" },
                new() { Name = "Dry Mixing", Time = 180, Temp = 24.0, Pressure = 1.0, Speed = 120, Remark = "OK" },
                new() { Name = "Granulation", Time = 240, Temp = 35.0, Pressure = 1.3, Speed = 250, Remark = "OK" },
                new() { Name = "Drying", Time = 360, Temp = 60.0, Pressure = 0.8, Speed = 100, Remark = "OK" },
                new() { Name = "Final Mixing", Time = 150, Temp = 25.0, Pressure = 1.0, Speed = 180, Remark = "OK" },
                new() { Name = "Quality Control", Time = 120, Temp = 22.0, Pressure = 1.0, Speed = 0, Remark = "OK" }
            }
        });
    }
    
    private void GenerateSampleReports()
    {
        if (_htmlReportService == null)
            return;
            
        foreach (var report in _reports)
        {
            try
            {
                _htmlReportService.GenerateReport(report);
                _logger.LogInformation($"Generated HTML report for: {report.RecipeName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate HTML report for: {report.RecipeName}");
            }
        }
    }
}
