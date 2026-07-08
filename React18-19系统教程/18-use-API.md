# 第十八章 · `use` API：在渲染中读取 Promise 与 Context

> 本文是《React 18 & 19 系统教程》的第 18 章。完整目录见 [README](README.md)。

> React 19 新增了一个特别的 API：`use`。它能在渲染过程中**读取一个资源**——目前支持读取 **Promise**（会自动配合 Suspense 挂起）和 **Context**。它长得像 Hook，但有一个 Hook 做不到的能力：**可以在条件语句、循环、提前返回之后调用**。

### （A）用 use 读取 Promise

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 296：use(promise) 配合 Suspense 读取异步数据</h3>

```jsx
import { use, Suspense } from 'react';

function Comments({ commentsPromise }) {
  // use 会"挂起"组件，直到 promise resolve
  const comments = use(commentsPromise);
  return comments.map((c) => <p key={c.id}>{c.text}</p>);
}

function Page({ commentsPromise }) {
  // Comments 挂起时，展示这个 Suspense 的 fallback
  return (
    <Suspense fallback={<div>加载评论中…</div>}>
      <Comments commentsPromise={commentsPromise} />
    </Suspense>
  );
}
```

**详解**：`use(promise)` 会让组件**挂起（suspend）**，直到 Promise 完成，期间由最近的 `<Suspense>` 显示 fallback；Promise resolve 后，`use` 直接返回结果值。相比 React 18 里"`useEffect` + `useState` 手动请求"，`use` 让异步数据读取像同步代码一样自然。若 Promise 被 reject，错误会冒泡到最近的 Error Boundary。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 297：陷阱——不要在渲染中创建 Promise</h3>

```jsx
// ❌ 错误：每次渲染都创建新 promise，React 会警告
function Comments() {
  const comments = use(fetch('/api/comments').then((r) => r.json()));
  // Console: A component was suspended by an uncached promise...
  return /* ... */;
}

// ✅ 正确：promise 由外部/框架/缓存层创建后传进来
function Page() {
  const commentsPromise = useMemo(() => fetchComments(), []); // 或来自框架的缓存
  return (
    <Suspense fallback="加载中…">
      <Comments commentsPromise={commentsPromise} />
    </Suspense>
  );
}
```

**详解**：`use` **不支持在渲染中直接创建的 Promise**。因为组件挂起后会重新渲染，如果每次渲染都 `fetch()` 新建 Promise，就会陷入"请求→挂起→重渲染→再请求"的死循环。正确做法是让 Promise 来自**支持缓存的框架/库**（如 Next.js、React Query），或在组件外/更上层稳定地创建后作为 prop 传入。React 19 官方也表示未来会提供更方便的"渲染中缓存 Promise"的能力。

### （B）用 use 读取 Context（可条件调用）

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 298：use(Context) 可以写在提前返回之后</h3>

```jsx
import { use } from 'react';
import ThemeContext from './ThemeContext';

function Heading({ children }) {
  if (children == null) {
    return null; // 提前返回
  }

  // ✅ use 可以在 if 之后调用；useContext 在这里会违反 Hook 规则
  const theme = use(ThemeContext);
  return <h1 style={{ color: theme.color }}>{children}</h1>;
}
```

**详解**：这正是 `use` 相对 `useContext` 的独特之处。**Hook（包括 `useContext`）必须在组件顶层无条件调用**，不能放在 `if` / 提前 `return` 之后。而 `use` **可以条件调用**——上面的组件在 `children` 为空时提前返回、根本不读取 Context，避免了不必要的订阅。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 299：use vs useContext 对比</h3>

```jsx
// 传统方式（依然可用）
function A() {
  const theme = useContext(ThemeContext); // 必须在顶层
  return <div style={{ color: theme.color }} />;
}

// React 19 新方式
function B({ show }) {
  if (!show) return null;
  const theme = use(ThemeContext); // 可以在条件后
  return <div style={{ color: theme.color }} />;
}
```

**详解**：`use` 和 Hook 一样**只能在渲染中调用**，但**不受"必须在顶层"的限制**，可以放进 `if` / 循环里。简单记忆：
- 只是读 Context、且在组件顶层 → `useContext` 或 `use` 都行。
- 需要在条件分支/提前返回之后读 Context → 用 `use`。
- 读 Promise（挂起等待数据）→ 只能用 `use`。

---

---
[← 上一章](17-Actions.md) · [📖 目录](README.md) · [下一章 →](19-组件API的改进.md)
