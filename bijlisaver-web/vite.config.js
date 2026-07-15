import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Proxy /api → the .NET backend so the browser never fights CORS in dev.
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': { target: 'http://localhost:5000', changeOrigin: true },
    },
  },
})
