import { useEffect, useState } from 'react';
import mqtt, { type MqttClient } from 'mqtt';
import { getMqttStatusTopic, getMqttUrl } from '../config';
import type { ProcessStatus } from '../services/api';

export type ProcessStatusQuery = {
  data: ProcessStatus | undefined;
  isLoading: boolean;
  error: Error | null;
  isMqttConnected: boolean;
};

/**
 * Live PackML / process status via the Blazor embedded MQTT broker
 * (replaces HTTP polling of GET /api/process/status).
 */
export function useProcessStatus(enabled = true): ProcessStatusQuery {
  const [data, setData] = useState<ProcessStatus | undefined>();
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);
  const [isMqttConnected, setIsMqttConnected] = useState(false);

  useEffect(() => {
    if (!enabled) {
      setIsLoading(false);
      return;
    }

    setIsLoading(true);
    setError(null);

    const mqttUrl = getMqttUrl();
    const statusTopic = getMqttStatusTopic();

    let client: MqttClient;
    try {
      client = mqtt.connect(mqttUrl, {
        protocolVersion: 4,
        reconnectPeriod: 2000,
        connectTimeout: 10_000,
        clean: true,
      });
    } catch (err) {
      setError(err instanceof Error ? err : new Error(String(err)));
      setIsLoading(false);
      return;
    }

    const onConnect = () => {
      setIsMqttConnected(true);
      setError(null);
      client.subscribe(statusTopic, { qos: 1 }, (err) => {
        if (err) setError(err);
      });
    };

    const onReconnect = () => {
      setIsMqttConnected(false);
    };

    const onClose = () => {
      setIsMqttConnected(false);
    };

    const onError = (err: Error) => {
      setError(err);
      setIsMqttConnected(false);
    };

    const onMessage = (topic: string, payload: Buffer) => {
      if (topic !== statusTopic) return;
      try {
        const parsed = JSON.parse(payload.toString()) as ProcessStatus;
        setData(parsed);
        setIsLoading(false);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err : new Error('Invalid MQTT payload'));
      }
    };

    client.on('connect', onConnect);
    client.on('reconnect', onReconnect);
    client.on('close', onClose);
    client.on('error', onError);
    client.on('message', onMessage);

    return () => {
      client.off('connect', onConnect);
      client.off('reconnect', onReconnect);
      client.off('close', onClose);
      client.off('error', onError);
      client.off('message', onMessage);
      client.end(true);
    };
  }, [enabled]);

  return { data, isLoading, error, isMqttConnected };
}
