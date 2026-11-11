import { NavLink, Route, Routes } from 'react-router-dom'
import { StudentIdBar } from './components/StudentIdBar'
import { ClassesPage } from './pages/ClassesPage'
import { CreateClassPage } from './pages/CreateClassPage'
import { HoldPage } from './pages/HoldPage'
import { MyApplicationsPage } from './pages/MyApplicationsPage'

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
          <Route path="/me" element={<MyApplicationsPage />} />
          <Route path="/admin/classes/new" element={<CreateClassPage />} />
          <Route path="/holds/:holdId" element={<HoldPage />} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>
    </div>
  )
}
