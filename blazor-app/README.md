# blazor-app

HMI Blazor Server (.NET 8) + API JWT + broker MQTT + ADS TwinCAT.

## Lancer

```bash
cd blazor-app
dotnet watch run
```

→ http://localhost:5223

Prérequis : .NET 8, TwinCAT Message Router, PLC `ReceipeManager` en Run (port **851**).

## Config (`appsettings.json`)

- **ADS** : `AmsNetId`, `AmsPort` (851)
- **Jwt** : secret / issuer pour l’API React
- **Mqtt** : topic `gateway/process/status`, WS `/mqtt`, TCP `1883`

## Parcours HMI

1. **Home** → Stop PackML  
2. **Recipe** → étapes Ajout / Mélange / Cuisson → Send recipe  
3. **Cooking** → Run / Hold / Stop → Generate PDF  
4. **Favorites** / **History**

## Persistence (`Data/`)

| Donnée | Fichier |
|--------|---------|
| Users | `auth.db` (LiteDB) |
| Favoris | `favorites.db` (LiteDB) |
| Historique | `report-history.json` |
| PDF | `wwwroot/reports/report_{id}.pdf` |

## Auth

- **HMI** : Register / Sign in dans le header (session circuit Blazor)
- **API / React** : même compte → `POST /api/auth/login` → JWT Bearer

## API REST

Base `http://localhost:5223` · CORS `localhost:5173`

| | | |
|--|--|--|
| `POST` | `/api/auth/login` | JWT |
| `GET` | `/api/recipes` | catalogue |
| `GET` | `/api/favorites` | favoris |
| `GET` | `/api/reports` | historique |
| `GET` | `/api/reports/{id}/download` | PDF |
| `GET` | `/api/process/status` | PackML (debug) |

## MQTT (dans ce process)

Pas de Mosquitto externe. Le broker tourne dans Blazor.

| | |
|--|--|
| WebSocket | `ws://localhost:5223/mqtt` |
| Topic | `gateway/process/status` (retain) |
| TCP | `localhost:1883` (outils) |

`ProcessStatusMqttPublisher` publie à chaque update `AppState`.

## ADS / PackML

Polling **500 ms**. Symboles : `GVL_Command`, `GVL_State`, `GVL_Recipe`, `GVL_Process`.  
États : Stopped → Idle → Execute → Complete (+ Hold).

## Dépannage

- Un seul `dotnet watch` à la fois  
- Footer HMI « connected » + état PackML  
- Port 1883 déjà pris → `"EnableTcp": false` dans `Mqtt`
