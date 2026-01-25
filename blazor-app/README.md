# Blazor Gateway ADS - Industrial Application

Complete industrial application for recipe management, PLC control via ADS, and real-time monitoring, built with **Blazor Server** and **.NET 8**.

## 🎯 Features

- **Recipe Management**: Create and manage recipes with multiple ingredients
- **PLC Communication**: Real-time communication with TwinCAT 3 PLC via ADS protocol using Beckhoff.TwinCAT.Ads library
- **Real-time Monitoring**: Live monitoring of machine parameters (temperature, pressure, speed)
- **Process Control**: Launch and monitor automated processes
- **PDF Reports**: Generate comprehensive PDF reports of completed recipes using QuestPDF
- **Multi-Machine Support**: Select and connect to different machines
- **Real-time Updates**: Automatic UI updates when PLC data changes

## 🏗️ Architecture

```
/blazor-app
  /Components
    /Layout       - Application layout and navigation
    /Pages        - Main pages (Home, NewRecipe, History, MachineSettings)
    /Shared       - Reusable components (MachineSelector, MachineStatus, ProcessTimeline, RealtimeChart)
  /Services       - Business logic and PLC communication
    - AdsService.cs           - TwinCAT ADS communication
    - AppStateService.cs      - Application state management
    - MachineService.cs       - Machine configuration
    - ReportService.cs        - Report management
    - PdfService.cs           - PDF generation
    - PlcPollingService.cs    - Background PLC polling
  /Models         - Data models (Recipe, Machine, ProcessStatus, etc.)
  /wwwroot        - Static files and generated reports
/plc-simulator    - TwinCAT 3 PLC program (unchanged from original)
```

## 🛠️ Technologies

### Blazor Application
- **.NET 8** - Framework
- **Blazor Server** - Interactive web UI
- **Beckhoff.TwinCAT.Ads** (v6.1.203) - ADS communication with TwinCAT PLC
- **QuestPDF** (v2024.7.3) - PDF report generation
- **C#** - Programming language

### PLC
- **TwinCAT 3** - PLC runtime
- **Structured Text (ST)** - Programming language

## 📋 Prerequisites

- **.NET 8 SDK** or later
- **TwinCAT 3 XAE** (for PLC simulation)
- **Windows** (for TwinCAT 3 - ADS protocol is Windows-specific)

## 🚀 Installation 

### 1. Clone the Repository 

```bash
git clone https://github.com/NicolasLBN/gateway-ads.git
cd gateway-ads/blazor-app
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Application

The default configuration in `appsettings.json`:

```json
{
  "ADS": {
    "DefaultAmsNetId": "127.0.0.1.1.1",
    "DefaultAmsPort": 851
  }
}
```

You can modify these settings if your TwinCAT setup uses different values.

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

## 🎮 Running the Application

### Start the Blazor Application

```bash
cd blazor-app
dotnet run
```

Or with hot reload for development:

```bash
dotnet watch
```

The application will start on `https://localhost:5001` (or the port shown in console).

Open your browser and navigate to the URL shown in the console.

## 📱 Usage

### 1. Machine Settings

- Navigate to **Machine Settings** from the main menu
- Select a machine from the dropdown (Mixing Unit A, B, or C)
- Click **Connect** to establish ADS connection with the PLC
- Connection status will update in the header

### 2. Create New Recipe

- Navigate to **New Recipe**
- Enter recipe details:
  - Recipe name
  - Preparation volume and concentration (optional)
  - Add ingredients with name, quantity, volume, and molar mass
- Click **Send Recipe to PLC** to upload the recipe
- Click **Run Process** to start the automated process
- Monitor real-time progress:
  - Machine status (temperature, pressure, speed)
  - Process timeline with step progress
  - Real-time charts of machine parameters
- When process completes, click **Generate PDF Report**

### 3. Recipe History

- View all completed recipes
- See recipe details and ingredients
- Download PDF reports

## 🔌 PLC Communication

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

## 📊 Process Steps

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

## 🎨 UI Theme

The application uses a modern industrial theme:
- Clean, professional design
- Blue gradient header
- High contrast for readability
- Responsive layout for different screen sizes
- Real-time status indicators
- Color-coded warnings and alerts

## 🔧 Configuration

### Multiple Machines

The application supports multiple machines out of the box. Configure additional machines in `Services/MachineService.cs`:

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
private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(500); // 500ms default
```

## 🐛 Troubleshooting

### ADS Connection Issues

1. **Ensure TwinCAT 3 is running** in Config or Run mode
2. **Check AMS Net ID and Port** in Machine Settings
3. **Verify Windows Firewall** allows ADS communication (UDP port 48898)
4. **Ensure TwinCAT router is running** (system tray icon)
5. **Check route configuration** in TwinCAT System Manager

### Application Won't Start

1. Verify .NET 8 SDK is installed: `dotnet --version`
2. Restore dependencies: `dotnet restore`
3. Check for port conflicts (default is 5001)

### PDF Generation Issues

1. Ensure `wwwroot/reports` directory exists and is writable
2. Check available disk space
3. Verify QuestPDF license (Community edition is used)

## 🔄 Migration from React/Node.js

This Blazor application replaces the original React frontend and Node.js backend with a single integrated solution:

- **React components** → **Blazor components** (.razor files)
- **Zustand store** → **AppStateService** (singleton service)
- **WebSocket** → **Background polling service** with state change events
- **node-ads** → **Beckhoff.TwinCAT.Ads** library
- **Puppeteer PDF** → **QuestPDF**
- **Mantine UI** → **Custom CSS** with similar styling

### Key Differences

| Feature | React/Node.js | Blazor |
|---------|---------------|---------|
| Frontend | React.js | Blazor Server |
| Backend | Node.js/Express | Integrated in Blazor |
| State Management | Zustand | AppStateService |
| Real-time Updates | WebSocket | SignalR (built-in) + Polling |
| ADS Library | node-ads | Beckhoff.TwinCAT.Ads |
| PDF Generation | Puppeteer | QuestPDF |
| Language | JavaScript/TypeScript | C# |

## 📄 License

MIT

## 👥 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Contact

For questions or support, please open an issue on GitHub.

---

Built with ❤️ for industrial automation using Blazor and TwinCAT
