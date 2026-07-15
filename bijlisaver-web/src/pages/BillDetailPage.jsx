import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getBill } from '../api.js'

const rs = (n) =>
  n == null ? '—' : `Rs ${Number(n).toLocaleString('en-PK', { maximumFractionDigits: 0 })}`

const CATEGORY_STYLE = {
  energy:     { chip: 'bg-brand-tint text-brand-dark', label: 'Electricity' },
  tax:        { chip: 'bg-warn-tint text-warn-ink',    label: 'Tax' },
  surcharge:  { chip: 'bg-warn-tint text-warn-ink',    label: 'Surcharge' },
  fee:        { chip: 'bg-warn-tint text-warn-ink',    label: 'Fee' },
  adjustment: { chip: 'bg-line/60 text-ink-muted',     label: 'Adjustment' },
  penalty:    { chip: 'bg-danger-tint text-danger',    label: 'Penalty' },
  other:      { chip: 'bg-line/60 text-ink-muted',     label: 'Other' },
}

export default function BillDetailPage() {
  const { id } = useParams()
  const [bill, setBill] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => { getBill(id).then(setBill).catch(e => setError(e.message)) }, [id])

  if (error) return <p className="mx-auto max-w-6xl px-5 py-10 text-danger">{error}</p>
  if (!bill) return <p className="mx-auto max-w-6xl px-5 py-10 text-ink-muted">Loading your bill…</p>

  const i = bill.insight
  const needsReview = bill.ocrStatus === 'needs_review'

  return (
    <div className="mx-auto w-full max-w-6xl px-5 py-9">
      {/* Header row */}
      <div className="flex items-baseline justify-between">
        <h1 className="font-serif text-3xl font-bold text-brand-dark">Bill for {bill.billingMonth}</h1>
        <Link to="/history" className="text-base font-medium text-brand hover:underline">← All bills</Link>
      </div>

      {needsReview && (
        <div className="mt-4 rounded-card border-l-4 border-warn bg-warn-tint px-4 py-3 text-base font-medium text-warn-ink">
          We could not read this bill clearly, so the numbers below may be incomplete.
          Please try uploading a clearer photo.
        </div>
      )}

      {/* Headline strip */}
      <div className="mt-6 grid gap-4 rounded-panel border border-line bg-card p-6 shadow-panel sm:grid-cols-3">
        <div>
          <div className="text-sm font-semibold uppercase tracking-wider text-ink-muted">This month's bill</div>
          <div className="mt-1 text-3xl font-extrabold text-brand-dark">{rs(bill.currentBill)}</div>
          {bill.unitsConsumed != null && (
            <div className="mt-0.5 text-base text-ink-muted">{bill.unitsConsumed} units used</div>
          )}
        </div>
        <div>
          <div className="text-sm font-semibold uppercase tracking-wider text-ink-muted">Pay by {bill.dueDate ?? '—'}</div>
          <div className="mt-1 text-2xl font-bold">{rs(bill.payableWithinDue)}</div>
          {bill.payableAfterDue != null && (
            <div className="mt-0.5 text-sm text-ink-muted">after due date: {rs(bill.payableAfterDue)}</div>
          )}
        </div>
        {i?.taxPercentage != null && (
          <div className="rounded-card bg-brand-tint p-4">
            <div className="text-sm font-semibold uppercase tracking-wider text-brand">Taxes & extra charges</div>
            <div className="mt-1 text-2xl font-extrabold text-brand-dark">{i.taxPercentage}%</div>
            <div className="mt-0.5 text-sm text-brand-dark/80">{rs(i.taxTotal)} of this month's bill</div>
          </div>
        )}
      </div>

      {i?.arrearsNote && (
        <div className="mt-4 rounded-card border-l-4 border-warn bg-warn-tint px-4 py-3 text-base font-medium text-warn-ink">
          {i.arrearsNote}
        </div>
      )}

      {/* ===== Two columns: charges left, insights right ===== */}
      <div className="mt-8 grid items-start gap-8 lg:grid-cols-[1.15fr_0.85fr]">
        {/* Charges */}
        <div>
          <h2 className="font-serif text-xl font-bold text-brand-dark">Every charge, explained</h2>
          <div className="mt-3 rounded-panel border border-line bg-card p-6 shadow-panel">
            {bill.charges.map((c, idx) => {
              const style = CATEGORY_STYLE[c.category] ?? CATEGORY_STYLE.other
              return (
                <div key={idx} className={idx > 0 ? 'mt-4 border-t border-line pt-4' : ''}>
                  <div className="flex items-baseline justify-between gap-3">
                    <span className="font-medium">
                      {c.displayName}
                      <span className={`ml-2 rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide ${style.chip}`}>
                        {style.label}
                      </span>
                    </span>
                    <span className={`whitespace-nowrap font-semibold ${c.amount < 0 ? 'text-success' : ''}`}>
                      {c.amount < 0 ? `− Rs ${Math.abs(c.amount).toLocaleString('en-PK')}` : rs(c.amount)}
                    </span>
                  </div>
                  {c.explanation && (
                    <p className="mt-1 text-sm leading-relaxed text-ink-muted">{c.explanation}</p>
                  )}
                </div>
              )
            })}
          </div>
        </div>

        {/* Insights sidebar */}
        <div className="space-y-4 lg:sticky lg:top-24">
          {i?.applianceSummary && (
            <div className="rounded-card border border-line bg-card p-5">
              <div className="text-2xl">🌀</div>
              <p className="mt-2 text-base leading-relaxed">{i.applianceSummary}</p>
            </div>
          )}
          {i?.unitsToNextSlab != null && (
            <div className="rounded-card border border-line bg-card p-5">
              <div className="text-2xl">⚠️</div>
              <p className="mt-2 text-base leading-relaxed">
                You are <b>{i.unitsToNextSlab} units</b> away from the next price level
                {i.nextSlabPenalty != null && <> — crossing it costs about <b>{rs(i.nextSlabPenalty)}</b> more</>}.
              </p>
            </div>
          )}
          {i?.predictedNextAmt != null && (
            <div className="rounded-card border border-line bg-card p-5">
              <div className="text-2xl">🔮</div>
              <p className="mt-2 text-base leading-relaxed">
                Based on your usage history, next month's bill will be around <b>{rs(i.predictedNextAmt)}</b>.
              </p>
            </div>
          )}
          {i?.savingsTip && (
            <div className="rounded-panel bg-brand-dark p-6 text-night-text">
              <h3 className="font-serif text-lg font-bold">💰 Your savings plan</h3>
              <div className="mt-2 space-y-2 text-base leading-relaxed text-night-muted">
                {i.savingsTip.split('\n').filter(Boolean).map((line, idx) => <p key={idx}>{line}</p>)}
              </div>
            </div>
          )}
          <Link to="/" className="block rounded-btn bg-brand px-6 py-3 text-center font-semibold text-white transition hover:bg-brand-dark">
            Upload Another Bill
          </Link>
        </div>
      </div>
    </div>
  )
}
