import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // 开发代理：/api 开头的请求转发给 5080 端口的 Minimal API（第 11 章是 3001 端口的模拟接口）
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
})
