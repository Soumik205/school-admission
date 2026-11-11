import { Route, Routes } from 'react-router-dom'

export default function App() {
  return (
    <div className="page">
      <header className="site-header">
        <h1>School Admission</h1>
      </header>
      <main>
        <Routes>
          <Route path="*" element={<p>Pages are added in the next steps.</p>} />
        </Routes>
      </main>
    </div>
  )
}
