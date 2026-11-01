// src/components/ProductCard.tsx
// 练习 2：金额显示改用 utils/format.ts 里的 formatPrice
// ../ 表示上一级文件夹：从 components 回到 src，再进 utils
import { formatPrice } from '../utils/format.ts'

export function ProductCard() {
  const name = '凸阵探头 C5-2'
  const price = 12800
  const stock = 6

  return (
    <section className="card">
      <h2>{name}</h2>
      <p>单价：{formatPrice(price)}</p>
      <p>库存：{stock} 台</p>
    </section>
  )
}
