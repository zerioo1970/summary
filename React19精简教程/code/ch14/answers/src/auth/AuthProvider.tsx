// src/auth/AuthProvider.tsx
// 练习 2 之后的版本：登录状态存进 localStorage，刷新页面不丢
// 提供者组件：自己保管「当前用户」这个 state，通过 AuthContext 提供给里面的所有组件
import { useState } from 'react'
import type { ReactNode } from 'react'
import * as api from '../api.ts'
import type { User } from '../types.ts'
import { AuthContext } from './AuthContext.ts'

type AuthProviderProps = {
  children: ReactNode
}

// localStorage 里用的键名
const STORAGE_KEY = 'price-list-user'

// 页面打开时读取上次保存的用户。没有保存过时 getItem 返回 null
function loadSavedUser(): User | null {
  const text = localStorage.getItem(STORAGE_KEY)
  if (text === null) {
    return null
  }
  // JSON.parse 的结果是 any，马上标注类型，不让 any 往下传
  const saved: User = JSON.parse(text)
  return saved
}

export function AuthProvider({ children }: AuthProviderProps) {
  // 给 useState 传一个函数（而不是值）：React 只在第一次显示时调用它，之后每次渲染不会再去读 localStorage
  const [user, setUser] = useState<User | null>(loadSavedUser)

  async function login(username: string, password: string) {
    // 用户名或密码错误时 api.login 抛出错误，这里不接住，交给登录页显示
    const loggedIn = await api.login(username, password)
    // localStorage 只能存字符串，对象要转成 JSON
    localStorage.setItem(STORAGE_KEY, JSON.stringify(loggedIn))
    setUser(loggedIn)
  }

  function logout() {
    localStorage.removeItem(STORAGE_KEY)
    setUser(null)
  }

  // React 19 起，Context 对象本身就能当提供者用：<AuthContext value={...}>。
  // 以前的写法是 <AuthContext.Provider value={...}>，网上的资料大多是这种，两种效果相同
  return <AuthContext value={{ user, login, logout }}>{children}</AuthContext>
}
