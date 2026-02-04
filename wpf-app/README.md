# WPF Gateway ADS Application

A Windows Presentation Foundation (WPF) application for industrial recipe management and PLC control via ADS protocol, with Python-based PDF report generation.

## Overview

This WPF application is based on the existing Blazor application functionality and provides:
- Real-time PLC communication via TwinCAT ADS protocol
- Recipe management and process control
- Machine status monitoring
- Python-based PDF report generation using ReportLab

## Architecture

```
/wpf-app
  /Models           - Data models (Recipe, Machine, MachineStatus, ProcessStatus, Report)
  /Services         - Business logic and PLC communication
    - AdsService.cs             - TwinCAT ADS communication
    - AppStateService.cs        - Application state management
    - MachineService.cs         - Machine configuration
    - PlcPollingService.cs      - Background PLC polling
    - PythonReportService.cs    - Python script integration for PDF reports
  /ViewModels       - MVVM ViewModels
    - MainWindowViewModel.cs    - Main window logic
    - RelayCommand.cs           - Command implementation
  /Converters       - Value converters for XAML binding
  /Views            - WPF user controls and windows
  /Python           - Python scripts for report generation
    - generate_report.py        - PDF generation script
    - requirements.txt          - Python dependencies
  MainWindow.xaml   - Main application window
  App.xaml          - Application entry point
```

## Technologies

### .NET/WPF
- **.NET 8** - Framework
- **WPF** - Windows Presentation Foundation UI
- **Beckhoff.TwinCAT.Ads** (v6.1.203) - ADS communication with TwinCAT PLC
- **Microsoft.Extensions.DependencyInjection** - Dependency injection
- **Microsoft.Extensions.Logging** - Logging
- **Newtonsoft.Json** - JSON serialization
- **MVVM Pattern** - Model-View-ViewModel architecture

### Python
- **Python 3.x** - Runtime
- **ReportLab** (v4.0.7) - PDF generation library

### PLC
- **TwinCAT 3** - PLC runtime
- **Structured Text (ST)** - Programming language

## Prerequisites

- **.NET 8 SDK** or later
- **Python 3.8+** with pip
- **TwinCAT 3 XAE** (for PLC simulation)
- **Windows** (required for WPF and TwinCAT ADS)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/NicolasLBN/gateway-ads.git
cd gateway-ads/wpf-app
```

### 2. Install .NET Dependencies

```bash
dotnet restore
```

### 3. Install Python Dependencies

```bash
cd Python
pip install -r requirements.txt
cd ..
```

Alternatively, use a virtual environment:

```bash
cd Python
python -m venv venv
# On Windows:
venv\Scripts\activate
# On Linux/Mac:
source venv/bin/activate
pip install -r requirements.txt
cd ..
```

### 4. PLC Setup

If you have TwinCAT 3 installed:

1. Open TwinCAT 3 XAE (Visual Studio with TwinCAT extension)
2. Create a new TwinCAT PLC Project
3. Import files from `/plc-simulator` directory:
   - Import GVL files (from `GVLs/`) into your project's GVLs folder
   - Import POU files (from `POUs/`) into your project's POUs folder
   - Configure PlcTask (10ms cycle, port 851)
4. Build and activate the configuration
5. Start the PLC in Run mode

See `/plc-simulator/README_PLC.md` for detailed PLC setup instructions.

## Running the Application

### Start the WPF Application

```bash
dotnet run
```

Or build and run the executable:

```bash
dotnet build
cd bin/Debug/net8.0-windows
./WpfApp.exe
```

## Usage

### 1. Machine Connection

1. **Select a Machine**: Choose from Mixing Unit A, B, or C from the dropdown
2. **Connect**: Click the "Connect" button to establish ADS connection with the PLC
3. **Monitor Status**: View real-time machine parameters:
   - Motor Temperature (°C)
   - Oil Pressure (bar)
   - Motor Speed (RPM)
4. **Process Status**: Monitor current process step and overall progress

### 2. Recipe Management

1. **Create Recipe**:
   - Enter recipe name
   - Set preparation volume and concentration
   - Add ingredients (name, quantity, volume, molar mass)
2. **Send to PLC**: Click "Send Recipe to PLC" to upload the recipe
3. **Start Process**: Click "Start Process" to begin automated process execution
4. **Monitor Progress**: Watch real-time updates of:
   - Current step
   - Progress percentage
   - Machine parameters
5. **Reset**: Click "Reset Process" to reset the process state

### 3. Report Generation (Python)

When a process is complete, reports can be generated using the Python script:

```csharp
var reportService = new PythonReportService(logger);
var pdfPath = await reportService.GeneratePdfReportAsync(report);
```

The Python script generates professional PDF reports with:
- Recipe information
- Ingredients table
- Process execution steps with parameters
- Timestamps and metadata

## PLC Communication

The application uses the Beckhoff.TwinCAT.Ads library to communicate with TwinCAT 3 PLC:

### Read Variables (Machine Status)
- `GVL_Machine.MotorTemperature` - Motor temperature (°C)
- `GVL_Machine.OilPressure` - Oil pressure (bar)
- `GVL_Machine.MotorSpeed` - Motor speed (RPM)
- Warning flags for each parameter

### Read Variables (Process Status)
- `GVL_Process.CurrentStep` - Current step number (0-7)
- `GVL_Process.StepName` - Current step name
- `GVL_Process.Progress` - Overall progress (0.0-1.0)
- `GVL_Process.StepProgress` - Current step progress (0.0-1.0)
- Time counters and error information

### Write Variables (Recipe Data)
- `GVL_Recipe.RecipeName` - Recipe name
- `GVL_Recipe.NumIngredients` - Number of ingredients
- Arrays for ingredient data (name, quantity, volume, molar mass)

### Write Variables (Commands)
- `GVL_Command.StartProcess` - Start the process
- `GVL_Command.ResetProcess` - Reset the process
- `GVL_Command.AdsConnected` - Connection status

## Process Steps

The PLC simulator implements a 7-step process:

1. **Idle** (Step 0) - Waiting for start
2. **Preparation** (Step 1) - 5 seconds
3. **Dosing Ingredient A** (Step 2) - 7 seconds
4. **Dosing Ingredient B** (Step 3) - 7 seconds
5. **Mixing** (Step 4) - 10 seconds
6. **Verification** (Step 5) - 4 seconds
7. **Finalizing** (Step 6) - 3 seconds
8. **Done** (Step 7) - Complete

Total process time: ~36 seconds

## Configuration

### Machine Configuration

Modify machines in `Services/MachineService.cs`:

```csharp
new Machine
{
    Id = "4",
    Name = "Your Machine Name",
    AmsNetId = "192.168.1.100.1.1",  // Your PLC AMS Net ID
    AmsPort = 851,
    Description = "Description of your machine"
}
```

### Polling Interval

Adjust PLC polling frequency in `Services/PlcPollingService.cs`:

```csharp
_timer = new DispatcherTimer
{
    Interval = TimeSpan.FromMilliseconds(500) // 500ms default
};
```

### Python Script Path

The Python script path is automatically resolved relative to the application directory. If you need to customize it, modify `Services/PythonReportService.cs`:

```csharp
_pythonScriptPath = Path.Combine(appDirectory, "Python", "generate_report.py");
```

## Troubleshooting

### ADS Connection Issues

1. **Ensure TwinCAT 3 is running** in Config or Run mode
2. **Check AMS Net ID and Port** - Default is `127.0.0.1.1.1:851`
3. **Verify Windows Firewall** allows ADS communication (UDP port 48898)
4. **Ensure TwinCAT router is running** (system tray icon)
5. **Check route configuration** in TwinCAT System Manager

### Build Issues

1. Verify .NET 8 SDK is installed: `dotnet --version`
2. Restore dependencies: `dotnet restore`
3. Clean and rebuild: `dotnet clean && dotnet build`

### Python Report Generation Issues

1. **Python not found**:
   - Ensure Python is installed and in PATH
   - Try using full path: `python.exe` or `python3`

2. **Missing packages**:
   ```bash
   cd Python
   pip install -r requirements.txt
   ```

3. **Reports directory**:
   - Reports are saved in `bin/Debug/net8.0-windows/Reports/`
   - Directory is created automatically

4. **Permission errors**:
   - Ensure the application has write permissions
   - Check antivirus isn't blocking file creation

## Differences from Blazor Version

| Feature | Blazor | WPF |
|---------|--------|-----|
| UI Framework | Blazor Server (Web) | WPF (Desktop) |
| State Management | AppStateService | AppStateService + MVVM |
| Real-time Updates | SignalR + Polling | Polling with INotifyPropertyChanged |
| PDF Generation | QuestPDF (C#) | Python + ReportLab |
| Deployment | Web Server | Desktop Application |
| Platform | Windows (ADS) + Web Browser | Windows Only |

## Development

### Project Structure

- **MVVM Pattern**: Separation of concerns with Models, Views, and ViewModels
- **Dependency Injection**: Services registered in `App.xaml.cs`
- **Async/Await**: Asynchronous PLC communication
- **INotifyPropertyChanged**: Property change notifications for UI binding
- **RelayCommand**: Command pattern for button actions

### Adding New Features

1. **New Service**: Add to `Services/` and register in `App.xaml.cs`
2. **New View**: Add XAML in `Views/` or root, with code-behind
3. **New ViewModel**: Add to `ViewModels/` with INotifyPropertyChanged
4. **New Model**: Add to `Models/`

## License

MIT

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Contact

For questions or support, please open an issue on GitHub.

---

Built with ❤️ for industrial automation using WPF, .NET, and Python
