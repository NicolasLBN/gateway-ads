namespace BlazorApp.Models;

/// <summary>
/// Fixed ADS connection settings loaded from appsettings.json (section "ADS").
/// </summary>
public class AdsOptions
{
    public const string SectionName = "ADS";

    /// <summary>Target AMS Net ID, e.g. "127.0.0.1.1.1" or "local".</summary>
    public string AmsNetId { get; set; } = "127.0.0.1.1.1";

    /// <summary>Target PLC AMS port (851 = first PLC runtime).</summary>
    public int AmsPort { get; set; } = 851;

    /// <summary>Display name shown in the UI for this configured target.</summary>
    public string MachineName { get; set; } = "Configured PLC";

    /// <summary>Connect automatically when the Blazor app starts.</summary>
    public bool AutoConnect { get; set; } = false;

    /// <summary>ADS client timeout in milliseconds.</summary>
    public int TimeoutMs { get; set; } = 5000;
}
