// src/App.tsx
// 两个练习都做完后的版本
import { useState } from 'react'
import type { Product } from './types.ts'
import { initialProducts } from './data.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { PageFooter } from './components/PageFooter.tsx'

function App() {
  // 尖括号里写明 state 的类型：Product 数组
  const [products, setProducts] = useState<Product[]>(initialProducts)
  // 练习 2：是否只看有货。一个开关就是一个 boolean state
  const [onlyInStock, setOnlyInStock] = useState(false)

  function handleDelete(id: number) {
    // filter 返回一个新数组，原数组不动
    setProducts(products.filter((p) => p.id !== id))
  }

  function handleRestock(id: number) {
    // map 返回新数组；要改的那一行用 { ...p, stock: 新值 } 生成新对象，其余行原样保留
    setProducts(products.map((p) => (p.id === id ? { ...p, stock: p.stock + 1 } : p)))
  }

  // 练习 1：出库和入库写法相同，只是减 1
  function handleShip(id: number) {
    setProducts(products.map((p) => (p.id === id ? { ...p, stock: p.stock - 1 } : p)))
  }

  // 练习 2：要显示的行由 state 算出来，不另存一份「筛选后的数组」
  const visibleProducts = onlyInStock ? products.filter((p) => p.stock > 0) : products

  return (
    <main>
      <PageHeader />
      <p>
        共 {products.length} 种产品{' '}
        <button type="button" onClick={() => setOnlyInStock(!onlyInStock)}>
          {onlyInStock ? '显示全部' : '只看有货'}
        </button>
      </p>
      {/* 条件 ? A : B：二选一。列表空了显示提示，否则显示表格 */}
      {products.length === 0 ? (
        <p className="empty">暂无产品</p>
      ) : (
        <table className="product-table">
          <thead>
            <tr>
              <th>编号</th>
              <th>名称</th>
              <th>单价（元）</th>
              <th>库存</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            {visibleProducts.map((p) => (
              <tr key={p.id}>
                <td>{p.code}</td>
                <td>
                  {p.name}
                  {/* 条件 && A：条件成立才显示 A，否则什么都不显示 */}
                  {p.stock === 0 && <span className="out-of-stock">缺货</span>}
                </td>
                <td>{p.price.toLocaleString()}</td>
                {/* 库存少于 5 用红色提醒：三元表达式也可以用在属性上 */}
                <td className={p.stock < 5 ? 'low' : undefined}>{p.stock}</td>
                <td>
                  {/* 要把 p.id 传进去，所以包一层箭头函数，点击时才执行 */}
                  <button type="button" onClick={() => handleRestock(p.id)}>入库 +1</button>{' '}
                  {/* 库存为 0 时不能再出库 */}
                  <button type="button" onClick={() => handleShip(p.id)} disabled={p.stock === 0}>
                    出库 −1
                  </button>{' '}
                  <button type="button" onClick={() => handleDelete(p.id)}>删除</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      <PageFooter />
    </main>
  )
}

export default App
