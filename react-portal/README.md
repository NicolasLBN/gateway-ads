# Gateway ADS — React lab portal (REST consumer)

## Run backend (Blazor + API)
```bash
cd blazor-app
dotnet watch run
```
API base: `http://localhost:5223`

## Run React portal
```bash
cd react-portal
npm install
npm run dev
```
UI: `http://localhost:5173`

## Auth
Register a user in the Blazor HMI header, then sign in here with the same credentials.
JWT is stored in `localStorage`.

## Endpoints
- `POST /api/auth/login`
- `GET /api/formulations`
- `GET /api/favorites`
- `GET /api/reports`
- `GET /api/reports/{id}/download`
- `GET /api/process/status` (polled every 2s)
