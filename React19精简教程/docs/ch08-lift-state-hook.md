# 第 8 章 组件之间共享数据与自定义 Hook

## 这一章解决什么问题

表单录入的产品要出现在表格里，但产品列表在 `App` 里，表单在 `ProductForm` 里，表单够不着 `App` 的 state。表格上方还要加一个搜索框，搜索框和表格又是两个组件。`App` 已经一百多行，取数代码和页面代码混在一起。本章解决这三件事。

## 开始之前

- 上一章你已经会了：`useEffect` 和依赖数组、用判别联合表示加载状态。
- 起点代码：仓库 `code/ch07/price-list`。在 `D:\react-study\price-list` 执行 `npm run dev`。预计耗时：50 分钟。

## 正文

### 8.1 回调 props：子组件通知父组件

**白话定义**：回调 props（callback props）是父组件通过 props 传给子组件的**函数**。子组件不自己改数据，而是在用户操作时调用这个函数，把「发生了什么」告诉父组件，由父组件去改 state。

**业务场景**：把表格拆成 `ProductTable` 组件后，「删除」按钮在表格里，产品数据在 `App` 里。`App` 传 `onDelete` 给表格，表格里点删除时调用 `onDelete(p.id)`。表单也一样：保存成功时调用 `onAdd(product)`。

**反例（什么时候不该用）**：子组件自己用、别人不关心的数据，不要为了「统一管理」都提到父组件再用回调改。比如第 4 章 `QuantityPicker` 的询价数量，只有卡片自己用，就留在它自己的 state 里。

#### 🖼 图解

```mermaid
graph TB
    A["App<br/>持有 products state"] -->|"props：products、onDelete"| B["ProductTable"]
    B -->|"用户点删除：<br/>调用 onDelete(3)"| C["App 的 handleDelete(3)<br/>setState 生成新数组"]
    C -->|重新渲染，传新 products| B
```

数据往下传（props），事件往上报（回调）。数据始终只在 `App` 一个地方改。

#### 💻 最小可运行代码

**① 新建 `src/components/ProductTable.tsx`**，把第 7 章 `App` 里的表格搬过来：

```tsx
// src/components/ProductTable.tsx
// 只负责显示表格。数据从 props 来；点了按钮不自己改数据，而是调用父组件给的函数「通知」它
import type { Product } from '../types.ts'

type ProductTableProps = {
  products: Product[]
  // 回调 props：类型写清楚「调用时传什么参数、返回什么」。void 表示不关心返回值
  onDelete: (id: number) => void
  onRestock: (id: number) => void
}

export function ProductTable({ products, onDelete, onRestock }: ProductTableProps) {
  if (products.length === 0) {
    return <p className="empty">暂无产品</p>
  }

  return (
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
              <button type="button" onClick={() => onRestock(p.id)}>入库 +1</button>{' '}
              <button type="button" onClick={() => onDelete(p.id)}>删除</button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
```

**② 改 `src/components/ProductForm.tsx`**。用下面的内容整个替换。和第 6 章相比改了两处：`export function ProductForm` 上面多了 `ProductFormProps` 类型，函数改成接收 `{ onAdd }`；`handleSubmit` 里原来的 `console.log('新产品', product)` 换成了 `onAdd(product)`。

```tsx
// src/components/ProductForm.tsx
import { useState } from 'react'
import type { ChangeEvent, SubmitEvent } from 'react'
import type { Product } from '../types.ts'

type ProductFormProps = {
  // 保存成功时把新产品交给父组件。表单不知道、也不关心父组件怎么存
  onAdd: (product: Product) => void
}

export function ProductForm({ onAdd }: ProductFormProps) {
  // 每个输入框对应一个 state。输入框里的内容永远是字符串，单价也先按字符串存
  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [category, setCategory] = useState('探头')
  const [price, setPrice] = useState('')
  // 保存成功后显示的提示
  const [message, setMessage] = useState('')
  // 点过一次「保存」才显示错误，免得一打开页面就满屏红字
  const [triedSubmit, setTriedSubmit] = useState(false)

  // 派生值：每次渲染都从 state 现算，不另存 state，所以永远和输入框一致
  const priceNumber = Number(price)
  const codeError = code.trim() === '' ? '请填写编号' : ''
  const nameError = name.trim() === '' ? '请填写名称' : ''
  // Number('abc') 是 NaN，NaN > 0 不成立，所以「不是数字」和「小于等于 0」一起拦住
  const priceError = price.trim() === '' ? '请填写单价' : priceNumber > 0 ? '' : '单价必须是大于 0 的数字'
  const isValid = codeError === '' && nameError === '' && priceError === ''

  // 单独写成函数时，参数类型要写明：input 元素上的变化事件
  function handlePriceChange(e: ChangeEvent<HTMLInputElement>) {
    setPrice(e.target.value)
  }

  function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    // 阻止浏览器默认的「提交表单 = 刷新整页」，改由我们自己处理
    e.preventDefault()
    setTriedSubmit(true)
    if (!isValid) {
      setMessage('')
      return
    }
    // 新产品还没入库，库存为 0；id 先用当前时间的毫秒数，保证不重复
    const product: Product = { id: Date.now(), code: code.trim(), name: name.trim(), category, price: priceNumber, stock: 0 }
    onAdd(product)
    setMessage(`已录入：${product.name}，单价 ${product.price.toLocaleString()} 元`)
    // 清空输入框，并收起错误提示
    setCode('')
    setName('')
    setPrice('')
    setTriedSubmit(false)
  }

  return (
    <form className="product-form" onSubmit={handleSubmit}>
      <h2>录入新产品</h2>
      {/* value 来自 state，onChange 把用户输入写回 state：输入框完全由 React 控制 */}
      <label>编号 <input value={code} onChange={(e) => setCode(e.target.value)} /></label>
      {triedSubmit && codeError && <span className="error">{codeError}</span>}
      <label>名称 <input value={name} onChange={(e) => setName(e.target.value)} /></label>
      {triedSubmit && nameError && <span className="error">{nameError}</span>}
      <label>
        分类{' '}
        <select value={category} onChange={(e) => setCategory(e.target.value)}>
          <option value="探头">探头</option>
          <option value="主机">主机</option>
          <option value="耗材">耗材</option>
        </select>
      </label>
      <label>单价 <input value={price} onChange={handlePriceChange} /></label>
      {triedSubmit && priceError && <span className="error">{priceError}</span>}
      {/* type="submit"：点击或在输入框里按回车都会触发表单的 onSubmit */}
      <button type="submit">保存</button>
      {message && <p className="success">{message}</p>}
    </form>
  )
}
```

**③ 改 `src/App.tsx`**。用下面的内容整个替换。和第 7 章相比：

- import 区多了 `ProductTable`；
- 多了 `handleAdd` 函数，用 `[...state.products, product]` 把新产品加到数组末尾（第 5 章表格里「添加」那一行）；
- 原来那一大段 `<table>` 和 `else if (state.products.length === 0)` 分支都删掉了（表格和「暂无产品」已搬进 `ProductTable`），换成一行 `<ProductTable ... />`；
- `<ProductForm />` 变成 `<ProductForm onAdd={handleAdd} />`。

```tsx
// src/App.tsx（8.1 版本，8.2 节会再改）
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { Product } from './types.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { ProductTable } from './components/ProductTable.tsx'
import { ProductForm } from './components/ProductForm.tsx'
import { PageFooter } from './components/PageFooter.tsx'

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; products: Product[] }

function App() {
  const [state, setState] = useState<LoadState>({ status: 'loading' })

  useEffect(() => {
    let ignore = false

    async function load() {
      try {
        // 模拟慢网络，便于看到「加载中」。正式代码删掉这一行
        await new Promise((resolve) => setTimeout(resolve, 1000))
        const response = await fetch('/products.json')
        if (!response.ok) {
          throw new Error(`服务器返回 ${response.status}`)
        }
        const products: Product[] = await response.json()
        if (!ignore) {
          setState({ status: 'success', products })
        }
      } catch (err) {
        if (!ignore) {
          setState({ status: 'error', message: err instanceof Error ? err.message : String(err) })
        }
      }
    }

    load()
    return () => {
      ignore = true
    }
  }, [])

  const count = state.status === 'success' ? state.products.length : 0

  useEffect(() => {
    document.title = `产品价格清单（${count} 种）`
  }, [count])

  function handleAdd(product: Product) {
    if (state.status !== 'success') return
    // 添加：展开旧数组，在末尾放新产品，得到一个新数组
    setState({ status: 'success', products: [...state.products, product] })
  }

  function handleDelete(id: number) {
    if (state.status !== 'success') return
    setState({ status: 'success', products: state.products.filter((p) => p.id !== id) })
  }

  function handleRestock(id: number) {
    if (state.status !== 'success') return
    setState({
      status: 'success',
      products: state.products.map((p) => (p.id === id ? { ...p, stock: p.stock + 1 } : p)),
    })
  }

  let content: ReactNode
  if (state.status === 'loading') {
    content = <p>加载中…</p>
  } else if (state.status === 'error') {
    content = <p className="error">加载失败：{state.message}</p>
  } else {
    // 回调 props：把「删除」「入库」的处理函数交给表格，表格里点按钮时调用它们
    content = <ProductTable products={state.products} onDelete={handleDelete} onRestock={handleRestock} />
  }

  return (
    <main>
      <PageHeader />
      <p>共 {count} 种产品</p>
      {content}
      {/* 表单保存成功时调用 handleAdd，新产品就进了 App 的 state */}
      <ProductForm onAdd={handleAdd} />
      <PageFooter />
    </main>
  )
}

export default App
```

#### 🔍 拆解运行结果

等表格加载出来，在表单里填「P-003」「微凸探头 C8-4」「9800」保存。实测：表格最后多出一行「P-003 微凸探头 C8-4 9,800 0」，名称后带「缺货」（新产品库存为 0）；「共 6 种产品」，标签页标题变成「（6 种）」。点这一行的「删除」，它又消失了。

| 代码 | 说明 |
| --- | --- |
| `onDelete: (id: number) => void` | 函数类型：调用时传一个数字，不关心返回值（`void`）。TypeScript 会检查父组件传进来的函数是否符合 |
| `onClick={() => onDelete(p.id)}` | 表格里点按钮时，调用父组件给的函数，并告诉它是哪一行 |
| `onDelete={handleDelete}` | 父组件把自己的处理函数传下去。传的是函数本身，没有括号 |
| 名字以 `on` / `handle` 开头 | 惯例：回调 props 叫 `onXxx`（「发生了 Xxx 时」），父组件里的处理函数叫 `handleXxx`（「处理 Xxx」） |
| `[...state.products, product]` | 展开旧数组，末尾加一项，生成新数组。和第 5 章一样，不能用 `push` 改原数组 |

整个过程按时间顺序是：用户点「删除」→ 表格调用 `onDelete(5)` → 实际执行的是 `App` 的 `handleDelete(5)` → `setState` 存入新数组 → `App` 重新渲染，把新数组作为 `products` 传给表格 → 表格少了一行。

`ProductTable` 只管显示和报告事件，不知道数据存在哪。以后数据改成存到服务器，表格一行都不用改。

#### 改坏一处试试

不传 `onAdd`（写 `<ProductForm />`），报 `TS2741: Property 'onAdd' is missing in type '{}' but required in type 'ProductFormProps'.`。传一个参数类型不对的函数 `onDelete={(p: string) => console.log(p)}`，报 `TS2322: Type '(p: string) => void' is not assignable to type '(id: number) => void'.`

### 8.2 状态提升

**白话定义**：状态提升（lifting state up）是指：两个兄弟组件要用同一份数据时，把 state 移到它们**最近的共同父组件**里，再通过 props 分别传下去。

**业务场景**：搜索框 `FilterBar` 里输入「探头」，表格 `ProductTable` 只显示名称或编号里含「探头」的行。关键字 `keyword` 两个组件都要用，所以放在它们共同的父组件 `App` 里。

**反例（什么时候不该用）**：只有一个组件用的 state 不要提升。提得越高，这个 state 一变，重新渲染的组件就越多，代码也要多传一层 props。比如表单里正在输入的编号、名称，只有表单用，就留在 `ProductForm` 里。

#### 🖼 图解

```mermaid
graph TB
    A["App<br/>keyword state"] -->|"keyword、onKeywordChange"| B["FilterBar 搜索框"]
    A -->|"按 keyword 筛选后的 products"| C["ProductTable 表格"]
    B -->|"输入时调用<br/>onKeywordChange"| A
```

#### 💻 最小可运行代码

新建 `src/components/FilterBar.tsx`：

```tsx
// src/components/FilterBar.tsx
// 搜索框。它自己不存关键字：值从父组件传进来，用户输入时调用 onKeywordChange 通知父组件
type FilterBarProps = {
  keyword: string
  onKeywordChange: (keyword: string) => void
}

export function FilterBar({ keyword, onKeywordChange }: FilterBarProps) {
  return (
    <label className="filter-bar">
      搜索 <input value={keyword} placeholder="编号或名称" onChange={(e) => onKeywordChange(e.target.value)} />
    </label>
  )
}
```

在 `src/index.css` 末尾加一行：

```css
.filter-bar { display: block; margin-bottom: 8px; }
```

然后用下面的内容整个替换 `src/App.tsx`。和 8.1 相比改了四处：import 区多了 `FilterBar`；多了 `keyword` 这个 state；`else` 分支里先筛选出 `visible` 再传给表格；`{content}` 上面多了 `<FilterBar ... />`。

```tsx
// src/App.tsx（8.2 版本，8.3 节会再改）
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { Product } from './types.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { FilterBar } from './components/FilterBar.tsx'
import { ProductTable } from './components/ProductTable.tsx'
import { ProductForm } from './components/ProductForm.tsx'
import { PageFooter } from './components/PageFooter.tsx'

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; products: Product[] }

function App() {
  const [state, setState] = useState<LoadState>({ status: 'loading' })
  // 状态提升：搜索框和表格都要用 keyword，所以放在它们共同的父组件 App 里
  const [keyword, setKeyword] = useState('')

  useEffect(() => {
    let ignore = false

    async function load() {
      try {
        // 模拟慢网络，便于看到「加载中」。正式代码删掉这一行
        await new Promise((resolve) => setTimeout(resolve, 1000))
        const response = await fetch('/products.json')
        if (!response.ok) {
          throw new Error(`服务器返回 ${response.status}`)
        }
        const products: Product[] = await response.json()
        if (!ignore) {
          setState({ status: 'success', products })
        }
      } catch (err) {
        if (!ignore) {
          setState({ status: 'error', message: err instanceof Error ? err.message : String(err) })
        }
      }
    }

    load()
    return () => {
      ignore = true
    }
  }, [])

  const count = state.status === 'success' ? state.products.length : 0

  useEffect(() => {
    document.title = `产品价格清单（${count} 种）`
  }, [count])

  function handleAdd(product: Product) {
    if (state.status !== 'success') return
    // 添加：展开旧数组，在末尾放新产品，得到一个新数组
    setState({ status: 'success', products: [...state.products, product] })
  }

  function handleDelete(id: number) {
    if (state.status !== 'success') return
    setState({ status: 'success', products: state.products.filter((p) => p.id !== id) })
  }

  function handleRestock(id: number) {
    if (state.status !== 'success') return
    setState({
      status: 'success',
      products: state.products.map((p) => (p.id === id ? { ...p, stock: p.stock + 1 } : p)),
    })
  }

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
    content = <ProductTable products={visible} onDelete={handleDelete} onRestock={handleRestock} />
  }

  return (
    <main>
      <PageHeader />
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar keyword={keyword} onKeywordChange={setKeyword} />
      {content}
      <ProductForm onAdd={handleAdd} />
      <PageFooter />
    </main>
  )
}

export default App
```

筛选那几行用到的字符串方法：

| 写法 | 作用 | 例子 |
| --- | --- | --- |
| `trim()` | 去掉首尾空格 | `' 探头 '.trim()` 得到 `'探头'` |
| `toLowerCase()` | 转成小写，让搜索不区分大小写 | `'C-001'.toLowerCase()` 得到 `'c-001'` |
| `includes(k)` | 是否包含 `k`；`k` 是空字符串时总是 `true`，所以搜索框为空时显示全部 | `'凸阵探头'.includes('探头')` 得到 `true` |

#### 🔍 拆解运行结果

实测：

| 搜索框输入 | 表格显示 |
| --- | --- |
| `探头` | 凸阵探头 C5-2、线阵探头 L12-5、探头保护套 |
| `c-00`（小写） | 耦合剂 250ml、探头保护套（编号 C-001、C-002） |
| `xyz` | 暂无产品 |

「共 5 种产品」不随搜索变化：它用完整的 `state.products`，表格用筛选后的 `visible`。`onKeywordChange={setKeyword}` 能直接传，因为 `setKeyword` 本身就是「接收一个字符串」的函数，正好符合 `(keyword: string) => void`。

#### 改坏一处试试

写成 `onKeywordChange={setKeyword('')}`，报 `TS2322: Type 'void' is not assignable to type '(keyword: string) => void'.` 和第 4 章一样：要传函数本身，不是调用结果。

### 8.3 自定义 Hook

**白话定义**：自定义 Hook（custom hook）是一个名字以 `use` 开头、内部调用了其他 Hook 的普通函数。它把一段「state + effect + 修改函数」打包起来，组件调用它就得到这套逻辑。

**业务场景**：第 9 章的产品详情页也要加载 `products.json`。把加载和增删改的代码从 `App` 里抽成 `useProducts()`，列表页、详情页各调用一次就行，`App` 也只剩页面结构。

**反例（什么时候不该用）**：自定义 Hook 共享的是**逻辑**，不是**数据**。两个组件各调用一次 `useProducts()`，就是两份独立的 state、两次请求，一边删了产品另一边不会变（下面「改坏一处」实测）。要多个组件看到同一份数据，用 8.2 的状态提升。另外，不包含任何 Hook 的工具函数（比如格式化金额）写成普通函数，不要加 `use` 前缀。

#### 🖼 图解

```mermaid
graph TB
    A["useProducts()<br/>state + 加载 effect<br/>+ 增删改函数"] --> B["返回对象<br/>state、addProduct 等"]
    B --> C["App 解构拿到需要的部分<br/>只写页面结构"]
```

#### 💻 最小可运行代码

新建文件夹 `src/hooks`，在里面新建 `useProducts.ts`（没有 JSX，所以是 `.ts`）。把第 7 章 `App` 里的 `LoadState`、加载 effect、增删改函数搬进来：

```ts
// src/hooks/useProducts.ts
// 自定义 Hook：把「加载产品 + 增删改」这一整套逻辑打包，任何组件调用它都能用
import { useEffect, useState } from 'react'
import type { Product } from '../types.ts'

export type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'success'; products: Product[] }

// 名字必须以 use 开头：React 和 lint 工具靠这个识别它是 Hook，按 Hook 的规则检查
export function useProducts() {
  const [state, setState] = useState<LoadState>({ status: 'loading' })

  useEffect(() => {
    let ignore = false

    async function load() {
      try {
        // 模拟慢网络，便于看到「加载中」。正式代码删掉这一行
        await new Promise((resolve) => setTimeout(resolve, 1000))
        const response = await fetch('/products.json')
        if (!response.ok) {
          throw new Error(`服务器返回 ${response.status}`)
        }
        const products: Product[] = await response.json()
        if (!ignore) {
          setState({ status: 'success', products })
        }
      } catch (err) {
        if (!ignore) {
          setState({ status: 'error', message: err instanceof Error ? err.message : String(err) })
        }
      }
    }

    load()
    return () => {
      ignore = true
    }
  }, [])

  function addProduct(product: Product) {
    if (state.status !== 'success') return
    // 添加：展开旧数组，在末尾放新产品，得到一个新数组
    setState({ status: 'success', products: [...state.products, product] })
  }

  function deleteProduct(id: number) {
    if (state.status !== 'success') return
    setState({ status: 'success', products: state.products.filter((p) => p.id !== id) })
  }

  function restockProduct(id: number) {
    if (state.status !== 'success') return
    setState({
      status: 'success',
      products: state.products.map((p) => (p.id === id ? { ...p, stock: p.stock + 1 } : p)),
    })
  }

  // 返回一个对象，调用方按名字取自己需要的部分
  return { state, addProduct, deleteProduct, restockProduct }
}
```

用下面的内容整个替换 `src/App.tsx`：

```tsx
// src/App.tsx
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useProducts } from './hooks/useProducts.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { FilterBar } from './components/FilterBar.tsx'
import { ProductTable } from './components/ProductTable.tsx'
import { ProductForm } from './components/ProductForm.tsx'
import { PageFooter } from './components/PageFooter.tsx'

function App() {
  // 加载和增删改的细节都在 useProducts 里，App 只拿结果
  const { state, addProduct, deleteProduct, restockProduct } = useProducts()
  // 状态提升：搜索框和表格都要用 keyword，所以放在它们共同的父组件 App 里
  const [keyword, setKeyword] = useState('')

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
    // 把 useProducts 给的函数直接作为回调传下去
    content = <ProductTable products={visible} onDelete={deleteProduct} onRestock={restockProduct} />
  }

  return (
    <main>
      <PageHeader />
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar keyword={keyword} onKeywordChange={setKeyword} />
      {content}
      <ProductForm onAdd={addProduct} />
      <PageFooter />
    </main>
  )
}

export default App
```

#### 🔍 拆解运行结果

页面行为和 8.2 完全一样，`npm run lint` 显示 `Found 0 warnings and 0 errors.`。`App` 从一百多行变成五十行，只描述「页面由什么组成」。

这次重构把 8.2 的 `App` 拆成了两部分：

| 原来在 8.2 `App` 里的代码 | 现在在哪 | 名字变化 |
| --- | --- | --- |
| `type LoadState = ...` | `useProducts.ts`，并加了 `export`，第 9 章别的文件要用 | 不变 |
| `useState<LoadState>` 和加载数据的 `useEffect` | `useProducts.ts` | 不变 |
| `handleAdd`、`handleDelete`、`handleRestock` | `useProducts.ts` | 改成 `addProduct`、`deleteProduct`、`restockProduct`：它们现在是「数据的操作」，不再是某个页面的事件处理 |
| `keyword`、筛选、标题 `useEffect`、JSX | 留在 `App` | 不变 |

`const { state, addProduct, deleteProduct, restockProduct } = useProducts()` 是对象解构：`useProducts()` 返回一个有 4 个字段的对象，这一行按名字把它们取出来，变成 4 个变量。

自定义 Hook 里的 `useState`、`useEffect` 就像直接写在调用它的组件里一样：state 属于调用它的那个组件（这里是 `App`），Hook 的调用规则（最顶层、不能放在 `if` 里）对它同样适用。

#### 改坏一处试试

**① 调用两次，看是不是同一份数据。** 在 `App` 里 `keyword` 那行下面临时加 `const other = useProducts()`，并在「共 x 种产品」下面加 `<p>另一份：{other.state.status === 'success' ? other.state.products.length : 0} 种</p>`。刷新后删除「耦合剂」，实测显示：

```text
共 4 种产品
另一份：5 种
```

「网络」里的 `products.json` 请求也从 2 次变成 4 次：两份完全独立的 state。实验完删掉这两行。

**② 在不以 `use` 开头的函数里调用 Hook。** 在 `App` 里写一个 `function loadCount() { return useProducts() }`，`tsc` 不报错，但 `npm run lint` 报 `React Hook "useProducts" is called in function "loadCount" that is neither a React function component nor a custom React Hook function.`，并提示 Hook 的名字必须以 `use` 开头。

## ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 两个组件都调用了 `useProducts()`，一边删了产品另一边没变 | 自定义 Hook 只共享逻辑，每次调用都是独立的 state | 在共同的父组件里调用一次，把结果用 props 传下去 |
| 子组件里改了 props 里的数组（如 `products.splice(...)`），页面不更新或乱掉 | 数据归父组件所有，子组件改的是父组件的原数组，React 不知道数据变了 | 子组件只调用回调（`onDelete(id)`），由父组件生成新数组 |
| 搜索框能输入，表格却不筛选 | 只在 `FilterBar` 里存了关键字，`App` 拿不到；或者 `App` 里筛选后仍把 `state.products` 传给了表格 | 关键字放在 `App`；表格要传筛选后的 `visible` |
| 新增的产品刷新后就没了 | 数据只在浏览器内存的 state 里，`products.json` 是只读的静态文件 | 正常现象。要持久保存，需要后端接口把数据写进数据库 |

## 🎯 动手练习

**练习 1：按分类筛选**

在搜索框后面加分类下拉框（全部 / 探头 / 主机 / 耗材），和关键字同时生效。分类的 state 也放在 `App`。

验收标准：

- 选「耗材」：只显示耦合剂 250ml、探头保护套。
- 再在搜索框输入「保护」：只剩探头保护套。
- 选回「全部」、清空搜索框：5 行都回来。

**练习 2：写一个 `useDocumentTitle` Hook**

新建 `src/hooks/useDocumentTitle.ts`，导出 `useDocumentTitle(title: string)`，把 `App` 里改标题的 `useEffect` 换成一行 `useDocumentTitle(...)`。

验收标准：

- 刷新后标题是「产品价格清单（5 种）」，删除一行后变成「（4 种）」。
- `App.tsx` 里不再有 `useEffect`，`npm run lint` 和 `npm run build` 都通过。

卡住了看文末参考答案。

## 📌 一句话总结

共享的 state 放共同父组件，子组件用回调报事件，可复用的逻辑抽成 useXxx。

## 下一章

所有内容都挤在一个页面上。第 9 章加路由，点产品名进入详情页，地址栏跟着变。

---

<!-- 以下是参考答案，先自己动手，卡住了再往下翻 -->































## 参考答案

### 练习 1

思路：分类和关键字一样是 `FilterBar` 与表格共用的数据，所以提升到 `App`。筛选条件用 `&&` 组合：分类为「全部」或等于产品分类，并且关键字匹配。

```tsx
// src/components/FilterBar.tsx
// 练习 1：多了分类下拉框。两个值都由父组件保管，这里只负责显示和通知
type FilterBarProps = {
  keyword: string
  onKeywordChange: (keyword: string) => void
  category: string
  onCategoryChange: (category: string) => void
}

export function FilterBar({ keyword, onKeywordChange, category, onCategoryChange }: FilterBarProps) {
  return (
    <div className="filter-bar">
      <label>
        搜索 <input value={keyword} placeholder="编号或名称" onChange={(e) => onKeywordChange(e.target.value)} />
      </label>{' '}
      <label>
        分类{' '}
        <select value={category} onChange={(e) => onCategoryChange(e.target.value)}>
          <option value="全部">全部</option>
          <option value="探头">探头</option>
          <option value="主机">主机</option>
          <option value="耗材">耗材</option>
        </select>
      </label>
    </div>
  )
}
```

### 练习 2

思路：把 `useEffect` 原样搬进一个 `use` 开头的函数，要变化的部分（标题文字）作为参数传进来，并写进依赖数组。

```ts
// src/hooks/useDocumentTitle.ts
// 练习 2：把「随数据更新标签页标题」抽成自定义 Hook，任何页面一行就能用
import { useEffect } from 'react'

export function useDocumentTitle(title: string) {
  useEffect(() => {
    document.title = title
  }, [title])
}
```

两个练习都做完后的 `App.tsx`：

```tsx
// src/App.tsx
// 两个练习都做完后的版本
import { useState } from 'react'
import type { ReactNode } from 'react'
import { useProducts } from './hooks/useProducts.ts'
import { useDocumentTitle } from './hooks/useDocumentTitle.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { FilterBar } from './components/FilterBar.tsx'
import { ProductTable } from './components/ProductTable.tsx'
import { ProductForm } from './components/ProductForm.tsx'
import { PageFooter } from './components/PageFooter.tsx'

function App() {
  // 加载和增删改的细节都在 useProducts 里，App 只拿结果
  const { state, addProduct, deleteProduct, restockProduct } = useProducts()
  // 状态提升：搜索框和表格都要用 keyword，所以放在它们共同的父组件 App 里
  const [keyword, setKeyword] = useState('')
  // 练习 1：分类筛选同样提升到 App
  const [category, setCategory] = useState('全部')

  const count = state.status === 'success' ? state.products.length : 0

  // 练习 2：原来的 useEffect 换成一行自定义 Hook
  useDocumentTitle(`产品价格清单（${count} 种）`)

  let content: ReactNode
  if (state.status === 'loading') {
    content = <p>加载中…</p>
  } else if (state.status === 'error') {
    content = <p className="error">加载失败：{state.message}</p>
  } else {
    // 筛选结果是派生值：由 products、keyword、category 现算，不另存 state
    const k = keyword.trim().toLowerCase()
    const visible = state.products.filter(
      (p) =>
        (category === '全部' || p.category === category) &&
        (p.name.toLowerCase().includes(k) || p.code.toLowerCase().includes(k)),
    )
    // 把 useProducts 给的函数直接作为回调传下去
    content = <ProductTable products={visible} onDelete={deleteProduct} onRestock={restockProduct} />
  }

  return (
    <main>
      <PageHeader />
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar
        keyword={keyword}
        onKeywordChange={setKeyword}
        category={category}
        onCategoryChange={setCategory}
      />
      {content}
      <ProductForm onAdd={addProduct} />
      <PageFooter />
    </main>
  )
}

export default App
```
