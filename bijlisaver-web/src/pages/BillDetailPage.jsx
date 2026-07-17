import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
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
  const navigate = useNavigate()
  const [bill, setBill] = useState(null)
  const [error, setError] = useState(null)
  const [urdu, setUrdu] = useState(false)

  useEffect(() => { getBill(id).then(setBill).catch(e => setError(e.message)) }, [id])

  function goUpload() {
    navigate('/')
    setTimeout(() => document.getElementById('upload-zone')?.scrollIntoView({ behavior: 'smooth', block: 'center' }), 50)
  }

  function shareOnWhatsApp() {
    const i = bill.insight
    const lines = [
      `⚡ BijliSaver — my ${bill.billingMonth} electricity bill, explained`,
      `Bill: ${rs(bill.currentBill)}${bill.unitsConsumed != null ? ` for ${bill.unitsConsumed} units` : ''}`,
    ]
    if (i?.taxPercentage != null)
      lines.push(`Taxes & extra charges: ${i.taxPercentage}% (${rs(i.taxTotal)})`)
    const cmp = bill.comparison
    if (cmp?.amountDelta != null)
      lines.push(cmp.amountDelta > 0
        ? `📈 Rs ${Math.abs(cmp.amountDelta).toLocaleString('en-PK')} more than ${cmp.previousMonth}`
        : `📉 Rs ${Math.abs(cmp.amountDelta).toLocaleString('en-PK')} less than ${cmp.previousMonth}`)
    const firstTip = i?.savingsTip?.split('\n').filter(Boolean)[0]
    if (firstTip) lines.push(`💰 ${firstTip}`)
    lines.push('', `Understand your own bill (free): ${window.location.origin}`)
    window.open(`https://wa.me/?text=${encodeURIComponent(lines.join('\n'))}`, '_blank', 'noopener')
  }

  if (error) return <p className="mx-auto max-w-6xl px-5 py-10 text-danger">{error}</p>
  if (!bill) return <p className="mx-auto max-w-6xl px-5 py-10 text-ink-muted">Loading your bill…</p>

  const i = bill.insight
  const cmp = bill.comparison
  const needsReview = bill.ocrStatus === 'needs_review'

  return (
    <div className="mx-auto w-full max-w-6xl px-5 py-7">
      {/* Header row */}
      <div className="flex items-baseline justify-between">
        <h1 className="font-serif text-2xl font-bold text-brand-dark">Bill for {bill.billingMonth}</h1>
        <Link to="/history" className="text-sm font-medium text-brand hover:underline">← All bills</Link>
      </div>

      {needsReview && (
        <div className="mt-3 rounded-card border-l-4 border-warn bg-warn-tint px-4 py-2.5 text-sm font-medium text-warn-ink">
          We could not read this bill clearly, so the numbers below may be incomplete.
          Please try uploading a clearer photo.
        </div>
      )}

      {/* Headline strip */}
      <div className="mt-4 grid gap-4 rounded-panel border border-line bg-card p-5 shadow-panel sm:grid-cols-3">
        <div>
          <div className="text-xs font-semibold uppercase tracking-wider text-ink-muted">This month's bill</div>
          <div className="mt-1 text-2xl font-extrabold text-brand-dark">{rs(bill.currentBill)}</div>
          {bill.unitsConsumed != null && (
            <div className="mt-0.5 text-sm text-ink-muted">{bill.unitsConsumed} units used</div>
          )}
        </div>
        <div>
          <div className="text-xs font-semibold uppercase tracking-wider text-ink-muted">Pay by {bill.dueDate ?? '—'}</div>
          <div className="mt-1 text-xl font-bold">{rs(bill.payableWithinDue)}</div>
          {bill.payableAfterDue != null && (
            <div className="mt-0.5 text-xs text-ink-muted">after due date: {rs(bill.payableAfterDue)}</div>
          )}
        </div>
        {i?.taxPercentage != null && (
          <div className="rounded-card bg-brand-tint p-3.5">
            <div className="text-xs font-semibold uppercase tracking-wider text-brand">Taxes & extra charges</div>
            <div className="mt-1 text-xl font-extrabold text-brand-dark">{i.taxPercentage}%</div>
            <div className="mt-0.5 text-xs text-brand-dark/80">{rs(i.taxTotal)} of this month's bill</div>
          </div>
        )}
      </div>

      {i?.arrearsNote && (
        <div className="mt-3 rounded-card border-l-4 border-warn bg-warn-tint px-4 py-2.5 text-sm font-medium text-warn-ink">
          {i.arrearsNote}
        </div>
      )}

      {/* ===== Two columns: charges left, insights right ===== */}
      <div className="mt-6 grid items-start gap-6 lg:grid-cols-[1.15fr_0.85fr]">
        {/* Charges */}
        <div>
          <div className="flex items-center justify-between gap-3">
            <h2 className="font-serif text-lg font-bold text-brand-dark">
              {urdu ? 'ہر چارج کی وضاحت' : 'Every charge, explained'}
            </h2>
            <div className="flex rounded-btn border border-line bg-card p-0.5 text-xs font-semibold">
              <button
                onClick={() => setUrdu(false)}
                className={`rounded-[6px] px-2.5 py-1 transition ${!urdu ? 'bg-brand text-white' : 'text-ink-muted'}`}
              >
                English
              </button>
              <button
                onClick={() => setUrdu(true)}
                className={`rounded-[6px] px-2.5 py-1 font-urdu transition ${urdu ? 'bg-brand text-white' : 'text-ink-muted'}`}
              >
                اردو
              </button>
            </div>
          </div>
          <div className="mt-3 rounded-panel border border-line bg-card p-5 shadow-panel">
            {bill.charges.map((c, idx) => {
              const style = CATEGORY_STYLE[c.category] ?? CATEGORY_STYLE.other
              const explanation = urdu ? (c.explanationUr ?? c.explanation) : c.explanation
              const isUrduText = urdu && c.explanationUr != null
              return (
                <div key={idx} className={idx > 0 ? 'mt-3 border-t border-line pt-3' : ''}>
                  <div className="flex items-baseline justify-between gap-3 text-sm">
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
                  {explanation && (
                    isUrduText ? (
                      <p dir="rtl" className="mt-1.5 font-urdu text-sm leading-loose text-ink-muted">{explanation}</p>
                    ) : (
                      <p className="mt-1 text-xs leading-relaxed text-ink-muted">{explanation}</p>
                    )
                  )}
                </div>
              )
            })}
          </div>
        </div>

        {/* Insights sidebar */}
        <div className="space-y-3 lg:sticky lg:top-24">
          {cmp && (cmp.amountDelta != null || cmp.unitsDelta != null) && (
            <div className="rounded-card border border-line bg-card p-4">
              <div className="text-xl">📊</div>
              <p className="mt-1.5 text-sm leading-relaxed">
                Compared to <b>{cmp.previousMonth}</b>
                {cmp.amountDelta != null && (
                  cmp.amountDelta > 0
                    ? <>, this bill is <b className="text-danger">Rs {Math.abs(cmp.amountDelta).toLocaleString('en-PK')} higher</b></>
                    : cmp.amountDelta < 0
                      ? <>, this bill is <b className="text-success">Rs {Math.abs(cmp.amountDelta).toLocaleString('en-PK')} lower</b></>
                      : <>, this bill is <b>the same amount</b></>
                )}
                {cmp.unitsDelta != null && (
                  <> ({cmp.unitsDelta > 0 ? '+' : ''}{cmp.unitsDelta} units)</>
                )}.
              </p>
              {cmp.topChanges?.length > 0 && (
                <div className="mt-2 space-y-1 border-t border-line pt-2 text-xs text-ink-muted">
                  <div className="font-semibold uppercase tracking-wider">Biggest changes</div>
                  {cmp.topChanges.map(d => (
                    <div key={d.displayName} className="flex items-baseline justify-between gap-2">
                      <span>{d.displayName}</span>
                      <span className={`whitespace-nowrap font-semibold ${d.delta > 0 ? 'text-danger' : 'text-success'}`}>
                        {d.delta > 0 ? '+' : '−'} Rs {Math.abs(d.delta).toLocaleString('en-PK')}
                      </span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
          {i?.applianceSummary && (
            <div className="rounded-card border border-line bg-card p-4">
              <div className="text-xl">🌀</div>
              <p className="mt-1.5 text-sm leading-relaxed">{i.applianceSummary}</p>
            </div>
          )}
          {i?.unitsToNextSlab != null && (
            <div className="rounded-card border border-line bg-card p-4">
              <div className="text-xl">⚠️</div>
              <p className="mt-1.5 text-sm leading-relaxed">
                You are <b>{i.unitsToNextSlab} units</b> away from the next price level
                {i.nextSlabPenalty != null && <> — crossing it costs about <b>{rs(i.nextSlabPenalty)}</b> more</>}.
              </p>
            </div>
          )}
          {i?.predictedNextAmt != null && (
            <div className="rounded-card border border-line bg-card p-4">
              <div className="text-xl">🔮</div>
              <p className="mt-1.5 text-sm leading-relaxed">
                Based on your usage history, next month's bill will be around <b>{rs(i.predictedNextAmt)}</b>.
              </p>
            </div>
          )}
          {i?.savingsTip && (
            <div className="rounded-panel bg-brand-dark p-5 text-night-text">
              <h3 className="font-serif text-base font-bold">💰 Your savings plan</h3>
              <div className="mt-1.5 space-y-1.5 text-sm leading-relaxed text-night-muted">
                {i.savingsTip.split('\n').filter(Boolean).map((line, idx) => <p key={idx}>{line}</p>)}
              </div>
            </div>
          )}
          <button
            onClick={shareOnWhatsApp}
            className="flex w-full items-center justify-center gap-2 rounded-btn bg-[#25D366] px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-[#1DA851]"
          >
            <svg viewBox="0 0 24 24" className="h-4 w-4 fill-current" aria-hidden="true">
              <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.297-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 0 1-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 0 1-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 0 1 2.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0 0 12.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 0 0 5.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 0 0-3.48-8.413Z" />
            </svg>
            Share on WhatsApp
          </button>
          <button onClick={goUpload} className="block w-full rounded-btn bg-brand px-5 py-2.5 text-center text-sm font-semibold text-white transition hover:bg-brand-dark">
            Upload Another Bill
          </button>
        </div>
      </div>
    </div>
  )
}
