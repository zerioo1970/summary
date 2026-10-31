import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    // 开发代理：浏览器请求 /api 开头的地址时，Vite 开发服务器替它转发给 3001 端口的模拟接口
    proxy: {
      '/api': 'http://localhost:3001',
    },
  },
})
