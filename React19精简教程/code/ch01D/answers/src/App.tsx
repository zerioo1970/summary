// src/App.tsx
// 练习 1 做完后的版本
// 参数写了 : number，严格模式下不再报 TS7006
function formatPrice(price: number) {
  return price.toLocaleString() + ' 元'
}

function App() {
  return (
    <main>
      <h1>产品价格清单</h1>
      <p>第 1 章：项目跑起来了。</p>
      {/* JSX 里用花括号放表达式，第 2B 章细讲 */}
      <p>示例单价：{formatPrice(12800)}</p>
    </main>
  )
}

export default App
