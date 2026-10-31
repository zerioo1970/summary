// src/components/QuantityPicker.tsx（4.1a 版本：错误示范，马上会改掉）
// 用普通变量记数量：点按钮时变量确实变了，但页面不会更新

type QuantityPickerProps = {
  price: number
}

export function QuantityPicker({ price }: QuantityPickerProps) {
  let quantity = 0

  function handleAdd() {
    quantity = quantity + 1
    console.log('quantity 现在是', quantity)
  }

  return (
    <div>
      询价数量：<strong>{quantity}</strong>{' '}
      <button type="button" onClick={handleAdd}>+</button>
      <p>小计：{(price * quantity).toLocaleString()} 元</p>
    </div>
  )
}
