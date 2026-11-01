// src/pages/ListPage.tsx
// 练习 1 之后的版本：数据和增删改函数都由 App 通过 props 传进来
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { Product } from '../types.ts'
import type { LoadState } from '../hooks/useProducts.ts'
import { FilterBar } from '../components/FilterBar.tsx'
import { ProductTable } from '../components/ProductTable.tsx'
import { ProductForm } from '../components/ProductForm.tsx'

type ListPageProps = {
  state: LoadState
  onAdd: (product: Product) => void
  onDelete: (id: number) => void
  onRestock: (id: number) => void
}

export function ListPage({ state, onAdd, onDelete, onRestock }: ListPageProps) {
  // 状态提升：搜索框和表格都要用 keyword，所以放在它们共同的父组件 App 里
  const [keyword, setKeyword] = useState('')

  const count = state.status === 'success' ? state.products.length : 0

  useEffect(() => {
    document.title = `产品价格清单（${count} 种）`
  }, [count])

  let content: ReactNode
  if (state.status === 'loading') {
    content = <p>加载中…</p>
  } else if (state.status === 'error') {
    content = <p className="error">加载失败：{state.message}</p>
  } else {
    // 筛选结果是派生值：由 products 和 keyword 现算，不另存 state
    const k = keyword.trim().toLowerCase()
    const visible = state.products.filter(
      (p) => p.name.toLowerCase().includes(k) || p.code.toLowerCase().includes(k),
    )
    content = <ProductTable products={visible} onDelete={onDelete} onRestock={onRestock} />
  }

  return (
    <section>
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar keyword={keyword} onKeywordChange={setKeyword} />
      {content}
      <ProductForm onAdd={onAdd} />
    </section>
  )
}
