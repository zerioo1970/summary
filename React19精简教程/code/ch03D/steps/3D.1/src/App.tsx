// src/App.tsx（3D.1 版本：把一段 JSX 存进变量，打印出来看看）
import type { Product } from './types.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { ProductCard } from './components/ProductCard.tsx'
import { PageFooter } from './components/PageFooter.tsx'
import { Panel } from './components/Panel.tsx'

// 标注 : Product 后，少写字段或类型不对，这里就会报错
const probe: Product = { id: 1, code: 'P-001', name: '凸阵探头 C5-2', category: '探头', price: 12800, stock: 6 }
// 多一个探头，「探头」分组里才有两张卡片
const linear: Product = { id: 2, code: 'P-002', name: '线阵探头 L12-5', category: '探头', price: 15600, stock: 3 }
const gel: Product = { id: 4, code: 'C-001', name: '耦合剂 250ml', category: '耗材', price: 18, stock: 120 }

// 一段 JSX 写完就是一个值，可以像数字、字符串一样存进变量
const note = <p>以上价格为含税单价，不含运费。</p>
const card = <ProductCard product={gel} unit="瓶" />
// 打印出来看看这两个值里装的是什么
console.log('note 是', note)
console.log('card 是', card)

function App() {
  return (
    <main>
      <PageHeader />
      <Panel title="探头">
        <ProductCard product={probe} />
        <ProductCard product={linear} />
      </Panel>
      {/* 变量里的 JSX 放进 {} 就显示出来，和直接写在这里效果一样 */}
      <Panel title="耗材">{card}</Panel>
      <Panel title="说明">{note}</Panel>
      <PageFooter />
    </main>
  )
}

export default App
