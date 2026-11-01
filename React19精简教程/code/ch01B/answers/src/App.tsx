// src/App.tsx
// 练习 1：函数名改成 PriceListApp，main.tsx 不用改
function PriceListApp() {
  return (
    <main>
      <h1>产品价格清单</h1>
      <p>第 1 章：项目跑起来了。</p>
    </main>
  )
}

// 默认导出不带名字，main.tsx 写 import App from './App.tsx' 照样拿到它
export default PriceListApp
