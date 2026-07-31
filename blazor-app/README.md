# blazor-app

Blazor Server HMI (.NET 8) + JWT REST API + embedded MQTT broker + TwinCAT ADS.

## Run

```bash
cd blazor-app
dotnet watch run
```

→ http://localhost:5223

Requires: .NET 8, TwinCAT Message Router, PLC `ReceipeManager` in Run (port **851**).

## Config (`appsettings.json`)

- **ADS** — `AmsNetId`, `AmsPort` (851)
- **Jwt** — secret / issuer for the React API
- **Mqtt** — topic `gateway/process/status`, WebSocket `/mqtt`, TCP `1883`

## Operator flow

1. **Home** → PackML Stop  
2. **Recipe** → Add / Mix / Cook steps → Send recipe  
3. **Cooking** → Run / Hold / Stop → Generate PDF  
4. **Favorites** / **History**

## Persistence (`Data/`)

| Data | Storage |
|------|---------|
| Users | `auth.db` (LiteDB) |
| Favorites | `favorites.db` (LiteDB) |
| Report history | `report-history.json` |
| PDF files | `wwwroot/reports/report_{id}.pdf` |

## Auth

- **HMI** — Register / Sign in in the header (Blazor circuit session)
- **API / React** — same account → `POST /api/auth/login` → JWT Bearer

## REST API

Base `http://localhost:5223` · CORS `localhost:5173`

| Method | Endpoint | Purpose |
|--------|----------|---------|
| `POST` | `/api/auth/login` | JWT |
| `GET` | `/api/recipes` | recipe catalog |
| `GET` | `/api/favorites` | favorites |
| `GET` | `/api/reports` | report list |
| `GET` | `/api/reports/{id}/download` | PDF file |
| `GET` | `/api/process/status` | PackML (debug / fallback) |

## MQTT (in-process)

No external Mosquitto. The broker runs inside Blazor (`MQTTnet`).

| | |
|--|--|
| WebSocket | `ws://localhost:5223/mqtt` |
| Topic | `gateway/process/status` (retain) |
| TCP | `localhost:1883` (tools) |

`ProcessStatusMqttPublisher` publishes on every `AppState` update.

## ADS / PackML

Polling every **500 ms**. Symbols: `GVL_Command`, `GVL_State`, `GVL_Recipe`, `GVL_Process`.  
States: Stopped → Idle → Execute → Complete (+ Hold).

## Troubleshooting

- Only one `dotnet watch` at a time  
- HMI footer should show connected + PackML state  
- Port 1883 already in use → set `"EnableTcp": false` under `Mqtt`
