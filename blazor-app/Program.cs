using BlazorApp.Components;
using BlazorApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register application services
builder.Services.AddSingleton<AdsService>();
builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<MachineService>();
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<HtmlReportService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<FavoritesService>();

// Add background service for PLC polling
builder.Services.AddHostedService<PlcPollingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files (including HTML reports)
app.UseStaticFiles();

app.UseAntiforgery();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
