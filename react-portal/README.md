# react-portal

Portail labo React (lecture seule) : recipes, favoris, rapports PDF, PackML live.

La commande PLC reste uniquement sur l’HMI Blazor.

## Lancer

```bash
cd react-portal
npm install
npm run dev
```

→ http://localhost:5173  
Backend : Blazor sur http://localhost:5223 (Register un compte dans le header HMI).

`.env` :

```env
VITE_API_BASE=http://localhost:5223
VITE_MQTT_URL=ws://localhost:5223/mqtt
VITE_MQTT_STATUS_TOPIC=gateway/process/status
```

Node : 20.19+ ou 22.12+ (Vite 6).

## Données

| Source | Usage |
|--------|--------|
| REST + JWT | login, recipes, favoris, liste rapports, download PDF |
| MQTT | statut PackML (`useProcessStatus`) |

Broker = Blazor embarqué (`ws://localhost:5223/mqtt`), pas un service séparé.

## Structure

```
src/services/api.ts           REST + JWT
src/hooks/useApi.ts           Query recipes / favorites / reports
src/hooks/useProcessStatus.ts MQTT PackML
src/components/               LoginForm, PackMLStatusBadge
```
