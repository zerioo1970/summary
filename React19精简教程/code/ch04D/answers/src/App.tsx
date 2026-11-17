// src/App.tsx
// 两个练习都做完后的版本，🟨 标出练习改动的地方
// 导入 useState
import { useState } from 'react'
import type { Product } from './types.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { Panel } from './components/Panel.tsx'
import { ProductCard } from './components/ProductCard.tsx'
import { PageFooter } from './components/PageFooter.tsx'

// 标注 : Product 后，少写字段或类型不对，这里就会报错
const probe: Product = { id: 1, code: 'P-001', name: '凸阵探头 C5-2', category: '探头', price: 12800, stock: 6 }
const linear: Product = { id: 2, code: 'P-002', name: '线阵探头 L12-5', category: '探头', price: 15600, stock: 3 }
const gel: Product = { id: 4, code: 'C-001', name: '耦合剂 250ml', category: '耗材', price: 18, stock: 120 }

function App() {
  // 两个 state。一个组件里可以有多个 state，每个 useState 管一份数据
  // 初始值 true 推断出 boolean，初始值 '' 推断出 string
  const [showGel, setShowGel] = useState(true)
  const [note, setNote] = useState('')
  // 🟨 练习 1：再加一个 state，管探头面板显示不显示
  const [showProbe, setShowProbe] = useState(true)
  // 🟨 练习 2：耗材被隐藏了几次
  const [hideCount, setHideCount] = useState(0)
  // 每次渲染都打印，用来观察 state 的值和渲染次数
  console.log(`App 渲染，showGel = ${showGel}，note = ${note}`)

  // 🟨 练习 2：已经隐藏时再点，不算一次。showGel 是这次渲染的快照，点击时读到的就是页面上的状态
  function handleHideGel() {
    if (showGel) {
      setHideCount(hideCount + 1)
    }
    setShowGel(false)
  }

  return (
    <main>
      <PageHeader />
      {/* 三个按钮，后面显示核对备注 */}
      <p>
        {/* 🟨 练习 2：改成调用 handleHideGel */}
        <button type="button" onClick={handleHideGel}>隐藏耗材</button>{' '}
        <button type="button" onClick={() => setShowGel(true)}>显示耗材</button>{' '}
        <button type="button" onClick={() => setNote('已核对')}>标记已核对</button>
        {' '}{note}
      </p>
      {/* 🟨 练习 1、2：探头开关和隐藏次数 */}
      <p>
        <button type="button" onClick={() => setShowProbe(false)}>隐藏探头</button>{' '}
        <button type="button" onClick={() => setShowProbe(true)}>显示探头</button>{' '}
        耗材已隐藏 {hideCount} 次
      </p>
      {/* 🟨 练习 1：探头面板也按 state 决定显示不显示 */}
      {showProbe && (
        <Panel title="探头">
          <ProductCard product={probe} />
          <ProductCard product={linear} />
        </Panel>
      )}
      {/* showGel 为 false 时，整个耗材面板（连同里面的卡片）从页面上移除 */}
      {showGel && (
        <Panel title="耗材">
          {/* 耗材按「瓶」计，传 unit 覆盖默认的「台」 */}
          <ProductCard product={gel} unit="瓶" />
        </Panel>
      )}
      <PageFooter />
    </main>
  )
}

export default App
