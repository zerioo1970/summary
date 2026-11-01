# 第 9C 章 URL 参数：一个详情页显示任意一个产品

## 这一章解决什么问题

每个产品要有自己的详情页，地址里带着产品编号：`/products/1` 是凸阵探头，`/products/2` 是线阵探头。产品有几十种，不可能每种写一条路由、一个页面。本章用一条带参数的路由和一个页面组件，显示任意一个产品；表格里的产品名可以点进去。

本章只讲一个技术点：**URL 参数**。

## 开始之前

- 上一章你已经会了：用 `Link` 做导航，换页不重新加载。
- 起点代码：仓库 `code/ch09B/price-list`（不含练习答案）。在 `D:\react-study\price-list` 执行 `npm run dev`。
- 预计耗时：35 分钟。

## 正文

### 9C.1 URL 参数是什么

**白话定义**：路由的 `path` 里以 `:` 开头的部分是参数（URL parameter），例如 `/products/:id`，它能匹配 `/products/1`、`/products/25`。页面组件里用 `useParams()` 取出参数的值。

**业务场景**：每个产品一个详情页，地址里带产品编号。不同产品共用一个 `ProductDetailPage` 组件，组件从地址里读出编号，去数据里找对应的产品。

**反例（什么时候不该用）**：筛选条件、排序方式这类**可选**的、可能有多个的值，不要硬塞进路径（比如 `/products/探头/价格升序`）。这类值一般放在查询字符串里（`?category=探头&sort=price`），本教程不展开。路径参数适合「标识是哪一个」的必需值。

#### 🖼 图解

```mermaid
graph TB
    A["地址 /products/3"] --> B["匹配 path<br/>/products/:id"]
    B --> C["useParams() 返回<br/>{ id: '3' }（字符串）"]
    C --> D["Number('3') 后<br/>在产品数组里 find"]
```

### 9C.2 详情页

#### 💻 最小可运行代码

新建 `src/pages/ProductDetailPage.tsx`：

```tsx
// src/pages/ProductDetailPage.tsx
// 产品详情页：地址是 /products/3 这种形式，从地址里取出 3，再找到对应的产品
import { Link, useParams } from 'react-router'
import { useProducts } from '../hooks/useProducts.ts'
import { ProductCard } from '../components/ProductCard.tsx'

export function ProductDetailPage() {
  // 路由表里写的是 :id，这里就按 id 取。地址里的值永远是字符串，也可能没有（undefined）
  const { id } = useParams()
  // 详情页自己加载一次数据（第 8C 章说过：这和列表页那份是两份独立的数据）
  const { state } = useProducts()

  if (state.status === 'loading') {
    return <p>加载中…</p>
  }
  if (state.status === 'error') {
    return <p className="error">加载失败：{state.message}</p>
  }

  // 地址里的 '3' 是字符串，产品的 id 是数字，先转换再比较
  const product = state.products.find((p) => p.id === Number(id))

  return (
    <section>
      <p>
        <Link to="/">← 返回列表</Link>
      </p>
      {/* 第 3、4 章做的卡片在这里派上用场，自带询价数量 */}
      {product ? <ProductCard product={product} /> : <p className="empty">找不到编号为 {id} 的产品</p>}
    </section>
  )
}
```

用下面的内容整个替换 `src/App.tsx`。和上一章相比加了两行：import 区的 `ProductDetailPage`，以及路由表里的 `/products/:id` 这一条。`Routes` 会按「具体程度」选出最匹配的一条，和书写顺序无关：实测把 `*` 那一条挪到最前面，`/about`、`/products/2` 照样显示正确的页面。习惯上仍把 `*` 写在最后，读代码时一眼能看出它是兜底。

```tsx
// src/App.tsx
// App 变成「外框」：页头、导航、页脚所有页面共用，中间按地址显示不同的页面
import { Link, Route, Routes } from 'react-router'
import { PageHeader } from './components/PageHeader.tsx'
import { PageFooter } from './components/PageFooter.tsx'
import { ListPage } from './pages/ListPage.tsx'
import { AboutPage } from './pages/AboutPage.tsx'
import { ProductDetailPage } from './pages/ProductDetailPage.tsx'
import { NotFoundPage } from './pages/NotFoundPage.tsx'

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
        {/* :id 是参数，/products/1、/products/25 都匹配这一条 */}
        <Route path="/products/:id" element={<ProductDetailPage />} />
        {/* * 匹配任何地址，但只有别的路由都对不上时才会用它，相当于兜底 */}
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
      <PageFooter />
    </main>
  )
}

export default App
```

最后让表格里的产品名可以点。用下面的内容整个替换 `src/components/ProductTable.tsx`。和第 8A 章相比改了两处：顶部多了 `import { Link } from 'react-router'`；名称单元格里的 `{p.name}` 换成了 `<Link>`。

```tsx
// src/components/ProductTable.tsx
// 只负责显示表格。数据从 props 来；点了按钮不自己改数据，而是调用父组件给的函数「通知」它
import { Link } from 'react-router'
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
              {/* 点产品名进入详情页，地址形如 /products/3 */}
              <Link to={`/products/${p.id}`}>{p.name}</Link>
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

`` to={`/products/${p.id}`} `` 用模板字符串拼出地址：`p.id` 是 3 时，地址就是 `/products/3`。

#### 🔍 拆解运行结果

实测：

| 操作 | 结果 |
| --- | --- |
| 列表里点「凸阵探头 C5-2」 | 地址变成 `/products/1`，先「加载中…」，约 1 秒后显示第 4A 章的产品卡片（含询价数量） |
| 点「← 返回列表」或浏览器「后退」 | 回到列表 |
| 直接打开 `/products/2` 或在详情页按 `F5` | 显示线阵探头 L12-5 |
| 打开 `/products/99` | 「找不到编号为 99 的产品」 |

| 代码 | 说明 |
| --- | --- |
| `const { id } = useParams()` | 路由表里写的是 `:id`，这里就按 `id` 取。值永远是字符串，也可能没有（`undefined`） |
| `state.products.find((p) => p.id === Number(id))` | 数组的 `find` 返回第一个满足条件的元素，找不到返回 `undefined` |
| `product ? <ProductCard ... /> : <p>找不到…</p>` | 找到和找不到两种情况，用三元表达式二选一（第 5C 章） |
| 先 `return` 加载中、出错 | 和 `ProductTable` 一样，组件可以提前返回 |

详情页先显示「加载中…」，「网络」里多了两次 `products.json` 请求：详情页自己调用了 `useProducts()`，又加载了一遍。这带来一个问题，见下面的实验。

`F5` 刷新详情页在开发时正常，是因为开发服务器对不存在的路径一律返回 `index.html`。部署到正式服务器时要专门配置，第 10 章讲。

#### 改坏一处试试

**① 不转换类型直接比较。** 把 `p.id === Number(id)` 改成 `p.id === id`，报：

```text
error TS2367: This comparison appears to be unintentional because the types 'number' and 'string | undefined' have no overlap.
```

TypeScript 发现数字和字符串永远不会相等。改回去。

**② 新录入的产品点进去找不到。** 在列表页录入一个新产品「微凸探头 C8-4」，再点它的名字。实测地址是 `/products/1793378287241`（第 6B 章用 `Date.now()` 生成的 id），页面显示「找不到编号为 1793378287241 的产品」。按「后退」回到列表，显示「共 5 种产品」，刚录入的产品也不见了。

原因有两层：详情页自己调用 `useProducts()`，拿到的是从 `products.json` 新加载的一份，里面没有新产品；离开列表页时 `ListPage` 被移除，它的 state 跟着丢了，回来时又重新加载。练习 1 用第 8B 章的状态提升解决。

## ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 详情页永远显示「找不到」 | `useParams()` 得到的是字符串，和数字 id 比较永远不相等（有类型检查时会报 `TS2367`） | `Number(id)` 后再比较 |
| 换页后，之前新增、删除的数据都恢复原样 | 数据放在页面组件里，换页时页面组件被移除，state 一起丢掉 | 把 `useProducts()` 提到不会被移除的 `App` 里，用 props 传给各页面（练习 1） |
| 路由写成 `/products/id`（少了冒号），只有地址正好是 `/products/id` 时才匹配 | 没有 `:` 就是普通文字，不是参数 | 写成 `/products/:id` |
| `useParams()` 取出来是 `undefined` | 取的名字和路由表里 `:` 后面的名字不一样，比如路由写 `:productId`，取的是 `id` | 两边名字保持一致 |

## 🎯 动手练习

**练习 1：两个页面共用一份数据**

把 `useProducts()` 改为只在 `App` 里调用一次，`state` 和三个函数通过 props 传给 `ListPage`，`state` 传给 `ProductDetailPage`。两个页面里都不再调用 `useProducts()`。

验收标准：

- 录入「微凸探头 C8-4」后点它的名字，详情页显示这个产品，没有「加载中…」。
- 点「关于」再点「产品清单」，显示「共 6 种产品」，「网络」里没有新的 `products.json` 请求。

提示：`LoadState` 类型已经从 `useProducts.ts` 导出了。

**练习 2：详情页的「上一个 / 下一个」**

详情页卡片下面显示「上一个：xxx | 下一个：xxx」两个链接，顺序和列表一致。第一个产品没有上一个，显示「已经是第一个」；最后一个显示「已经是最后一个」。

验收标准：

- 打开凸阵探头 C5-2 的详情：显示「已经是第一个 | 下一个：线阵探头 L12-5」。
- 点「下一个」，地址变成 `/products/2`，显示线阵探头。

提示：数组的 `findIndex` 返回找到的下标，找不到返回 `-1`。

卡住了看文末参考答案。

## 📌 一句话总结

path 里写 :id，页面里用 useParams 取出，记得转成数字。

## 下一章

页面还很朴素，而且只能在开发服务器上看。第 10 章整理样式，打包成能部署的文件。

---

<!-- 以下是参考答案，先自己动手，卡住了再往下翻 -->































## 参考答案

### 练习 1

思路：`App` 不会因为换页被移除，在它里面调用一次 `useProducts()`，数据就一直在，两个页面通过 props 拿到的是同一份。这就是第 8B 章的状态提升，只是这次共同父组件是 `App`。

```tsx
// src/App.tsx
// 两个练习都做完后的版本
import { Link, Route, Routes } from 'react-router'
import { useProducts } from './hooks/useProducts.ts'
import { PageHeader } from './components/PageHeader.tsx'
import { PageFooter } from './components/PageFooter.tsx'
import { ListPage } from './pages/ListPage.tsx'
import { AboutPage } from './pages/AboutPage.tsx'
import { ProductDetailPage } from './pages/ProductDetailPage.tsx'
import { NotFoundPage } from './pages/NotFoundPage.tsx'

function App() {
  // 练习 1：只在 App 里调用一次。App 不会随换页被移除，数据就一直在；两个页面看到的是同一份
  const { state, addProduct, deleteProduct, restockProduct } = useProducts()

  return (
    <main>
      <PageHeader />
      <nav className="nav">
        {/* Link 改地址但不刷新页面；React Router 发现地址变了，换掉下面 Routes 里显示的页面 */}
        <Link to="/">产品清单</Link> | <Link to="/about">关于</Link>
      </nav>
      {/* 路由表：React Router 找出与当前地址最匹配的那条 path，显示它的 element */}
      <Routes>
        <Route
          path="/"
          element={<ListPage state={state} onAdd={addProduct} onDelete={deleteProduct} onRestock={restockProduct} />}
        />
        <Route path="/about" element={<AboutPage />} />
        <Route path="/products/:id" element={<ProductDetailPage state={state} />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
      <PageFooter />
    </main>
  )
}

export default App
```

`ListPage.tsx` 的改法：删掉 `useProducts` 的 import 和调用，加上 `ListPageProps` 类型，把用到 `addProduct`、`deleteProduct`、`restockProduct` 的地方换成 `onAdd`、`onDelete`、`onRestock`。完整文件：

```tsx
// src/pages/ListPage.tsx
// 练习 1 之后的版本：数据和增删改函数都由 App 通过 props 传进来
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { Product } from '../types.ts'
import type { LoadState } from '../hooks/useProducts.ts'
import { FilterBar } from '../components/FilterBar.tsx'
import { ProductTable } from '../components/ProductTable.tsx'
import { ProductForm } from '../components/ProductForm.tsx'

type ListPageProps = {
  state: LoadState
  onAdd: (product: Product) => void
  onDelete: (id: number) => void
  onRestock: (id: number) => void
}

export function ListPage({ state, onAdd, onDelete, onRestock }: ListPageProps) {
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
    content = <ProductTable products={visible} onDelete={onDelete} onRestock={onRestock} />
  }

  return (
    <section>
      <p>共 {count} 种产品</p>
      {/* 把值和「修改它的函数」一起交给搜索框 */}
      <FilterBar keyword={keyword} onKeywordChange={setKeyword} />
      {content}
      <ProductForm onAdd={onAdd} />
    </section>
  )
}
```

### 练习 2

思路：用 `findIndex` 找到当前产品的下标，下标减 1、加 1 就是前后两个；到头时显示文字而不是链接。

```tsx
// src/pages/ProductDetailPage.tsx
// 两个练习都做完后的版本
import { Link, useParams } from 'react-router'
import type { LoadState } from '../hooks/useProducts.ts'
import { ProductCard } from '../components/ProductCard.tsx'

type ProductDetailPageProps = {
  // 练习 1：数据由 App 传进来，不再自己调用 useProducts
  state: LoadState
}

export function ProductDetailPage({ state }: ProductDetailPageProps) {
  const { id } = useParams()

  if (state.status === 'loading') {
    return <p>加载中…</p>
  }
  if (state.status === 'error') {
    return <p className="error">加载失败：{state.message}</p>
  }

  const products = state.products
  // 练习 2：用下标找前后两个产品。找不到时 findIndex 返回 -1
  const index = products.findIndex((p) => p.id === Number(id))
  const product = index === -1 ? undefined : products[index]
  const prev = index > 0 ? products[index - 1] : undefined
  const next = index !== -1 && index < products.length - 1 ? products[index + 1] : undefined

  return (
    <section>
      <p>
        <Link to="/">← 返回列表</Link>
      </p>
      {product ? <ProductCard product={product} /> : <p className="empty">找不到编号为 {id} 的产品</p>}
      <p>
        {prev ? <Link to={`/products/${prev.id}`}>上一个：{prev.name}</Link> : '已经是第一个'}
        {' | '}
        {next ? <Link to={`/products/${next.id}`}>下一个：{next.name}</Link> : '已经是最后一个'}
      </p>
    </section>
  )
}
```
