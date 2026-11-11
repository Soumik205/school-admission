import { ApiError, messageFor, networkErrorMessage } from './errors'
import type { CreateClassInput, Hold, SchoolClass } from './types'

declare global {
  interface Window {
    APP_CONFIG?: { apiBaseUrl?: string }
  }
}

const baseUrl = (
  window.APP_CONFIG?.apiBaseUrl ||
  import.meta.env.VITE_API_BASE_URL ||
  'http://localhost:8080'
).replace(/\/+$/, '')

interface RequestOptions {
  method?: string
  body?: unknown
  studentId?: string
}

interface ProblemBody {
  code?: string
  errors?: Record<string, string[]>
}

async function request<T>(path: string, { method = 'GET', body, studentId }: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' }
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }
  if (studentId) {
    headers['X-Student-Id'] = studentId
  }

  let response: Response
  try {
    response = await fetch(baseUrl + path, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError('NETWORK_ERROR', 0, networkErrorMessage)
  }

  if (!response.ok) {
    let problem: ProblemBody = {}
    try {
      problem = await response.json()
    } catch {
      // Not JSON (for example a proxy error page); fall back to the status code.
    }
    throw new ApiError(
      problem.code ?? 'HTTP_ERROR',
      response.status,
      messageFor(problem.code, response.status),
      problem.errors ?? {},
    )
  }

  if (response.status === 204) {
    return undefined as T
  }
  return response.json() as Promise<T>
}

export const api = {
  health: () => request<{ status: string }>('/health'),
  listClasses: () => request<SchoolClass[]>('/classes'),
  createClass: (input: CreateClassInput) => request<SchoolClass>('/classes', { method: 'POST', body: input }),
  applyForSeat: (classId: string, studentId: string) =>
    request<Hold>(`/classes/${encodeURIComponent(classId)}/holds`, { method: 'POST', studentId }),
  getHold: (holdId: string) => request<Hold>(`/holds/${encodeURIComponent(holdId)}`),
  confirmHold: (holdId: string, studentId: string) =>
    request<Hold>(`/holds/${encodeURIComponent(holdId)}/confirm`, { method: 'POST', studentId }),
  cancelHold: (holdId: string, studentId: string) =>
    request<void>(`/holds/${encodeURIComponent(holdId)}`, { method: 'DELETE', studentId }),
  myHolds: (studentId: string) => request<Hold[]>('/me/holds', { studentId }),
}
