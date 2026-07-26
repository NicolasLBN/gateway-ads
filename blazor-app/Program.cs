using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Fixed ADS target from appsettings.json (section "ADS")
builder.Services.Configure<AdsOptions>(builder.Configuration.GetSection(AdsOptions.SectionName));

// Register application services
builder.Services.AddSingleton<AdsService>();
builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<MachineService>();
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<PdfService>();
builder.Services.AddSingleton<FavoritesService>();

// Add background service for PLC polling
builder.Services.AddHostedService<PlcPollingService>();

var app = builder.Build();

// Optional: connect at startup using the fixed appsettings values
var adsOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AdsOptions>>().Value;
if (adsOptions.AutoConnect)
{
    var ads = app.Services.GetRequiredService<AdsService>();
    var appState = app.Services.GetRequiredService<AppStateService>();
    var connected = await ads.ConnectFromConfigAsync();
    if (connected)
    {
        appState.SetSelectedMachine(new Machine
        {
            Id = "config",
            Name = adsOptions.MachineName,
            AmsNetId = adsOptions.AmsNetId,
            AmsPort = adsOptions.AmsPort,
            Description = "Loaded from appsettings.json"
        });
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files (including PDFs)
app.UseStaticFiles();

app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
