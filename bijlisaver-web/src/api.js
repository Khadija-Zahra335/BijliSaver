// One place for all backend calls. In dev, Vite proxies /api → localhost:5000.
// In production, VITE_API_BASE_URL points straight at the deployed API
// (Render's free tier has no private networking between services, so the
// frontend calls the API's public URL directly instead of a same-origin proxy).
import { authHeader } from './auth.jsx'

const API_BASE = import.meta.env.VITE_API_BASE_URL || ''

async function fail(res, fallback) {
  throw new Error((await res.text()) || fallback)
}

export async function uploadBill(file) {
  const form = new FormData()
  form.append('file', file)
  const res = await fetch(`${API_BASE}/api/bills/upload`, {
    method: 'POST', body: form, headers: { ...authHeader() },
  })
  if (!res.ok) await fail(res, `Upload failed (${res.status})`)
  return res.json()
}

export async function listBills() {
  const res = await fetch(`${API_BASE}/api/bills`, { headers: { ...authHeader() } })
  if (res.status === 401) throw new Error('LOGIN_REQUIRED')
  if (!res.ok) await fail(res, 'Could not load bills')
  return res.json()
}

export async function getBill(id) {
  const res = await fetch(`${API_BASE}/api/bills/${id}`, { headers: { ...authHeader() } })
  if (!res.ok) await fail(res, 'Could not load this bill')
  return res.json()
}

export async function deleteBill(id) {
  const res = await fetch(`${API_BASE}/api/bills/${id}`, {
    method: 'DELETE', headers: { ...authHeader() },
  })
  if (!res.ok) await fail(res, 'Could not delete this bill')
}

export async function register(name, email, password) {
  const res = await fetch(`${API_BASE}/api/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, email, password }),
  })
  if (!res.ok) await fail(res, 'Could not create your account')
  return res.json()
}

export async function loginRequest(email, password) {
  const res = await fetch(`${API_BASE}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  })
  if (!res.ok) await fail(res, 'Email or password is incorrect')
  return res.json()
}
