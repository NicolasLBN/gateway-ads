import { useState } from 'react';
import type { FormEvent } from 'react';
import { api, setToken } from '../services/api';
import './LoginForm.css';

type Props = { onLoggedIn: (username: string) => void };

export function LoginForm({ onLoggedIn }: Props) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setBusy(true);
    setError(null);
    try {
      const res = await api.login(username, password);
      setToken(res.token);
      onLoggedIn(res.username);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed');
    } finally {
      setBusy(false);
    }
  }

  return (
    <form className="login-card" onSubmit={onSubmit}>
      <h1>Lab portal</h1>
      <p className="lead">Connexion API REST — consultation recipes / reports (hors HMI machine).</p>
      <label>
        Username
        <input value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="username" required />
      </label>
      <label>
        Password
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="current-password"
          required
        />
      </label>
      {error ? <p className="error">{error}</p> : null}
      <button type="submit" disabled={busy}>{busy ? '…' : 'Sign in'}</button>
    </form>
  );
}
