// src/components/Notice.tsx
// 练习 2：提示条。children 可以不传，不传时显示「暂无说明」
import type { ReactNode } from 'react'

type NoticeProps = {
  // 问号：children 可选。不传时它的值是 undefined
  children?: ReactNode
}

export function Notice({ children }: NoticeProps) {
  // ?? ：左边是 undefined 或 null 时，用右边的值
  return <p className="notice">提示：{children ?? '暂无说明'}</p>
}
