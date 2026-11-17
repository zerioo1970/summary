// src/components/QuantityPicker.tsx
// 两个练习都做完后的版本
import { useState } from 'react'

type QuantityPickerProps = {
  price: number
}

export function QuantityPicker({ price }: QuantityPickerProps) {
  const [quantity, setQuantity] = useState(0)
  // 练习 2：第二个 state。一个组件里可以有多个 state，各管各的
  const [samples, setSamples] = useState(0)

  function handleAdd() {
    setQuantity(quantity + 1)
  }

  function handleMinus() {
    if (quantity > 0) {
      setQuantity(quantity - 1)
    }
  }

  // 练习 1：清零就是把 state 设回初始值
  function handleReset() {
    setQuantity(0)
  }

  function handleAddSample() {
    setSamples(samples + 1)
  }

  return (
    <div>
      询价数量：
      <button type="button" onClick={handleMinus}>−</button>
      <strong> {quantity} </strong>
      <button type="button" onClick={handleAdd}>+</button>{' '}
      <button type="button" onClick={handleReset}>清零</button>
      <p>
        样品数量：<strong>{samples}</strong>{' '}
        <button type="button" onClick={handleAddSample}>+</button>
      </p>
      {/* 小计只和询价数量有关，样品不计价 */}
      <p>小计：{(price * quantity).toLocaleString()} 元</p>
    </div>
  )
}
