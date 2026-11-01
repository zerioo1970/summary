// src/App.tsx
// 先把几个组件都写在 App.tsx 里，第 2C 章再拆到各自的文件

// 页面顶部：标题和一句说明
function PageHeader() {
  return (
    <header>
      <h1>产品价格清单</h1>
      <p className="subtitle">超声事业部 · 内部报价参考</p>
    </header>
  )
}

// 一张产品卡片。本章数据先写死在组件里，第 3A 章改成由外部传入
function ProductCard() {
  const name = '凸阵探头 C5-2'
  const price = 12800
  const stock = 6

  return (
    <section className="card">
      <h2>{name}</h2>
      {/* 花括号里可以放任何 JavaScript 表达式，结果会显示在页面上 */}
      <p>单价：{price.toLocaleString()} 元</p>
      <p>库存：{stock} 台</p>
    </section>
  )
}

// 页面底部：显示数据更新日期
function PageFooter() {
  const updatedAt = '2026-10-30'
  return <footer>数据更新日期：{updatedAt}</footer>
}

// App 只负责「摆放」：页面由哪些组件组成、按什么顺序排列
function App() {
  return (
    <main>
      <PageHeader />
      {/* 同一个组件可以用多次，每次都是一份独立的界面 */}
      <ProductCard />
      <ProductCard />
      <PageFooter />
    </main>
  )
}

export default App
