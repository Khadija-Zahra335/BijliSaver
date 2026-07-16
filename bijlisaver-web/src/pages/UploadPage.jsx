import { useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { uploadBill } from '../api.js'

const STAGES = [
  'Reading your bill…',
  'Separating charges and taxes…',
  'Checking the math…',
  'Preparing your breakdown…',
]

// ---- The demo bill, now clearly framed as a SAMPLE REPORT ----
const DEMO_ROWS = [
  { label: 'Cost of Electricity', amount: 'Rs 5,890',
    note: 'The actual electricity you used — about 1 AC running 5 hours daily.' },
  { label: 'Fuel Price Adjustment', amount: 'Rs 685',
    note: 'Extra charge added when the fuel used to make electricity gets pricier.' },
  { label: 'Sales Tax (18%)', amount: 'Rs 1,180',
    note: 'Government tax on top of your electricity cost — like the tax on shopping.' },
  { label: 'Other Taxes & Fees', amount: 'Rs 785',
    note: 'Electricity duty, TV fee, and surcharges — each explained in your report.' },
]

function SampleReport() {
  const [active, setActive] = useState(null)
  return (
    <div className="relative">
      {/* Clear framing: this is an example, not a stuck UI element */}
      <div className="absolute -top-3 left-5 z-10 rounded-full bg-brand px-3 py-1 text-[11px] font-bold uppercase tracking-widest text-white shadow">
        Sample report
      </div>
      <div className="rounded-panel border-2 border-brand/25 bg-card p-4 pt-6 shadow-panel">
        <div className="flex items-baseline justify-between border-b border-dashed border-line pb-2">
          <b className="text-sm text-brand-dark">June Bill — what you'll get</b>
          <span className="text-sm text-ink-muted">312 units</span>
        </div>
        <div className="mt-2 space-y-0.5">
          {DEMO_ROWS.map((r, idx) => (
            <div
              key={r.label}
              onMouseEnter={() => setActive(idx)}
              onMouseLeave={() => setActive(null)}
              className={`cursor-default rounded-lg px-2 py-1.5 transition ${active === idx ? 'bg-brand-tint' : ''}`}
            >
              <div className="flex items-baseline justify-between text-sm">
                <span className="font-medium">{r.label}</span>
                <span className="font-semibold">{r.amount}</span>
              </div>
              <p className={`mt-0.5 text-xs leading-snug transition ${active === idx ? 'text-ink' : 'text-ink-muted'}`}>
                {r.note}
              </p>
            </div>
          ))}
        </div>
        <div className="mt-2 rounded-r-lg border-l-[3px] border-warn bg-warn-tint px-2.5 py-2 text-sm font-semibold text-warn-ink">
          23% of this bill is taxes and extra charges — not electricity.
        </div>
        <div className="mt-2 flex items-baseline justify-between border-t-2 border-ink px-2 pt-2 text-base font-bold">
          <span>Total Payable</span><span>Rs 8,540</span>
        </div>
        <div className="mt-2 rounded-card bg-brand-tint p-2.5">
          <div className="text-xs font-semibold text-brand-dark">💰 Your savings plan</div>
          <p className="mt-0.5 text-xs leading-snug text-brand-dark/80">
            312 units is about the same as running 1 AC for 8.7 hours a day.
            Cutting just 1 hour a day saves roughly Rs 750 a month.
          </p>
        </div>
        <p className="mt-2 text-center text-xs text-ink-muted">
          Hover any line — your real report explains every charge like this.
        </p>
      </div>
    </div>
  )
}

export default function UploadPage() {
  const [file, setFile] = useState(null)
  const [busy, setBusy] = useState(false)
  const [stage, setStage] = useState(0)
  const [error, setError] = useState(null)
  const [dragOver, setDragOver] = useState(false)
  const inputRef = useRef(null)
  const navigate = useNavigate()

  function pick(f) {
    setError(null)
    if (!f) return
    const okTypes = ['image/jpeg', 'image/png', 'image/webp', 'application/pdf']
    if (!okTypes.includes(f.type)) { setError('Please upload a photo (JPG/PNG) or PDF of your bill.'); return }
    if (f.size > 10 * 1024 * 1024) { setError('File is too large — maximum 10 MB.'); return }
    setFile(f)
  }

  async function submit() {
    if (!file) return
    setBusy(true); setError(null); setStage(0)
    const timer = setInterval(() => setStage(s => Math.min(s + 1, STAGES.length - 1)), 7000)
    try {
      const bill = await uploadBill(file)
      navigate(`/bills/${bill.id}`)
    } catch (e) { setError(e.message) }
    finally { clearInterval(timer); setBusy(false) }
  }

  return (
    <>
      {/* ============ HERO: copy + sample side by side ============ */}
      <section className="hero-grid">
        <div className="mx-auto grid w-full max-w-6xl items-center gap-10 px-5 pb-8 pt-8 lg:grid-cols-[1.05fr_0.95fr]">
          <div>
            <span className="fade-up inline-flex items-center gap-2 rounded-full bg-brand-tint px-3 py-1.5 text-xs font-semibold uppercase tracking-wider text-brand">
              <span className="pulse-dot inline-block h-1.5 w-1.5 rounded-full bg-brand" />
              For LESCO bills · Lahore
            </span>
            <h1 className="fade-up delay-1 mt-4 font-serif text-3xl font-bold leading-[1.18] text-brand-dark lg:text-4xl">
              Your electricity bill, explained in{' '}
              <em className="not-italic text-brand underline decoration-warn decoration-4 underline-offset-[7px]">simple words</em>.
            </h1>
            <p dir="rtl" className="fade-up delay-2 mt-3 text-left font-urdu text-lg text-ink-muted">
              بل کی تصویر بھیجیں — ہم ہر روپیہ سمجھا دیں گے
            </p>
            <p className="fade-up delay-2 mt-3 max-w-[48ch] text-sm leading-relaxed text-ink-muted">
              Upload a photo of your bill. We explain every single charge in plain language,
              warn you before your bill jumps to a higher rate, and give you a savings plan
              written for your home.
            </p>
            <div className="fade-up delay-3 mt-4 flex flex-wrap gap-x-6 gap-y-1.5 text-sm text-ink-muted">
              <span><b className="text-brand-dark">Free</b> forever</span>
              <span><b className="text-brand-dark">30 seconds</b> to full breakdown</span>
              <span><b className="text-brand-dark">No account</b> needed</span>
            </div>
          </div>

          <div className="fade-up delay-3 hidden lg:block"><SampleReport /></div>
        </div>
      </section>

      {/* ============ UPLOAD: its own roomy section ============ */}
      <section className="border-y border-line bg-card">
        <div className="mx-auto w-full max-w-3xl px-5 py-10 text-center">
          <h2 className="font-serif text-2xl font-bold text-brand-dark">Try it with your bill</h2>
          <p className="mt-2 text-sm text-ink-muted">One photo. Thirty seconds. Every rupee explained.</p>

          <div
            id="upload-zone"
            className={`mx-auto mt-6 cursor-pointer rounded-panel border-2 border-dashed p-8 transition
              ${dragOver ? 'border-brand bg-brand-tint' : 'border-line bg-paper hover:border-brand/60'}`}
            onClick={() => inputRef.current?.click()}
            onDragOver={e => { e.preventDefault(); setDragOver(true) }}
            onDragLeave={() => setDragOver(false)}
            onDrop={e => { e.preventDefault(); setDragOver(false); pick(e.dataTransfer.files[0]) }}
          >
            <input ref={inputRef} type="file" accept="image/jpeg,image/png,image/webp,application/pdf"
                   className="hidden" onChange={e => pick(e.target.files[0])} />
            <div className="text-4xl">📷</div>
            {file ? (
              <p className="mt-3 text-base font-semibold text-brand-dark">{file.name}</p>
            ) : (
              <>
                <p className="mt-3 text-base font-semibold text-brand-dark">Tap to choose your bill photo</p>
                <p className="mt-1.5 text-sm text-ink-muted">or drag and drop · JPG, PNG or PDF · even a blurry phone photo works</p>
              </>
            )}
          </div>

          {error && (
            <div className="mt-4 rounded-btn bg-danger-tint px-4 py-2.5 text-sm font-medium text-danger">{error}</div>
          )}

          <button
            onClick={submit}
            disabled={!file || busy}
            className="mt-5 w-full max-w-md rounded-btn bg-brand px-6 py-3.5 text-base font-semibold text-white transition
                       hover:-translate-y-0.5 hover:bg-brand-dark hover:text-paper hover:shadow-lg disabled:cursor-not-allowed disabled:opacity-40 disabled:hover:translate-y-0"
          >
            {busy ? STAGES[stage] : 'Explain My Bill'}
          </button>
          {busy && (
            <p className="mt-3 text-sm text-ink-muted">
              Reading takes about half a minute — we check every number twice.
            </p>
          )}
        </div>
      </section>

      {/* ============ STAT STRIP ============ */}
      <section className="mx-auto grid w-full max-w-6xl gap-8 px-5 py-10 text-center sm:grid-cols-4">
        {[
          ['19', 'charge types explained'],
          ['4', 'math checks on every bill'],
          ['30s', 'photo to full breakdown'],
          ['Rs 0', 'free, no account needed'],
        ].map(([num, label]) => (
          <div key={label}>
            <div className="font-serif text-3xl font-bold text-brand">{num}</div>
            <div className="mt-1 text-xs font-medium uppercase tracking-wider text-ink-muted">{label}</div>
          </div>
        ))}
      </section>

      {/* ============ HOW IT WORKS ============ */}
      <section className="border-y border-line bg-card">
        <div className="mx-auto w-full max-w-6xl px-5 py-10">
          <h2 className="text-center font-serif text-2xl font-bold text-brand-dark">Three simple steps</h2>
          <p className="mt-2 text-center text-sm text-ink-muted">No sign-up. No payment. No confusing words.</p>
          <div className="mt-8 grid gap-5 md:grid-cols-3">
            {[
              ['📷', 'Send a photo', 'Upload a photo or PDF of your bill. Even a blurry phone photo works.'],
              ['🔍', 'We read it for you', 'Every charge and tax is separated and explained in words anyone can understand.'],
              ['💰', 'Start saving', 'Get a warning before your bill jumps to a higher rate, plus a savings plan written for your home.'],
            ].map(([icon, title, body]) => (
              <div key={title} className="rounded-card border border-line bg-paper p-5 transition hover:-translate-y-1 hover:shadow-panel">
                <div className="grid h-10 w-10 place-items-center rounded-chip bg-brand-tint text-xl">{icon}</div>
                <h3 className="mt-3 text-base font-semibold text-brand-dark">{title}</h3>
                <p className="mt-1.5 text-sm leading-relaxed text-ink-muted">{body}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ============ SLAB BAND (dark) ============ */}
      <section className="mx-auto w-full max-w-6xl px-5 py-10">
        <div className="grid items-center gap-8 rounded-panel bg-[#0E3B2E] p-7 text-night-text lg:grid-cols-2">
          <div>
            <h2 className="font-serif text-2xl font-bold">The 200-unit trap, explained</h2>
            <p className="mt-3 text-sm leading-relaxed text-night-muted">
              Electricity has price levels called slabs. The moment you cross 200 units, your
              whole bill is charged at a higher rate — not just the extra units. BijliSaver
              warns you before you cross, so saving just a few units can save you thousands
              of rupees.
            </p>
          </div>
          <div className="rounded-card bg-[#0A2C22] p-5">
            <div className="text-sm font-semibold">Example: 312 of 400 units used</div>
            <div className="mt-3 flex h-4 overflow-hidden rounded-lg">
              <div className="w-1/4 bg-[#2e7d5b]" /><div className="w-1/4 bg-[#4a9b74]" />
              <div className="w-[28%] bg-warn" /><div className="w-[22%] bg-[#1a4535]" />
            </div>
            <div className="mt-2 flex justify-between text-[11px] text-[#8fb1a2]">
              <span>0</span><span>100</span><span>200</span><span>300</span><span>400+</span>
            </div>
            <div className="mt-3 text-sm font-semibold text-warn">
              ⚠ Only 88 units left before the next price jump (+Rs 1,400)
            </div>
          </div>
        </div>
      </section>
    </>
  )
}
