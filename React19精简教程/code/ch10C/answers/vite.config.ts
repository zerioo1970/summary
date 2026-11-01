// vite.config.ts
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // 练习 1：网站部署在 https://服务器/price-list/ 下。开头和结尾的 / 都不能少
  base: '/price-list/',
})
