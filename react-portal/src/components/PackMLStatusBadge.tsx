import type { ProcessStatus } from '../services/api';
import './PackMLStatusBadge.css';

const tone = (state?: string) => {
  const s = (state ?? '').toLowerCase();
  if (s.includes('execute') || s.includes('complete')) return 'run';
  if (s.includes('start') || s.includes('hold') || s.includes('reset') || s.includes('idle')) return 'warn';
  if (s.includes('abort') || s.includes('stop') || s.includes('offline')) return 'alarm';
  return 'neutral';
};

type Props = {
  status?: ProcessStatus;
  loading?: boolean;
  mqttConnected?: boolean;
  error?: string | null;
};

export function PackMLStatusBadge({ status, loading, mqttConnected, error }: Props) {
  const label = loading
    ? 'MQTT…'
    : status
      ? `${status.state}${status.isHeld ? ' (Held)' : ''}`
      : error
        ? 'No data'
        : 'Unknown';

  return (
    <div className={`packml-badge tone-${tone(status?.state)}`}>
      <span className="dot" />
      <div>
        <div className="eyebrow">PackML · MQTT {mqttConnected ? 'live' : 'offline'}</div>
        <div className="value">{label}</div>
        {error ? <div className="sub">{error}</div> : null}
        {status?.currentStepName ? (
          <div className="sub">
            {status.currentStepName} · {Math.round((status.progress ?? 0) * 100)}%
          </div>
        ) : null}
      </div>
      <div className={`conn ${status?.connected ? 'on' : 'off'}`}>
        {status?.connected ? 'PLC linked' : 'PLC offline'}
      </div>
    </div>
  );
}
