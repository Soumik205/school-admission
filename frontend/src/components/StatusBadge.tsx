import type { HoldStatus } from '../api/types'

export function StatusBadge({ status }: { status: HoldStatus }) {
  return <span className={`status status-${status.toLowerCase()}`}>{status}</span>
}
