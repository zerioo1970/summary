// src/pages/ProductDetailPage.tsx
// 练习 1：详情页只请求这一个产品，不再加载整张列表
import { Link, useParams } from 'react-router'
import { useProduct } from '../hooks/useProduct.ts'
import { ProductCard } from '../components/ProductCard.tsx'

export function ProductDetailPage() {
  const { id } = useParams()
  const state = useProduct(id)

  return (
    <section>
      <p>
        <Link to="/">← 返回列表</Link>
      </p>
      {state.status === 'loading' && <p>加载中…</p>}
      {state.status === 'error' && <p className="error">加载失败：{state.message}</p>}
      {state.status === 'success' && <ProductCard product={state.product} />}
    </section>
  )
}
