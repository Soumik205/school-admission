import { useState, type FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { ApiError, errorMessage } from '../api/errors'
import type { SchoolClass } from '../api/types'
import { Alert } from '../components/Alert'

interface FieldErrors {
  name?: string
  totalSeats?: string
}

function validate(name: string, totalSeats: string): FieldErrors {
  const errors: FieldErrors = {}
  const trimmed = name.trim()
  if (!trimmed) {
    errors.name = 'Class name is required.'
  } else if (trimmed.length > 100) {
    errors.name = 'Class name must be at most 100 characters.'
  }
  const seats = Number(totalSeats)
  if (!totalSeats.trim()) {
    errors.totalSeats = 'Total seats is required.'
  } else if (!Number.isInteger(seats) || seats < 1 || seats > 10000) {
    errors.totalSeats = 'Total seats must be a whole number from 1 to 10000.'
  }
  return errors
}

export function CreateClassPage() {
  const queryClient = useQueryClient()
  const [name, setName] = useState('')
  const [totalSeats, setTotalSeats] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [created, setCreated] = useState<SchoolClass | null>(null)

  const createClass = useMutation({
    mutationFn: api.createClass,
    onSuccess: (schoolClass) => {
      setCreated(schoolClass)
      setName('')
      setTotalSeats('')
      queryClient.invalidateQueries({ queryKey: ['classes'] })
    },
    onError: (error) => {
      if (error instanceof ApiError && error.code === 'CLASS_NAME_TAKEN') {
        setFieldErrors({ name: error.message })
      } else if (error instanceof ApiError && error.code === 'VALIDATION_FAILED') {
        setFieldErrors({ name: error.fieldErrors.name?.[0], totalSeats: error.fieldErrors.totalSeats?.[0] })
      }
    },
  })

  const submit = (event: FormEvent) => {
    event.preventDefault()
    setCreated(null)
    createClass.reset()
    const errors = validate(name, totalSeats)
    setFieldErrors(errors)
    if (errors.name || errors.totalSeats) {
      return
    }
    createClass.mutate({ name: name.trim(), totalSeats: Number(totalSeats) })
  }

  const formError =
    createClass.isError &&
    !(createClass.error instanceof ApiError && ['CLASS_NAME_TAKEN', 'VALIDATION_FAILED'].includes(createClass.error.code))

  return (
    <section>
      <h2>Create class</h2>
      <p className="muted">Create each shift as its own class, for example "Class 6 - Morning Shift".</p>

      {created && (
        <div className="success" role="status">
          Class created: {created.name} ({created.totalSeats} seats). <Link to="/">Go to Classes</Link>
        </div>
      )}
      {formError && <Alert>{errorMessage(createClass.error)}</Alert>}

      <form className="form" onSubmit={submit} noValidate>
        <div className="field">
          <label htmlFor="class-name">Class name</label>
          <input
            id="class-name"
            value={name}
            maxLength={100}
            onChange={(event) => setName(event.target.value)}
            aria-invalid={fieldErrors.name ? true : undefined}
            aria-describedby={fieldErrors.name ? 'class-name-error' : undefined}
          />
          {fieldErrors.name && (
            <p id="class-name-error" className="field-error">
              {fieldErrors.name}
            </p>
          )}
        </div>

        <div className="field">
          <label htmlFor="total-seats">Total seats</label>
          <input
            id="total-seats"
            type="number"
            inputMode="numeric"
            min={1}
            max={10000}
            step={1}
            value={totalSeats}
            onChange={(event) => setTotalSeats(event.target.value)}
            aria-invalid={fieldErrors.totalSeats ? true : undefined}
            aria-describedby={fieldErrors.totalSeats ? 'total-seats-error' : undefined}
          />
          {fieldErrors.totalSeats && (
            <p id="total-seats-error" className="field-error">
              {fieldErrors.totalSeats}
            </p>
          )}
        </div>

        <div>
          <button type="submit" disabled={createClass.isPending}>
            {createClass.isPending ? 'Creating' : 'Create class'}
          </button>
        </div>
      </form>
    </section>
  )
}
