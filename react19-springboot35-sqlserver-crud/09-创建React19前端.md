# 第 9 章 · 第七步：用 Vite 创建 React 19 前端

> 上一章：[08-启动与接口测试](08-启动与接口测试.md) ｜ 下一章：[10-封装axios与API层](10-封装axios与API层.md)

## 9.1 创建项目

打开终端（可用 IDEA 内置 Terminal），执行：

```bash
npm create vite@latest crud-frontend -- --template react
cd crud-frontend
npm install
```

> `--template react` 生成 JavaScript 版；想用 TypeScript 就用 `react-ts`。Vite 会自动装最新的 React 19。

安装 axios：

```bash
npm install axios
```

启动开发服务器：

```bash
npm run dev
```

默认地址是 `http://localhost:5173`（和后端 CORS 配的要一致）。

## 9.2 前端目录结构

```
crud-frontend/
├── index.html
├── vite.config.js
├── package.json
└── src/
    ├── main.jsx          ← 入口
    ├── App.jsx           ← 根组件
    ├── api/
    │   ├── request.js    ← axios 实例（拦截器）
    │   └── userApi.js    ← 用户相关接口
    └── components/
        └── UserManager.jsx   ← CRUD 页面
```

---

> 下一章 👉 [10-封装axios与API层](10-封装axios与API层.md)
