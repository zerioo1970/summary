// src/pages/NotFoundPage.tsx
// 练习 2 做完后的版本
import { useEffect } from 'react'
import { Link } from 'react-router'

export function NotFoundPage() {
  // 练习 2：标签页标题在 #root 之外，用 effect 修改（第 7A 章）
  useEffect(() => {
    document.title = '页面不存在'
  }, [])

  return (
    <section>
      <h2>页面不存在</h2>
      <p>
        <Link to="/">返回产品清单</Link>
      </p>
    </section>
  )
}
