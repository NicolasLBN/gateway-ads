# Architecture Documentation

## System Overview

This industrial application follows a three-tier architecture:

1. **Frontend (React)** - User interface
2. **Backend (Node.js)** - API server and business logic
3. **PLC (TwinCAT 3)** - Industrial control simulation

## Communication Flow

```
┌─────────────┐     HTTP/REST      ┌─────────────┐      ADS       ┌─────────────┐
│             │ ← ─ ─ ─ ─ ─ ─ ─ ─ → │             │ ← ─ ─ ─ ─ ─ → │             │
│   Frontend  │                     │   Backend   │                │     PLC     │
│   (React)   │ ← ─ ─ WebSocket ─ ─ │  (Node.js)  │                │ (TwinCAT 3) │
│             │                     │             │                │             │
└─────────────┘                     └─────────────┘                └─────────────┘
```

## Frontend Architecture

### Technology Stack
- React.js 18
- Mantine UI (v7) - Component library
- Zustand - State management
- Recharts - Data visualization
- React Hook Form - Form handling
- React Router - Navigation

### State Management (Zustand)

```javascript
Store State:
├── machines[]              - Available machines
├── selectedMachine         - Currently selected machine
├── isConnected            - PLC connection status
├── machineData            - Real-time machine sensors
│   ├── motorTemperature
│   ├── oilPressure
│   └── motorSpeed
├── processData            - Process state
│   ├── currentStep
│   ├── progress
│   └── stepProgress
├── currentRecipe          - Active recipe
├── processHistory[]       - Time series data for charts
└── recipeHistory[]        - Completed recipes
```

### Component Hierarchy

```
App
├── Header
│   └── Navigation
├── HomePage
│   ├── MachineSelector
│   ├── MachineStatus
│   └── ProcessStatus
├── NewRecipePage
│   ├── RecipeForm
│   ├── MachineStatus
│   ├── ProcessTimeline
│   └── RealtimeChart
└── HistoryPage
    └── HistoryTable
```

## Backend Architecture

### Technology Stack
- Node.js
- Express.js - Web framework
- node-ads - ADS protocol client
- Puppeteer - PDF generation
- WebSocket (ws) - Real-time communication
- Mustache - HTML templating

### API Structure

```
Backend Services:
├── Express HTTP Server
│   ├── /api/machines      - Machine management
│   ├── /api/status        - Connection & status
│   ├── /api/recipe        - Recipe operations
│   └── /api/history       - Report management
├── WebSocket Server
│   └── /ws/status         - Real-time updates
├── ADS Client
│   ├── Connection management
│   ├── Read operations
│   └── Write operations
└── PDF Generator
    ├── Template rendering
    └── PDF creation
```

### Data Flow

1. **Recipe Creation**
   ```
   Frontend Form → POST /api/recipe → ADS Client → PLC Variables
   ```

2. **Process Execution**
   ```
   Frontend → POST /api/recipe/run → ADS Write → PLC Start Command
   ```

3. **Real-time Monitoring**
   ```
   PLC Variables → ADS Read (1Hz) → WebSocket Push → Frontend Update
   ```

4. **Report Generation**
   ```
   Frontend → POST /api/history → PDF Generator → File System → Download
   ```

## PLC Architecture

### Program Structure

```
MAIN (Program)
├── FB_ProcessStateMachine
│   └── State machine logic (7 steps)
└── FB_MachineSimulation
    └── Sensor simulation logic

Global Variables:
├── GVL_Recipe          - Recipe data
├── GVL_Process         - Process state
├── GVL_Machine         - Sensor values
└── GVL_Command         - Control commands
```

### State Machine

```
States:
0: Idle
1: Preparation (5s)
2: Dosing A (7s)
3: Dosing B (7s)
4: Mixing (10s)
5: Verification (4s)
6: Finalizing (3s)
7: Done

Transitions:
- Start command triggers 0→1
- Timer completion triggers next step
- Error can trigger any→error
- Reset command triggers any→0
```

### Simulation Logic

The machine simulation provides realistic sensor values:

- **Temperature**: Increases during process (25°C → 35°C), cools when idle
- **Pressure**: Varies with motor speed (3.0 ± 0.5 bar)
- **Speed**: ~1500 RPM during active steps, 0 when idle
- **Anomalies**: 1% random chance per cycle

## Communication Protocols

### ADS (Automation Device Specification)

```
Protocol: TCP/IP based
Port: 851 (default PlcTask port)
Operations:
  - Read symbols by name
  - Write symbols by name
  - Subscribe to notifications (not used in this implementation)

Connection Parameters:
  - AMS Net ID: 127.0.0.1.1.1 (localhost)
  - AMS Port: 851
```

### WebSocket

```
Protocol: ws://
Endpoint: /ws/status
Message Format: JSON

Message Types:
  - status: Real-time data update
  - connected: Connection established
  - error: Error notification
  - ack: Command acknowledgment

Update Rate: 1 Hz (1 second interval)
```

### REST API

```
Format: JSON
Authentication: None (for development)
Error Handling: Standard HTTP status codes

Status Codes:
  200: Success
  400: Bad request
  404: Not found
  500: Server error
  503: Service unavailable (PLC not connected)
```

## Data Models

### Recipe Model
```javascript
{
  name: string,
  ingredients: [
    {
      name: string,
      quantity: number,    // grams
      volume: number,      // ml
      molarMass: number    // g/L
    }
  ]
}
```

### Machine Status Model
```javascript
{
  motorTemperature: number,  // °C
  oilPressure: number,       // bar
  motorSpeed: number,        // RPM
  tempWarning: boolean,
  pressureWarning: boolean,
  speedWarning: boolean
}
```

### Process Status Model
```javascript
{
  currentStep: number,       // 0-7
  stepName: string,
  progress: number,          // 0.0-1.0
  stepProgress: number,      // 0.0-1.0
  stepTime_s: number,
  totalTime_s: number,
  errorCode: number,
  errorText: string,
  processDone: boolean
}
```

### Report Model
```javascript
{
  id: string,
  recipeName: string,
  machineName: string,
  date: ISO8601 string,
  products: Ingredient[],
  steps: [
    {
      name: string,
      time: number,
      temp: number,
      pressure: number,
      speed: number,
      remark: string
    }
  ],
  pdfPath: string,
  pdfUrl: string
}
```

## Security Considerations

### Current Implementation (Development)
- No authentication/authorization
- No input validation/sanitization
- Local connections only
- No encryption

### Production Recommendations
- Add user authentication (JWT)
- Implement role-based access control
- Add input validation and sanitization
- Use HTTPS/WSS for encryption
- Implement rate limiting
- Add audit logging
- Secure ADS connection with VPN
- Environment-based configuration

## Performance Considerations

### Frontend
- React memo for expensive components
- Virtualization for large lists
- Debouncing for real-time updates
- Lazy loading for routes

### Backend
- Connection pooling for ADS
- Caching for static data
- Compression for API responses
- Rate limiting for WebSocket

### PLC
- 10ms cycle time (100Hz)
- Optimized state machine logic
- Minimal calculations per cycle

## Deployment

### Development
```bash
# Backend
cd backend && npm run dev

# Frontend
cd frontend && npm start
```

### Production
```bash
# Backend
cd backend && npm start

# Frontend
cd frontend && npm run build
# Serve build folder with nginx or similar
```

### Docker (Future Enhancement)
```
docker-compose up
├── frontend (nginx)
├── backend (node)
└── database (optional)
```

## Scalability

### Current Limitations
- Single PLC connection
- In-memory data storage
- No load balancing

### Scaling Strategies
- Add database (PostgreSQL/MongoDB)
- Implement connection pooling
- Add Redis for caching
- Use message queue (RabbitMQ)
- Containerization with Kubernetes
- Multiple backend instances

## Monitoring & Logging

### Current Logging
- Console.log statements
- Error logging in backend

### Recommended Improvements
- Structured logging (Winston)
- Log aggregation (ELK stack)
- Application monitoring (PM2)
- Error tracking (Sentry)
- Performance monitoring (New Relic)
- Health check endpoints

## Testing Strategy

### Frontend Testing
- Unit tests: Jest + React Testing Library
- Component tests: Storybook
- E2E tests: Playwright/Cypress

### Backend Testing
- Unit tests: Jest
- Integration tests: Supertest
- API tests: Postman/Newman

### PLC Testing
- Unit tests: TcUnit
- Simulation testing: Virtual PLC
- Integration testing with real hardware

## Maintenance

### Version Control
- Git flow branching strategy
- Semantic versioning
- Changelog maintenance

### Documentation
- Code comments
- API documentation (Swagger)
- Architecture diagrams
- Deployment guides

### Dependencies
- Regular updates
- Security audits
- License compliance
