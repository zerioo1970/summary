// src/pages/LoginPage.tsx
// 登录页：地址 /login。登录成功后回到列表
import { useState } from 'react'
import type { SubmitEvent } from 'react'
import { useNavigate } from 'react-router'
import { useAuth } from '../auth/AuthContext.ts'

export function LoginPage() {
  // 从 Context 里取出登录函数。LoginPage 和 AuthProvider 之间隔着 App、Routes 好几层，没有传任何 props
  const { login } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault()
    setSubmitting(true)
    setError('')
    try {
      await login(username, password)
      navigate('/')
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="product-form" onSubmit={handleSubmit}>
      <h2>登录</h2>
      <label>用户名 <input value={username} onChange={(e) => setUsername(e.target.value)} /></label>
      {/* type="password"：输入的内容显示成圆点 */}
      <label>密码 <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} /></label>
      <button type="submit" disabled={submitting}>
        {submitting ? '登录中…' : '登录'}
      </button>
      {error && <p className="error">登录失败：{error}</p>}
      <p className="subtitle">测试账号：admin / 123456（管理员），zhangsan / 123456（销售）</p>
    </form>
  )
}
