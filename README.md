# Gateway ADS

Industrial recipe management with TwinCAT ADS / PackML control.

| Folder | Role |
|--------|------|
| [`blazor-app/`](./blazor-app) | Operator HMI + REST API + embedded MQTT broker + ADS |
| [`react-portal/`](./react-portal) | Lab portal (read-only) |
| [`ReceipeManager/`](./ReceipeManager) | TwinCAT PLC project |

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         blazor-app (.NET 8)                             │
│                                                                         │
│  ┌──────────────┐    ADS poll     ┌──────────────┐                      │
│  │ Blazor HMI   │◄───────────────►│  AdsService  │◄──► TwinCAT PLC      │
│  │ (operator)   │                 │  AppState    │     (ReceipeManager) │
│  └──────┬───────┘                 └──────┬───────┘                      │
│         │                                │                              │
│         │ write                          │ StateChanged                 │
│         ▼                                ▼                              │
│  ┌──────────────┐               ┌─────────────────┐                     │
│  │ Persistence  │               │ MQTT publisher  │                     │
│  │              │               │ → inject msg    │                     │
│  │ LiteDB       │               └────────┬────────┘                     │
│  │  auth.db     │                        │                              │
│  │  favorites.db│                        ▼                              │
│  │ JSON         │               ┌─────────────────┐                     │
│  │  report-     │               │ MQTT broker     │                     │
│  │  history.json│               │ (MQTTnet)       │                     │
│  │ QuestPDF     │               │ ws://…/mqtt     │                     │
│  │  wwwroot/    │               │ tcp://…:1883    │                     │
│  │  reports/*.pdf│              └────────┬────────┘                     │
│  └──────────────┘                        │                              │
│                                          │ subscribe                    │
│  ┌──────────────┐                        │ topic:                       │
│  │ REST API     │                        │ gateway/process/status       │
│  │ JWT Bearer   │                        │                              │
│  └──────┬───────┘                        │                              │
└─────────┼────────────────────────────────┼──────────────────────────────┘
          │                                │
          │ HTTP                           │ MQTT over WebSocket
          │ /api/auth/login                │
          │ /api/recipes                   │
          │ /api/favorites                 │
          │ /api/reports                   │
          │ /api/reports/{id}/download     │
          ▼                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      react-portal (Vite + React)                        │
│                                                                         │
│  Login (JWT) ──► api.ts ──► REST calls (recipes, favorites, reports)    │
│                                                                         │
│  useProcessStatus ──► mqtt.js ──► subscribe PackML status (live)        │
│                                                                         │
│  Download PDF ──► GET /api/reports/{id}/download ──► blob save          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Data flow (short)

| Flow | Path |
|------|------|
| PLC → UI / MQTT | ADS → `AppState` → Blazor UI + MQTT publish (retain) |
| React PackML | MQTT subscribe `gateway/process/status` |
| React lists / PDF | REST + JWT → LiteDB / JSON / PDF files |
| Save favorite | Blazor HMI → `favorites.db` (LiteDB) |
| Generate report | Cooking → `report-history.json` + QuestPDF → `wwwroot/reports/` |

**Note:** React never commands the PLC. Only the Blazor HMI writes ADS commands.

## Quick start

1. TwinCAT `ReceipeManager` in **Run**, ADS port **851**
2. `cd blazor-app && dotnet watch run` → http://localhost:5223
3. (optional) `cd react-portal && npm install && npm run dev` → http://localhost:5173  
   Create a user via Blazor header **Register**, then sign in on React.

More detail: [`blazor-app/README.md`](./blazor-app/README.md) · [`react-portal/README.md`](./react-portal/README.md)
