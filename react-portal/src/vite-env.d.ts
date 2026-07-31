/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE?: string;
  readonly VITE_MQTT_URL?: string;
  readonly VITE_MQTT_STATUS_TOPIC?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
