// src/App.tsx
// 两个练习都做完后的版本
import type { Product } from './types.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { ProductCard } from './components/ProductCard.tsx'
import { PageFooter } from './components/PageFooter.tsx'

// 标注 : Product 后，少写字段或类型不对，这里就会报错
const probe: Product = { id: 1, code: 'P-001', name: '凸阵探头 C5-2', category: '探头', price: 12800, stock: 6 }
const gel: Product = { id: 4, code: 'C-001', name: '耦合剂 250ml', category: '耗材', price: 18, stock: 120 }
// 练习 2：第三个产品，同样标注 : Product
const host: Product = { id: 3, code: 'H-001', name: '便携彩超主机', category: '主机', price: 168000, stock: 2 }

function App() {
  return (
    <main>
      <PageHeader />
      {/* 练习 1：只有这张卡片打九折 */}
      <ProductCard product={probe} discount={0.9} />
      {/* 耗材按「瓶」计，传 unit 覆盖默认的「台」 */}
      <ProductCard product={gel} unit="瓶" />
      <ProductCard product={host} />
      <PageFooter />
    </main>
  )
}

export default App
