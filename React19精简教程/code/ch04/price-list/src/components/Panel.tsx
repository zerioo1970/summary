// src/components/Panel.tsx
// 带标题的分组框。框里放什么由使用者决定，通过 children 传进来
import type { ReactNode } from 'react'

type PanelProps = {
  title: string
  // ReactNode：任何 React 能显示的东西（JSX、文字、数字、数组……）
  children: ReactNode
}

export function Panel({ title, children }: PanelProps) {
  return (
    <section className="panel">
      <h2>{title}</h2>
      {/* 写在 <Panel>...</Panel> 中间的内容，会出现在这里 */}
      {children}
    </section>
  )
}
