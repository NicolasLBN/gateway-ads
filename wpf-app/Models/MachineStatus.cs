namespace WpfApp.Models;

public class MachineStatus
{
    public double MotorTemperature { get; set; }
    public double OilPressure { get; set; }
    public double MotorSpeed { get; set; }
    public bool TempWarning { get; set; }
    public bool PressureWarning { get; set; }
    public bool SpeedWarning { get; set; }
}
