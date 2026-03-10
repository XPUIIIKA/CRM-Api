import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'


export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Всі запити, що починаються з /api, Vite буде пересилати на бекенд
      '/api': {
        target: 'http://localhost:5210', // Або https://localhost:5210, якщо там https
        changeOrigin: true,
        secure: false, // Допомагає, якщо на локальному бекенді немає нормального SSL-сертифіката
      }
    }
  }
})