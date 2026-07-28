import type { ProcessStatus } from '../services/api';
import './PackMLStatusBadge.css';

const tone = (state?: string) => {
  const s = (state ?? '').toLowerCase();
  if (s.includes('execute') || s.includes('complete')) return 'run';
  if (s.includes('start') || s.includes('hold') || s.includes('reset') || s.includes('idle')) return 'warn';
  if (s.includes('abort') || s.includes('stop') || s.includes('offline')) return 'alarm';
  return 'neutral';
};

type Props = { status?: ProcessStatus; loading?: boolean };

export function PackMLStatusBadge({ status, loading }: Props) {
  const label = loading
    ? 'Polling…'
    : status
      ? `${status.state}${status.isHeld ? ' (Held)' : ''}`
      : 'Unknown';

  return (
    <div className={`packml-badge tone-${tone(status?.state)}`}>
      <span className="dot" />
      <div>
        <div className="eyebrow">PackML</div>
        <div className="value">{label}</div>
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
