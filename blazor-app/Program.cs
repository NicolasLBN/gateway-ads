using System.Text;
using BlazorApp;
using BlazorApp.Components;
using BlazorApp.Models;
using BlazorApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MQTTnet.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    // Allow React portal from localhost or LAN devices (any host on ports 5173/3000).
    options.AddPolicy("ReactPortal", policy =>
        policy.SetIsOriginAllowed(static origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return false;
                return uri.Port is 5173 or 3000;
            })
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.Configure<AdsOptions>(builder.Configuration.GetSection(AdsOptions.SectionName));
builder.Services.Configure<MqttOptions>(builder.Configuration.GetSection(MqttOptions.SectionName));

var mqttOptions = builder.Configuration.GetSection(MqttOptions.SectionName).Get<MqttOptions>() ?? new MqttOptions();

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "GatewayAdsDevSecretKey_ChangeMe_32chars!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "gateway-ads",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "gateway-ads-clients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<AdsService>();
builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<MachineService>();
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<PdfService>();
builder.Services.AddSingleton<FavoritesService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddSingleton<ProcessStatusMqttPublisher>();
builder.Services.AddHostedService<PlcPollingService>();

if (mqttOptions.Enabled)
{
    builder.Services
        .AddHostedMqttServer(options =>
        {
            if (mqttOptions.EnableTcp)
            {
                options.WithDefaultEndpoint()
                    .WithDefaultEndpointPort(mqttOptions.TcpPort);
            }
            else
            {
                options.WithoutDefaultEndpoint();
            }
        })
        .AddMqttConnectionHandler()
        .AddConnections();
}

var app = builder.Build();

await app.PreBuildAdsAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("ReactPortal");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

if (mqttOptions.Enabled)
{
    var wsPath = string.IsNullOrWhiteSpace(mqttOptions.WebSocketPath) ? "/mqtt" : mqttOptions.WebSocketPath;
    app.MapMqtt(wsPath);
    app.UseMqttServer(server =>
    {
        app.Services.GetRequiredService<ProcessStatusMqttPublisher>().Attach(server);
        app.Logger.LogInformation(
            "MQTT broker ready: ws://localhost:5223{Path} topic={Topic} tcp={Tcp}",
            wsPath,
            mqttOptions.StatusTopic,
            mqttOptions.EnableTcp ? mqttOptions.TcpPort.ToString() : "off");
    });
}

app.Run();
