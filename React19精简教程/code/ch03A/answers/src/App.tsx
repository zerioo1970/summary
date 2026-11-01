// src/App.tsx
// 两个练习都做完后的版本
import { PageHeader } from './components/PageHeader.tsx'
import { ProductCard } from './components/ProductCard.tsx'
import { PageFooter } from './components/PageFooter.tsx'

function App() {
  return (
    <main>
      <PageHeader />
      {/* 练习 1：每张卡片多传一个 code */}
      <ProductCard code="P-001" name="凸阵探头 C5-2" price={12800} stock={6} />
      <ProductCard code="P-002" name="线阵探头 L12-5" price={15600} stock={3} />
      {/* 练习 2：第三张卡片。组件不用改，多写一行就多一张 */}
      <ProductCard code="C-001" name="耦合剂 250ml" price={18} stock={120} />
      <PageFooter />
    </main>
  )
}

export default App
