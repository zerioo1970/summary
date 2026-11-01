// src/components/QuantityPicker.tsx
import { useState } from 'react'
// MouseEvent 是 React 的鼠标事件类型，只用来标注参数，所以 import type
import type { MouseEvent } from 'react'

type QuantityPickerProps = {
  price: number
}

export function QuantityPicker({ price }: QuantityPickerProps) {
  const [quantity, setQuantity] = useState(0)
  // 组件函数每执行一次（每次渲染）就打印一次，用来观察什么时候重新渲染
  console.log(`QuantityPicker 渲染，quantity = ${quantity}`)

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

  function handleAddTwo() {
    // 传「更新函数」：React 把上一次的结果作为 q 传进来，两次调用会依次累加
    setQuantity((q) => q + 1)
    setQuantity((q) => q + 1)
  }

  return (
    <div>
      询价数量：
      <button type="button" onClick={handleMinus}>−</button>
      <strong> {quantity} </strong>
      {/* 传函数本身 handleAdd，不要写成 handleAdd() */}
      <button type="button" onClick={handleAdd}>+</button>{' '}
      <button type="button" onClick={handleAddTwo}>+2</button>
      <p>小计：{(price * quantity).toLocaleString()} 元</p>
    </div>
  )
}
