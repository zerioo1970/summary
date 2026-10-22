// code/ch04/Frontend/vite.config.ts
import { defineConfig } from "vite";

export default defineConfig({
  build: {
    // 构建产物直接写进后端的静态文件目录。
    // 以后改用 Nginx 单独部署前端时，只改这一行（或删掉它，用默认的 dist/）。
    outDir: "../Backend/wwwroot",
    // outDir 在 Frontend 目录之外时，Vite 默认不敢清空它，旧的 js 会越积越多。
    // 这里明确告诉它：这个目录全归构建管，每次先清空再写。
    emptyOutDir: true,
  },
});
