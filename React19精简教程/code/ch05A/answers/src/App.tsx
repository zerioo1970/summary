// src/App.tsx
// 两个练习都做完后的版本
import { initialProducts } from './data.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { PageFooter } from './components/PageFooter.tsx'

function App() {
  // 练习 2：先用 filter 挑出探头，再用 map 生成行
  const probes = initialProducts.filter((p) => p.category === '探头')

  return (
    <main>
      <PageHeader />
      <table className="product-table">
        <thead>
          <tr>
            <th>编号</th>
            <th>名称</th>
            {/* 练习 1：多一列分类 */}
            <th>分类</th>
            <th>单价（元）</th>
            <th>库存</th>
          </tr>
        </thead>
        <tbody>
          {/* map 把每个产品对象变成一行 <tr>；key 让 React 认出每一行是谁 */}
          {probes.map((p) => (
            <tr key={p.id}>
              <td>{p.code}</td>
              <td>{p.name}</td>
              <td>{p.category}</td>
              <td>{p.price.toLocaleString()}</td>
              <td>{p.stock}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <PageFooter />
    </main>
  )
}

export default App
