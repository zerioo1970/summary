# 第十四章 · React Router 路由

> 本文是《React 18 & 19 系统教程》的第 14 章。完整目录见 [README](README.md)。

> **为什么需要路由？** React 默认是"单页应用（SPA）"——整个网站只有一个 HTML 页面。但用户仍希望有"多个页面"的体验：不同的 URL 显示不同内容、能前进后退、能分享某个页面的链接。**路由（Router）** 就是负责"根据当前 URL 显示对应组件"的库。
>
> **React Router 是什么？** 它是 React 生态最主流的路由库（本章基于 **React Router v7**，其声明式 API 与 v6 完全兼容；v7 于 2024 年底发布，已与 Remix 合并）。它让你把"URL 路径"和"要渲染的组件"对应起来，并提供导航、参数、嵌套布局等能力。
>
> 本章从"最简单的两页切换"讲到"受保护路由、懒加载"，共 16 个示例。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 252：安装与基本配置</h3>

```bash
npm install react-router-dom
```

```jsx
// main.jsx —— 用 BrowserRouter 把整个应用包起来
import { BrowserRouter } from 'react-router-dom';
import { createRoot } from 'react-dom/client';
import App from './App';

createRoot(document.getElementById('root')).render(
  <BrowserRouter>
    <App />
  </BrowserRouter>
);
```

**详解**：使用 React Router 的第一步，是在应用最外层包一个 **`<BrowserRouter>`**。它负责监听浏览器地址栏的变化、并把"当前 URL"提供给内部所有组件——就像第十二章的 Context Provider 一样。`BrowserRouter` 使用 HTML5 的 History API，URL 形如 `/about`（干净、无 `#`）。包好之后，内部才能使用 `<Routes>`、`<Link>`、`useNavigate` 等路由功能。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 253：Routes 与 Route（URL → 组件）</h3>

```jsx
import { Routes, Route } from 'react-router-dom';

function Home()  { return <h1>首页</h1>; }
function About() { return <h1>关于我们</h1>; }

function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/about" element={<About />} />
    </Routes>
  );
}
```

**详解**：路由的核心是 `<Routes>` 和 `<Route>`。`<Route path="路径" element={<组件 />} />` 定义一条规则："当 URL 匹配 `path` 时，渲染 `element` 里的组件"。`<Routes>` 是所有 `Route` 的容器，它会**从中挑出与当前 URL 最匹配的那一条**来渲染。上例中访问 `/` 显示首页、访问 `/about` 显示关于页。注意 `element` 接收的是 JSX 元素（`<Home />`），不是组件本身。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 254：Link 导航（不刷新页面）</h3>

```jsx
import { Link } from 'react-router-dom';

function Nav() {
  return (
    <nav>
      <Link to="/">首页</Link>
      <Link to="/about">关于</Link>
    </nav>
  );
}
```

**详解**：页面间跳转要用 **`<Link>`** 而不是普通的 `<a>` 标签。区别很关键：`<a href>` 会让浏览器**重新加载整个页面**（白屏一下、丢失应用状态），而 `<Link to>` 由 React Router 拦截，只是**在前端切换组件、更新 URL，不刷新页面**——这才是 SPA 流畅体验的关键。`to` 属性写目标路径。渲染到页面上它最终还是个 `<a>`，但点击行为被接管了。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 255：NavLink（高亮当前链接）</h3>

```jsx
import { NavLink } from 'react-router-dom';

function Nav() {
  return (
    <nav>
      <NavLink to="/" style={({ isActive }) => ({ color: isActive ? 'red' : 'black' })}>
        首页
      </NavLink>
      <NavLink to="/about" className={({ isActive }) => isActive ? 'active' : ''}>
        关于
      </NavLink>
    </nav>
  );
}
```

**详解**：`<NavLink>` 是 `<Link>` 的增强版，专门用于导航菜单——它能**自动知道自己是否是"当前页"**。它的 `style` 或 `className` 可以接收一个函数，参数里的 `isActive` 表示"当前 URL 是否匹配这个链接"。据此给当前项加高亮样式（变色、加下划线等），让用户知道自己在哪个页面。这是做导航栏"当前项高亮"的标准方式。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 256：动态路由参数（useParams）</h3>

```jsx
import { Routes, Route, useParams } from 'react-router-dom';

function UserDetail() {
  const { id } = useParams();      // 取出 URL 里的 :id
  return <h1>用户 ID：{id}</h1>;
}

function App() {
  return (
    <Routes>
      <Route path="/user/:id" element={<UserDetail />} /> {/* :id 是动态段 */}
    </Routes>
  );
}
// 访问 /user/42 → 显示"用户 ID：42"
```

**详解**：路径里用 **`:参数名`** 定义"动态段"，比如 `/user/:id` 能匹配 `/user/1`、`/user/42` 等任意值。组件内用 **`useParams()`** 这个 Hook 拿到这些参数（返回一个对象，键就是参数名）。这是"详情页"最常见的模式——列表点某一项，跳到 `/user/该项id`，详情页据此 `id` 请求并展示对应数据。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 257：编程式导航（useNavigate）</h3>

```jsx
import { useNavigate } from 'react-router-dom';

function LoginForm() {
  const navigate = useNavigate();
  const handleLogin = () => {
    // ...登录成功后
    navigate('/dashboard');       // 跳转到某个页面
    // navigate(-1);              // 后退一步（相当于浏览器返回）
    // navigate('/home', { replace: true }); // 替换当前历史记录，不能再后退回来
  };
  return <button onClick={handleLogin}>登录</button>;
}
```

**详解**：除了用户点 `<Link>`，有时需要**在代码里主动跳转**（如登录成功后自动进入首页、提交后返回列表）。用 `useNavigate()` 拿到一个 `navigate` 函数：`navigate('/路径')` 跳转到指定页面；`navigate(-1)` 相当于点浏览器后退；加 `{ replace: true }` 会替换当前历史记录（用户按后退键回不到这一页，适合登录页跳转后）。这是"编程式导航"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 258：查询参数（useSearchParams）</h3>

```jsx
import { useSearchParams } from 'react-router-dom';

function ProductList() {
  const [searchParams, setSearchParams] = useSearchParams();
  const category = searchParams.get('category') || 'all'; // 读取 ?category=xxx
  return (
    <div>
      <p>当前分类：{category}</p>
      <button onClick={() => setSearchParams({ category: 'books' })}>看图书</button>
    </div>
  );
}
// URL 形如 /products?category=books
```

**详解**：查询参数（URL 里 `?` 后面的部分，如 `?category=books&sort=price`）用 **`useSearchParams()`** 处理，用法很像 `useState`：返回 `[searchParams, setSearchParams]`。用 `searchParams.get('键')` 读取某个参数；用 `setSearchParams({...})` 修改它（会更新 URL）。它适合存放"页面状态"——筛选条件、排序、页码、搜索词等，好处是这些状态**体现在 URL 里**，可分享、可刷新恢复、可前进后退。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 259：嵌套路由与 Outlet</h3>

```jsx
import { Routes, Route, Outlet, Link } from 'react-router-dom';

function Layout() {
  return (
    <div>
      <nav><Link to="/">首页</Link> | <Link to="/about">关于</Link></nav>
      <hr />
      <Outlet /> {/* 子路由的内容渲染在这里 */}
    </div>
  );
}

function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Home />} />       {/* 匹配 / */}
        <Route path="about" element={<About />} /> {/* 匹配 /about */}
      </Route>
    </Routes>
  );
}
```

**详解**：真实应用里很多页面共享同一套"外壳"（导航栏、侧边栏、页脚），只有中间内容不同。**嵌套路由**能优雅实现：把 `<Route>` 嵌套起来，父路由渲染布局组件（`Layout`），子路由的内容通过父组件里的 **`<Outlet />`** 占位符渲染出来。这样切换子页面时，外层导航栏不会重新渲染。`<Outlet>` 就是"子路由内容的插槽"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 260：index 路由（默认子页面）</h3>

```jsx
<Route path="/dashboard" element={<DashboardLayout />}>
  <Route index element={<Overview />} />          {/* /dashboard */}
  <Route path="stats" element={<Stats />} />       {/* /dashboard/stats */}
  <Route path="settings" element={<Settings />} /> {/* /dashboard/settings */}
</Route>
```

**详解**：`<Route index element={...} />` 定义"**索引路由**"——当 URL 正好匹配父路径（这里 `/dashboard`）、还没有更深的子路径时，渲染它。可以理解为"这个布局的默认首页"。它没有 `path`，用 `index` 关键字标记。上例访问 `/dashboard` 显示概览，访问 `/dashboard/stats` 显示统计。嵌套路由 + index 路由是搭建"带侧边栏的后台管理界面"的标准结构。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 261：404 页面（通配符 *）</h3>

```jsx
function NotFound() { return <h1>404 - 页面不存在</h1>; }

function App() {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/about" element={<About />} />
      <Route path="*" element={<NotFound />} /> {/* 匹配所有未定义的路径 */}
    </Routes>
  );
}
```

**详解**：当用户访问一个没有定义的路径时，应该显示友好的"404 未找到"页面，而不是空白。用 `path="*"`（通配符）定义一条"兜底路由"——它匹配所有前面都没匹配上的 URL。`<Routes>` 总是选择最匹配的那条，所以 `*` 只在其它都不匹配时才生效。把它放在路由列表最后。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 262：重定向（Navigate 组件）</h3>

```jsx
import { Navigate } from 'react-router-dom';

function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/home" replace />} /> {/* 访问 / 自动跳到 /home */}
      <Route path="/home" element={<Home />} />
    </Routes>
  );
}
```

**详解**：有时需要"重定向"——访问某路径时自动跳到另一个路径（比如把旧地址跳到新地址，或把 `/` 跳到 `/home`）。在路由里渲染 **`<Navigate to="目标" replace />`** 组件即可：它一旦被渲染，就会立即导航到 `to` 指定的路径。加 `replace` 表示替换历史记录（用户按后退不会回到这个中转地址）。它也常用于"未登录就跳到登录页"（见示例 265）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 263：读取当前位置（useLocation）</h3>

```jsx
import { useLocation } from 'react-router-dom';

function PageTracker() {
  const location = useLocation();
  useEffect(() => {
    console.log('访问了页面：', location.pathname); // 如 '/about'
    // 这里可以上报页面访问统计（埋点）
  }, [location.pathname]);
  return null;
}
```

**详解**：`useLocation()` 返回当前 URL 的详细信息对象，常用字段有 `pathname`（路径，如 `/about`）、`search`（查询串，如 `?id=1`）、`hash`、`state`。典型用途是**监听路由变化做统计埋点**（页面切换时上报）、或读取通过 `navigate('/x', { state })` 传递的额外数据。配合 `useEffect` 监听 `location.pathname` 就能在每次页面切换时执行逻辑。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 264：受保护路由（登录验证）</h3>

```jsx
import { Navigate } from 'react-router-dom';

function RequireAuth({ children }) {
  const isLoggedIn = Boolean(localStorage.getItem('token'));
  // 未登录就重定向到登录页，否则正常渲染子内容
  return isLoggedIn ? children : <Navigate to="/login" replace />;
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/dashboard" element={
        <RequireAuth><Dashboard /></RequireAuth>
      } />
    </Routes>
  );
}
```

**详解**：很多页面需要"登录后才能访问"。做法是封装一个 `RequireAuth` 组件当"守卫"：它检查登录状态，已登录就渲染 `children`（真正的页面），未登录就用 `<Navigate>` 重定向到登录页。把需要保护的路由用它包起来即可。这是"路由守卫/权限控制"的常见实现思路，实际项目里 `isLoggedIn` 通常来自 Context 里的全局登录状态（第十二章）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 265：路由懒加载（配合 Suspense）</h3>

```jsx
import { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router-dom';

const Home = lazy(() => import('./pages/Home'));
const About = lazy(() => import('./pages/About'));

function App() {
  return (
    <Suspense fallback={<p>页面加载中...</p>}>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About />} />
      </Routes>
    </Suspense>
  );
}
```

**详解**：路由是做"代码分割"最理想的边界——每个页面单独打包，用户访问哪个页才下载哪个页的代码，大幅减小首屏体积（第十章示例 193、第十一章示例 215 讲过原理）。用 `React.lazy` 包裹每个页面组件，再用一个 `<Suspense>` 包住 `<Routes>` 提供加载中占位。这是中大型 React 应用几乎必备的性能优化。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 266：数据路由 createBrowserRouter（v6.4+ / v7 新方式）</h3>

```jsx
import { createBrowserRouter, RouterProvider } from 'react-router-dom';

const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      { index: true, element: <Home /> },
      { path: 'about', element: <About /> },
      { path: 'user/:id', element: <UserDetail />, loader: userLoader },
    ],
  },
]);

function App() {
  return <RouterProvider router={router} />;
}

// loader 在渲染组件"之前"就把数据准备好
async function userLoader({ params }) {
  const res = await fetch(`/api/users/${params.id}`);
  return res.json();
}
```

**详解**：React Router v6.4 起（v7 主推）引入了"**数据路由**"——用 `createBrowserRouter` 以**配置对象数组**的方式定义路由（而非 JSX），再用 `<RouterProvider>` 渲染。它最大的新能力是 **`loader`**：在页面组件渲染**之前**就先加载好数据，避免"先渲染空壳再请求"的瀑布式加载，配合组件里的 `useLoaderData()` 读取。它还支持 `action`（处理表单提交）等。这是官方推荐的现代方式，但声明式的 `<Routes>`（前面示例）依然完全可用、上手更简单。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 267：React Router 常用 API 小结</h3>

```jsx
// 配置
<BrowserRouter>          // 应用最外层包裹
<Routes> / <Route>       // 定义"路径 → 组件"
<Route path=":id">       // 动态参数
<Route index>            // 默认子路由
<Route path="*">         // 404 兜底
<Outlet />               // 嵌套路由的子内容占位

// 导航
<Link to="/x">           // 声明式跳转（不刷新）
<NavLink to="/x">        // 带"当前项高亮"的 Link
<Navigate to="/x" />     // 重定向

// Hooks
useNavigate()   // 编程式跳转
useParams()     // 读取路径参数 :id
useSearchParams() // 读取/修改查询参数 ?a=b
useLocation()   // 当前 URL 信息
```

**详解**：把本章的 API 汇总成速查表。**学习建议**：先熟练掌握"声明式"这套（`BrowserRouter` + `Routes` + `Route` + `Link` + `useNavigate` + `useParams`），它能覆盖绝大多数需求、也最好理解。等做复杂应用（需要在渲染前加载数据、处理表单提交）时，再学 `createBrowserRouter` 的数据路由与 `loader`/`action`。React Router 是学 React 之后**最应该掌握的第一个生态库**。

---

---
[← 上一章](13-进阶与实战.md) · [📖 目录](README.md) · [下一章 →](15-数据请求ReactQuery.md)
