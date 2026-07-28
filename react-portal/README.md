# React Lab Portal — Gateway ADS

Client **React + TypeScript** (Vite) qui consomme l’API REST du backend Blazor.  
Usage typique : consultation labo / QA **hors HMI machine** (formulations, favoris, rapports, statut PackML en lecture seule).

L’HMI Blazor reste le seul point de **commande PLC** (Start / Stop / Send recipe).

## Prérequis

- Node.js 18+
- Backend Blazor lancé sur `http://localhost:5223`
- Un compte créé via **Register** dans le header Blazor

## Installation

```bash
cd react-portal
npm install
```

Fichier `.env` :

```env
VITE_API_BASE=http://localhost:5223
```

## Lancer

```bash
npm run dev
```

→ **http://localhost:5173**

## Fonctionnalités UI

- **Login JWT** (`localStorage`)
- **PackMLStatusBadge** — polling `/api/process/status` toutes les **2 s**
- Listes **Formulations** / **Favorites** / **Reports**
- Bouton **Télécharger le rapport PDF** (blob via `URL.createObjectURL`)

## Structure

```
src/
  services/api.ts          Fetch + JWT
  hooks/useApi.ts          TanStack Query (useFormulations, useFavorites, useReports, useProcessStatus)
  components/
    LoginForm.tsx
    PackMLStatusBadge.tsx
  App.tsx                  Shell + dashboard
```

## Endpoints utilisés

| Endpoint | Méthode | Usage |
|----------|---------|--------|
| `/api/auth/login` | `POST` | Connexion, stockage token |
| `/api/formulations` | `GET` | Liste formulations |
| `/api/favorites` | `GET` | Favoris détaillés |
| `/api/reports` | `GET` | Historique |
| `/api/reports/:id/download` | `GET` | PDF |
| `/api/process/status` | `GET` | Badge PackML |

Header : `Authorization: Bearer <token>` (sauf login).

## Stack

- React 19 + TypeScript + Vite
- TanStack Query (`@tanstack/react-query`)
- Fetch natif (pas d’Axios)

## Scripts

```bash
npm run dev      # développement
npm run build    # production
npm run preview  # preview build
```

## Dépannage

| Symptôme | Cause probable |
|----------|----------------|
| Login 500 / validation record | Corrigé côté API (`LoginRequest` class) — redémarrer Blazor |
| 401 Unauthorized | Token manquant / expiré — se reconnecter |
| CORS error | Backend doit autoriser `http://localhost:5173` |
| PackML « offline » | ADS non connecté côté Blazor ; footer HMI à vérifier |
| PDF 404 | Générer un rapport depuis Cooking (Complete → Generate PDF) |

Doc backend : [`../blazor-app/README.md`](../blazor-app/README.md)
