// src/App.tsx
// 两个练习都做完后的版本

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

// 练习 1：公告栏，只有一行文字
function NoticeBar() {
  const validUntil = '2026-12-31'
  return <p>价格含税，有效期至 {validUntil}</p>
}

// 练习 2：联系人信息。它不出现在 App 里，而是被 PageFooter 使用
function ContactInfo() {
  return <p>联系人：张三　分机 8021</p>
}

// 页面底部：显示数据更新日期，下面放联系人
function PageFooter() {
  const updatedAt = '2026-10-30'
  return (
    <footer>
      数据更新日期：{updatedAt}
      {/* 练习 2：组件里也可以用别的组件 */}
      <ContactInfo />
    </footer>
  )
}

// App 只负责「摆放」：页面由哪些组件组成、按什么顺序排列
function App() {
  return (
    <main>
      <PageHeader />
      {/* 练习 1：放在页头下面、卡片上面 */}
      <NoticeBar />
      {/* 同一个组件可以用多次，每次都是一份独立的界面 */}
      <ProductCard />
      <ProductCard />
      <PageFooter />
    </main>
  )
}

export default App
