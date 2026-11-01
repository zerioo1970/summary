// src/components/PageHeader.tsx
// 练习 2：标题来自 .env 里的 VITE_APP_TITLE
export function PageHeader() {
  return (
    <header>
      <h1>{import.meta.env.VITE_APP_TITLE}</h1>
      <p className="subtitle">超声事业部 · 内部报价参考</p>
    </header>
  )
}
