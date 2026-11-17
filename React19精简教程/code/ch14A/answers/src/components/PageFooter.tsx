// src/components/PageFooter.tsx
// 练习 2：页脚也显示当前用户。PageFooter 没有任何 props，直接从 Context 里取
import { useAuth } from '../auth/AuthContext.ts'

export function PageFooter() {
  const updatedAt = '2026-10-30'
  const { user } = useAuth()
  return (
    <footer>
      数据更新日期：{updatedAt}
      {/* 三元表达式：登录了显示名字，没登录显示「未登录」 */}
      <p>当前用户：{user ? user.name : '未登录'}</p>
    </footer>
  )
}
