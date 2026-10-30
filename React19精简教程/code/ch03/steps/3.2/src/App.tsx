// src/App.tsx（3.2 版本，3.3 节会再改）
import type { Product } from './types.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { ProductCard } from './components/ProductCard.tsx'
import { PageFooter } from './components/PageFooter.tsx'

// 标注 : Product 后，少写字段或类型不对，这里就会报错
const probe: Product = { id: 1, code: 'P-001', name: '凸阵探头 C5-2', category: '探头', price: 12800, stock: 6 }
const gel: Product = { id: 4, code: 'C-001', name: '耦合剂 250ml', category: '耗材', price: 18, stock: 120 }

function App() {
  return (
    <main>
      <PageHeader />
      <ProductCard product={probe} />
      {/* 耗材按「瓶」计，传 unit 覆盖默认的「台」 */}
      <ProductCard product={gel} unit="瓶" />
      <PageFooter />
    </main>
  )
}

export default App
