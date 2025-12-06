# Industrial Application - Gateway ADS

Complete industrial application for recipe management, PLC control via ADS, and real-time monitoring.

## 🎯 Features

- **Recipe Management**: Create and manage recipes with multiple ingredients
- **PLC Communication**: Real-time communication with TwinCAT 3 PLC via ADS protocol
- **Real-time Monitoring**: Live monitoring of machine parameters (temperature, pressure, speed)
- **Process Control**: Launch and monitor automated processes
- **PDF Reports**: Generate comprehensive PDF reports of completed recipes
- **Multi-Machine Support**: Select and connect to different machines
- **WebSocket Communication**: Real-time data updates via WebSocket

## 🏗️ Architecture

```
/project
  /frontend          - React.js application
    /src
      /components    - Reusable UI components
      /pages         - Page components (Home, NewRecipe, History)
      /hooks         - Custom hooks (Zustand store, WebSocket)
      /services      - API and WebSocket services
  /backend           - Node.js/Express server
    /routes          - API routes
    /ads             - ADS client for PLC communication
    /pdf             - PDF generation
    /ws              - WebSocket server
  /plc-simulator     - TwinCAT 3 PLC program
    /GVLs            - Global Variable Lists
    /POUs            - Program Organization Units
    /Tasks           - Task configuration
```

## 🛠️ Technologies

### Frontend
- **React.js** - UI framework
- **Mantine** - UI component library (industrial dark theme)
- **Zustand** - State management
- **Recharts** - Real-time data visualization
- **React Hook Form** - Form management
- **WebSocket** - Real-time communication

### Backend
- **Node.js** - Runtime
- **Express** - Web framework
- **node-ads** - ADS communication with TwinCAT PLC
- **Puppeteer** - PDF generation
- **Mustache** - HTML templating
- **ws** - WebSocket server

### PLC
- **TwinCAT 3** - PLC runtime
- **Structured Text (ST)** - Programming language

## 📋 Prerequisites

- **Node.js** 16+ and npm
- **TwinCAT 3 XAE** (for PLC simulation)
- **Windows** (for TwinCAT 3)

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/NicolasLBN/gateway-ads.git
cd gateway-ads
```

### 2. Backend Setup

```bash
cd backend
npm install

# Copy environment file
cp .env.example .env

# Edit .env with your PLC configuration
# AMS_NET_ID=127.0.0.1.1.1
# AMS_PORT=851
```

### 3. Frontend Setup

```bash
cd frontend
npm install

# Copy environment file
cp .env.example .env

# Edit .env if needed
# REACT_APP_API_URL=http://localhost:3001/api
# REACT_APP_WS_URL=ws://localhost:3001/ws/status
```

### 4. PLC Setup (Optional)

If you have TwinCAT 3:

1. Open TwinCAT 3 XAE (Visual Studio with TwinCAT extension)
2. Create a new TwinCAT PLC Project
3. Import files from `/plc-simulator` directory:
   - Import GVL files into GVLs folder
   - Import POU files into POUs folder
   - Configure PlcTask (10ms cycle, port 851)
4. Build and activate the configuration
5. Run the PLC

See `/plc-simulator/README_PLC.md` for detailed instructions.

## 🎮 Running the Application

### Start Backend Server

```bash
cd backend
npm start

# For development with auto-reload:
npm run dev
```

Backend will run on `http://localhost:3001`

### Start Frontend Application

```bash
cd frontend
npm start
```

Frontend will run on `http://localhost:3000`

The application will automatically open in your browser.

## 📱 Usage

### 1. Home Page

- View connection status
- Select a machine from the dropdown
- Connect to the PLC
- Monitor real-time machine status
- View process status

### 2. Create New Recipe

- Enter recipe name
- Add ingredients with:
  - Name
  - Quantity (grams)
  - Volume (ml)
  - Molar mass (g/L)
- Send recipe to PLC
- Launch the process
- Monitor progress in real-time:
  - Process timeline
  - Real-time charts
  - Machine parameters
- Generate PDF report when complete

### 3. Recipe History

- View all completed recipes
- Download PDF reports
- See recipe details

## 🔌 API Endpoints

### Machines
- `GET /api/machines` - Get list of machines
- `GET /api/machines/:id` - Get specific machine

### Connection
- `POST /api/status/connect` - Connect to PLC
- `GET /api/status` - Get connection and PLC status
- `POST /api/status/disconnect` - Disconnect from PLC

### Recipe
- `POST /api/recipe` - Send recipe to PLC
- `POST /api/recipe/run` - Start process
- `POST /api/recipe/reset` - Reset process

### History
- `GET /api/history` - Get all reports
- `GET /api/history/:id` - Get specific report
- `POST /api/history` - Create new report
- `GET /api/history/:id/pdf` - Download PDF

### WebSocket
- `ws://localhost:3001/ws/status` - Real-time updates

## 🔧 Configuration

### Backend Configuration (.env)

```env
PORT=3001
AMS_NET_ID=127.0.0.1.1.1
AMS_PORT=851
```

### Frontend Configuration (.env)

```env
REACT_APP_API_URL=http://localhost:3001/api
REACT_APP_WS_URL=ws://localhost:3001/ws/status
```

## 📊 Process Steps

The PLC simulator implements a 7-step process:

1. **Idle** - Waiting for start
2. **Preparation** - 5 seconds
3. **Dosing Ingredient A** - 7 seconds
4. **Dosing Ingredient B** - 7 seconds
5. **Mixing** - 10 seconds
6. **Verification** - 4 seconds
7. **Finalizing** - 3 seconds
8. **Done** - Process complete

Total process time: ~36 seconds

## 🎨 UI Theme

The application uses a dark industrial theme with Mantine components:
- Dark color scheme
- High contrast
- Professional industrial aesthetics
- Responsive design

## 📝 PLC Variables

### Machine Status (Read)
- `GVL_Machine.MotorTemperature` - Motor temperature (°C)
- `GVL_Machine.OilPressure` - Oil pressure (bar)
- `GVL_Machine.MotorSpeed` - Motor speed (RPM)
- Warning flags for each parameter

### Process Status (Read)
- `GVL_Process.CurrentStep` - Current step number
- `GVL_Process.StepName` - Step name
- `GVL_Process.Progress` - Overall progress (0-1)
- `GVL_Process.StepProgress` - Current step progress (0-1)
- Time counters and error information

### Recipe Data (Write)
- `GVL_Recipe.RecipeName` - Recipe name
- `GVL_Recipe.NumIngredients` - Number of ingredients
- Arrays for ingredient data (name, quantity, volume, molar mass)

### Commands (Write)
- `GVL_Command.StartProcess` - Start process
- `GVL_Command.ResetProcess` - Reset process
- `GVL_Command.AdsConnected` - Connection status

## 🐛 Troubleshooting

### ADS Connection Issues

1. Ensure TwinCAT 3 is running
2. Check AMS Net ID and Port in `.env`
3. Verify Windows Firewall allows ADS communication
4. Ensure TwinCAT router is running

### WebSocket Connection Issues

1. Check backend is running
2. Verify WebSocket URL in frontend `.env`
3. Check browser console for errors

### PDF Generation Issues

1. Ensure Puppeteer is installed correctly
2. Check reports directory exists and is writable
3. Verify sufficient disk space

## 📄 License

MIT

## 👥 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Contact

For questions or support, please open an issue on GitHub.

---

Built with ❤️ for industrial automation