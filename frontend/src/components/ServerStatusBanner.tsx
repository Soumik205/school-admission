import { useQuery } from '@tanstack/react-query'
import { api } from '../api/client'

export function ServerStatusBanner() {
  const health = useQuery({
    queryKey: ['health'],
    queryFn: api.health,
    refetchInterval: 5000,
    retry: false,
  })

  if (!health.isError) {
    return null
  }
  return (
    <div className="alert" role="alert">
      Cannot reach the server. Data shown may be out of date. Retrying every few seconds.
    </div>
  )
}
