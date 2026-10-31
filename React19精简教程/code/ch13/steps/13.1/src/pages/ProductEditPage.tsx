// src/pages/ProductEditPage.tsx（13.1 版本，13.2 节会再改）
// 编辑页：地址 /products/3/edit。先加载这个产品，加载完再显示表单
import { Link, useParams } from 'react-router'
import { useProduct } from '../hooks/useProduct.ts'
import { ProductForm } from '../components/ProductForm.tsx'
import * as api from '../api.ts'
import type { ProductInput } from '../api.ts'

export function ProductEditPage() {
  const { id } = useParams()
  const state = useProduct(id)

  async function handleSubmit(input: ProductInput) {
    // 调用修改接口。失败时 updateProduct 抛出错误，这里不接住，交给表单显示
    return await api.updateProduct(Number(id), input)
  }

  return (
    <section>
      <p>
        <Link to="/">← 返回列表</Link>
      </p>
      {state.status === 'loading' && <p>加载中…</p>}
      {state.status === 'error' && <p className="error">加载失败：{state.message}</p>}
      {/* 加载成功后才显示表单：表单的 useState 只在第一次显示时读取 initial，必须那时数据已经到了 */}
      {state.status === 'success' && <ProductForm title="编辑产品" initial={state.product} onSubmit={handleSubmit} />}
    </section>
  )
}
