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

  // 练习 2：每一行都生成新对象，所以 map 里不用判断 id
  function handleRestockAll() {
    setProducts(products.map((p) => ({ ...p, stock: p.stock + 10 })))
  }

  return (
    <main>
      <PageHeader />
      <p>
        <button type="button" onClick={handleRestockAll}>全部入库 +10</button>
      </p>
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
          {products.map((p) => (
            <tr key={p.id}>
              <td>{p.code}</td>
              <td>{p.name}</td>
              <td>{p.price.toLocaleString()}</td>
              <td>{p.stock}</td>
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
      <PageFooter />
    </main>
  )
}

export default App
