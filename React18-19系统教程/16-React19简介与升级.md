# 第十六章 · React 19 简介与升级

> 本文是《React 18 & 19 系统教程》的第 16 章。完整目录见 [README](README.md)。

## 关于 React 19 部分（React 18 之后的全部新内容）

> 上半部分我们系统学习了 React 18。**React 19 已于 2024 年 12 月正式发布（stable）**，它在 React 18 的并发能力之上，进一步把"数据变更 → 更新界面"这件最常见的事做成了内建能力（Actions），并新增了 `use` API、`ref` 直接作为 prop、文档元数据/样式表/资源预加载等一大批实用特性，同时移除了一批过时 API。
>
> 本部分**紧接上文的示例编号**（从示例 284 继续），风格与上半部分一致：先给概念，再给可运行示例，最后逐条**详解**。为了突出"新旧对比"，很多示例会同时给出 **React 18 的老写法** 和 **React 19 的新写法**。
>
> 阅读前提：你已经掌握上半部分的 Hooks、Suspense、`useTransition`、`createRoot` 等基础。


> React 19 不是"推倒重来"，而是在 React 18 并发架构上的增量升级。绝大多数 React 18 项目**改动很小**即可升级。本节先把"怎么装、入口要不要改、有哪些前置要求"讲清楚。

### （A）安装与升级

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 284：安装 React 19</h3>

```bash
# npm
npm install --save-exact react@^19.0.0 react-dom@^19.0.0

# 如果用 TypeScript，类型也要一起升级
npm install --save-exact @types/react@^19.0.0 @types/react-dom@^19.0.0

# yarn
yarn add --exact react@^19.0.0 react-dom@^19.0.0
```

**详解**：`react` 和 `react-dom` 必须**版本一致**，二者是配套的。用 `--save-exact`（或 yarn 的 `--exact`）锁定精确版本，避免自动升到不兼容的小版本。TypeScript 项目一定要同步升级 `@types/react`、`@types/react-dom`，否则会有大量类型报错（React 19 的类型定义有变化，例如 `ref` 现在是普通 prop）。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 285：入口写法不变——仍然用 createRoot</h3>

```jsx
// main.jsx —— 和 React 18 完全一样
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App.jsx';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>
);
```

**详解**：React 18 引入的 `createRoot` 在 React 19 里**依旧是标准入口**，写法不变。区别在于：React 18 里还"能用但会警告"的老入口 `ReactDOM.render` / `ReactDOM.hydrate`，在 React 19 里被**彻底移除**了（见示例 322）。所以如果你的项目还在用 `ReactDOM.render`，升级 19 前必须先改成 `createRoot`。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 286：升级前置要求——新版 JSX Transform</h3>

```jsx
// React 19 依赖 2020 年引入的"新 JSX 转换"
// 特征：文件顶部不再需要手动 import React 就能写 JSX
function Hello() {
  return <h1>无需 import React 即可使用 JSX</h1>;
}

// 如果构建工具仍用旧 transform，控制台会警告：
// "Your app (or one of its dependencies) is using an outdated JSX transform."
```

**详解**：React 19 的一些新能力（比如 `ref` 作为 prop、JSX 提速）**要求启用新 JSX Transform**。好消息是，Vite、Next.js、Create React App 等主流环境**默认早已启用**，绝大多数项目无需改动。只有很老的自定义 Babel 配置可能需要把 `@babel/preset-react` 的 `runtime` 设为 `"automatic"`。看到上面那条警告时，去更新构建配置即可。

**升级心智小结**：① 换 `react`/`react-dom`/类型三件套；② 确保 `createRoot` 入口；③ 确保新 JSX Transform；④ 处理被移除的 API（本部分最后一章）。官方还提供了一系列 codemod 自动改写，命令形如 `npx codemod@latest react/19/...`。

---

---
[← 上一章](15-数据请求ReactQuery.md) · [📖 目录](README.md) · [下一章 →](17-Actions.md)
