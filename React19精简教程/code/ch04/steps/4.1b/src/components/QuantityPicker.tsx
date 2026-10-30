// src/components/QuantityPicker.tsx（4.1b 版本，4.2 节会再改）
import { useState } from 'react'

type QuantityPickerProps = {
  price: number
}

export function QuantityPicker({ price }: QuantityPickerProps) {
  // useState 返回两样东西：当前值、修改它的函数。0 是初始值
  const [quantity, setQuantity] = useState(0)

  function handleAdd() {
    // 不是直接改 quantity，而是告诉 React「新值是多少」，React 会重新渲染
    setQuantity(quantity + 1)
  }

  function handleMinus() {
    if (quantity > 0) {
      setQuantity(quantity - 1)
    }
  }

  return (
    <div>
      询价数量：
      <button type="button" onClick={handleMinus}>−</button>
      <strong> {quantity} </strong>
      <button type="button" onClick={handleAdd}>+</button>
      <p>小计：{(price * quantity).toLocaleString()} 元</p>
    </div>
  )
}
