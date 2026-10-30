// src/components/QuantityPicker.tsx（4.2 版本，4.3 节会再改）
import { useState } from 'react'
// MouseEvent 是 React 的鼠标事件类型，只用来标注参数，所以 import type
import type { MouseEvent } from 'react'

type QuantityPickerProps = {
  price: number
}

export function QuantityPicker({ price }: QuantityPickerProps) {
  const [quantity, setQuantity] = useState(0)

  // 按住 Shift 点「+」一次加 10。要读事件对象，参数类型写清是哪种元素上的鼠标事件
  function handleAdd(e: MouseEvent<HTMLButtonElement>) {
    const step = e.shiftKey ? 10 : 1
    setQuantity(quantity + step)
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
      {/* 传函数本身 handleAdd，不要写成 handleAdd() */}
      <button type="button" onClick={handleAdd}>+</button>
      <p>小计：{(price * quantity).toLocaleString()} 元</p>
    </div>
  )
}
