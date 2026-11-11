import { NavLink, Route, Routes } from 'react-router-dom'
import { StudentIdBar } from './components/StudentIdBar'
import { ClassesPage } from './pages/ClassesPage'

export default function App() {
  return (
    <div className="page">
      <header className="site-header">
        <h1>School Admission</h1>
        <nav className="site-nav">
          <NavLink to="/" end>
            Classes
          </NavLink>
          <NavLink to="/me">My applications</NavLink>
          <NavLink to="/admin/classes/new">Create class (admin)</NavLink>
        </nav>
      </header>
      <StudentIdBar />
      <main>
        <Routes>
          <Route path="/" element={<ClassesPage />} />
          <Route path="/me" element={<h2>My applications</h2>} />
          <Route path="/admin/classes/new" element={<h2>Create class</h2>} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>
    </div>
  )
}
