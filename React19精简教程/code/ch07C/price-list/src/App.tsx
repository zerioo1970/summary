// src/App.tsx
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { Product } from './types.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { PageFooter } from './components/PageFooter.tsx'
import { ProductForm } from './components/ProductForm.tsx'

function App() {
  // 数据一开始是空的，加载回来再填进去
  const [products, setProducts] = useState<Product[]>([])
  // 加载中、错误信息各用一个 state。第 7D 章会把这三个合成一个
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  // 依赖数组为空 []：只在组件第一次显示到页面上之后执行一次
  useEffect(() => {
    // 组件被移除后（或开发模式下第一次被 StrictMode 拆掉后）不再更新 state
    let ignore = false

    async function load() {
      try {
        // 模拟慢网络，便于看到「加载中」。正式代码删掉这一行
        await new Promise((resolve) => setTimeout(resolve, 1000))
        // 以 / 开头：从网站根目录取，对应项目里的 public/products.json
        const response = await fetch('/products.json')
        if (!response.ok) {
          throw new Error(`服务器返回 ${response.status}`)
        }
        // response.json() 的类型是 Promise<any>，马上标注成 Product[]，不让 any 往下传
        const data: Product[] = await response.json()
        if (!ignore) {
          setProducts(data)
          setLoading(false)
        }
      } catch (err) {
        if (!ignore) {
          // catch 到的东西类型是 unknown，确认是 Error 才能读 message
          setError(err instanceof Error ? err.message : String(err))
          setLoading(false)
        }
      }
    }

    console.log('effect 执行：开始加载')
    load()

    // 清理函数：下次执行这个 effect 之前、或组件被移除时，React 会调用它
    return () => {
      ignore = true
    }
  }, [])

  useEffect(() => {
    document.title = `产品价格清单（${products.length} 种）`
  }, [products.length])

  function handleDelete(id: number) {
    setProducts(products.filter((p) => p.id !== id))
  }

  function handleRestock(id: number) {
    setProducts(products.map((p) => (p.id === id ? { ...p, stock: p.stock + 1 } : p)))
  }

  // 情况多时，在 return 之前用 if 算好要显示的内容，JSX 里只放结果
  let content: ReactNode
  if (loading) {
    content = <p>加载中…</p>
  } else if (error !== '') {
    content = <p className="error">加载失败：{error}</p>
  } else if (products.length === 0) {
    content = <p className="empty">暂无产品</p>
  } else {
    content = (
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
              <td>
                {p.name}
                {p.stock === 0 && <span className="out-of-stock">缺货</span>}
              </td>
              <td>{p.price.toLocaleString()}</td>
              <td className={p.stock < 5 ? 'low' : undefined}>{p.stock}</td>
              <td>
                <button type="button" onClick={() => handleRestock(p.id)}>入库 +1</button>{' '}
                <button type="button" onClick={() => handleDelete(p.id)}>删除</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    )
  }

  return (
    <main>
      <PageHeader />
      <p>共 {products.length} 种产品</p>
      {content}
      <ProductForm />
      <PageFooter />
    </main>
  )
}

export default App
