// src/auth/AuthContext.ts
// 「当前登录用户」的 Context：任何组件都能直接取到，不用一层层用 props 往下传
import { createContext, useContext } from 'react'
import type { User } from '../types.ts'

// Context 里放的东西：当前用户（没登录是 null），以及登录、退出两个函数
export type AuthValue = {
  user: User | null
  login: (username: string, password: string) => Promise<void>
  logout: () => void
}

// 创建 Context。参数是默认值：组件不在 AuthProvider 里面时取到的就是它。用 null 表示「没有提供者」
export const AuthContext = createContext<AuthValue | null>(null)

// 自定义 Hook：组件里写 const { user } = useAuth() 就能拿到当前用户
export function useAuth(): AuthValue {
  const value = useContext(AuthContext)
  // 取到 null 说明忘了在外层包 AuthProvider。立刻报错，比悄悄当成「没登录」更容易发现问题
  if (value === null) {
    throw new Error('useAuth 必须在 AuthProvider 里面使用')
  }
  return value
}
