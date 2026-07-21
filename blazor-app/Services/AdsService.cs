using TwinCAT.Ads;
using BlazorApp.Models;
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
    private bool _isConnected;
    private string _amsNetId = "127.0.0.1.1.1";
    private int _amsPort = 851;
    private bool _isHeld;

    public event EventHandler<ProcessStatus>? ProcessStatusUpdated;

    public bool IsConnected => _isConnected;

    public AdsService(ILogger<AdsService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ConnectAsync(string amsNetId, int amsPort)
    {
        try
        {
            _amsNetId = amsNetId;
            _amsPort = amsPort;

            _adsClient?.Dispose();
            _adsClient = new AdsClient();

            await Task.Run(() => _adsClient.Connect(_amsNetId, _amsPort));

            _isConnected = _adsClient.IsConnected;

            if (_isConnected)
            {
                _logger.LogInformation($"Connected to PLC at {_amsNetId}:{_amsPort}");
                _isHeld = false;

                // Let the PLC know a client is now connected
                await WriteSymbolAsync("GVL_Command.bAdsConnected", true);
            }

            return _isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to PLC");
            _isConnected = false;
            return false;
        }
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
