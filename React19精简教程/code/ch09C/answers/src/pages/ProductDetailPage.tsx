// src/pages/ProductDetailPage.tsx
// 两个练习都做完后的版本
import { Link, useParams } from 'react-router'
import type { LoadState } from '../hooks/useProducts.ts'
import { ProductCard } from '../components/ProductCard.tsx'

type ProductDetailPageProps = {
  // 练习 1：数据由 App 传进来，不再自己调用 useProducts
  state: LoadState
}

export function ProductDetailPage({ state }: ProductDetailPageProps) {
  const { id } = useParams()

  if (state.status === 'loading') {
    return <p>加载中…</p>
  }
  if (state.status === 'error') {
    return <p className="error">加载失败：{state.message}</p>
  }

  const products = state.products
  // 练习 2：用下标找前后两个产品。找不到时 findIndex 返回 -1
  const index = products.findIndex((p) => p.id === Number(id))
  const product = index === -1 ? undefined : products[index]
  const prev = index > 0 ? products[index - 1] : undefined
  const next = index !== -1 && index < products.length - 1 ? products[index + 1] : undefined

  return (
    <section>
      <p>
        <Link to="/">← 返回列表</Link>
      </p>
      {product ? <ProductCard product={product} /> : <p className="empty">找不到编号为 {id} 的产品</p>}
      <p>
        {prev ? <Link to={`/products/${prev.id}`}>上一个：{prev.name}</Link> : '已经是第一个'}
        {' | '}
        {next ? <Link to={`/products/${next.id}`}>下一个：{next.name}</Link> : '已经是最后一个'}
      </p>
    </section>
  )
}
