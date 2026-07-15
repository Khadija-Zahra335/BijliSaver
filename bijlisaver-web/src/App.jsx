import { useEffect, useState } from 'react'
import { Routes, Route, Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from './auth.jsx'
import LoginPage from './pages/LoginPage.jsx'
import UploadPage from './pages/UploadPage.jsx'
import BillDetailPage from './pages/BillDetailPage.jsx'
import HistoryPage from './pages/HistoryPage.jsx'

// ====== EDIT THESE: your real links ======
const ME = {
  name: 'Anees',
  role: 'Full-Stack Developer · Lahore, Pakistan',
  github: 'https://github.com/YOUR_USERNAME',
  linkedin: 'https://linkedin.com/in/YOUR_USERNAME',
  portfolio: 'https://YOUR_PORTFOLIO_SITE',
  email: 'you@example.com',
}

const navClass = ({ isActive }) =>
  `text-lg font-medium transition ${isActive ? 'text-brand' : 'text-ink-muted hover:text-brand'}`

const signInClass = ({ isActive }) =>
  `rounded-btn border px-4 py-2 text-base font-semibold transition ${
    isActive ? 'border-brand bg-brand text-white' : 'border-line text-brand-dark hover:border-brand hover:bg-brand-tint'
  }`

export default function App() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const [dark, setDark] = useState(() => {
    const saved = localStorage.getItem('theme')
    return saved ? saved === 'dark' : true   // default to dark mode
  })
  const [menuOpen, setMenuOpen] = useState(false)

  useEffect(() => {
    document.documentElement.classList.toggle('dark', dark)
    localStorage.setItem('theme', dark ? 'dark' : 'light')
  }, [dark])

  function goUpload() {
    setMenuOpen(false)
    navigate('/')
    setTimeout(() => document.getElementById('upload-zone')?.scrollIntoView({ behavior: 'smooth', block: 'center' }), 50)
  }

  return (
    <div className="flex min-h-screen flex-col">
      <div className="h-1 w-full bg-gradient-to-r from-brand via-warn to-brand" />

      <nav className="sticky top-0 z-10 border-b border-line bg-paper/95 backdrop-blur">
        <div className="mx-auto flex h-[68px] w-full max-w-6xl items-center justify-between px-5">
          <Link to="/" className="flex items-center gap-2.5 text-xl font-bold text-brand-dark" onClick={() => setMenuOpen(false)}>
            <span className="grid h-9 w-9 place-items-center rounded-chip bg-brand text-base text-white shadow-sm">⚡</span>
            BijliSaver
            <span className="mt-1 hidden text-[11px] font-semibold uppercase tracking-widest text-ink-muted md:block">
              bill samjho · paisa bachao
            </span>
          </Link>

          {/* Desktop nav — full row, md and up */}
          <div className="hidden items-center gap-6 md:flex">
            <NavLink to="/" end className={navClass}>Home</NavLink>
            <NavLink to="/history" className={navClass}>My Bills</NavLink>
            {user ? (
              <span className="flex items-center gap-2 text-base">
                <span className="font-semibold text-brand-dark">Hi, {user.name.split(' ')[0]}</span>
                <button onClick={() => { logout(); navigate('/') }} className="text-ink-muted transition hover:text-danger">
                  Sign out
                </button>
              </span>
            ) : (
              <NavLink to="/login" className={signInClass}>Sign in</NavLink>
            )}
            <button
              onClick={goUpload}
              className="rounded-btn bg-brand px-4 py-2.5 text-base font-semibold text-white transition hover:bg-brand-dark hover:text-paper"
            >
              Upload Your Bill
            </button>
            <button
              onClick={() => setDark(d => !d)}
              title={dark ? 'Switch to light mode' : 'Switch to dark mode'}
              className="grid h-9 w-9 place-items-center rounded-btn border border-line text-base transition hover:border-brand"
            >
              {dark ? '☀️' : '🌙'}
            </button>
          </div>

          {/* Mobile controls — dark toggle + hamburger, below md */}
          <div className="flex items-center gap-2 md:hidden">
            <button
              onClick={() => setDark(d => !d)}
              title={dark ? 'Switch to light mode' : 'Switch to dark mode'}
              className="grid h-9 w-9 place-items-center rounded-btn border border-line text-base transition hover:border-brand"
            >
              {dark ? '☀️' : '🌙'}
            </button>
            <button
              onClick={() => setMenuOpen(o => !o)}
              aria-label="Toggle menu"
              aria-expanded={menuOpen}
              className="grid h-9 w-9 place-items-center rounded-btn border border-line text-lg transition hover:border-brand"
            >
              {menuOpen ? '✕' : '☰'}
            </button>
          </div>
        </div>

        {/* Mobile menu panel */}
        {menuOpen && (
          <div className="border-t border-line bg-paper px-5 py-4 md:hidden">
            <div className="flex flex-col items-start gap-4">
              <NavLink to="/" end className={navClass} onClick={() => setMenuOpen(false)}>Home</NavLink>
              <NavLink to="/history" className={navClass} onClick={() => setMenuOpen(false)}>My Bills</NavLink>
              {user ? (
                <span className="flex items-center gap-2 text-base">
                  <span className="font-semibold text-brand-dark">Hi, {user.name.split(' ')[0]}</span>
                  <button onClick={() => { logout(); setMenuOpen(false); navigate('/') }} className="text-ink-muted transition hover:text-danger">
                    Sign out
                  </button>
                </span>
              ) : (
                <NavLink to="/login" className={signInClass} onClick={() => setMenuOpen(false)}>Sign in</NavLink>
              )}
              <button
                onClick={goUpload}
                className="w-full rounded-btn bg-brand px-4 py-2.5 text-base font-semibold text-white transition hover:bg-brand-dark hover:text-paper"
              >
                Upload Your Bill
              </button>
            </div>
          </div>
        )}
      </nav>

      <main className="w-full flex-1">
        <Routes>
          <Route path="/" element={<UploadPage />} />
          <Route path="/bills/:id" element={<BillDetailPage />} />
          <Route path="/history" element={<HistoryPage />} />
          <Route path="/login" element={<LoginPage />} />
        </Routes>
      </main>

      {/* Footer — always dark by design, independent of theme */}
      <footer className="bg-[#0E3B2E] text-night-text">
        <div className="mx-auto grid w-full max-w-6xl gap-10 px-5 py-12 md:grid-cols-3">
          <div>
            <div className="flex items-center gap-2 text-lg font-bold">
              <span className="grid h-8 w-8 place-items-center rounded-chip bg-warn text-sm text-[#3d2e02]">⚡</span>
              BijliSaver
            </div>
            <p className="mt-3 max-w-[36ch] text-base leading-relaxed text-night-muted">
              Electricity bills in Pakistan are confusing. BijliSaver reads yours and
              explains every rupee in simple words.
            </p>
          </div>

          <div>
            <h3 className="text-sm font-semibold uppercase tracking-widest text-warn">Under the hood</h3>
            <ul className="mt-3 space-y-2 text-base text-night-muted">
              <li>React + Tailwind frontend</li>
              <li>ASP.NET Core API · PostgreSQL</li>
              <li>FastAPI vision service (Gemini)</li>
              <li>Math-verified AI extraction — 4 arithmetic identity checks on every bill</li>
            </ul>
          </div>

          <div>
            <h3 className="text-sm font-semibold uppercase tracking-widest text-warn">Built by</h3>
            <p className="mt-3 text-lg font-semibold">{ME.name}</p>
            <p className="text-base text-night-muted">{ME.role}</p>
            <div className="mt-3 flex flex-wrap gap-3 text-base">
              <a href={ME.github} target="_blank" rel="noreferrer" className="rounded-btn border border-night-muted/40 px-3 py-1.5 transition hover:border-warn hover:text-warn">GitHub</a>
              <a href={ME.linkedin} target="_blank" rel="noreferrer" className="rounded-btn border border-night-muted/40 px-3 py-1.5 transition hover:border-warn hover:text-warn">LinkedIn</a>
              <a href={ME.portfolio} target="_blank" rel="noreferrer" className="rounded-btn border border-night-muted/40 px-3 py-1.5 transition hover:border-warn hover:text-warn">Portfolio</a>
              <a href={`mailto:${ME.email}`} className="rounded-btn border border-night-muted/40 px-3 py-1.5 transition hover:border-warn hover:text-warn">Email</a>
            </div>
          </div>
        </div>
        <div className="border-t border-white/10">
          <div className="mx-auto flex w-full max-w-6xl flex-wrap items-center justify-between gap-2 px-5 py-4 text-base text-night-muted">
            <span>© {new Date().getFullYear()} BijliSaver — made for everyday people · Made in Lahore 🇵🇰</span>
            <span>LESCO supported · more DISCOs coming</span>
          </div>
        </div>
      </footer>
    </div>
  )
}
