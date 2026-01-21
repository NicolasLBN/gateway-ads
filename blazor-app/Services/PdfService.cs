using BlazorApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BlazorApp.Services;

public class PdfService
{
    private readonly ILogger<PdfService> _logger;
    private readonly string _reportsDirectory;

    public PdfService(ILogger<PdfService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _reportsDirectory = Path.Combine(env.WebRootPath, "reports");
        
        // Ensure reports directory exists
        if (!Directory.Exists(_reportsDirectory))
        {
            Directory.CreateDirectory(_reportsDirectory);
        }

        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public string GenerateReport(Report report)
    {
        try
        {
            var fileName = $"report_{report.Id}.pdf";
            var filePath = Path.Combine(_reportsDirectory, fileName);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Text($"Production Report - {report.RecipeName}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Recipe Information
                            column.Item().Element(c => ComposeRecipeInfo(c, report));

                            // Products Table
                            column.Item().Element(c => ComposeProductsTable(c, report));

                            // Process Steps Table
                            if (report.Steps.Any())
                            {
                                column.Item().Element(c => ComposeStepsTable(c, report));
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                            x.Span($" - Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        });
                });
            });

            document.GeneratePdf(filePath);
            _logger.LogInformation($"PDF report generated: {fileName}");

            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report");
            throw;
        }
    }

    private void ComposeRecipeInfo(IContainer container, Report report)
    {
        container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Spacing(5);
            column.Item().Text($"Recipe: {report.RecipeName}").SemiBold().FontSize(14);
            column.Item().Text($"Machine: {report.MachineName}");
            column.Item().Text($"Date: {report.Date:yyyy-MM-dd HH:mm:ss}");
        });
    }

    private void ComposeProductsTable(IContainer container, Report report)
    {
        container.Column(column =>
        {
            column.Item().Text("Ingredients").SemiBold().FontSize(14);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(100);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Name").FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Quantity (g)").FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Volume (ml)").FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("M.Mass (g/L)").FontColor(Colors.White);
                });

                foreach (var product in report.Products)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(product.Name);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{product.Quantity:F2}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{product.Volume:F2}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{product.MolarMass:F2}");
                }
            });
        });
    }

    private void ComposeStepsTable(IContainer container, Report report)
    {
        container.Column(column =>
        {
            column.Item().Text("Process Steps").SemiBold().FontSize(14);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Step").FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Time (s)").FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Temp (°C)").FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Press (bar)").FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Speed (rpm)").FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Remark").FontColor(Colors.White);
                });

                foreach (var step in report.Steps)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(step.Name);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{step.Time}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{step.Temp:F1}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{step.Pressure:F1}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{step.Speed:F0}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(step.Remark);
                }
            });
        });
    }

    public string GetReportPath(string fileName)
    {
        return Path.Combine(_reportsDirectory, fileName);
    }
}
