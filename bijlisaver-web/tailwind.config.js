/** BijliSaver — Saada theme, v4: tokens as CSS variables (enables dark mode) */
export default {
  content: ["./index.html", "./src/**/*.{js,jsx}"],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        paper: "var(--paper)",
        card: "var(--card)",
        line: "var(--line)",
        brand: { DEFAULT: "var(--brand)", dark: "var(--brand-dark)", tint: "var(--brand-tint)" },
        ink: { DEFAULT: "var(--ink)", muted: "var(--ink-muted)" },
        warn: { DEFAULT: "#E4A812", tint: "var(--warn-tint)", ink: "var(--warn-ink)" },
        danger: { DEFAULT: "var(--danger)", tint: "var(--danger-tint)" },
        success: { DEFAULT: "#16A34A", tint: "var(--success-tint)" },
        night: { DEFAULT: "#0A2C22", text: "#EAF3EE", muted: "#B7CCC1" },
      },
      fontFamily: {
        sans: ["'IBM Plex Sans'", "system-ui", "sans-serif"],
        serif: ["'IBM Plex Serif'", "Georgia", "serif"],
        urdu: ["'Noto Nastaliq Urdu'", "serif"],
      },
      borderRadius: { chip: "7px", btn: "9px", card: "14px", panel: "18px" },
      boxShadow: { panel: "0 8px 30px rgba(14, 59, 46, 0.07)" },
    },
  },
  plugins: [],
}
