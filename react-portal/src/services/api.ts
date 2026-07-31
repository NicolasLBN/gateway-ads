const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5223';

const TOKEN_KEY = 'gateway_ads_token';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string | null) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers = new Headers(options.headers);
  headers.set('Accept', 'application/json');
  if (options.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }
  const token = getToken();
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
  if (!res.ok) {
    let message = res.statusText;
    try {
      const err = await res.json();
      message = err.error ?? err.title ?? message;
    } catch {
      /* ignore */
    }
    throw new Error(message || `HTTP ${res.status}`);
  }

  if (res.status === 204) return undefined as T;
  const contentType = res.headers.get('content-type') ?? '';
  if (contentType.includes('application/json')) return res.json() as Promise<T>;
  return undefined as T;
}

export type LoginResponse = {
  token: string;
  username: string;
  expiresAtUtc: string;
};

export type Formulation = {
  id: string;
  name: string;
  stepCount: number;
  ingredientCount: number;
  steps: { type: string; details: string }[];
  savedAt: string;
};

export type Favorite = {
  id: string;
  savedAt: string;
  recipe: {
    name: string;
    processSteps: unknown[];
    ingredients: {
      name: string;
      amount: number;
      amountUnit: string;
      concentration: number;
      concentrationUnit: string;
    }[];
  };
};

export type ReportSummary = {
  id: string;
  recipeName: string;
  machineName: string;
  date: string;
  stepCount: number;
  hasPdf: boolean;
};

export type ProcessStatus = {
  connected: boolean;
  state: string;
  stateCode: number | null;
  stateName?: string;
  currentStepIndex: number;
  currentStepName?: string;
  totalSteps: number;
  progress: number;
  stepTimeRemaining: number;
  isHeld: boolean;
  processDone: boolean;
  recipeName?: string;
};

export const api = {
  login: (username: string, password: string) =>
    request<LoginResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    }),

  getFormulations: () => request<Formulation[]>('/api/formulations'),
  getFavorites: () => request<Favorite[]>('/api/favorites'),
  getReports: () => request<ReportSummary[]>('/api/reports'),

  async downloadReport(id: string, fallbackName = 'report.pdf') {
    const token = getToken();
    const res = await fetch(`${API_BASE}/api/reports/${id}/download`, {
      headers: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
    if (!res.ok) throw new Error('Download failed');
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fallbackName;
    a.click();
    URL.revokeObjectURL(url);
  },
};
