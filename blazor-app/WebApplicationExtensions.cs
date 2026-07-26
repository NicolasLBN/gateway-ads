using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.Extensions.Options;

namespace BlazorApp;

/// <summary>
/// Startup helpers inspired by ThermalWinch <c>WebApplicationExtensions.PreBuildSettings</c>.
/// Registers the ADS master target from config and starts connection (+ auto-reconnect).
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures ADS from appsettings and starts connecting to the PLC via AMS Net ID.
    /// </summary>
    public static async Task PreBuildAdsAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.Services.GetRequiredService<IOptions<AdsOptions>>().Value;
        var ads = app.Services.GetRequiredService<AdsService>();
        var appState = app.Services.GetRequiredService<AppStateService>();

        appState.SetSelectedMachine(new Machine
        {
            Id = "config",
            Name = options.MachineName,
            AmsNetId = options.AmsNetId,
            AmsPort = options.AmsPort,
            Description = "Loaded from appsettings.json (ThermalWinch-style PreBuild)"
        });

        if (!options.AutoConnect)
        {
            app.Logger.LogInformation(
                "ADS AutoConnect=false — waiting for manual Connect. Target {AmsNetId}:{Port}",
                options.AmsNetId, options.AmsPort);
            return;
        }

        app.Logger.LogInformation(
            "PreBuildAds: connecting to {AmsNetId}:{Port} (AutoReconnect={AutoReconnect})",
            options.AmsNetId, options.AmsPort, options.AutoReconnect);

        await ads.StartAutoConnectAsync();
    }
}
