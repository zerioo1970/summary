// src/components/ProductCard.tsx
// 一张产品卡片。本章数据先写死在组件里，第 3 章改成由外部传入
export function ProductCard() {
  const name = '凸阵探头 C5-2'
  const price = 12800
  const stock = 6

  return (
    <section className="card">
      <h2>{name}</h2>
      {/* 花括号里可以放任何 JavaScript 表达式，结果会显示在页面上 */}
      <p>单价：{price.toLocaleString()} 元</p>
      {/* 计算直接写在花括号里；toLocaleString 顺便去掉了小数点后的误差 */}
      <p>含税价（13%）：{(price * 1.13).toLocaleString()} 元</p>
      <p>库存：{stock} 台</p>
    </section>
  )
}
