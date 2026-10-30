// src/App.tsx
// App 只负责「摆放」：页面由哪些组件组成、按什么顺序排列
import { PageHeader } from './components/PageHeader.tsx'
import { NoticeBar } from './components/NoticeBar.tsx'
import { ProductCard } from './components/ProductCard.tsx'
import { PageFooter } from './components/PageFooter.tsx'

function App() {
  return (
    <main>
      <PageHeader />
      <NoticeBar />
      {/* 同一个组件可以用多次，每次都是一份独立的界面 */}
      <ProductCard />
      <ProductCard />
      <PageFooter />
    </main>
  )
}

export default App
