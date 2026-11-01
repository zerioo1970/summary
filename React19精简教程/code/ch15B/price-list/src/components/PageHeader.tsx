// src/components/PageHeader.tsx
// 页面顶部：标题、说明，右边显示当前用户
import { Link } from 'react-router'
import { useAuth } from '../auth/AuthContext.ts'

// 角色在数据里是英文，显示给用户看时换成中文
const ROLE_NAMES = { admin: '管理员', sales: '销售' }

export function PageHeader() {
  const { user, logout } = useAuth()

  return (
    <header>
      <h1>产品价格清单</h1>
      <p className="subtitle">
        超声事业部 · 内部报价参考 ·{' '}
        {user ? (
          <>
            {user.name}（{ROLE_NAMES[user.role]}）{' '}
            <button type="button" onClick={logout}>退出</button>
          </>
        ) : (
          <Link to="/login">登录</Link>
        )}
      </p>
    </header>
  )
}
