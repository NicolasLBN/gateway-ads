# Blazor Gateway ADS — HMI & API REST

HMI opérateur **Blazor Server (.NET 8)** pour formulations pharmaceutiques / cosmétiques / parfumerie, communication **TwinCAT ADS**, contrôle **PackML**, persistence **LiteDB**, et **API REST JWT** consommée par le portail React.

## Fonctionnalités

- **Formulations par étapes** : `Ajout` → `Mélange` → `Cuisson` (règles métier UI)
- **Inputs touch-friendly** : steppers `+` / `−` (44–48 px) + unités
- **PackML** : Stopped / Idle / Execute / Complete (+ Hold)
- **Cooking** : live step progress, animations (gouttes / vortex / chaleur)
- **Favoris** : LiteDB + seed pro (sérum HA, PBS 10x, accord parfum)
- **Auth HMI** : inscription / login (hash mots de passe, LiteDB)
- **Rapports** : PDF QuestPDF + JSON locaux (`wwwroot/reports`, `exports/`)
- **API REST** : pour clients externes (React lab portal)

## Structure

```
blazor-app/
  Components/
    Layout/          MainLayout, AuthHeaderPanel, PlcStatusFooter
    Pages/           Home, NewRecipe, Cooking, Favorites, History, MachineSettings
    Shared/          NumberStepper, StepProcessAnimation, RealtimeChart, SaveToast…
  Controllers/       Auth, Formulations, Reports, Process (REST)
  Services/          AdsService, AppState, Favorites, Auth, Jwt, Pdf, Reports, Polling
  Models/            Recipe / RecipeStep, PackML, ProcessStatus, Report, AuthUser
  Data/              auth.db, favorites.db (gitignored)
  exports/           PDF/JSON locaux (gitignored)
  wwwroot/reports/   PDF servis au navigateur
```

## Prérequis

- .NET 8 SDK
- TwinCAT 3 + Message Router (Windows)
- Projet PLC `ReceipeManager` en Run (port **851**)

## Configuration ADS

`appsettings.json` / `appsettings.Development.json` :

```json
{
  "ADS": {
    "AmsNetId": "199.4.42.250.1.1",
    "AmsPort": 851,
    "MachineName": "ReceipeManager (local)",
    "AutoConnect": true,
    "AutoReconnect": true,
    "ReconnectIntervalMs": 5000,
    "TimeoutMs": 10000
  },
  "Jwt": {
    "Secret": "GatewayAdsDevSecretKey_ChangeMe_32chars!",
    "Issuer": "gateway-ads",
    "Audience": "gateway-ads-clients",
    "ExpiresHours": 12
  }
}
```

Connexion ADS au démarrage via `PreBuildAdsAsync` (style ThermalWinch).

## Lancer l’HMI

```bash
cd blazor-app
dotnet restore
dotnet watch run
```

URL typique : **http://localhost:5223**

## Parcours opérateur

1. **Home** — navigation (pas de tuile Cooking) ; envoi auto **Stop** PackML à l’entrée
2. **Formulation** (`/new-recipe`) — nom + Add Step ; force **Idle** à l’entrée
   - 1ʳᵉ étape = **Ajout** (obligatoire)
   - **Mélange** si ≥ 2 ingrédients déjà ajoutés (checkboxes)
   - **Cuisson** : °C + durée
3. **Send recipe** → Reset auto si besoin → `/cooking`
4. **Cooking** — Run / Hold / Stop, animations selon l’étape, Reset / Clear / PDF
5. **Favorites** — load & send vers Cooking
6. Header : logo → Home ; droite = Sign in / Register

## Modèle formulation

```csharp
Recipe
  Name
  ProcessSteps[] : RecipeStep
    Type = Ajout | Melange | Cuisson
    // Ajout: Ingredients (Name, Amount, AmountUnit, Concentration, ConcentrationUnit)
    // Melange: SelectedIngredientNames, MixDurationMinutes, MixSpeedRpm
    // Cuisson: TargetTemperatureC, CookDurationMinutes
```

Helpers PLC : `Steps` (noms) et `Ingredients` (aplatis depuis les Ajouts).

## PackML & symboles ADS

### Commandes (`GVL_Command`)
- `bStart`, `bStop`, `bReset`, `bClear`, `bHold`, `bAdsConnected`

### État (`GVL_State`)
- `nState`, `sStateName`, `bHeld`

### Recette (`GVL_Recipe`)
- `sRecipeName`, `nNumSteps`, `aStepNames[]`
- `nNumIngredients`, `aIngredientName[]`, `aIngredientQuantity[]`, `aIngredientVolume[]`, `aIngredientMolarMass[]`
- `fPreparationVolume`, `fPreparationConcentration`

### Process (`GVL_Process`)
- `nCurrentStepIndex`, `sCurrentStepName`, `nTotalSteps`
- `nStepTimeElapsed_s`, `nStepTimeRemaining_s`, `fProgress`
- `bProcessDone`, `nErrorCode`, `sErrorText`

Polling : `PlcPollingService` toutes les **500 ms**.

États utilisés : Clearing(0), Stopped(1), Resetting(2), Idle(3), Starting(4), Execute(5), Completing(6), Complete(7).

> **Note :** Home envoie Stop à l’arrivée ; Recipe details force Idle. Ne pas naviguer vers Home pendant un Execute si vous ne voulez pas interrompre le cycle.

## Persistence

| Donnée | Stockage |
|--------|----------|
| Utilisateurs | `Data/auth.db` (LiteDB, PasswordHasher) |
| Favoris | `Data/favorites.db` (LiteDB, seed 3 formulations pro) |
| Historique rapports | `Data/report-history.json` |
| PDF web | `wwwroot/reports/report_{id}.pdf` |
| Exports locaux | `exports/formulation_*.pdf` + `.json` |

## Service d’authentification

L’auth repose sur **trois briques** qui partagent la même base utilisateurs LiteDB, mais deux modes de session différents (HMI vs API).

```
┌─────────────────┐     Register / Login      ┌──────────────────┐
│ AuthHeaderPanel │ ───────────────────────► │ AuthService      │
│ (HMI Blazor)    │ ◄── AuthSessionService   │ (LiteDB auth.db) │
└─────────────────┘      (mémoire circuit)   │ PasswordHasher   │
                                              └────────┬─────────┘
┌─────────────────┐     POST /api/auth/login           │
│ React portal    │ ───────────────────────────────────┤
│ + JWT Bearer    │ ◄── JwtTokenService (token signé)  │
└─────────────────┘                                    │
```

### 1. `AuthService` (singleton) — source de vérité

Fichier : `Services/AuthService.cs`  
Stockage : `Data/auth.db`, collection `users` (index unique sur `Username`).

| Opération | Comportement |
|-----------|----------------|
| **Register** | Username normalisé (`trim` + minuscules), ≥ 3 car. ; mot de passe ≥ 6 car. ; rejet si username déjà pris ; hash via `PasswordHasher<AuthUser>` (ASP.NET Identity) ; insert LiteDB |
| **Login** | Recherche par username ; `VerifyHashedPassword` ; message générique si échec ; re-hash auto si `SuccessRehashNeeded` |

Le **mot de passe en clair n’est jamais stocké** — uniquement `PasswordHash`.

Modèle `AuthUser` : `Id` (ObjectId LiteDB), `Username`, `PasswordHash`, `CreatedAt`.

### 2. Session HMI — `AuthSessionService` (scoped)

Fichier : `Services/AuthSessionService.cs`  
UI : `Components/Layout/AuthHeaderPanel.razor` (header droite).

- **Pas de cookie / pas de JWT** pour le Blazor HMI : après Register ou Sign in, `SetUser` garde en mémoire (circuit Blazor) l’utilisateur **sans** le hash.
- `Sign out` → `Clear()`.
- Session **perdue au refresh navigateur** ou à la coupure du circuit SignalR (comportement HMI atelier, pas SSO).
- L’HMI n’est **pas encore gate** par `[Authorize]` sur les pages : l’auth header sert surtout à créer le compte partagé avec l’API / React.

Flux UI :
1. **Register** → `AuthService.Register` → `AuthSessionService.SetUser`
2. **Sign in** → `AuthService.Login` → `SetUser`
3. Affichage avatar + username + Sign out

### 3. API REST — JWT (`JwtTokenService` + Bearer)

Fichiers : `Services/JwtTokenService.cs`, `Controllers/AuthController.cs`, config `Jwt` dans `appsettings.json`.

1. Client React (ou autre) appelle `POST /api/auth/login` avec `{ "username", "password" }`.
2. `AuthController` réutilise **le même** `AuthService.Login`.
3. Si OK → `JwtTokenService.CreateToken` signe un JWT (HMAC-SHA256) avec claims `sub` (id), `unique_name` / `Name`.
4. Réponse : `{ token, username, expiresAtUtc }` (durée = `Jwt:ExpiresHours`, défaut 12 h).
5. Les endpoints protégés (`[Authorize]` : formulations, favorites, reports, process) exigent  
   `Authorization: Bearer <token>`.
6. Validation côté serveur : `AddAuthentication(JwtBearer)` dans `Program.cs` (Issuer, Audience, Secret).

Le React portal stocke le token dans `localStorage` (`gateway_ads_token`).

### Règles pratiques

| Action | Où |
|--------|-----|
| Créer un compte | Header Blazor → **Register** |
| Se connecter à l’HMI | Header → **Sign in** (session circuit) |
| Se connecter au portail React | Même username/password → JWT |
| Changer le secret JWT | `appsettings.json` → `Jwt:Secret` (min. 32 car. recommandés en prod) |

### Ce que ce n’est pas

- Pas de rôles / claims avancés (Admin, Operator) pour l’instant  
- Pas de refresh token  
- Pas de lien entre session Blazor et JWT (deux canaux indépendants sur la même base users)

## API REST (JWT)

Base : `http://localhost:5223`  
CORS : `http://localhost:5173`, `http://localhost:3000`

| Méthode | Endpoint | Auth | Description |
|---------|----------|------|-------------|
| `POST` | `/api/auth/login` | Non | `{ username, password }` → JWT |
| `GET` | `/api/formulations` | Oui | Catalogue (favoris) |
| `GET` | `/api/favorites` | Oui | Favoris détaillés |
| `GET` | `/api/reports` | Oui | Historique |
| `GET` | `/api/reports/{id}/download` | Oui | PDF (blob) |
| `GET` | `/api/process/status` | Oui | PackML + connexion (polling React 2 s) |

Créer un utilisateur via le header Blazor **Register**, puis login API / React.

Packages : `Beckhoff.TwinCAT.Ads` 6.2.x, `LiteDB`, `QuestPDF`, `Microsoft.AspNetCore.Authentication.JwtBearer`.

## Technologies

- .NET 8 / Blazor Server (InteractiveServer)
- TwinCAT ADS
- LiteDB, QuestPDF
- JWT Bearer

## Dépannage ADS

1. TwinCAT Runtime **Run**, port **851**
2. Message Router actif (pas d’erreur `ClientPortNotOpen`)
3. Vérifier `AmsNetId` dans appsettings
4. Un seul `dotnet run` / `watch` à la fois (verrou `BlazorApp.exe`)
5. Footer HMI : « HMI connected to PLC » + chips Stopped / Idle / Execute

## Client React

Voir [`../react-portal/README.md`](../react-portal/README.md).

## Licence

MIT
