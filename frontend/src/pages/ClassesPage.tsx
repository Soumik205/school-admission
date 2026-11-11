import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { errorMessage } from '../api/errors'
import { Alert } from '../components/Alert'

export function ClassesPage() {
  const classes = useQuery({
    queryKey: ['classes'],
    queryFn: api.listClasses,
    refetchInterval: 3000,
  })

  return (
    <section>
      <div className="section-heading">
        <h2>Classes</h2>
        {classes.isError && classes.data && <span className="notice">Could not refresh. Retrying.</span>}
      </div>

      {classes.isPending && <p className="muted">Loading classes.</p>}

      {classes.isError && !classes.data && <Alert>{errorMessage(classes.error)}</Alert>}

      {classes.data && classes.data.length === 0 && (
        <p className="muted">
          No classes yet. An admin can add one under <Link to="/admin/classes/new">Create class (admin)</Link>.
        </p>
      )}

      {classes.data && classes.data.length > 0 && (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Class</th>
                <th className="num">Total</th>
                <th className="num">Held</th>
                <th className="num">Confirmed</th>
                <th className="num">Available</th>
              </tr>
            </thead>
            <tbody>
              {classes.data.map((schoolClass) => (
                <tr key={schoolClass.id}>
                  <td>{schoolClass.name}</td>
                  <td className="num">{schoolClass.totalSeats}</td>
                  <td className="num">{schoolClass.heldSeats}</td>
                  <td className="num">{schoolClass.confirmedSeats}</td>
                  <td className={schoolClass.availableSeats === 0 ? 'num full' : 'num'}>
                    {schoolClass.availableSeats === 0 ? 'Full' : schoolClass.availableSeats}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
