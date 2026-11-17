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

  function handleAdd(e: MouseEvent<HTMLButtonElement>) {
    const step = e.shiftKey ? 10 : 1
    // 练习 1：Shift 一次加 10 也可能超过库存，用 Math.min 截到最大值
    setQuantity(Math.min(quantity + step, max))
  }

  // 练习 2：要读 e.ctrlKey，所以这里也写上事件参数
  function handleMinus(e: MouseEvent<HTMLButtonElement>) {
    if (e.ctrlKey) {
      setQuantity(0)
    } else if (quantity > 0) {
      setQuantity(quantity - 1)
    }
  }

  // 练习 1：是否到达上限由 quantity 和 max 算出来，不另存 state
  const reachedMax = quantity >= max

  return (
    <div>
      询价数量：
      <button type="button" onClick={handleMinus}>−</button>
      <strong> {quantity} </strong>
      <button type="button" onClick={handleAdd} disabled={reachedMax}>+</button>
      <p>小计：{(price * quantity).toLocaleString()} 元</p>
    </div>
  )
}
