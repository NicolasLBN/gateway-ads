# API Documentation

Complete REST API and WebSocket documentation for the Industrial Application.

## Base URL

```
http://localhost:3001/api
```

## Authentication

Currently, no authentication is required (development mode).

---

## Machines Endpoints

### GET /machines

Get list of all available machines.

**Response:**
```json
{
  "success": true,
  "machines": [
    {
      "id": "machine-1",
      "name": "Mixing Unit A",
      "amsNetId": "127.0.0.1.1.1",
      "amsPort": 851,
      "location": "Production Line 1"
    }
  ]
}
```

### GET /machines/:id

Get details of a specific machine.

**Parameters:**
- `id` (path) - Machine ID

**Response:**
```json
{
  "success": true,
  "machine": {
    "id": "machine-1",
    "name": "Mixing Unit A",
    "amsNetId": "127.0.0.1.1.1",
    "amsPort": 851,
    "location": "Production Line 1"
  }
}
```

---

## Status Endpoints

### POST /status/connect

Connect to a PLC via ADS protocol.

**Request Body:**
```json
{
  "amsNetId": "127.0.0.1.1.1",
  "amsPort": 851
}
```

**Response (Success):**
```json
{
  "success": true
}
```

**Response (Error):**
```json
{
  "success": false,
  "error": "Connection failed: timeout"
}
```

### GET /status

Get current PLC connection status and real-time data.

**Response (Connected):**
```json
{
  "success": true,
  "connected": true,
  "machine": {
    "motorTemperature": 28.5,
    "oilPressure": 3.2,
    "motorSpeed": 1523.4,
    "tempWarning": false,
    "pressureWarning": false,
    "speedWarning": false
  },
  "process": {
    "currentStep": 2,
    "stepName": "Dosing Ingredient A",
    "progress": 0.25,
    "stepProgress": 0.5,
    "stepTime_s": 3,
    "totalTime_s": 12,
    "errorCode": 0,
    "errorText": "",
    "processDone": false
  },
  "timestamp": "2024-12-06T17:30:00.000Z"
}
```

**Response (Not Connected):**
```json
{
  "success": true,
  "connected": false,
  "message": "Not connected to PLC"
}
```

### POST /status/disconnect

Disconnect from the PLC.

**Response:**
```json
{
  "success": true,
  "message": "Disconnected from PLC"
}
```

---

## Recipe Endpoints

### POST /recipe

Send a recipe to the PLC.

**Request Body:**
```json
{
  "name": "Chocolate Mix Recipe",
  "ingredients": [
    {
      "name": "Cocoa Powder",
      "quantity": 250,
      "volume": 180,
      "molarMass": 12.5
    },
    {
      "name": "Sugar",
      "quantity": 500,
      "volume": 350,
      "molarMass": 8.3
    }
  ]
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Recipe sent to PLC",
  "recipe": {
    "name": "Chocolate Mix Recipe",
    "ingredients": [...]
  }
}
```

**Response (Not Connected):**
```json
{
  "success": false,
  "error": "Not connected to PLC. Please connect first."
}
```

### POST /recipe/run

Start the process execution.

**Request Body:** None

**Response:**
```json
{
  "success": true,
  "message": "Process started"
}
```

### POST /recipe/reset

Reset the process to idle state.

**Request Body:** None

**Response:**
```json
{
  "success": true,
  "message": "Process reset"
}
```

---

## History Endpoints

### GET /history

Get all recipe reports.

**Response:**
```json
{
  "success": true,
  "history": [
    {
      "id": "report-1701878400000",
      "recipeName": "Chocolate Mix Recipe",
      "machineName": "Mixing Unit A",
      "date": "2024-12-06T17:00:00.000Z",
      "products": [...],
      "steps": [...],
      "pdfPath": "/path/to/report.pdf",
      "pdfUrl": "/reports/report-1701878400000.pdf",
      "createdAt": "2024-12-06T17:00:00.000Z"
    }
  ]
}
```

### GET /history/:id

Get a specific report.

**Parameters:**
- `id` (path) - Report ID

**Response:**
```json
{
  "success": true,
  "report": {
    "id": "report-1701878400000",
    "recipeName": "Chocolate Mix Recipe",
    "machineName": "Mixing Unit A",
    "date": "2024-12-06T17:00:00.000Z",
    "products": [
      {
        "name": "Cocoa Powder",
        "quantity": 250,
        "volume": 180,
        "molarMass": 12.5
      }
    ],
    "steps": [
      {
        "name": "Preparation",
        "time": 5,
        "temp": 28.2,
        "pressure": 3.1,
        "speed": 0,
        "remark": "OK"
      }
    ],
    "pdfPath": "/path/to/report.pdf",
    "pdfUrl": "/reports/report-1701878400000.pdf"
  }
}
```

### POST /history

Create a new report (generate PDF).

**Request Body:**
```json
{
  "recipeName": "Chocolate Mix Recipe",
  "machineName": "Mixing Unit A",
  "products": [
    {
      "name": "Cocoa Powder",
      "quantity": 250,
      "volume": 180,
      "molarMass": 12.5
    }
  ],
  "steps": [
    {
      "name": "Preparation",
      "time": 5,
      "temp": 28.2,
      "pressure": 3.1,
      "speed": 0,
      "remark": "OK"
    },
    {
      "name": "Dosing Ingredient A",
      "time": 7,
      "temp": 30.5,
      "pressure": 3.3,
      "speed": 1500,
      "remark": "OK"
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "report": {
    "id": "report-1701878400000",
    "recipeName": "Chocolate Mix Recipe",
    "machineName": "Mixing Unit A",
    "date": "2024-12-06T17:00:00.000Z",
    "products": [...],
    "steps": [...],
    "pdfPath": "/path/to/report.pdf",
    "pdfUrl": "/reports/report-1701878400000.pdf",
    "createdAt": "2024-12-06T17:00:00.000Z"
  }
}
```

### GET /history/:id/pdf

Download the PDF report.

**Parameters:**
- `id` (path) - Report ID

**Response:**
- Content-Type: application/pdf
- File download

---

## WebSocket API

### Connection

```
ws://localhost:3001/ws/status
```

### Messages from Client

#### Start Updates
```json
{
  "command": "start_updates"
}
```

#### Stop Updates
```json
{
  "command": "stop_updates"
}
```

### Messages from Server

#### Connected
```json
{
  "type": "connected",
  "message": "Connected to WebSocket server",
  "timestamp": "2024-12-06T17:30:00.000Z"
}
```

#### Status Update
```json
{
  "type": "status",
  "connected": true,
  "machine": {
    "motorTemperature": 28.5,
    "oilPressure": 3.2,
    "motorSpeed": 1523.4,
    "tempWarning": false,
    "pressureWarning": false,
    "speedWarning": false
  },
  "process": {
    "currentStep": 2,
    "stepName": "Dosing Ingredient A",
    "progress": 0.25,
    "stepProgress": 0.5,
    "stepTime_s": 3,
    "totalTime_s": 12,
    "errorCode": 0,
    "errorText": "",
    "processDone": false
  },
  "timestamp": "2024-12-06T17:30:00.000Z"
}
```

#### Error
```json
{
  "type": "error",
  "error": "Failed to read PLC data",
  "timestamp": "2024-12-06T17:30:00.000Z"
}
```

#### Acknowledgment
```json
{
  "type": "ack",
  "message": "Started real-time updates"
}
```

---

## Error Codes

### HTTP Status Codes

- **200** - Success
- **400** - Bad Request (invalid input)
- **404** - Not Found
- **500** - Internal Server Error
- **503** - Service Unavailable (PLC not connected)

### Error Response Format

```json
{
  "success": false,
  "error": "Error message description"
}
```

---

## Rate Limiting

Currently no rate limiting is implemented. In production:
- API: 100 requests per minute per IP
- WebSocket: 1 message per second

---

## Data Types

### Ingredient
```typescript
{
  name: string,
  quantity: number,    // grams
  volume: number,      // milliliters
  molarMass: number    // g/L
}
```

### Machine Status
```typescript
{
  motorTemperature: number,  // °C
  oilPressure: number,       // bar
  motorSpeed: number,        // RPM
  tempWarning: boolean,
  pressureWarning: boolean,
  speedWarning: boolean
}
```

### Process Status
```typescript
{
  currentStep: number,       // 0-7
  stepName: string,
  progress: number,          // 0.0-1.0
  stepProgress: number,      // 0.0-1.0
  stepTime_s: number,        // seconds
  totalTime_s: number,       // seconds
  errorCode: number,         // 0 = no error
  errorText: string,
  processDone: boolean
}
```

### Process Step
```typescript
{
  name: string,
  time: number,        // seconds
  temp: number,        // °C
  pressure: number,    // bar
  speed: number,       // RPM
  remark: string       // "OK", "Warning", etc.
}
```

---

## Examples

### Complete Workflow Example

```bash
# 1. Get machines
curl http://localhost:3001/api/machines

# 2. Connect to PLC
curl -X POST http://localhost:3001/api/status/connect \
  -H "Content-Type: application/json" \
  -d '{"amsNetId":"127.0.0.1.1.1","amsPort":851}'

# 3. Check status
curl http://localhost:3001/api/status

# 4. Send recipe
curl -X POST http://localhost:3001/api/recipe \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Recipe",
    "ingredients": [
      {"name": "Ingredient A", "quantity": 100, "volume": 50, "molarMass": 10}
    ]
  }'

# 5. Start process
curl -X POST http://localhost:3001/api/recipe/run

# 6. Create report (after process completion)
curl -X POST http://localhost:3001/api/history \
  -H "Content-Type: application/json" \
  -d '{
    "recipeName": "Test Recipe",
    "machineName": "Mixing Unit A",
    "products": [...],
    "steps": [...]
  }'

# 7. Download PDF
curl http://localhost:3001/api/history/report-12345/pdf > report.pdf
```

---

## Support

For questions or issues with the API, please open an issue on GitHub.
