import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  ResponsiveContainer, BarChart, Bar, XAxis, YAxis, Tooltip, CartesianGrid,
} from 'recharts'
import { listBills, deleteBill } from '../api.js'

const rs = (n) =>
  n == null ? '—' : `Rs ${Number(n).toLocaleString('en-PK', { maximumFractionDigits: 0 })}`

export default function HistoryPage() {
  const [bills, setBills] = useState(null)
  const [error, setError] = useState(null)
  const [confirming, setConfirming] = useState(null)   // bill id pending confirmation
  const [deleting, setDeleting] = useState(null)
  const navigate = useNavigate()

  useEffect(() => {
    listBills().then(setBills).catch(e => {
      if (e.message === 'LOGIN_REQUIRED')
        navigate('/login', { state: { from: '/history' } })
      else setError(e.message)
    })
  }, [navigate])

  async function onDelete(id) {
    setDeleting(id)
    try {
      await deleteBill(id)
      setBills(bs => bs.filter(b => b.id !== id))
    } catch (e) { setError(e.message) }
    finally { setDeleting(null); setConfirming(null) }
  }

  if (error) return <p className="mx-auto max-w-6xl px-5 py-10 text-danger">{error}</p>
  if (!bills) return <p className="mx-auto max-w-6xl px-5 py-10 text-ink-muted">Loading your bills…</p>

  if (bills.length === 0) {
    return (
      <div className="mx-auto max-w-md px-5 py-20 text-center">
        <div className="text-5xl">🧾</div>
        <h1 className="mt-4 font-serif text-3xl font-bold text-brand-dark">No bills yet</h1>
        <p className="mt-2 text-base text-ink-muted">Upload your first bill and it will appear here with a full breakdown.</p>
        <Link to="/" className="mt-6 inline-block rounded-btn bg-brand px-6 py-3 text-base font-semibold text-white hover:bg-brand-dark">
          Upload a Bill
        </Link>
      </div>
    )
  }

  const chartData = [...bills]
    .sort((a, b) => a.billingMonth.localeCompare(b.billingMonth))
    .map(b => ({ month: b.billingMonth, units: b.unitsConsumed ?? 0 }))

  return (
    <div className="mx-auto w-full max-w-6xl px-5 py-10">
      <h1 className="font-serif text-3xl font-bold text-brand-dark">My Bills</h1>

      <div className="mt-7 grid items-start gap-8 lg:grid-cols-[1fr_1.1fr]">
        <div className="space-y-4">
          {bills.map(b => (
            <div key={b.id} className="rounded-card border border-line bg-card p-5 transition hover:border-brand hover:shadow-panel">
              <div className="flex items-center justify-between gap-3">
                <Link to={`/bills/${b.id}`} className="min-w-0 flex-1">
                  <div className="text-lg font-semibold text-brand-dark">{b.billingMonth}</div>
                  <div className="text-base text-ink-muted">
                    {b.unitsConsumed != null ? `${b.unitsConsumed} units` : 'units unknown'}
                    {b.ocrStatus === 'needs_review' && (
                      <span className="ml-2 rounded-full bg-warn-tint px-2 py-0.5 text-[10px] font-semibold uppercase text-warn-ink">
                        needs clearer photo
                      </span>
                    )}
                  </div>
                </Link>
                <div className="text-right">
                  <div className="text-lg font-bold">{rs(b.payableWithinDue ?? b.currentBill)}</div>
                  <Link to={`/bills/${b.id}`} className="text-base text-brand hover:underline">see breakdown →</Link>
                </div>
              </div>

              {/* Delete flow: explicit confirm, never one accidental click */}
              <div className="mt-3 border-t border-line pt-3">
                {confirming === b.id ? (
                  <div className="flex items-center justify-between gap-3 text-base">
                    <span className="text-ink-muted">Delete this bill and its breakdown permanently?</span>
                    <span className="flex gap-2">
                      <button
                        onClick={() => onDelete(b.id)}
                        disabled={deleting === b.id}
                        className="rounded-btn bg-danger px-3 py-1.5 font-semibold text-white disabled:opacity-50"
                      >
                        {deleting === b.id ? 'Deleting…' : 'Yes, delete'}
                      </button>
                      <button onClick={() => setConfirming(null)} className="rounded-btn border border-line px-3 py-1.5">
                        Keep it
                      </button>
                    </span>
                  </div>
                ) : (
                  <button onClick={() => setConfirming(b.id)} className="text-base text-ink-muted transition hover:text-danger">
                    🗑 Delete bill
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>

        {chartData.length > 1 && (
          <div className="rounded-panel border border-line bg-card p-6 shadow-panel lg:sticky lg:top-24">
            <h2 className="text-base font-semibold text-ink-muted">Units used per month</h2>
            <div className="mt-3 h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--line)" />
                  <XAxis dataKey="month" tick={{ fontSize: 13, fill: 'var(--ink-muted)' }} />
                  <YAxis tick={{ fontSize: 13, fill: 'var(--ink-muted)' }} />
                  <Tooltip contentStyle={{ background: 'var(--card)', border: '1px solid var(--line)', color: 'var(--ink)' }} />
                  <Bar dataKey="units" fill="var(--brand)" radius={[6, 6, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
