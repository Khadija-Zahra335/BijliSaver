import { useEffect, useState } from 'react'
import { Routes, Route, Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from './auth.jsx'
import LoginPage from './pages/LoginPage.jsx'
import UploadPage from './pages/UploadPage.jsx'
import BillDetailPage from './pages/BillDetailPage.jsx'
import HistoryPage from './pages/HistoryPage.jsx'

// ====== EDIT THESE: your real links ======
const ME = {
  name: 'Khadija Zahra',
  role: 'Full-Stack Developer · Lahore, Pakistan',
  github: 'https://github.com/Khadija-Zahra335',
  linkedin: 'https://www.linkedin.com/in/khadija-zahra-06a37a270/',
  portfolio: 'https://khadijazahra-portfolio.vercel.app/',
  email: 'khadijazahra153@gmail.com',
}

const navClass = ({ isActive }) =>
  `text-sm font-medium transition ${isActive ? 'text-brand' : 'text-ink-muted hover:text-brand'}`

const signInClass = ({ isActive }) =>
  `rounded-btn border px-3.5 py-1.5 text-sm font-semibold transition ${
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
              <span className="flex items-center gap-2 text-sm">
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
              className="rounded-btn bg-brand px-3.5 py-2 text-sm font-semibold text-white transition hover:bg-brand-dark hover:text-paper"
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
        <div className="mx-auto flex w-full max-w-6xl flex-wrap justify-between gap-6 px-5 py-8">
          <div>
            <div className="flex items-center gap-2 text-base font-bold">
              <span className="grid h-7 w-7 place-items-center rounded-chip bg-warn text-sm text-[#3d2e02]">⚡</span>
              BijliSaver
            </div>
            <p className="mt-2 max-w-[36ch] text-sm leading-relaxed text-night-muted">
              Electricity bills in Pakistan are confusing. BijliSaver reads yours and
              explains every rupee in simple words.
            </p>
          </div>

          <div>
            <h3 className="text-xs font-semibold uppercase tracking-widest text-warn">Built by</h3>
            <p className="mt-2 text-base font-semibold">{ME.name}</p>
            <p className="text-sm text-night-muted">{ME.role}</p>
            <div className="mt-2 flex flex-wrap gap-2 text-sm">
              <a href={ME.github} target="_blank" rel="noreferrer" className="rounded-btn border border-night-muted/40 px-2.5 py-1 transition hover:border-warn hover:text-warn">GitHub</a>
              <a href={ME.linkedin} target="_blank" rel="noreferrer" className="rounded-btn border border-night-muted/40 px-2.5 py-1 transition hover:border-warn hover:text-warn">LinkedIn</a>
              <a href={ME.portfolio} target="_blank" rel="noreferrer" className="rounded-btn border border-night-muted/40 px-2.5 py-1 transition hover:border-warn hover:text-warn">Portfolio</a>
              <a href={`mailto:${ME.email}`} className="rounded-btn border border-night-muted/40 px-2.5 py-1 transition hover:border-warn hover:text-warn">Email</a>
            </div>
          </div>
        </div>
        <div className="border-t border-white/10">
          <div className="mx-auto flex w-full max-w-6xl flex-wrap items-center justify-between gap-2 px-5 py-3 text-xs text-night-muted">
            <span>© {new Date().getFullYear()} BijliSaver — made for everyday people · Made in Lahore 🇵🇰</span>
            <span>LESCO supported · more DISCOs coming</span>
          </div>
        </div>
      </footer>
    </div>
  )
}
