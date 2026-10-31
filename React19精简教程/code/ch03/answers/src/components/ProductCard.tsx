// src/components/ProductCard.tsx
// 两个练习都做完后的版本
import type { Product } from '../types.ts'
import { Tag } from './Tag.tsx'

type ProductCardProps = {
  product: Product
  unit?: string
  // 练习 1：可选的折扣率，0.9 表示九折；不传就是不打折
  discount?: number
}

export function ProductCard({ product, unit = '台', discount = 1 }: ProductCardProps) {
  // 先在 return 前算好，JSX 里只放结果
  const finalPrice = product.price * discount

  return (
    <div className="card">
      <h3>
        {product.name} <Tag>{product.category}</Tag>
      </h3>
      <p>编号：{product.code}</p>
      <p>单价：{product.price.toLocaleString()} 元</p>
      <p>折后价：{finalPrice.toLocaleString()} 元</p>
      <p>
        库存：{product.stock} {unit}
      </p>
    </div>
  )
}
