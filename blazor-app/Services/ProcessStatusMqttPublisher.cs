using System.Text.Json;
using BlazorApp.Models;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace BlazorApp.Services;

/// <summary>
/// Publishes AppState process status into the embedded MQTT broker (retain)
/// so React (and other clients) can subscribe instead of polling HTTP.
/// </summary>
public sealed class ProcessStatusMqttPublisher : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AppStateService _appState;
    private readonly IOptions<MqttOptions> _options;
    private readonly ILogger<ProcessStatusMqttPublisher> _logger;
    private readonly object _gate = new();
    private MqttServer? _server;
    private int _publishQueued;
    private bool _attached;

    public ProcessStatusMqttPublisher(
        AppStateService appState,
        IOptions<MqttOptions> options,
        ILogger<ProcessStatusMqttPublisher> logger)
    {
        _appState = appState;
        _options = options;
        _logger = logger;
    }

    public void Attach(MqttServer server)
    {
        ArgumentNullException.ThrowIfNull(server);

        lock (_gate)
        {
            if (_attached)
                return;

            _server = server;
            _appState.StateChanged += OnStateChanged;
            _attached = true;
        }

        _logger.LogInformation(
            "MQTT process status publisher attached (topic={Topic})",
            _options.Value.StatusTopic);

        QueuePublish();
    }

    private void OnStateChanged() => QueuePublish();

    private void QueuePublish()
    {
        if (Interlocked.Exchange(ref _publishQueued, 1) == 1)
            return;

        _ = PublishLoopAsync();
    }

    private async Task PublishLoopAsync()
    {
        try
        {
            do
            {
                Interlocked.Exchange(ref _publishQueued, 0);
                await PublishOnceAsync().ConfigureAwait(false);
            }
            while (Interlocked.CompareExchange(ref _publishQueued, 0, 0) == 1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MQTT process status publish failed");
            Interlocked.Exchange(ref _publishQueued, 0);
        }
    }

    private async Task PublishOnceAsync()
    {
        var opts = _options.Value;
        if (!opts.Enabled)
            return;

        MqttServer? server;
        lock (_gate)
            server = _server;

        if (server is null || !server.IsStarted)
            return;

        var payload = BuildPayload();
        var json = JsonSerializer.Serialize(payload, JsonOptions);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(opts.StatusTopic)
            .WithPayload(json)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag()
            .Build();

        await server.InjectApplicationMessage(new InjectedMqttApplicationMessage(message)
        {
            SenderClientId = "gateway-ads"
        }).ConfigureAwait(false);
    }

    private object BuildPayload()
    {
        var status = _appState.LatestProcessStatus;
        return new
        {
            connected = _appState.IsConnected,
            state = status?.State.ToString() ?? (_appState.IsConnected ? "Unknown" : "Offline"),
            stateCode = status?.State is PackMLState s ? (int)s : (int?)null,
            stateName = status?.StateName,
            currentStepIndex = status?.CurrentStepIndex ?? 0,
            currentStepName = status?.CurrentStepName,
            totalSteps = status?.TotalSteps ?? 0,
            progress = status?.Progress ?? 0,
            stepTimeRemaining = status?.StepTimeRemaining ?? 0,
            isHeld = status?.IsHeld ?? false,
            processDone = status?.ProcessDone ?? false,
            recipeName = _appState.CurrentRecipe?.Name
        };
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (!_attached)
                return;

            _appState.StateChanged -= OnStateChanged;
            _server = null;
            _attached = false;
        }
    }
}
