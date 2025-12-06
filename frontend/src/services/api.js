const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:3001/api';

export const api = {
  // Machines
  getMachines: async () => {
    const response = await fetch(`${API_URL}/machines`);
    return response.json();
  },

  getMachine: async (id) => {
    const response = await fetch(`${API_URL}/machines/${id}`);
    return response.json();
  },

  // Status
  connectToPLC: async (amsNetId, amsPort) => {
    const response = await fetch(`${API_URL}/status/connect`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ amsNetId, amsPort }),
    });
    return response.json();
  },

  getStatus: async () => {
    const response = await fetch(`${API_URL}/status`);
    return response.json();
  },

  disconnectFromPLC: async () => {
    const response = await fetch(`${API_URL}/status/disconnect`, {
      method: 'POST',
    });
    return response.json();
  },

  // Recipe
  sendRecipe: async (recipe) => {
    const response = await fetch(`${API_URL}/recipe`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(recipe),
    });
    return response.json();
  },

  runProcess: async () => {
    const response = await fetch(`${API_URL}/recipe/run`, {
      method: 'POST',
    });
    return response.json();
  },

  resetProcess: async () => {
    const response = await fetch(`${API_URL}/recipe/reset`, {
      method: 'POST',
    });
    return response.json();
  },

  // History
  getHistory: async () => {
    const response = await fetch(`${API_URL}/history`);
    return response.json();
  },

  getReport: async (id) => {
    const response = await fetch(`${API_URL}/history/${id}`);
    return response.json();
  },

  createReport: async (reportData) => {
    const response = await fetch(`${API_URL}/history`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(reportData),
    });
    return response.json();
  },

  downloadPDF: (id) => {
    window.open(`${API_URL}/history/${id}/pdf`, '_blank');
  },
};
