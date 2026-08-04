/** Resolve backend host from the page URL so phones/LAN work (not hardcoded localhost). */
function pageHost(): string {
  if (typeof window === 'undefined') return 'localhost';
  return window.location.hostname || 'localhost';
}

function pageProtocol(): string {
  if (typeof window === 'undefined') return 'http:';
  return window.location.protocol === 'https:' ? 'https:' : 'http:';
}

/** API base: VITE_API_BASE or http(s)://<page-host>:5223 */
export function getApiBase(): string {
  const fromEnv = import.meta.env.VITE_API_BASE?.trim();
  if (fromEnv) return fromEnv.replace(/\/$/, '');
  return `${pageProtocol()}//${pageHost()}:5223`;
}

/** MQTT WebSocket: VITE_MQTT_URL or ws(s)://<page-host>:5223/mqtt */
export function getMqttUrl(): string {
  const fromEnv = import.meta.env.VITE_MQTT_URL?.trim();
  if (fromEnv) return fromEnv;
  const ws = pageProtocol() === 'https:' ? 'wss:' : 'ws:';
  return `${ws}//${pageHost()}:5223/mqtt`;
}

export function getMqttStatusTopic(): string {
  return import.meta.env.VITE_MQTT_STATUS_TOPIC?.trim() || 'gateway/process/status';
}
