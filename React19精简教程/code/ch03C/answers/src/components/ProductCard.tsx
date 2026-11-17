// src/components/ProductCard.tsx
// 练习 1 做完后的版本
import type { Product } from '../types.ts'
import { Tag } from './Tag.tsx'

type ProductCardProps = {
  product: Product
  unit?: string
}

export function ProductCard({ product, unit = '台' }: ProductCardProps) {
  return (
    <section className="card">
      {/* 练习 1：分类来自数据，用 Tag 包起来 */}
      <h2>
        {product.name} <Tag>{product.category}</Tag>
      </h2>
      <p>编号：{product.code}</p>
      <p>单价：{product.price.toLocaleString()} 元</p>
      <p>
        库存：{product.stock} {unit}
      </p>
    </section>
  )
}
