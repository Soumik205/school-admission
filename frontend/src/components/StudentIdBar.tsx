import { useState, type FormEvent } from 'react'
import { isValidStudentId, useStudentId } from '../hooks/useStudentId'

export function StudentIdBar() {
  const { studentId, setStudentId } = useStudentId()
  const [draft, setDraft] = useState('')
  const [error, setError] = useState<string | null>(null)

  const submit = (event: FormEvent) => {
    event.preventDefault()
    const value = draft.trim()
    if (!isValidStudentId(value)) {
      setError('Birth registration number must be exactly 17 digits.')
      return
    }
    setError(null)
    setStudentId(value)
  }

  if (studentId) {
    return (
      <div className="student-bar">
        <span className="student-bar-label">Birth registration number</span>
        <strong className="student-bar-value">{studentId}</strong>
        <button
          type="button"
          className="button-secondary"
          onClick={() => {
            setDraft('')
            setStudentId(null)
          }}
        >
          Change
        </button>
      </div>
    )
  }

  return (
    <form className="student-bar" onSubmit={submit} noValidate>
      <label className="student-bar-label" htmlFor="student-id">
        Birth registration number
      </label>
      <input
        id="student-id"
        inputMode="numeric"
        autoComplete="off"
        placeholder="17 digits"
        value={draft}
        onChange={(event) => setDraft(event.target.value)}
        aria-invalid={error ? true : undefined}
        aria-describedby={error ? 'student-id-error' : undefined}
      />
      <button type="submit">Use</button>
      {error && (
        <p id="student-id-error" className="field-error">
          {error}
        </p>
      )}
    </form>
  )
}
