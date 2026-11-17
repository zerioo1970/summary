// src/App.tsx
// 两个练习都做完后的版本
import { Link, Route, Routes } from 'react-router'
import { useProducts } from './hooks/useProducts.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { PageFooter } from './components/PageFooter.tsx'
import { ListPage } from './pages/ListPage.tsx'
import { AboutPage } from './pages/AboutPage.tsx'
import { ProductDetailPage } from './pages/ProductDetailPage.tsx'
import { NotFoundPage } from './pages/NotFoundPage.tsx'

function App() {
  // 练习 1：只在 App 里调用一次。App 不会随换页被移除，数据就一直在；两个页面看到的是同一份
  const { state, addProduct, deleteProduct, restockProduct } = useProducts()

  return (
    <main>
      <PageHeader />
      <nav className="nav">
        {/* Link 改地址但不刷新页面；React Router 发现地址变了，换掉下面 Routes 里显示的页面 */}
        <Link to="/">产品清单</Link> | <Link to="/about">关于</Link>
      </nav>
      {/* 路由表：React Router 找出与当前地址最匹配的那条 path，显示它的 element */}
      <Routes>
        <Route
          path="/"
          element={<ListPage state={state} onAdd={addProduct} onDelete={deleteProduct} onRestock={restockProduct} />}
        />
        <Route path="/about" element={<AboutPage />} />
        <Route path="/products/:id" element={<ProductDetailPage state={state} />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
      <PageFooter />
    </main>
  )
}

export default App
