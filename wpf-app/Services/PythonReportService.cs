using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WpfApp.Models;

namespace WpfApp.Services;

public class PythonReportService
{
    private readonly ILogger<PythonReportService> _logger;
    private readonly string _pythonScriptPath;
    private readonly string _reportsDirectory;

    public PythonReportService(ILogger<PythonReportService> logger)
    {
        _logger = logger;
        
        // Get the application directory
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _pythonScriptPath = Path.Combine(appDirectory, "Python", "generate_report.py");
        _reportsDirectory = Path.Combine(appDirectory, "Reports");
        
        // Create reports directory if it doesn't exist
        Directory.CreateDirectory(_reportsDirectory);
    }

    public async Task<string?> GeneratePdfReportAsync(Report report)
    {
        try
        {
            // Create a unique filename
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var safeRecipeName = SanitizeFilename(report.RecipeName);
            var pdfFilename = $"{safeRecipeName}_{timestamp}.pdf";
            var pdfPath = Path.Combine(_reportsDirectory, pdfFilename);

            // Create temporary JSON file
            var jsonFilename = $"report_{timestamp}.json";
            var jsonPath = Path.Combine(Path.GetTempPath(), jsonFilename);

            // Serialize report data to JSON
            var jsonData = JsonConvert.SerializeObject(new
            {
                id = report.Id,
                recipeName = report.RecipeName,
                machineName = report.MachineName,
                date = report.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                products = report.Products?.Select(p => new
                {
                    name = p.Name,
                    quantity = p.Quantity,
                    volume = p.Volume,
                    molarMass = p.MolarMass
                }).ToList(),
                steps = report.Steps?.Select(s => new
                {
                    name = s.Name,
                    time = s.Time,
                    temp = s.Temp,
                    pressure = s.Pressure,
                    speed = s.Speed,
                    remark = s.Remark
                }).ToList()
            }, Formatting.Indented);

            await File.WriteAllTextAsync(jsonPath, jsonData);

            // Call Python script
            var success = await ExecutePythonScriptAsync(_pythonScriptPath, jsonPath, pdfPath);

            // Clean up temporary JSON file
            try
            {
                File.Delete(jsonPath);
            }
            catch
            {
                // Ignore cleanup errors
            }

            if (success && File.Exists(pdfPath))
            {
                _logger.LogInformation($"PDF report generated successfully: {pdfPath}");
                return pdfPath;
            }
            else
            {
                _logger.LogError("Failed to generate PDF report");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report");
            return null;
        }
    }

    private async Task<bool> ExecutePythonScriptAsync(string scriptPath, string jsonPath, string outputPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\" \"{jsonPath}\" \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                    _logger.LogInformation($"Python output: {e.Data}");
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    _logger.LogError($"Python error: {e.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var exitCode = process.ExitCode;
            
            if (exitCode != 0)
            {
                _logger.LogError($"Python script exited with code {exitCode}. Error: {errorBuilder}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Python script");
            return false;
        }
    }

    private static string SanitizeFilename(string filename)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(filename.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return sanitized.Trim();
    }

    public string GetReportsDirectory()
    {
        return _reportsDirectory;
    }
}
