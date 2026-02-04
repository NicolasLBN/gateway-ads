using TwinCAT.Ads;
using WpfApp.Models;
using System.Text;
using Microsoft.Extensions.Logging;

namespace WpfApp.Services;

public class AdsService : IDisposable
{
    private const int ADS_STRING_BUFFER_SIZE = 255;
    private const int ADS_STRING_MAX_LENGTH = 254;
    
    private AdsClient? _adsClient;
    private readonly ILogger<AdsService> _logger;
    private bool _isConnected;
    private string _amsNetId = "127.0.0.1.1.1";
    private int _amsPort = 851;

    public event EventHandler<MachineStatus>? MachineStatusUpdated;
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
                
                // Set connection status in PLC
                await WriteSymbolAsync("GVL_Command.AdsConnected", true);
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

    public async Task<MachineStatus?> ReadMachineStatusAsync()
    {
        if (!_isConnected || _adsClient == null)
            return null;

        try
        {
            var status = new MachineStatus
            {
                MotorTemperature = await ReadSymbolAsync<float>("GVL_Machine.MotorTemperature"),
                OilPressure = await ReadSymbolAsync<float>("GVL_Machine.OilPressure"),
                MotorSpeed = await ReadSymbolAsync<float>("GVL_Machine.MotorSpeed"),
                TempWarning = await ReadSymbolAsync<bool>("GVL_Machine.TempWarning"),
                PressureWarning = await ReadSymbolAsync<bool>("GVL_Machine.PressureWarning"),
                SpeedWarning = await ReadSymbolAsync<bool>("GVL_Machine.SpeedWarning")
            };

            MachineStatusUpdated?.Invoke(this, status);
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading machine status");
            return null;
        }
    }

    public async Task<ProcessStatus?> ReadProcessStatusAsync()
    {
        if (!_isConnected || _adsClient == null)
            return null;

        try
        {
            var status = new ProcessStatus
            {
                CurrentStep = await ReadSymbolAsync<short>("GVL_Process.CurrentStep"),
                StepName = await ReadSymbolAsync<string>("GVL_Process.StepName"),
                Progress = await ReadSymbolAsync<float>("GVL_Process.Progress"),
                StepProgress = await ReadSymbolAsync<float>("GVL_Process.StepProgress"),
                StepTime = await ReadSymbolAsync<uint>("GVL_Process.StepTime_s"),
                TotalTime = await ReadSymbolAsync<uint>("GVL_Process.TotalTime_s"),
                ErrorCode = await ReadSymbolAsync<short>("GVL_Process.ErrorCode"),
                ErrorText = await ReadSymbolAsync<string>("GVL_Process.ErrorText"),
                ProcessDone = await ReadSymbolAsync<bool>("GVL_Process.ProcessDone")
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
            // Send recipe name
            await WriteSymbolAsync("GVL_Recipe.RecipeName", recipe.Name);
            
            // Send number of ingredients
            await WriteSymbolAsync("GVL_Recipe.NumIngredients", (uint)recipe.Ingredients.Count);

            // Send ingredient data
            for (int i = 0; i < recipe.Ingredients.Count && i < 10; i++)
            {
                var ingredient = recipe.Ingredients[i];
                await WriteSymbolAsync($"GVL_Recipe.IngredientName[{i + 1}]", ingredient.Name);
                await WriteSymbolAsync($"GVL_Recipe.IngredientQuantity[{i + 1}]", (float)ingredient.Quantity);
                await WriteSymbolAsync($"GVL_Recipe.IngredientVolume[{i + 1}]", (float)ingredient.Volume);
                await WriteSymbolAsync($"GVL_Recipe.IngredientMolarMass[{i + 1}]", (float)ingredient.MolarMass);
            }

            _logger.LogInformation($"Recipe '{recipe.Name}' sent to PLC");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending recipe to PLC");
            return false;
        }
    }

    public async Task<bool> StartProcessAsync()
    {
        if (!_isConnected || _adsClient == null)
            return false;

        try
        {
            // Reset first, then start
            await WriteSymbolAsync("GVL_Command.ResetProcess", false);
            await Task.Delay(100);
            await WriteSymbolAsync("GVL_Command.StartProcess", true);
            await Task.Delay(100);
            await WriteSymbolAsync("GVL_Command.StartProcess", false);
            
            _logger.LogInformation("Process started");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting process");
            return false;
        }
    }

    public async Task<bool> ResetProcessAsync()
    {
        if (!_isConnected || _adsClient == null)
            return false;

        try
        {
            await WriteSymbolAsync("GVL_Command.ResetProcess", true);
            await Task.Delay(100);
            await WriteSymbolAsync("GVL_Command.ResetProcess", false);
            
            _logger.LogInformation("Process reset");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting process");
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
