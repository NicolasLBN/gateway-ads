namespace BlazorApp.Models;

/// <summary>
/// Fixed ADS connection settings loaded from appsettings.json (section "ADS").
/// Pattern aligned with ThermalWinch ApplicationConfiguration / IOMasterAds.
/// </summary>
public class AdsOptions
{
    public const string SectionName = "ADS";

    /// <summary>Target AMS Net ID, e.g. "199.4.42.250.1.1" or "127.0.0.1.1.1".</summary>
    public string AmsNetId { get; set; } = "127.0.0.1.1.1";

    /// <summary>Target PLC AMS port (851 = first PLC runtime).</summary>
    public int AmsPort { get; set; } = 851;

    /// <summary>Display name shown in the UI for this configured target.</summary>
    public string MachineName { get; set; } = "Configured PLC";

    /// <summary>Connect automatically when the Blazor app starts (PreBuildAds).</summary>
    public bool AutoConnect { get; set; } = true;

    /// <summary>Retry connection in background when disconnected (like IOMasterAds.AutoReconnect).</summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>Delay between reconnect attempts in milliseconds.</summary>
    public int ReconnectIntervalMs { get; set; } = 2000;

    /// <summary>ADS client timeout in milliseconds.</summary>
    public int TimeoutMs { get; set; } = 10000;
}
