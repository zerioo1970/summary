// src/pages/NotFoundPage.tsx
// 地址和路由表里的哪一条都对不上时显示
import { Link } from 'react-router'

export function NotFoundPage() {
  return (
    <section>
      <h2>页面不存在</h2>
      <p>
        <Link to="/">返回产品清单</Link>
      </p>
    </section>
  )
}
