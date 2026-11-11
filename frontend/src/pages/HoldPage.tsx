import { useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { errorMessage } from '../api/errors'
import { Alert } from '../components/Alert'
import { StatusBadge } from '../components/StatusBadge'
import { displayStatus, formatCountdown, formatDateTime, formatTime, msLeft } from '../holdTime'
import { useNow } from '../hooks/useNow'
import { useStudentId } from '../hooks/useStudentId'

export function HoldPage() {
  const { holdId = '' } = useParams()
  const queryClient = useQueryClient()
  const now = useNow()
  const { studentId } = useStudentId()

  const hold = useQuery({
    queryKey: ['hold', holdId],
    queryFn: () => api.getHold(holdId),
    refetchInterval: 5000,
  })

  const status = hold.data ? displayStatus(hold.data, now) : null
  const expiredLocally = hold.data?.status === 'HELD' && status === 'EXPIRED'

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ['hold', holdId] })
    queryClient.invalidateQueries({ queryKey: ['classes'] })
    queryClient.invalidateQueries({ queryKey: ['me'] })
  }

  const pay = useMutation({
    mutationFn: () => api.confirmHold(holdId, studentId ?? ''),
    onSuccess: (confirmed) => {
      queryClient.setQueryData(['hold', holdId], confirmed)
      refresh()
    },
    onError: refresh,
  })

  const cancel = useMutation({
    mutationFn: () => api.cancelHold(holdId, studentId ?? ''),
    onSettled: refresh,
  })

  const busy = pay.isPending || cancel.isPending
  const actionError = pay.error ?? cancel.error

  useEffect(() => {
    if (expiredLocally) {
      queryClient.invalidateQueries({ queryKey: ['hold', holdId] })
      queryClient.invalidateQueries({ queryKey: ['classes'] })
    }
  }, [expiredLocally, holdId, queryClient])

  return (
    <section>
      <div className="section-heading">
        <h2>Application</h2>
        {hold.isError && hold.data && <span className="notice">
            Could not refresh. Showing data from {formatTime(hold.dataUpdatedAt)}.
          </span>}
      </div>

      {hold.isPending && <p className="muted">Loading application.</p>}
      {hold.isError && !hold.data && <Alert>{errorMessage(hold.error)}</Alert>}

      {hold.data && status && (
        <>
          <dl className="details">
            <dt>Class</dt>
            <dd>{hold.data.className}</dd>
            <dt>Birth registration number</dt>
            <dd className="mono">{hold.data.studentId}</dd>
            <dt>Status</dt>
            <dd>
              <StatusBadge status={status} />
            </dd>
            {status === 'HELD' && (
              <>
                <dt>Time left</dt>
                <dd>
                  <span className="countdown">{formatCountdown(msLeft(hold.data, now))}</span>
                  <span className="muted"> (pay before {formatDateTime(hold.data.expiresAt)})</span>
                </dd>
              </>
            )}
            {status === 'CONFIRMED' && hold.data.confirmedAt && (
              <>
                <dt>Confirmed at</dt>
                <dd>{formatDateTime(hold.data.confirmedAt)}</dd>
              </>
            )}
          </dl>

          {actionError && <Alert>{errorMessage(actionError)}</Alert>}

          {status === 'HELD' && (
            <div className="hold-actions">
              <button type="button" disabled={busy || !studentId} onClick={() => pay.mutate()}>
                {pay.isPending ? 'Paying' : 'Pay'}
              </button>
              <button
                type="button"
                className="button-secondary"
                disabled={busy || !studentId}
                onClick={() => cancel.mutate()}
              >
                {cancel.isPending ? 'Cancelling' : 'Cancel application'}
              </button>
              {!studentId && (
                <span className="muted">Enter your birth registration number above to pay or cancel.</span>
              )}
            </div>
          )}

          {status === 'CONFIRMED' && <p className="success">Payment received. Admission is confirmed.</p>}

          {status === 'CANCELLED' && (
            <p className="muted">This application was cancelled and the seat was released.</p>
          )}

          {status === 'EXPIRED' && (
            <p className="muted">
              The time to pay has passed and the seat was released. You can apply again from{' '}
              <Link to="/">Classes</Link> if seats are available.
            </p>
          )}
        </>
      )}

      <p>
        <Link to="/me">View all my applications</Link>
      </p>
    </section>
  )
}
