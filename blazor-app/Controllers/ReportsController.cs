using BlazorApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reports;
    private readonly PdfService _pdf;
    private readonly IWebHostEnvironment _env;

    public ReportsController(ReportService reports, PdfService pdf, IWebHostEnvironment env)
    {
        _reports = reports;
        _pdf = pdf;
        _env = env;
    }

    [HttpGet]
    public ActionResult<IEnumerable<object>> GetReports()
    {
        var items = _reports.GetReports().Select(r => new
        {
            id = r.Id,
            recipeName = r.RecipeName,
            machineName = r.MachineName,
            date = r.Date,
            stepCount = r.FormulationSteps.Count > 0 ? r.FormulationSteps.Count : r.Steps.Count,
            hasPdf = System.IO.File.Exists(_pdf.GetReportPath($"report_{r.Id}.pdf"))
        });
        return Ok(items);
    }

    [HttpGet("{id}/download")]
    public IActionResult Download(string id)
    {
        var report = _reports.GetReport(id);
        if (report == null)
            return NotFound(new { error = "Report not found" });

        var path = _pdf.GetReportPath($"report_{id}.pdf");
        if (!System.IO.File.Exists(path))
        {
            // Regenerate if missing
            try
            {
                _pdf.GenerateReport(report);
            }
            catch
            {
                return NotFound(new { error = "PDF file not found" });
            }
        }

        if (!System.IO.File.Exists(path))
            return NotFound(new { error = "PDF file not found" });

        var bytes = System.IO.File.ReadAllBytes(path);
        var fileName = $"report_{report.RecipeName}_{report.Date:yyyyMMdd}.pdf";
        return File(bytes, "application/pdf", fileName);
    }
}
