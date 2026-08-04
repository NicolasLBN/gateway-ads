using System.Text.Json;
using BlazorApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BlazorApp.Services;

/// <summary>
/// Builds production PDF reports with QuestPDF.
/// Writes both a web-served copy (<c>wwwroot/reports</c>) and a local export pair (PDF + JSON under <c>exports/</c>).
/// </summary>
public class PdfService
{
    private readonly ILogger<PdfService> _logger;
    private readonly string _reportsDirectory;
    private readonly string _exportsDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public PdfService(ILogger<PdfService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        // Served / downloaded via GET /api/reports/{id}/download
        _reportsDirectory = Path.Combine(env.WebRootPath, "reports");
        // Local archive next to the app (not exposed over HTTP)
        _exportsDirectory = Path.Combine(env.ContentRootPath, "exports");

        Directory.CreateDirectory(_reportsDirectory);
        Directory.CreateDirectory(_exportsDirectory);

        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates <c>report_{id}.pdf</c> for the API, plus timestamped PDF/JSON under <c>exports/</c>.
    /// Returns the web file name (e.g. report_abc.pdf).
    /// </summary>
    public string GenerateReport(Report report)
    {
        try
        {
            var stamp = report.Date.ToString("yyyyMMdd_HHmmss");
            var safeName = string.Concat(report.RecipeName.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
            var baseName = $"recipe_{safeName}_{stamp}";

            var pdfWebName = $"report_{report.Id}.pdf";
            var pdfWebPath = Path.Combine(_reportsDirectory, pdfWebName);
            var pdfExportPath = Path.Combine(_exportsDirectory, $"{baseName}.pdf");
            var jsonExportPath = Path.Combine(_exportsDirectory, $"{baseName}.json");

            var document = BuildDocument(report);
            document.GeneratePdf(pdfWebPath);
            document.GeneratePdf(pdfExportPath);
            File.WriteAllText(jsonExportPath, JsonSerializer.Serialize(report, JsonOptions));

            _logger.LogInformation("Report exported: {Pdf} + {Json}", pdfExportPath, jsonExportPath);
            return pdfWebName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report");
            throw;
        }
    }

    /// <summary>A4 layout: recipe header, planned steps, PackML execution timeline.</summary>
    private static Document BuildDocument(Report report) =>
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text($"Rapport de fabrication — {report.RecipeName}")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(16);
                        column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(info =>
                        {
                            info.Spacing(4);
                            info.Item().Text($"Recipe: {report.RecipeName}").SemiBold().FontSize(13);
                            info.Item().Text($"Machine: {report.MachineName}");
                            info.Item().Text($"Date/heure: {report.Date:yyyy-MM-dd HH:mm:ss}");
                        });

                        // Planned recipe definition (from ProcessSteps at generation time)
                        if (report.RecipeSteps.Count > 0)
                        {
                            column.Item().Text("Séquence chronologique des étapes").SemiBold().FontSize(14);
                            foreach (var step in report.RecipeSteps)
                            {
                                column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).Column(s =>
                                {
                                    s.Item().Text($"{step.Index}. {step.Type}").SemiBold();
                                    s.Item().Text(step.Details).FontSize(10).FontColor(Colors.Grey.Darken2);
                                });
                            }
                        }

                        // Actual run timeline collected by AppState during Execute
                        if (report.Steps.Count > 0)
                        {
                            column.Item().Text("Timeline d'exécution PackML").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(90);
                                    columns.ConstantColumn(90);
                                    columns.ConstantColumn(70);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("#").FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Étape").FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Début").FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Fin").FontColor(Colors.White);
                                    header.Cell().Background(Colors.Blue.Darken2).Padding(4).Text("Durée (s)").FontColor(Colors.White);
                                });

                                for (var i = 0; i < report.Steps.Count; i++)
                                {
                                    var step = report.Steps[i];
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{i + 1}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(step.Name);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(step.StartTime.ToString("HH:mm:ss"));
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(step.EndTime.ToString("HH:mm:ss"));
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{step.DurationSeconds:F1}");
                                }
                            });
                        }
                    });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span($" — généré le {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                });
            });
        });

    /// <summary>Absolute path under wwwroot/reports for a given file name.</summary>
    public string GetReportPath(string fileName) => Path.Combine(_reportsDirectory, fileName);
}
