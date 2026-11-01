# 第 12C 章 提交中与失败状态：保存中禁用按钮，失败显示原因

## 这一章解决什么问题

保存要等服务器响应，这期间页面没有任何提示，用户可能以为没点上又点一次，结果同一个产品提交了两次。编号重复、还有库存不能删时，`api.ts` 抛出了错误，但只出现在控制台里，用户对着没反应的页面发呆。本章把「保存中」和「失败了」显示出来。

本章只讲一个技术点：**提交中与失败状态**（`try / catch / finally`）。

## 开始之前

- 上一章你已经会了：`await` 之后用更新函数改 state。
- 起点代码：仓库 `code/ch12B/price-list`（不含练习答案）。两个终端分别运行开发服务器和 `npm run mock`。
- 预计耗时：30 分钟。

## 正文

### 12C.1 提交中与失败状态是什么

**白话定义**：一次保存有三个阶段：等待中、成功、失败。「等待中」要让用户看到并防止重复提交（按钮禁用、文字变成「保存中…」）；「失败」要把服务器给的原因显示出来，并保留用户填的内容。实现上就是两个 state：`submitting`（是否正在提交）和 `submitError`（失败原因），再配合 `try / catch / finally`。

**业务场景**：网络慢时，用户点了「保存」没反应，又点了一次，结果同一个产品提交了两次；编号重复时，用户要知道为什么没保存成功，而不是对着没反应的页面发呆。

**反例（什么时候不该用）**：不要用 `alert()` 弹窗显示每个错误。弹窗会挡住页面、必须点掉才能继续，用户也没法一边看错误一边改表单。错误就近显示在表单或列表上方。确认删除这种「需要用户做决定」的场合可以用浏览器自带的确认框（练习 1）。

#### 🖼 图解

```mermaid
graph TB
    A["点保存<br/>submitting = true<br/>按钮禁用、显示保存中…"] --> B["await onAdd(input)"]
    B --> C["成功：显示已录入<br/>清空输入框"]
    B --> D["失败（catch）：<br/>显示失败原因，保留输入"]
    C --> E["finally：<br/>submitting = false"]
    D --> E
```

`try` 里的代码抛出错误时，跳到 `catch`；`finally` 里的代码**不管成功还是失败都会执行**，恢复按钮的代码写在这里，就不会出现「失败后按钮一直是灰的」。

#### 💻 最小可运行代码

**① 用下面的内容整个替换 `src/components/ProductForm.tsx`。** 和第 12A 章相比：多了 `submitting`、`submitError` 两个 state；`await onAdd(input)` 包进了 `try / catch / finally`；按钮加了 `disabled={submitting}` 和「保存中…」；下面多了失败提示。

```tsx
// src/components/ProductForm.tsx
import { useState } from 'react'
import type { ChangeEvent, SubmitEvent } from 'react'
import type { Product } from '../types.ts'
import type { ProductInput } from '../api.ts'

type ProductFormProps = {
  // 保存时把填好的内容交给父组件，父组件负责调用接口。返回 Promise：表单要等它完成，才知道成功还是失败
  onAdd: (input: ProductInput) => Promise<Product>
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
  // 正在等服务器响应。这期间禁用保存按钮，防止重复提交
  const [submitting, setSubmitting] = useState(false)
  // 服务器返回的失败原因
  const [submitError, setSubmitError] = useState('')

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

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    // 阻止浏览器默认的「提交表单 = 刷新整页」，改由我们自己处理
    e.preventDefault()
    setTriedSubmit(true)
    if (!isValid) {
      setMessage('')
      return
    }
    // 新产品还没入库，库存为 0。不再自己生成 id，由服务器分配
    const input: ProductInput = { code: code.trim(), name: name.trim(), category, price: priceNumber, stock: 0 }
    setSubmitting(true)
    setSubmitError('')
    setMessage('')
    try {
      const saved = await onAdd(input)
      setMessage(`已录入：${saved.name}（编号 ${saved.code}，id ${saved.id}）`)
      // 保存成功才清空输入框；失败时保留，方便用户改了再提交
      setCode('')
      setName('')
      setPrice('')
      setTriedSubmit(false)
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : String(err))
    } finally {
      // 不管成功还是失败，都要恢复按钮。写在 finally 里就不会漏
      setSubmitting(false)
    }
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
      <button type="submit" disabled={submitting}>
        {submitting ? '保存中…' : '保存'}
      </button>
      {message && <p className="success">{message}</p>}
      {submitError && <p className="error">保存失败：{submitError}</p>}
    </form>
  )
}
```

**② 用下面的内容整个替换 `src/pages/ListPage.tsx`。** 表格的删除、入库失败也要显示：多了 `actionError` 这个 state 和 `handleDelete`、`handleRestock` 两个函数，它们调用 `useProducts` 给的函数并接住错误；传给表格的换成这两个函数；表格上方多了错误提示。

```tsx
// src/pages/ListPage.tsx
// 产品列表页：第 8C 章 App 的内容原样搬过来，去掉页头页脚（它们留在 App 里，所有页面共用）
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useProducts } from '../hooks/useProducts.ts'
import { FilterBar } from '../components/FilterBar.tsx'
import { ProductTable } from '../components/ProductTable.tsx'
import { ProductForm } from '../components/ProductForm.tsx'

export function ListPage() {
  // 加载和增删改的细节都在 useProducts 里，列表页只拿结果
  const { state, addProduct, deleteProduct, restockProduct } = useProducts()
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
      <ProductForm onAdd={addProduct} />
    </section>
  )
}
```

`ProductTable` 的 `onDelete` 类型还是 `(id: number) => void`，传进去的 `handleDelete` 返回 `Promise<void>`，TypeScript 允许：`void` 表示「不关心返回值」，返回什么都可以。

#### 🔍 拆解运行结果

重启模拟接口后实测：

| 操作 | 页面 |
| --- | --- |
| 录入 P-003 点保存，0.15 秒时 | 按钮变成灰色的「保存中…」，点不动 |
| 约半秒后 | 按钮恢复「保存」，提示「已录入：微凸探头 C8-4（编号 P-003，id 6）」，输入框清空，表格多一行 |
| 再录入编号 P-003、名称「重复」保存 | 红字「保存失败：编号 P-003 已存在」，输入框里的 P-003 和「重复」**保留着**，改一下编号就能再提交 |
| 「凸阵探头 C5-2」点删除 | 表格上方红字「删除失败：凸阵探头 C5-2 还有库存 6，不能删除」，这一行还在 |
| 停掉模拟接口，再删除、保存 | 分别显示「删除失败：服务器返回 502」「保存失败：服务器返回 502」 |

502 时为什么只显示状态码？代理返回的错误内容不是 JSON，`failMessage` 里的 `response.json()` 解析失败，走到 `catch`，只能用状态码。

#### 改坏一处试试

**① 去掉按钮的 `disabled={submitting}`。** 录入 P-007 后**快速点两次**保存。实测第二个终端出现两次 `POST /api/products`：第一次 201 保存成功，第二次 400。页面上同时显示「已录入：双击测试」和「保存失败：编号 P-007 已存在」。这次幸好编号重复被后端拦住了；如果是没有唯一性检查的数据（比如一条留言、一笔订单），就会真的保存两份。改回去。

**② 去掉请求头。** 删掉 `api.ts` 里 `createProduct` 的 `headers: { 'Content-Type': 'application/json' },` 那一行，保存一个新产品。实测显示「保存失败：请求头 Content-Type 必须是 application/json」，状态码 415（Unsupported Media Type，不支持的内容类型）。请求体虽然是 JSON 文本，但没有请求头说明，服务器不知道该按什么格式解析。真实后端（比如 Spring Boot）大多也会这样拒绝。改回去。

**③ 忘了 `await`。** 把 `ListPage` 里 `handleDelete` 的 `await deleteProduct(id)` 去掉 `await`，再删除「凸阵探头 C5-2」。实测表格上方**没有任何提示**，控制台里却出现了未处理的错误「凸阵探头 C5-2 还有库存 6，不能删除」。

没有 `await`，`deleteProduct(id)` 只是「发出去」就继续往下走了，`try` 块立刻结束；等半秒后错误抛出来时，早已不在 `try` 里，`catch` 接不到。TypeScript 和 `npm run lint` 都不报这个错，只能靠习惯：**调用返回 Promise 的函数，前面写 `await`。** 改回去。

## ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 保存失败了，页面没有任何提示；控制台里有未处理的错误 | 错误抛出了但没人接住：没写 `try / catch`，或者调用时忘了 `await` | 调用接口的地方用 `try { await ... } catch (err) { 显示错误 }` |
| 网络慢时同一条数据保存了两次 | 提交期间按钮没有禁用，用户点了两次 | 用 `submitting` state 禁用按钮，成功或失败后在 `finally` 里恢复 |
| 失败后保存按钮一直是灰的 | 恢复 `submitting` 的代码写在 `try` 的末尾，出错时跳过了 | 写在 `finally` 里 |
| 保存失败后，用户填的内容全没了 | 清空输入框的代码写在了 `finally` 里，失败时也执行了 | 清空只在成功时做，写在 `try` 里 `await` 的后面 |

## 🎯 动手练习

**练习 1：删除前确认**

点「删除」时先弹出浏览器自带的确认框「确定删除「探头保护套」吗？」，点「取消」什么都不发生，点「确定」才调用接口。

验收标准：

- 点「取消」：模拟接口的终端没有出现 `DELETE`，表格不变。
- 点「确定」：出现 `DELETE /api/products/5`，这一行消失。

提示：`window.confirm('文字')` 弹出确认框，点「确定」返回 `true`，点「取消」返回 `false`。确认框会暂停页面上的一切，这里正好需要用户做决定。

**练习 2：表格里的「处理中」**

点某一行的「删除」或「入库 +1」后，在服务器响应前，这一行的两个按钮都禁用，并在按钮后面显示「处理中…」。

验收标准：

- 「耦合剂 250ml」点「入库 +1」后立刻看：这一行显示「处理中…」，两个按钮是灰的；约半秒后恢复，库存加 1。
- 其他行的按钮不受影响。
- 第 12B 章练习 2 的「同一行快速点两次只加 1」不会再发生。

提示：在 `ListPage` 里用一个 `useState<number | null>(null)` 记住正在处理的那一行的 id，传给 `ProductTable`。`number | null` 表示「要么是数字，要么是 null（没有）」。

卡住了看文末参考答案。

## 📌 一句话总结

保存中禁用按钮，失败在 catch 里显示原因，恢复按钮写在 finally 里。

## 下一章

现在只能「库存加 1」这样改。第 13A 章做编辑页，修改产品的任意字段。

---

<!-- 以下是参考答案，先自己动手，卡住了再往下翻 -->
































## 参考答案

两个练习都改 `ListPage.tsx`，练习 2 还要改 `ProductTable.tsx`。下面是都做完后的版本。

### 练习 1

思路：在调用接口之前先 `window.confirm`，返回 `false` 就直接 `return`。为了在提示里显示产品名，先从列表里找出这个产品；`target?.name` 是可选链（optional chaining）：`target` 是 `undefined` 时整个表达式得到 `undefined`，不会报错，`?? id` 再退回显示 id。

### 练习 2

思路：「正在处理哪一行」只有一个值，用 `busyId` 记住；开始时设成这一行的 id，在 `finally` 里设回 `null`。表格按 `busyId === p.id` 决定这一行的按钮是否禁用。

```tsx
// src/pages/ListPage.tsx
// 两个练习都做完后的版本
// 产品列表页：第 8C 章 App 的内容原样搬过来，去掉页头页脚（它们留在 App 里，所有页面共用）
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useProducts } from '../hooks/useProducts.ts'
import { FilterBar } from '../components/FilterBar.tsx'
import { ProductTable } from '../components/ProductTable.tsx'
import { ProductForm } from '../components/ProductForm.tsx'

export function ListPage() {
  // 加载和增删改的细节都在 useProducts 里，列表页只拿结果
  const { state, addProduct, deleteProduct, restockProduct } = useProducts()
  // 状态提升：搜索框和表格都要用 keyword，所以放在它们共同的父组件 App 里
  const [keyword, setKeyword] = useState('')
  // 删除、入库失败时显示的原因（例如「还有库存，不能删除」）
  const [actionError, setActionError] = useState('')
  // 练习 2：正在处理的那一行的 id，没有就是 null
  const [busyId, setBusyId] = useState<number | null>(null)

  // 表格只负责报告「点了哪一行」，调用接口、处理失败在这里做
  async function handleDelete(id: number) {
    // 练习 1：先确认。confirm 弹出浏览器自带的确认框，点「确定」返回 true，点「取消」返回 false
    const target = state.status === 'success' ? state.products.find((p) => p.id === id) : undefined
    if (!window.confirm(`确定删除「${target?.name ?? id}」吗？`)) {
      return
    }
    setActionError('')
    setBusyId(id)
    try {
      await deleteProduct(id)
    } catch (err) {
      setActionError(`删除失败：${err instanceof Error ? err.message : String(err)}`)
    } finally {
      setBusyId(null)
    }
  }

  async function handleRestock(id: number) {
    setActionError('')
    setBusyId(id)
    try {
      await restockProduct(id)
    } catch (err) {
      setActionError(`入库失败：${err instanceof Error ? err.message : String(err)}`)
    } finally {
      setBusyId(null)
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
    content = <ProductTable products={visible} busyId={busyId} onDelete={handleDelete} onRestock={handleRestock} />
  }

  return (
    <section>
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar keyword={keyword} onKeywordChange={setKeyword} />
      {actionError && <p className="error">{actionError}</p>}
      {content}
      <ProductForm onAdd={addProduct} />
    </section>
  )
}
```

```tsx
// src/components/ProductTable.tsx
// 练习 2 之后的版本：多了 busyId
// 只负责显示表格。数据从 props 来；点了按钮不自己改数据，而是调用父组件给的函数「通知」它
import { Link } from 'react-router'
import type { Product } from '../types.ts'
// 导入 CSS Module 得到一个对象：键是你写的类名，值是打包后真正使用的类名
import styles from './ProductTable.module.css'

type ProductTableProps = {
  products: Product[]
  // 正在处理的那一行。这一行的按钮禁用，避免重复点击
  busyId: number | null
  // 回调 props：类型写清楚「调用时传什么参数、返回什么」。void 表示不关心返回值
  onDelete: (id: number) => void
  onRestock: (id: number) => void
}

export function ProductTable({ products, busyId, onDelete, onRestock }: ProductTableProps) {
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
              <button type="button" disabled={busyId === p.id} onClick={() => onRestock(p.id)}>
                入库 +1
              </button>{' '}
              <button type="button" disabled={busyId === p.id} onClick={() => onDelete(p.id)}>
                删除
              </button>
              {busyId === p.id && ' 处理中…'}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
```

实测：点「取消」没有发出请求；点「确定」后这一行先显示「处理中…」、按钮变灰，约半秒后消失。
