# 第 14 章 登录用户：Context 共享数据与调试工具

## 这一章解决什么问题

系统要知道「现在是谁在用」：页头显示当前用户的名字，没登录不能录入产品，只有管理员能看到「删除」按钮。「当前用户」页头要用、列表页要用、表格也要用，它们之间隔着好几层组件。按第 8 章的状态提升，就得从 `App` 一层层用 props 传下去，每一层都要加 prop，中间的组件自己根本用不到它。本章用 Context 解决这个问题，再介绍排查这类问题最常用的工具 React DevTools。

## 开始之前

- 上一章你已经会了：编辑页、`useNavigate`、把后端字段错误显示到输入框下面。
- 起点代码：仓库 `code/ch13/price-list`（不含练习答案）。两个终端分别运行 `npm run dev` 和 `npm run mock`。
- 模拟接口里有两个测试账号：`admin / 123456`（管理员），`zhangsan / 123456`（销售）。
- 预计耗时：60 分钟。

## 正文

### 14.1 Context：跨层级共享数据

**白话定义**：Context（上下文）是 React 提供的一种「广播」方式：在组件树的某一层放一个提供者（provider），给它一个值；它里面**任何一层**的组件都可以用 `useContext` 直接取到这个值，中间的组件不用传 props。值变了，所有用到它的组件都会重新渲染。

**业务场景**：「当前登录用户」几乎每个页面都要用：页头显示名字、列表页决定能不能录入、表格决定显不显示删除按钮、编辑页决定让不让进。把它放进 Context，哪个组件需要就自己取。

**反例（什么时候不该用）**：只有父子两层、或者只有一两个组件用的数据，用 props 传就好。Context 让数据来源变得不明显：看一个组件的 props 就知道它依赖什么，用了 Context 就得翻进代码里才看得到。所以 Context 只放「全局性」的、很多地方都要用的数据，比如当前用户、界面语言、主题颜色。产品列表这种业务数据，仍然按第 8、9 章的办法处理。

不用 Context 时，要把用户传给表格，需要这样一层层传：

| 组件 | 自己用不用 `user` | 为了传下去要加的 prop |
| --- | --- | --- |
| `App` | 不用 | 传给 `ListPage` |
| `ListPage` | 用（决定显不显示表单） | 传给 `ProductTable` |
| `ProductTable` | 用（决定显不显示删除） | |

页面越多、层级越深，这种「路过的 props」越多，改一处要改一串。

#### 🖼 图解

```mermaid
graph TB
    A["AuthProvider<br/>保管 user state"] --> B["App → Routes → ListPage<br/>中间各层不传 user"]
    B --> C["ProductTable<br/>useAuth() 直接取到 user"]
    A --> D["PageHeader<br/>useAuth() 直接取到 user"]
```

#### 💻 最小可运行代码

**① 用户类型。** 用下面的内容整个替换 `src/types.ts`，在末尾加了 `User` 类型：

```ts
// src/types.ts
// 产品数据的形状。全书共用这一个类型，以后数据从 JSON 文件来也是这个结构
export type Product = {
  id: number
  code: string
  name: string
  category: string
  price: number
  stock: number
}

// 登录用户。role：admin 是管理员，sales 是销售
export type User = {
  username: string
  name: string
  role: 'admin' | 'sales'
}
```

`role: 'admin' | 'sales'` 表示 `role` 只能是这两个字符串之一，写别的 TypeScript 会报错。

**② 登录接口。** 用下面的内容整个替换 `src/api.ts`，第一行 import 多了 `User`，末尾多了 `login` 函数：

```ts
// src/api.ts
// 所有「写」接口集中放在这里。页面和 Hook 只调用这些函数，不直接拼地址、写请求头
import type { Product, User } from './types.ts'

// 新增、修改时提交的内容：Product 去掉 id。id 由服务器分配，前端不能自己定
export type ProductInput = Omit<Product, 'id'>

const BASE = import.meta.env.VITE_API_BASE

// 接口失败时抛出的错误。比普通 Error 多带两样东西：HTTP 状态码，以及逐个字段的校验错误
export class ApiError extends Error {
  status: number
  // 例如 { code: '编号 P-001 已存在' }。没有字段错误时是空对象
  fieldErrors: Record<string, string>

  constructor(message: string, status: number, fieldErrors: Record<string, string>) {
    // super(...) 调用父类 Error 的构造函数，设置 message
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

// 把失败的响应转换成 ApiError。模拟接口出错时返回 { message, errors? }
async function toApiError(response: Response): Promise<ApiError> {
  try {
    const body: { message?: string; errors?: Record<string, string> } = await response.json()
    return new ApiError(body.message ?? `服务器返回 ${response.status}`, response.status, body.errors ?? {})
  } catch {
    // 响应内容不是 JSON（例如代理返回的 502 页面），只能用状态码
    return new ApiError(`服务器返回 ${response.status}`, response.status, {})
  }
}

export async function createProduct(input: ProductInput): Promise<Product> {
  const response = await fetch(`${BASE}/products`, {
    // 默认是 GET，新增要写明 POST
    method: 'POST',
    // 告诉服务器：请求体是 JSON。不写的话很多后端不知道怎么解析，会拒绝
    headers: { 'Content-Type': 'application/json' },
    // 请求体只能是字符串，把对象转成 JSON 文本
    body: JSON.stringify(input),
  })
  if (!response.ok) {
    throw await toApiError(response)
  }
  // 服务器返回保存后的产品，带上了它分配的 id
  const saved: Product = await response.json()
  return saved
}

export async function updateProduct(id: number, input: ProductInput): Promise<Product> {
  const response = await fetch(`${BASE}/products/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(input),
  })
  if (!response.ok) {
    throw await toApiError(response)
  }
  const saved: Product = await response.json()
  return saved
}

export async function deleteProduct(id: number): Promise<void> {
  const response = await fetch(`${BASE}/products/${id}`, { method: 'DELETE' })
  if (!response.ok) {
    throw await toApiError(response)
  }
  // 删除成功返回 204，没有内容，不要调用 response.json()
}

// 登录：用户名密码正确时返回用户信息（不含密码）；错误时服务器返回 401，这里抛出 ApiError
export async function login(username: string, password: string): Promise<User> {
  const response = await fetch(`${BASE}/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, password }),
  })
  if (!response.ok) {
    throw await toApiError(response)
  }
  const user: User = await response.json()
  return user
}
```

**③ 创建 Context。** 新建文件夹 `src/auth`，在里面新建 `AuthContext.ts`：

```ts
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
```

| 代码 | 说明 |
| --- | --- |
| `createContext<AuthValue \| null>(null)` | 创建一个 Context，尖括号里写它装的值的类型。参数是默认值，组件**不在**提供者里面时取到的就是它 |
| `useContext(AuthContext)` | 取出离自己最近的那个提供者给的值 |
| `useAuth()` | 包了一层自定义 Hook（第 8 章）：其他组件只要写 `useAuth()`，不用每次都 import `AuthContext` 再判断 `null` |

**④ 提供者组件。** 在 `src/auth` 里新建 `AuthProvider.tsx`：

```tsx
// src/auth/AuthProvider.tsx
// 提供者组件：自己保管「当前用户」这个 state，通过 AuthContext 提供给里面的所有组件
import { useState } from 'react'
import type { ReactNode } from 'react'
import * as api from '../api.ts'
import type { User } from '../types.ts'
import { AuthContext } from './AuthContext.ts'

type AuthProviderProps = {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  // 当前用户只存在浏览器内存里：刷新页面就回到「未登录」（练习 2 解决）
  const [user, setUser] = useState<User | null>(null)

  async function login(username: string, password: string) {
    // 用户名或密码错误时 api.login 抛出错误，这里不接住，交给登录页显示
    const loggedIn = await api.login(username, password)
    setUser(loggedIn)
  }

  function logout() {
    setUser(null)
  }

  // React 19 起，Context 对象本身就能当提供者用：<AuthContext value={...}>。
  // 以前的写法是 <AuthContext.Provider value={...}>，网上的资料大多是这种，两种效果相同
  return <AuthContext value={{ user, login, logout }}>{children}</AuthContext>
}
```

`AuthProvider` 是一个普通组件：它有自己的 `user` state，把 `user`、`login`、`logout` 打包成一个对象交给 `<AuthContext value={...}>`。`login` 成功后 `setUser`，`AuthProvider` 重新渲染，Context 的值变了，所有用到 `useAuth()` 的组件跟着更新。

为什么 Context 和提供者分成两个文件？`AuthContext.ts` 导出的是 Context 和 Hook，`AuthProvider.tsx` 只导出一个组件。如果放在同一个 `.tsx` 文件里，`npm run lint` 会警告 `react(only-export-components): Fast refresh only works when a file only exports components.`：一个文件里既有组件又有别的导出时，第 1 章的热更新没法只替换组件，会退回整页刷新。

**⑤ 放到最外层。** 用下面的内容整个替换 `src/main.tsx`，在 `BrowserRouter` 里面、`App` 外面包了 `AuthProvider`：

```tsx
// src/main.tsx
// 整个前端的入口：index.html 里的 <script> 只引用这一个文件
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router'
import { AuthProvider } from './auth/AuthProvider.tsx'
// CSS 也能 import：Vite 会把它变成 <style> 插进页面
import './index.css'
import App from './App.tsx'

// 找到 index.html 里的 <div id="root">，React 从此接管它里面的内容
// 末尾的 ! 告诉 TypeScript：这个元素一定存在（否则它认为可能是 null）
createRoot(document.getElementById('root')!).render(
  // StrictMode 只在开发时做额外检查，打包后的正式版本不受影响
  <StrictMode>
    {/* BrowserRouter 用浏览器地址栏的路径决定显示哪个页面；包在最外层，里面的组件才能用路由功能 */}
    {/* basename：网站部署在子路径（如 /price-list/）时，路由要去掉这个前缀再匹配。值来自 vite.config.ts 的 base */}
    <BrowserRouter basename={import.meta.env.BASE_URL}>
      {/* AuthProvider 包住 App：App 里面所有组件都能用 useAuth() 取到当前用户 */}
      <AuthProvider>
        <App />
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
)
```

`AuthProvider` 放在 `BrowserRouter` 外面还是里面？实测两种都能正常工作：`AuthProvider` 自己没有用路由功能，用 `useNavigate` 的是登录页，而登录页在两种写法下都在 `BrowserRouter` 里面。这里放在里面，以后如果想在 `AuthProvider` 里加「退出后跳到登录页」，可以直接用 `useNavigate`。

**⑥ 登录页。** 新建 `src/pages/LoginPage.tsx`：

```tsx
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
```

**⑦ 加路由。** 用下面的内容整个替换 `src/App.tsx`，import 和路由表各多了一行 `/login`：

```tsx
// src/App.tsx
// App 变成「外框」：页头、导航、页脚所有页面共用，中间按地址显示不同的页面
import { Link, Route, Routes } from 'react-router'
import { PageHeader } from './components/PageHeader.tsx'
import { PageFooter } from './components/PageFooter.tsx'
import { ListPage } from './pages/ListPage.tsx'
import { AboutPage } from './pages/AboutPage.tsx'
import { ProductDetailPage } from './pages/ProductDetailPage.tsx'
import { ProductEditPage } from './pages/ProductEditPage.tsx'
import { NotFoundPage } from './pages/NotFoundPage.tsx'
import { LoginPage } from './pages/LoginPage.tsx'

function App() {
  return (
    <main>
      <PageHeader />
      <nav className="nav">
        {/* Link 改地址但不刷新页面；React Router 发现地址变了，换掉下面 Routes 里显示的页面 */}
        <Link to="/">产品清单</Link> | <Link to="/about">关于</Link>
      </nav>
      {/* 路由表：React Router 找出与当前地址最匹配的那条 path，显示它的 element */}
      <Routes>
        <Route path="/" element={<ListPage />} />
        <Route path="/about" element={<AboutPage />} />
        <Route path="/login" element={<LoginPage />} />
        {/* :id 是参数，/products/1、/products/25 都匹配这一条 */}
        <Route path="/products/:id" element={<ProductDetailPage />} />
        {/* 编辑页：/products/3/edit */}
        <Route path="/products/:id/edit" element={<ProductEditPage />} />
        {/* * 匹配任何地址，但只有别的路由都对不上时才会用它，相当于兜底 */}
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
      <PageFooter />
    </main>
  )
}

export default App
```

**⑧ 页头显示当前用户。** 用下面的内容整个替换 `src/components/PageHeader.tsx`：

```tsx
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
```

`ROLE_NAMES[user.role]`：`user.role` 是 `'admin'` 时取到 `'管理员'`。TypeScript 知道 `role` 只有两种可能，`ROLE_NAMES` 里两种都有，所以不会报「取不到」。

#### 🔍 拆解运行结果

实测（这一节还没改表格，所以删除按钮暂时谁都能看到）：

| 操作 | 结果 |
| --- | --- |
| 打开首页 | 页头「超声事业部 · 内部报价参考 · 登录」 |
| 点「登录」，填 zhangsan 和错误密码，点登录 | 按钮先变成「登录中…」，约半秒后显示「登录失败：用户名或密码错误」，地址还是 `/login` |
| 改成正确密码 123456 登录 | 跳回 `/`，页头「张三（销售） 退出」 |
| 点「关于」 | 页头仍然是「张三（销售） 退出」：换了页面，`AuthProvider` 没有被移除，用户还在 |
| 按 `F5` 刷新 | 页头变回「登录」 |

刷新后为什么丢了？`user` 是 `AuthProvider` 里的 state，只存在于浏览器内存，刷新页面整个应用重新启动，state 回到初始值 `null`。练习 2 解决。

`LoginPage` 拿到 `login` 函数时，没有任何组件给它传 props：它和 `AuthProvider` 之间隔着 `App`、`Routes`，`useAuth()` 直接取到了。

#### 改坏一处试试

**① 去掉提供者。** 把 `main.tsx` 里的 `<AuthProvider>` 和 `</AuthProvider>` 删掉，只留 `<App />`。实测页面整片空白，控制台报：

```text
Uncaught Error: useAuth 必须在 AuthProvider 里面使用
```

这就是 `useAuth` 里那个 `if (value === null)` 的作用：`useContext` 取到了默认值 `null`，说明外面没有提供者。如果不检查，`user` 会悄悄变成「没登录」，你会以为是登录逻辑出了问题。改回去。

**② 不检查 `null`。** 把 `useAuth` 里那个 `if` 整段删掉，直接 `return value`，报：

```text
error TS2322: Type 'AuthValue | null' is not assignable to type 'AuthValue'.
```

**③ 提供的值少一样。** 把 `AuthProvider` 里的 `value={{ user, login, logout }}` 写成 `value={{ user, login }}`，报 `TS2741: Property 'logout' is missing in type ... but required in type 'AuthValue'.` 和 props 一样，Context 的值也按类型检查。

每次实验后改回。

### 14.2 按登录状态和角色显示界面

**白话定义**：有了 Context 里的 `user`，任何组件都可以按「有没有登录」「是什么角色」决定显示什么，写法就是第 5 章的条件渲染。

**业务场景**：没登录时不显示录入表单，提示「登录后才能录入新产品」；销售登录后能录入、能编辑，但看不到「删除」；管理员能看到「删除」。

**反例（什么时候不该用）**：**不要以为在界面上藏起按钮，就做好了权限控制。** 按钮藏起来了，接口还在。懂一点技术的人可以在浏览器控制台里直接调用接口（下面「改坏一处」会实测）。前端按角色显示界面，只是为了**好用**（不让用户看到点了也没用的按钮）；真正的权限必须由**后端**在每个接口里检查：请求来自谁、他有没有这个权限。

#### 🖼 图解

```mermaid
graph TB
    A["useAuth() 取到 user"] --> B["user 为 null<br/>不显示表单、不显示删除"]
    A --> C["user.role 是 sales<br/>显示表单、不显示删除"]
    A --> D["user.role 是 admin<br/>表单和删除都显示"]
```

#### 💻 最小可运行代码

用下面的内容整个替换 `src/components/ProductTable.tsx`。和第 13 章相比：多了 `useAuth` 的 import 和调用；「删除」按钮外面包了 `{isAdmin && (...)}`。

```tsx
// src/components/ProductTable.tsx
// 只负责显示表格。数据从 props 来；点了按钮不自己改数据，而是调用父组件给的函数「通知」它
import { Link } from 'react-router'
import type { Product } from '../types.ts'
import { useAuth } from '../auth/AuthContext.ts'
// 导入 CSS Module 得到一个对象：键是你写的类名，值是打包后真正使用的类名
import styles from './ProductTable.module.css'

type ProductTableProps = {
  products: Product[]
  // 回调 props：类型写清楚「调用时传什么参数、返回什么」。void 表示不关心返回值
  onDelete: (id: number) => void
  onRestock: (id: number) => void
}

export function ProductTable({ products, onDelete, onRestock }: ProductTableProps) {
  // 直接从 Context 取当前用户，ListPage 不用为了它多传一个 prop
  const { user } = useAuth()
  // 可选链：没登录时 user 是 null，user?.role 得到 undefined，不会报错
  const isAdmin = user?.role === 'admin'

  if (products.length === 0) {
    return <p className="empty">暂无产品</p>
  }

  return (
    <table className={styles.table}>
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
              {/* 点产品名进入详情页，地址形如 /products/3 */}
              <Link to={`/products/${p.id}`}>{p.name}</Link>
              {p.stock === 0 && <span className={styles.outOfStock}>缺货</span>}
            </td>
            <td>{p.price.toLocaleString()}</td>
            <td className={p.stock < 5 ? styles.low : undefined}>{p.stock}</td>
            <td>
              <button type="button" onClick={() => onRestock(p.id)}>入库 +1</button>{' '}
              {/* 只有管理员看得到删除按钮。注意：这只是界面上藏起来，真正的权限要靠后端检查 */}
              {isAdmin && (
                <>
                  <button type="button" onClick={() => onDelete(p.id)}>删除</button>{' '}
                </>
              )}
              {/* 编辑是换一个页面，所以用 Link，不是按钮 */}
              <Link to={`/products/${p.id}/edit`}>编辑</Link>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
```

用下面的内容整个替换 `src/pages/ListPage.tsx`。和第 13 章相比：多了 `useAuth` 的 import 和 `const { user } = useAuth()`；表单改成登录后才显示。

```tsx
// src/pages/ListPage.tsx
// 产品列表页：第 8 章 App 的内容原样搬过来，去掉页头页脚（它们留在 App 里，所有页面共用）
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useProducts } from '../hooks/useProducts.ts'
import { FilterBar } from '../components/FilterBar.tsx'
import { ProductTable } from '../components/ProductTable.tsx'
import { ProductForm } from '../components/ProductForm.tsx'
import { useAuth } from '../auth/AuthContext.ts'

export function ListPage() {
  // 加载和增删改的细节都在 useProducts 里，列表页只拿结果
  const { state, addProduct, deleteProduct, restockProduct } = useProducts()
  // 登录后才能录入新产品
  const { user } = useAuth()
  // 状态提升：搜索框和表格都要用 keyword，所以放在它们共同的父组件 App 里
  const [keyword, setKeyword] = useState('')
  // 删除、入库失败时显示的原因（例如「还有库存，不能删除」）
  const [actionError, setActionError] = useState('')

  // 表格只负责报告「点了哪一行」，调用接口、处理失败在这里做
  async function handleDelete(id: number) {
    setActionError('')
    try {
      await deleteProduct(id)
    } catch (err) {
      setActionError(`删除失败：${err instanceof Error ? err.message : String(err)}`)
    }
  }

  async function handleRestock(id: number) {
    setActionError('')
    try {
      await restockProduct(id)
    } catch (err) {
      setActionError(`入库失败：${err instanceof Error ? err.message : String(err)}`)
    }
  }

  const count = state.status === 'success' ? state.products.length : 0

  useEffect(() => {
    document.title = `产品价格清单（${count} 种）`
  }, [count])

  let content: ReactNode
  if (state.status === 'loading') {
    content = <p>加载中…</p>
  } else if (state.status === 'error') {
    content = <p className="error">加载失败：{state.message}</p>
  } else {
    // 筛选结果是派生值：由 products 和 keyword 现算，不另存 state
    const k = keyword.trim().toLowerCase()
    const visible = state.products.filter(
      (p) => p.name.toLowerCase().includes(k) || p.code.toLowerCase().includes(k),
    )
    // 传的是上面包了一层错误处理的函数
    content = <ProductTable products={visible} onDelete={handleDelete} onRestock={handleRestock} />
  }

  return (
    <section>
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar keyword={keyword} onKeywordChange={setKeyword} />
      {actionError && <p className="error">{actionError}</p>}
      {content}
      {user ? <ProductForm onSubmit={addProduct} /> : <p className="empty">登录后才能录入新产品</p>}
    </section>
  )
}
```

`ProductTable` 自己调用 `useAuth()`，`ListPage` 没有为它多传一个 prop。这就是 Context 省掉的那一层传递。

#### 🔍 拆解运行结果

实测：

| 状态 | 页头 | 删除按钮 | 录入表单 |
| --- | --- | --- | --- |
| 未登录 | 登录 | 0 个 | 显示「登录后才能录入新产品」 |
| 张三（销售） | 张三（销售） 退出 | 0 个 | 显示 |
| 点「退出」 | 登录 | 0 个 | 不显示 |
| 系统管理员（管理员） | 系统管理员（管理员） 退出 | 每行一个 | 显示 |

`user?.role === 'admin'` 用了第 12 章练习里的可选链：没登录时 `user` 是 `null`，`user?.role` 得到 `undefined`，比较结果是 `false`，不会报错。

#### 改坏一处试试

**① 角色名写错。** 把 `'admin'` 写成 `'administrator'`，报：

```text
error TS2367: This comparison appears to be unintentional because the types '"admin" | "sales" | undefined' and '"administrator"' have no overlap.
```

`User` 类型里把 `role` 写成了两个固定值的联合，拼错角色名在写代码时就会发现。如果写成 `role: string`，这个错误就漏过去了。

**② 绕过界面直接调接口。** 以张三（销售）登录，页面上没有删除按钮。按 `F12` 打开控制台，输入下面这行回车：

```js
await fetch('/api/products/5', { method: 'DELETE' }).then((r) => r.status)
```

实测返回 `204`，回到产品清单，「探头保护套」已经被删掉了。本教程的模拟接口没有做权限检查，所以谁都能删。真实系统里，后端要在删除接口里检查当前用户是不是管理员，不是就返回 403（Forbidden，禁止访问）。这需要登录时由后端发一个凭证（令牌 token 或 Cookie 会话），之后每个请求都带上它，后端据此认出是谁。这部分属于后端和安全设计，本教程不展开。做完实验重启模拟接口恢复数据。

### 14.3 React DevTools：看组件、props 和 state

> ⚠️ 本节的浏览器扩展安装和界面操作未实机验证（实测环境是无头浏览器，不能安装扩展）。面板名称和按钮位置以你安装的版本为准。

**白话定义**：React DevTools（React 开发者工具）是 React 官方提供的浏览器扩展。装好后，`F12` 开发者工具里会多出「⚛ Components」面板：以组件树的形式显示页面，选中一个组件，就能看到它此刻的 props、state、用到的 Context 值，还能直接修改它们看效果。

**业务场景**：页面上「删除按钮怎么没出来」。在 Components 面板选中 `ProductTable`，看它用到的 Context 里 `user` 是不是 `null`、`role` 是什么；选中 `ListPage`，看 `keyword`、`actionError` 这些 state 的当前值。不用到处加 `console.log`。

**反例（什么时候不该用）**：看**请求**有没有发出去、服务器返回了什么，不用 React DevTools，用浏览器自带的「网络（Network）」面板（第 7、11 章用过）。React DevTools 只管 React 组件本身。

#### 🖼 图解

```mermaid
graph TB
    A["F12 → Components 面板<br/>组件树"] --> B["选中 ProductTable"]
    B --> C["右侧显示 props：<br/>products、onDelete…"]
    B --> D["右侧显示 hooks：<br/>用到的 Context 值等"]
```

#### 💻 安装与使用

1. 在 Chrome 或 Edge 的扩展商店搜索「React Developer Tools」，安装 React 官方（Meta）发布的那个。
2. 打开 `http://localhost:5173/`，按 `F12`。开发者工具的标签页里多了「⚛ Components」和「⚛ Profiler」。没看到的话，点标签栏右边的 `»` 找一找，或者关掉开发者工具重新打开。
3. 点「⚛ Components」。左边是组件树：`App`、`AuthProvider`、`PageHeader`、`ListPage`、`ProductTable`……和你写的组件一一对应。
4. 点选 `ListPage`，右边能看到它的 hooks：`useProducts` 里的 state（加载状态和产品列表）、`keyword`、`actionError` 等。
5. 在搜索框里输入几个字，看右边 `keyword` 的值跟着变；点「入库 +1」，看产品列表里那一行的 `stock` 跟着变。

还有一个顺手的用法：左上角有个「选择元素」的图标，点它，再点页面上的某个按钮，组件树会跳到渲染这个按钮的组件。接手别人的代码、不知道某块界面是哪个文件写的时，这个功能很有用。

#### 🔍 你会看到什么

第 1 章开始，控制台里一直有一行提示：

```text
Download the React DevTools for a better development experience: https://react.dev/link/react-devtools
```

这行就是 React 在开发模式下建议你安装这个扩展。装好后这行提示消失（⚠️ 未实机验证）。

#### 改坏一处试试

在 Components 面板里选中 `AuthProvider`，找到它的 state（值是 `null` 或者当前用户对象），双击把它改成别的值（⚠️ 未实机验证：能否直接编辑对象类型的 state，取决于扩展版本）。页头和删除按钮会立刻跟着变化，但刷新页面后恢复，第二个终端里也不会有任何请求：这里改的只是浏览器内存里的 state。这再次说明，前端看到的「我是管理员」完全可以被用户自己改掉，权限只能由后端判断。

## ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 页面空白，控制台 `useAuth 必须在 AuthProvider 里面使用` | 用了 `useAuth()` 的组件不在 `AuthProvider` 里面 | 在 `main.tsx` 里用 `<AuthProvider>` 包住 `<App />` |
| `npm run lint` 警告 `only-export-components` | 一个 `.tsx` 文件里同时导出了组件和非组件（Context、Hook） | 拆成两个文件：Context 和 Hook 放 `.ts`，组件放 `.tsx` |
| 刷新页面后要重新登录 | 当前用户只存在内存里的 state | 登录后存进 `localStorage`，启动时读出来（练习 2）；真实系统通常由后端用 Cookie 会话或令牌保持登录 |
| 藏起了删除按钮，数据还是被销售删掉了 | 前端隐藏只是界面效果，接口本身没有权限检查 | 后端在每个接口里检查当前用户的权限 |
| 网上的代码写 `<AuthContext.Provider value={...}>` | React 19 之前的写法 | 两种写法效果相同，React 19 可以直接写 `<AuthContext value={...}>` |

## 🎯 动手练习

**练习 1：编辑页要求登录**

没登录时直接打开 `/products/3/edit`，不显示表单，显示「请先登录再编辑产品。」和「去登录」链接。

验收标准：

- 未登录打开 `/products/3/edit`：只看到提示和链接，没有表单。
- 点「去登录」登录后，再打开这个地址，表单正常显示编号 H-001。
- 在编辑页点页头的「退出」，表单立刻消失，换成提示。

提示：判断 `user` 后提前 `return` 时，要写在所有 Hook（`useParams`、`useProduct`、`useNavigate`、`useAuth`）调用**之后**，原因见第 4 章踩坑警告。

**练习 2：刷新后保持登录**

登录成功后把用户信息存进浏览器的 `localStorage`，页面打开时读出来作为初始值；退出时删掉。

验收标准：

- 用 admin 登录后按 `F5`：页头仍是「系统管理员（管理员） 退出」。
- 点「退出」再按 `F5`：页头是「登录」。

提示：`localStorage.setItem('键', '字符串')` 保存，`localStorage.getItem('键')` 读取（没有时返回 `null`），`localStorage.removeItem('键')` 删除。只能存字符串，对象要用 `JSON.stringify` / `JSON.parse` 转换。

卡住了看文末参考答案。

## 📌 一句话总结

全局数据放 Context，用 useAuth 随处取；界面按角色显示，权限必须后端检查。

## 下一章

后端一直是模拟接口。第 15 章换成 ASP.NET Core Minimal API，并把前端打包进后端，合成一个网站。

---

<!-- 以下是参考答案，先自己动手，卡住了再往下翻 -->































## 参考答案

### 练习 1

思路：编辑页自己 `useAuth()` 取当前用户，没登录就提前返回提示。所有 Hook 都在 `if` 前面调用，保证每次渲染调用的 Hook 数量和顺序都一样。退出时 Context 的值变了，编辑页重新渲染，走进 `if` 分支。

```tsx
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
```

### 练习 2

思路：只改 `AuthProvider`。`useState(loadSavedUser)` 传的是**函数本身**（没有括号），React 只在第一次显示时调用它取初始值；如果写成 `useState(loadSavedUser())`，每次渲染都会去读一遍 `localStorage`，虽然结果一样但白白浪费。

```tsx
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
```

实测：admin 登录后 `localStorage` 里存着 `{"username":"admin","name":"系统管理员","role":"admin"}`，刷新后仍是登录状态；退出后这一项被删除，刷新后回到「登录」。

注意：`localStorage` 里的内容用户可以在开发者工具里随意修改，比如把 `role` 改成 `admin`。这再次说明前端的「我是谁」不可信，权限必须由后端判断。
