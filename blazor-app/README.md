# blazor-app

Blazor Server HMI (.NET 8) + JWT REST API + embedded MQTT broker + TwinCAT ADS.

## Run

```bash
cd blazor-app
dotnet watch run
```

Listens on **`http://0.0.0.0:5223`** (all interfaces).  
Local: http://localhost:5223 · LAN: `http://<PC-IP>:5223`

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

## Auth / login

One user store (`Data/auth.db` via LiteDB), two session modes.

```
Register / Sign in (Blazor header)
              │
              ▼
        AuthService  ──►  auth.db
        (hash passwords)     │
              │              │
              ├── HMI ──► AuthSessionService (in-memory Blazor circuit)
              │              lost on browser refresh
              │
              └── React ──► POST /api/auth/login
                              │
                              ▼
                         JwtTokenService → JWT
                              │
                              ▼
                    React localStorage + Bearer on /api/*
```

| Piece | Role |
|-------|------|
| `AuthService` | Register / Login; username ≥ 3 chars, password ≥ 6; `PasswordHasher`; unique username |
| `AuthSessionService` | HMI only — keeps user in memory for the SignalR circuit (no JWT) |
| `AuthController` | `POST /api/auth/login` — validates via `AuthService`, returns JWT |
| `JwtTokenService` | Signs token (HMAC, issuer/audience/secret from `Jwt` config, default 12 h) |

**Typical flow:** create the account once with **Register** in the Blazor header, then use the same username/password on the React portal. Protected API routes (`recipes`, `favorites`, `reports`, …) require `Authorization: Bearer <token>`.

## REST API

Base `http://<host>:5223` · CORS allows any host on ports **5173** / **3000** (LAN React).

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
