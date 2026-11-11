import type { Hold, HoldStatus } from './api/types'

export function msLeft(hold: Hold, now: number): number {
  return Math.max(0, new Date(hold.expiresAt).getTime() - now)
}

// The server is the authority, but a hold whose time has run out is shown as expired right away
// instead of waiting for the next poll.
export function displayStatus(hold: Hold, now: number): HoldStatus {
  return hold.status === 'HELD' && msLeft(hold, now) === 0 ? 'EXPIRED' : hold.status
}

export function formatCountdown(ms: number): string {
  const totalSeconds = Math.ceil(ms / 1000)
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'medium' })
}

export function formatTime(ms: number): string {
  return new Date(ms).toLocaleTimeString(undefined, { timeStyle: 'medium' })
}
