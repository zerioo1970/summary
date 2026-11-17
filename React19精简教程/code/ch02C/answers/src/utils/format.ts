// src/utils/format.ts
// 练习 2：格式化金额的函数。里面没有 JSX，所以扩展名是 .ts
export function formatPrice(price: number) {
  return price.toLocaleString() + ' 元'
}
