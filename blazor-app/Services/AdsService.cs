using TwinCAT.Ads;
using BlazorApp.Models;
using Microsoft.Extensions.Options;
using System.Text;

namespace BlazorApp.Services;

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
    private bool _isConnected;
    private string _amsNetId;
    private int _amsPort;
    private bool _isHeld;

    public event EventHandler<ProcessStatus>? ProcessStatusUpdated;

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
    }

    /// <summary>
    /// Connect using the fixed AMS Net ID + port from appsettings.json.
    /// </summary>
    public Task<bool> ConnectFromConfigAsync()
    {
        return ConnectAsync(_options.AmsNetId, _options.AmsPort);
    }

    public async Task<bool> ConnectAsync(string amsNetId, int amsPort)
    {
        LastError = null;

        try
        {
            _amsNetId = amsNetId?.Trim() ?? string.Empty;
            _amsPort = amsPort;

            _adsClient?.Dispose();
            // CompatibilityDefault / Default use Router+TcpIp (needed for TwinCAT UmRT).
            _adsClient = new AdsClient(new AdsClientSettings(Math.Max(1000, _options.TimeoutMs)));

            // Prefer AmsNetId.Local for loopback targets: the machine's real AMS Net ID
            // is often not 127.0.0.1.1.1, and string Connect can fail earlier with
            // ClientPortNotOpen when the router rejects a mismatched route.
            await Task.Run(() =>
            {
                // Always prefer Local for the machine's own runtime (UmRT / TcSysSrv).
                // Using the numeric local NetId as a "remote" target can fail earlier
                // while registering the Dynamic client port.
                if (ShouldUseLocalRouter(_amsNetId))
                {
                    _logger.LogInformation("ADS Connect via AmsNetId.Local -> port {Port}", _amsPort);
                    _adsClient.Connect(AmsNetId.Local, _amsPort);
                }
                else
                {
                    var netId = AmsNetId.Parse(_amsNetId);
                    _logger.LogInformation("ADS Connect via {NetId} -> port {Port}", netId, _amsPort);
                    _adsClient.Connect(netId, _amsPort);
                }
            });

            _isConnected = _adsClient.IsConnected;

            if (_isConnected)
            {
                _logger.LogInformation(
                    "Connected to PLC at {AmsNetId}:{AmsPort} (from config, local={IsLocal})",
                    _amsNetId, _amsPort, IsLocalAmsNetId(_amsNetId));
                _isHeld = false;

                await WriteSymbolAsync("GVL_Command.bAdsConnected", true);
            }
            else
            {
                LastError = $"ADS Connect returned without error but IsConnected=false for {_amsNetId}:{_amsPort}.";
            }

            return _isConnected;
        }
        catch (Exception ex)
        {
            LastError = BuildFriendlyAdsError(ex, amsNetId, amsPort);
            _logger.LogError(ex, "Failed to connect to PLC: {Detail}", LastError);
            _isConnected = false;
            return false;
        }
    }

    private static bool ShouldUseLocalRouter(string amsNetId)
    {
        if (string.IsNullOrWhiteSpace(amsNetId))
            return true;

        var normalized = amsNetId.Trim().ToLowerInvariant();
        if (normalized is "local" or "127.0.0.1.1.1" or "::1" or "localhost")
            return true;

        // Also treat the machine's real AMS Net ID as local (shown in UmRT / TcSysUI).
        try
        {
            var configured = AmsNetId.Parse(amsNetId.Trim());
            return configured == AmsNetId.Local;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsLocalAmsNetId(string amsNetId) => ShouldUseLocalRouter(amsNetId);

    private static string BuildFriendlyAdsError(Exception ex, string? amsNetId, int amsPort)
    {
        var message = ex.Message ?? string.Empty;

        if (message.Contains("ClientPortNotOpen", StringComparison.OrdinalIgnoreCase)
            || message.Contains("ConnectPortFailed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Cannot register AmsPort", StringComparison.OrdinalIgnoreCase))
        {
            return
                $"Cannot open an ADS client port to {amsNetId}:{amsPort}. " +
                "TwinCAT Message Router rejected the connection (ClientPortNotOpen). " +
                "Likely cause on this PC: TWO TcSystemServiceUm processes — " +
                "an orphan owns ADS port 48898 while UmRT_Default is another PID. " +
                "Fix: Task Manager → end the older TcSystemServiceUm that is NOT UmRT_Default, " +
                "or in UmRT press 'x' then restart UmRT_Default only (one instance). " +
                "Then confirm state=Run ('s'), activate ReceipeManager on port 851, restart Blazor, retry. " +
                $"Raw: {message}";
        }

        return $"ADS connection to {amsNetId}:{amsPort} failed: {message}";
    }

    public void Disconnect()
    {
        try
        {
            if (_adsClient != null && _isConnected)
            {
                _adsClient.Disconnect();
                _isConnected = false;
                _logger.LogInformation("Disconnected from PLC");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from PLC");
        }
    }

    public async Task<ProcessStatus?> ReadProcessStatusAsync()
    {
        if (!_isConnected || _adsClient == null)
            return null;

        try
        {
            var stateCode = await ReadSymbolAsync<short>("GVL_State.nState");

            var status = new ProcessStatus
            {
                State = Enum.IsDefined(typeof(PackMLState), (int)stateCode)
                    ? (PackMLState)stateCode
                    : PackMLState.Clearing,
                StateName = await ReadSymbolAsync<string>("GVL_State.sStateName"),
                CurrentStepIndex = await ReadSymbolAsync<ushort>("GVL_Process.nCurrentStepIndex"),
                CurrentStepName = await ReadSymbolAsync<string>("GVL_Process.sCurrentStepName"),
                TotalSteps = await ReadSymbolAsync<ushort>("GVL_Process.nTotalSteps"),
                StepTimeElapsed = await ReadSymbolAsync<ushort>("GVL_Process.nStepTimeElapsed_s"),
                StepTimeRemaining = await ReadSymbolAsync<ushort>("GVL_Process.nStepTimeRemaining_s"),
                Progress = await ReadSymbolAsync<float>("GVL_Process.fProgress"),
                ProcessDone = await ReadSymbolAsync<bool>("GVL_Process.bProcessDone"),
                IsHeld = await ReadSymbolAsync<bool>("GVL_State.bHeld"),
                ErrorCode = await ReadSymbolAsync<short>("GVL_Process.nErrorCode"),
                ErrorText = await ReadSymbolAsync<string>("GVL_Process.sErrorText")
            };

            ProcessStatusUpdated?.Invoke(this, status);
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading process status");
            return null;
        }
    }

    public async Task<bool> SendRecipeAsync(Recipe recipe)
    {
        if (!_isConnected || _adsClient == null)
            return false;

        try
        {
            await WriteSymbolAsync("GVL_Recipe.sRecipeName", recipe.Name);
            await WriteSymbolAsync("GVL_Recipe.fPreparationVolume", (float)recipe.PreparationVolume);
            await WriteSymbolAsync("GVL_Recipe.fPreparationConcentration", (float)recipe.PreparationConcentration);

            // Ordered process steps (Dosage, Melange, Extraction, Cuisson, ...)
            var steps = recipe.Steps.Take(MAX_STEPS).ToList();
            await WriteSymbolAsync("GVL_Recipe.nNumSteps", (ushort)steps.Count);
            for (int i = 0; i < steps.Count; i++)
            {
                await WriteSymbolAsync($"GVL_Recipe.aStepNames[{i + 1}]", steps[i]);
            }

            // Ingredients and quantities
            var ingredients = recipe.Ingredients.Take(MAX_INGREDIENTS).ToList();
            await WriteSymbolAsync("GVL_Recipe.nNumIngredients", (ushort)ingredients.Count);
            for (int i = 0; i < ingredients.Count; i++)
            {
                var ingredient = ingredients[i];
                await WriteSymbolAsync($"GVL_Recipe.aIngredientName[{i + 1}]", ingredient.Name);
                await WriteSymbolAsync($"GVL_Recipe.aIngredientQuantity[{i + 1}]", (float)ingredient.Quantity);
                await WriteSymbolAsync($"GVL_Recipe.aIngredientVolume[{i + 1}]", (float)ingredient.Volume);
                await WriteSymbolAsync($"GVL_Recipe.aIngredientMolarMass[{i + 1}]", (float)ingredient.MolarMass);
            }

            _logger.LogInformation($"Recipe '{recipe.Name}' sent to PLC ({steps.Count} steps, {ingredients.Count} ingredients)");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending recipe to PLC");
            return false;
        }
    }

    /// <summary>
    /// Sends a PackML command to the PLC. Reset/Clear/Start/Stop are sent as a momentary
    /// pulse (TRUE then FALSE); Hold toggles a level signal that pauses/resumes the active step.
    /// </summary>
    public async Task<bool> SendCommandAsync(PackMLCommand command)
    {
        if (!_isConnected || _adsClient == null)
            return false;

        try
        {
            if (command == PackMLCommand.Hold)
            {
                _isHeld = !_isHeld;
                await WriteSymbolAsync("GVL_Command.bHold", _isHeld);
                _logger.LogInformation($"Hold set to {_isHeld}");
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

            await WriteSymbolAsync(symbolName, true);
            await Task.Delay(COMMAND_PULSE_MS);
            await WriteSymbolAsync(symbolName, false);

            _logger.LogInformation($"PackML command {command} sent");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending PackML command {command}");
            return false;
        }
    }

    private async Task<T> ReadSymbolAsync<T>(string symbolName)
    {
        if (_adsClient == null)
            throw new InvalidOperationException("ADS client not connected");

        return await Task.Run(() =>
        {
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
                else
                {
                    return (T)_adsClient.ReadAny(handle, typeof(T));
                }
            }
            finally
            {
                _adsClient.DeleteVariableHandle(handle);
            }
        });
    }

    private async Task WriteSymbolAsync<T>(string symbolName, T value)
    {
        if (_adsClient == null)
            throw new InvalidOperationException("ADS client not connected");

        await Task.Run(() =>
        {
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
        });
    }

    public void Dispose()
    {
        Disconnect();
        _adsClient?.Dispose();
    }
}
