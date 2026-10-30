// src/components/ProductCard.tsx（3.1 版本，3.2 节会再改）
// 卡片显示什么，由使用它的地方通过 props 传进来

// props 的类型：调用方必须传这三个值，类型也必须对
type ProductCardProps = {
  name: string
  price: number
  stock: number
}

// 参数位置的 { name, price, stock } 是解构：直接从 props 对象里取出三个值
export function ProductCard({ name, price, stock }: ProductCardProps) {
  return (
    <section className="card">
      <h2>{name}</h2>
      <p>单价：{price.toLocaleString()} 元</p>
      <p>库存：{stock} 台</p>
    </section>
  )
}
