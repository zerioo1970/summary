# 第 12A 章 写请求：新增、删除、入库保存到接口

## 这一章解决什么问题

第 8A 章的新增、删除、入库只改了浏览器里的 state，刷新页面就没了。第 12 章让这三个操作真正调用接口：数据保存在服务器上，刷新、换电脑打开都还在。分三步：本章发出写请求；第 12B 章修掉一个只有异步操作才会出现的 bug；第 12C 章处理「保存中」和「失败了」。

本章只讲一个技术点：**写请求**（带上方法、请求头和请求体的 `fetch`）。

## 开始之前

- 上一章你已经会了：启动模拟接口、用开发代理连接 `/api`、用 `.env` 配置接口前缀。
- 起点代码：仓库 `code/ch11D/price-list`（不含练习答案）。
- 两个终端都要运行：一个开发服务器，一个 `npm run mock`。第 12 章会多次「想从头再来」，在模拟接口的终端按 `Ctrl + C` 再 `npm run mock`，数据就恢复成初始的 5 个产品。
- 预计耗时：35 分钟。

## 正文

### 12A.1 写请求是什么

**白话定义**：`fetch` 默认发 GET 请求，只取数据。要让服务器保存数据，第二个参数里写明三样东西：`method`（请求方法：新增 `POST`、修改 `PUT`、删除 `DELETE`）、`headers`（请求头，告诉服务器请求体是什么格式）、`body`（请求体，要保存的内容，必须是字符串）。

**业务场景**：录入新产品时，把表单内容 `POST` 到 `/api/products`，服务器分配 id 后返回保存好的产品；点「入库 +1」时，把库存加 1 后的产品 `PUT` 到 `/api/products/4`；点「删除」时 `DELETE /api/products/5`。

**反例（什么时候不该用）**：不要在每个组件里各写一遍 `fetch(...)` 和请求头。地址、请求头、错误处理散落在各处，后端改一个规则就要找遍全项目。本节把所有写请求放进一个文件 `src/api.ts`，组件只调用 `createProduct(...)` 这样的函数。

| 操作 | 方法 | 地址 | 请求体 | 成功时返回 |
| --- | --- | --- | --- | --- |
| 新增 | `POST` | `/api/products` | 不含 id 的产品 | 201 + 带 id 的新产品 |
| 修改 | `PUT` | `/api/products/4` | 修改后的完整产品（不含 id） | 200 + 修改后的产品 |
| 删除 | `DELETE` | `/api/products/5` | 无 | 204，没有内容 |

#### 🖼 图解

```mermaid
graph TB
    A["表单内容（对象）<br/>{ code, name, ... }"] --> B["JSON.stringify<br/>变成 JSON 文本"]
    B --> C["fetch POST /api/products<br/>Content-Type: application/json"]
    C --> D["服务器保存，分配 id<br/>返回 201 + 新产品"]
    D --> E["response.json()<br/>拿到带 id 的产品"]
```

### 12A.2 api.ts 和三个保存函数

#### 💻 最小可运行代码

**① 新建 `src/api.ts`**，放三个写接口的函数：

```ts
// src/api.ts
// 所有「写」接口集中放在这里。页面和 Hook 只调用这些函数，不直接拼地址、写请求头
import type { Product } from './types.ts'

// 新增、修改时提交的内容：Product 去掉 id。id 由服务器分配，前端不能自己定
export type ProductInput = Omit<Product, 'id'>

const BASE = import.meta.env.VITE_API_BASE

// 失败时从响应里取出给用户看的文字。模拟接口出错时返回 { message, errors? }
async function failMessage(response: Response): Promise<string> {
  try {
    const body: { message?: string; errors?: Record<string, string> } = await response.json()
    // 有逐个字段的错误时（例如编号重复），把它们连起来显示，比「校验失败」更具体
    if (body.errors) {
      return Object.values(body.errors).join('；')
    }
    return body.message ?? `服务器返回 ${response.status}`
  } catch {
    // 响应内容不是 JSON（例如代理返回的 502 页面），只能显示状态码
    return `服务器返回 ${response.status}`
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
    throw new Error(await failMessage(response))
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
    throw new Error(await failMessage(response))
  }
  const saved: Product = await response.json()
  return saved
}

export async function deleteProduct(id: number): Promise<void> {
  const response = await fetch(`${BASE}/products/${id}`, { method: 'DELETE' })
  if (!response.ok) {
    throw new Error(await failMessage(response))
  }
  // 删除成功返回 204，没有内容，不要调用 response.json()
}
```

这个文件里有几样新写法：

| 写法 | 含义 |
| --- | --- |
| `Omit<Product, 'id'>` | TypeScript 自带的类型工具：取 `Product` 的所有字段，**去掉** `id`。新增时 id 由服务器分配，前端提交的内容里不该有它 |
| `JSON.stringify(input)` | 把对象转成 JSON 文本。`body` 只接受字符串等少数几种类型，不接受普通对象 |
| `Record<string, string>` | 「键是字符串、值也是字符串」的对象，用来描述 `errors: { code: '编号 P-003 已存在' }` 这种结构 |
| `Object.values(body.errors).join('；')` | 取出对象的所有值组成数组，再用「；」连成一句话 |
| `throw new Error(...)` | 主动抛出错误。调用方用 `try / catch` 接住（第 12C 章） |
| `Promise<void>` | 这个 `async` 函数完成时没有返回值 |

**② 用下面的内容整个替换 `src/hooks/useProducts.ts`。** 加载部分和第 11D 章一样；`addProduct`、`deleteProduct`、`restockProduct` 三个函数改成先调用接口、成功后再改 state：

```ts
// src/hooks/useProducts.ts
// 自定义 Hook：把「加载产品 + 增删改」这一整套逻辑打包，任何组件调用它都能用
import { useEffect, useState } from 'react'
import type { Product } from '../types.ts'
// 把 api.ts 导出的所有东西收进一个叫 api 的对象，用 api.createProduct 这样调用，不和下面同名的函数混淆
import * as api from '../api.ts'
import type { ProductInput } from '../api.ts'

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
        // 接口地址的前半段来自 .env 文件里的 VITE_API_BASE，换环境只改配置，不改代码
        const response = await fetch(`${import.meta.env.VITE_API_BASE}/products`)
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

  // 下面三个函数先调用接口，接口成功后再改 state。接口失败时抛出的错误不在这里处理，交给调用方显示
  async function addProduct(input: ProductInput): Promise<Product> {
    const saved = await api.createProduct(input)
    if (state.status !== 'success') return saved
    // 添加：服务器返回的产品带着它分配的 id，把它加到列表末尾
    setState({ status: 'success', products: [...state.products, saved] })
    return saved
  }

  async function deleteProduct(id: number): Promise<void> {
    await api.deleteProduct(id)
    if (state.status !== 'success') return
    setState({ status: 'success', products: state.products.filter((p) => p.id !== id) })
  }

  async function restockProduct(id: number): Promise<void> {
    if (state.status !== 'success') return
    const current = state.products.find((p) => p.id === id)
    if (!current) return
    // 把库存加 1 后的完整内容发给服务器，用服务器返回的结果替换这一行
    const saved = await api.updateProduct(id, { ...current, stock: current.stock + 1 })
    setState({ status: 'success', products: state.products.map((p) => (p.id === id ? saved : p)) })
  }

  // 返回一个对象，调用方按名字取自己需要的部分
  return { state, addProduct, deleteProduct, restockProduct }
}
```

`import * as api from '../api.ts'` 把 `api.ts` 导出的所有函数收进一个名叫 `api` 的对象。这里 Hook 自己也有一个 `deleteProduct`，写成 `api.deleteProduct` 一眼就能看出调用的是接口。每个函数都是「先 `await` 接口，成功后再 `setState`」：接口失败时 `await` 那一行就抛出错误，后面的 `setState` 不会执行，页面不会显示一个服务器上并不存在的结果。

**③ 用下面的内容整个替换 `src/components/ProductForm.tsx`。** 和第 6C、8A 章相比改了三处：`onAdd` 的类型变了（交出不含 id 的内容，拿回服务器保存好的产品）；`handleSubmit` 前面加了 `async`，里面 `await onAdd(input)`；成功提示里显示服务器分配的 id。

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
    // 等接口返回保存后的产品。await 只能写在 async 函数里，所以 handleSubmit 前面加了 async
    const saved = await onAdd(input)
    setMessage(`已录入：${saved.name}（编号 ${saved.code}，id ${saved.id}）`)
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

`ListPage.tsx` 这一节不用改：它把 `useProducts()` 给的 `addProduct` 传给表单、把 `deleteProduct`、`restockProduct` 传给表格，函数名没变。

#### 🔍 拆解运行结果

实测：

| 操作 | 页面 | 第二个终端（模拟接口） |
| --- | --- | --- |
| 录入「P-003 微凸探头 C8-4 9800」保存 | 约半秒后表格末尾多一行 P-003，提示「已录入：微凸探头 C8-4（编号 P-003，id 6）」 | `POST /api/products` |
| 刷新页面 | P-003 **还在** | `GET /api/products` |
| 「耦合剂 250ml」点「入库 +1」 | 约半秒后库存 120 变成 121；刷新后还是 121 | `PUT /api/products/4` |
| 「探头保护套」（库存 0）点「删除」 | 约半秒后这一行消失 | `DELETE /api/products/5` |

数据现在保存在模拟接口里。刷新浏览器只是重新 `GET` 一次，拿到的是服务器上的最新数据。id 6 是服务器分配的：现有最大 id 加 1。

注意有个半秒的空档：点了「入库 +1」，数字要等服务器响应后才变。这期间页面没有任何提示，用户可能以为没点上又点一次。第 12C 章解决。

再试两个会失败的操作：

- 再录入一次编号 P-003：页面**没有任何变化**，输入框也没清空。控制台里有两条报错：`Failed to load resource: the server responded with a status of 400 (Bad Request)`，以及一条未处理的错误，内容是「编号 P-003 已存在」。
- 「凸阵探头 C5-2」（库存 6）点「删除」：页面同样没变化，控制台是 409 和「凸阵探头 C5-2 还有库存 6，不能删除」。

`api.ts` 正确地抛出了错误，但没有人接住，用户什么也看不到。这也是第 12C 章要做的。

#### 改坏一处试试

- 把 `createProduct` 里的 `body: JSON.stringify(input)` 写成 `body: input`，报 `TS2322: Type 'ProductInput' is not assignable to type 'BodyInit | null | undefined'.`：`body` 不接受普通对象。
- 把 `addProduct` 里的 `await api.createProduct(input)` 去掉 `await`，`saved` 就成了一个 Promise 而不是产品，TypeScript 在下一行报一长串错误，关键是最后一句 `Type 'Promise<Product>' is missing the following properties from type 'Product': id, code, name, category, and 2 more.`。忘写 `await` 是异步代码最常见的错误，看到 `Promise<...>` 出现在不该出现的地方，先查 `await`。

## ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 后端返回 415 | 请求头没写 `Content-Type: application/json` | 所有带 JSON 请求体的请求都加上这个请求头；集中写在 `api.ts` 里就不会漏 |
| 后端说请求体是空的或格式不对 | `body` 直接传了对象（TypeScript 会报 `TS2322`），或者传了 `String(obj)`（得到 `[object Object]`） | 用 `JSON.stringify(对象)` |
| 删除成功了却报 `Failed to execute 'json' on 'Response': Unexpected end of JSON input` | 删除返回 204 没有内容，代码里仍然调用了 `response.json()` | 204 时不要解析响应体 |
| 保存失败了，页面没有任何提示，控制台里有一条红色的未处理错误 | `api.ts` 抛出了错误，但还没有人接住 | 第 12C 章用 `try / catch` 接住并显示 |

## 🎯 动手练习

**练习 1：出库也保存到接口**

表格每行加「出库 −1」按钮，库存为 0 时禁用。点了以后用 `PUT` 把库存减 1 后的产品保存到接口。

验收标准：

- 「便携彩超主机」点一次「出库 −1」，约半秒后库存从 2 变成 1；刷新页面后还是 1。
- 「探头保护套」（库存 0）的「出库 −1」是灰色的。

**练习 2：在「网络」里看请求体和响应**

按 `F12` 打开「网络」，录入一个新产品，点开 `products` 那条 `POST` 请求。

验收标准：

- 在「负载（Payload）」里看到发出去的 JSON，里面没有 `id`。
- 在「响应（Response）」里看到服务器返回的 JSON，多了 `"id":6`，状态码 201。
- 再点「入库 +1」，找到 `PUT` 请求，说出它的请求体和 `POST` 的有什么不同。

卡住了看文末参考答案。

## 📌 一句话总结

写请求写明 method、Content-Type 和 JSON.stringify 的 body，集中放在 api.ts。

## 下一章

本章的三个保存函数在 `await` 之后直接用了外面的 `state`，平时看不出问题，操作一快就会显示错误的数据。第 12B 章复现并修掉它。

---

<!-- 以下是参考答案，先自己动手，卡住了再往下翻 -->
































## 参考答案

### 练习 1

思路：出库和入库一样，提交库存减 1 后的完整内容，用服务器返回的结果替换这一行。三个文件都要改：`useProducts` 多一个 `shipProduct`，`ProductTable` 多一个回调 prop `onShip`，`ListPage` 把它传下去。

```ts
// src/hooks/useProducts.ts
// 练习 1 做完后的版本
// 自定义 Hook：把「加载产品 + 增删改」这一整套逻辑打包，任何组件调用它都能用
import { useEffect, useState } from 'react'
import type { Product } from '../types.ts'
// 把 api.ts 导出的所有东西收进一个叫 api 的对象，用 api.createProduct 这样调用，不和下面同名的函数混淆
import * as api from '../api.ts'
import type { ProductInput } from '../api.ts'

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
        // 接口地址的前半段来自 .env 文件里的 VITE_API_BASE，换环境只改配置，不改代码
        const response = await fetch(`${import.meta.env.VITE_API_BASE}/products`)
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

  // 下面三个函数先调用接口，接口成功后再改 state。接口失败时抛出的错误不在这里处理，交给调用方显示
  async function addProduct(input: ProductInput): Promise<Product> {
    const saved = await api.createProduct(input)
    if (state.status !== 'success') return saved
    // 添加：服务器返回的产品带着它分配的 id，把它加到列表末尾
    setState({ status: 'success', products: [...state.products, saved] })
    return saved
  }

  async function deleteProduct(id: number): Promise<void> {
    await api.deleteProduct(id)
    if (state.status !== 'success') return
    setState({ status: 'success', products: state.products.filter((p) => p.id !== id) })
  }

  async function restockProduct(id: number): Promise<void> {
    if (state.status !== 'success') return
    const current = state.products.find((p) => p.id === id)
    if (!current) return
    // 把库存加 1 后的完整内容发给服务器，用服务器返回的结果替换这一行
    const saved = await api.updateProduct(id, { ...current, stock: current.stock + 1 })
    setState({ status: 'success', products: state.products.map((p) => (p.id === id ? saved : p)) })
  }

  // 练习 1：出库和入库一样，提交库存减 1 后的完整内容
  async function shipProduct(id: number): Promise<void> {
    if (state.status !== 'success') return
    const current = state.products.find((p) => p.id === id)
    if (!current) return
    const saved = await api.updateProduct(id, { ...current, stock: current.stock - 1 })
    setState({ status: 'success', products: state.products.map((p) => (p.id === id ? saved : p)) })
  }

  // 返回一个对象，调用方按名字取自己需要的部分
  return { state, addProduct, deleteProduct, restockProduct, shipProduct }
}
```

```tsx
// src/components/ProductTable.tsx
// 练习 1 做完后的版本
// 只负责显示表格。数据从 props 来；点了按钮不自己改数据，而是调用父组件给的函数「通知」它
import { Link } from 'react-router'
import type { Product } from '../types.ts'
// 导入 CSS Module 得到一个对象：键是你写的类名，值是打包后真正使用的类名
import styles from './ProductTable.module.css'

type ProductTableProps = {
  products: Product[]
  // 回调 props：类型写清楚「调用时传什么参数、返回什么」。void 表示不关心返回值
  onDelete: (id: number) => void
  onRestock: (id: number) => void
  // 练习 1：出库
  onShip: (id: number) => void
}

export function ProductTable({ products, onDelete, onRestock, onShip }: ProductTableProps) {
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
              {/* 库存为 0 时不能再出库 */}
              <button type="button" onClick={() => onShip(p.id)} disabled={p.stock === 0}>
                出库 −1
              </button>{' '}
              <button type="button" onClick={() => onDelete(p.id)}>删除</button>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
```

```tsx
// src/pages/ListPage.tsx
// 练习 1 做完后的版本
// 产品列表页：第 8C 章 App 的内容原样搬过来，去掉页头页脚（它们留在 App 里，所有页面共用）
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useProducts } from '../hooks/useProducts.ts'
import { FilterBar } from '../components/FilterBar.tsx'
import { ProductTable } from '../components/ProductTable.tsx'
import { ProductForm } from '../components/ProductForm.tsx'

export function ListPage() {
  // 加载和增删改的细节都在 useProducts 里，列表页只拿结果
  const { state, addProduct, deleteProduct, restockProduct, shipProduct } = useProducts()
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
    // 练习 1：多传一个出库函数
    content = (
      <ProductTable products={visible} onDelete={deleteProduct} onRestock={restockProduct} onShip={shipProduct} />
    )
  }

  return (
    <section>
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar keyword={keyword} onKeywordChange={setKeyword} />
      {content}
      <ProductForm onAdd={addProduct} />
    </section>
  )
}
```

### 练习 2

不用写代码。实测录入「P-003 微凸探头 C8-4 9800」时：

```text
负载：{"code":"P-003","name":"微凸探头 C8-4","category":"探头","price":9800,"stock":0}
响应（201）：{"id":6,"code":"P-003","name":"微凸探头 C8-4","category":"探头","price":9800,"stock":0}
```

点「耦合剂 250ml」的「入库 +1」时，`PUT /api/products/4` 的请求体是：

```text
{"id":4,"code":"C-001","name":"耦合剂 250ml","category":"耗材","price":18,"stock":121}
```

两点不同：地址里带着 id（`/products/4`），并且请求体里也有 `"id":4`。后者是因为 `{ ...current, stock: ... }` 把 `current` 的所有字段都展开了，包括 `id`。`ProductInput` 类型里没有 `id`，但 TypeScript 不检查展开进来的多余字段。模拟接口只认地址里的 id，忽略请求体里的，所以结果正确。
