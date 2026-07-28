using TwinCAT.Ads;
using BlazorApp.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace BlazorApp.Services;

/// <summary>
/// ADS client for RecipeManager, inspired by ThermalWinch <c>IOMasterAds</c>:
/// connect via AMS Net ID string, optional auto-reconnect loop, startup from config.
/// </summary>
public class AdsService : IDisposable
{
    private const int ADS_STRING_BUFFER_SIZE = 255;
    private const int ADS_STRING_MAX_LENGTH = 254;
    private const int MAX_STEPS = 20;
    private const int MAX_INGREDIENTS = 10;
    private const int COMMAND_PULSE_MS = 150;

    private AdsClient? _adsClient;
    private readonly ILogger<AdsService> _logger;
    private readonly AdsOptions _options;
    private readonly object _lock = new();
    private readonly CancellationTokenSource _reconnectCts = new();
    private Task? _reconnectLoop;
    private bool _isConnected;
    private bool _autoReconnectEnabled;
    private bool _manualDisconnect;
    private string _amsNetId;
    private int _amsPort;
    private bool _isHeld;
    private bool _disposed;

    public event EventHandler<ProcessStatus>? ProcessStatusUpdated;
    public event EventHandler? ConnectionStateChanged;

    public bool IsConnected => _isConnected;

    /// <summary>Fixed target from appsettings.json (section ADS).</summary>
    public AdsOptions ConfiguredTarget => _options;

    /// <summary>Last connection failure detail, surfaced to the UI for diagnostics.</summary>
    public string? LastError { get; private set; }

    public AdsService(ILogger<AdsService> logger, IOptions<AdsOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _amsNetId = _options.AmsNetId;
        _amsPort = _options.AmsPort;
        _autoReconnectEnabled = _options.AutoReconnect;

        // Same as ThermalWinch IOMasterAds: one long-lived AdsClient, reconnect via Disconnect+Connect
        _adsClient = new AdsClient
        {
            Timeout = Math.Max(1000, _options.TimeoutMs)
        };
    }

    /// <summary>
    /// Starts background connection using AMS Net ID from appsettings (ThermalWinch PreBuild pattern).
    /// Does not throw if the PLC is not ready yet — AutoReconnect keeps retrying.
    /// </summary>
    public Task StartAutoConnectAsync()
    {
        _manualDisconnect = false;
        _amsNetId = _options.AmsNetId?.Trim() ?? string.Empty;
        _amsPort = _options.AmsPort;
        _autoReconnectEnabled = _options.AutoReconnect;

        // Immediate first attempt (non-blocking for callers that await a single try)
        var connected = TryConnect();
        if (connected)
        {
            _logger.LogInformation("ADS connected at startup to {AmsNetId}:{Port}", _amsNetId, _amsPort);
        }
        else
        {
            _logger.LogWarning(
                "ADS not available at startup ({Detail}). AutoReconnect={AutoReconnect}",
                LastError, _autoReconnectEnabled);
        }

        EnsureReconnectLoop();
        return Task.CompletedTask;
    }

    /// <summary>Connect using the fixed AMS Net ID + port from appsettings.json.</summary>
    public Task<bool> ConnectFromConfigAsync()
    {
        _manualDisconnect = false;
        var ok = TryConnect(_options.AmsNetId, _options.AmsPort);
        if (!ok && _options.AutoReconnect)
        {
            EnsureReconnectLoop();
        }

        return Task.FromResult(ok);
    }

    public Task<bool> ConnectAsync(string amsNetId, int amsPort)
    {
        _manualDisconnect = false;
        return Task.FromResult(TryConnect(amsNetId, amsPort));
    }

    /// <summary>
    /// Same contract as ThermalWinch <c>IOMasterAds.TryConnect</c>:
    /// <c>_client.Connect(AmsNetId, Port)</c> then verify state.
    /// </summary>
    public bool TryConnect() => TryConnect(_amsNetId, _amsPort);

    public bool TryConnect(string amsNetId, int amsPort)
    {
        LastError = null;

        lock (_lock)
        {
            try
            {
                _amsNetId = amsNetId?.Trim() ?? string.Empty;
                _amsPort = amsPort;

                EnsureClient();
                var client = _adsClient!;

                // ThermalWinch IOMasterAds: Disconnect() then Connect on the SAME client (no Dispose)
                try
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
                catch
                {
                    // ignore disconnect errors before reconnect
                }

                _logger.LogInformation("ADS Connect({AmsNetId}, {Port})", _amsNetId, _amsPort);
                client.Connect(_amsNetId, _amsPort);

                var adsState = client.ReadState().AdsState;
                _logger.LogInformation("ADS state after connect: {AdsState}", adsState);

                _isConnected = client.IsConnected;
                _isHeld = false;

                if (_isConnected)
                {
                    try
                    {
                        WriteSymbol("GVL_Command.bAdsConnected", true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Connected but could not write GVL_Command.bAdsConnected (symbols may be missing)");
                    }
                }
                else
                {
                    LastError = $"ADS Connect returned IsConnected=false for {_amsNetId}:{_amsPort}.";
                }

                ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
                return _isConnected;
            }
            catch (Exception ex)
            {
                LastError = BuildFriendlyAdsError(ex, amsNetId, amsPort);
                _logger.LogError(ex, "Failed to connect to PLC: {Detail}", LastError);
                _isConnected = false;

                // Keep the client instance (ThermalWinch style); only disconnect
                try { _adsClient?.Disconnect(); } catch { /* ignore */ }

                ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
                return false;
            }
        }
    }

    private void EnsureClient()
    {
        if (_adsClient != null)
            return;

        _adsClient = new AdsClient
        {
            Timeout = Math.Max(1000, _options.TimeoutMs)
        };
    }

    public void Disconnect()
    {
        _manualDisconnect = true;
        lock (_lock)
        {
            DisconnectInternal(notify: true);
        }
    }

    /// <summary>Request a reconnect on the next auto-reconnect cycle (like IOMasterAds.Reset).</summary>
    public void Reset()
    {
        _manualDisconnect = false;
        lock (_lock)
        {
            DisconnectInternal(notify: true);
        }

        EnsureReconnectLoop();
    }

    public async Task<ProcessStatus?> ReadProcessStatusAsync()
    {
        if (!_isConnected || _adsClient == null)
            return null;

        try
        {
            var status = await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_isConnected || _adsClient == null)
                        return null;

                    var stateCode = ReadSymbol<short>("GVL_State.nState");

                    return new ProcessStatus
                    {
                        State = Enum.IsDefined(typeof(PackMLState), (int)stateCode)
                            ? (PackMLState)stateCode
                            : PackMLState.Clearing,
                        StateName = ReadSymbol<string>("GVL_State.sStateName"),
                        CurrentStepIndex = ReadSymbol<ushort>("GVL_Process.nCurrentStepIndex"),
                        CurrentStepName = ReadSymbol<string>("GVL_Process.sCurrentStepName"),
                        TotalSteps = ReadSymbol<ushort>("GVL_Process.nTotalSteps"),
                        StepTimeElapsed = ReadSymbol<ushort>("GVL_Process.nStepTimeElapsed_s"),
                        StepTimeRemaining = ReadSymbol<ushort>("GVL_Process.nStepTimeRemaining_s"),
                        Progress = ReadSymbol<float>("GVL_Process.fProgress"),
                        ProcessDone = ReadSymbol<bool>("GVL_Process.bProcessDone"),
                        IsHeld = ReadSymbol<bool>("GVL_State.bHeld"),
                        ErrorCode = ReadSymbol<short>("GVL_Process.nErrorCode"),
                        ErrorText = ReadSymbol<string>("GVL_Process.sErrorText")
                    };
                }
            });

            if (status != null)
                ProcessStatusUpdated?.Invoke(this, status);

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading process status");
            MarkDisconnectedOnError(ex);
            return null;
        }
    }

    public async Task<bool> SendRecipeAsync(Recipe recipe)
    {
        if (!_isConnected || _adsClient == null)
            return false;

        try
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (!_isConnected || _adsClient == null)
                        return false;

                    WriteSymbol("GVL_Recipe.sRecipeName", recipe.Name);
                    WriteSymbol("GVL_Recipe.fPreparationVolume", (float)recipe.PreparationVolume);
                    WriteSymbol("GVL_Recipe.fPreparationConcentration", (float)recipe.PreparationConcentration);

                    var steps = recipe.Steps.Take(MAX_STEPS).ToList();
                    WriteSymbol("GVL_Recipe.nNumSteps", (ushort)steps.Count);
                    for (int i = 0; i < steps.Count; i++)
                    {
                        WriteSymbol($"GVL_Recipe.aStepNames[{i + 1}]", steps[i]);
                    }

                    var ingredients = recipe.Ingredients.Take(MAX_INGREDIENTS).ToList();
                    WriteSymbol("GVL_Recipe.nNumIngredients", (ushort)ingredients.Count);
                    for (int i = 0; i < ingredients.Count; i++)
                    {
                        var ingredient = ingredients[i];
                        WriteSymbol($"GVL_Recipe.aIngredientName[{i + 1}]", ingredient.Name);
                        WriteSymbol($"GVL_Recipe.aIngredientQuantity[{i + 1}]", (float)ingredient.QuantityForPlc);
                        WriteSymbol($"GVL_Recipe.aIngredientVolume[{i + 1}]", (float)ingredient.VolumeForPlc);
                        WriteSymbol($"GVL_Recipe.aIngredientMolarMass[{i + 1}]", (float)ingredient.MolarMassForPlc);
                    }

                    _logger.LogInformation(
                        "Recipe '{Name}' sent to PLC ({StepCount} steps, {IngredientCount} ingredients)",
                        recipe.Name, steps.Count, ingredients.Count);
                    return true;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending recipe to PLC");
            MarkDisconnectedOnError(ex);
            return false;
        }
    }

    public async Task<bool> SendCommandAsync(PackMLCommand command)
    {
        if (!_isConnected || _adsClient == null)
            return false;

        try
        {
            if (command == PackMLCommand.Hold)
            {
                await Task.Run(() =>
                {
                    lock (_lock)
                    {
                        _isHeld = !_isHeld;
                        WriteSymbol("GVL_Command.bHold", _isHeld);
                    }
                });
                _logger.LogInformation("Hold set to {Held}", _isHeld);
                return true;
            }

            var symbolName = command switch
            {
                PackMLCommand.Reset => "GVL_Command.bReset",
                PackMLCommand.Clear => "GVL_Command.bClear",
                PackMLCommand.Start => "GVL_Command.bStart",
                PackMLCommand.Stop => "GVL_Command.bStop",
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            };

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    WriteSymbol(symbolName, true);
                }
            });
            await Task.Delay(COMMAND_PULSE_MS);
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    WriteSymbol(symbolName, false);
                }
            });

            _logger.LogInformation("PackML command {Command} sent", command);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending PackML command {Command}", command);
            MarkDisconnectedOnError(ex);
            return false;
        }
    }

    private void EnsureReconnectLoop()
    {
        if (!_autoReconnectEnabled || _disposed)
            return;

        if (_reconnectLoop is { IsCompleted: false })
            return;

        _reconnectLoop = Task.Run(() => ReconnectLoopAsync(_reconnectCts.Token));
    }

    private async Task ReconnectLoopAsync(CancellationToken cancellationToken)
    {
        var interval = Math.Max(500, _options.ReconnectIntervalMs);

        while (!cancellationToken.IsCancellationRequested && !_disposed)
        {
            try
            {
                if (!_manualDisconnect && !_isConnected && _autoReconnectEnabled)
                {
                    _logger.LogDebug("ADS auto-reconnect attempt to {AmsNetId}:{Port}", _amsNetId, _amsPort);
                    TryConnect(_amsNetId, _amsPort);
                }

                await Task.Delay(interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ADS reconnect loop");
                await Task.Delay(interval, CancellationToken.None);
            }
        }
    }

    private void MarkDisconnectedOnError(Exception ex)
    {
        lock (_lock)
        {
            if (!_isConnected)
                return;

            _logger.LogWarning(ex, "ADS connection lost");
            DisconnectInternal(notify: true);
        }

        if (_autoReconnectEnabled && !_manualDisconnect)
            EnsureReconnectLoop();
    }

    private void DisconnectInternal(bool notify)
    {
        try
        {
            if (_adsClient != null)
            {
                try { _adsClient.Disconnect(); } catch { /* ignore */ }
                // Do NOT Dispose the AdsClient here — reuse it like IOMasterAds
            }
        }
        finally
        {
            var wasConnected = _isConnected;
            _isConnected = false;
            if (notify)
                ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
            else if (wasConnected)
                ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private T ReadSymbol<T>(string symbolName)
    {
        if (_adsClient == null)
            throw new InvalidOperationException("ADS client not connected");

        var handle = _adsClient.CreateVariableHandle(symbolName);
        try
        {
            if (typeof(T) == typeof(string))
            {
                var bytes = new byte[ADS_STRING_BUFFER_SIZE];
                _adsClient.Read(handle, bytes);
                var str = Encoding.ASCII.GetString(bytes).TrimEnd('\0');
                return (T)(object)str;
            }

            return (T)_adsClient.ReadAny(handle, typeof(T));
        }
        finally
        {
            _adsClient.DeleteVariableHandle(handle);
        }
    }

    private void WriteSymbol<T>(string symbolName, T value)
    {
        if (_adsClient == null)
            throw new InvalidOperationException("ADS client not connected");

        var handle = _adsClient.CreateVariableHandle(symbolName);
        try
        {
            if (typeof(T) == typeof(string))
            {
                var str = value?.ToString() ?? string.Empty;
                var bytes = new byte[ADS_STRING_BUFFER_SIZE];
                var strBytes = Encoding.ASCII.GetBytes(str);
                Array.Copy(strBytes, bytes, Math.Min(strBytes.Length, ADS_STRING_MAX_LENGTH));
                _adsClient.Write(handle, bytes);
            }
            else if (value != null)
            {
                _adsClient.WriteAny(handle, value);
            }
        }
        finally
        {
            _adsClient.DeleteVariableHandle(handle);
        }
    }

    private static string BuildFriendlyAdsError(Exception ex, string? amsNetId, int amsPort)
    {
        var message = ex.Message ?? string.Empty;

        if (message.Contains("ClientPortNotOpen", StringComparison.OrdinalIgnoreCase)
            || message.Contains("ConnectPortFailed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Cannot register AmsPort", StringComparison.OrdinalIgnoreCase)
            || message.Contains("LoopbackNotRegistered", StringComparison.OrdinalIgnoreCase)
            || message.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
        {
            return
                $"Cannot reach TwinCAT ADS router for {amsNetId}:{amsPort}. " +
                "Ensure UmRT/TwinCAT is in Run mode and port 48898 is listening " +
                "(only one TcSystemServiceUm). Raw: " + message;
        }

        return $"ADS connection to {amsNetId}:{amsPort} failed: {message}";
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _manualDisconnect = true;
        _autoReconnectEnabled = false;

        try { _reconnectCts.Cancel(); } catch { /* ignore */ }
        try { _reconnectLoop?.Wait(TimeSpan.FromSeconds(2)); } catch { /* ignore */ }
        _reconnectCts.Dispose();

        lock (_lock)
        {
            try { _adsClient?.Disconnect(); } catch { /* ignore */ }
            try { _adsClient?.Dispose(); } catch { /* ignore */ }
            _adsClient = null;
            _isConnected = false;
        }
    }
}
