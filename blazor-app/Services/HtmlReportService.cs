using BlazorApp.Models;
using System.Text;

namespace BlazorApp.Services;

public class HtmlReportService
{
    private readonly ILogger<HtmlReportService> _logger;
    private readonly string _reportsDirectory;

    public HtmlReportService(ILogger<HtmlReportService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _reportsDirectory = Path.Combine(env.WebRootPath, "reports");
        
        if (!Directory.Exists(_reportsDirectory))
        {
            Directory.CreateDirectory(_reportsDirectory);
        }
    }

    public string GenerateReport(Report report)
    {
        try
        {
            var fileName = $"report_{report.Id}.html";
            var filePath = Path.Combine(_reportsDirectory, fileName);

            var html = GenerateHtmlTemplate(report);
            File.WriteAllText(filePath, html, Encoding.UTF8);

            _logger.LogInformation($"HTML report generated: {fileName}");
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HTML report");
            throw;
        }
    }

    private string GenerateHtmlTemplate(Report report)
    {
        var ingredientsRows = new StringBuilder();
        foreach (var ingredient in report.Products)
        {
            ingredientsRows.AppendLine($@"
                <tr>
                    <td>{ingredient.Name}</td>
                    <td>{ingredient.Quantity:F2} g</td>
                    <td>{ingredient.Volume:F2} ml</td>
                    <td>{ingredient.MolarMass:F2} g/L</td>
                </tr>");
        }

        var stepsRows = new StringBuilder();
        var timestamps = new StringBuilder();
        var temperatures = new StringBuilder();
        var stepLabels = new StringBuilder();

        var currentTime = 0;
        foreach (var step in report.Steps)
        {
            stepsRows.AppendLine($@"
                <tr>
                    <td><strong>{step.Name}</strong></td>
                    <td>{FormatTimestamp(currentTime)}</td>
                    <td>{step.Temp:F1}°C</td>
                    <td>{step.Pressure:F1} bar</td>
                    <td>{step.Speed:F0} rpm</td>
                    <td><span class='status-ok'>{step.Remark}</span></td>
                </tr>");

            timestamps.Append($"{currentTime},");
            temperatures.Append($"{step.Temp:F1},");
            stepLabels.Append($"'{step.Name}',");
            currentTime += step.Time;
        }

        return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Recipe Report - {report.RecipeName}</title>
    <script src='../chart.umd.min.js'></script>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}

        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f5f5f5;
            padding: 20px;
        }}

        .container {{
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}

        .header {{
            text-align: center;
            border-bottom: 3px solid #228be6;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}

        .header h1 {{
            color: #228be6;
            font-size: 32px;
            margin-bottom: 10px;
        }}

        .header .subtitle {{
            color: #666;
            font-size: 18px;
        }}

        .info-section {{
            background: #f8f9fa;
            padding: 20px;
            border-radius: 6px;
            margin-bottom: 30px;
        }}

        .info-grid {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 15px;
        }}

        .info-item {{
            display: flex;
            justify-content: space-between;
        }}

        .info-label {{
            font-weight: 600;
            color: #495057;
        }}

        .info-value {{
            color: #212529;
        }}

        section {{
            margin-bottom: 40px;
        }}

        h2 {{
            color: #228be6;
            font-size: 24px;
            margin-bottom: 20px;
            padding-bottom: 10px;
            border-bottom: 2px solid #e9ecef;
        }}

        table {{
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }}

        th {{
            background-color: #228be6;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
        }}

        td {{
            padding: 12px;
            border-bottom: 1px solid #e9ecef;
        }}

        tr:hover {{
            background-color: #f8f9fa;
        }}

        .chart-container {{
            position: relative;
            height: 400px;
            margin: 30px 0;
            background: white;
            padding: 20px;
            border-radius: 6px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }}

        .status-ok {{
            background-color: #d3f9d8;
            color: #2b8a3e;
            padding: 4px 10px;
            border-radius: 4px;
            font-weight: 600;
            font-size: 12px;
        }}

        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 2px solid #e9ecef;
            text-align: center;
            color: #6c757d;
            font-size: 14px;
        }}

        @media print {{
            body {{
                background: white;
                padding: 0;
            }}
            .container {{
                box-shadow: none;
                padding: 20px;
            }}
            .chart-container {{
                page-break-inside: avoid;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Production Report</h1>
            <div class='subtitle'>{report.RecipeName}</div>
        </div>

        <div class='info-section'>
            <div class='info-grid'>
                <div class='info-item'>
                    <span class='info-label'>Recipe Name:</span>
                    <span class='info-value'>{report.RecipeName}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Machine:</span>
                    <span class='info-value'>{report.MachineName}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Date:</span>
                    <span class='info-value'>{report.Date:yyyy-MM-dd HH:mm:ss}</span>
                </div>
                <div class='info-item'>
                    <span class='info-label'>Total Duration:</span>
                    <span class='info-value'>{FormatTimestamp(report.Steps.Sum(s => s.Time))}</span>
                </div>
            </div>
        </div>

        <section>
            <h2>Chemical Products</h2>
            <table>
                <thead>
                    <tr>
                        <th>Product Name</th>
                        <th>Quantity</th>
                        <th>Volume</th>
                        <th>Molar Mass</th>
                    </tr>
                </thead>
                <tbody>
                    {ingredientsRows}
                </tbody>
            </table>
        </section>

        <section>
            <h2>Process Steps</h2>
            <table>
                <thead>
                    <tr>
                        <th>Step Name</th>
                        <th>Timestamp</th>
                        <th>Temperature</th>
                        <th>Pressure</th>
                        <th>Speed</th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    {stepsRows}
                </tbody>
            </table>
        </section>

        <section>
            <h2>Temperature Profile</h2>
            <div class='chart-container'>
                <canvas id='tempChart'></canvas>
            </div>
        </section>

        <div class='footer'>
            Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Recipe Manager System
        </div>
    </div>

    <script>
        // Chart.js Temperature Profile
        if (typeof Chart !== 'undefined') {{
            const ctx = document.getElementById('tempChart').getContext('2d');
            const tempChart = new Chart(ctx, {{
                type: 'line',
                data: {{
                    labels: [{stepLabels.ToString().TrimEnd(',')}],
                    datasets: [{{
                        label: 'Temperature (°C)',
                        data: [{temperatures.ToString().TrimEnd(',')}],
                        borderColor: '#228be6',
                        backgroundColor: 'rgba(34, 139, 230, 0.1)',
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4,
                        pointRadius: 6,
                        pointHoverRadius: 8,
                        pointBackgroundColor: '#228be6',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2
                    }}]
                }},
                options: {{
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {{
                        legend: {{
                            display: true,
                            position: 'top'
                        }},
                        title: {{
                            display: true,
                            text: 'Temperature Probe Readings Throughout Process',
                            font: {{
                                size: 16,
                                weight: 'bold'
                            }}
                        }}
                    }},
                    scales: {{
                        y: {{
                            beginAtZero: true,
                            title: {{
                                display: true,
                                text: 'Temperature (°C)'
                            }},
                            grid: {{
                                color: 'rgba(0, 0, 0, 0.05)'
                            }}
                        }},
                        x: {{
                            title: {{
                                display: true,
                                text: 'Process Steps'
                            }},
                            grid: {{
                                color: 'rgba(0, 0, 0, 0.05)'
                            }}
                        }}
                    }}
                }}
            }});
        }} else {{
            document.querySelector('.chart-container').innerHTML = '<p style=""text-align: center; color: #6c757d; padding: 40px;"">Chart.js library not loaded. Please include the chart.umd.min.js file.</p>';
        }}
    </script>
</body>
</html>";
    }

    private string FormatTimestamp(int seconds)
    {
        var mins = seconds / 60;
        var secs = seconds % 60;
        return $"{mins:D2}:{secs:D2}";
    }

    public string GetReportPath(string fileName)
    {
        return Path.Combine(_reportsDirectory, fileName);
    }
}
