import { useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LoginForm } from './components/LoginForm';
import { PackMLStatusBadge } from './components/PackMLStatusBadge';
import { useFavorites, useFormulations, useProcessStatus, useReports } from './hooks/useApi';
import { api, getToken, setToken } from './services/api';
import './App.css';

const queryClient = new QueryClient();

function Dashboard({ username, onLogout }: { username: string; onLogout: () => void }) {
  const status = useProcessStatus(true);
  const formulations = useFormulations(true);
  const favorites = useFavorites(true);
  const reports = useReports(true);
  const [downloadError, setDownloadError] = useState<string | null>(null);

  async function download(id: string, name: string) {
    setDownloadError(null);
    try {
      await api.downloadReport(id, `${name}.pdf`);
    } catch (err) {
      setDownloadError(err instanceof Error ? err.message : 'Download failed');
    }
  }

  return (
    <div className="portal">
      <header className="top">
        <div>
          <div className="brand">Gateway ADS · Lab portal</div>
          <div className="user">Signed in as <strong>{username}</strong></div>
        </div>
        <button className="ghost" type="button" onClick={onLogout}>Sign out</button>
      </header>

      <PackMLStatusBadge
        status={status.data}
        loading={status.isLoading}
        mqttConnected={status.isMqttConnected}
        error={status.error?.message ?? null}
      />

      <section className="grid">
        <article>
          <h2>Formulations</h2>
          {formulations.isLoading ? <p className="muted">Loading…</p> : null}
          {formulations.error ? <p className="error">{(formulations.error as Error).message}</p> : null}
          <ul>
            {(formulations.data ?? []).map((f) => (
              <li key={f.id}>
                <strong>{f.name}</strong>
                <span>{f.stepCount} steps · {f.ingredientCount} ingredients</span>
              </li>
            ))}
          </ul>
        </article>

        <article>
          <h2>Favorites</h2>
          {favorites.isLoading ? <p className="muted">Loading…</p> : null}
          <ul>
            {(favorites.data ?? []).map((f) => (
              <li key={f.id}>
                <strong>{f.recipe.name}</strong>
                <span>{new Date(f.savedAt).toLocaleString()}</span>
              </li>
            ))}
          </ul>
        </article>

        <article className="wide">
          <h2>Reports</h2>
          {downloadError ? <p className="error">{downloadError}</p> : null}
          {reports.isLoading ? <p className="muted">Loading…</p> : null}
          <div className="report-list">
            {(reports.data ?? []).map((r) => (
              <div className="report-row" key={r.id}>
                <div>
                  <strong>{r.recipeName}</strong>
                  <span>{new Date(r.date).toLocaleString()} · {r.machineName}</span>
                </div>
                <button type="button" onClick={() => download(r.id, r.recipeName)}>
                  Télécharger le rapport PDF
                </button>
              </div>
            ))}
            {(reports.data?.length ?? 0) === 0 && !reports.isLoading ? (
              <p className="muted">Aucun rapport pour le moment.</p>
            ) : null}
          </div>
        </article>
      </section>
    </div>
  );
}

function AppShell() {
  const [username, setUsername] = useState<string | null>(() => (getToken() ? 'operator' : null));

  if (!getToken() || !username) {
    return (
      <LoginForm
        onLoggedIn={(u) => {
          setUsername(u);
          queryClient.clear();
        }}
      />
    );
  }

  return (
    <Dashboard
      username={username}
      onLogout={() => {
        setToken(null);
        setUsername(null);
        queryClient.clear();
      }}
    />
  );
}

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AppShell />
    </QueryClientProvider>
  );
}
