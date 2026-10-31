// src/App.tsx（3.1 版本，3.2 节会再改）
import { PageHeader } from './components/PageHeader.tsx'
import { ProductCard } from './components/ProductCard.tsx'
import { PageFooter } from './components/PageFooter.tsx'

function App() {
  return (
    <main>
      <PageHeader />
      {/* 字符串可以直接写引号；数字要放在花括号里，否则传进去的是字符串 */}
      <ProductCard name="凸阵探头 C5-2" price={12800} stock={6} />
      <ProductCard name="线阵探头 L12-5" price={15600} stock={3} />
      <PageFooter />
    </main>
  )
}

export default App
