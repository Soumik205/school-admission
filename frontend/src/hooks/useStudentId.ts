import { createContext, useContext } from 'react'

export interface StudentIdState {
  studentId: string | null
  setStudentId: (id: string | null) => void
}

export const StudentIdContext = createContext<StudentIdState | null>(null)

export function useStudentId(): StudentIdState {
  const state = useContext(StudentIdContext)
  if (!state) {
    throw new Error('useStudentId must be used inside StudentIdProvider')
  }
  return state
}

export function isValidStudentId(value: string): boolean {
  return /^\d{17}$/.test(value)
}
