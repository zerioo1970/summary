# 第十五章 · 数据请求（React Query）

> 本文是《React 18 & 19 系统教程》的第 15 章。完整目录见 [README](README.md)。

> **为什么需要 React Query？** 回顾第十三章示例 242——用 `useEffect` + 三个 state 手写数据请求，要处理加载态、错误态、竞态，还没有缓存、重试、后台刷新。每个组件都重复这套样板，很繁琐。**React Query（现名 TanStack Query）** 专门管理"**服务端状态**"（来自后端、你不完全掌控、会过期的数据），把这些都封装好了。
>
> **它解决什么？** 缓存、自动重新请求、加载/错误状态、请求去重、后台刷新、分页、乐观更新……让你用几行声明式代码就搞定复杂的数据请求逻辑。
>
> 本章基于 **TanStack Query v5**（需 React 18+），从"最简单的一次查询"讲到"变更数据与缓存更新"，共 16 个示例。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 268：安装与配置 QueryClientProvider</h3>

```bash
npm install @tanstack/react-query
```

```jsx
// main.jsx
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { createRoot } from 'react-dom/client';
import App from './App';

const queryClient = new QueryClient(); // 缓存等都存在这里

createRoot(document.getElementById('root')).render(
  <QueryClientProvider client={queryClient}>
    <App />
  </QueryClientProvider>
);
```

**详解**：使用前要做两步配置：① 创建一个 `QueryClient` 实例——它是"大脑"，管理所有查询的缓存、状态；② 用 `<QueryClientProvider client={queryClient}>` 把应用包起来（又是 Context 模式），这样内部所有组件才能使用 `useQuery`、`useMutation`。`queryClient` 通常在应用里只创建一个、全局共享。配置好后就能在任意组件里请求数据了。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 269：useQuery 基础</h3>

```jsx
import { useQuery } from '@tanstack/react-query';

function UserList() {
  const { data, isPending, isError } = useQuery({
    queryKey: ['users'],                                   // 这份数据的唯一标识
    queryFn: () => fetch('/api/users').then(r => r.json()), // 怎么获取数据
  });

  if (isPending) return <p>加载中...</p>;
  if (isError)   return <p>加载失败</p>;
  return <ul>{data.map(u => <li key={u.id}>{u.name}</li>)}</ul>;
}
```

**详解**：这就是 React Query 的核心 `useQuery`，对比第十三章示例 242 的手写版本，代码大幅简化。它接收一个对象：
- **`queryKey`**：这份数据的唯一标识（数组形式），React Query 用它做缓存的键；
- **`queryFn`**：一个返回 Promise 的函数，负责实际获取数据（可用 fetch、axios 等）。

它自动返回 `data`（数据）、`isPending`（是否加载中）、`isError`（是否出错）等状态——加载、错误、竞态处理全部内置，你只管根据状态渲染。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 270：查询的几种状态</h3>

```jsx
function Todos() {
  const { data, status, error, isFetching } = useQuery({
    queryKey: ['todos'],
    queryFn: fetchTodos,
  });

  // status 有三种：'pending'（加载中）| 'error'（出错）| 'success'（成功）
  if (status === 'pending') return <p>加载中...</p>;
  if (status === 'error')   return <p>出错了：{error.message}</p>;
  return (
    <div>
      {isFetching && <span>后台刷新中...</span>}
      <ul>{data.map(t => <li key={t.id}>{t.title}</li>)}</ul>
    </div>
  );
}
```

**详解**：`useQuery` 返回丰富的状态字段，理解它们很重要：
- **`status`**：`'pending'`（首次加载、还没数据）、`'error'`（出错）、`'success'`（有数据）；也可用布尔快捷方式 `isPending`/`isError`/`isSuccess`。
- **`isFetching`**：是否正在请求（**包括后台的重新验证**）。区别在于——首次加载时 `isPending` 和 `isFetching` 都为真；但已有缓存数据、正在后台悄悄刷新时，`isPending` 为假、`isFetching` 为真。据此可显示"后台刷新中"的轻提示，同时仍展示旧数据。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 271：带参数的查询（queryKey 依赖）</h3>

```jsx
function UserDetail({ userId }) {
  const { data, isPending } = useQuery({
    queryKey: ['user', userId],                              // userId 是 key 的一部分
    queryFn: () => fetch(`/api/users/${userId}`).then(r => r.json()),
  });
  if (isPending) return <p>加载中...</p>;
  return <h3>{data.name}</h3>;
}
```

**详解**：当请求依赖某个变量（如 `userId`）时，把它**放进 `queryKey` 数组**。这有两个作用：① 不同 `userId` 会被当作**不同的查询分别缓存**（`['user', 1]` 和 `['user', 2]` 各存各的）；② 当 `userId` 变化时，React Query **自动重新请求**新数据——你不用像 `useEffect` 那样手动管理依赖。可以把 `queryKey` 理解为"这份数据的身份 + 它依赖的参数"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 272：条件查询（enabled）</h3>

```jsx
function UserPosts({ userId }) {
  const { data } = useQuery({
    queryKey: ['posts', userId],
    queryFn: () => fetch(`/api/users/${userId}/posts`).then(r => r.json()),
    enabled: !!userId, // 只有 userId 存在时才发请求
  });
  return <div>{data?.length ?? 0} 篇文章</div>;
}
```

**详解**：有时需要"等某个条件满足了才发请求"——比如 `userId` 还没拿到时不该请求它的文章。用 **`enabled`** 选项控制：`enabled: false` 时该查询暂停、不会执行 `queryFn`；变为 `true` 时才自动发起。常见于"依赖上一个请求结果"的链式查询（等第一个查询成功拿到 id，再启用第二个查询）。这比手写一堆 if 判断优雅得多。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 273：缓存与自动重新请求（staleTime / gcTime）</h3>

```jsx
const { data } = useQuery({
  queryKey: ['config'],
  queryFn: fetchConfig,
  staleTime: 5 * 60 * 1000, // 5 分钟内数据视为"新鲜"，不重新请求
  gcTime: 10 * 60 * 1000,   // 数据无人使用后，缓存再保留 10 分钟才回收
});
```

**详解**：React Query 最强大的地方是自动缓存。两个关键配置（注意 v5 里 `cacheTime` 已改名为 `gcTime`）：
- **`staleTime`（新鲜时间）**：数据保持"新鲜"的时长。这段时间内再次使用同一 `queryKey`，**直接用缓存、不重新请求**。默认是 0（即数据立刻过期，一有机会就后台刷新）。
- **`gcTime`（垃圾回收时间）**：数据不再被任何组件使用后，缓存在内存里保留多久才被清除。默认 5 分钟。

默认情况下，React Query 还会在"窗口重新获得焦点""网络重连"时自动后台刷新数据——保证用户看到的数据尽量新。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 274：请求去重与共享缓存</h3>

```jsx
// 三个组件同时用相同的 queryKey，只会发一次请求，共享同一份缓存
function A() { const { data } = useQuery({ queryKey: ['me'], queryFn: fetchMe }); /* ... */ }
function B() { const { data } = useQuery({ queryKey: ['me'], queryFn: fetchMe }); /* ... */ }
function C() { const { data } = useQuery({ queryKey: ['me'], queryFn: fetchMe }); /* ... */ }
```

**详解**：这是 React Query 相对手写请求的一大优势——**自动去重和共享**。上面三个组件都请求 `['me']`，如果它们同时挂载，React Query **只会真正发起一次网络请求**，然后三个组件共享这份缓存结果。手写 `useEffect` 版本则会发三次重复请求。这个特性让你可以放心地"在需要数据的每个组件里各自 `useQuery`"，而不必把数据提升到顶层再层层传递——大大简化了数据共享。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 275：手动刷新（refetch）</h3>

```jsx
function Dashboard() {
  const { data, refetch, isFetching } = useQuery({
    queryKey: ['stats'],
    queryFn: fetchStats,
  });
  return (
    <div>
      <button onClick={() => refetch()} disabled={isFetching}>
        {isFetching ? '刷新中...' : '刷新数据'}
      </button>
      <pre>{JSON.stringify(data, null, 2)}</pre>
    </div>
  );
}
```

**详解**：除了自动刷新，`useQuery` 还返回一个 **`refetch`** 函数，让你手动触发重新请求（比如给个"刷新"按钮）。配合 `isFetching` 在刷新期间禁用按钮、显示"刷新中"。这适合用户主动要求获取最新数据的场景。注意：多数时候你**不需要**手动 refetch——React Query 的自动刷新和缓存失效机制（见下例）已经能覆盖绝大部分刷新需求。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 276：useMutation 基础（增删改）</h3>

```jsx
import { useMutation } from '@tanstack/react-query';

function AddTodo() {
  const mutation = useMutation({
    mutationFn: (newTodo) =>
      fetch('/api/todos', {
        method: 'POST',
        body: JSON.stringify(newTodo),
      }).then(r => r.json()),
  });

  return (
    <button
      onClick={() => mutation.mutate({ title: '新任务' })}
      disabled={mutation.isPending}
    >
      {mutation.isPending ? '提交中...' : '添加任务'}
    </button>
  );
}
```

**详解**：`useQuery` 用于**读取**数据；**修改**数据（增、删、改，即"写操作"）则用 **`useMutation`**。它接收 `mutationFn`（执行修改的函数，通常是 POST/PUT/DELETE 请求），返回一个对象，其中 `mutate(参数)` 用来触发这次修改，`isPending` 表示"正在提交"。和 `useQuery` 不同，mutation **不会自动执行**，只有你调用 `mutate()` 时才发起。这里点按钮时提交新任务，提交期间按钮禁用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 277：变更后使缓存失效（invalidateQueries）</h3>

```jsx
import { useMutation, useQueryClient } from '@tanstack/react-query';

function AddTodo() {
  const queryClient = useQueryClient();
  const mutation = useMutation({
    mutationFn: (newTodo) => postTodo(newTodo),
    onSuccess: () => {
      // 提交成功后，让 ['todos'] 缓存失效 → 自动重新请求最新列表
      queryClient.invalidateQueries({ queryKey: ['todos'] });
    },
  });
  return <button onClick={() => mutation.mutate({ title: '任务' })}>添加</button>;
}
```

**详解**：这是 React Query 最核心的模式之一——**增删改之后，如何让列表自动更新？** 答案是"使相关缓存失效"。通过 `useQueryClient()` 拿到 `queryClient`，在 mutation 的 `onSuccess` 回调里调用 `invalidateQueries({ queryKey: ['todos'] })`——它会把 `['todos']` 标记为过期，React Query 随即**自动重新请求**该列表，界面就显示出刚添加的数据了。你不用手动改本地状态，"改完让缓存失效、由 React Query 重新拉取"是最省心、最不易出错的做法。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 278：mutation 的成功/失败回调</h3>

```jsx
const mutation = useMutation({
  mutationFn: saveUser,
  onSuccess: (data) => {
    alert('保存成功！');            // 成功时
  },
  onError: (error) => {
    alert('保存失败：' + error.message); // 失败时
  },
  onSettled: () => {
    console.log('无论成功失败都会执行'); // 结束时（类似 finally）
  },
});

// 也可以在调用时传回调：
// mutation.mutate(user, { onSuccess: () => navigate('/list') });
```

**详解**：`useMutation` 提供了几个生命周期回调：`onSuccess`（成功后，能拿到返回数据）、`onError`（失败后，能拿到错误）、`onSettled`（无论成败都执行，类似 `try/finally`）。它们适合做提示、跳转、失效缓存等副作用。回调既可以定义在 `useMutation` 配置里（每次都触发），也可以在调用 `mutate(数据, { onSuccess })` 时传入（仅本次触发）。**注意**：v5 里已移除了 `useQuery` 的 `onSuccess/onError`，这类回调现在只用于 mutation。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 279：乐观更新（先改界面，再等服务器）</h3>

```jsx
const queryClient = useQueryClient();
const toggleTodo = useMutation({
  mutationFn: updateTodo,
  onMutate: async (newTodo) => {
    await queryClient.cancelQueries({ queryKey: ['todos'] });
    const previous = queryClient.getQueryData(['todos']); // 备份旧数据
    // 立即乐观地更新缓存（界面马上变化，不等服务器）
    queryClient.setQueryData(['todos'], (old) =>
      old.map(t => t.id === newTodo.id ? newTodo : t)
    );
    return { previous }; // 传给 onError 用于回滚
  },
  onError: (err, newTodo, context) => {
    queryClient.setQueryData(['todos'], context.previous); // 出错回滚
  },
  onSettled: () => queryClient.invalidateQueries({ queryKey: ['todos'] }),
});
```

**详解**："乐观更新"是一种提升体验的高级技巧——**不等服务器返回，先立即更新界面**（假设操作会成功），让交互瞬间响应；万一服务器返回失败，再把界面回滚到操作前。实现靠 `onMutate`（发请求前：备份旧数据、乐观改缓存）、`onError`（失败：用备份回滚）、`onSettled`（结束：让缓存失效以和服务器同步）。常用于点赞、勾选待办等"几乎总会成功"的高频操作。这是进阶内容，理解思路即可。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 280：分页查询（placeholderData 保留上页）</h3>

```jsx
import { useQuery, keepPreviousData } from '@tanstack/react-query';
import { useState } from 'react';

function PagedList() {
  const [page, setPage] = useState(1);
  const { data, isFetching } = useQuery({
    queryKey: ['items', page],
    queryFn: () => fetch(`/api/items?page=${page}`).then(r => r.json()),
    placeholderData: keepPreviousData, // 翻页时保留上一页数据，避免闪烁
  });
  return (
    <div>
      <ul>{data?.map(i => <li key={i.id}>{i.name}</li>)}</ul>
      <button disabled={page === 1} onClick={() => setPage(p => p - 1)}>上一页</button>
      <span>第 {page} 页 {isFetching && '（加载中）'}</span>
      <button onClick={() => setPage(p => p + 1)}>下一页</button>
    </div>
  );
}
```

**详解**：分页时把 `page` 放进 `queryKey`（`['items', page]`），`page` 变化就自动请求对应页（每页数据分别缓存，翻回已看过的页会秒开）。关键选项 **`placeholderData: keepPreviousData`**（v5 写法，替代 v4 的 `keepPreviousData: true`）——翻页加载新数据时，**先继续显示上一页的数据**，而不是闪现空白/loading，等新数据到了再替换，体验更平滑。用 `isFetching` 给个"加载中"提示即可。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 281：无限滚动（useInfiniteQuery）</h3>

```jsx
import { useInfiniteQuery } from '@tanstack/react-query';

function Feed() {
  const { data, fetchNextPage, hasNextPage, isFetchingNextPage } =
    useInfiniteQuery({
      queryKey: ['feed'],
      queryFn: ({ pageParam }) =>
        fetch(`/api/feed?cursor=${pageParam}`).then(r => r.json()),
      initialPageParam: 0,
      getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined, // 返回下一页游标
    });

  return (
    <div>
      {data?.pages.map((page, i) =>
        page.items.map(item => <p key={item.id}>{item.text}</p>)
      )}
      <button onClick={() => fetchNextPage()} disabled={!hasNextPage || isFetchingNextPage}>
        {isFetchingNextPage ? '加载中...' : hasNextPage ? '加载更多' : '没有更多了'}
      </button>
    </div>
  );
}
```

**详解**："加载更多/无限滚动"用专门的 `useInfiniteQuery`。它和 `useQuery` 的区别：数据以"**分页累积**"形式保存在 `data.pages`（一个数组，每项是一页）。关键配置：`initialPageParam`（首页参数）、`getNextPageParam`（从上一页数据里算出"下一页的参数"，返回 `undefined` 表示没有更多了）。`fetchNextPage()` 加载下一页并追加，`hasNextPage` 判断是否还有更多。配合滚动到底部自动调用 `fetchNextPage` 即可实现无限滚动（v5 要求显式提供 `initialPageParam`）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 282：预取数据（prefetch，鼠标悬停时提前加载）</h3>

```jsx
function UserLink({ userId }) {
  const queryClient = useQueryClient();
  const prefetch = () => {
    queryClient.prefetchQuery({
      queryKey: ['user', userId],
      queryFn: () => fetch(`/api/users/${userId}`).then(r => r.json()),
    });
  };
  // 鼠标悬停时就提前请求，等用户真正点进去时数据已就绪
  return <Link to={`/user/${userId}`} onMouseEnter={prefetch}>查看用户</Link>;
}
```

**详解**：`prefetchQuery` 让你**提前**把数据请求好并放进缓存，等真正需要时直接命中缓存、瞬间显示。经典用法是"鼠标悬停在链接上时就预取目标页数据"——等用户点击进入，数据往往已经加载好了，几乎没有等待。这是 React Query 用来"消除感知加载时间"的实用技巧，能显著提升体验。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 283：React Query 小结与 SWR 对比</h3>

```jsx
// 读数据：useQuery（自动缓存、状态、后台刷新、去重）
const { data, isPending, isError } = useQuery({ queryKey, queryFn });

// 写数据：useMutation + 失效缓存
const m = useMutation({ mutationFn, onSuccess: () => qc.invalidateQueries({ queryKey }) });

// 分页：placeholderData: keepPreviousData
// 无限滚动：useInfiniteQuery
// 提前加载：queryClient.prefetchQuery
```

**详解**：本章要点回顾——**用 `useQuery` 读、`useMutation` 写、改完让缓存失效**，这三招覆盖了日常绝大多数数据请求需求，其余（分页、无限滚动、预取、乐观更新）按需使用。

**核心心智**：React Query 管理的是"**服务端状态**"（远程、会过期的数据），它和 `useState`（管理本地 UI 状态）职责不同、互补使用——**别再用 `useState` + `useEffect` 手写请求了**。

**和 SWR 的对比**：SWR（由 Vercel 出品）是另一个流行的数据请求库，理念相似、更轻量、API 更精简（`useSWR(key, fetcher)`）；React Query 功能更全面（mutation、无限查询、更强的缓存控制、开发者工具）。中小项目用 SWR 足够，功能需求多则选 React Query。两者都远胜于手写请求。至此，你已经掌握了现代 React 应用最重要的两个生态库——路由与数据请求。

---

---
[← 上一章](14-ReactRouter路由.md) · [📖 目录](README.md) · [下一章 →](16-React19简介与升级.md)
