# Quick Start Guide - Blazor Gateway ADS

Get up and running with the Blazor industrial application in minutes!

## Prerequisites

✅ .NET 10 SDK ([Download](https://dotnet.microsoft.com/download))
✅ TwinCAT 3 XAE (optional, for PLC simulation)
✅ Windows OS (for ADS communication)

## Installation

### Step 1: Clone the Repository

```bash
git clone https://github.com/NicolasLBN/gateway-ads.git
cd gateway-ads/blazor-app
```

### Step 2: Restore Dependencies

```bash
dotnet restore
```

### Step 3: Run the Application

```bash
dotnet run
```

The application will start on `https://localhost:5001`

Open your browser and navigate to the URL shown in the console.

## First Time Setup

### 1. Start Without PLC (Development Mode)

You can run the application without a PLC connected. The UI will work, but you won't be able to connect to a machine or run processes.

### 2. Setup TwinCAT PLC (For Full Functionality)

If you want to test with the PLC simulator:

1. **Install TwinCAT 3 XAE** from Beckhoff
2. **Create a new PLC project** in Visual Studio
3. **Import PLC files:**
   - Copy all files from `/plc-simulator/GVLs/` to your project's GVLs folder
   - Copy all files from `/plc-simulator/POUs/` to your project's POUs folder
4. **Configure Task:**
   - Create PlcTask with 10ms cycle time
   - Assign port 851
5. **Build and activate** the configuration
6. **Run the PLC** in Run mode

### 3. Connect to PLC

1. Navigate to **Machine Settings** in the app
2. Select "Mixing Unit A" from the dropdown
3. Click **Connect**
4. You should see "Connected" status in the header

## Using the Application

### Create a Recipe

1. Go to **New Recipe**
2. Enter a recipe name (e.g., "Test Recipe")
3. Add ingredients:
   - Click "Add Ingredient"
   - Enter name (e.g., "Sugar")
   - Set quantity, volume, and molar mass
4. Click **Send Recipe to PLC**
5. Click **Run Process** to start

### Monitor Process

Watch the real-time updates:
- **Machine Status**: Temperature, pressure, speed
- **Process Timeline**: Current step and progress
- **Real-time Charts**: Live data visualization

### Generate Report

When the process completes (after ~36 seconds):
1. Click **Generate PDF Report**
2. Go to **History** to view and download the report

## Troubleshooting

### Application won't start

**Error**: "The framework 'Microsoft.NETCore.App', version '10.0.0' was not found"
**Solution**: Install .NET 10 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

### Can't connect to PLC

**Error**: "Failed to connect to PLC"
**Solutions**:
- Ensure TwinCAT is running (check system tray)
- Verify PLC is in Run mode (green light)
- Check Windows Firewall allows ADS (port 48898)
- Confirm AMS Net ID is correct (default: 127.0.0.1.1.1)

### Port already in use

**Error**: "Unable to bind to https://localhost:5001"
**Solution**: Change port in `Properties/launchSettings.json`

```json
"applicationUrl": "https://localhost:5002;http://localhost:5003"
```

## Development

### Hot Reload

For development with automatic reload on file changes:

```bash
dotnet watch
```

### Build for Production

```bash
dotnet publish -c Release -o ./publish
```

The output will be in the `./publish` directory.

### Run Production Build

```bash
cd publish
./BlazorApp
```

## Configuration

Edit `appsettings.json` to change default PLC settings:

```json
{
  "ADS": {
    "DefaultAmsNetId": "127.0.0.1.1.1",  // Change if using remote PLC
    "DefaultAmsPort": 851                 // Change if using different port
  }
}
```

## Project Structure

```
blazor-app/
├── Components/
│   ├── Pages/          # Main application pages
│   ├── Shared/         # Reusable components
│   └── Layout/         # Layout and navigation
├── Services/           # Business logic and PLC communication
├── Models/             # Data models
├── wwwroot/            # Static files and reports
└── Program.cs          # Application startup
```

## Common Tasks

### Add a New Machine

Edit `Services/MachineService.cs`:

```csharp
_machines.Add(new Machine
{
    Id = "4",
    Name = "Your Machine Name",
    AmsNetId = "192.168.1.100.1.1",
    AmsPort = 851,
    Description = "Your machine description"
});
```

### Change Polling Interval

Edit `Services/PlcPollingService.cs`:

```csharp
private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(500); // Change to desired interval
```

### Customize Theme Colors

Edit the CSS in component files or create a global stylesheet in `wwwroot/app.css`.

## Next Steps

- 📖 Read the [full README](README.md) for detailed documentation
- 🔄 Check the [Migration Guide](../MIGRATION_GUIDE.md) to understand the architecture
- 🏭 Review the [PLC documentation](../plc-simulator/README_PLC.md)

## Support

For issues or questions:
- Check the [Troubleshooting](#troubleshooting) section
- Review the documentation in `/blazor-app/README.md`
- Open an issue on GitHub

---

Happy coding! 🎉
