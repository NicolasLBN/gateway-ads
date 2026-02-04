using BlazorApp.Models;
using System.Diagnostics;
using System.Text.Json;

namespace BlazorApp.Services;

public class PythonPdfService
{
    private readonly ILogger<PythonPdfService> _logger;
    private readonly string _reportsDirectory;
    private readonly string _pythonScriptPath;
    private readonly string _tempDirectory;

    public PythonPdfService(ILogger<PythonPdfService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _reportsDirectory = Path.Combine(env.WebRootPath, "reports");
        _pythonScriptPath = Path.Combine(env.ContentRootPath, "Python", "generate_report.py");
        _tempDirectory = Path.Combine(env.ContentRootPath, "temp");
        
        // Ensure directories exist
        if (!Directory.Exists(_reportsDirectory))
        {
            Directory.CreateDirectory(_reportsDirectory);
        }
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
    }

    public async Task<string> GenerateReportAsync(Report report)
    {
        try
        {
            // Create temporary JSON file with report data
            var tempJsonPath = Path.Combine(_tempDirectory, $"report_{report.Id}_data.json");
            var outputPdfPath = Path.Combine(_reportsDirectory, $"report_{report.Id}.pdf");

            // Convert report to JSON format expected by Python script
            var reportData = new
            {
                id = report.Id,
                recipeName = report.RecipeName,
                machineName = report.MachineName,
                date = report.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                products = report.Products.Select(p => new
                {
                    name = p.Name,
                    quantity = p.Quantity,
                    volume = p.Volume,
                    molarMass = p.MolarMass
                }).ToList(),
                steps = report.Steps.Select(s => new
                {
                    name = s.Name,
                    time = s.Time,
                    temp = s.Temp,
                    pressure = s.Pressure,
                    speed = s.Speed,
                    remark = s.Remark
                }).ToList()
            };

            // Write JSON data to temp file
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonContent = JsonSerializer.Serialize(reportData, jsonOptions);
            await File.WriteAllTextAsync(tempJsonPath, jsonContent);

            // Call Python script to generate PDF
            var success = await RunPythonScriptAsync(tempJsonPath, outputPdfPath);

            // Clean up temp file
            if (File.Exists(tempJsonPath))
            {
                File.Delete(tempJsonPath);
            }

            if (success)
            {
                _logger.LogInformation($"PDF report generated successfully: report_{report.Id}.pdf");
                return $"report_{report.Id}.pdf";
            }
            else
            {
                throw new Exception("Python script failed to generate PDF. Please ensure Python and reportlab are installed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report with Python");
            throw;
        }
    }

    private async Task<bool> RunPythonScriptAsync(string jsonPath, string outputPath)
    {
        try
        {
            // Try python3 first, then python
            var pythonCommand = await GetPythonCommandAsync();
            
            if (string.IsNullOrEmpty(pythonCommand))
            {
                _logger.LogError("Python is not installed or not in PATH");
                return false;
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = pythonCommand,
                Arguments = $"\"{_pythonScriptPath}\" \"{jsonPath}\" \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                _logger.LogError($"Failed to start Python process. Command: {pythonCommand} {processInfo.Arguments}");
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError($"Python script error (exit code {process.ExitCode}): {error}");
                return false;
            }

            _logger.LogInformation($"Python script output: {output}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running Python script");
            return false;
        }
    }

    private async Task<string> GetPythonCommandAsync()
    {
        // Try python3 first (Linux/Mac)
        if (await IsPythonAvailableAsync("python3"))
        {
            return "python3";
        }
        
        // Try python (Windows)
        if (await IsPythonAvailableAsync("python"))
        {
            return "python";
        }

        return string.Empty;
    }

    private async Task<bool> IsPythonAvailableAsync(string command)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public string GetReportPath(string fileName)
    {
        return Path.Combine(_reportsDirectory, fileName);
    }

    public bool ReportExists(string fileName)
    {
        var path = GetReportPath(fileName);
        return File.Exists(path);
    }
}
