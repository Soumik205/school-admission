import { useState, type ReactNode } from 'react'
import { StudentIdContext } from '../hooks/useStudentId'

const storageKey = 'studentId'

function readStoredId(): string | null {
  try {
    return localStorage.getItem(storageKey)
  } catch {
    return null
  }
}

export function StudentIdProvider({ children }: { children: ReactNode }) {
  const [studentId, setState] = useState<string | null>(readStoredId)

  const setStudentId = (id: string | null) => {
    setState(id)
    try {
      if (id) {
        localStorage.setItem(storageKey, id)
      } else {
        localStorage.removeItem(storageKey)
      }
    } catch {
      // Storage can be blocked (private mode); the ID still works for this tab.
    }
  }

  return <StudentIdContext.Provider value={{ studentId, setStudentId }}>{children}</StudentIdContext.Provider>
}
