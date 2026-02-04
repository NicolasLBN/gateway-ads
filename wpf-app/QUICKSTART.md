# Quick Start Guide - WPF Gateway ADS Application

## Prerequisites Check

Before starting, ensure you have:

- [x] Windows OS (required for WPF and TwinCAT ADS)
- [x] .NET 8 SDK installed - Check with: `dotnet --version`
- [x] Python 3.8+ installed - Check with: `python --version`
- [x] TwinCAT 3 installed (optional, for PLC simulation)

## 5-Minute Setup

### Step 1: Install Dependencies

```bash
# Navigate to wpf-app directory
cd wpf-app

# Restore .NET packages
dotnet restore

# Install Python dependencies
cd Python
pip install -r requirements.txt
cd ..
```

### Step 2: Run the Application

```bash
# Run the application
dotnet run
```

The WPF window should open with the title "Gateway ADS - WPF Application".

## First Run - Without PLC

If you don't have a PLC connected, you can still explore the UI:

1. **Machine Connection Tab**:
   - Select a machine from the dropdown (Mixing Unit A, B, or C)
   - Click "Connect" - it will fail without a PLC, but you can see the UI
   
2. **Recipe Management Tab**:
   - Fill in recipe details
   - Add preparation volume and concentration
   - Explore the interface

3. **History Tab**:
   - Placeholder for viewing completed recipes

## With PLC Simulation

### Step 1: Start TwinCAT PLC

1. Open TwinCAT 3 XAE (Visual Studio with TwinCAT extension)
2. Load or create your PLC project (see `/plc-simulator` for details)
3. Set PLC to **Run mode**
4. Verify AMS Net ID is `127.0.0.1.1.1` and port is `851`

### Step 2: Connect in WPF App

1. Launch the WPF application: `dotnet run`
2. Go to **Machine Connection** tab
3. Select "Mixing Unit A" (or your configured machine)
4. Click **Connect**
5. You should see "Connected to Mixing Unit A" in the header
6. Machine status values should update in real-time

### Step 3: Create and Run a Recipe

1. Switch to **Recipe Management** tab
2. Enter recipe details:
   - Recipe Name: "Test Recipe"
   - Preparation Volume: 1000
   - Preparation Concentration: 0.5
3. Click **Send Recipe to PLC**
4. Click **Start Process**
5. Watch the process execute in the Process Status section

### Step 4: Generate PDF Report

After the process completes (takes ~36 seconds), you can generate a report:

```csharp
// This will be available in the History tab in the future
// For now, it can be triggered programmatically
```

Reports are saved in: `bin/Debug/net8.0-windows/Reports/`

## Troubleshooting Quick Fixes

### "Connection failed"
- ✅ Check TwinCAT is running
- ✅ Verify PLC is in Run mode
- ✅ Check firewall settings (allow UDP 48898)
- ✅ Verify AMS Net ID matches in Machine Settings

### "Python script failed"
- ✅ Check Python is in PATH: `python --version`
- ✅ Install dependencies: `pip install reportlab`
- ✅ Check Python script exists in `Python/` folder

### "Build failed"
- ✅ Clean and restore: `dotnet clean && dotnet restore`
- ✅ Check .NET 8 SDK: `dotnet --version`
- ✅ Rebuild: `dotnet build`

## Next Steps

1. **Configure Machines**: Edit `Services/MachineService.cs` to add your machines
2. **Adjust Polling**: Modify polling interval in `Services/PlcPollingService.cs`
3. **Customize UI**: Edit `MainWindow.xaml` for layout changes
4. **Add Features**: Extend ViewModels and Services as needed

## Key Files

- `App.xaml.cs` - Application startup and dependency injection
- `MainWindow.xaml` - Main UI layout
- `ViewModels/MainWindowViewModel.cs` - Main window logic
- `Services/AdsService.cs` - PLC communication
- `Python/generate_report.py` - PDF report generation

## Default Configuration

- **AMS Net ID**: 127.0.0.1.1.1
- **AMS Port**: 851
- **Polling Interval**: 500ms
- **Reports Directory**: `bin/Debug/net8.0-windows/Reports/`

## Support

- See full README.md for detailed documentation
- Check `/plc-simulator/README_PLC.md` for PLC setup
- Open an issue on GitHub for help

---

**Tip**: Start with the Machine Connection tab to test PLC connectivity before creating recipes!
