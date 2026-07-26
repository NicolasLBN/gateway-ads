using BlazorApp;
using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Fixed ADS target from appsettings.json (section "ADS") — like ThermalWinch ApplicationConfiguration
builder.Services.Configure<AdsOptions>(builder.Configuration.GetSection(AdsOptions.SectionName));

// Register application services
builder.Services.AddSingleton<AdsService>();
builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<MachineService>();
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<PdfService>();
builder.Services.AddSingleton<FavoritesService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddScoped<AuthSessionService>();

// Add background service for PLC polling
builder.Services.AddHostedService<PlcPollingService>();

var app = builder.Build();

// ThermalWinch-style PreBuild: wire AMS Net ID and start ADS connection before serving UI
await app.PreBuildAdsAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
