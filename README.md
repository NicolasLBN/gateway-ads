# Gateway ADS

Formulations industrielles + pilotage TwinCAT (ADS / PackML).

| Dossier | Rôle |
|---------|------|
| [`blazor-app/`](./blazor-app) | HMI + API REST + broker MQTT + ADS |
| [`react-portal/`](./react-portal) | Portail labo (lecture seule) |
| [`ReceipeManager/`](./ReceipeManager) | PLC TwinCAT |

```
Blazor HMI  ◄── ADS ──►  TwinCAT PLC
    │
    ├── REST / JWT ──► React (listes, PDF)
    └── MQTT (embarqué) ──► React (PackML live)
```

## Démarrage

1. TwinCAT `ReceipeManager` en **Run**, port ADS **851**
2. `cd blazor-app && dotnet watch run` → http://localhost:5223  
3. (optionnel) `cd react-portal && npm install && npm run dev` → http://localhost:5173  
   Compte : **Register** dans le header Blazor, puis login React.

Détails : [`blazor-app/README.md`](./blazor-app/README.md) · [`react-portal/README.md`](./react-portal/README.md)
