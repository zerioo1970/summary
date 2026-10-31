// src/pages/ProductEditPage.tsx
// 练习 1 之后的版本：没登录时不显示表单
// 编辑页：地址 /products/3/edit。先加载这个产品，加载完再显示表单；保存成功后回到列表
import { Link, useNavigate, useParams } from 'react-router'
import { useProduct } from '../hooks/useProduct.ts'
import { ProductForm } from '../components/ProductForm.tsx'
import { useAuth } from '../auth/AuthContext.ts'
import * as api from '../api.ts'
import type { ProductInput } from '../api.ts'

export function ProductEditPage() {
  const { id } = useParams()
  const state = useProduct(id)
  // useNavigate 返回一个函数，调用它就跳转到指定地址，效果和点 Link 一样
  const navigate = useNavigate()
  const { user } = useAuth()

  async function handleSubmit(input: ProductInput) {
    // 失败时 updateProduct 抛出 ApiError，这里不接住，交给表单显示
    const saved = await api.updateProduct(Number(id), input)
    navigate('/')
    return saved
  }

  // 练习 1：没登录就提前返回。注意要写在所有 Hook 调用之后（第 4 章：Hook 不能写在 return 后面）
  if (!user) {
    return (
      <section>
        <p className="error">请先登录再编辑产品。</p>
        <p>
          <Link to="/login">去登录</Link>
        </p>
      </section>
    )
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
