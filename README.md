# Gateway ADS

Application industrielle de **gestion de formulations**, pilotage PLC TwinCAT via **ADS / PackML**, HMI Blazor, et portail labo React (API REST).

## Projets

| Dossier | Rôle |
|---------|------|
| [`blazor-app/`](./blazor-app/README.md) | HMI opérateur Blazor Server (.NET 8) + API REST JWT + ADS |
| [`react-portal/`](./react-portal/README.md) | Portail consultation React (formulations, favoris, rapports, PackML) |
| [`ReceipeManager/`](./ReceipeManager) | Projet TwinCAT ST (PackML + GVL Recipe/Process/Command/State) |

## Démarrage rapide

### 1. PLC (TwinCAT)
- Activer `ReceipeManager`, port ADS **851**, runtime **Run**
- Message Router TwinCAT démarré

### 2. HMI + API
```bash
cd blazor-app
dotnet watch run
```
→ `http://localhost:5223`

### 3. Portail React (optionnel)
```bash
cd react-portal
npm install
npm run dev
```
→ `http://localhost:5173`  
Créer un compte via le header Blazor, puis se connecter au portail.

## Architecture (vue d’ensemble)

```
Opérateur (atelier)          Labo / bureau / LIMS
        │                            │
        ▼                            ▼
  Blazor HMI  ◄── ADS ──►  TwinCAT PLC
        │
        └── REST / JWT ──►  React portal
```

- **Blazor** : saisie formulation, envoi PLC, Run/Hold/Stop, animations de procédé, favoris LiteDB, PDF.
- **React** : lecture seule (formulations, favoris, historique, statut PackML, download PDF).
- **PLC** : machine d’états PackML (Stopped → Idle → Execute → Complete).

## Documentation détaillée

- HMI / ADS / PackML / persistence → [`blazor-app/README.md`](./blazor-app/README.md)
- Client REST React → [`react-portal/README.md`](./react-portal/README.md)

## Licence

MIT
