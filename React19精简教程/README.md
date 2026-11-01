# React 19 精简教程：日常开发够用的基本功

> 版本基线：`react 19.3.x` + `react-dom 19.3.x` ｜ `vite 8.3.x`（`react-ts` 模板，create-vite 9.3.3）｜ `typescript 7.0.x`（`strict` 开启，禁止 `any`）｜ `@types/react 19.3.x` ｜ `react-router 8.5.1`（第 9 章起）｜ Node.js 24 LTS（≥ 22.22）｜ VS Code
> 第 15 章后端：.NET 10 SDK（实测 10.0.403）+ C# 14 + ASP.NET Core Minimal API ｜ Visual Studio 2026 或命令行 `dotnet`
> 版本于 2026-10-30 用 `npm create vite@latest -- --template react-ts` 实际生成项目核对；全部代码在 Node.js 24.22.2 上实测。
>
> 篇幅：讲清楚、全面、连贯优先，单章正文不受 400 行限制（规范 6.1，2026-10-30 确认）；每章新概念仍 ≤ 3 个。代码一律给完整文件，不用「见仓库」代替。
>
> 每章固定骨架：这一章解决什么问题 · 开始之前 · 正文（🖼 图解 → 💻 可运行代码 → 🔍 拆解结果 → 改坏一处试试）· ⚠️ 踩坑警告 · 🎯 动手练习 · 📌 一句话总结 · 下一章 · 参考答案

---

## 这本教程写给谁

- 有多年后端/业务系统开发经验，但现代前端是全新学习。
- 想用最短时间掌握 React 日常开发的基本方法：组件、数据、交互、表单、取数、路由、打包。
- 要的是「能跑起来的页面」，不是 React 概念大全。

全书用一个**产品价格清单**小应用贯穿：从一个静态页面开始，每章加一点功能，最后得到带列表、筛选、新增、详情页、数据加载的完整前端，并打包成可部署的静态文件。

第 1–10 章的数据来自项目里的 `public/products.json`；第 11–14 章换成项目自带的本地模拟接口（`mock-server/server.ts`，只用 Node.js 自带功能），不需要安装后端或数据库；第 15 章用 .NET 10 Minimal API 写真正的后端（数据放内存，仍不需要数据库）。

**最短路径**：第 1–7 章 ≈ 6 小时 20 分，做出能加载数据、删除、入库、录入新产品的单页清单。
**完整路径**：15 章 ≈ 13 小时 50 分，按每天 2 小时约 7 天。第 11–14 章学完，能做出连接接口、能增删改查、带登录的页面；第 15 章换成 ASP.NET Core Minimal API 后端，前后端合成一个网站。

---

## 进度

| 章节 | 状态 |
| --- | --- |
| 第 1A 章 装好工具，建项目并启动开发服务器 | ✅ |
| 第 1B 章 页面从哪来：index.html → main.tsx → App.tsx | ✅ |
| 第 1C 章 热更新：保存即生效，写错了看遮罩 | ✅ |
| 第 1D 章 打开 TypeScript 严格检查 | ✅ |
| 第 2A 章 函数组件：把页面拆成几块 | ✅ |
| 第 2B 章 JSX 规则：和 HTML 不一样的几处 | ✅ |
| 第 2C 章 用 import / export 把组件拆到各自的文件 | ✅ |
| 第 3A 章 props：给组件传数据 | ✅ |
| 第 3B 章 用 type 描述产品：对象 prop 与可选 prop | ✅ |
| 第 3C 章 children：把一段界面传给组件 | ✅ |
| 第 4A 章 useState：让组件记住会变的数据 | ✅ |
| 第 4B 章 事件处理：onClick 与事件对象的类型 | ✅ |
| 第 4C 章 重新渲染：state 变了之后发生什么 | ✅ |
| 第 5A 章 map + key：用一个数组生成整张表 | ✅ |
| 第 5B 章 数组的不可变更新：删除一行、修改一行 | ✅ |
| 第 5C 章 条件渲染：缺货标红、删光了显示「暂无产品」 | ✅ |
| 第 6A 章 受控输入框：输入框的内容由 state 决定 | ✅ |
| 第 6B 章 表单提交：onSubmit 与 preventDefault | ✅ |
| 第 6C 章 派生值：校验结果从 state 现算 | ✅ |
| 第 7A 章 useEffect：页面显示之后再做的事 | ✅ |
| 第 7B 章 依赖数组：什么变了才执行 effect | ✅ |
| 第 7C 章 在 useEffect 里加载数据：fetch、await 与清理函数 | ✅ |
| 第 7D 章 判别联合：用一个 state 表示加载状态 | ✅ |
| 第 8A 章 回调 props：子组件通知父组件 | ✅ |
| 第 8B 章 状态提升：两个兄弟组件共用一份数据 | ✅ |
| 第 8C 章 自定义 Hook：把取数逻辑抽成 useProducts | ✅ |
| 第 9A 章 路由表：按地址显示不同的页面 | ✅ |
| 第 9B 章 Link：换页不重新加载 | ✅ |
| 第 9C 章 URL 参数：一个详情页显示任意一个产品 | ✅ |
| 第 10A 章 CSS Modules：组件自己的样式不互相干扰 | ✅ |
| 第 10B 章 npm run build：打包成浏览器能直接用的文件 | ✅ |
| 第 10C 章 部署 dist：刷新 404 与子路径 | ✅ |
| 第 11 章 连接接口：开发代理、跨域与环境变量 | ✅ |
| 第 12 章 保存数据：新增、删除、修改写回接口 | ✅ |
| 第 13 章 编辑产品：表单初始值、保存后跳转、后端校验错误 | ✅ |
| 第 14 章 登录用户：Context 共享数据与调试工具 | ✅ |
| **第 15 章 换成真实后端：ASP.NET Core Minimal API** | ✅ |

---

## 目录

### 第一部分 把页面搭起来（约 2 小时 35 分）

第 1 章拆成 4 个小章，每个小章只讲一个技术点：

**[第 1A 章 装好工具，建项目并启动开发服务器](docs/ch01A-setup.md)**（30 分钟）
装 Node.js 和 VS Code，用 Vite 建项目，在浏览器里看到模板欢迎页。
新概念：Vite 开发服务器

**[第 1B 章 页面从哪来：index.html → main.tsx → App.tsx](docs/ch01B-entry.md)**（25 分钟）
找到页面的源头，把欢迎页换成自己的「产品价格清单」。
新概念：入口文件 `main.tsx` → `App.tsx`

**[第 1C 章 热更新：保存即生效，写错了看遮罩](docs/ch01C-hmr.md)**（20 分钟）
分清局部更新和整页刷新，读懂错误遮罩。
新概念：热更新（HMR）

**[第 1D 章 打开 TypeScript 严格检查](docs/ch01D-strict.md)**（20 分钟）
开发服务器不查类型，学会用编辑器和 `npm run build` 找类型错误。
新概念：`strict` 严格模式

第 2 章拆成 3 个小章，每个小章只讲一个技术点：

**[第 2A 章 函数组件：把页面拆成几块](docs/ch02A-components.md)**（20 分钟）
页头、卡片、页脚各写成一个组件，`App` 只负责摆放。
新概念：函数组件

**[第 2B 章 JSX 规则：和 HTML 不一样的几处](docs/ch02B-jsx.md)**（25 分钟）
从 HTML 复制过来为什么报错：根元素、闭合、`className`、花括号。
新概念：JSX 规则

**[第 2C 章 用 import / export 把组件拆到各自的文件](docs/ch02C-modules.md)**（25 分钟）
每个组件一个文件，`App.tsx` 只留摆放的代码。
新概念：`export` / `import`

第 3 章拆成 3 个小章，每个小章只讲一个技术点：

**[第 3A 章 props：给组件传数据](docs/ch03A-props.md)**（20 分钟）
同一个产品卡片组件，显示不同的产品。
新概念：props

**[第 3B 章 用 type 描述产品：对象 prop 与可选 prop](docs/ch03B-product-type.md)**（25 分钟）
产品的字段只定义一次，整个产品对象传给卡片；耗材按「瓶」计。
新概念：共享类型文件 `types.ts` · 可选 prop 与默认值 · `import type`

**[第 3C 章 children：把一段界面传给组件](docs/ch03C-children.md)**（20 分钟）
带标题的分组框，探头和耗材分开放。
新概念：`children`

### 第二部分 让页面能交互（3 章，约 2 小时 30 分）

第 4 章拆成 3 个小章，每个小章只讲一个技术点：

**[第 4A 章 useState：让组件记住会变的数据](docs/ch04A-usestate.md)**（25 分钟）
询价数量点一下就要变，但直接改变量页面没反应，本章解决这个。
新概念：`useState`

**[第 4B 章 事件处理：onClick 与事件对象的类型](docs/ch04B-events.md)**（25 分钟）
按住 Shift 点「+」一次加 10：从事件对象里读信息。
新概念：事件处理及其类型

**[第 4C 章 重新渲染：state 变了之后发生什么](docs/ch04C-rerender.md)**（30 分钟）
把渲染过程打印出来，看清快照和更新函数。
新概念：重新渲染的原理 · 更新函数

第 5 章拆成 3 个小章，每个小章只讲一个技术点：

**[第 5A 章 map + key：用一个数组生成整张表](docs/ch05A-map-key.md)**（20 分钟）
产品放进数组，用 map 生成表格行。
新概念：`map` + `key`

**[第 5B 章 数组的不可变更新：删除一行、修改一行](docs/ch05B-immutable.md)**（30 分钟）
能删除、能入库，学会正确修改 state 里的数组。
新概念：不可变更新

**[第 5C 章 条件渲染：缺货标红、删光了显示「暂无产品」](docs/ch05C-conditional.md)**（25 分钟）
按条件决定显示什么。
新概念：条件渲染（`&&`、`? :`）

第 6 章拆成 3 个小章，每个小章只讲一个技术点：

**[第 6A 章 受控输入框：输入框的内容由 state 决定](docs/ch06A-controlled.md)**（25 分钟）
录入表单的输入部分：编号、名称、分类、单价。
新概念：受控输入框

**[第 6B 章 表单提交：onSubmit 与 preventDefault](docs/ch06B-submit.md)**（25 分钟）
点保存或按回车，组装出新产品。
新概念：表单提交（`SubmitEvent`；`@types/react` 19.3 已把旧的 `FormEvent` 标为弃用）

**[第 6C 章 派生值：校验结果从 state 现算](docs/ch06C-derived.md)**（30 分钟）
名称为空、价格不是数字时给出提示，提示随输入实时变化。
新概念：派生值

### 第三部分 数据、复用与多页面（3 章，约 2 小时 40 分）

第 7 章拆成 4 个小章，每个小章只讲一个技术点：

**[第 7A 章 useEffect：页面显示之后再做的事](docs/ch07A-useeffect.md)**（20 分钟）
标签页标题显示产品种数。
新概念：`useEffect`

**[第 7B 章 依赖数组：什么变了才执行 effect](docs/ch07B-deps.md)**（20 分钟）
只在数量变化时改标题。
新概念：依赖数组

**[第 7C 章 在 useEffect 里加载数据：fetch、await 与清理函数](docs/ch07C-fetch.md)**（35 分钟）
页面打开时自动从 `public/products.json` 取数据，显示「加载中」「出错了」。
新概念：在 effect 里加载数据（`fetch`、`async` / `await`、清理函数）

**[第 7D 章 判别联合：用一个 state 表示加载状态](docs/ch07D-union.md)**（30 分钟）
三个 state 合成一个，不可能的组合写不出来。
新概念：判别联合

第 8 章拆成 3 个小章，每个小章只讲一个技术点：

**[第 8A 章 回调 props：子组件通知父组件](docs/ch08A-callback.md)**（30 分钟）
表格拆成组件，表单录入的产品加进表格。
新概念：回调 props

**[第 8B 章 状态提升：两个兄弟组件共用一份数据](docs/ch08B-lift-state.md)**（25 分钟）
搜索框和表格联动。
新概念：状态提升

**[第 8C 章 自定义 Hook：把取数逻辑抽成 useProducts](docs/ch08C-custom-hook.md)**（30 分钟）
加载和增删改逻辑抽出来，App 只剩页面结构。
新概念：自定义 Hook

第 9 章拆成 3 个小章，每个小章只讲一个技术点：

**[第 9A 章 路由表：按地址显示不同的页面](docs/ch09A-routes.md)**（30 分钟）
安装 React Router，`/`、`/about`、不存在的地址各显示不同页面。
新概念：路由表（`BrowserRouter` / `Routes` / `Route`）

**[第 9B 章 Link：换页不重新加载](docs/ch09B-link.md)**（20 分钟）
页头下面的导航，点一下就换页。
新概念：`Link`

**[第 9C 章 URL 参数：一个详情页显示任意一个产品](docs/ch09C-params.md)**（35 分钟）
从列表点进产品详情页，地址栏跟着变，刷新后仍停在详情页。
新概念：URL 参数（`useParams`）

### 第四部分 交付（1 章，约 40 分钟）

第 10 章拆成 3 个小章，每个小章只讲一个技术点：

**[第 10A 章 CSS Modules：组件自己的样式不互相干扰](docs/ch10A-css-modules.md)**（25 分钟）
表格的样式搬进模块，类名不再冲突。
新概念：CSS Modules

**[第 10B 章 npm run build：打包成浏览器能直接用的文件](docs/ch10B-build.md)**（20 分钟）
打包到 `dist`，看清楚里面是什么。
新概念：`npm run build`

**[第 10C 章 部署 dist：刷新 404 与子路径](docs/ch10C-deploy.md)**（30 分钟）
部署时最常见的两个问题：刷新子页面 404、子路径 `base`。
新概念：`dist` 部署（回退规则、`base`）

### 第五部分 连接后端（4 章，约 4 小时 10 分）

**[第 11 章 连接接口：开发代理、跨域与环境变量](docs/ch11-api-proxy.md)**（60 分钟）
启动本地模拟接口，数据改从接口加载；遇到跨域并用开发代理解决；接口地址放进 `.env`。
新概念：跨域（CORS）· 开发代理 · 环境变量

**[第 12 章 保存数据：新增、删除、修改写回接口](docs/ch12-save-data.md)**（70 分钟）
新增、删除、入库真正保存到服务器，刷新还在；处理保存中、失败，避开异步操作的两个 bug。
新概念：写请求（方法、请求头、请求体）· `await` 之后用更新函数改 state · 提交中与失败状态

**[第 13 章 编辑产品：表单初始值、保存后跳转、后端校验错误](docs/ch13-edit-validation.md)**（60 分钟）
编辑页带出原值，保存成功回到列表；「编号重复」这类服务器的错误显示在对应输入框下面。
新概念：用已有数据做表单初始值 · `useNavigate` · 后端字段错误映射到表单

**[第 14 章 登录用户：Context 共享数据与调试工具](docs/ch14-auth-context.md)**（60 分钟）
登录后各页面都能取到当前用户，按角色显示删除按钮；用 React DevTools 查看组件状态。
新概念：Context（`createContext` / `useContext`）· 按登录状态和角色显示界面 · React DevTools

### 第六部分 真实后端（1 章，约 80 分钟）

**[第 15 章 换成真实后端：ASP.NET Core Minimal API](docs/ch15-minimal-api.md)**（80 分钟）
用 .NET 10 Minimal API 写出和模拟接口相同的 6 个接口；前端只改代理端口和 `api.ts`；打包进后端 `wwwroot`，前后端合成一个网站。
新概念：Minimal API 接口与 ProblemDetails 错误格式 · 前端对接新的错误格式 · 同站点部署（`wwwroot` + 页面回退）

---

## 有意不讲的内容

| 内容 | 为什么不讲 | 什么时候需要 |
| --- | --- | --- |
| Redux / Zustand 等状态库 | 日常页面用 state + 状态提升 + 自定义 Hook 就够 | 很多不相邻的组件要共享同一份数据、改动频繁时 |
| `useReducer` | 状态更新逻辑复杂时才需要，`useState` 已能覆盖日常页面 | 一个组件里有很多互相关联的 state、更新规则复杂时 |
| TanStack Query 等取数库 | 先弄懂 `useEffect` 取数的原理再用库 | 需要缓存、自动重试、分页加载时 |
| React 19 Actions / `use()` / Server Components | 依赖框架或新写法，单纯 Vite 项目日常用不到 | 使用 Next.js 等框架时 |
| Next.js | 是在 React 上加了一层框架，概念更多 | 需要服务端渲染、SEO 时 |
| 单元测试 | 本教程目标是先把页面做出来 | 项目进入长期维护时 |

---

## 目录结构

```
React19精简教程/
├── README.md
├── docs/                      # 每章一个 Markdown
└── code/
    └── chNN/
        ├── price-list/        # 本章结束时的完整项目（不含练习答案、不含 node_modules）
        ├── steps/N.N/         # 本章中间步骤的文件（正文里标着「N.N 版本」的代码）
        └── answers/           # 练习参考答案，目录结构与项目相同
        （第 15 章另有 PriceApi/：后端 Minimal API 项目，和 price-list 并排）
```

每章的 `price-list` 都能直接运行：进入该目录执行 `npm install`，再执行 `npm run dev`。正文里所有带文件路径注释的代码块，都和这里的文件逐字一致。

---

## 实测说明

- 每章的代码都通过了 `tsc -b`（strict）、`vite build` 和 `oxlint`；页面行为、控制台报错、错误遮罩文字是在 Vite 开发服务器 + 无头 Chromium 中实际操作得到的。实测环境是 Linux，命令和输出与 Windows 相同，只有路径写法不同。
- 无法实测、正文已标注「⚠️ 未实机验证」的部分：Windows 下 Node.js / VS Code 的安装界面和菜单文字、PowerShell 执行策略报错、IIS 的 `web.config`（URL Rewrite）配置和反向代理、React DevTools 浏览器扩展的界面操作。
- 第 11–14 章的接口行为（状态码、返回内容、报错文字）都是启动模拟接口后实际请求得到的；第 15 章的后端在 .NET 10.0.403 上用 `dotnet run` 实测，Visual Studio 2026 的界面操作未实机验证。
