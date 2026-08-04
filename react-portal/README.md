# react-portal

Read-only lab portal (React + Vite): recipes, favorites, PDF reports, live PackML.

PLC commands stay on the Blazor HMI only.

## Run

```bash
cd react-portal
npm install
npm run dev
```

Listens on **`0.0.0.0:5173`**.  
Local: http://localhost:5173 · LAN: `http://<PC-IP>:5173`

Backend Blazor must be running on port **5223** (register a user in the HMI header first).

`.env` (optional overrides — leave API/MQTT unset for auto LAN host):

```env
# VITE_API_BASE=http://192.168.x.x:5223
# VITE_MQTT_URL=ws://192.168.x.x:5223/mqtt
VITE_MQTT_STATUS_TOPIC=gateway/process/status
```

By default, `config.ts` uses `window.location.hostname` + `:5223` for REST and MQTT.

Node: 20.19+ or 22.12+ (Vite 6).

## How it connects

| Source | Usage |
|--------|--------|
| REST + JWT | login, recipes, favorites, report list, PDF download |
| MQTT | live PackML status (`useProcessStatus`) |

Broker is embedded in Blazor (`ws://<host>:5223/mqtt`) — not a separate process.

```
React
  ├─ api.ts / config.ts  → POST /api/auth/login, GET /api/recipes, …
  ├─ useProcessStatus    → MQTT subscribe gateway/process/status
  └─ downloadReport      → GET /api/reports/{id}/download → blob
```

## Structure

```
src/config.ts                 API/MQTT host resolution (LAN-friendly)
src/services/api.ts           REST + JWT
src/hooks/useApi.ts           Query recipes / favorites / reports
src/hooks/useProcessStatus.ts MQTT PackML
src/components/               LoginForm, PackMLStatusBadge
```
