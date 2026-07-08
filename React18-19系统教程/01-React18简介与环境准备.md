# 第一章 · React 18 简介与环境准备

> 本文是《React 18 & 19 系统教程》的第 1 章。完整目录见 [README](README.md)。

**React 是什么？** React 是一个用于构建用户界面（UI）的 JavaScript 库。它的核心思想是"组件化"（把界面拆成一个个可复用的小块）和"声明式"（你只需描述"界面长什么样"，React 负责在数据变化时高效地更新真实页面），你不用手动操作 DOM。

**React 18 带来了什么？** React 18 最重要的变化是引入了**并发渲染（Concurrent Rendering）**。可以把它理解为：React 从"一件事必须一口气做完、期间会卡住页面"升级为"渲染可以被中断、暂停、恢复，优先响应用户操作"。这套底层能力衍生出以下新特性：

- **新的根 API `createRoot`**：React 18 的新入口，取代旧的 `ReactDOM.render`，用它才能启用并发特性。
- **自动批处理（Automatic Batching）**：多次 `setState` 会自动合并成一次重新渲染，减少不必要的渲染。
- **`startTransition` / `useTransition` / `useDeferredValue`**：区分"紧急更新"（如打字）和"非紧急更新"（如大列表过滤），保证界面流畅。
- **新 Hook**：`useId`、`useSyncExternalStore`、`useInsertionEffect`。
- **更完善的 `Suspense`**：更好地处理异步加载状态。

> 本章目标：把一个 React 18 项目从"零"跑起来，并理解入口文件里每一行的作用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 1：创建一个 React 18 项目</h3>

```bash
# 使用 Vite（推荐，启动快、配置少）
npm create vite@latest my-app -- --template react
cd my-app
npm install
npm run dev
```

**这是什么？** 这几行命令用脚手架工具 Vite 生成一个开箱即用的 React 项目。

**为什么用 Vite？** 早期大家用 `create-react-app`（CRA），但它较慢、已逐渐停止维护。Vite 基于原生 ES 模块，启动和热更新几乎是秒级，是目前社区的主流选择。

**每行做了什么？**
- `npm create vite@latest my-app -- --template react`：创建名为 `my-app` 的项目，`--template react` 表示用 React 模板（想用 TypeScript 就换成 `react-ts`）。
- `cd my-app`：进入项目目录。
- `npm install`：安装 `package.json` 里声明的依赖（React、Vite 等）。
- `npm run dev`：启动开发服务器，终端会给出一个本地地址（默认 `http://localhost:5173`），在浏览器打开就能看到页面。

**注意**：确认本机已安装 Node.js（建议 18 或更高版本），否则命令会报错。安装完成后，项目里的 `src/main.jsx` 就是整个应用的入口，也就是下面示例 2 要讲的内容。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 2：React 18 的入口写法（createRoot）</h3>

```jsx
// main.jsx —— 整个应用的起点
import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';

const root = createRoot(document.getElementById('root'));
root.render(<App />);
```

**这是什么？** 这是 React 18 应用的"入口文件"，作用是把你写的 React 组件（`<App />`）挂载到网页的某个真实 DOM 节点上，让它显示出来。

**为什么需要它？** 你的 HTML 里通常有一个空容器，比如 `<div id="root"></div>`。React 本身不知道该把界面渲染到哪里，这个文件就负责把"React 世界"和"真实网页"连接起来。

**逐行详解：**
- `import { createRoot } from 'react-dom/client'`：从 `react-dom/client` 引入 `createRoot`。注意路径是 `react-dom/client`（带 `/client`），这是 React 18 的新路径。
- `document.getElementById('root')`：拿到 HTML 里那个 `id="root"` 的容器节点。
- `createRoot(容器)`：为这个容器创建一个 React"根"，返回一个 `root` 对象。**创建根之后，React 就以并发模式运行**。
- `root.render(<App />)`：把 `App` 组件渲染进这个根。`<App />` 是 JSX 写法，表示"渲染 App 这个组件"。

**一句话总结**：`createRoot(容器).render(<组件/>)` 是 React 18 启动应用的固定套路。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 3：对比 React 17 的旧写法（理解为什么要换）</h3>

```jsx
// React 17 及更早（已废弃，不要再用）
import ReactDOM from 'react-dom';
ReactDOM.render(<App />, document.getElementById('root'));

// React 18（推荐写法）
import { createRoot } from 'react-dom/client';
createRoot(document.getElementById('root')).render(<App />);
```

**为什么要讲旧写法？** 网上大量老教程、老项目仍在用 `ReactDOM.render`。了解区别能帮你看懂旧代码，也能明白升级时该改什么。

**两者的关键区别：**
- **旧写法** `ReactDOM.render(组件, 容器)`：一个函数同时接收组件和容器。它运行在"旧的同步渲染模式"，**无法使用 React 18 的并发特性**（如 `useTransition`、自动批处理的完整能力等）。
- **新写法** `createRoot(容器)` 先创建根，再 `.render(组件)`：把"创建根"和"渲染"分成两步。只有这样 React 才会启用并发渲染。

**如果继续用旧写法会怎样？** 在 React 18 里调用 `ReactDOM.render` 仍能工作，但控制台会警告它已废弃，并且你的应用会退回到"非并发"行为，享受不到新特性。所以升级到 React 18 的第一步，就是把入口改成 `createRoot`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 4：开启严格模式（StrictMode）</h3>

```jsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>
);
```

**什么是严格模式？** `StrictMode` 是 React 提供的一个"开发辅助工具"组件。你把应用（或某部分）包在 `<StrictMode>` 里，它**不会渲染任何真实界面、也不影响生产环境**，只在**开发阶段**帮你提前发现潜在问题和不规范的写法。

**为什么要开启严格模式？** 它能帮你在开发时就暴露以下几类隐患：
1. **检测不安全或过时的写法**：比如使用了已废弃的旧生命周期方法、旧版 Context API 等，会在控制台给出警告。
2. **暴露副作用的问题**：为了帮你验证组件是否"可重复挂载而不出错"，严格模式在开发环境下会**故意让组件多渲染一次、并让 `useEffect` 执行两次**（挂载→卸载→再挂载）。如果你的 `useEffect` 没写清理函数、或依赖了"只能执行一次"的假设，问题就会立刻显现（比如定时器重复、请求发两次）。这倒逼你写出正确、幂等、可清理的副作用代码。
3. **为未来的并发特性做准备**：并发渲染下，组件可能被 React 多次调用/中断，严格模式的双重调用能提前帮你发现"不纯"的渲染逻辑。

**几个常见疑问：**
- **"副作用执行两次"是 bug 吗？** 不是，这是**故意的、只在开发环境**发生。生产构建（`npm run build`）里只会执行一次。如果双重执行让你的代码出问题，说明你的代码本身有隐患，应该修复它，而不是关掉严格模式。
- **要不要开启？** 强烈建议开启。Vite、CRA 等脚手架默认就帮你加上了它。它没有任何运行时代价（生产环境会被完全忽略），却能帮你写出更健壮的代码。

**注意**：`<StrictMode>` 可以只包裹一部分组件树，实现局部开启，但通常直接包住整个 `<App />`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 5：卸载根节点（root.unmount）</h3>

```jsx
const root = createRoot(document.getElementById('root'));
root.render(<App />);

// 需要时可以彻底卸载整个 React 应用
root.unmount();
```

**这是什么？** `createRoot` 返回的 `root` 对象除了 `.render()`，还有一个 `.unmount()` 方法，用来**把整个 React 应用从容器里彻底移除**，清理掉它的所有组件、状态和事件监听。

**什么时候会用到？** 大多数单页应用（SPA）从头到尾只渲染一次、不会主动卸载，所以你平时几乎用不到它。它主要出现在这些场景：
- **微前端 / 嵌入式组件**：把一个 React 应用挂到某个宿主页面的局部区域，宿主在切换时需要把它干净地卸载掉。
- **测试**：每个测试用例结束后卸载组件，避免相互干扰。
- **手动集成**：在非 React 页面里临时挂载一个 React 小部件，用完再移除。

**注意**：`.unmount()` 要在对应的 `root` 上调用。卸载后，这个 `root` 就不能再 `.render()` 了，需要重新 `createRoot`。

---

---
[📖 目录](README.md) · [下一章 →](02-React页面的组成.md)
