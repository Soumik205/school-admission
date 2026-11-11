import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { errorMessage } from '../api/errors'
import { Alert } from '../components/Alert'
import { StatusBadge } from '../components/StatusBadge'
import { displayStatus, formatCountdown, formatDateTime, formatTime, msLeft } from '../holdTime'
import { useNow } from '../hooks/useNow'
import { useStudentId } from '../hooks/useStudentId'

export function MyApplicationsPage() {
  const { studentId } = useStudentId()
  const now = useNow()

  const holds = useQuery({
    queryKey: ['me', studentId],
    queryFn: () => api.myHolds(studentId!),
    enabled: !!studentId,
    refetchInterval: 5000,
  })

  return (
    <section>
      <div className="section-heading">
        <h2>My applications</h2>
        {holds.isError && holds.data && <span className="notice">
            Could not refresh. Showing data from {formatTime(holds.dataUpdatedAt)}.
          </span>}
      </div>

      {!studentId && <p className="hint">Enter a birth registration number above to see applications.</p>}

      {studentId && holds.isPending && <p className="muted">Loading applications.</p>}
      {holds.isError && !holds.data && <Alert>{errorMessage(holds.error)}</Alert>}

      {holds.data && holds.data.length === 0 && (
        <p className="muted">
          No applications yet. Apply from <Link to="/">Classes</Link>.
        </p>
      )}

      {holds.data && holds.data.length > 0 && (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Class</th>
                <th>Status</th>
                <th>Time left</th>
                <th>Applied at</th>
                <th aria-label="Actions" />
              </tr>
            </thead>
            <tbody>
              {holds.data.map((hold) => {
                const status = displayStatus(hold, now)
                return (
                  <tr key={hold.holdId}>
                    <td>{hold.className}</td>
                    <td>
                      <StatusBadge status={status} />
                    </td>
                    <td className="countdown">{status === 'HELD' ? formatCountdown(msLeft(hold, now)) : ''}</td>
                    <td>{formatDateTime(hold.createdAt)}</td>
                    <td className="actions">
                      <Link to={`/holds/${hold.holdId}`}>View</Link>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
