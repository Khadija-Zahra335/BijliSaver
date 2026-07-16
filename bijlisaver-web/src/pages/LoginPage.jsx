import { useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth.jsx'
import { loginRequest, register } from '../api.js'

export default function LoginPage() {
  const [mode, setMode] = useState('login')        // 'login' | 'register'
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [busy, setBusy] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const goTo = location.state?.from ?? '/history'

  async function submit() {
    setError(null)
    if (mode === 'register' && name.trim().length < 2) { setError('Please enter your name.'); return }
    if (!email.includes('@')) { setError('Please enter a valid email address.'); return }
    if (password.length < 8) { setError('Password must be at least 8 characters.'); return }

    setBusy(true)
    try {
      const auth = mode === 'login'
        ? await loginRequest(email, password)
        : await register(name, email, password)
      login(auth)
      navigate(goTo)
    } catch (e) { setError(e.message) }
    finally { setBusy(false) }
  }

  const input =
    'mt-1 w-full rounded-btn border border-line bg-paper px-3.5 py-2.5 text-sm outline-none transition focus:border-brand'

  return (
    <div className="flex min-h-[calc(100vh-69px)] items-center justify-center px-5 py-8">
      <div className="w-full max-w-md">
        <h1 className="text-center font-serif text-2xl font-bold text-brand-dark">
          {mode === 'login' ? 'Welcome back' : 'Create your account'}
        </h1>
        <p className="mt-1.5 text-center text-sm text-ink-muted">
          {mode === 'login'
            ? 'Sign in to see your saved bills.'
            : 'Save your bills and track your usage month by month.'}
        </p>

        {/* mode switch */}
        <div className="mx-auto mt-5 flex w-fit rounded-btn border border-line bg-card p-1 text-sm font-semibold">
          {['login', 'register'].map(m => (
            <button
              key={m}
              onClick={() => { setMode(m); setError(null) }}
              className={`rounded-[7px] px-4 py-1.5 transition ${mode === m ? 'bg-brand text-white' : 'text-ink-muted'}`}
            >
              {m === 'login' ? 'Sign in' : 'Register'}
            </button>
          ))}
        </div>

        <div className="mt-5 rounded-panel border border-line bg-card p-5 shadow-panel">
          {mode === 'register' && (
            <label className="block text-sm font-semibold text-brand-dark">
              Your name
              <input className={input} value={name} onChange={e => setName(e.target.value)}
                     placeholder="Anees" autoComplete="name" />
            </label>
          )}
          <label className={`block text-sm font-semibold text-brand-dark ${mode === 'register' ? 'mt-3' : ''}`}>
            Email
            <input className={input} type="email" value={email} onChange={e => setEmail(e.target.value)}
                   placeholder="you@example.com" autoComplete="email" />
          </label>
          <label className="mt-3 block text-sm font-semibold text-brand-dark">
            Password
            <input className={input} type="password" value={password} onChange={e => setPassword(e.target.value)}
                   placeholder="At least 8 characters"
                   autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
                   onKeyDown={e => e.key === 'Enter' && submit()} />
          </label>

          {error && (
            <div className="mt-3 rounded-btn bg-danger-tint px-3.5 py-2.5 text-sm font-medium text-danger">{error}</div>
          )}

          <button
            onClick={submit}
            disabled={busy}
            className="mt-5 w-full rounded-btn bg-brand px-5 py-3 text-sm font-semibold text-white transition hover:bg-brand-dark hover:text-paper disabled:opacity-50"
          >
            {busy ? 'One moment…' : mode === 'login' ? 'Sign in' : 'Create account'}
          </button>

          <p className="mt-3 text-center text-xs text-ink-muted">
            You can still try BijliSaver without an account — only saving bill history needs one.
          </p>
        </div>
      </div>
    </div>
  )
}
