import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  return {
    plugins: [vue()],
    server: {
      port: 5173,
      strictPort: true,
      proxy: {
        '/api': {
          target: env.API_PROXY_TARGET || 'https://localhost:7228',
          changeOrigin: true,
          // Certificado de desenvolvimento do ASP.NET, somente no proxy local.
          secure: false,
        },
      },
    },
  }
})
