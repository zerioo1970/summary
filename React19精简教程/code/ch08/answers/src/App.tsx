// src/App.tsx
// 两个练习都做完后的版本
import { useState } from 'react'
import type { ReactNode } from 'react'
import { useProducts } from './hooks/useProducts.ts'
import { useDocumentTitle } from './hooks/useDocumentTitle.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { FilterBar } from './components/FilterBar.tsx'
import { ProductTable } from './components/ProductTable.tsx'
import { ProductForm } from './components/ProductForm.tsx'
import { PageFooter } from './components/PageFooter.tsx'

function App() {
  // 加载和增删改的细节都在 useProducts 里，App 只拿结果
  const { state, addProduct, deleteProduct, restockProduct } = useProducts()
  // 状态提升：搜索框和表格都要用 keyword，所以放在它们共同的父组件 App 里
  const [keyword, setKeyword] = useState('')
  // 练习 1：分类筛选同样提升到 App
  const [category, setCategory] = useState('全部')

  const count = state.status === 'success' ? state.products.length : 0

  // 练习 2：原来的 useEffect 换成一行自定义 Hook
  useDocumentTitle(`产品价格清单（${count} 种）`)

  let content: ReactNode
  if (state.status === 'loading') {
    content = <p>加载中…</p>
  } else if (state.status === 'error') {
    content = <p className="error">加载失败：{state.message}</p>
  } else {
    // 筛选结果是派生值：由 products、keyword、category 现算，不另存 state
    const k = keyword.trim().toLowerCase()
    const visible = state.products.filter(
      (p) =>
        (category === '全部' || p.category === category) &&
        (p.name.toLowerCase().includes(k) || p.code.toLowerCase().includes(k)),
    )
    // 把 useProducts 给的函数直接作为回调传下去
    content = <ProductTable products={visible} onDelete={deleteProduct} onRestock={restockProduct} />
  }

  return (
    <main>
      <PageHeader />
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar
        keyword={keyword}
        onKeywordChange={setKeyword}
        category={category}
        onCategoryChange={setCategory}
      />
      {content}
      <ProductForm onAdd={addProduct} />
      <PageFooter />
    </main>
  )
}

export default App
