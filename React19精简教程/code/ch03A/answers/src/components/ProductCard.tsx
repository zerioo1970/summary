// src/components/ProductCard.tsx
// 两个练习都做完后的版本
type ProductCardProps = {
  // 练习 1：新加的编号。没有写问号，所以每张卡片都必须传
  code: string
  name: string
  price: number
  stock: number
}

export function ProductCard({ code, name, price, stock }: ProductCardProps) {
  return (
    <section className="card">
      <h2>{name}</h2>
      <p>编号：{code}</p>
      <p>单价：{price.toLocaleString()} 元</p>
      <p>库存：{stock} 台</p>
    </section>
  )
}
