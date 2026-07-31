# react-portal

Read-only lab portal (React + Vite): recipes, favorites, PDF reports, live PackML.

PLC commands stay on the Blazor HMI only.

## Run

```bash
cd react-portal
npm install
npm run dev
```

→ http://localhost:5173  
Backend: Blazor at http://localhost:5223 (register a user in the HMI header first).

`.env`:

```env
VITE_API_BASE=http://localhost:5223
VITE_MQTT_URL=ws://localhost:5223/mqtt
VITE_MQTT_STATUS_TOPIC=gateway/process/status
```

Node: 20.19+ or 22.12+ (Vite 6).

## How it connects

| Source | Usage |
|--------|--------|
| REST + JWT | login, recipes, favorites, report list, PDF download |
| MQTT | live PackML status (`useProcessStatus`) |

Broker is embedded in Blazor (`ws://localhost:5223/mqtt`) — not a separate process.

```
React
  ├─ api.ts              → POST /api/auth/login, GET /api/recipes, …
  ├─ useProcessStatus    → MQTT subscribe gateway/process/status
  └─ downloadReport      → GET /api/reports/{id}/download → blob
```

## Structure

```
src/services/api.ts           REST + JWT
src/hooks/useApi.ts           Query recipes / favorites / reports
src/hooks/useProcessStatus.ts MQTT PackML
src/components/               LoginForm, PackMLStatusBadge
```
