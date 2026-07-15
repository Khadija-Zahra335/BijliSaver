// Auth state: token + user kept in localStorage, shared via context.
import { createContext, useContext, useState } from 'react'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const raw = localStorage.getItem('auth')
    return raw ? JSON.parse(raw) : null
  })

  function login(auth) {                       // { token, name, email }
    localStorage.setItem('auth', JSON.stringify(auth))
    setUser(auth)
  }
  function logout() {
    localStorage.removeItem('auth')
    setUser(null)
  }

  return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>
}

export const useAuth = () => useContext(AuthContext)

export function authHeader() {
  const raw = localStorage.getItem('auth')
  const token = raw ? JSON.parse(raw).token : null
  return token ? { Authorization: `Bearer ${token}` } : {}
}
