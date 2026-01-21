# Migration Guide: React/Node.js to Blazor .NET

This document provides a comprehensive guide on migrating the Industrial Gateway ADS application from React/Node.js to Blazor Server with .NET 10.

## Overview

The migration replaces:
- **React frontend** → **Blazor Server components**
- **Node.js backend** → **Integrated .NET services**
- **node-ads** → **Beckhoff.TwinCAT.Ads**
- **Zustand** → **AppStateService**
- **WebSocket** → **Background polling with state events**
- **Puppeteer** → **QuestPDF**

## Architecture Comparison

### Original (React/Node.js)

```
┌─────────────────────────────────────────────────────┐
│                     Browser                          │
│  ┌────────────────────────────────────────────┐    │
│  │          React Frontend                    │    │
│  │  - Mantine UI Components                   │    │
│  │  - Zustand State Management                │    │
│  │  - WebSocket Client                        │    │
│  └─────────────┬──────────────────────────────┘    │
└────────────────┼──────────────────────────────────┘
                 │ HTTP/WebSocket
                 ▼
┌─────────────────────────────────────────────────────┐
│              Node.js Backend                         │
│  ┌────────────────────────────────────────────┐    │
│  │  Express Server                            │    │
│  │  - REST API Routes                         │    │
│  │  - node-ads (ADS Client)                   │    │
│  │  - WebSocket Server                        │    │
│  │  - Puppeteer (PDF Generation)              │    │
│  └─────────────┬──────────────────────────────┘    │
└────────────────┼──────────────────────────────────┘
                 │ ADS Protocol
                 ▼
┌─────────────────────────────────────────────────────┐
│              TwinCAT 3 PLC                           │
│  - GVL Variables (Recipe, Process, Machine)        │
│  - Process State Machine                            │
│  - Machine Simulation                               │
└─────────────────────────────────────────────────────┘
```

### New (Blazor .NET)

```
┌─────────────────────────────────────────────────────┐
│                     Browser                          │
│  ┌────────────────────────────────────────────┐    │
│  │    Blazor Server UI (SignalR)              │    │
│  │  - Razor Components                        │    │
│  │  - Real-time Updates                       │    │
│  └─────────────┬──────────────────────────────┘    │
└────────────────┼──────────────────────────────────┘
                 │ SignalR (Built-in)
                 ▼
┌─────────────────────────────────────────────────────┐
│          Blazor Server Application (.NET 10)         │
│  ┌────────────────────────────────────────────┐    │
│  │  Services Layer                            │    │
│  │  - AdsService (Beckhoff.TwinCAT.Ads)       │    │
│  │  - AppStateService                         │    │
│  │  - PlcPollingService                       │    │
│  │  - PdfService (QuestPDF)                   │    │
│  └─────────────┬──────────────────────────────┘    │
└────────────────┼──────────────────────────────────┘
                 │ ADS Protocol
                 ▼
┌─────────────────────────────────────────────────────┐
│              TwinCAT 3 PLC                           │
│  - GVL Variables (Recipe, Process, Machine)        │
│  - Process State Machine                            │
│  - Machine Simulation                               │
│  *** NO CHANGES TO PLC CODE ***                     │
└─────────────────────────────────────────────────────┘
```

## Component Mapping

### Pages

| React Component | Blazor Component | Notes |
|----------------|------------------|-------|
| `HomePage.js` | `Home.razor` | Dashboard with navigation cards |
| `NewRecipePage.js` | `NewRecipe.razor` | Recipe creation and process monitoring |
| `HistoryPage.js` | `History.razor` | Recipe history and PDF downloads |
| `MachineSettingsPage.js` | `MachineSettings.razor` | PLC connection management |

### Shared Components

| React Component | Blazor Component | Notes |
|----------------|------------------|-------|
| `Header.js` | `MainLayout.razor` | Integrated into main layout |
| `MachineSelector.js` | `MachineSelector.razor` | Machine selection and connection |
| `MachineStatus.js` | `MachineStatus.razor` | Real-time machine parameters |
| `ProcessTimeline.js` | `ProcessTimeline.razor` | Process steps visualization |
| `RealtimeChart.js` | `RealtimeChart.razor` | SVG-based charts |

### Services & State Management

| React/Node.js | Blazor .NET | Implementation |
|---------------|-------------|----------------|
| Zustand Store | `AppStateService` | Singleton service with events |
| WebSocket | `PlcPollingService` | Background service polling PLC |
| node-ads | `AdsService` | Using Beckhoff.TwinCAT.Ads |
| API Routes | Direct service calls | No separate API layer needed |
| Puppeteer | `PdfService` | Using QuestPDF library |

## Key Technical Changes

### 1. ADS Communication

**Before (node-ads):**
```javascript
const ads = require('ads-client');
const client = new ads.Client({
    targetAmsNetId: '127.0.0.1.1.1',
    targetAdsPort: 851
});

await client.connect();
const value = await client.readSymbol('GVL_Machine.MotorTemperature');
```

**After (Beckhoff.TwinCAT.Ads):**
```csharp
using TwinCAT.Ads;

var adsClient = new AdsClient();
adsClient.Connect("127.0.0.1.1.1", 851);

var handle = adsClient.CreateVariableHandle("GVL_Machine.MotorTemperature");
var value = (float)adsClient.ReadAny(handle, typeof(float));
adsClient.DeleteVariableHandle(handle);
```

### 2. State Management

**Before (Zustand):**
```javascript
const useStore = create((set) => ({
    isConnected: false,
    machineStatus: null,
    setMachineStatus: (status) => set({ machineStatus: status })
}));
```

**After (AppStateService):**
```csharp
public class AppStateService
{
    public event Action? StateChanged;
    public MachineStatus? LatestMachineStatus { get; private set; }
    
    private void OnMachineStatusUpdated(object? sender, MachineStatus status)
    {
        LatestMachineStatus = status;
        StateChanged?.Invoke();
    }
}
```

### 3. Real-time Updates

**Before (WebSocket):**
```javascript
// Backend
wss.on('connection', (ws) => {
    setInterval(() => {
        const data = readPLCData();
        ws.send(JSON.stringify(data));
    }, 500);
});

// Frontend
useEffect(() => {
    const ws = new WebSocket('ws://localhost:3001/ws/status');
    ws.onmessage = (event) => {
        const data = JSON.parse(event.data);
        updateState(data);
    };
}, []);
```

**After (Background Service + Events):**
```csharp
// Background Service
public class PlcPollingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (adsService.IsConnected)
            {
                await adsService.ReadMachineStatusAsync(); // Triggers events
            }
            await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
        }
    }
}

// Component
@code {
    protected override void OnInitialized()
    {
        AppState.StateChanged += OnStateChanged;
    }
    
    private void OnStateChanged()
    {
        InvokeAsync(StateHasChanged); // Re-render component
    }
}
```

### 4. PDF Generation

**Before (Puppeteer):**
```javascript
const puppeteer = require('puppeteer');
const mustache = require('mustache');

const browser = await puppeteer.launch();
const page = await browser.newPage();
const html = mustache.render(template, data);
await page.setContent(html);
await page.pdf({ path: 'report.pdf' });
```

**After (QuestPDF):**
```csharp
using QuestPDF.Fluent;

var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Content().Column(column =>
        {
            column.Item().Text(report.RecipeName).SemiBold();
            column.Item().Table(table => {
                // Build table
            });
        });
    });
});

document.GeneratePdf(filePath);
```

## Styling Migration

The original app used Mantine UI components with a dark industrial theme. The Blazor version uses custom CSS that replicates the same visual style:

- **Color Scheme**: Blue gradients, clean whites, industrial grays
- **Typography**: Clear, readable fonts with proper hierarchy
- **Components**: Cards, buttons, forms styled to match Mantine
- **Layout**: Responsive grid system
- **Interactions**: Hover effects, transitions, animations

## Benefits of Migration

1. **Single Technology Stack**: Everything in C#/.NET
2. **Simplified Deployment**: One application instead of two
3. **Type Safety**: Full compile-time type checking
4. **Native ADS Support**: Official Beckhoff library
5. **Better Performance**: Compiled code, server-side rendering
6. **Easier Debugging**: Integrated debugging in Visual Studio/VS Code
7. **Maintainability**: Less code duplication, clearer architecture

## Running Both Versions

Both versions can coexist in the repository:

**Original (React/Node.js):**
```bash
# Terminal 1 - Backend
cd backend
npm install
npm start

# Terminal 2 - Frontend
cd frontend
npm install
npm start
```

**New (Blazor):**
```bash
cd blazor-app
dotnet run
```

## Migration Checklist

- [x] Create Blazor Server project structure
- [x] Install Beckhoff.TwinCAT.Ads NuGet package
- [x] Implement ADS service for PLC communication
- [x] Create data models (Recipe, Machine, ProcessStatus, etc.)
- [x] Implement state management service
- [x] Create background polling service
- [x] Migrate all React pages to Blazor components
- [x] Migrate all shared components
- [x] Implement PDF generation
- [x] Apply industrial theme styling
- [x] Test with PLC simulator
- [x] Code review and security scan
- [x] Documentation

## PLC Compatibility

✅ **The PLC code requires NO CHANGES**

The Blazor application communicates with the exact same TwinCAT PLC program using the same:
- Global Variable Lists (GVLs)
- Variable names and types
- ADS protocol
- Process logic

## Future Enhancements

Possible improvements for the Blazor version:

1. **Authentication**: Add user authentication and authorization
2. **Database Integration**: Store recipes and history in database
3. **SignalR Enhancement**: Replace polling with SignalR push
4. **Mobile App**: Create Blazor Hybrid mobile app
5. **Advanced Charts**: Use chart library for enhanced visualizations
6. **Multi-tenancy**: Support multiple customers/factories
7. **Alerts**: Email/SMS notifications for process issues
8. **Audit Log**: Track all user actions and changes

## Conclusion

The Blazor migration successfully replicates all functionality from the React/Node.js version while providing a more integrated, maintainable solution. The application is production-ready with proper error handling, security scanning, and comprehensive documentation.
