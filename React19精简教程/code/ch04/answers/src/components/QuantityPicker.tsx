// src/components/QuantityPicker.tsx
// 两个练习都做完后的版本
import { useState } from 'react'
import type { MouseEvent } from 'react'

type QuantityPickerProps = {
  price: number
  // 练习 1：最多能询价的数量（库存）
  max: number
}

export function QuantityPicker({ price, max }: QuantityPickerProps) {
  const [quantity, setQuantity] = useState(0)
  console.log(`QuantityPicker 渲染，quantity = ${quantity}`)

  function handleAdd(e: MouseEvent<HTMLButtonElement>) {
    const step = e.shiftKey ? 10 : 1
    // Shift 一次加 10 也可能超过库存，用 Math.min 截到最大值
    setQuantity(Math.min(quantity + step, max))
  }

  function handleMinus() {
    if (quantity > 0) {
      setQuantity(quantity - 1)
    }
  }

  function handleAddTwo() {
    setQuantity((q) => Math.min(q + 1, max))
    setQuantity((q) => Math.min(q + 1, max))
  }

  // 练习 2：清零就是把 state 设回初始值
  function handleReset() {
    setQuantity(0)
  }

  // 由 state 算出来的值，不需要再存一个 state
  const reachedMax = quantity >= max

  return (
    <div>
      询价数量：
      <button type="button" onClick={handleMinus}>−</button>
      <strong> {quantity} </strong>
      <button type="button" onClick={handleAdd} disabled={reachedMax}>+</button>{' '}
      <button type="button" onClick={handleAddTwo} disabled={reachedMax}>+2</button>{' '}
      <button type="button" onClick={handleReset}>清零</button>
      <p>小计：{(price * quantity).toLocaleString()} 元</p>
    </div>
  )
}
