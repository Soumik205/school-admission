const messagesByCode: Record<string, string> = {
  CLASS_FULL: 'No seats left in this class.',
  ALREADY_APPLIED: 'This student already has an active application or admission in this class.',
  HOLD_EXPIRED:
    'This application has expired and the seat was released. You can apply again if seats are available.',
  HOLD_CANCELLED: 'This application was cancelled.',
  HOLD_ALREADY_CONFIRMED: 'Admission is already confirmed for this application.',
  HOLD_NOT_OWNED: 'This application belongs to a different birth registration number.',
  INVALID_STUDENT_ID: 'Birth registration number must be exactly 17 digits.',
  CLASS_NAME_TAKEN: 'A class with this name already exists.',
  CLASS_NOT_FOUND: 'This class could not be found.',
  HOLD_NOT_FOUND: 'This application could not be found.',
  VALIDATION_FAILED: 'Some fields are not valid. Check the form and try again.',
}

export const networkErrorMessage = 'Cannot reach the server. Check that it is running and try again.'

export class ApiError extends Error {
  readonly code: string
  readonly status: number
  readonly fieldErrors: Record<string, string[]>

  constructor(code: string, status: number, message: string, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.name = 'ApiError'
    this.code = code
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

export function messageFor(code: string | undefined, status: number): string {
  if (code && messagesByCode[code]) {
    return messagesByCode[code]
  }
  if (status >= 500) {
    return 'Something went wrong on the server. Please try again.'
  }
  return `Unexpected error (HTTP ${status}).`
}

export function errorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.message
  }
  return 'Something went wrong. Please try again.'
}
