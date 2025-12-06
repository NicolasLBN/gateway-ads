# Quick Start Guide

Get the application running in 5 minutes!

## Prerequisites

- Node.js 16+ installed
- npm installed

## Installation

### 1. Install Backend Dependencies

```bash
cd backend
npm install
```

This will install:
- express
- cors
- ws (WebSocket)
- node-ads (ADS protocol)
- puppeteer (PDF generation)
- mustache (templating)
- uuid

### 2. Install Frontend Dependencies

```bash
cd ../frontend
npm install
```

This will install:
- react
- react-router-dom
- @mantine/core (UI framework)
- recharts (charts)
- zustand (state management)
- react-hook-form

## Configuration

### Backend Configuration

```bash
cd backend
cp .env.example .env
```

Edit `.env` if needed (optional for testing without real PLC):
```env
PORT=3001
AMS_NET_ID=127.0.0.1.1.1
AMS_PORT=851
```

### Frontend Configuration

```bash
cd frontend
cp .env.example .env
```

The defaults should work for local development.

## Running the Application

### Terminal 1 - Backend

```bash
cd backend
npm start
```

You should see:
```
🚀 Server running on port 3001
📡 WebSocket available at ws://localhost:3001
```

### Terminal 2 - Frontend

```bash
cd frontend
npm start
```

The application will automatically open in your browser at `http://localhost:3000`

## Testing Without a Real PLC

The application will work without a TwinCAT PLC for testing the UI:

1. The backend will attempt ADS connection when you click "Connect"
2. If no PLC is available, you'll see connection errors but the UI will still work
3. You can still:
   - Create recipes
   - View the UI and components
   - Test form validation
   - See the application structure

## Testing With TwinCAT PLC Simulator

If you have TwinCAT 3 installed:

1. Import the PLC files from `/plc-simulator` directory
2. Build and run the PLC project
3. Use the application normally - full functionality will be available

## Next Steps

1. **Explore the Home Page**: View machine selector and status displays
2. **Create a Recipe**: Go to "New Recipe" and add ingredients
3. **View History**: Check the history page (will be empty initially)

## Common Issues

### Backend Won't Start

- Check if port 3001 is available
- Make sure all dependencies are installed: `npm install`

### Frontend Won't Start

- Check if port 3000 is available
- Clear node_modules and reinstall: `rm -rf node_modules && npm install`

### Puppeteer Installation Issues

Puppeteer downloads Chromium during installation. If it fails:

```bash
cd backend
npm install puppeteer --unsafe-perm=true
```

Or use an alternative:
```bash
npm install puppeteer-core
```

### WebSocket Connection Fails

- Make sure backend is running
- Check browser console for errors
- Verify WebSocket URL in frontend/.env

## Development Mode

For auto-reload during development:

**Backend:**
```bash
cd backend
npm run dev
```

**Frontend:**
```bash
cd frontend
npm start
```

This is the default for frontend, backend requires nodemon (already in devDependencies).

## Production Build

**Frontend:**
```bash
cd frontend
npm run build
```

This creates an optimized production build in the `build/` directory.

## Getting Help

- Check the main [README.md](README.md) for detailed documentation
- Review [ARCHITECTURE.md](ARCHITECTURE.md) for technical details
- Check browser console for frontend errors
- Check terminal output for backend errors

---

**Ready to go?** Run both backend and frontend, then open http://localhost:3000 🚀
