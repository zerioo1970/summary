# React 18 详细使用说明（100+ 示例）

> 本文档面向已了解 JavaScript / ES6 的开发者，通过 280 多个由浅入深的小示例，系统讲解 React 18 的用法，并覆盖 React Router 路由与 React Query 数据请求两大常用生态库。
> 每个示例都尽量短小、独立，方便直接复制运行。

---

## 目录

1. [React 18 简介与环境准备](#一react-18-简介与环境准备)
2. [JSX 基础](#二jsx-基础)
3. [组件与 Props](#三组件与-props)
4. [State 与事件](#四state-与事件)
5. [条件渲染与列表](#五条件渲染与列表)
6. [表单处理](#六表单处理)
7. [核心 Hooks](#七核心-hooks)
8. [React 18 新增 Hooks](#八react-18-新增-hooks)
9. [并发特性（Concurrent Features）](#九并发特性concurrent-features)
10. [性能优化](#十性能优化)
11. [Context 与组件通信](#十一context-与组件通信)
12. [进阶与实战](#十二进阶与实战)
13. [React Router 路由](#十三react-router-路由)
14. [数据请求（React Query）](#十四数据请求react-query)

---

## 一、React 18 简介与环境准备

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

## 二、JSX 基础

> **JSX 是什么？** JSX（JavaScript XML）是 React 提供的一种语法糖，让你能在 JavaScript 里用类似 HTML 的写法来描述界面。它不是字符串，也不是 HTML，而是"会被编译成 JavaScript 的特殊语法"。
>
> **为什么要用 JSX？** 因为界面结构（标签）和界面逻辑（JS）本来就紧密相关，JSX 让它们写在一起，直观且好维护。虽然 React 也能不用 JSX（直接调用 `React.createElement`），但那样非常啰嗦，几乎没人这么写。
>
> 本章从"最简单的一行 JSX"讲起，逐步覆盖表达式、属性、结构规则和内容渲染细节，共 24 个示例。

### （A）JSX 的本质与表达式

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 6：最简单的 JSX</h3>

```jsx
const element = <h1>Hello, React 18!</h1>;
```

**详解**：这一行看起来像 HTML，但它其实是 JavaScript。`element` 是一个普通的 JS 变量，值是一个"React 元素"（描述界面长什么样的对象）。注意它右边**没有引号**——`<h1>...</h1>` 不是字符串，而是 JSX 语法。这是理解 JSX 的第一步：**它是代码，不是文本**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 7：JSX 的本质——会被编译成 React.createElement</h3>

```jsx
// 你写的 JSX：
const element = <h1 className="title">你好</h1>;

// 编译工具（Babel/Vite）会把它转成等价的 JS：
const element = React.createElement('h1', { className: 'title' }, '你好');
```

**详解**：这是 JSX 最重要的原理。浏览器并不认识 JSX，所以构建工具会把每个 JSX 标签转换成 `React.createElement(标签, 属性对象, 子内容)` 的函数调用，最终得到一个描述 UI 的普通 JS 对象（称为"虚拟 DOM"）。理解这一点能帮你想通很多规则，比如：为什么属性用 `className`（因为它其实是对象的一个键）、为什么必须有单一根节点（因为一个函数调用只能返回一个对象）。**平时你不用手写 createElement，但知道 JSX 会变成它，很多疑惑就迎刃而解了。**

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 8：在 JSX 中嵌入变量（大括号 `{}`）</h3>

```jsx
const name = '张三';
const element = <h1>你好，{name}</h1>;
```

**详解**：在 JSX 里用一对大括号 `{}` 可以"嵌入"任何 JavaScript 表达式。这里 `{name}` 会被替换成变量 `name` 的值，最终渲染成"你好，张三"。**大括号是 JSX 与 JS 之间的桥梁**：括号外是"类 HTML 的结构"，括号内是"真正的 JS"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 9：大括号里可以放各种表达式</h3>

```jsx
const user = { firstName: '三', lastName: '张' };
const element = (
  <div>
    <p>字符串拼接：{'你好，' + user.lastName + user.firstName}</p>
    <p>调用方法：{user.lastName.toUpperCase()}</p>
    <p>访问数组：{[1, 2, 3][0]}</p>
    <p>三元表达式：{user ? '已登录' : '未登录'}</p>
  </div>
);
```

**详解**：`{}` 里能放的是**表达式**——即"能算出一个值的代码"。包括：变量、运算、函数调用、属性访问、三元表达式、`&&`、模板字符串等。上面演示了几种常见形式。记住关键词"**表达式**"，它是判断能不能写进 `{}` 的标准。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 10：大括号里不能放语句</h3>

```jsx
// ❌ 错误：if 是"语句"，不能直接写进 {}
// <p>{ if (x) { return 'a'; } }</p>

// ✅ 正确：改用三元"表达式"
const element = <p>{x ? 'a' : 'b'}</p>;

// ✅ 或者把逻辑提到 JSX 外面，用变量承接
let text;
if (x) text = 'a'; else text = 'b';
const element2 = <p>{text}</p>;
```

**详解**：这是新手最容易困惑的点。`{}` 里只能放"表达式"，**不能放 `if`、`for`、`switch` 这类"语句"**。原因回到示例 7——`{}` 里的内容最终要作为参数传给 `createElement`，而参数必须是一个值，语句不产生值。遇到复杂逻辑有两种办法：① 用三元/`&&` 等表达式；② 把逻辑写在 JSX 外面，用变量存结果再嵌入（第五章会详细展开）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 11：在 JSX 中做运算与调用函数</h3>

```jsx
function formatPrice(n) {
  return '¥' + n.toFixed(2);
}

const count = 3, price = 19.9;
const element = (
  <div>
    <p>总数：{count * 2}</p>
    <p>单价：{formatPrice(price)}</p>
    <p>时间：{new Date().getFullYear()} 年</p>
  </div>
);
```

**详解**：既然 `{}` 里是 JS 表达式，那当然可以做算术运算（`count * 2`）、调用自己写的函数（`formatPrice(price)`）、甚至调用内置 API（`new Date().getFullYear()`）。这让"数据"和"展示格式"能灵活结合。建议把复杂的格式化逻辑抽成函数（如 `formatPrice`），保持 JSX 简洁易读。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 12：在 JSX 中使用三元表达式（内联条件）</h3>

```jsx
const isVip = true;
const element = (
  <div>
    <p>{isVip ? '尊贵的会员' : '普通用户'}</p>
    <span>{isVip ? '⭐' : ''}</span>
  </div>
);
```

**详解**：因为 `if` 语句不能进 `{}`，所以 JSX 里做条件判断最常用三元表达式 `条件 ? A : B`。它是个表达式，能算出一个值。这里根据 `isVip` 显示不同文字和图标。（更多条件渲染的写法——`&&`、多分支、空状态等——集中在第五章详细讲解。）

### （B）JSX 属性（Attributes）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 13：JSX 属性基础</h3>

```jsx
const element = (
  <a href="https://react.dev" title="官方文档" target="_blank">
    React 官网
  </a>
);
```

**详解**：给 JSX 标签设置属性，写法和 HTML 很像：`属性名="值"`。这些属性最终会变成 `createElement` 第二个参数（属性对象）里的键值对。大多数标准 HTML 属性都能直接用，但有几个特殊的（见下面 `className`、`style`）需要注意。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 14：className —— 为什么不用 class</h3>

```jsx
// ❌ 不要写 class
// const el = <div class="box">内容</div>;

// ✅ 要写 className
const el = <div className="box card">内容</div>;
```

**详解**：在 HTML 里设置 CSS 类用 `class`，但在 JSX 里必须写成 **`className`**。原因是：JSX 最终编译成 JS 对象（示例 7），而 `class` 是 JavaScript 的保留关键字（用于定义类），不能当对象的属性名，所以 React 用 `className` 代替。同理，HTML 的 `for` 属性（label 用）在 JSX 里要写成 `htmlFor`。多个类名之间用空格分隔，和 HTML 一样。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 15：style —— 内联样式用对象</h3>

```jsx
const element = (
  <div style={{ color: 'red', fontSize: 20, backgroundColor: '#eee' }}>
    带样式的文字
  </div>
);
```

**详解**：JSX 的 `style` 属性接收的**不是字符串，而是一个 JavaScript 对象**。注意三个细节：
1. **双层大括号 `{{ }}`**：外层 `{}` 表示"嵌入 JS 表达式"，内层 `{}` 是"对象字面量"，合起来就是"嵌入一个对象"。
2. **属性名用驼峰命名**：CSS 里的 `font-size`、`background-color` 要写成 `fontSize`、`backgroundColor`（因为带连字符的名字不能直接做 JS 对象的键）。
3. **数字默认单位是 px**：`fontSize: 20` 等价于 `20px`；需要其他单位就写成字符串，如 `width: '50%'`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 16：动态拼接 className</h3>

```jsx
function Button({ primary, disabled }) {
  const className = `btn ${primary ? 'btn-primary' : ''} ${disabled ? 'btn-disabled' : ''}`;
  return <button className={className}>按钮</button>;
}
```

**详解**：类名常常需要根据状态动态变化。因为 `className` 的值也能用 `{}` 嵌入表达式，所以可以用模板字符串拼接。这里根据 `primary`、`disabled` 决定加不加对应的类。当条件很多时，社区常用 `clsx` 或 `classnames` 这类小工具库来更优雅地拼接类名。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 17：属性值使用变量与表达式</h3>

```jsx
function Avatar({ url, size }) {
  return (
    <img
      src={url}                        // 值来自变量
      width={size}                     // 数字变量
      alt={'用户头像 ' + size + 'px'}   // 表达式
    />
  );
}
```

**详解**：属性值不一定是写死的字符串，也可以用 `{}` 嵌入变量或表达式（此时**不要加引号**）。规则是：**值是字面字符串用引号 `"..."`；值是 JS 表达式用大括号 `{...}`**。写成 `src="url"` 会把字符串 "url" 当地址（错误），写成 `src={url}` 才是用变量 `url` 的值。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 18：布尔属性与属性简写</h3>

```jsx
function Input({ isDisabled }) {
  return (
    <>
      <input disabled={isDisabled} />   {/* 用变量控制 */}
      <input disabled />                {/* 只写属性名，等价于 disabled={true} */}
      <input disabled={false} />        {/* 明确关闭 */}
    </>
  );
}
```

**详解**：像 `disabled`、`checked`、`readOnly` 这类布尔属性，可以传布尔值控制开关。只写属性名（如 `<input disabled />`）等价于 `disabled={true}`。要动态控制时，用 `disabled={变量}`。注意：想关闭时要写 `disabled={false}`，而不是干脆不写——虽然效果类似，但用变量显式控制更清晰。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 19：用展开运算符传递属性 `{...obj}`</h3>

```jsx
function TextInput(props) {
  // 把 props 里的所有属性一次性传给 input
  return <input {...props} />;
}

// 使用：<TextInput type="text" placeholder="请输入" maxLength={10} />
const buttonProps = { type: 'submit', className: 'btn' };
const el = <button {...buttonProps}>提交</button>;
```

**详解**：`{...对象}` 是 JS 的展开语法，在 JSX 里用它可以把一个对象的所有键值对"摊开"成属性，省去逐个书写。常用于"透传属性"——比如封装组件时，把外部传入的 `props` 原样转发给内部的原生元素。若展开后又单独写了同名属性，后写的会覆盖前面的（如 `<input {...props} type="password" />` 会强制 type 为 password）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 20：自定义 data-* 与无障碍 aria-* 属性</h3>

```jsx
const element = (
  <button
    data-id="123"                 // 自定义数据属性
    data-role="submit"
    aria-label="提交表单"          // 无障碍标签
    aria-disabled={false}
  >
    提交
  </button>
);
```

**详解**：和标准属性不同，`data-*`（自定义数据属性）和 `aria-*`（无障碍属性）在 JSX 里**保留连字符写法**，不用改成驼峰。`data-*` 用于在 DOM 上存放自定义数据；`aria-*` 用于提升可访问性（让屏幕阅读器等辅助设备理解界面）。这是 JSX 属性命名规则的一个例外，记住即可。

### （C）JSX 的结构规则

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 21：JSX 必须有唯一的根元素</h3>

```jsx
// ❌ 错误：返回了两个并列元素，没有共同的父节点
// function App() {
//   return (
//     <h1>标题</h1>
//     <p>段落</p>
//   );
// }

// ✅ 正确：用一个父元素包起来
function App() {
  return (
    <div>
      <h1>标题</h1>
      <p>段落</p>
    </div>
  );
}
```

**详解**：一段 JSX 必须有且只有一个"根元素"。原因还是回到示例 7——JSX 编译成 `createElement` 调用，而一个 `return` 只能返回一个值（一个元素对象），不能同时返回两个并列的元素。所以要么用一个真实标签（如 `<div>`）包裹，要么用下面讲的 Fragment。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 22：用 Fragment 包裹多个元素（不产生多余 DOM）</h3>

```jsx
import { Fragment } from 'react';

function Info() {
  return (
    <Fragment>
      <h1>标题</h1>
      <p>段落</p>
    </Fragment>
  );
}
```

**详解**：有时你只想满足"单一根节点"的要求，但**不想**在页面上多套一层 `<div>`（多余的 div 会打乱布局、影响 CSS）。这时用 `<Fragment>` 包裹：它满足"唯一根元素"的语法要求，但**不会渲染成任何真实 DOM 节点**。上面的代码最终在页面上只有 `<h1>` 和 `<p>`，没有额外的容器。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 23：Fragment 的简写 `<>...</>`</h3>

```jsx
function Info() {
  return (
    <>
      <h1>标题</h1>
      <p>段落</p>
    </>
  );
}
```

**详解**：因为 Fragment 用得很频繁，React 提供了简写：空标签 `<>` 和 `</>`。它和 `<Fragment>` 完全等价，且不用 `import`，更简洁。**唯一的限制**：简写形式不能带任何属性——如果你需要给 Fragment 加 `key`（比如在列表里循环生成，见第五章），就必须用完整的 `<Fragment key={...}>` 写法。日常包裹用 `<>` 即可。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 24：标签必须闭合（含自闭合标签）</h3>

```jsx
function Media() {
  return (
    <div>
      <img src="a.jpg" alt="图" />   {/* 自闭合，末尾必须有 /> */}
      <br />
      <input type="text" />
      <hr />
    </div>
  );
}
```

**详解**：JSX 比 HTML 更严格——**所有标签都必须闭合**。像 `<img>`、`<br>`、`<input>`、`<hr>` 这些在 HTML 里可以不闭合的"空元素"，在 JSX 里必须写成自闭合形式，即末尾加 `/>`（如 `<img ... />`）。忘记闭合会直接导致编译报错。有子内容的标签则要成对出现，如 `<div>...</div>`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 25：JSX 中的注释</h3>

```jsx
function App() {
  return (
    <div>
      {/* 这是 JSX 内部的注释，必须包在大括号里 */}
      <p>内容</p>
      {/* 多行注释
          也这样写 */}
    </div>
  );
}
```

**详解**：在 JSX 的标签之间写注释，要用 `{/* ... */}` 的形式——因为注释也要放进 `{}` 才能被 JSX 识别。不能像 HTML 那样用 `<!-- -->`，也不能在标签内容区直接写 `//`。在 JSX 外面（普通 JS 代码里）则照常用 `//` 或 `/* */`。

### （D）JSX 内容渲染细节

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 26：哪些值不会被渲染（null / undefined / false / true）</h3>

```jsx
function Demo() {
  return (
    <div>
      {null}        {/* 不渲染 */}
      {undefined}   {/* 不渲染 */}
      {false}       {/* 不渲染 */}
      {true}        {/* 不渲染 */}
      {0}           {/* ⚠️ 会渲染出 "0" */}
      {'文本'}       {/* 渲染文本 */}
    </div>
  );
}
```

**详解**：React 在渲染 `{}` 里的值时，对某些值会"忽略、什么都不显示"：`null`、`undefined`、`false`、`true` 都不渲染。**这正是条件渲染 `{条件 && <组件/>}` 能工作的基础**——条件为 `false` 时整体值是 `false`，于是什么都不显示。但要特别小心：数字 `0` 和空字符串会被当作有效内容渲染出来（`0` 会在页面上显示一个"0"），这是常见的坑（第五章示例会专门讲）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 27：在 JSX 中渲染数组</h3>

```jsx
function List() {
  const items = [<li key="a">苹果</li>, <li key="b">香蕉</li>];
  return <ul>{items}</ul>;
}
```

**详解**：`{}` 里可以直接放一个"元素数组"，React 会依次渲染数组里的每个元素。这就是列表渲染的底层原理——平时用 `array.map(...)` 生成的正是这样一个元素数组。注意数组里的每个元素都需要一个唯一的 `key` 属性，帮助 React 识别每一项（第五章会深入讲 `key` 的作用）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 28：多行 JSX 用小括号包裹</h3>

```jsx
function Card() {
  return (
    <div className="card">
      <h3>标题</h3>
      <p>正文</p>
    </div>
  );
}
```

**详解**：当 JSX 有多行时，习惯用一对小括号 `( ... )` 把它包起来，紧跟在 `return` 后面。**为什么？** JavaScript 有"自动分号插入"机制——如果 `return` 后面直接换行，JS 可能会自作主张在 `return` 后加分号，导致返回 `undefined`。用括号把 JSX 包住，就能安全地把它写成多行、清晰缩进。单行 JSX 则不需要括号。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 29：渲染原始 HTML 字符串（dangerouslySetInnerHTML）</h3>

```jsx
function RichText() {
  const html = '<b>加粗</b> 和 <i>斜体</i>';
  return <div dangerouslySetInnerHTML={{ __html: html }} />;
}
```

**详解**：默认情况下，JSX 会把字符串里的 HTML 标签当**纯文本**转义显示（比如直接输出 `<b>加粗</b>` 这几个字），这是 React 防止 XSS 攻击的安全设计。如果你确实需要把一段 HTML 字符串当真正的 HTML 渲染，就用 `dangerouslySetInnerHTML={{ __html: 字符串 }}`。

**⚠️ 为什么名字里带"dangerously（危险地）"？** 这是 React 故意起的警示性名字。如果这段 HTML 来自用户输入或不可信来源，攻击者可能注入恶意脚本（XSS 攻击）。**使用原则**：只对完全可信的内容使用它，或先用 `DOMPurify` 等库消毒后再渲染。绝大多数情况下你都不需要它。

---

## 三、组件与 Props

> **组件（Component）是什么？** 组件是 React 的核心——它是一个"返回 JSX 的函数"，代表界面上一块可复用的部分（一个按钮、一张卡片、一个导航栏，乃至整个页面）。你把界面拆成一个个组件，再像搭积木一样把它们组合起来。
>
> **Props 是什么？** Props（properties 的缩写）是"父组件传给子组件的数据"，好比给函数传参数。它让同一个组件能根据不同数据显示不同内容，从而实现复用。
>
> **一条重要原则——单向数据流**：数据只能从父组件通过 props 往子组件流动，子组件不能反过来修改收到的 props。本章从"最简单的组件"讲到"组件之间如何组合与通信"，共 28 个示例。

### （A）组件的定义

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 30：最简单的函数组件</h3>

```jsx
function Welcome() {
  return <h1>欢迎光临</h1>;
}
```

**详解**：这就是一个组件——一个**返回 JSX 的普通 JavaScript 函数**。定义好之后，就能像标签一样使用它：`<Welcome />`。可以把组件理解为"自定义的 HTML 标签"，只不过它的内容由你用 JS 决定。这是 React 一切的基础：**界面 = 一堆组件的组合**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 31：组件名必须以大写字母开头</h3>

```jsx
// ✅ 正确：大写开头，React 当作组件
function UserCard() { return <div>卡片</div>; }
const el1 = <UserCard />;

// ❌ 错误：小写开头，React 会当作普通 HTML 标签 <usercard>
function usercard() { return <div>卡片</div>; }
// const el2 = <usercard />;  // 不会渲染你的组件
```

**详解**：这是一条必须记住的硬性规则——**组件名首字母要大写**。原因回到 JSX 的本质：`<UserCard />` 会编译成 `React.createElement(UserCard, ...)`（把大写的当变量，即你的组件），而 `<div />`、`<usercard />` 会编译成 `React.createElement('div', ...)`（把小写的当字符串，即原生 HTML 标签）。所以组件小写开头会被误认为不存在的 HTML 标签，导致渲染失败。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 32：箭头函数组件</h3>

```jsx
const Welcome = () => {
  return <h1>欢迎光临</h1>;
};

// 单行返回可以省略 return 和大括号：
const Hello = () => <h1>你好</h1>;
```

**详解**：组件既可以用 `function` 声明，也可以用箭头函数赋值给一个变量。两者功能完全一样，选哪种看团队习惯。箭头函数在只返回一个表达式时，可以省略 `{}` 和 `return`（如 `Hello`），写法更简洁。注意变量名同样要大写开头。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 33：组件的返回值规则</h3>

```jsx
function A() { return <p>单个元素</p>; }              // ✅
function B() { return <><p>1</p><p>2</p></>; }        // ✅ 用 Fragment 包多个
function C() { return null; }                          // ✅ 什么都不渲染
function D() { return 123; }                           // ✅ 也可返回字符串/数字
// function E() { return <p>1</p><p>2</p>; }           // ❌ 多个并列元素没包裹
```

**详解**：组件的返回值有几种合法情况：① 单个 JSX 元素；② 用 Fragment（`<>...</>`）包裹的多个元素；③ `null`（表示不渲染任何内容，常用于条件隐藏）；④ 字符串或数字（直接作为文本渲染）。**不合法**的是直接返回多个并列元素而不包裹——原因见第二章示例 21（一个函数只能返回一个值）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 34：组件可以重复使用</h3>

```jsx
function Star() { return <span>⭐</span>; }

function Rating() {
  return (
    <div>
      <Star />
      <Star />
      <Star />
    </div>
  );
}
```

**详解**：组件最大的价值是**复用**。定义一次 `Star`，就能在任何地方用任意多次。这里 `Rating` 里放了三个 `<Star />`。每一次使用都是一个独立的实例（后面学了 state 会知道，它们各自的状态互不干扰）。想改所有星星的样子，只需改 `Star` 一处。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 35：把界面拆分成多个组件</h3>

```jsx
function Header()  { return <header>页头</header>; }
function Content() { return <main>正文</main>; }
function Footer()  { return <footer>页脚</footer>; }

function Page() {
  return (
    <div>
      <Header />
      <Content />
      <Footer />
    </div>
  );
}
```

**详解**：真实项目里，一个页面会拆成许多小组件，再由一个"父组件"把它们组装起来。好处是：每个组件职责单一、易读、易维护、可单独复用。拆分的经验法则是"**一个组件只做一件事**"——当一个组件变得又长又杂时，就是该拆分的信号。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 36：组件的导出与导入</h3>

```jsx
// Button.jsx —— 每个组件通常单独放一个文件
export default function Button() {
  return <button>按钮</button>;
}

// App.jsx —— 在别处导入使用
import Button from './Button';

function App() {
  return <Button />;
}
```

**详解**：工程中习惯把每个组件放在单独的文件里，用 `export` 导出、`import` 导入。`export default`（默认导出）在导入时名字可以随意起，一个文件只能有一个；也可以用命名导出 `export function Button() {}`，导入时要用 `import { Button }` 并保持同名，一个文件可以有多个。合理的文件拆分让项目结构清晰。

### （B）Props：父组件向子组件传数据

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 37：接收 props（父传子）</h3>

```jsx
function Welcome(props) {
  return <h1>你好，{props.name}</h1>;
}

// 使用时像写 HTML 属性一样传值：
// <Welcome name="李四" />   → 渲染 "你好，李四"
// <Welcome name="王五" />   → 渲染 "你好，王五"
```

**详解**：props 就是父组件传进来的数据。React 会把使用组件时写的所有属性（`name="李四"`）收集成一个对象，作为**第一个参数** `props` 传给组件函数。于是组件内部通过 `props.name` 读取。这就是复用的关键：同一个 `Welcome`，传不同的 `name` 就显示不同的问候语。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 38：解构 props（推荐写法）</h3>

```jsx
function Welcome({ name, age }) {
  return <p>{name}，{age} 岁</p>;
}

// 等价于：
function Welcome2(props) {
  const { name, age } = props;
  return <p>{name}，{age} 岁</p>;
}
```

**详解**：与其每次都写 `props.name`、`props.age`，不如在函数参数里直接用对象解构 `{ name, age }` 把需要的字段取出来。这是社区最主流的写法——一眼就能看出这个组件用到了哪些 props，代码也更简洁。两种写法完全等价，推荐用解构。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 39：传递不同类型的值（引号 vs 大括号）</h3>

```jsx
function Demo({ title, count, active }) {
  return <p>{title} / {count} / {active ? '开' : '关'}</p>;
}

// 字符串用引号，其它类型（数字、布尔、变量）用大括号：
// <Demo title="标题" count={5} active={true} />
```

**详解**：传 props 时要区分值的类型：
- **字符串**：可以直接用引号 `title="标题"`；
- **数字、布尔、数组、对象、变量、表达式**：必须用大括号 `count={5}`、`active={true}`。

常见错误是 `count="5"`——这样传进去的是字符串 `"5"` 而不是数字 `5`，做数学运算时会出问题。记住：**除了写死的字符串，其余一律用大括号**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 40：传递对象和数组</h3>

```jsx
function UserCard({ user, tags }) {
  return (
    <div>
      <h3>{user.name}</h3>
      <p>标签：{tags.join('、')}</p>
    </div>
  );
}

// 使用：
const user = { name: '张三', age: 20 };
const tags = ['前端', 'React'];
// <UserCard user={user} tags={tags} />
```

**详解**：props 不仅能传简单值，也能传对象、数组等复杂数据。传的时候用大括号包住变量（`user={user}`），组件内部就能像操作普通对象/数组那样使用它们（`user.name`、`tags.join()`）。这在传递一整条数据记录时非常常用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 41：props 默认值</h3>

```jsx
function Button({ text = '点击', type = 'default' }) {
  return <button className={type}>{text}</button>;
}

// <Button />                    → 显示"点击"，type 为 default
// <Button text="提交" />         → 显示"提交"，type 仍为 default
```

**详解**：在解构时用 `=` 给 props 设默认值，当父组件**没传**该 prop（值为 `undefined`）时就用默认值。这让组件更健壮、更好用——调用者只需传关心的 props，其余走默认。注意：只有 `undefined` 会触发默认值，如果显式传了 `null`，默认值不会生效。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 42：props 是只读的（不能修改）</h3>

```jsx
function Welcome({ name }) {
  // ❌ 绝对不要这样做：修改 props 会破坏单向数据流
  // name = name.toUpperCase();

  // ✅ 需要加工时，用一个新变量
  const upperName = name.toUpperCase();
  return <h1>你好，{upperName}</h1>;
}
```

**详解**：这是 React 的铁律——**props 是只读的，组件绝不能修改自己收到的 props**。React 遵循"单向数据流"：数据从父流向子，子只能读、不能改。如果子组件想改变数据，应该由父组件传一个"回调函数"下来，子组件调用它通知父组件去改（见示例 52、53）。需要基于 props 计算新值时，创建一个新变量即可。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 43：布尔 props 的简写</h3>

```jsx
function Modal({ visible, closable }) {
  return <div>{visible ? '显示' : '隐藏'}，{closable ? '可关闭' : '不可关闭'}</div>;
}

// 只写属性名，等价于传 true：
// <Modal visible closable />
// 等价于 <Modal visible={true} closable={true} />
```

**详解**：当 prop 是布尔值且想传 `true` 时，可以**只写属性名**，省略 `={true}`。这和 HTML 里 `<input disabled>` 是一个道理。要传 `false` 则必须显式写 `visible={false}`。这种简写在传递开关类 props（如 `disabled`、`loading`、`visible`）时很常见。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 44：props 重命名与默认值组合</h3>

```jsx
function Avatar({ src: imageUrl, size = 40 }) {
  // 把 src 重命名为 imageUrl，size 默认 40
  return <img src={imageUrl} width={size} height={size} alt="头像" />;
}

// <Avatar src="a.jpg" />          → 用默认 size 40
// <Avatar src="b.jpg" size={80} />
```

**详解**：解构时可以用 `原名: 新名` 给 prop 改一个组件内部更合适的名字（这里把 `src` 改成 `imageUrl`），也能同时设默认值。这利用的是 JS 对象解构的能力。重命名在避免命名冲突或让内部代码更清晰时有用，但不必滥用——多数时候保持原名即可。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 45：用展开运算符透传所有 props</h3>

```jsx
function Input(props) {
  return <input {...props} />;
}

// 外部传的所有属性都会转发给内部的 input：
// <Input type="text" placeholder="请输入" maxLength={10} disabled />
```

**详解**：`{...props}` 把 `props` 对象里的所有字段一次性"摊开"成属性传给内部元素，省去逐个转发。这在**封装原生元素**（如自定义 Input、Button）时特别有用——你不用预先知道调用者会传哪些属性，全部原样透传即可。这体现了组件封装的灵活性。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 46：透传并覆盖/扩展部分 props</h3>

```jsx
function PrimaryButton({ className, ...rest }) {
  // 把 className 单独取出来合并，其余用 rest 透传
  return <button className={`btn-primary ${className || ''}`} {...rest} />;
}

// <PrimaryButton onClick={fn} disabled>提交</PrimaryButton>
```

**详解**：常见需求是"我要固定某些属性，同时把其余属性透传下去"。用**剩余参数** `...rest` 把除已解构字段外的所有 props 收集起来，再 `{...rest}` 展开。这里组件强制加上 `btn-primary` 类，又允许调用者补充自己的 `className` 和其它属性（`onClick`、`disabled`）。注意展开的先后顺序会影响同名属性的覆盖结果。

### （C）children：组件的"内容"

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 47：children 属性基础</h3>

```jsx
function Card({ children }) {
  return <div className="card">{children}</div>;
}

// 使用：标签中间的内容会作为 children 传入
// <Card>
//   <p>这是卡片里的内容</p>
// </Card>
```

**详解**：`children` 是一个特殊的 prop——它代表**组件开、闭标签之间的内容**。当你写 `<Card><p>...</p></Card>` 时，中间的 `<p>...</p>` 会自动作为 `children` 传给 `Card`。组件用 `{children}` 决定把这些内容渲染在哪里。这是打造"容器型组件"（卡片、弹窗、布局）的基础。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 48：children 可以是任意内容</h3>

```jsx
function Box({ children }) {
  return <div className="box">{children}</div>;
}

// children 可以是文本、单个元素、多个元素，甚至其它组件：
// <Box>纯文本</Box>
// <Box><h1>标题</h1><p>段落</p></Box>
// <Box><Avatar src="a.jpg" /></Box>
```

**详解**：`children` 的内容非常灵活——可以是纯文本、一个元素、多个并列元素、其它组件，或它们的任意混合。组件不需要关心传进来的具体是什么，只管把 `{children}` 放到合适的位置。正是这种灵活性，让容器组件可以包裹任何内容，实现高度复用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 49：通过具名 props 传递 JSX（多个"插槽"）</h3>

```jsx
function Layout({ header, sidebar, content }) {
  return (
    <div className="layout">
      <div className="header">{header}</div>
      <div className="sidebar">{sidebar}</div>
      <div className="content">{content}</div>
    </div>
  );
}

// 使用：把 JSX 作为不同的 prop 传入
// <Layout
//   header={<h1>标题</h1>}
//   sidebar={<nav>菜单</nav>}
//   content={<p>正文</p>}
// />
```

**详解**：`children` 只有一个"位置"。如果组件需要**多个可填充的区域**（比如布局的头部、侧栏、正文），可以把 JSX 作为普通 props 传进去（值用大括号包住 JSX）。这相当于给组件开了多个"插槽"。这是构建灵活布局组件的常用技巧。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 50：组件组合（Composition）</h3>

```jsx
function Card({ children }) {
  return <div className="card">{children}</div>;
}
function Avatar({ src }) { return <img src={src} alt="" />; }

function UserCard({ user }) {
  return (
    <Card>
      <Avatar src={user.avatar} />
      <h3>{user.name}</h3>
    </Card>
  );
}
```

**详解**：把小组件放进别的组件里组装出更复杂的界面，这叫"组合"。这里 `UserCard` 复用了通用的 `Card` 容器，往里面塞了 `Avatar` 和标题。React 官方提倡"**组合优于继承**"——不需要类的继承，靠组件嵌套 + props/children 就能灵活地复用和扩展 UI。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 51：用 children 做通用布局/包裹组件</h3>

```jsx
function Panel({ title, children }) {
  return (
    <section className="panel">
      <div className="panel-title">{title}</div>
      <div className="panel-body">{children}</div>
    </section>
  );
}

// <Panel title="用户信息">
//   <p>姓名：张三</p>
//   <p>年龄：20</p>
// </Panel>
```

**详解**：`children` 常和普通 props 搭配使用：普通 props 传"配置"（如标题 `title`），`children` 传"主体内容"。这样一个 `Panel` 组件就能承载任意内容，同时保持统一的外观结构。这是实现设计系统里"卡片、面板、对话框"等通用组件的标准模式。

### （D）组件通信与进阶

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 52：传递函数作为 prop（子触发父）</h3>

```jsx
function Child({ onAction }) {
  return <button onClick={onAction}>触发</button>;
}

function Parent() {
  const handle = () => alert('子组件触发了父组件的函数');
  return <Child onAction={handle} />;
}
```

**详解**：既然数据只能父传子，那子组件怎么"通知"父组件？答案是——**父组件把一个函数当作 prop 传给子组件**，子组件在合适的时机调用它。这里父组件把 `handle` 作为 `onAction` 传下去，子组件点击时调用 `onAction`，实际执行的是父组件的 `handle`。这是子→父通信的核心机制。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 53：子组件回传数据给父组件</h3>

```jsx
function SearchInput({ onSearch }) {
  return (
    <input
      placeholder="输入后回车"
      onKeyDown={(e) => {
        if (e.key === 'Enter') onSearch(e.target.value); // 把值回传给父
      }}
    />
  );
}

function Parent() {
  const handleSearch = (keyword) => {
    console.log('父组件收到关键字：', keyword);
  };
  return <SearchInput onSearch={handleSearch} />;
}
```

**详解**：子组件调用父传来的回调时，可以**带上参数**，从而把数据"回传"给父组件。这里子组件把输入框的值通过 `onSearch(值)` 传出去，父组件的 `handleSearch` 就能拿到。这就是"数据向上流动"的实现方式——本质仍是单向数据流，只是借助回调函数把数据从子传回父。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 54：把组件作为 prop 传递</h3>

```jsx
function List({ items, renderItem }) {
  return <ul>{items.map(renderItem)}</ul>;
}

// 使用者决定每一项怎么渲染：
// <List
//   items={users}
//   renderItem={(u) => <li key={u.id}>{u.name}</li>}
// />
```

**详解**：props 甚至可以是"一个返回 JSX 的函数"（称为 render prop）。这里 `List` 只负责遍历，而"每一项长什么样"交给使用者通过 `renderItem` 决定。这让组件更通用——同一个 `List` 能渲染用户列表、商品列表等任意内容。这是一种高级但很强大的复用模式。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 55：条件性地传递 props</h3>

```jsx
function Button({ disabled, onClick, children }) {
  return <button disabled={disabled} onClick={onClick}>{children}</button>;
}

function Form({ isSubmitting }) {
  return (
    <Button
      disabled={isSubmitting}
      onClick={isSubmitting ? undefined : () => console.log('提交')}
    >
      {isSubmitting ? '提交中...' : '提交'}
    </Button>
  );
}
```

**详解**：props 的值可以根据条件动态决定。这里根据 `isSubmitting` 状态，动态控制按钮是否禁用、点击行为、以及显示的文字。传 `undefined` 相当于不传该 prop（会走默认或视为没有）。灵活地根据状态传不同 props，是构建交互式 UI 的日常操作。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 56：用 PropTypes 校验 props 类型</h3>

```jsx
import PropTypes from 'prop-types';

function Greeting({ name, age }) {
  return <p>{name}，{age} 岁</p>;
}

Greeting.propTypes = {
  name: PropTypes.string.isRequired, // 必传的字符串
  age: PropTypes.number,             // 可选的数字
};
```

**详解**：`prop-types` 是一个用于在**开发阶段**校验 props 类型的库。如果父组件传错了类型（比如 `age` 传了字符串），或漏传了 `isRequired` 的必填项，控制台会给出警告，帮你及早发现 bug。它需要单独安装（`npm install prop-types`）。不过在现代项目中，**更推荐用 TypeScript** 来做类型检查——它能在编写代码时就提示错误，比运行时的 PropTypes 更强大。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 57：综合实战——可复用的商品卡片列表</h3>

```jsx
// 1) 通用容器组件
function Card({ children }) {
  return <div className="card">{children}</div>;
}

// 2) 展示单个商品（纯靠 props 显示，可复用）
function ProductCard({ product, onBuy }) {
  const { name, price, tags = [] } = product;
  return (
    <Card>
      <h3>{name}</h3>
      <p>¥{price.toFixed(2)}</p>
      <p>{tags.join(' / ')}</p>
      <button onClick={() => onBuy(product.id)}>购买</button>
    </Card>
  );
}

// 3) 父组件：传数据 + 传回调
function ProductList({ products }) {
  const handleBuy = (id) => console.log('购买商品', id);
  return (
    <div className="list">
      {products.map((p) => (
        <ProductCard key={p.id} product={p} onBuy={handleBuy} />
      ))}
    </div>
  );
}
```

**详解**：这个例子综合运用了本章几乎所有知识点：
1. **组件拆分**：容器 `Card`、展示 `ProductCard`、父级 `ProductList` 各司其职；
2. **组件组合**：`ProductCard` 复用了 `Card`；
3. **props 传数据**：把 `product` 对象传给子组件，内部解构并设默认值（`tags = []`）；
4. **函数 prop 通信**：父组件把 `handleBuy` 传下去，子组件点击时带上 `product.id` 回调；
5. **列表渲染 + key**：`map` 遍历时给每项加 `key`。

把这个例子看懂、能自己默写出来，就基本掌握了"组件 + Props"这一 React 最核心的部分。

---

## 四、State 与事件

> **State（状态）是什么？** state 是组件"自己的、会变化的数据"。props 是父组件传进来的（只读），而 state 是组件内部管理、可以随用户交互而改变的数据（比如计数器的数字、输入框的内容、开关的开/关）。
>
> **核心机制**：当你用 React 提供的方法更新 state 时，React 会**自动重新渲染**这个组件，让界面反映最新的数据。这就是 React"数据驱动界面"的精髓——你只管改数据，界面自动更新。
>
> **事件（Event）**：用户的点击、输入、按键等操作。React 用 `onClick`、`onChange` 等属性来绑定事件处理函数。state 通常在事件处理函数里被更新。
>
> 本章从"最简单的计数器"讲到"完整的交互组件"，共 28 个示例。

### （A）State 基础

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 58：最简单的 state（计数器）</h3>

```jsx
import { useState } from 'react';

function Counter() {
  const [count, setCount] = useState(0);
  return (
    <button onClick={() => setCount(count + 1)}>
      点击了 {count} 次
    </button>
  );
}
```

**详解**：`useState` 是最常用的 Hook，用来给组件添加一个状态。`useState(0)` 表示"创建一个初始值为 0 的状态"。它返回一个包含两项的数组：第一项 `count` 是当前状态值，第二项 `setCount` 是"更新它的函数"。点击按钮时调用 `setCount(count + 1)`，React 就会把 `count` 加 1 并**重新渲染**，界面上的数字随之更新。这就是一个完整的交互闭环。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 59：为什么要用 state，而不是普通变量</h3>

```jsx
// ❌ 用普通变量：点击时变量确实变了，但界面不会更新
function Broken() {
  let count = 0;
  return <button onClick={() => { count++; console.log(count); }}>{count}</button>;
}

// ✅ 用 state：更新会触发重新渲染，界面才会变
function Works() {
  const [count, setCount] = useState(0);
  return <button onClick={() => setCount(count + 1)}>{count}</button>;
}
```

**详解**：这是理解 state 的关键。普通变量 `let count` 改了之后，① 界面不会重新渲染（React 不知道数据变了）；② 而且组件每次重新渲染时，普通变量都会被重新初始化为 0，无法"记住"上次的值。而 state 有两个特殊能力：**更新它会触发重新渲染**，且 **React 会在多次渲染之间"记住"它的值**。所以凡是"变化后需要反映到界面上"的数据，都必须用 state。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 60：useState 语法详解</h3>

```jsx
const [count, setCount] = useState(0);
//     ↑当前值  ↑更新函数        ↑初始值

// 命名约定：更新函数用 set + 状态名（驼峰）
const [name, setName] = useState('');
const [isOpen, setIsOpen] = useState(false);
```

**详解**：`useState` 用到了 JS 的"数组解构"语法。它返回的其实是一个两元素数组 `[值, 更新函数]`，用 `[count, setCount]` 把它们取出来。名字可以随便起，但社区有强约定：**更新函数用 `set` + 状态名的驼峰形式**（`count` → `setCount`，`isOpen` → `setIsOpen`）。初始值作为 `useState()` 的参数传入，只在**第一次渲染**时用一次。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 61：state 可以是任意类型</h3>

```jsx
function Types() {
  const [count, setCount] = useState(0);        // 数字
  const [name, setName] = useState('张三');      // 字符串
  const [isOn, setIsOn] = useState(false);      // 布尔
  const [list, setList] = useState([]);         // 数组
  const [user, setUser] = useState({ id: 1 });  // 对象
  return <p>{count}-{name}-{String(isOn)}</p>;
}
```

**详解**：state 的值可以是任何 JavaScript 类型：数字、字符串、布尔、数组、对象，甚至 `null`。你根据要存的数据选择合适的类型。简单值（数字、字符串、布尔）更新起来最直接；数组和对象因为是"引用类型"，更新时有讲究（见后面的示例 66–73）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 62：使用多个 state</h3>

```jsx
function Form() {
  const [name, setName] = useState('');
  const [age, setAge] = useState(0);
  return (
    <>
      <input value={name} onChange={e => setName(e.target.value)} />
      <input value={age} onChange={e => setAge(Number(e.target.value))} />
      <p>{name}，{age} 岁</p>
    </>
  );
}
```

**详解**：一个组件里可以调用多次 `useState`，声明多个互相独立的状态。这里 `name` 和 `age` 各管各的，更新其中一个不影响另一个。**建议按"关注点"拆分 state**——把不相关的数据分开放，而不是硬塞进一个大对象。（当多个 state 关系紧密、更新逻辑复杂时，可考虑用第七章的 `useReducer`。）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 63：布尔 state 与切换（toggle）</h3>

```jsx
function Toggle() {
  const [isOpen, setIsOpen] = useState(false);
  return (
    <div>
      <button onClick={() => setIsOpen(!isOpen)}>
        {isOpen ? '收起' : '展开'}
      </button>
      {isOpen && <p>这是展开的内容</p>}
    </div>
  );
}
```

**详解**：布尔 state 用于表示"开/关"类状态（展开/收起、显示/隐藏、选中/未选中）。切换时用 `setIsOpen(!isOpen)` 取反当前值。配合条件渲染 `{isOpen && ...}`，就能实现"点击切换显示"的常见交互。这是最基础也最高频的 state 用法之一。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 64：惰性初始化 state（初始值来自昂贵计算）</h3>

```jsx
function Expensive() {
  // 传"函数"而不是"值"，这个函数只在首次渲染执行一次
  const [value, setValue] = useState(() => {
    console.log('只计算一次');
    return computeExpensiveValue();
  });
  return <p>{value}</p>;
}
function computeExpensiveValue() { return 42; }
```

**详解**：`useState` 的初始值如果需要一次昂贵的计算（比如读 localStorage、大量运算），不要直接写 `useState(computeExpensiveValue())`——那样**每次渲染都会调用它**（虽然结果只有第一次被采用，但计算白白执行了）。正确做法是**传一个函数** `useState(() => computeExpensiveValue())`，React 只在首次渲染时调用它一次。这叫"惰性初始化"。

### （B）正确地更新 state

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 65：更新 state 会触发重新渲染</h3>

```jsx
function Clock() {
  const [time, setTime] = useState(new Date().toLocaleTimeString());
  return (
    <div>
      <p>当前时间：{time}</p>
      <button onClick={() => setTime(new Date().toLocaleTimeString())}>
        刷新
      </button>
    </div>
  );
}
```

**详解**：每次调用 `setTime(...)`，React 都会做两件事：① 把 state 更新成新值；② **重新执行整个组件函数**（重新渲染），用新的 state 值生成新界面。理解"更新 state → 组件重跑 → 界面更新"这条链路，是理解 React 的核心。组件函数会被反复调用，所以别在函数体里写会产生副作用的代码（那属于 `useEffect` 的活，见第七章）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 66：不要直接修改 state（不可变原则）</h3>

```jsx
function Demo() {
  const [user, setUser] = useState({ name: '张三', age: 20 });

  // ❌ 错误：直接改 state，React 检测不到变化，界面不更新
  const wrong = () => { user.age = 21; };

  // ✅ 正确：用 setUser 传入一个新对象
  const right = () => setUser({ ...user, age: 21 });

  return <button onClick={right}>{user.name}: {user.age}</button>;
}
```

**详解**：这是最重要的 state 规则——**永远不要直接修改 state，而要用更新函数传入新值**。原因：React 靠比较"新旧值的引用是否相同"来判断要不要重新渲染。直接改 `user.age`，对象引用没变，React 以为没变化，界面不更新。必须用 `{ ...user, age: 21 }` 创建一个**新对象**再 `setUser`。这条原则对对象和数组尤其关键（数字/字符串是原始值，本身不可变，不受影响）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 67：函数式更新（依赖旧值时）</h3>

```jsx
function Counter() {
  const [count, setCount] = useState(0);
  // 连续加 3 次，必须用函数式更新
  const addThree = () => {
    setCount(c => c + 1);
    setCount(c => c + 1);
    setCount(c => c + 1);
  };
  return <button onClick={addThree}>{count}</button>;
}
```

**详解**：当新值依赖旧值时，推荐用**函数式更新** `setCount(c => c + 1)`——参数 `c` 是 React 保证的"最新的 state 值"。为什么这里必须用它？因为下一个示例会讲：在同一个事件里连续调用 `setCount(count + 1)` 三次，`count` 都是同一个旧值（比如 0），三次都算成 0+1=1，结果只加了 1。而用 `c => c + 1`，React 会依次拿上一次的结果，最终正确地加到 3。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 68：为什么连续 setCount(count+1) 不生效</h3>

```jsx
function Counter() {
  const [count, setCount] = useState(0);
  const wrong = () => {
    setCount(count + 1); // count 是 0 → 设成 1
    setCount(count + 1); // count 还是 0 → 又设成 1
    setCount(count + 1); // count 还是 0 → 又设成 1，最终只 +1
  };
  return <button onClick={wrong}>{count}</button>;
}
```

**详解**：这解释了示例 67 的原因。在一次事件处理中，`count` 的值是"这次渲染时被冻结的快照"，整个函数里它都是同一个值（0）。所以三次 `setCount(count + 1)` 都是 `setCount(0 + 1)`，等于设了三次 1。而且 React 会把同一事件里的多次更新**批处理**（合并成一次渲染），最终 `count` 只变成 1。要连续基于最新值更新，就必须用函数式更新 `c => c + 1`（示例 67）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 69：state 为对象（更新要展开旧值）</h3>

```jsx
function Profile() {
  const [user, setUser] = useState({ name: '张三', age: 20, city: '北京' });
  // 只想改 age，但要把其它字段一起带上
  const grow = () => setUser({ ...user, age: user.age + 1 });
  return <button onClick={grow}>{user.name} {user.age} {user.city}</button>;
}
```

**详解**：更新对象型 state 时，因为要遵守"不可变原则"（示例 66），得创建新对象。用展开运算符 `{ ...user }` 先复制旧对象的所有字段，再覆盖要改的字段（`age: user.age + 1`）。如果只写 `setUser({ age: 21 })`，会丢掉 `name` 和 `city`！记住这个模式：**`{ ...旧对象, 要改的字段: 新值 }`**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 70：更新嵌套对象</h3>

```jsx
function Settings() {
  const [config, setConfig] = useState({
    theme: 'light',
    notify: { email: true, sms: false },
  });
  // 修改嵌套的 notify.sms，每一层都要展开
  const toggleSms = () =>
    setConfig({
      ...config,
      notify: { ...config.notify, sms: !config.notify.sms },
    });
  return <button onClick={toggleSms}>短信：{String(config.notify.sms)}</button>;
}
```

**详解**：嵌套对象更新时，**每一层都要展开复制**。修改 `config.notify.sms`，既要 `{ ...config }` 复制外层，又要 `{ ...config.notify }` 复制内层，再改 `sms`。层级太深时这会很啰嗦——这也是为什么 state 结构不宜嵌套过深，或可借助 Immer 这类库来简化。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 71：state 为数组（添加元素）</h3>

```jsx
function TodoList() {
  const [items, setItems] = useState(['学习']);
  const add = () => setItems([...items, '新任务']); // 展开旧数组 + 新元素
  return (
    <div>
      <button onClick={add}>添加</button>
      <ul>{items.map((t, i) => <li key={i}>{t}</li>)}</ul>
    </div>
  );
}
```

**详解**：给数组型 state 添加元素，同样要遵守不可变原则——**不要用 `items.push()`**（那是原地修改）。正确做法是用展开语法创建新数组：`[...items, 新元素]`（加到末尾）或 `[新元素, ...items]`（加到开头）。React 看到是新数组引用，才会重新渲染。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 72：state 为数组（删除元素）</h3>

```jsx
function List() {
  const [items, setItems] = useState(['a', 'b', 'c']);
  const remove = (index) => setItems(items.filter((_, i) => i !== index));
  return (
    <ul>
      {items.map((t, i) => (
        <li key={i}>{t} <button onClick={() => remove(i)}>删除</button></li>
      ))}
    </ul>
  );
}
```

**详解**：删除数组元素用 `filter`——它返回一个**新数组**，只保留满足条件的项。这里保留"下标不等于要删除下标"的所有项。`filter` 天然符合不可变原则（不改原数组），是删除的首选。若按 id 删，则写 `items.filter(item => item.id !== targetId)`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 73：state 为数组（修改某一项）</h3>

```jsx
function TodoList() {
  const [todos, setTodos] = useState([
    { id: 1, text: '学习', done: false },
    { id: 2, text: '运动', done: false },
  ]);
  const toggle = (id) =>
    setTodos(todos.map(t => t.id === id ? { ...t, done: !t.done } : t));
  return (
    <ul>
      {todos.map(t => (
        <li key={t.id} onClick={() => toggle(t.id)}
            style={{ textDecoration: t.done ? 'line-through' : 'none' }}>
          {t.text}
        </li>
      ))}
    </ul>
  );
}
```

**详解**：修改数组里的某一项用 `map`——遍历每一项，命中目标（`t.id === id`）就返回一个"改过的新对象"（`{ ...t, done: !t.done }`），其余原样返回。`map` 返回新数组，且被改的那一项也是新对象，完全符合不可变原则。**增删改分别对应 `[...arr, x]` / `filter` / `map`**，记住这三板斧就能应对绝大多数数组 state 操作。

### （C）事件处理

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 74：事件处理基础（onClick）</h3>

```jsx
function Button() {
  const handleClick = () => {
    alert('按钮被点击了');
  };
  return <button onClick={handleClick}>点我</button>;
}
```

**详解**：React 用 `onClick`、`onChange` 这类**驼峰命名**的属性来绑定事件（注意不是 HTML 的全小写 `onclick`）。属性值是一个函数——事件发生时 React 会调用它。这里把 `handleClick` 函数传给 `onClick`。事件处理函数常写成箭头函数，命名习惯是 `handle + 事件名`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 75：传递函数 vs 调用函数（高频错误）</h3>

```jsx
function Demo() {
  const handleClick = () => alert('点击');

  return (
    <>
      <button onClick={handleClick}>✅ 正确：传函数引用</button>
      {/* ❌ 错误：加了括号 = 渲染时立即执行，而不是点击时 */}
      {/* <button onClick={handleClick()}>错误</button> */}
      {/* ✅ 需要传参时用箭头函数包一层 */}
      <button onClick={() => handleClick()}>✅ 正确：箭头函数包裹</button>
    </>
  );
}
```

**详解**：这是新手最常犯的错误。`onClick={handleClick}` 传的是"函数本身"（点击时才调用），正确；而 `onClick={handleClick()}` 带了括号，意思是"**立即执行** `handleClick`，把它的返回值给 onClick"——这会在渲染时就弹窗，且行为错误。**规则**：绑定事件传函数引用（不加括号）；如果需要传参，用箭头函数包一层 `() => handleClick(参数)`（见示例 77）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 76：事件对象 event</h3>

```jsx
function Link() {
  const handle = (e) => {
    console.log('事件类型：', e.type);       // 'click'
    console.log('目标元素：', e.target);      // 被点击的 DOM
    console.log('坐标：', e.clientX, e.clientY);
  };
  return <a href="/" onClick={handle}>点我</a>;
}
```

**详解**：事件处理函数会自动收到一个"事件对象" `e`（习惯命名为 `e` 或 `event`），里面包含这次事件的详细信息：`e.type`（事件类型）、`e.target`（触发事件的元素）、鼠标坐标、按键信息等。React 的事件对象是"合成事件"（SyntheticEvent），对各浏览器做了统一封装，用法和原生基本一致。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 77：传参给事件处理函数</h3>

```jsx
function Buttons() {
  const handle = (id) => alert('点了按钮 ' + id);
  return (
    <>
      <button onClick={() => handle(1)}>按钮 1</button>
      <button onClick={() => handle(2)}>按钮 2</button>
    </>
  );
}
```

**详解**：想给事件处理函数传自定义参数时，用箭头函数包一层：`onClick={() => handle(1)}`。这样传给 `onClick` 的是"一个点击时才会执行 `handle(1)` 的新函数"，符合示例 75 的规则。如果既要传参又要用事件对象，写成 `onClick={(e) => handle(1, e)}`。列表里给每一项绑事件时，这个模式极其常用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 78：阻止默认行为（preventDefault）</h3>

```jsx
function Form() {
  const handleSubmit = (e) => {
    e.preventDefault(); // 阻止表单提交导致的页面刷新
    console.log('用 JS 处理提交，不刷新页面');
  };
  return (
    <form onSubmit={handleSubmit}>
      <button type="submit">提交</button>
    </form>
  );
}
```

**详解**：某些元素有"默认行为"——表单提交会刷新页面、点链接会跳转、右键会弹出菜单。调用 `e.preventDefault()` 可以阻止这些默认行为，改由你的 JS 代码接管。表单场景最典型：不加它，点提交按钮页面会整个刷新，破坏单页应用体验。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 79：阻止事件冒泡（stopPropagation）</h3>

```jsx
function Box() {
  return (
    <div onClick={() => console.log('外层 div 被点击')}>
      <button onClick={(e) => {
        e.stopPropagation(); // 阻止事件向上冒泡到外层 div
        console.log('只有按钮被点击');
      }}>
        点我不触发外层
      </button>
    </div>
  );
}
```

**详解**：事件默认会"冒泡"——点击内层按钮，事件会依次向上传播到外层 `div`，导致两个 `onClick` 都触发。调用 `e.stopPropagation()` 能阻止事件继续向上冒泡，这样点按钮时只执行按钮自己的处理函数。常用于弹窗（点内容区不关闭，点遮罩才关闭）等场景。注意区分：`preventDefault` 阻止默认行为，`stopPropagation` 阻止冒泡传播，两者不同。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 80：键盘事件</h3>

```jsx
function SearchBox() {
  const onKeyDown = (e) => {
    if (e.key === 'Enter') {
      alert('搜索：' + e.target.value);
    }
    if (e.key === 'Escape') {
      e.target.value = '';
    }
  };
  return <input onKeyDown={onKeyDown} placeholder="回车搜索，Esc 清空" />;
}
```

**详解**：键盘事件（`onKeyDown`、`onKeyUp`）的事件对象里，`e.key` 表示按下的键名（`'Enter'`、`'Escape'`、`'a'`、`'ArrowUp'` 等）。通过判断 `e.key` 实现快捷键、回车提交等交互。此外 `e.ctrlKey`、`e.shiftKey` 可判断是否同时按了组合键。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 81：常见事件类型一览</h3>

```jsx
function EventsDemo() {
  return (
    <div
      onClick={() => console.log('点击')}
      onDoubleClick={() => console.log('双击')}
      onMouseEnter={() => console.log('鼠标移入')}
      onMouseLeave={() => console.log('鼠标移出')}
    >
      <input
        onChange={(e) => console.log('输入变化', e.target.value)}
        onFocus={() => console.log('获得焦点')}
        onBlur={() => console.log('失去焦点')}
      />
    </div>
  );
}
```

**详解**：React 支持大量事件，都是驼峰命名。常见的有：鼠标类 `onClick`/`onDoubleClick`/`onMouseEnter`/`onMouseLeave`；表单类 `onChange`（输入变化）/`onFocus`（聚焦）/`onBlur`（失焦）/`onSubmit`（提交）；键盘类 `onKeyDown`/`onKeyUp`。其中 `onChange` 是表单开发的核心（下一节和第六章详讲）。

### （D）State + 事件综合与 React 18 特性

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 82：输入框与 state 联动（受控组件基础）</h3>

```jsx
function NameInput() {
  const [name, setName] = useState('');
  return (
    <div>
      <input value={name} onChange={(e) => setName(e.target.value)} />
      <p>你好，{name || '陌生人'}</p>
    </div>
  );
}
```

**详解**：这是 state 和事件结合的经典模式，也是"受控组件"的雏形。输入框的 `value` 绑定到 state（`value={name}`），用户每次输入触发 `onChange`，从 `e.target.value` 拿到最新输入值再 `setName` 更新 state，state 一变界面就刷新。数据流形成闭环：**state 决定输入框显示什么，输入又更新 state**。（表单的完整用法见第六章。）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 83：自动批处理（React 18 新行为）</h3>

```jsx
function Batching() {
  const [a, setA] = useState(0);
  const [b, setB] = useState(0);
  const handle = () => {
    setTimeout(() => {
      setA(x => x + 1);
      setB(x => x + 1); // React 18：这两次更新合并成一次重新渲染
    }, 100);
  };
  console.log('渲染');
  return <button onClick={handle}>{a}-{b}</button>;
}
```

**详解**："批处理"指 React 把同一时机的多次 state 更新**合并成一次重新渲染**，以提升性能。在 React 17，只有事件处理函数内的更新才会批处理；而在 `setTimeout`、Promise、原生事件里则不会（会渲染多次）。**React 18 的改进**：无论更新发生在哪里（包括 `setTimeout`、异步回调），都会自动批处理。上面点击后控制台只打印一次"渲染"，而不是两次。这是 React 18 开箱即用的性能优化。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 84：退出批处理（flushSync）</h3>

```jsx
import { flushSync } from 'react-dom';

function Demo() {
  const [count, setCount] = useState(0);
  const handle = () => {
    flushSync(() => setCount(c => c + 1)); // 强制立即同步更新并重渲染
    console.log('此时 DOM 已经更新了');
  };
  return <button onClick={handle}>{count}</button>;
}
```

**详解**：极少数情况下，你需要"立即"更新 DOM，而不想等批处理结束（比如更新后马上要读取新的 DOM 尺寸、或控制滚动位置）。`flushSync(() => {...})` 会强制里面的更新同步执行并立刻重新渲染 DOM。**注意**：它会打断批处理、影响性能，属于"逃生舱"，只在确有需要时使用，绝大多数场景不需要它。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 85：综合实战——带增删的任务清单</h3>

```jsx
import { useState } from 'react';

function TodoApp() {
  const [todos, setTodos] = useState([]);
  const [text, setText] = useState('');

  const add = () => {
    if (!text.trim()) return;                 // 空输入不添加
    setTodos([...todos, { id: Date.now(), text }]); // 添加（示例 71）
    setText('');                              // 清空输入框
  };
  const remove = (id) =>
    setTodos(todos.filter(t => t.id !== id)); // 删除（示例 72）

  return (
    <div>
      <input
        value={text}
        onChange={(e) => setText(e.target.value)}      // 受控输入（示例 82）
        onKeyDown={(e) => e.key === 'Enter' && add()}  // 回车添加（示例 80）
      />
      <button onClick={add}>添加</button>
      <ul>
        {todos.map(t => (                              // 列表渲染
          <li key={t.id}>
            {t.text}
            <button onClick={() => remove(t.id)}>删除</button>
          </li>
        ))}
      </ul>
      <p>共 {todos.length} 项</p>
    </div>
  );
}
```

**详解**：这个小应用综合了本章的核心知识：
1. **两个 state**：任务列表 `todos`（数组）和输入框内容 `text`（字符串）；
2. **受控输入**：`value` + `onChange` 双向联动（示例 82）；
3. **数组不可变更新**：添加用 `[...todos, 新项]`（示例 71），删除用 `filter`（示例 72）；
4. **事件处理**：点击添加、回车添加（示例 80）、点击删除并传参（示例 77）；
5. **边界处理**：空输入不添加、添加后清空输入框。

把这个例子亲手敲一遍并理解每一行，就真正掌握了 state 与事件的配合——这是几乎所有 React 交互功能的基础。

---

## 五、条件渲染与列表

> 本章从"最简单的条件判断"讲起，逐步过渡到列表渲染，再到两者结合的实战写法，共 30 个示例。
> 核心思想只有两条：**条件渲染 = 用 JavaScript 的判断决定返回什么 JSX**；**列表渲染 = 用数组的 `map` 把数据变成一组 JSX**。

### （A）条件渲染 —— 从最简单开始

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 86：最简单的条件——提前 return</h3>

```jsx
function Greeting({ isLoggedIn }) {
  if (isLoggedIn) {
    return <p>欢迎回来</p>;
  }
  return <p>请先登录</p>;
}
```

**详解**：这是最直观的写法。组件本质是一个函数，你完全可以用普通的 `if` 判断，然后 `return` 不同的 JSX。命中第一个 `return` 后函数就结束了，所以下面那行只有在 `isLoggedIn` 为假时才会执行。适合"整块内容完全不同"的场景。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 87：三元运算符（内联在 JSX 里）</h3>

```jsx
function Status({ online }) {
  return <p>{online ? '在线' : '离线'}</p>;
}
```

**详解**：当只是"一小段内容"随条件变化时，用 `if` 拆成两个 `return` 太啰嗦。JSX 的 `{}` 里可以放**表达式**，而三元 `条件 ? A : B` 正是一个表达式。`online` 为真显示"在线"，否则"离线"。记住：`{}` 里不能放 `if` 语句，但可以放三元表达式。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 88：三元里返回 JSX 元素</h3>

```jsx
function LoginButton({ isLoggedIn }) {
  return (
    <div>
      {isLoggedIn
        ? <button>退出登录</button>
        : <button>点击登录</button>}
    </div>
  );
}
```

**详解**：三元的两个分支不仅能返回字符串，也能返回完整的 JSX 元素。相比示例 86 的提前 return，这种写法能让"页面大部分相同、只有局部不同"的结构写在一起，一眼看清差异在哪。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 89：`&&` 短路渲染（有则显示，无则不显示）</h3>

```jsx
function Inbox({ count }) {
  return (
    <div>
      {count > 0 && <span>你有 {count} 条新消息</span>}
    </div>
  );
}
```

**详解**：`A && B` 的规则是——`A` 为真时返回 `B`，`A` 为假时返回 `A` 本身。当 `count > 0` 为 `true` 时，就渲染右边的 `<span>`；为 `false` 时整个表达式的值是 `false`，而 React 对 `false` 的处理是"什么都不渲染"。这是"满足条件才显示"的最常用写法。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 90：`&&` 的经典陷阱——数字 0 会被显示出来</h3>

```jsx
function List({ items }) {
  // ❌ 错误：当 items.length 为 0 时，页面上会出现一个"0"
  return <div>{items.length && <ul>...</ul>}</div>;
}

function ListFixed({ items }) {
  // ✅ 正确：把左边转成明确的布尔值
  return <div>{items.length > 0 && <ul>...</ul>}</div>;
}
```

**详解**：这是新手最容易踩的坑。`0 && <ul>` 的结果是 `0`，而 React **会把数字 0 当作有效内容渲染出来**（只有 `false`、`null`、`undefined` 才不渲染）。所以 `&&` 左边一定要是真正的布尔值，用 `length > 0`、`Boolean(x)` 或 `!!x` 来保证。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 91：`||` 提供默认内容（兜底）</h3>

```jsx
function UserName({ name }) {
  return <p>{name || '匿名用户'}</p>;
}
```

**详解**：`A || B` 表示 `A` 为真用 `A`，否则用 `B`。当 `name` 是空字符串、`null`、`undefined` 等假值时，就显示"匿名用户"。这是给缺省数据做兜底的简洁写法。若你希望 `0` 或 `''` 也算有效值，应改用空值合并 `??`（见示例 100）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 92：用 null 隐藏整个组件</h3>

```jsx
function Warning({ show }) {
  if (!show) return null; // 返回 null 表示"渲染但不产生任何 DOM"
  return <div className="warn">⚠️ 警告！</div>;
}
```

**详解**：组件返回 `null` 是完全合法的，表示"这个组件此刻不显示任何东西"。它和示例 89 的 `&&` 效果类似，但写在组件内部，适合"组件自己决定要不要显示"的封装场景（比如一个通用的提示框组件）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 93：先把 JSX 存进变量，再渲染</h3>

```jsx
function Page({ isLoading, data }) {
  let content;
  if (isLoading) {
    content = <p>加载中...</p>;
  } else if (!data) {
    content = <p>暂无数据</p>;
  } else {
    content = <div>{data.title}</div>;
  }
  return <div className="page">{content}</div>;
}
```

**详解**：当条件较复杂、分支较多时，把每个分支的 JSX 先赋值给一个变量，最后统一在 `return` 里使用，会比一长串嵌套三元清晰得多。这样外层结构（如 `<div className="page">`）只写一次，逻辑和结构分离，可读性高。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 94：多分支 if / else if</h3>

```jsx
function Grade({ score }) {
  if (score >= 90) return <span>优秀</span>;
  if (score >= 60) return <span>及格</span>;
  return <span>不及格</span>;
}
```

**详解**：多个区间判断时，连续的 `if + return` 是最清晰的表达方式。命中即返回，无需写 `else`。注意判断顺序要"从高到低"，否则 `score >= 60` 会先把 95 分也拦下。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 95：用 switch 处理多状态</h3>

```jsx
function StatusText({ status }) {
  switch (status) {
    case 'loading': return <span>加载中</span>;
    case 'success': return <span>成功</span>;
    case 'error':   return <span>失败</span>;
    default:        return <span>未知状态</span>;
  }
}
```

**详解**：当条件是"一个变量等于若干枚举值之一"时，`switch` 比一堆 `if` 更工整。别忘了 `default` 分支处理意外值。由于每个 `case` 都直接 `return`，所以不需要写 `break`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 96：用对象映射代替 switch（推荐）</h3>

```jsx
function Icon({ type }) {
  const map = {
    success: '✅',
    error: '❌',
    loading: '⏳',
  };
  return <span>{map[type] || '❓'}</span>;
}
```

**详解**：如果每个分支只是"取一个值"，用对象做"字典查表"比 `switch` 更简洁，也更容易扩展——加一种类型只需加一行。`map[type]` 取不到时用 `|| '❓'` 兜底。映射的值同样可以是 JSX 元素，不只是字符串。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 97：在 JSX 中用立即执行函数写复杂逻辑（IIFE）</h3>

```jsx
function Dashboard({ role }) {
  return (
    <div>
      {(() => {
        if (role === 'admin') return <AdminPanel />;
        if (role === 'user') return <UserPanel />;
        return <GuestPanel />;
      })()}
    </div>
  );
}
```

**详解**：JSX 的 `{}` 里只能放表达式、不能放语句。当你确实想在此处写 `if/switch` 这类语句，可以用"立即执行函数"`(() => { ... })()` 把语句包起来——它整体是一个表达式。不过多数情况下，示例 93（变量存 JSX）更易读，IIFE 应谨慎使用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 98：把条件判断抽成子组件</h3>

```jsx
function AuthButton({ isLoggedIn, onLogin, onLogout }) {
  return isLoggedIn
    ? <LogoutButton onClick={onLogout} />
    : <LoginButton onClick={onLogin} />;
}

function LoginButton({ onClick })  { return <button onClick={onClick}>登录</button>; }
function LogoutButton({ onClick }) { return <button onClick={onClick}>退出</button>; }
```

**详解**：当条件分支各自的逻辑变复杂时，与其在一个组件里堆砌，不如把每个分支抽成独立子组件。父组件只负责"选哪个"，子组件各自负责"怎么显示"。这就是组件化拆分的思路，让每部分职责单一、便于复用和测试。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 99：加载 / 错误 / 成功三态渲染（实战常见）</h3>

```jsx
function UserProfile({ loading, error, user }) {
  if (loading) return <p>加载中...</p>;
  if (error)   return <p style={{ color: 'red' }}>出错了：{error}</p>;
  if (!user)   return <p>暂无用户</p>;
  return (
    <div>
      <h3>{user.name}</h3>
      <p>{user.email}</p>
    </div>
  );
}
```

**详解**：几乎所有涉及数据请求的组件都要处理这三种（甚至四种）状态。用连续的提前 `return` 逐一"排除"异常情况，最后剩下的才是正常渲染。这种"卫语句"风格避免了深层嵌套，是处理异步 UI 的标准套路。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 100：可选链 `?.` 与空值合并 `??` 结合条件渲染</h3>

```jsx
function Profile({ user }) {
  return (
    <div>
      {/* user 可能为 null，用 ?. 安全访问，用 ?? 提供默认值 */}
      <p>城市：{user?.address?.city ?? '未填写'}</p>
      {user?.vip && <span>VIP 会员</span>}
    </div>
  );
}
```

**详解**：`user?.address?.city` 中任意一层为 `null/undefined` 都会安全地返回 `undefined`，不会报错；`?? '未填写'` 只在左边是 `null/undefined` 时兜底（区别于 `||`，`??` 不会把 `0`、`''` 当作缺失）。这三者配合能优雅处理"深层嵌套、可能缺失"的数据。

### （B）列表渲染 —— 把数组变成 JSX

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 101：最简单的列表——map 渲染字符串数组</h3>

```jsx
function Fruits() {
  const fruits = ['苹果', '香蕉', '橙子'];
  return (
    <ul>
      {fruits.map(fruit => <li key={fruit}>{fruit}</li>)}
    </ul>
  );
}
```

**详解**：列表渲染的核心是数组的 `map` 方法：它把数组里的每一项"映射"成一个 JSX 元素，最终得到一个 JSX 元素数组，React 会依次渲染它们。这里每个水果名唯一，所以直接用它当 `key`（下面会详细讲 key）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 102：map 带索引参数</h3>

```jsx
function RankList() {
  const players = ['小明', '小红', '小刚'];
  return (
    <ol>
      {players.map((name, index) => (
        <li key={name}>第 {index + 1} 名：{name}</li>
      ))}
    </ol>
  );
}
```

**详解**：`map` 的回调第二个参数是当前项的下标 `index`（从 0 开始）。这里用 `index + 1` 显示排名。注意：**用 index 来显示序号没问题，但用它当 `key` 要谨慎**（见示例 104）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 103：渲染对象数组</h3>

```jsx
function ProductList() {
  const products = [
    { id: 1, name: '手机', price: 3999 },
    { id: 2, name: '耳机', price: 299 },
  ];
  return (
    <ul>
      {products.map(p => (
        <li key={p.id}>{p.name} —— ¥{p.price}</li>
      ))}
    </ul>
  );
}
```

**详解**：真实数据几乎都是对象数组。每个对象通常自带一个唯一 `id`，这正是理想的 `key`。在回调里通过 `p.name`、`p.price` 访问对象的属性来构造 JSX。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 104：key 的作用与"不要用 index 当 key"</h3>

```jsx
function TodoList({ todos }) {
  // ✅ 推荐：用数据自带的稳定唯一 id
  return (
    <ul>
      {todos.map(todo => <li key={todo.id}>{todo.text}</li>)}
    </ul>
  );
}
```

**详解**：`key` 是 React 用来识别"列表里每一项是谁"的身份证。当列表增删、排序时，React 靠 `key` 判断哪些项该复用、哪些该新建或删除。

- **为什么不用数组 index 当 key？** 因为 index 会随位置变化。比如在列表开头插入一项，所有元素的 index 都变了，React 会误以为"内容全变了"，可能导致输入框内容错位、动画异常、性能下降等问题。
- **什么时候可以用 index？** 仅当列表是"静态的、永不重排/增删"时才勉强可用。
- **原则**：尽量用数据里稳定且唯一的字段（如 `id`）作为 key。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 105：用 filter 过滤后再渲染</h3>

```jsx
function ActiveUsers({ users }) {
  return (
    <ul>
      {users
        .filter(u => u.active)      // 先筛选出活跃用户
        .map(u => <li key={u.id}>{u.name}</li>)}
    </ul>
  );
}
```

**详解**：`filter` 先按条件把数组"过滤"成一个更小的数组，再用 `map` 渲染。链式调用 `filter().map()` 是极常见的组合。注意 `filter` 不改变原数组，而是返回新数组，符合 React"不可变数据"的理念。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 106：用 sort 排序后渲染（先拷贝再排序）</h3>

```jsx
function ScoreBoard({ scores }) {
  // 注意：sort 会修改原数组，所以先用 [...scores] 拷贝一份
  const sorted = [...scores].sort((a, b) => b.point - a.point);
  return (
    <ol>
      {sorted.map(s => <li key={s.id}>{s.name}：{s.point} 分</li>)}
    </ol>
  );
}
```

**详解**：`sort` 是"原地排序"，会**直接修改**传入的数组。如果 `scores` 来自 props 或 state，直接排序就等于偷偷改了原数据，可能引发 bug。正确做法是先用展开语法 `[...scores]` 复制一份再排。`(a, b) => b.point - a.point` 表示按分数从高到低。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 107：一次返回多个元素——带 key 的 Fragment</h3>

```jsx
function DefinitionList({ items }) {
  return (
    <dl>
      {items.map(item => (
        <React.Fragment key={item.id}>
          <dt>{item.term}</dt>
          <dd>{item.desc}</dd>
        </React.Fragment>
      ))}
    </dl>
  );
}
```

**详解**：有时每次循环需要返回**多个并列元素**（这里是 `<dt>` 和 `<dd>`），又不想额外包一层 `<div>`。此时用 `<React.Fragment>`，它不产生真实 DOM。注意：需要写 `key` 时，必须用完整的 `<React.Fragment key={...}>`，简写的 `<>...</>` 不支持 key。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 108：列表 + 条件——每一项内部再做条件渲染</h3>

```jsx
function TaskList({ tasks }) {
  return (
    <ul>
      {tasks.map(task => (
        <li key={task.id}>
          {task.text}
          {task.done && <span> ✅</span>}
          {task.urgent && <span style={{ color: 'red' }}> （紧急）</span>}
        </li>
      ))}
    </ul>
  );
}
```

**详解**：条件渲染和列表渲染经常嵌套使用。这里外层用 `map` 遍历任务，内层用 `&&` 决定每项是否显示"已完成"标记和"紧急"标签。这是真实列表 UI 最常见的形态。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 109：空列表的友好提示</h3>

```jsx
function MessageList({ messages }) {
  if (messages.length === 0) {
    return <p className="empty">📭 暂无消息</p>;
  }
  return (
    <ul>
      {messages.map(m => <li key={m.id}>{m.content}</li>)}
    </ul>
  );
}
```

**详解**：列表为空时如果什么都不显示，用户会以为页面出错了。养成习惯：先判断 `length === 0` 给出"空状态"提示，再渲染正常列表。这是条件渲染与列表渲染结合的典型场景。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 110：嵌套列表（列表里再套列表）</h3>

```jsx
function CategoryList({ categories }) {
  return (
    <div>
      {categories.map(cat => (
        <div key={cat.id}>
          <h4>{cat.name}</h4>
          <ul>
            {cat.items.map(item => (
              <li key={item.id}>{item.title}</li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  );
}
```

**详解**：处理"分类 → 分类下的条目"这类树状/二维数据时，外层 `map` 遍历分类，内层 `map` 遍历每个分类的条目。**每一层 map 都要有自己的 key**，且 key 只需在同一层的兄弟节点间唯一即可。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 111：斑马纹 / 高亮——用 index 决定样式</h3>

```jsx
function StripedList({ rows }) {
  return (
    <ul>
      {rows.map((row, index) => (
        <li
          key={row.id}
          style={{ background: index % 2 === 0 ? '#f5f5f5' : '#fff' }}
        >
          {row.text}
        </li>
      ))}
    </ul>
  );
}
```

**详解**：`index` 除了显示序号，也常用来做"隔行变色"（`index % 2`）或"高亮第一项/最后一项"等样式逻辑。这里注意：**index 用于计算样式没问题，但 `key` 仍然用稳定的 `row.id`**，两者用途不同，别混用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 112：列表项绑定事件并传递该项数据</h3>

```jsx
function UserList({ users, onSelect }) {
  return (
    <ul>
      {users.map(user => (
        <li key={user.id} onClick={() => onSelect(user.id)}>
          {user.name}
        </li>
      ))}
    </ul>
  );
}
```

**详解**：列表中每一项都需要知道"点击的是我"。用箭头函数 `() => onSelect(user.id)` 把当前项的 `id` 传给回调。注意要写成箭头函数包一层，而不是直接 `onClick={onSelect(user.id)}`（后者会在渲染时立即执行，而不是点击时执行）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 113：把数据转成组件数组（渲染子组件列表）</h3>

```jsx
function ProductGrid({ products }) {
  return (
    <div className="grid">
      {products.map(p => (
        <ProductCard key={p.id} product={p} />
      ))}
    </div>
  );
}

function ProductCard({ product }) {
  return (
    <div className="card">
      <h4>{product.name}</h4>
      <p>¥{product.price}</p>
    </div>
  );
}
```

**详解**：当列表每一项的结构较复杂时，应把单项抽成独立的子组件（`ProductCard`），`map` 里只负责传数据。**`key` 要写在 `map` 直接返回的那个元素上**（这里是 `<ProductCard>`），而不是子组件内部的元素。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 114：分组渲染（先用 reduce 分组，再渲染）</h3>

```jsx
function GroupedContacts({ contacts }) {
  // 按姓名首字母分组：{ A: [...], B: [...] }
  const groups = contacts.reduce((acc, c) => {
    const letter = c.name[0].toUpperCase();
    (acc[letter] = acc[letter] || []).push(c);
    return acc;
  }, {});

  return (
    <div>
      {Object.keys(groups).sort().map(letter => (
        <section key={letter}>
          <h3>{letter}</h3>
          <ul>
            {groups[letter].map(c => <li key={c.id}>{c.name}</li>)}
          </ul>
        </section>
      ))}
    </div>
  );
}
```

**详解**：这是一个进阶技巧——数据往往需要先"加工"再渲染。这里用 `reduce` 把联系人按首字母分组成对象，再用 `Object.keys()` 拿到所有分组字母、排序后遍历渲染。渲染逻辑本身仍是嵌套 `map`，关键在于**渲染前先把数据整理成合适的结构**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 115：综合实战——搜索过滤 + 排序 + 空态 + 计数</h3>

```jsx
import { useState } from 'react';

function SearchableList({ items }) {
  const [keyword, setKeyword] = useState('');
  const [asc, setAsc] = useState(true);

  // 1) 过滤：只保留包含关键字的项
  const filtered = items.filter(item =>
    item.name.toLowerCase().includes(keyword.toLowerCase())
  );

  // 2) 排序：拷贝后按名称升/降序
  const result = [...filtered].sort((a, b) =>
    asc ? a.name.localeCompare(b.name) : b.name.localeCompare(a.name)
  );

  return (
    <div>
      <input
        value={keyword}
        onChange={e => setKeyword(e.target.value)}
        placeholder="搜索..."
      />
      <button onClick={() => setAsc(a => !a)}>
        {asc ? '升序 ↑' : '降序 ↓'}
      </button>

      {/* 3) 空态处理 */}
      {result.length === 0 ? (
        <p>没有匹配的结果</p>
      ) : (
        <ul>
          {result.map(item => <li key={item.id}>{item.name}</li>)}
        </ul>
      )}

      {/* 4) 计数 */}
      <p>共 {result.length} 项</p>
    </div>
  );
}
```

**详解**：这是本章所有知识点的综合运用，也是真实项目中列表的标准形态：
1. 用 state 保存搜索关键字和排序方向；
2. `filter` 按关键字过滤（转小写做不区分大小写匹配）；
3. 拷贝后 `sort` 排序（`localeCompare` 适合中英文排序）；
4. 用三元做空态判断，非空时才 `map` 渲染；
5. 底部展示结果计数。
把"过滤 → 排序 → 判空 → 渲染"这条链路理解透，就掌握了绝大多数列表 UI 的写法。

---

## 六、表单处理

> **React 里的表单和普通 HTML 表单有什么不同？** 在 HTML 里，`<input>` 等控件自己保存并管理用户输入的值。而在 React 中，我们通常让 **state 成为"唯一数据源"**——控件显示什么由 state 决定，用户输入又通过事件更新 state。这种模式叫"**受控组件（Controlled Component）**"。
>
> **受控组件的两步闭环**：① `value={state}`（控件的值由 state 决定）；② `onChange` 中把最新输入写回 state。数据流动形成闭环，state 始终是最新、最权威的值。
>
> 本章从"最简单的受控输入框"讲到"完整的注册表单"，覆盖各类控件、多字段处理、提交校验、文件上传等，共 23 个示例。

### （A）受控组件与各类控件

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 116：什么是受控输入框</h3>

```jsx
import { useState } from 'react';

function NameInput() {
  const [name, setName] = useState('');
  return (
    <div>
      <input value={name} onChange={e => setName(e.target.value)} />
      <p>你输入了：{name}</p>
    </div>
  );
}
```

**详解**：这是受控组件的最基本形态，请务必吃透它的闭环：
1. `value={name}`：输入框显示的内容由 state `name` 决定；
2. 用户敲键盘 → 触发 `onChange` → `e.target.value` 是输入框最新的值 → `setName` 更新 state；
3. state 变了 → 组件重新渲染 → 输入框显示新的 `name`。

因为 `value` 始终绑定 state，所以 **state 是唯一数据源**。想清空输入框只需 `setName('')`，想预填只需给 state 设初始值。整章的其它控件都是这个模式的变体。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 117：受控 textarea（多行文本）</h3>

```jsx
function Comment() {
  const [text, setText] = useState('');
  return (
    <div>
      <textarea value={text} onChange={e => setText(e.target.value)} rows={4} />
      <p>字数：{text.length}</p>
    </div>
  );
}
```

**详解**：注意一个和 HTML 的区别——HTML 里 textarea 的内容写在标签之间（`<textarea>内容</textarea>`），而 **React 里统一用 `value` 属性**，和普通 input 一模一样。这样保持了一致性。绑定 state 后，可以顺便实现字数统计（`text.length`）等功能。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 118：受控 select 下拉框</h3>

```jsx
function CitySelect() {
  const [city, setCity] = useState('bj');
  return (
    <select value={city} onChange={e => setCity(e.target.value)}>
      <option value="bj">北京</option>
      <option value="sh">上海</option>
      <option value="gz">广州</option>
    </select>
  );
}
```

**详解**：又一处和 HTML 的区别——HTML 里用 `<option selected>` 来标记默认选中项，而 **React 里在 `<select>` 上用 `value` 统一控制**当前选中值。`value={city}` 等于 `'bj'` 时，北京那一项就自动选中。用户切换选项时 `onChange` 拿到所选 `option` 的 `value`。这种一致的 `value + onChange` 模式让所有控件用法统一。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 119：多选下拉框（multiple）</h3>

```jsx
function TagSelect() {
  const [tags, setTags] = useState([]);
  const handle = (e) => {
    // 从选中的 options 里收集所有 value
    const selected = Array.from(e.target.selectedOptions, o => o.value);
    setTags(selected);
  };
  return (
    <select multiple value={tags} onChange={handle}>
      <option value="react">React</option>
      <option value="vue">Vue</option>
      <option value="ng">Angular</option>
    </select>
  );
}
```

**详解**：给 `<select>` 加 `multiple` 属性可多选，此时 `value` 要绑定一个**数组**。因为可能选中多项，`onChange` 里不能只取 `e.target.value`，而要用 `e.target.selectedOptions` 拿到所有选中的 option，再用 `Array.from` 把它们的 `value` 收集成数组。多选控件的状态天然是数组。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 120：复选框（单个 checkbox）</h3>

```jsx
function Agree() {
  const [checked, setChecked] = useState(false);
  return (
    <label>
      <input
        type="checkbox"
        checked={checked}
        onChange={e => setChecked(e.target.checked)}
      />
      我已阅读并同意用户协议
    </label>
  );
}
```

**详解**：复选框有两点特殊：① 它用 **`checked`**（布尔）而不是 `value` 来表示选中状态；② `onChange` 里要读 **`e.target.checked`**（布尔）而不是 `e.target.value`。单个复选框适合"同意协议""记住我"这类开关。把 `<input>` 包在 `<label>` 里，点文字也能勾选，体验更好。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 121：一组复选框（结果存数组）</h3>

```jsx
function Hobbies() {
  const [hobbies, setHobbies] = useState([]);
  const toggle = (value) => {
    setHobbies(prev =>
      prev.includes(value)
        ? prev.filter(h => h !== value)  // 已选 → 取消
        : [...prev, value]               // 未选 → 加入
    );
  };
  const options = ['阅读', '运动', '音乐'];
  return (
    <>
      {options.map(opt => (
        <label key={opt}>
          <input
            type="checkbox"
            checked={hobbies.includes(opt)}
            onChange={() => toggle(opt)}
          />
          {opt}
        </label>
      ))}
      <p>已选：{hobbies.join('、')}</p>
    </>
  );
}
```

**详解**：多个复选框代表"可多选"，状态用**数组**保存选中的值。每个框的 `checked` 由 `hobbies.includes(该项)` 决定；点击时 `toggle`：已在数组里就 `filter` 移除，不在就 `[...prev, value]` 加入。这里用了函数式更新 `prev =>`，保证基于最新的数组操作。这是"多选组"的标准写法。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 122：单选按钮（radio）</h3>

```jsx
function Gender() {
  const [gender, setGender] = useState('male');
  return (
    <>
      <label>
        <input type="radio" name="gender" value="male"
          checked={gender === 'male'} onChange={e => setGender(e.target.value)} />
        男
      </label>
      <label>
        <input type="radio" name="gender" value="female"
          checked={gender === 'female'} onChange={e => setGender(e.target.value)} />
        女
      </label>
    </>
  );
}
```

**详解**：单选按钮组代表"多选一"，用**一个 state** 保存当前选中的值。每个 radio 的 `checked` 通过 `gender === 该项value` 判断——只有等于当前 state 的那个会选中。多个 radio 用相同的 `name` 归为一组（保证互斥）。`onChange` 里读 `e.target.value` 拿到被选中项的值。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 123：数字输入框（类型转换）</h3>

```jsx
function AgeInput() {
  const [age, setAge] = useState(0);
  return (
    <div>
      <input
        type="number"
        value={age}
        onChange={e => setAge(Number(e.target.value))} // 转成数字
      />
      <p>十年后你 {age + 10} 岁</p>
    </div>
  );
}
```

**详解**：一个大坑——**`e.target.value` 永远是字符串**，即使 input 的 `type="number"`。如果不转换直接存进 state，做数学运算时会出错（`'5' + 10` 得到 `'510'` 而不是 `15`）。所以要用 `Number(e.target.value)` 或 `parseInt` 转成数字再存。空输入会得到 `NaN`，实际项目里可能要额外处理。

### （B）多字段表单的统一处理

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 124：一个函数处理多个字段</h3>

```jsx
function Form() {
  const [form, setForm] = useState({ name: '', email: '' });
  const handle = (e) => {
    // 用计算属性名，根据 name 动态更新对应字段
    setForm({ ...form, [e.target.name]: e.target.value });
  };
  return (
    <>
      <input name="name" value={form.name} onChange={handle} />
      <input name="email" value={form.email} onChange={handle} />
    </>
  );
}
```

**详解**：表单字段多时，为每个字段写一个处理函数太啰嗦。技巧是：① 给每个 input 设 `name` 属性（对应 state 里的字段名）；② 用**计算属性名** `[e.target.name]` 动态定位要更新的字段。这样一个 `handle` 函数搞定所有字段。`{ ...form, [e.target.name]: e.target.value }` 的意思是"复制整个 form，只更新 name 所指的那个字段"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 125：用对象统一管理表单状态</h3>

```jsx
function ProfileForm() {
  const [form, setForm] = useState({ name: '', age: '', bio: '' });
  const update = (field) => (e) =>
    setForm(prev => ({ ...prev, [field]: e.target.value }));
  return (
    <>
      <input value={form.name} onChange={update('name')} />
      <input value={form.age} onChange={update('age')} />
      <textarea value={form.bio} onChange={update('bio')} />
    </>
  );
}
```

**详解**：这里用了"柯里化"技巧——`update('name')` 返回一个专门更新 `name` 字段的处理函数。相比示例 124 依赖 `name` 属性，这种写法更灵活、更明确，且不需要给控件设 `name`。用函数式更新 `prev =>` 确保基于最新状态。当字段较多、关系紧密时，把它们放进一个对象统一管理会比一堆独立 `useState` 更清爽。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 126：混合类型字段的统一处理（用 type 判断）</h3>

```jsx
function MixedForm() {
  const [form, setForm] = useState({ username: '', subscribe: false });
  const handle = (e) => {
    const { name, type, value, checked } = e.target;
    // checkbox 取 checked，其它取 value
    setForm(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
  };
  return (
    <>
      <input name="username" value={form.username} onChange={handle} />
      <label>
        <input name="subscribe" type="checkbox" checked={form.subscribe} onChange={handle} />
        订阅邮件
      </label>
    </>
  );
}
```

**详解**：一个通用处理函数如果要同时管文本框和复选框，就得区分类型：checkbox 要取 `e.target.checked`，其它取 `e.target.value`。通过判断 `type === 'checkbox'` 决定取哪个。这是编写"一个 handle 管所有控件"的通用型表单处理函数的关键，很多表单库内部也是这么做的。

### （C）表单提交与校验

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 127：表单提交（onSubmit + preventDefault）</h3>

```jsx
function LoginForm() {
  const [user, setUser] = useState('');
  const submit = (e) => {
    e.preventDefault(); // 关键：阻止表单默认提交导致页面刷新
    alert('提交：' + user);
  };
  return (
    <form onSubmit={submit}>
      <input value={user} onChange={e => setUser(e.target.value)} />
      <button type="submit">登录</button>
    </form>
  );
}
```

**详解**：处理提交要把 `onSubmit` 绑在 `<form>` 上（而不是按钮的 `onClick`），这样点击 `type="submit"` 按钮**或**在输入框按回车都能触发。**必须调用 `e.preventDefault()`**——否则浏览器会执行默认的表单提交行为（刷新/跳转页面），破坏单页应用。阻止默认后，就用 JS 自行处理提交逻辑（如发请求）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 128：提交时收集并校验数据</h3>

```jsx
function SignupForm() {
  const [form, setForm] = useState({ name: '', password: '' });
  const submit = (e) => {
    e.preventDefault();
    if (!form.name.trim()) { alert('请输入用户名'); return; }
    if (form.password.length < 6) { alert('密码至少 6 位'); return; }
    console.log('校验通过，提交：', form);
  };
  const update = (f) => (e) => setForm(p => ({ ...p, [f]: e.target.value }));
  return (
    <form onSubmit={submit}>
      <input value={form.name} onChange={update('name')} placeholder="用户名" />
      <input type="password" value={form.password} onChange={update('password')} />
      <button type="submit">注册</button>
    </form>
  );
}
```

**详解**：因为整个表单数据都在 state 里，提交时可直接读取 `form` 进行校验。这里用"卫语句"逐条检查：不通过就提示并 `return`（中断提交），全部通过才执行真正的提交。这是"提交时校验"的基本套路——简单直接，适合校验规则不复杂的表单。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 129：实时校验（边输入边提示）</h3>

```jsx
function EmailForm() {
  const [email, setEmail] = useState('');
  const isValid = /^\S+@\S+\.\S+$/.test(email);
  return (
    <div>
      <input value={email} onChange={e => setEmail(e.target.value)} placeholder="邮箱" />
      {email && !isValid && <p style={{ color: 'red' }}>邮箱格式不正确</p>}
    </div>
  );
}
```

**详解**：实时校验是在**输入过程中**就给出反馈。因为 state 每次输入都会更新、组件重渲染，所以可以在渲染时直接根据当前值计算校验结果（`isValid`），再条件渲染错误提示。这里用 `email && !isValid` 保证"用户还没输入时不报错，输入了但格式错才提示"。这种"从 state 推导出校验状态"的思路很 React。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 130：把错误信息存进 state 显示</h3>

```jsx
function Form() {
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const handleChange = (e) => {
    const value = e.target.value;
    setName(value);
    setError(value.trim() ? '' : '用户名不能为空'); // 同步更新错误信息
  };
  return (
    <div>
      <input value={name} onChange={handleChange} />
      {error && <p style={{ color: 'red' }}>{error}</p>}
    </div>
  );
}
```

**详解**：当校验逻辑较复杂、或需要保存"某字段的错误消息"时，用一个专门的 state（如 `error`）来存错误文本。输入时更新值的同时更新错误信息，界面根据 `error` 是否有内容来显示提示。多字段时，`error` 常设计成一个对象 `{ name: '...', email: '...' }`，为每个字段单独存错误。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 131：根据校验结果禁用提交按钮</h3>

```jsx
function Form() {
  const [form, setForm] = useState({ name: '', agree: false });
  const canSubmit = form.name.trim() !== '' && form.agree;
  const update = (f, key = 'value') => (e) =>
    setForm(p => ({ ...p, [f]: e.target[key] }));
  return (
    <form>
      <input value={form.name} onChange={update('name')} placeholder="用户名" />
      <label>
        <input type="checkbox" checked={form.agree} onChange={update('agree', 'checked')} />
        同意协议
      </label>
      <button type="submit" disabled={!canSubmit}>提交</button>
    </form>
  );
}
```

**详解**：更友好的做法是在表单不满足条件时**禁用提交按钮**（`disabled={!canSubmit}`），从源头防止无效提交。`canSubmit` 是从 state 推导出来的布尔值——用户名非空且勾选了协议才允许提交。因为 state 一变就重新计算，按钮的可用状态会实时随表单变化。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 132：提交中状态（防重复提交）</h3>

```jsx
function AsyncForm() {
  const [value, setValue] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const submit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await fakeApi(value);       // 模拟发请求
      alert('提交成功');
    } finally {
      setSubmitting(false);       // 无论成功失败都恢复
    }
  };
  return (
    <form onSubmit={submit}>
      <input value={value} onChange={e => setValue(e.target.value)} disabled={submitting} />
      <button type="submit" disabled={submitting}>
        {submitting ? '提交中...' : '提交'}
      </button>
    </form>
  );
}
function fakeApi(v) { return new Promise(r => setTimeout(r, 1000)); }
```

**详解**：提交涉及异步请求时，要用一个 `submitting` 状态标记"正在提交"。提交期间禁用按钮和输入框、按钮文字改成"提交中..."，防止用户重复点击造成多次请求。用 `try/finally` 保证请求无论成功或失败，最后都把 `submitting` 复位。这是异步表单的标准处理。

### （D）非受控组件与文件上传

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 133：非受控组件（ref + defaultValue）</h3>

```jsx
import { useRef } from 'react';

function UncontrolledForm() {
  const inputRef = useRef(null);
  const submit = (e) => {
    e.preventDefault();
    alert('值是：' + inputRef.current.value); // 提交时才读取
  };
  return (
    <form onSubmit={submit}>
      <input ref={inputRef} defaultValue="初始值" />
      <button type="submit">读取</button>
    </form>
  );
}
```

**详解**：与受控组件相对的是"非受控组件"——不用 state 绑定 `value`，而是让 DOM 自己保管输入值，需要时用 `ref` 读取。注意用 **`defaultValue`**（而不是 `value`）设置初始值，否则值会被锁死无法输入。非受控组件代码更少，但你无法在输入过程中实时拿到值、做校验或联动。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 134：受控 vs 非受控如何选择</h3>

```jsx
// 受控：value 绑 state，实时可控
<input value={name} onChange={e => setName(e.target.value)} />

// 非受控：defaultValue + ref，提交时读取
<input ref={ref} defaultValue="" />
```

**详解**：如何选择：
- **用受控组件（推荐，默认选它）**：需要实时校验、根据输入联动其它 UI、动态启用/禁用按钮、格式化输入等——凡是"输入过程中要对值做点什么"的场景。React 生态绝大多数表单都用受控。
- **用非受控组件**：只在提交时读一次值、表单极其简单、或对接文件上传（file 输入本身就是非受控，见下例）、集成非 React 的第三方库时。

一句话：**默认用受控，特殊情况才用非受控**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 135：文件上传（单文件）</h3>

```jsx
function FileUpload() {
  const [fileName, setFileName] = useState('');
  const onChange = (e) => {
    const file = e.target.files[0]; // 取第一个文件
    if (file) {
      setFileName(file.name);
      console.log('大小：', file.size, '类型：', file.type);
    }
  };
  return (
    <div>
      <input type="file" onChange={onChange} />
      {fileName && <p>已选择：{fileName}</p>}
    </div>
  );
}
```

**详解**：`<input type="file">` 是特殊的——出于安全原因它**只能是非受控的**（你不能用 `value` 设置用户要上传哪个文件）。通过 `e.target.files` 拿到用户选择的文件列表（一个类数组的 `FileList`），`files[0]` 是第一个文件。文件对象有 `name`、`size`、`type` 等属性。真正上传时通常把 file 放进 `FormData` 发给后端。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 136：多文件上传与图片预览</h3>

```jsx
function MultiUpload() {
  const [previews, setPreviews] = useState([]);
  const onChange = (e) => {
    const files = Array.from(e.target.files);          // FileList → 数组
    const urls = files.map(f => URL.createObjectURL(f)); // 生成本地预览地址
    setPreviews(urls);
  };
  return (
    <div>
      <input type="file" accept="image/*" multiple onChange={onChange} />
      <div>{previews.map((url, i) => <img key={i} src={url} width={80} alt={`预览 ${i + 1}`} />)}</div>
    </div>
  );
}
```

**详解**：加 `multiple` 允许选多个文件，`accept="image/*"` 限制只能选图片。`e.target.files` 是类数组，用 `Array.from` 转成真数组才能 `map`。`URL.createObjectURL(file)` 能为本地文件生成一个临时 URL，用于在上传前预览图片。（严谨起见，组件卸载时应调用 `URL.revokeObjectURL` 释放这些临时地址。）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 137：重置表单</h3>

```jsx
function Form() {
  const initial = { name: '', email: '' };
  const [form, setForm] = useState(initial);
  const update = (f) => (e) => setForm(p => ({ ...p, [f]: e.target.value }));
  return (
    <form>
      <input value={form.name} onChange={update('name')} />
      <input value={form.email} onChange={update('email')} />
      <button type="button" onClick={() => setForm(initial)}>重置</button>
    </form>
  );
}
```

**详解**：受控表单重置非常简单——把 state 设回初始值即可（`setForm(initial)`），所有绑定该 state 的控件会自动清空。把初始值抽成一个 `initial` 常量便于复用。注意重置按钮要设 `type="button"`，否则在 `<form>` 里它默认是 `type="submit"`，点击会触发提交。

### （E）综合实战

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 138：综合实战——完整注册表单</h3>

```jsx
import { useState } from 'react';

function RegisterForm() {
  const [form, setForm] = useState({ name: '', email: '', password: '', agree: false });
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  // 统一处理各类控件
  const handle = (e) => {
    const { name, type, value, checked } = e.target;
    setForm(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
  };

  // 校验，返回错误对象
  const validate = () => {
    const errs = {};
    if (!form.name.trim()) errs.name = '请输入用户名';
    if (!/^\S+@\S+\.\S+$/.test(form.email)) errs.email = '邮箱格式不正确';
    if (form.password.length < 6) errs.password = '密码至少 6 位';
    if (!form.agree) errs.agree = '请勾选同意协议';
    return errs;
  };

  const submit = async (e) => {
    e.preventDefault();
    const errs = validate();
    setErrors(errs);
    if (Object.keys(errs).length > 0) return; // 有错误则中断
    setSubmitting(true);
    try {
      await fakeApi(form);
      alert('注册成功！');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={submit}>
      <div>
        <input name="name" value={form.name} onChange={handle} placeholder="用户名" />
        {errors.name && <span style={{ color: 'red' }}>{errors.name}</span>}
      </div>
      <div>
        <input name="email" value={form.email} onChange={handle} placeholder="邮箱" />
        {errors.email && <span style={{ color: 'red' }}>{errors.email}</span>}
      </div>
      <div>
        <input name="password" type="password" value={form.password} onChange={handle} placeholder="密码" />
        {errors.password && <span style={{ color: 'red' }}>{errors.password}</span>}
      </div>
      <div>
        <label>
          <input name="agree" type="checkbox" checked={form.agree} onChange={handle} />
          我已阅读并同意用户协议
        </label>
        {errors.agree && <span style={{ color: 'red' }}>{errors.agree}</span>}
      </div>
      <button type="submit" disabled={submitting}>
        {submitting ? '注册中...' : '注册'}
      </button>
    </form>
  );
}
function fakeApi(data) { return new Promise(r => setTimeout(r, 1000)); }
```

**详解**：这个注册表单综合了本章几乎所有知识点：
1. **对象 state 管理多字段** + **一个 `handle` 处理所有控件**（含 checkbox 的 type 判断，示例 126）；
2. **集中校验函数** `validate` 返回错误对象，把每个字段的错误存进 `errors` state（示例 130）；
3. **提交流程**：`preventDefault` → 校验 → 有错中断并显示、无错继续（示例 128）；
4. **异步提交状态** `submitting` 防重复提交（示例 132）；
5. **字段级错误提示**：每个字段下方按 `errors.字段` 条件渲染红色错误文案。

这套结构就是真实项目中手写表单的通用骨架。规则更复杂时，可考虑用 React Hook Form、Formik 等成熟表单库，它们把这些模式封装得更简洁。

---

## 七、核心 Hooks

> Hooks 是 React 16.8 引入、在函数组件里"钩入" React 能力（状态、生命周期等）的函数。本章按 `useEffect → useRef → useContext → useReducer → useMemo/useCallback → useLayoutEffect/useImperativeHandle → 自定义 Hook` 的顺序，由浅入深讲 30 个示例。
>
> **两条铁律先记住**：① Hooks 只能在组件函数或自定义 Hook 的**顶层**调用，不能写在 `if`、循环、嵌套函数里；② Hook 名必须以 `use` 开头。这保证 React 每次渲染都能按相同顺序识别每个 Hook。

### （A）useEffect —— 处理副作用

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 139：useEffect 最简单的样子（每次渲染后执行）</h3>

```jsx
import { useState, useEffect } from 'react';

function Title() {
  const [count, setCount] = useState(0);
  useEffect(() => {
    document.title = `点击了 ${count} 次`; // 修改标题属于"副作用"
  });
  return <button onClick={() => setCount(count + 1)}>{count}</button>;
}
```

**详解**：所谓"副作用"是指渲染之外、会影响外部世界的操作，比如改文档标题、发网络请求、操作 DOM、设置定时器等。`useEffect(fn)` 会在**每次渲染完成后**执行 `fn`。这里没写第二个参数，所以每次 `count` 变化导致重渲染后，标题都会更新。副作用不能直接写在组件函数体里（那样会在渲染过程中执行，可能引发问题），必须放进 `useEffect`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 140：空依赖数组（只在挂载时执行一次）</h3>

```jsx
function OnMount() {
  useEffect(() => {
    console.log('组件挂载了，这句只打印一次');
  }, []); // 第二个参数是空数组
  return <p>Hello</p>;
}
```

**详解**：`useEffect` 的第二个参数叫"依赖数组"。传空数组 `[]` 表示"没有任何依赖"，于是这个 effect 只在组件**首次挂载后**执行一次，后续重渲染都不再执行。适合做只需一次的初始化，比如获取初始数据、注册全局监听。（注意：开发环境的 `StrictMode` 下会故意执行两次以帮你发现问题，生产环境只执行一次。）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 141：指定依赖（依赖变化时才执行）</h3>

```jsx
function Watcher({ userId }) {
  useEffect(() => {
    console.log('userId 变成了：', userId);
  }, [userId]); // 只有 userId 变化时才重新执行
  return <p>当前用户：{userId}</p>;
}
```

**详解**：依赖数组里列出的值，只要**任意一个**在两次渲染之间发生变化，effect 就会重新执行。这里 `userId` 不变时，即使组件因别的原因重渲染，effect 也不会跑。React 用 `Object.is` 逐个比较依赖项，所以依赖应放"原始值或稳定引用"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 142：依赖数组的三种形态对比（重点总结）</h3>

```jsx
useEffect(() => { /* ... */ });          // ① 不传：每次渲染后都执行
useEffect(() => { /* ... */ }, []);      // ② 空数组：只在挂载后执行一次
useEffect(() => { /* ... */ }, [a, b]);  // ③ 有依赖：a 或 b 变化时执行
```

**详解**：这是理解 `useEffect` 的关键。记住这张对照表：
- **不传第二个参数** → 每次渲染后都执行（很少用，通常是没想清楚）；
- **`[]`** → 仅挂载时执行一次，卸载时执行清理；
- **`[a, b]`** → 挂载时执行，之后每当 `a` 或 `b` 变化时再执行。

选哪种，取决于你的副作用"依赖了哪些数据"。原则是：**effect 内部用到的每一个组件内变量（props、state、函数），都应出现在依赖数组里**（见示例 148）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 143：清理函数（以定时器为例）</h3>

```jsx
function Timer() {
  const [sec, setSec] = useState(0);
  useEffect(() => {
    const id = setInterval(() => setSec(s => s + 1), 1000);
    return () => clearInterval(id); // 返回的函数就是"清理函数"
  }, []);
  return <p>已运行 {sec} 秒</p>;
}
```

**详解**：`useEffect` 的回调可以 `return` 一个"清理函数"。React 会在**组件卸载时**、以及**下一次执行该 effect 之前**调用它。定时器、订阅这类会"持续占用资源"的副作用，必须在清理函数里释放（这里 `clearInterval`），否则组件卸载后定时器还在跑，会造成内存泄漏和报错。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 144：清理事件监听</h3>

```jsx
function WindowSize() {
  const [width, setWidth] = useState(window.innerWidth);
  useEffect(() => {
    const onResize = () => setWidth(window.innerWidth);
    window.addEventListener('resize', onResize);
    return () => window.removeEventListener('resize', onResize); // 成对出现
  }, []);
  return <p>窗口宽度：{width}px</p>;
}
```

**详解**：给 `window`、`document` 等外部对象添加的监听器，React 不会自动帮你移除。规则很简单——**`addEventListener` 和 `removeEventListener` 必须成对出现**，后者放在清理函数里，且传入的必须是同一个函数引用（所以这里把 `onResize` 提取成具名函数）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 145：在 useEffect 中请求数据（基础版）</h3>

```jsx
function UserProfile({ id }) {
  const [user, setUser] = useState(null);
  useEffect(() => {
    fetch(`/api/users/${id}`)
      .then(r => r.json())
      .then(data => setUser(data));
  }, [id]); // id 变化就重新请求
  return user ? <p>{user.name}</p> : <p>加载中...</p>;
}
```

**详解**：数据请求是最常见的副作用。把 `fetch` 放进 `useEffect`，并把请求依赖的 `id` 放进依赖数组，这样每当 `id` 变化就会自动重新请求。渲染时先展示"加载中"，数据回来后 `setUser` 触发重渲染显示内容。但这个版本有个隐患——见下一个示例。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 146：请求数据的竞态问题与 ignore 标志</h3>

```jsx
function UserProfile({ id }) {
  const [user, setUser] = useState(null);
  useEffect(() => {
    let ignore = false; // 标记本次 effect 是否已"作废"
    fetch(`/api/users/${id}`)
      .then(r => r.json())
      .then(data => {
        if (!ignore) setUser(data); // 只有没作废时才更新
      });
    return () => { ignore = true; }; // 清理时把上一次请求标记为作废
  }, [id]);
  return user ? <p>{user.name}</p> : <p>加载中...</p>;
}
```

**详解**：如果 `id` 快速变化（比如从 1 切到 2），会发起两个请求。但网络返回顺序不保证——万一"id=1"的响应比"id=2"晚到，就会用旧数据覆盖新数据，这叫"竞态条件"。解决办法：在清理函数里把上一次 effect 标记为 `ignore = true`，其响应回来后就不再 `setUser`。这是 React 官方推荐的处理请求竞态的标准模式。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 147：闭包陷阱——读到"过期"的 state</h3>

```jsx
function Counter() {
  const [count, setCount] = useState(0);
  useEffect(() => {
    const id = setInterval(() => {
      // ❌ 若写 setCount(count + 1)，这里的 count 永远是 0（闭包捕获了初始值）
      setCount(c => c + 1); // ✅ 用函数式更新，拿到的是最新值
    }, 1000);
    return () => clearInterval(id);
  }, []); // 空依赖，effect 只建立一次
  return <p>{count}</p>;
}
```

**详解**：这是 Hooks 里最经典的坑。因为依赖是 `[]`，effect 只在挂载时运行一次，此时 `count` 的值 `0` 被闭包"永久捕获"。定时器回调里若直接用 `count`，永远是 `0`，导致计数卡在 1。**解决方案**：用函数式更新 `setCount(c => c + 1)`，`c` 是 React 传入的最新值，与闭包无关。这样既避免了陷阱，又不必把 `count` 加进依赖数组导致定时器反复重建。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 148：不要漏写依赖（并理解为什么）</h3>

```jsx
function Search({ query, onResult }) {
  useEffect(() => {
    fetchData(query).then(onResult);
    // 依赖数组应包含 effect 内用到的所有外部变量
  }, [query, onResult]);
  return null;
}
```

**详解**：ESLint 的 `react-hooks/exhaustive-deps` 规则会提醒你补全依赖。漏写依赖的后果是：effect 内部读到的是某次渲染时"冻结"的旧值，行为难以预测。原则是**诚实地列出 effect 用到的每一个组件内变量**。如果某个依赖变化太频繁导致 effect 反复执行，正确做法不是删依赖，而是用 `useCallback`/`useMemo` 稳定它，或用函数式更新绕开（如示例 147）。

### （B）useRef —— 引用 DOM 与保存可变值

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 149：useRef 引用 DOM 元素</h3>

```jsx
import { useRef, useEffect } from 'react';

function AutoFocus() {
  const inputRef = useRef(null);
  useEffect(() => {
    inputRef.current.focus(); // 挂载后自动聚焦
  }, []);
  return <input ref={inputRef} placeholder="自动聚焦" />;
}
```

**详解**：`useRef(null)` 返回一个 `{ current: null }` 的对象。把它通过 `ref={inputRef}` 挂到 JSX 元素上后，React 会在渲染后把真实 DOM 节点放进 `inputRef.current`。于是你能命令式地操作 DOM（聚焦、测量、滚动、播放视频等）。注意要在 `useEffect` 里访问 `.current`，因为渲染阶段 DOM 还没就绪。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 150：useRef 保存可变值（修改它不会触发渲染）</h3>

```jsx
function Stopwatch() {
  const timerId = useRef(null);
  const start = () => {
    timerId.current = setInterval(() => console.log('tick'), 1000);
  };
  const stop = () => clearInterval(timerId.current);
  return (
    <>
      <button onClick={start}>开始</button>
      <button onClick={stop}>停止</button>
    </>
  );
}
```

**详解**：`useRef` 的第二个用途是"在多次渲染之间存放一个可变值"。和 `state` 不同，**修改 `ref.current` 不会触发重新渲染**。这里用它保存定时器 id，因为这个 id 只是内部记录、不需要显示到界面上。凡是"需要跨渲染记住、但改变时不需要更新 UI"的值，都适合用 ref。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 151：useRef vs useState 的区别（对照理解）</h3>

```jsx
function Demo() {
  const [stateVal, setStateVal] = useState(0);
  const refVal = useRef(0);
  return (
    <div>
      <p>state: {stateVal}，ref: {refVal.current}</p>
      <button onClick={() => setStateVal(stateVal + 1)}>改 state（会刷新）</button>
      <button onClick={() => { refVal.current++; }}>改 ref（界面不动）</button>
    </div>
  );
}
```

**详解**：点"改 ref"按钮，`refVal.current` 确实变了，但界面上的数字不会更新，因为改 ref 不触发渲染——只有下次因别的原因重渲染时，才会看到新值。对照记忆：
- **需要显示在界面、变化要驱动重渲染** → 用 `useState`；
- **只是内部记录、变化不该刷新界面**（DOM 引用、定时器 id、上一次的值等） → 用 `useRef`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 152：useRef 保存上一次的值（自定义 usePrevious）</h3>

```jsx
function usePrevious(value) {
  const ref = useRef();
  useEffect(() => {
    ref.current = value; // 渲染后才更新，所以读到的是"上一次"
  });
  return ref.current;
}

function PriceDisplay({ price }) {
  const prevPrice = usePrevious(price);
  return <p>现价 {price}，上次 {prevPrice ?? '—'}</p>;
}
```

**详解**：这是 ref"跨渲染记忆"能力的经典应用。关键在时序：渲染时先 `return ref.current`（还是旧值），渲染完成后 `useEffect` 才把它更新为当前值。所以每次渲染读到的都是"上一次的值"。这个 `usePrevious` 常用于对比前后变化、触发动画等。

### （C）useContext —— 跨层级共享数据

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 153：useContext 基础用法</h3>

```jsx
import { createContext, useContext } from 'react';

const ThemeContext = createContext('light'); // 参数是默认值

function ThemedButton() {
  const theme = useContext(ThemeContext); // 读取最近的 Provider 的值
  return <button className={theme}>当前主题：{theme}</button>;
}

function App() {
  return (
    <ThemeContext.Provider value="dark">
      <ThemedButton />
    </ThemeContext.Provider>
  );
}
```

**详解**：Context 用于"跨越多层组件共享数据"。三步走：① `createContext(默认值)` 创建；② 用 `<Context.Provider value={...}>` 在上层提供数据；③ 子孙组件用 `useContext(Context)` 直接读取，无论隔了多少层。`useContext` 拿到的是"组件树中最近的那个 Provider"的 `value`；若上方没有 Provider，则用创建时的默认值。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 154：useContext 解决"逐层传递 props"（prop drilling）</h3>

```jsx
const UserContext = createContext(null);

function App() {
  const user = { name: '张三', role: 'admin' };
  return (
    <UserContext.Provider value={user}>
      <Layout />
    </UserContext.Provider>
  );
}

function Layout()  { return <Header />; }              // 不需要接收/转发 user
function Header()  { return <UserBadge />; }           // 不需要接收/转发 user
function UserBadge() {
  const user = useContext(UserContext);                // 深层组件直接取用
  return <span>{user.name}（{user.role}）</span>;
}
```

**详解**：如果不用 Context，`user` 就得从 `App` 一层层通过 props 传到 `UserBadge`，中间的 `Layout`、`Header` 明明用不到却要负责"中转"，这就是"prop drilling"。Context 让深层组件直接取数据，中间层彻底解耦。适合放主题、当前用户、语言、全局配置等"很多地方都要用"的数据。

### （D）useReducer —— 管理复杂状态

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 155：useReducer 计数器（入门）</h3>

```jsx
import { useReducer } from 'react';

function reducer(state, action) {
  switch (action.type) {
    case 'inc': return { count: state.count + 1 };
    case 'dec': return { count: state.count - 1 };
    default: return state;
  }
}

function Counter() {
  const [state, dispatch] = useReducer(reducer, { count: 0 });
  return (
    <>
      <button onClick={() => dispatch({ type: 'dec' })}>-</button>
      <span>{state.count}</span>
      <button onClick={() => dispatch({ type: 'inc' })}>+</button>
    </>
  );
}
```

**详解**：`useReducer` 是 `useState` 的"进阶版"，思路来自 Redux。它接收一个 `reducer(旧状态, action) => 新状态` 函数和初始状态，返回当前 `state` 和一个 `dispatch` 函数。你不再直接改状态，而是 `dispatch({ type: '动作' })` 派发一个动作，由 reducer 集中决定状态怎么变。好处是把"状态更新逻辑"从组件里抽出来，集中、可预测、易测试。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 156：useReducer 管理复杂表单</h3>

```jsx
function formReducer(state, action) {
  switch (action.type) {
    case 'change': return { ...state, [action.field]: action.value };
    case 'reset':  return { name: '', email: '' };
    default:       return state;
  }
}

function Form() {
  const [form, dispatch] = useReducer(formReducer, { name: '', email: '' });
  const onChange = e =>
    dispatch({ type: 'change', field: e.target.name, value: e.target.value });
  return (
    <form>
      <input name="name" value={form.name} onChange={onChange} />
      <input name="email" value={form.email} onChange={onChange} />
      <button type="button" onClick={() => dispatch({ type: 'reset' })}>重置</button>
    </form>
  );
}
```

**详解**：当一个状态是"包含多个字段的对象"、更新逻辑又有多种（修改某字段、重置、批量校验等）时，用 `useReducer` 比多个 `useState` 更清爽。所有更新集中在 reducer 里，用 `action.type` 区分操作。注意 reducer 里始终返回**新对象**（`{ ...state, ... }`），不要直接改旧 state。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 157：useReducer 管理列表</h3>

```jsx
function todoReducer(todos, action) {
  switch (action.type) {
    case 'add':    return [...todos, { id: Date.now(), text: action.text, done: false }];
    case 'toggle': return todos.map(t => t.id === action.id ? { ...t, done: !t.done } : t);
    case 'remove': return todos.filter(t => t.id !== action.id);
    default:       return todos;
  }
}

function Todos() {
  const [todos, dispatch] = useReducer(todoReducer, []);
  return (
    <>
      <button onClick={() => dispatch({ type: 'add', text: '新任务' })}>添加</button>
      <ul>
        {todos.map(t => (
          <li key={t.id}>
            <span onClick={() => dispatch({ type: 'toggle', id: t.id })}
              style={{ textDecoration: t.done ? 'line-through' : 'none' }}>{t.text}</span>
            <button onClick={() => dispatch({ type: 'remove', id: t.id })}>删</button>
          </li>
        ))}
      </ul>
    </>
  );
}
```

**详解**：列表的增、删、改往往逻辑集中，非常适合 `useReducer`。每种操作对应一个 `case`，都返回新数组（`[...]`/`map`/`filter`，绝不原地修改）。组件里只管 `dispatch` 语义化的动作，读起来像在"描述发生了什么"，而不是"怎么改数据"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 158：useReducer vs useState 如何选择</h3>

```jsx
// 简单、独立的状态 → useState
const [open, setOpen] = useState(false);

// 多字段、多种更新方式、下一个状态依赖上一个 → useReducer
const [state, dispatch] = useReducer(reducer, initialState);
```

**详解**：两者能力等价，选择看复杂度：
- **用 `useState`**：状态简单（布尔、数字、字符串）、更新逻辑就一两处；
- **用 `useReducer`**：状态是复杂对象/数组、有多种更新动作、更新逻辑分散在多个事件里、或希望更新逻辑可单独测试。
经验法则：当你发现多个 `setXxx` 总是一起出现、或更新逻辑越来越绕时，就该考虑换成 `useReducer`。

### （E）useMemo / useCallback —— 缓存以优化性能

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 159：useMemo 缓存昂贵的计算结果</h3>

```jsx
import { useMemo } from 'react';

function ExpensiveList({ items, keyword }) {
  const filtered = useMemo(() => {
    console.log('执行了过滤计算'); // 依赖不变时不会打印
    return items.filter(i => i.includes(keyword));
  }, [items, keyword]);
  return <ul>{filtered.map((i, k) => <li key={k}>{i}</li>)}</ul>;
}
```

**详解**：`useMemo(fn, deps)` 会"记住" `fn` 的返回值，只有依赖 `deps` 变化时才重新计算，否则复用上次结果。它用于避免"每次渲染都重复做昂贵计算"（如大数组过滤/排序、复杂派生数据）。这里只要 `items` 和 `keyword` 不变，即使组件因别的 state 重渲染，过滤也不会重跑。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 160：useMemo 稳定对象引用（配合 React.memo）</h3>

```jsx
function Parent({ userId }) {
  const [count, setCount] = useState(0);
  // 不用 useMemo 的话，每次渲染 config 都是新对象，Child 会白白重渲染
  const config = useMemo(() => ({ userId, theme: 'dark' }), [userId]);
  return (
    <>
      <button onClick={() => setCount(c => c + 1)}>{count}</button>
      <Child config={config} />
    </>
  );
}

const Child = React.memo(({ config }) => {
  console.log('Child 渲染');
  return <p>{config.userId}</p>;
});
```

**详解**：`React.memo` 通过"浅比较 props"来跳过重渲染。但对象/数组/函数每次渲染都是新引用，浅比较必然判定"变了"，`memo` 就失效了。用 `useMemo` 把对象缓存起来，只要 `userId` 不变，`config` 就是同一个引用，`Child` 才能真正被 `memo` 跳过。点计数按钮时 Child 不再重渲染。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 161：useCallback 缓存函数</h3>

```jsx
import { useCallback } from 'react';

function Parent() {
  const [count, setCount] = useState(0);
  const handleClick = useCallback(() => {
    console.log('点击子按钮');
  }, []); // 依赖为空 → 函数引用永远不变
  return (
    <>
      <button onClick={() => setCount(count + 1)}>父：{count}</button>
      <Child onClick={handleClick} />
    </>
  );
}

const Child = React.memo(({ onClick }) => {
  console.log('Child 渲染');
  return <button onClick={onClick}>子按钮</button>;
});
```

**详解**：`useCallback(fn, deps)` 相当于 `useMemo(() => fn, deps)`，专门用来缓存"函数"。道理同示例 160：函数每次渲染都是新引用，会让接收它的 `memo` 子组件失效。用 `useCallback` 固定函数引用后，父组件计数变化不再连累 `Child` 重渲染。依赖数组里要放函数内部用到的会变化的变量。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 162：不要滥用 useMemo / useCallback</h3>

```jsx
// ❌ 没必要：加法本身极快，缓存的开销比计算还大
const sum = useMemo(() => a + b, [a, b]);

// ❌ 没必要：这个函数没传给 memo 子组件，也没进依赖数组
const onClick = useCallback(() => setOpen(true), []);

// ✅ 直接写就好
const sum2 = a + b;
const onClick2 = () => setOpen(true);
```

**详解**：`useMemo`/`useCallback` 本身也有成本（要保存值、比较依赖），并非"加了就更快"。**只在真正需要时使用**：① 计算确实昂贵；② 值/函数要作为 props 传给 `React.memo` 优化过的子组件；③ 值/函数被用作其他 Hook 的依赖项。除此之外，直接写普通变量和函数更简单、可读性更好。过早优化只会增加复杂度。

### （F）useLayoutEffect / useImperativeHandle

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 163：useLayoutEffect 同步测量避免闪烁</h3>

```jsx
import { useLayoutEffect, useRef, useState } from 'react';

function Tooltip() {
  const ref = useRef(null);
  const [height, setHeight] = useState(0);
  useLayoutEffect(() => {
    // 在浏览器"绘制到屏幕之前"同步测量，用户不会看到跳动
    setHeight(ref.current.getBoundingClientRect().height);
  }, []);
  return <div ref={ref}>我的高度是 {height}px</div>;
}
```

**详解**：`useLayoutEffect` 的用法和 `useEffect` 一样，但执行时机不同：它在 DOM 更新后、**浏览器绘制前同步执行**。所以适合"读取布局并立即同步修改 DOM"的场景（测量尺寸、调整滚动位置、定位弹层），能避免用户看到中间的闪烁。代价是它会阻塞绘制，用多了影响性能。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 164：useLayoutEffect 与 useEffect 的区别</h3>

```jsx
useEffect(() => { /* 绘制后异步执行，不阻塞渲染，99% 情况用它 */ });
useLayoutEffect(() => { /* 绘制前同步执行，会阻塞渲染，仅测量/定位时用 */ });
```

**详解**：一句话记忆——**默认永远用 `useEffect`**。它在浏览器绘制后异步执行，不会拖慢首屏。只有当你遇到"用了 useEffect 会出现明显闪烁/跳动"（因为你需要在绘制前读布局并改 DOM）时，才换成 `useLayoutEffect`。两者 API 完全一样，区别只在执行时机与是否阻塞绘制。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 165：useImperativeHandle 向父组件暴露方法</h3>

```jsx
import { forwardRef, useImperativeHandle, useRef } from 'react';

const FancyInput = forwardRef((props, ref) => {
  const inputRef = useRef();
  useImperativeHandle(ref, () => ({
    focus: () => inputRef.current.focus(),
    clear: () => { inputRef.current.value = ''; },
  }));
  return <input ref={inputRef} />;
});

function App() {
  const fancyRef = useRef();
  return (
    <>
      <FancyInput ref={fancyRef} />
      <button onClick={() => fancyRef.current.focus()}>聚焦</button>
      <button onClick={() => fancyRef.current.clear()}>清空</button>
    </>
  );
}
```

**详解**：默认情况下父组件拿到子组件的 `ref` 会指向其内部 DOM。有时你想只暴露"几个特定方法"（而非整个 DOM），就用 `useImperativeHandle(ref, () => ({ ...方法 }))` 自定义暴露的内容，配合 `forwardRef` 转发 ref。这样父组件只能调用 `focus`、`clear`，封装更干净。注意：这属于"命令式"用法，应作为补充手段，优先还是用 props/state 声明式地控制子组件。

### （G）自定义 Hook —— 复用逻辑

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 166：自定义 Hook：useToggle</h3>

```jsx
import { useState, useCallback } from 'react';

function useToggle(initial = false) {
  const [on, setOn] = useState(initial);
  const toggle = useCallback(() => setOn(o => !o), []);
  return [on, toggle];
}

function Switch() {
  const [on, toggle] = useToggle();
  return <button onClick={toggle}>{on ? '开' : '关'}</button>;
}
```

**详解**：自定义 Hook 就是"名字以 use 开头、内部调用了其他 Hook 的普通函数"。它的价值在于**复用有状态的逻辑**：把一段常用逻辑（这里是布尔开关）封装起来，多个组件都能调用，各自拥有独立的状态。注意它复用的是"逻辑"，不是"状态"——两个组件各调一次 `useToggle`，状态互不干扰。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 167：自定义 Hook：useFetch（含加载态与竞态处理）</h3>

```jsx
import { useState, useEffect } from 'react';

function useFetch(url) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  useEffect(() => {
    let ignore = false;
    setLoading(true);
    fetch(url)
      .then(r => r.json())
      .then(d => { if (!ignore) { setData(d); setLoading(false); } })
      .catch(e => { if (!ignore) { setError(e); setLoading(false); } });
    return () => { ignore = true; };
  }, [url]);
  return { data, loading, error };
}

function Users() {
  const { data, loading, error } = useFetch('/api/users');
  if (loading) return <p>加载中...</p>;
  if (error)   return <p>出错了</p>;
  return <ul>{data.map(u => <li key={u.id}>{u.name}</li>)}</ul>;
}
```

**详解**：这个 `useFetch` 把"请求数据"这套通用逻辑——加载态、错误态、竞态处理（示例 146 的 `ignore` 标志）——全部封装。任何组件只要 `const { data, loading, error } = useFetch(url)` 就能拿到完整的请求状态，组件本身只关心怎么渲染。这正是自定义 Hook 的威力：把重复的副作用逻辑抽象成一个可复用的"能力"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 168：自定义 Hook：useLocalStorage（与浏览器存储同步）</h3>

```jsx
import { useState, useEffect } from 'react';

function useLocalStorage(key, initial) {
  const [value, setValue] = useState(() => {
    const stored = localStorage.getItem(key);   // 惰性初始化，只读一次
    return stored ? JSON.parse(stored) : initial;
  });
  useEffect(() => {
    localStorage.setItem(key, JSON.stringify(value)); // value 变了就写回
  }, [key, value]);
  return [value, setValue];
}

function Settings() {
  const [name, setName] = useLocalStorage('name', '');
  return <input value={name} onChange={e => setName(e.target.value)} />;
}
```

**详解**：这个 Hook 让一段状态自动与 `localStorage` 保持同步，刷新页面后仍能恢复。两个要点：① 初始值用**惰性初始化**（给 `useState` 传函数），保证只在首次渲染读一次 localStorage，不浪费性能；② 用 `useEffect` 监听 `value` 变化，每次变动就 `JSON.stringify` 写回。它的接口和 `useState` 几乎一样（返回 `[值, 设置函数]`），所以用起来毫无负担——这也是设计自定义 Hook 的好习惯：**贴近内置 Hook 的使用方式**。

---

## 八、React 18 新增 Hooks

> React 18 新增了 5 个 Hook，它们大多服务于两个目标：**并发渲染下的流畅体验**（`useTransition`、`useDeferredValue`）和**更严谨的底层能力**（`useId`、`useSyncExternalStore`、`useInsertionEffect`）。
>
> 先建立一个核心概念——**紧急更新 vs 非紧急（过渡）更新**：像"在输入框里打字"必须立即响应（紧急）；而"根据输入过滤一个上万条的大列表"可以稍微延迟、甚至被打断（非紧急）。React 18 让你能区分这两类更新，从而避免"打字卡顿"。
>
> 本章从最简单的 `useId` 讲到 `useInsertionEffect`，共 18 个示例。这些 Hook 属于进阶内容，日常开发中 `useId` 和 `useTransition` 用得较多，其余按需了解即可。

### （A）useId —— 生成稳定唯一的 id

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 169：useId 生成唯一 id</h3>

```jsx
import { useId } from 'react';

function Field() {
  const id = useId();
  return (
    <div>
      <label htmlFor={id}>邮箱</label>
      <input id={id} type="email" />
    </div>
  );
}
```

**详解**：`useId` 生成一个在整个应用中唯一的字符串 id（形如 `:r0:`）。最典型的用途是**关联 `<label>` 和表单控件**——`label` 的 `htmlFor` 要和 `input` 的 `id` 一致，点击文字标签就能聚焦到输入框（对无障碍很重要）。手写固定 id（如 `id="email"`）在组件被复用多次时会导致 id 重复，而 `useId` 保证每个组件实例拿到不同的 id。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 170：为什么需要 useId（服务端渲染的坑）</h3>

```jsx
// ❌ 用随机数/自增：服务端和客户端生成的 id 可能不一致，导致 hydration 报错
// let counter = 0;
// const id = `field-${counter++}`;

// ✅ 用 useId：React 保证服务端和客户端生成完全一致的 id
function Field() {
  const id = useId();
  return <input id={id} />;
}
```

**详解**：既然只是要个唯一 id，为什么不用 `Math.random()` 或自增计数器？因为在**服务端渲染（SSR）**场景下，同一个组件会先在服务端渲染出 HTML、再到客户端"注水（hydration）"接管。如果两端各自用随机数生成 id，就会不一致，React 会报"服务端和客户端不匹配"的错误。`useId` 的算法保证两端生成**完全相同**的 id，专为解决这个问题而生。这也是它比自己造 id 更可靠的原因。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 171：用一个 useId 生成多个相关 id</h3>

```jsx
function Form() {
  const id = useId(); // 一个组件通常只调用一次
  return (
    <>
      <label htmlFor={`${id}-name`}>姓名</label>
      <input id={`${id}-name`} />

      <label htmlFor={`${id}-email`}>邮箱</label>
      <input id={`${id}-email`} />
    </>
  );
}
```

**详解**：当一个组件里有多个字段时，**不需要调用多次 `useId`**。更推荐调用一次拿到一个基础 id，再通过拼接后缀（`${id}-name`、`${id}-email`）派生出多个相关 id。这样既保证唯一，又能体现它们同属一组，代码也更简洁。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 172：useId 的注意事项——不要用作列表 key</h3>

```jsx
// ❌ 错误用法：useId 不是为列表 key 设计的
// {items.map(item => <li key={useId()}>...</li>)}  // 甚至语法上就不允许

// ✅ 列表 key 应该用数据本身的稳定 id
// {items.map(item => <li key={item.id}>{item.text}</li>)}
```

**详解**：初学者容易误用——`useId` **不能**用来生成列表渲染的 `key`。原因：① Hook 不能在循环里调用（违反 Hook 规则）；② `key` 应该来自数据本身的稳定标识（如 `item.id`），用于让 React 追踪列表项的身份（见第五章）。`useId` 的定位是"为 DOM 元素生成唯一属性 id"，两者用途完全不同，别混淆。

### （B）useTransition / startTransition —— 标记非紧急更新

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 173：为什么需要过渡更新（问题场景）</h3>

```jsx
// 问题：输入框每敲一个字，都要过滤一个上万条的大列表并重新渲染，
// 导致输入框卡顿、打字不跟手
function SlowSearch({ allItems }) {
  const [query, setQuery] = useState('');
  const list = allItems.filter(i => i.includes(query)); // 昂贵
  return (
    <>
      <input value={query} onChange={e => setQuery(e.target.value)} />
      <ul>{list.map((i, k) => <li key={k}>{i}</li>)}</ul>
    </>
  );
}
```

**详解**：先理解要解决的问题。上面这个组件里，"更新输入框"和"过滤+渲染大列表"被绑在同一次更新中。渲染大列表很慢，会阻塞浏览器，结果就是**用户打字时输入框明显卡顿**。理想情况是：输入框要立即响应（紧急），大列表可以慢一点、边算边被新输入打断（非紧急）。`useTransition` 和 `useDeferredValue` 就是来做这种区分的。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 174：useTransition 基础用法</h3>

```jsx
import { useTransition } from 'react';

function Demo() {
  const [isPending, startTransition] = useTransition();
  const handle = () => {
    startTransition(() => {
      // 这里面的 state 更新被标记为"非紧急过渡更新"
      doSomeHeavyStateUpdate();
    });
  };
  return (
    <div>
      {isPending && <span>处理中...</span>}
      <button onClick={handle}>执行</button>
    </div>
  );
}
```

**详解**：`useTransition` 返回两样东西：① `isPending`——布尔值，表示过渡更新是否正在进行（可用来显示 loading 提示）；② `startTransition`——把包在它里面的 state 更新**标记为非紧急**。被标记的更新可以被 React 打断/延后，从而优先处理紧急更新（如用户输入），保持界面响应。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 175：useTransition 实战——搜索大列表不卡顿</h3>

```jsx
import { useState, useTransition } from 'react';

function SearchList({ allItems }) {
  const [query, setQuery] = useState('');
  const [list, setList] = useState(allItems);
  const [isPending, startTransition] = useTransition();

  const onChange = (e) => {
    const value = e.target.value;
    setQuery(value);                    // 紧急更新：输入框立即响应
    startTransition(() => {
      setList(allItems.filter(i => i.includes(value))); // 非紧急：过滤大列表
    });
  };

  return (
    <>
      <input value={query} onChange={onChange} />
      {isPending && <span>更新中...</span>}
      <ul style={{ opacity: isPending ? 0.5 : 1 }}>
        {list.map((i, k) => <li key={k}>{i}</li>)}
      </ul>
    </>
  );
}
```

**详解**：这是 `useTransition` 最经典的应用，对比示例 173：把 `setQuery`（输入框的值）留作**紧急更新**，保证打字流畅；把 `setList`（过滤大列表）放进 `startTransition`，标记为**非紧急**。这样即便列表还在慢慢重算，输入框也不会卡。`isPending` 期间可以给列表加个半透明效果，暗示"结果正在更新"。**注意**：`startTransition` 里只能放 state 更新，不要放异步/请求逻辑。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 176：startTransition（非 Hook 版本）</h3>

```jsx
import { startTransition } from 'react';

function TabButton({ onSelect }) {
  const click = () => {
    startTransition(() => {
      onSelect(); // 切换标签这类可能触发大量渲染的更新，标记为过渡
    });
  };
  return <button onClick={click}>切换标签</button>;
}
```

**详解**：`startTransition` 也能作为**独立函数**从 `react` 直接导入使用，用法和 `useTransition` 返回的那个一样。区别在于：独立版本**没有 `isPending`**（拿不到进行中的状态）。所以——需要显示"加载中"提示时用 `useTransition`（Hook 版）；只是想标记更新为非紧急、不关心 pending 状态时，用独立的 `startTransition` 即可（也可用在组件函数之外）。

### （C）useDeferredValue —— 延迟一个值

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 177：useDeferredValue 基础</h3>

```jsx
import { useState, useDeferredValue } from 'react';

function Demo() {
  const [text, setText] = useState('');
  const deferredText = useDeferredValue(text); // text 的"延迟版本"
  return (
    <>
      <input value={text} onChange={e => setText(e.target.value)} />
      <p>实时：{text}</p>
      <p>延迟：{deferredText}</p>
    </>
  );
}
```

**详解**：`useDeferredValue` 接收一个值，返回它的"延迟版本"。当原值 `text` 快速变化时，`deferredText` 会"落后"一点——React 会优先用新值更新紧急部分（输入框），然后在空闲时才把 `deferredText` 追上。快速输入时你能看到"实时"立刻变、"延迟"稍后才跟上。它和 `useTransition` 目标相同，但**作用于"值"而非"更新函数"**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 178：useDeferredValue 实战——延迟渲染大列表</h3>

```jsx
import { useState, useDeferredValue, useMemo } from 'react';

function Search({ allItems }) {
  const [text, setText] = useState('');
  const deferredText = useDeferredValue(text);

  // 用延迟值做昂贵计算，配合 useMemo 缓存
  const results = useMemo(
    () => allItems.filter(i => i.includes(deferredText)),
    [allItems, deferredText]
  );

  return (
    <>
      <input value={text} onChange={e => setText(e.target.value)} />
      <ul style={{ opacity: text !== deferredText ? 0.5 : 1 }}>
        {results.map((r, k) => <li key={k}>{r}</li>)}
      </ul>
    </>
  );
}
```

**详解**：输入框绑定的是即时的 `text`（打字流畅），而昂贵的过滤用延迟的 `deferredText`——这样大列表的重算被"推后"，不阻塞输入。配合 `useMemo` 缓存过滤结果（依赖 `deferredText`），避免每次渲染都重算。通过对比 `text !== deferredText` 可判断"列表是否还在追赶中"，给个半透明提示。它特别适合**你只能拿到值、拿不到更新函数**的场景（比如值来自 props）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 179：useTransition vs useDeferredValue 如何选择</h3>

```jsx
// 场景一：你自己触发 state 更新 → 用 useTransition 包住更新
startTransition(() => setList(filter(value)));

// 场景二：你只有一个值（可能来自 props），无法控制它何时更新 → 用 useDeferredValue
const deferred = useDeferredValue(value);
```

**详解**：两者都能避免"昂贵更新拖慢紧急更新"，选择取决于你**能否控制那次 state 更新**：
- **能控制更新**（你手里有 `setXxx`）→ 用 **`useTransition`**，把慢更新包进 `startTransition`；
- **只拿到一个值**（比如值是父组件传来的 prop，你无法包裹它的更新）→ 用 **`useDeferredValue`**，对这个值做延迟。

简单记：**"包更新"用 useTransition，"延迟值"用 useDeferredValue**。功能重叠时优先 `useTransition`（能顺便拿到 `isPending`）。

### （D）useSyncExternalStore —— 订阅外部数据源

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 180：useSyncExternalStore 是什么</h3>

```jsx
import { useSyncExternalStore } from 'react';

const value = useSyncExternalStore(
  subscribe,       // 订阅函数：数据变化时调用回调，返回取消订阅的函数
  getSnapshot,     // 读取当前值（客户端）
  getServerSnapshot // 读取当前值（服务端，SSR 时用，可选）
);
```

**详解**：`useSyncExternalStore` 用来**订阅 React 之外的数据源**，并在数据变化时安全地触发重渲染。它需要三个参数：① `subscribe`——注册一个"数据变了就调用"的回调，并返回取消订阅的函数；② `getSnapshot`——返回外部数据当前的值；③ 可选的服务端快照。它主要给**状态管理库作者**用（Redux、Zustand 等内部就用它），保证在并发渲染下读取外部数据不会出现"撕裂"（同一次渲染里读到新旧不一致的值）。业务代码里偶尔用它订阅浏览器 API。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 181：订阅浏览器在线状态</h3>

```jsx
import { useSyncExternalStore } from 'react';

function useOnlineStatus() {
  return useSyncExternalStore(
    (callback) => {
      window.addEventListener('online', callback);
      window.addEventListener('offline', callback);
      return () => {
        window.removeEventListener('online', callback);
        window.removeEventListener('offline', callback);
      };
    },
    () => navigator.onLine, // 客户端快照：当前是否在线
    () => true              // 服务端快照：SSR 时默认在线
  );
}

function StatusBar() {
  const isOnline = useOnlineStatus();
  return <p>{isOnline ? '✅ 在线' : '❌ 离线'}</p>;
}
```

**详解**：这是最实用的例子——把它封装成自定义 Hook `useOnlineStatus`。`subscribe` 里监听浏览器的 `online`/`offline` 事件（并返回清理函数）；`getSnapshot` 返回 `navigator.onLine`。当网络状态变化，事件触发 `callback`，React 重新读取快照并重渲染。相比自己用 `useEffect` + `useState` 实现，`useSyncExternalStore` 在并发场景下更严谨可靠。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 182：订阅窗口尺寸</h3>

```jsx
function useWindowWidth() {
  return useSyncExternalStore(
    (callback) => {
      window.addEventListener('resize', callback);
      return () => window.removeEventListener('resize', callback);
    },
    () => window.innerWidth,
    () => 1024 // 服务端没有 window，给个默认值
  );
}

function Responsive() {
  const width = useWindowWidth();
  return <p>{width < 768 ? '移动端布局' : '桌面布局'}</p>;
}
```

**详解**：同样的模式可用于订阅任何浏览器 API。这里订阅 `resize` 事件、快照返回 `window.innerWidth`，实现一个响应式宽度 Hook。注意**服务端快照**返回一个默认值（因为 SSR 环境没有 `window`），避免报错。这个模式（subscribe 监听事件 + getSnapshot 读取值）几乎适用于所有"外部会变化的数据"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 183：订阅自定义 store（迷你状态管理）</h3>

```jsx
// 一个极简的、React 之外的全局 store
const store = {
  state: 0,
  listeners: new Set(),
  increment() { this.state++; this.listeners.forEach(l => l()); },
  subscribe(l) { this.listeners.add(l); return () => this.listeners.delete(l); },
  getSnapshot() { return this.state; },
};

function Counter() {
  const count = useSyncExternalStore(
    store.subscribe.bind(store),
    store.getSnapshot.bind(store)
  );
  return <button onClick={() => store.increment()}>{count}</button>;
}
```

**详解**：这个例子展示了状态管理库的核心原理。`store` 完全独立于 React：它保存 `state`、维护一组 `listeners`，`increment` 改值后通知所有监听者。组件用 `useSyncExternalStore` 订阅它——`store` 一变，所有订阅的组件自动重渲染。多个组件订阅同一个 `store` 就能共享状态。Zustand 等轻量状态库正是基于这个思路实现的。

### （E）useInsertionEffect —— 供样式库使用

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 184：useInsertionEffect（CSS-in-JS 场景）</h3>

```jsx
import { useInsertionEffect } from 'react';

// 主要给 CSS-in-JS 库（如 styled-components、Emotion）作者使用
function useCss(rule) {
  useInsertionEffect(() => {
    const style = document.createElement('style');
    style.textContent = rule;
    document.head.appendChild(style);
    return () => document.head.removeChild(style);
  }, [rule]);
}
```

**详解**：`useInsertionEffect` 是三个 effect 里执行时机**最早**的——它在 DOM 变更之前、`useLayoutEffect` 之前触发。它的唯一用途是**给 CSS-in-JS 库在渲染前动态注入 `<style>` 标签**，这样后续的布局测量才能拿到正确的样式。**普通业务开发几乎永远用不到它**，它是给库作者准备的底层工具。你只需知道它存在、以及它的执行时机最靠前即可。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 185：三种 effect 的执行时机对比</h3>

```jsx
useInsertionEffect(() => { /* ① 最早：DOM 变更前，用于注入样式 */ });
useLayoutEffect(() =>   { /* ② 其次：DOM 变更后、浏览器绘制前（同步，可测量布局）*/ });
useEffect(() =>         { /* ③ 最后：浏览器绘制后（异步，日常首选）*/ });
```

**详解**：把 React 18 里三个 effect 的执行顺序记牢：**`useInsertionEffect` → `useLayoutEffect` → `useEffect`**。
- `useInsertionEffect`：DOM 变更**前**，只用于样式库注入 CSS；
- `useLayoutEffect`：DOM 变更后、绘制前，用于同步读取/修改布局（避免闪烁，见第七章）；
- `useEffect`：绘制后异步执行，**99% 的副作用都用它**。

按"用得多少"排序恰好相反：日常 `useEffect` 最常用，`useLayoutEffect` 偶尔用，`useInsertionEffect` 基本只有库作者用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 186：React 18 新 Hook 使用频率小结</h3>

```jsx
// 高频（业务开发常用）：
useId();            // 表单 label 关联、无障碍
useTransition();    // 大列表/重渲染保持界面流畅

// 中频（按需使用）：
useDeferredValue(); // 只有值、无法控制更新时的性能优化

// 低频（多为库作者使用）：
useSyncExternalStore(); // 订阅外部数据源、写状态管理库
useInsertionEffect();   // CSS-in-JS 样式注入
```

**详解**：给本章做个务实的总结，帮你分配学习精力：
- **优先掌握 `useId` 和 `useTransition`**——它们在真实业务里最常用，价值最高；
- **`useDeferredValue` 了解并会用即可**，它和 `useTransition` 二选一；
- **`useSyncExternalStore` 和 `useInsertionEffect` 知道它们解决什么问题即可**，除非你在写状态管理库或样式库，否则很少直接调用。

不必因为"是新 Hook"就强行在项目里使用它们——**只在遇到对应的问题（如打字卡顿、SSR id 不一致）时才用**，这才是正确的态度。

---

## 九、并发特性（Concurrent Features）

> **并发渲染（Concurrent Rendering）是什么？** 这是 React 18 最底层、最重要的升级。在 React 17 及以前，一旦开始渲染就必须一口气做完，期间会阻塞主线程、页面无法响应用户操作。React 18 的并发渲染让渲染过程**可以被中断、暂停、恢复、甚至放弃**，从而优先响应更紧急的操作（如用户输入），保证界面始终流畅。
>
> 上一章的 `useTransition`、`useDeferredValue`，以及本章的 `Suspense`、流式 SSR，都是建立在并发渲染之上的具体能力。
>
> 本章从"并发是什么"讲到"代码分割、Suspense、SSR 注水"，共 15 个示例。这些偏进阶，其中 `lazy + Suspense`（代码分割）在真实项目中最常用。

### （A）并发渲染基础

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 187：什么是并发渲染（如何启用）</h3>

```jsx
import { createRoot } from 'react-dom/client';
import App from './App';

// 用 createRoot 创建根，就自动启用了并发渲染能力
createRoot(document.getElementById('root')).render(<App />);
```

**详解**：好消息是——**你不需要做任何特殊配置来"开启"并发**。只要用 React 18 的 `createRoot`（第一章示例 2）创建应用，并发渲染能力就已就位。它平时"隐身"工作，只有当你使用 `useTransition`、`useDeferredValue`、`Suspense` 等 API 时，才会真正发挥"可中断渲染"的威力。可以把并发理解为 React 18 的"新引擎"，上层这些 API 是使用这个引擎的方向盘。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 188：并发不是多线程（澄清误解）</h3>

```jsx
// 并发 ≠ 多线程。JavaScript 仍然是单线程的。
// "并发"指的是：React 能把一次大的渲染工作拆成小块，
// 在每小块之间"让出"主线程去处理更紧急的事（如用户点击），
// 之后再回来继续渲染。
```

**详解**：这是个常见误解。"并发"**不是**开了多个线程并行计算——JavaScript 依然是单线程。React 的并发指的是一种**调度策略**：把渲染工作切成许多小片，每做完一片就检查"有没有更紧急的任务（比如用户刚点了按钮）"，有就先去处理，然后再回来接着渲染。就像一个人做长任务时，会时不时抬头看看有没有更急的事。理解这一点，才能明白为什么并发能"避免卡顿"却不是"变快了"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 189：并发带来的自动批处理</h3>

```jsx
function Demo() {
  const [a, setA] = useState(0);
  const [b, setB] = useState(0);
  const handle = () => {
    fetch('/api').then(() => {
      setA(x => x + 1);
      setB(x => x + 1); // React 18：即使在 Promise 里，也合并成一次重渲染
    });
  };
  return <button onClick={handle}>{a}-{b}</button>;
}
```

**详解**：自动批处理是并发引擎带来的"免费"优化（第四章示例 83 已介绍）。这里再从并发角度强调：React 18 之前，只有事件处理函数里的多次 `setState` 会合并；在 `Promise`、`setTimeout`、原生事件里则不会。React 18 统一了行为——**任何地方的多次更新都自动合并成一次重渲染**。你什么都不用做就能享受这个性能提升，这正是升级到 `createRoot` 的收益之一。

### （B）代码分割：lazy + Suspense

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 190：什么是代码分割 + lazy 懒加载</h3>

```jsx
import { lazy } from 'react';

// 不再一开始就打包/下载 HeavyComponent，而是用到时才动态加载
const HeavyComponent = lazy(() => import('./HeavyComponent'));
```

**详解**：默认情况下，打包工具会把所有组件打进一个大文件，用户首次访问就得下载全部代码，首屏变慢。"**代码分割**"就是把代码拆成多个小块，按需加载。`React.lazy(() => import('./xxx'))` 配合动态 `import()`，让某个组件**只在真正要渲染它时才下载**。比如一个很重的图表组件，用户不打开对应页面就不会加载它。`lazy` 返回一个"懒加载组件"，但它必须配合下面的 `Suspense` 使用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 191：Suspense 提供加载中占位</h3>

```jsx
import { Suspense, lazy } from 'react';

const HeavyComponent = lazy(() => import('./HeavyComponent'));

function App() {
  return (
    <Suspense fallback={<p>加载组件中...</p>}>
      <HeavyComponent />
    </Suspense>
  );
}
```

**详解**：`lazy` 组件在下载期间还"没准备好"，React 需要知道这段时间显示什么——这就是 `<Suspense>` 的作用。把懒加载组件包在 `<Suspense>` 里，`fallback` 属性指定"加载中"要显示的占位内容。加载完成后，React 自动用真正的组件替换掉 `fallback`。`Suspense` 可以理解为"**为还没准备好的内容提供等待界面**"的边界。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 192：多个 lazy 组件共享一个 Suspense</h3>

```jsx
const Chart = lazy(() => import('./Chart'));
const Table = lazy(() => import('./Table'));

function Dashboard() {
  return (
    <Suspense fallback={<p>加载仪表盘...</p>}>
      <Chart />
      <Table />
    </Suspense>
  );
}
```

**详解**：一个 `<Suspense>` 可以包裹多个懒加载组件。此时它会**等到里面所有组件都加载完成**，才一次性显示全部内容；在此之前统一显示同一个 `fallback`。这适合"这几块内容要一起出现"的场景。如果希望它们各自独立加载、谁好了先显示谁，就给它们各自包一个 `Suspense`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 193：路由级懒加载（最常见用法）</h3>

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

**详解**：代码分割最实用的场景是**按路由拆分**——每个页面单独打包，用户访问哪个页面才下载哪个页面的代码。这样首屏只需加载当前页，大幅减少初始体积。做法就是把每个页面组件用 `lazy` 包起来，再用一个 `Suspense` 包住路由。这是中大型 React 应用几乎必备的优化。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 194：嵌套 Suspense（分级加载）</h3>

```jsx
function Page() {
  return (
    <Suspense fallback={<p>加载整个页面...</p>}>
      <Header />
      <Suspense fallback={<p>加载正文...</p>}>
        <Content />
      </Suspense>
    </Suspense>
  );
}
```

**详解**：`Suspense` 可以嵌套，实现"分级、渐进式"的加载体验。外层负责整体框架，内层负责某块较慢的内容。这样 `Header` 一旦就绪就能先显示，`Content` 还在加载时只在它自己的区域显示"加载正文..."，而不是让整个页面都卡在等待。合理布置 Suspense 边界，能让页面**逐步呈现**而非"全有或全无"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 195：lazy 加载失败的处理（配合错误边界）</h3>

```jsx
function App() {
  return (
    <ErrorBoundary fallback={<p>组件加载失败，请刷新重试</p>}>
      <Suspense fallback={<p>加载中...</p>}>
        <HeavyComponent />
      </Suspense>
    </ErrorBoundary>
  );
}
// ErrorBoundary 是一个捕获渲染错误的类组件（见第十二章）
```

**详解**：懒加载依赖网络下载，可能失败（断网、文件 404）。`Suspense` 只管"加载中"状态，**不负责错误处理**。要优雅地处理加载失败，需要在外层再包一个"错误边界"（Error Boundary，见第十二章）——它能捕获子树的渲染错误并显示兜底 UI。所以健壮的懒加载结构是：**ErrorBoundary（管错误）+ Suspense（管等待）+ lazy 组件**三层嵌套。

### （C）Suspense 与数据请求

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 196：Suspense + 数据请求</h3>

```jsx
// 需配合支持 Suspense 的数据方案（React Query、SWR、Relay、RSC 等）
function Profile() {
  return (
    <Suspense fallback={<p>加载用户资料...</p>}>
      <UserDetails userId={1} />
    </Suspense>
  );
}

function UserDetails({ userId }) {
  // 假设 useUser 是支持 Suspense 的：数据没好时会"挂起"，由外层 Suspense 接管
  const user = useUser(userId);
  return <h3>{user.name}</h3>;
}
```

**详解**：`Suspense` 不仅能用于代码加载，也能用于**数据加载**。当子组件的数据还没就绪时，它会"挂起（suspend）"，由最近的 `Suspense` 显示 `fallback`；数据到位后再渲染真正内容。这让"加载中"逻辑从组件里剥离出来，代码更干净——组件里直接写 `const user = useUser(id)` 就好，仿佛数据是同步的。**注意**：这需要数据库/框架支持 Suspense（如 React Query 开启 suspense 模式、Relay、React Server Components），不能对普通 `fetch` 直接用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 197：用骨架屏作为 fallback</h3>

```jsx
function Skeleton() {
  return (
    <div className="skeleton">
      <div className="skeleton-line" />
      <div className="skeleton-line" />
    </div>
  );
}

function App() {
  return (
    <Suspense fallback={<Skeleton />}>
      <ArticleList />
    </Suspense>
  );
}
```

**详解**：`fallback` 不一定是一句"加载中"文字，用**骨架屏（Skeleton）**体验更好——它用灰色占位块模拟即将出现的内容轮廓，让用户感觉"内容马上就来"，减少等待焦虑。因为 `fallback` 接收任意 JSX，你可以传一个精心设计的占位组件。这是现代应用常见的加载体验优化。

### （D）过渡与 Suspense 结合

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 198：用 useTransition 避免 fallback 闪烁</h3>

```jsx
import { useState, useTransition, Suspense } from 'react';

function Tabs() {
  const [tab, setTab] = useState('home');
  const [isPending, startTransition] = useTransition();

  const select = (next) => {
    startTransition(() => setTab(next)); // 切换标记为过渡
  };

  return (
    <>
      <button onClick={() => select('home')}>首页</button>
      <button onClick={() => select('photos')}>相册</button>
      {isPending && <span>加载中...</span>}
      <Suspense fallback={<p>首次加载...</p>}>
        {tab === 'home' ? <Home /> : <Photos />}
      </Suspense>
    </>
  );
}
```

**详解**：这是并发的一个精妙用法。切换 Tab 时，如果新内容需要 Suspense 加载，默认会立刻把当前内容替换成 `fallback`，造成"闪一下空白"的糟糕体验。用 `startTransition` 包住切换更新后，React 会**保留旧内容继续显示**，直到新内容准备好才切换过去，避免了 fallback 闪烁。`isPending` 期间可以给个不打断视觉的轻提示。这体现了"过渡更新"与 Suspense 的协作。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 199：并发渲染避免切换卡顿（完整对比）</h3>

```jsx
function App() {
  const [tab, setTab] = useState('home');
  const [isPending, startTransition] = useTransition();

  const switchTab = (name) => {
    startTransition(() => setTab(name)); // 切换到重内容时保持界面响应
  };

  return (
    <>
      <button onClick={() => switchTab('home')}>首页</button>
      <button onClick={() => switchTab('list')}>大列表</button>
      {isPending && <span>切换中...</span>}
      {tab === 'home' ? <Home /> : <BigList />}
    </>
  );
}
```

**详解**：假设 `<BigList>` 渲染很慢。不用过渡时，点"大列表"按钮后，界面会**卡住**直到大列表渲染完，连按钮的高亮反馈都延迟。用 `startTransition` 把切换标记为非紧急后，React 会在后台渲染大列表，同时保持界面响应——按钮立即有反馈，`isPending` 显示"切换中"，大列表好了再一次性呈现。这就是并发渲染"避免卡顿"的直观体现。

### （E）服务端渲染（SSR）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 200：hydrateRoot 注水</h3>

```jsx
import { hydrateRoot } from 'react-dom/client';
import App from './App';

// SSR 场景：服务端先生成 HTML，客户端用 hydrateRoot 让它"活"起来
hydrateRoot(document.getElementById('root'), <App />);
```

**详解**：服务端渲染（SSR）会先在服务器上把组件渲染成 HTML 字符串发给浏览器，让用户**更快看到内容**（首屏快、利于 SEO）。但这段 HTML 还是"死"的，没有事件、没有交互。客户端需要用 `hydrateRoot`（React 18 的注水 API，取代旧的 `ReactDOM.hydrate`）把 React 的逻辑"附加"到这些已有 HTML 上，绑定事件、接管交互——这个过程叫"**注水（hydration）**"。注意：注水时组件的渲染结果必须和服务端一致，否则会报不匹配警告（这也是 `useId` 存在的原因，见第八章示例 170）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 201：React 18 流式 SSR 与选择性注水（了解）</h3>

```jsx
// 服务端：用 renderToPipeableStream 以"流"的方式边生成边发送 HTML
import { renderToPipeableStream } from 'react-dom/server';

const { pipe } = renderToPipeableStream(<App />, {
  onShellReady() {
    // 外壳准备好就开始发送，慢的部分（Suspense 包裹）稍后再流式补上
    pipe(response);
  },
});
```

**详解**：React 18 大幅增强了 SSR，两个关键改进（作为概念了解即可）：
1. **流式渲染（Streaming SSR）**：用 `renderToPipeableStream` 把 HTML **边生成边发送**给浏览器，不用等整页都渲染完。页面里被 `<Suspense>` 包裹的慢部分，会先发一个占位，等数据好了再通过流补发。
2. **选择性注水（Selective Hydration）**：不必等所有 JS 下载/注水完才能交互——React 能优先给用户正在操作的部分注水。

实际项目中，这些通常由 **Next.js、Remix** 等框架封装好，你很少直接写 `renderToPipeableStream`。理解"流式 + 按需注水让首屏和可交互时间都更早"这个价值即可。

---

## 十、性能优化

> **先记住一句话：不要过早优化。** React 本身已经很快，大多数应用无需刻意优化。优化的正确顺序是——**先测量，找到真正的瓶颈，再针对性地优化**，而不是到处套 `memo`、`useMemo`。盲目优化不仅让代码变复杂，有时反而更慢。
>
> **优化的核心思路**：React 慢，通常是因为"**不必要的重新渲染**"或"**一次渲染做了太多昂贵的工作**"。本章的技术都围绕这两点：减少重渲染次数（`memo`/`useMemo`/`useCallback`/组件拆分）、减少每次渲染的工作量（虚拟化/代码分割/防抖）。
>
> 本章从"如何认识重渲染"讲到具体优化手段和 checklist，共 17 个示例。

### （A）优化前的认知

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 202：先测量，别凭感觉优化</h3>

```jsx
// 用 React DevTools 的 Profiler 面板录制一次交互，
// 看哪些组件渲染了、耗时多少，再决定优化谁。

// 也可以用 Profiler 组件在代码里测量：
import { Profiler } from 'react';

<Profiler id="List" onRender={(id, phase, actualDuration) => {
  console.log(id, phase, actualDuration + 'ms'); // 本次渲染耗时
}}>
  <List />
</Profiler>
```

**详解**：优化的第一步永远是**测量**，而不是猜。安装浏览器的 **React DevTools** 扩展，用它的 **Profiler（性能分析器）** 录制一次操作，就能看到每个组件渲染了几次、各耗时多少，一眼定位瓶颈。代码里也可用 `<Profiler>` 组件包住某块，通过 `onRender` 回调拿到渲染耗时。**没有数据支撑的优化就是在浪费时间**——先找到真正慢的那 20%，集中火力。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 203：理解 React 何时重新渲染</h3>

```jsx
function Parent() {
  const [count, setCount] = useState(0);
  return (
    <div>
      <button onClick={() => setCount(count + 1)}>{count}</button>
      <Child /> {/* 父每次重渲染，Child 默认也会跟着重渲染 */}
    </div>
  );
}

function Child() {
  console.log('Child 渲染了'); // 点按钮会发现它也在打印
  return <p>我是子组件</p>;
}
```

**详解**：搞懂"何时重渲染"是优化的基础。一个组件会在以下情况重新渲染：① 它自己的 state 变了；② 它收到的 props 变了；③ **它的父组件重渲染了**（无论 props 变没变）。第③点最关键——上例点按钮改的是 `Parent` 的 state，但 `Child` 明明没接收任何会变的 props，却也跟着重渲染了。大多数"不必要的重渲染"都源于此。`React.memo` 就是用来打断这种连锁的。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 204：重新渲染 ≠ 操作真实 DOM</h3>

```jsx
// "重新渲染"只是 React 重新执行组件函数、生成新的虚拟 DOM 并对比，
// 只有对比出差异的部分，才会真正更新到浏览器 DOM。
// 所以：并非每次重渲染都很慢，也不是所有重渲染都需要优化。
```

**详解**：别把"重新渲染"想得太可怕。它指的是 React 重新调用组件函数、生成新的虚拟 DOM，然后和旧的对比（diff）——**只有真正变化的节点才会更新到真实 DOM**（真实 DOM 操作才是最贵的）。很多重渲染其实很轻量、无需优化。所以优化的目标不是"消灭一切重渲染"，而是"消灭那些**又频繁又昂贵**的重渲染"。这也是为什么要先测量（示例 202）。

### （B）React.memo —— 跳过不必要的重渲染

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 205：React.memo 基础</h3>

```jsx
const Child = React.memo(function Child({ text }) {
  console.log('Child 渲染：', text);
  return <li>{text}</li>;
});

// 现在：只有 text 这个 prop 变化时，Child 才会重渲染；
// 父组件因别的原因重渲染时，Child 会被跳过。
```

**详解**：`React.memo` 是一个"高阶组件"——把组件包起来后，React 会在每次父组件重渲染时**浅比较它的 props**：如果所有 props 都没变，就**跳过**这次重渲染，复用上次结果。这正好解决示例 203 的问题：父组件 state 变化不再连累无关的子组件。它适合"渲染较重、且 props 不常变"的组件。注意：props 频繁变化的组件用 memo 反而多做了比较，得不偿失。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 206：React.memo 为什么会"失效"</h3>

```jsx
const Child = React.memo(function Child({ config, onClick }) {
  return <button onClick={onClick}>{config.label}</button>;
});

function Parent() {
  const [n, setN] = useState(0);
  // ❌ 每次渲染都新建对象和函数，引用都变了，memo 形同虚设
  return (
    <>
      <button onClick={() => setN(n + 1)}>{n}</button>
      <Child config={{ label: '按钮' }} onClick={() => console.log('click')} />
    </>
  );
}
```

**详解**：这是 memo 最大的坑。`React.memo` 靠**浅比较**判断 props 是否变化，而对象、数组、函数每次渲染都是**新创建的、引用不同**。上例每次渲染 `Parent` 都新建了 `{ label: '按钮' }` 和 `() => ...`，浅比较认为"props 变了"，于是 `Child` 照样重渲染——memo 白加了。解决办法就是用 `useMemo` 缓存对象、`useCallback` 缓存函数（见示例 209、210），让引用保持稳定。**memo + useMemo/useCallback 通常要配套使用**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 207：React.memo 自定义比较函数</h3>

```jsx
const User = React.memo(
  function User({ user }) {
    return <p>{user.name}</p>;
  },
  (prevProps, nextProps) => {
    // 返回 true：视为"相同"，跳过重渲染
    // 返回 false：视为"不同"，重新渲染
    return prevProps.user.id === nextProps.user.id;
  }
);
```

**详解**：`React.memo` 的第二个参数可以传一个自定义比较函数，你自己决定"props 算不算变了"。这里只关心 `user.id`——只要 id 没变就跳过重渲染，哪怕 `user` 对象引用变了。适合"props 是复杂对象，但只有某些字段真正影响渲染"的场景。**注意比较逻辑本身别写得太重**，否则比较的开销可能超过省下的渲染开销。多数情况下默认的浅比较就够了。

### （C）useMemo / useCallback —— 稳定引用与缓存计算

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 208：useMemo 缓存昂贵计算</h3>

```jsx
import { useMemo } from 'react';

function ProductList({ products, keyword }) {
  const filtered = useMemo(() => {
    console.log('执行过滤（昂贵）'); // 依赖不变就不会打印
    return products.filter(p => p.name.includes(keyword));
  }, [products, keyword]); // 只有这两个变化时才重新计算
  return <ul>{filtered.map(p => <li key={p.id}>{p.name}</li>)}</ul>;
}
```

**详解**：`useMemo(fn, deps)` 会"记住" `fn` 的返回值，只有依赖 `deps` 变化时才重新计算，否则直接复用上次结果。它用来避免"每次渲染都重复做昂贵计算"（大数组过滤/排序、复杂派生数据）。上例中，如果组件因别的 state 重渲染但 `products`/`keyword` 没变，过滤就不会重跑。**判断要不要用**：这个计算是否真的慢、是否每次渲染都要跑——是才用。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 209：useMemo 稳定对象引用（配合 memo）</h3>

```jsx
const Child = React.memo(({ config }) => {
  console.log('Child 渲染');
  return <p>{config.theme}</p>;
});

function Parent({ userId }) {
  const [n, setN] = useState(0);
  // 用 useMemo 缓存对象，userId 不变时引用就不变
  const config = useMemo(() => ({ userId, theme: 'dark' }), [userId]);
  return (
    <>
      <button onClick={() => setN(n + 1)}>{n}</button>
      <Child config={config} /> {/* 点按钮时不再重渲染 */}
    </>
  );
}
```

**详解**：这是对示例 206 的修复。用 `useMemo` 把传给子组件的对象缓存起来——只要依赖 `userId` 不变，`config` 就始终是同一个引用，`Child` 的 memo 浅比较才判定"没变"，成功跳过重渲染。点计数按钮时，`Child` 不再打印。**记住这个组合**：给 memo 子组件传对象/数组 prop 时，用 useMemo 稳定它。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 210：useCallback 稳定函数引用（配合 memo）</h3>

```jsx
import { useCallback } from 'react';

const Child = React.memo(({ onClick }) => {
  console.log('Child 渲染');
  return <button onClick={onClick}>子按钮</button>;
});

function Parent() {
  const [n, setN] = useState(0);
  // useCallback 缓存函数，引用保持稳定
  const handleClick = useCallback(() => console.log('click'), []);
  return (
    <>
      <button onClick={() => setN(n + 1)}>{n}</button>
      <Child onClick={handleClick} />
    </>
  );
}
```

**详解**：`useCallback(fn, deps)` 专门缓存**函数**（等价于 `useMemo(() => fn, deps)`）。道理同示例 209：函数每次渲染都是新引用，会让 memo 子组件失效。用 `useCallback` 固定引用后，父组件计数变化不再连累 `Child`。依赖数组里要放函数内部用到的会变化的变量。**只有当这个函数会传给 memo 子组件、或作为其它 Hook 的依赖时，useCallback 才有意义**（否则纯属多余）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 211：不要滥用 memo / useMemo / useCallback</h3>

```jsx
// ❌ 没必要：加法极快，缓存的成本比计算还高
const sum = useMemo(() => a + b, [a, b]);
// ❌ 没必要：这个函数没传给 memo 子组件，也没进依赖
const onClick = useCallback(() => setOpen(true), []);
// ❌ 没必要：一个只显示静态文字的小组件包 memo，收益几乎为零
const Label = React.memo(() => <span>标题</span>);

// ✅ 直接写就好
const sum2 = a + b;
const onClick2 = () => setOpen(true);
```

**详解**：这三个 API 本身都有成本（要缓存值、比较依赖/props）。**只在真正需要时使用**：① 计算确实昂贵（useMemo）；② 值/函数要传给 memo 优化过的子组件、或作为其它 Hook 依赖（useMemo/useCallback）；③ 组件渲染重且 props 不常变（memo）。除此之外一律直接写普通变量和函数——更简单、更可读。**过早、无脑的优化是常见反模式**，先测量（示例 202）再决定。

### （D）结构性优化 —— 改变组件结构

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 212：组件拆分减少渲染范围</h3>

```jsx
function Page() {
  return (
    <div>
      <ExpensiveStaticPart /> {/* 静态的重内容，不该跟着时钟重渲染 */}
      <LiveClock />           {/* 把频繁变化的部分独立出来 */}
    </div>
  );
}

function LiveClock() {
  const [now, setNow] = useState(Date.now());
  useEffect(() => {
    const id = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(id);
  }, []);
  return <p>{new Date(now).toLocaleTimeString()}</p>;
}
```

**详解**：一个很有效但常被忽视的手段——**把频繁变化的 state 关进一个尽量小的组件里**。如果把时钟的 state 放在 `Page` 里，那么每秒 `Page` 及其所有子组件（包括昂贵的 `ExpensiveStaticPart`）都会重渲染。而把它独立成 `LiveClock`，每秒重渲染的就只有这个小组件。**state 影响的重渲染范围 = 拥有该 state 的组件及其子树**，把它下放到越小的组件，波及面越小。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 213：状态下放（把 state 移到真正用它的地方）</h3>

```jsx
// ❌ 输入框的 state 放在顶层，每次输入整个 App 都重渲染
function App() {
  const [text, setText] = useState('');
  return (
    <>
      <input value={text} onChange={e => setText(e.target.value)} />
      <HugeTree /> {/* 被无辜连累 */}
    </>
  );
}

// ✅ 把输入框和它的 state 一起下放到子组件
function SearchBox() {
  const [text, setText] = useState('');
  return <input value={text} onChange={e => setText(e.target.value)} />;
}
function App2() {
  return <><SearchBox /><HugeTree /></>;
}
```

**详解**：这是示例 212 思路的延伸。如果一个 state 只被局部使用，就**别把它放在高层组件**。上例把输入框 state 放在 `App`，导致每敲一个字整棵 `HugeTree` 都重渲染。把输入框连同其 state 一起抽到 `SearchBox`，`text` 变化就只影响 `SearchBox`，`HugeTree` 完全不受影响。**"状态应该住得离用它的地方尽量近"**——这往往比到处加 memo 更根本、更有效。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 214：内容提升（用 children 避免重渲染）</h3>

```jsx
// 当 state 必须放在高层、但有一大块内容不依赖它时，
// 把那块内容作为 children 传入，它就不会因该 state 变化而重渲染。
function Wrapper({ children }) {
  const [n, setN] = useState(0);
  return (
    <div onClick={() => setN(n + 1)}>
      <p>点击次数：{n}</p>
      {children} {/* children 是外部传入的，不会因 n 变化而重渲染 */}
    </div>
  );
}

function App() {
  return (
    <Wrapper>
      <ExpensiveTree /> {/* 在 App 里创建，不受 Wrapper 的 n 影响 */}
    </Wrapper>
  );
}
```

**详解**：一个巧妙的技巧。当某个 state 必须待在父组件里，但父组件里又有一大块内容不依赖这个 state 时，可以把那块内容**作为 `children` 从外部传入**。因为 `children`（`<ExpensiveTree />`）是在 `App` 里创建的、其引用不随 `Wrapper` 的 `n` 变化，所以 `n` 更新时 React 会复用同一个 `children` 元素，`ExpensiveTree` 不会重渲染。这叫"内容提升/传递 children"，是一种不靠 memo 就能避免重渲染的结构技巧。

### （E）加载与列表优化

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 215：代码分割 / 路由懒加载</h3>

```jsx
import { lazy, Suspense } from 'react';
import { Routes, Route } from 'react-router-dom';

const Home = lazy(() => import('./pages/Home'));
const About = lazy(() => import('./pages/About'));

function App() {
  return (
    <Suspense fallback={<p>加载中...</p>}>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/about" element={<About />} />
      </Routes>
    </Suspense>
  );
}
```

**详解**：这是**首屏加载**性能的关键优化（原理见第九章示例 190、193）。用 `lazy` + 动态 `import()` 把每个路由页面单独打包，用户访问哪个页才下载哪个页的代码，首屏只加载当前页所需的 JS，大幅减小初始包体积、加快首屏。除了按路由，也可对"体积大且非首屏必需"的组件（富文本编辑器、图表、弹窗等）做懒加载。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 216：长列表虚拟化（只渲染可见项）</h3>

```jsx
// 大列表（成千上万条）建议用 react-window / react-virtualized 库。
// 下面演示核心思想：只渲染视口内可见的那几条
function VirtualList({ items, itemHeight = 30, height = 300 }) {
  const [scrollTop, setScrollTop] = useState(0);
  const start = Math.floor(scrollTop / itemHeight);
  const count = Math.ceil(height / itemHeight);
  const visible = items.slice(start, start + count); // 只切出可见片段
  return (
    <div style={{ height, overflow: 'auto' }}
      onScroll={e => setScrollTop(e.target.scrollTop)}>
      <div style={{ height: items.length * itemHeight, position: 'relative' }}>
        {visible.map((item, i) => (
          <div key={start + i} style={{
            position: 'absolute', top: (start + i) * itemHeight, height: itemHeight,
          }}>{item}</div>
        ))}
      </div>
    </div>
  );
}
```

**详解**：渲染上万条 DOM 会严重卡顿。"**虚拟化（Virtualization）**"的思想是——不管数据有多少，**只渲染当前屏幕能看到的那十几条**，其余的不生成 DOM，滚动时动态替换。外层撑起总高度（保证滚动条正确），内层根据滚动位置切出可见片段绝对定位。实际项目**别自己造轮子**，用成熟库 `react-window`（轻量）或 `react-virtualized`（功能全）即可。这是长列表性能的终极方案。

### （F）减少更新频率与小结

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 217：防抖 / 节流减少高频更新</h3>

```jsx
import { useState, useEffect } from 'react';

function useDebounce(value, delay = 500) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(id); // 值又变了就取消上一次
  }, [value, delay]);
  return debounced;
}

function Search() {
  const [text, setText] = useState('');
  const debouncedText = useDebounce(text, 500); // 停止输入 500ms 后才更新
  useEffect(() => {
    if (debouncedText) console.log('发起搜索：', debouncedText);
  }, [debouncedText]);
  return <input value={text} onChange={e => setText(e.target.value)} />;
}
```

**详解**：有些操作触发得非常频繁（输入、滚动、resize），如果每次都执行昂贵逻辑（发请求、重算）会造成浪费和卡顿。**防抖（debounce）**：等操作"停下来"一段时间后才执行一次（适合搜索联想——停止打字才搜索）；**节流（throttle）**：固定时间间隔最多执行一次（适合滚动）。上例的 `useDebounce` 让搜索只在用户停止输入 500ms 后才发起，大幅减少无效请求。这是从"源头"减少更新的有效手段。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 218：性能优化 Checklist（小结）</h3>

```text
优化决策顺序（从上到下）：
1. 先用 React DevTools Profiler 测量，确认瓶颈真实存在
2. 结构优化优先：状态下放、组件拆分、children 传递（示例 212-214）
3. 用 React.memo 跳过无关子组件的重渲染（示例 205）
4. 配套用 useMemo/useCallback 稳定传给 memo 的对象/函数（示例 209-210）
5. 昂贵计算用 useMemo 缓存（示例 208）
6. 首屏大：代码分割 / 路由懒加载（示例 215）
7. 长列表：虚拟化（示例 216）
8. 高频事件：防抖 / 节流（示例 217）
```

**详解**：把本章串成一份可执行的清单。**关键原则**：
- **测量先行**——没有数据别动手（第 1 步）；
- **结构优化优于 memo**——能通过"状态下放、组件拆分、children"解决的，就别急着套 `memo`（第 2 步往往最有效且最简单）；
- **memo 与 useMemo/useCallback 配套使用**，单独用 memo 常因引用问题失效（第 3-4 步）；
- **别过早优化**——先把功能写对、写清晰，遇到实际性能问题再按此清单逐项排查。

记住：**可读性和正确性永远优先于性能**，只有测量证明有瓶颈时才优化。

---

## 十一、Context 与组件通信

> **组件之间怎么"说话"？** 组件不是孤岛，它们经常需要共享数据、互相通知。React 提供了几种通信方式，选哪种取决于两个组件的"距离"和数据流向：
> - **父 → 子**：用 props（最基础，第三章已讲）；
> - **子 → 父**：父传一个回调函数给子，子调用它；
> - **兄弟 → 兄弟**：把共享状态"提升"到共同的父组件；
> - **跨越很多层**：用 **Context**，避免一层层手动传 props。
>
> 本章先梳理这几种基础通信方式，再重点讲 Context 的原理、封装与陷阱，最后给出"如何选择"的小结，共 16 个示例。

### （A）组件通信的基础方式

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 219：组件通信方式总览</h3>

```jsx
// 1) 父 → 子：props
<Child name="张三" />

// 2) 子 → 父：父传回调，子调用
<Child onDone={(data) => console.log(data)} />

// 3) 兄弟 ↔ 兄弟：状态提升到共同父组件

// 4) 跨多层：Context（避免 props 层层传递）
```

**详解**：先建立全局认识。React 的数据流是"**单向的、自上而下**"的——数据主要通过 props 从父流向子。基于这个基础，衍生出四种通信模式（如上）。**判断用哪种的关键是"两个组件的关系"**：直接的父子用 props/回调；没有直接关系的兄弟靠共同父组件中转（状态提升）；相隔很多层、或"全局性"数据（主题、登录用户）则用 Context。接下来逐一详解。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 220：父传子（props）</h3>

```jsx
function Parent() {
  const user = { name: '张三', age: 20 };
  return <Child user={user} greeting="你好" />;
}

function Child({ user, greeting }) {
  return <p>{greeting}，{user.name}</p>;
}
```

**详解**：最基础的通信——父组件把数据作为 props 传给子组件（第三章已详讲）。这是单向数据流的正方向，简单可靠。任何"父组件已有、子组件要用"的数据，直接通过 props 传即可。记住 props 是**只读**的，子组件不能修改。当要传递的数据需要子组件"反向影响"父组件时，就需要下面的回调方式。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 221：子传父（回调函数）</h3>

```jsx
function Child({ onSend }) {
  return <button onClick={() => onSend('来自子组件的数据')}>发送给父组件</button>;
}

function Parent() {
  const [msg, setMsg] = useState('');
  return (
    <>
      <Child onSend={setMsg} /> {/* 把 setMsg 作为回调传下去 */}
      <p>收到：{msg}</p>
    </>
  );
}
```

**详解**：子组件不能直接改父组件的数据，但可以**调用父组件传下来的函数**，把数据"回传"上去。这里父组件把 `setMsg` 作为 `onSend` 传给子组件，子组件点击时调用 `onSend('...')`，实际执行的是父组件的 `setMsg`，从而更新父组件的状态。这就是"子 → 父"通信的本质：**数据向上流动是通过回调函数实现的**（第三章示例 52、53 也讲过）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 222：兄弟组件通信（状态提升）</h3>

```jsx
function Parent() {
  const [value, setValue] = useState(''); // 共享状态放在共同父组件
  return (
    <>
      <InputBox onChange={setValue} />  {/* 兄弟 A：修改数据 */}
      <Display value={value} />          {/* 兄弟 B：显示数据 */}
    </>
  );
}
function InputBox({ onChange }) {
  return <input onChange={e => onChange(e.target.value)} />;
}
function Display({ value }) {
  return <p>你输入了：{value}</p>;
}
```

**详解**：两个兄弟组件（`InputBox` 和 `Display`）之间没有直接的父子关系，不能互相传 props。解决办法是"**状态提升（Lifting State Up）**"——把它们都需要的共享状态，放到它们**共同的父组件**里。父组件把"修改数据的能力"（`onChange`）给一个兄弟、把"数据"（`value`）给另一个兄弟。数据在父组件汇总，两个兄弟通过父组件间接沟通。这是 React 中兄弟通信的标准做法。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 223：状态该提升到哪一层</h3>

```jsx
// 原则：把共享状态放在"需要它的所有组件的最近共同祖先"

function App() {
  const [selected, setSelected] = useState(null);
  return (
    <>
      <List onSelect={setSelected} />      {/* 需要设置 selected */}
      <Detail item={selected} />           {/* 需要读取 selected */}
    </>
  );
}
```

**详解**：状态提升有个度——**提升到"用到它的所有组件的最近共同祖先"即可，不要更高**。放太高会导致很多不相关的中间组件被卷入、重渲染范围变大（第十章示例 213 的"状态下放"讲的正是反向优化）。如果发现状态被提升得很高、要穿过很多层才能到达使用者，那就是该考虑用 Context 的信号了。

### （B）Context 基础

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 224：Context 解决什么问题（prop drilling）</h3>

```jsx
// ❌ 没有 Context：user 要一层层手动往下传，中间层被迫"中转"
function App() {
  const user = { name: '张三' };
  return <Layout user={user} />;
}
function Layout({ user })  { return <Header user={user} />; }   // 用不到却要转发
function Header({ user })  { return <Avatar user={user} />; }   // 用不到却要转发
function Avatar({ user })  { return <span>{user.name}</span>; } // 真正使用者
```

**详解**：先看问题。`user` 只有最深处的 `Avatar` 要用，但为了传到那里，中间的 `Layout`、`Header` 明明用不到，也被迫接收并转发它。这种"数据穿过一堆无关中间层"的现象叫"**逐层传递（prop drilling）**"，会让代码臃肿、难维护。层级越深越痛苦。Context 就是为了消除这种中间转发而生。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 225：创建与使用 Context（三步）</h3>

```jsx
import { createContext, useContext } from 'react';

// 第 1 步：创建 Context
const UserContext = createContext(null);

function App() {
  const user = { name: '张三' };
  // 第 2 步：用 Provider 在上层提供数据
  return (
    <UserContext.Provider value={user}>
      <Layout />
    </UserContext.Provider>
  );
}

function Layout() { return <Avatar />; } // 无需再转发 user
function Avatar() {
  // 第 3 步：深层组件用 useContext 直接读取
  const user = useContext(UserContext);
  return <span>{user.name}</span>;
}
```

**详解**：Context 的用法固定三步：① `createContext(默认值)` 创建一个 Context 对象；② 用 `<Context.Provider value={...}>` 在组件树上层"提供"数据；③ 任意深度的子孙组件用 `useContext(Context)` 直接"消费"数据。对比示例 224，中间的 `Layout` 彻底解脱——不用再接收和转发 `user` 了。`useContext` 拿到的是"组件树中最近的那个 Provider"提供的值。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 226：Context 默认值的作用</h3>

```jsx
const ThemeContext = createContext('light'); // 'light' 是默认值

function Button() {
  const theme = useContext(ThemeContext);
  return <button className={theme}>按钮</button>;
}

// 情况一：外层有 Provider → 用 Provider 的值
// <ThemeContext.Provider value="dark"><Button /></ThemeContext.Provider>  → dark

// 情况二：外层没有 Provider → 用默认值 'light'
// <Button />  → light
```

**详解**：`createContext(默认值)` 的参数是"默认值"——**只有当组件外层找不到任何对应的 Provider 时**，`useContext` 才会返回这个默认值。它的主要作用是：① 让组件在没有 Provider 包裹时也能独立工作（比如单元测试、或组件被单独使用）；② 提供文档意义，说明这个 Context 期望的数据形态。实际应用里通常都会有 Provider，默认值多作为"兜底"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 227：Context 传递"值 + 更新函数"</h3>

```jsx
const ThemeContext = createContext(null);

function App() {
  const [theme, setTheme] = useState('light');
  // value 是一个包含"数据"和"修改数据的方法"的对象
  return (
    <ThemeContext.Provider value={{ theme, setTheme }}>
      <Toolbar />
    </ThemeContext.Provider>
  );
}

function Toolbar() {
  const { theme, setTheme } = useContext(ThemeContext);
  return (
    <button onClick={() => setTheme(theme === 'light' ? 'dark' : 'light')}>
      当前：{theme}
    </button>
  );
}
```

**详解**：Context 不仅能传"数据"，还能把"修改数据的方法"一起传下去。做法是让 `value` 是一个对象 `{ theme, setTheme }`，深层组件既能读 `theme`、又能调 `setTheme` 去改它。这样任意深度的组件都能读写共享状态，实现真正的"跨层级双向通信"。这是 Context 最实用的模式，也是下面主题切换、全局状态的基础。

### （C）Context 进阶封装

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 228：可切换主题的 Context（完整封装 Provider）</h3>

```jsx
import { createContext, useContext, useState } from 'react';

const ThemeContext = createContext();

// 把状态和逻辑封装进一个专门的 Provider 组件
function ThemeProvider({ children }) {
  const [theme, setTheme] = useState('light');
  const toggle = () => setTheme(t => (t === 'light' ? 'dark' : 'light'));
  return (
    <ThemeContext.Provider value={{ theme, toggle }}>
      {children}
    </ThemeContext.Provider>
  );
}

function ThemeButton() {
  const { theme, toggle } = useContext(ThemeContext);
  return <button onClick={toggle}>当前主题：{theme}</button>;
}

// 使用：<ThemeProvider><ThemeButton /></ThemeProvider>
```

**详解**：推荐把"Context + 它的状态 + 操作逻辑"封装进一个专门的 `Provider` 组件（`ThemeProvider`），通过 `children` 包裹子树。这样 `App` 里只需写 `<ThemeProvider>...</ThemeProvider>`，所有主题相关的逻辑都收敛在一处，清晰且可复用。这是组织 Context 的最佳实践——**每个 Context 配一个自己的 Provider 组件**。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 229：封装自定义 Hook 消费 Context（含越界保护）</h3>

```jsx
function useTheme() {
  const context = useContext(ThemeContext);
  if (context === undefined) {
    throw new Error('useTheme 必须在 <ThemeProvider> 内部使用');
  }
  return context;
}

// 组件里用起来更简洁、也更安全：
function ThemeButton() {
  const { theme, toggle } = useTheme();
  return <button onClick={toggle}>{theme}</button>;
}
```

**详解**：更进一步，为每个 Context 封装一个专用的消费 Hook（`useTheme`）。好处有二：① **简洁**——组件里写 `useTheme()` 比 `useContext(ThemeContext)` 更语义化，也不用到处 import Context 对象；② **安全**——在 Hook 里检查"是否忘了用 Provider 包裹"，忘了就抛出清晰的错误提示，避免出现难以排查的 `undefined` bug。"**Provider 组件 + 消费 Hook**"是封装 Context 的黄金搭档。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 230：Context + useReducer 做全局状态</h3>

```jsx
import { createContext, useContext, useReducer } from 'react';

const StoreContext = createContext();

function reducer(state, action) {
  switch (action.type) {
    case 'inc': return { count: state.count + 1 };
    default: return state;
  }
}

function StoreProvider({ children }) {
  const [state, dispatch] = useReducer(reducer, { count: 0 });
  return (
    <StoreContext.Provider value={{ state, dispatch }}>
      {children}
    </StoreContext.Provider>
  );
}

function useStore() { return useContext(StoreContext); }

function Counter() {
  const { state, dispatch } = useStore();
  return <button onClick={() => dispatch({ type: 'inc' })}>{state.count}</button>;
}
```

**详解**：`Context`（负责"跨层级共享"）+ `useReducer`（负责"集中管理复杂状态"）是一对经典组合，能实现一个**轻量的全局状态管理**方案，不必引入 Redux。Provider 里用 `useReducer` 管理状态，把 `state` 和 `dispatch` 一起放进 Context，任意组件通过 `useStore()` 就能读状态、派发动作。中小型应用的全局状态用这个模式往往就够了。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 231：多个 Context 组合</h3>

```jsx
function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <StoreProvider>
          <Main />
        </StoreProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}
```

**详解**：一个应用通常有多个"全局关注点"——主题、登录用户、全局数据等，各用一个 Context。它们通过**嵌套 Provider** 组合起来。嵌套顺序一般不影响功能（除非某个 Provider 依赖另一个）。当嵌套层数多到影响可读性时，可以把这些 Provider 抽成一个 `AppProviders` 组件统一管理。**按关注点拆分成多个小 Context**，比塞进一个大 Context 更清晰，也利于性能（见下一例）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 232：Context 的性能陷阱与优化</h3>

```jsx
// ❌ 陷阱：value 是内联对象，Provider 每次渲染都新建它，
//    导致所有消费该 Context 的组件都重渲染
<ThemeContext.Provider value={{ theme, toggle }}>...</ThemeContext.Provider>

// ✅ 优化一：用 useMemo 稳定 value 引用
const value = useMemo(() => ({ theme, toggle }), [theme]);
<ThemeContext.Provider value={value}>...</ThemeContext.Provider>

// ✅ 优化二：把"频繁变的"和"很少变的"拆成不同 Context
```

**详解**：Context 有个重要陷阱——**当 Provider 的 `value` 变化时，所有 `useContext` 消费它的组件都会重渲染**。如果 `value` 写成内联对象 `{{ theme, toggle }}`，那么 Provider 每次渲染都生成新对象引用，即使内容没变也会触发所有消费者重渲染。优化办法：① 用 `useMemo` 缓存 `value`，只在真正的依赖变化时才更新引用；② 把"变化频率不同"的数据拆到不同 Context（比如把很少变的 `theme` 和频繁变的数据分开），减少不必要的重渲染波及面。

### （D）其它通信方式与小结

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 233：forwardRef 转发 ref（命令式通信）</h3>

```jsx
import { forwardRef, useRef, useImperativeHandle } from 'react';

const FancyInput = forwardRef((props, ref) => {
  const inputRef = useRef();
  useImperativeHandle(ref, () => ({
    focus: () => inputRef.current.focus(),
  }));
  return <input ref={inputRef} />;
});

function Parent() {
  const ref = useRef();
  return (
    <>
      <FancyInput ref={ref} />
      <button onClick={() => ref.current.focus()}>聚焦输入框</button>
    </>
  );
}
```

**详解**：前面的通信都是"声明式"（通过数据）。偶尔父组件需要**命令式地调用子组件的方法**（如让子组件的输入框聚焦、让子组件的视频播放）。这时用 `forwardRef` 把父组件的 ref 转发给子组件，再用 `useImperativeHandle` 决定暴露哪些方法（第七章示例 100 详讲过）。**这是补充手段**——命令式通信不如声明式清晰，应优先用 props/state/回调，只在确实需要触发某个动作时才用它。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 234：组件通信方式如何选择（小结）</h3>

```text
按"组件关系 + 需求"选择：
- 父 → 子           → props（示例 220）
- 子 → 父           → 回调函数（示例 221）
- 兄弟 ↔ 兄弟        → 状态提升到共同父组件（示例 222）
- 跨越多层 / 全局    → Context（示例 225）
- 父命令式调用子方法 → forwardRef + useImperativeHandle（示例 233）
- 大型复杂全局状态   → 状态管理库（Redux / Zustand / Jotai 等）
```

**详解**：把本章串成一张决策表。**核心原则**：
- **优先用最简单、最局部的方式**——能用 props/回调解决的，就别上 Context；
- **Context 适合"全局性、低频变化"的数据**（主题、当前用户、语言），不要拿它当"什么都往里塞"的万能状态库；
- **当状态非常多、更新逻辑复杂、需要中间件/时间旅行调试**时，才考虑引入专门的状态管理库（Zustand 轻量、Redux 生态成熟）。

记住：**通信方式没有绝对优劣，合适的才是最好的**。从最简单的方案开始，遇到痛点（如严重的 prop drilling）再升级。

---

## 十二、进阶与实战

> 本章把前面学到的知识（组件、state、事件、Hooks、条件/列表、表单、Context 等）综合起来，做成一个个**可直接使用的实战小组件和自定义 Hook**。它们由简单到复杂，覆盖真实项目中最常见的需求。
>
> 建议对照前面章节来看——你会发现每个实战都是若干基础知识点的组合。共 17 个示例。

### （A）常用交互组件

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 235：Tab 切换组件</h3>

```jsx
import { useState } from 'react';

function Tabs() {
  const [active, setActive] = useState(0);
  const tabs = ['介绍', '参数', '评价'];
  const contents = ['这是介绍内容', '这是参数内容', '这是评价内容'];
  return (
    <div>
      <div>
        {tabs.map((t, i) => (
          <button
            key={i}
            onClick={() => setActive(i)}
            style={{ fontWeight: active === i ? 'bold' : 'normal' }}
          >
            {t}
          </button>
        ))}
      </div>
      <div>{contents[active]}</div>
    </div>
  );
}
```

**详解**：Tab 切换是最基础的交互组件。核心是用一个 state `active` 记录"当前选中第几个"。点击某个按钮就 `setActive(i)`，再根据 `active` 决定按钮样式（高亮当前项）和显示哪块内容。这里综合了 state（示例 58）、列表渲染（示例 90）、事件传参（示例 77）、条件样式（示例 60）。理解它就掌握了"用一个 state 驱动多个 UI"的通用套路。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 236：折叠面板（Accordion）</h3>

```jsx
function Accordion({ items }) {
  const [openId, setOpenId] = useState(null);
  const toggle = (id) => setOpenId(prev => (prev === id ? null : id));
  return (
    <div>
      {items.map(item => (
        <div key={item.id}>
          <button onClick={() => toggle(item.id)}>
            {item.title} {openId === item.id ? '▲' : '▼'}
          </button>
          {openId === item.id && <div>{item.content}</div>}
        </div>
      ))}
    </div>
  );
}
```

**详解**：折叠面板（手风琴）实现"同时只展开一项"的效果。用一个 state `openId` 记录"当前展开的是哪一项"。点击时 `toggle`：如果点的正是已展开项就收起（设为 `null`），否则展开它。每一项通过 `openId === item.id` 判断是否显示内容和箭头方向。这是 Tab 思路的变体——不同点在于"可以全部收起"（`null` 状态）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 237：倒计时组件</h3>

```jsx
import { useState, useEffect } from 'react';

function Countdown({ seconds = 60 }) {
  const [left, setLeft] = useState(seconds);
  useEffect(() => {
    if (left <= 0) return;                 // 到 0 就停止
    const id = setTimeout(() => setLeft(left - 1), 1000);
    return () => clearTimeout(id);         // 清理，防止残留定时器
  }, [left]);
  return <p>{left > 0 ? `剩余 ${left} 秒` : '时间到！'}</p>;
}
```

**详解**：倒计时组件展示了 `useEffect` + 定时器的经典用法。这里用 `setTimeout` 而非 `setInterval`——每次 `left` 变化都重新建一个 1 秒后减 1 的定时器，`left` 到 0 时直接 `return` 不再设新定时器。清理函数 `clearTimeout` 确保组件卸载或重渲染时不留下野定时器（第七章示例 78）。常用于短信验证码"重新发送"倒计时、活动截止等。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 238：模态弹窗（Portal）</h3>

```jsx
import { useState } from 'react';
import { createPortal } from 'react-dom';

function Modal({ children, onClose }) {
  return createPortal(
    <div className="overlay" onClick={onClose}>
      <div className="modal" onClick={e => e.stopPropagation()}>
        {children}
        <button onClick={onClose}>关闭</button>
      </div>
    </div>,
    document.body // 渲染到 body，而非当前组件所在的 DOM 位置
  );
}

function App() {
  const [open, setOpen] = useState(false);
  return (
    <>
      <button onClick={() => setOpen(true)}>打开弹窗</button>
      {open && <Modal onClose={() => setOpen(false)}>这是弹窗内容</Modal>}
    </>
  );
}
```

**详解**：弹窗要覆盖整个页面，但组件可能嵌套在很深的、带 `overflow:hidden` 或定位的容器里，直接渲染会被裁剪。`createPortal(内容, 目标节点)` 能把内容**渲染到 DOM 树的另一个位置**（这里是 `document.body`），从而摆脱父容器的样式限制，同时逻辑上它仍是当前组件的子节点（事件照常冒泡）。点遮罩层关闭、点内容区用 `stopPropagation` 阻止关闭（示例 79）。这是弹窗、下拉菜单、提示框的标准实现。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 239：Toast 提示（自动消失）</h3>

```jsx
function useToast() {
  const [msg, setMsg] = useState('');
  const show = (text, duration = 2000) => {
    setMsg(text);
    setTimeout(() => setMsg(''), duration); // 到时自动清空
  };
  const toast = msg ? <div className="toast">{msg}</div> : null;
  return { toast, show };
}

function App() {
  const { toast, show } = useToast();
  return (
    <>
      <button onClick={() => show('操作成功！')}>触发提示</button>
      {toast}
    </>
  );
}
```

**详解**：Toast 是那种"弹出一句提示、几秒后自动消失"的轻提示。这里封装成自定义 Hook `useToast`——`show(text)` 设置消息并启动一个定时器到时清空，`toast` 是要渲染的元素（有消息才显示）。组件里解构出 `{ toast, show }`，把 `toast` 放到界面上、需要时调 `show`。这综合了 state、定时器、条件渲染，并体现了"把 UI + 逻辑打包成 Hook"的思想。

### （B）表单与校验实战

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 240：受控 + 实时校验的表单</h3>

```jsx
function EmailForm() {
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const submit = (e) => {
    e.preventDefault();
    if (!/^\S+@\S+\.\S+$/.test(email)) {
      setError('邮箱格式不正确');
      return;
    }
    setError('');
    alert('提交成功：' + email);
  };
  return (
    <form onSubmit={submit}>
      <input value={email} onChange={e => setEmail(e.target.value)} placeholder="邮箱" />
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <button type="submit">提交</button>
    </form>
  );
}
```

**详解**：这是表单校验的最小完整例子（第六章有更完整的版本）。要点：受控输入（`value` + `onChange`）、提交时 `preventDefault` 阻止刷新、用正则校验、把错误信息存进 `error` state 并条件渲染。这个"输入→校验→显示错误→提交"的闭环是所有表单的骨架。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 241：登录表单（含提交中状态）</h3>

```jsx
function LoginForm() {
  const [form, setForm] = useState({ username: '', password: '' });
  const [loading, setLoading] = useState(false);
  const update = (f) => (e) => setForm(p => ({ ...p, [f]: e.target.value }));
  const submit = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      await fakeLogin(form);
      alert('登录成功');
    } finally {
      setLoading(false);
    }
  };
  return (
    <form onSubmit={submit}>
      <input value={form.username} onChange={update('username')} placeholder="用户名" disabled={loading} />
      <input type="password" value={form.password} onChange={update('password')} disabled={loading} />
      <button type="submit" disabled={loading}>{loading ? '登录中...' : '登录'}</button>
    </form>
  );
}
function fakeLogin(data) { return new Promise(r => setTimeout(r, 1000)); }
```

**详解**：真实的登录表单要处理**异步提交**。用 `loading` state 标记"正在登录"，提交期间禁用输入框和按钮、按钮文字改成"登录中..."，防止用户重复点击。`try/finally` 保证无论成功失败最后都复位 `loading`（第六章示例 132）。综合了对象 state、多字段更新、异步事件处理。

### （C）数据请求实战

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 242：数据请求三态（loading / error / data）</h3>

```jsx
import { useState, useEffect } from 'react';

function UserList() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    let ignore = false;
    setLoading(true);
    fetch('/api/users')
      .then(r => r.json())
      .then(d => { if (!ignore) { setData(d); setLoading(false); } })
      .catch(e => { if (!ignore) { setError(e); setLoading(false); } });
    return () => { ignore = true; };
  }, []);

  if (loading) return <p>加载中...</p>;
  if (error)   return <p>加载失败</p>;
  return <ul>{data.map(u => <li key={u.id}>{u.name}</li>)}</ul>;
}
```

**详解**：几乎所有涉及网络请求的组件都要处理三种状态：加载中、出错、成功。这里用三个 state 分别记录，`useEffect` 里发请求（带 `ignore` 标志防竞态，第七章示例 81），渲染时用连续 `if` 逐一处理异常、最后才渲染正常内容（第五章示例 48）。这是手写数据请求的标准骨架。**实际项目更推荐用 React Query / SWR**——它们把这套逻辑连同缓存、重试封装好了。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 243：防抖搜索（自定义 Hook）</h3>

```jsx
import { useState, useEffect } from 'react';

function useDebounce(value, delay = 500) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(id); // 值又变了就取消上一次
  }, [value, delay]);
  return debounced;
}

function Search() {
  const [text, setText] = useState('');
  const debouncedText = useDebounce(text, 500);
  useEffect(() => {
    if (debouncedText) console.log('发起搜索：', debouncedText);
  }, [debouncedText]);
  return <input value={text} onChange={e => setText(e.target.value)} placeholder="搜索" />;
}
```

**详解**：搜索框如果每敲一个字就发一次请求，既浪费又可能触发大量无效请求。防抖让请求"等用户停止输入 500ms 后"才发一次。`useDebounce` 把这个逻辑封装成 Hook：`text` 每次变化都重设定时器，只有 500ms 内不再变化，`debouncedText` 才更新，进而触发搜索的 `useEffect`（第十章示例 217）。输入框绑定即时的 `text`（打字流畅），搜索用延迟的 `debouncedText`。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 244：分页数据加载</h3>

```jsx
function PagedList() {
  const [page, setPage] = useState(1);
  const [data, setData] = useState([]);
  useEffect(() => {
    fetch(`/api/list?page=${page}`).then(r => r.json()).then(setData);
  }, [page]); // page 变化就重新请求
  return (
    <div>
      <ul>{data.map(d => <li key={d.id}>{d.name}</li>)}</ul>
      <button disabled={page === 1} onClick={() => setPage(p => p - 1)}>上一页</button>
      <span>第 {page} 页</span>
      <button onClick={() => setPage(p => p + 1)}>下一页</button>
    </div>
  );
}
```

**详解**：分页的关键是把"当前页码 `page`"作为 `useEffect` 的依赖——`page` 一变就自动重新请求对应页的数据。翻页按钮用函数式更新 `setPage(p => p ± 1)`，第一页时禁用"上一页"。这个"某个参数变化 → useEffect 重新请求"的模式适用于各种筛选、排序、翻页场景。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 245：加载更多 / 无限滚动</h3>

```jsx
function InfiniteList() {
  const [items, setItems] = useState([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);

  const loadMore = async () => {
    setLoading(true);
    const res = await fetch(`/api/list?page=${page}`).then(r => r.json());
    setItems(prev => [...prev, ...res]); // 追加而非替换
    setPage(p => p + 1);
    setLoading(false);
  };

  useEffect(() => { loadMore(); }, []); // 首次自动加载第一页

  return (
    <div>
      <ul>{items.map(i => <li key={i.id}>{i.name}</li>)}</ul>
      <button onClick={loadMore} disabled={loading}>
        {loading ? '加载中...' : '加载更多'}
      </button>
    </div>
  );
}
```

**详解**：与分页"替换数据"不同，"加载更多"是**追加数据**——用 `setItems(prev => [...prev, ...新数据])` 把新一页拼到已有列表后面。每加载一页 `page` 加 1。配合滚动监听（滚到底部自动调用 `loadMore`）就是"无限滚动"。这里综合了数组不可变更新（示例 71）、异步请求、loading 状态。

### （D）实用自定义 Hook

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 246：useToggle（开关状态）</h3>

```jsx
import { useState, useCallback } from 'react';

function useToggle(initial = false) {
  const [on, setOn] = useState(initial);
  const toggle = useCallback(() => setOn(o => !o), []);
  return [on, toggle];
}

function App() {
  const [visible, toggleVisible] = useToggle();
  return (
    <>
      <button onClick={toggleVisible}>{visible ? '隐藏' : '显示'}</button>
      {visible && <p>可切换的内容</p>}
    </>
  );
}
```

**详解**：`useToggle` 把"布尔开关"这一超高频逻辑封装成一行可复用的 Hook，返回 `[当前值, 切换函数]`，用法类似 `useState`。任何"显示/隐藏、开/关、折叠/展开"的场景都能用它，避免重复写 `setX(x => !x)`。这体现了自定义 Hook 的价值：**把常用逻辑抽象成可复用的能力**（第七章示例 101）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 247：useLocalStorage（持久化状态）</h3>

```jsx
import { useState, useEffect } from 'react';

function useLocalStorage(key, initial) {
  const [value, setValue] = useState(() => {
    const stored = localStorage.getItem(key);
    return stored ? JSON.parse(stored) : initial; // 惰性初始化，只读一次
  });
  useEffect(() => {
    localStorage.setItem(key, JSON.stringify(value)); // value 变了就写回
  }, [key, value]);
  return [value, setValue];
}

function Settings() {
  const [name, setName] = useLocalStorage('username', '');
  return <input value={name} onChange={e => setName(e.target.value)} />;
}
```

**详解**：这个 Hook 让一段状态自动与 `localStorage` 同步，刷新页面后仍能恢复。用法和 `useState` 完全一致（返回 `[值, 设置函数]`），毫无学习负担。两个要点：初始值用**惰性初始化**（传函数，只读一次 localStorage）；用 `useEffect` 监听 `value` 变化并写回（第七章示例 103）。它是下一个示例（主题持久化）的基础。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 248：主题切换 + localStorage 持久化</h3>

```jsx
function ThemedApp() {
  const [dark, setDark] = useLocalStorage('dark', false); // 复用上一个 Hook
  useEffect(() => {
    document.body.className = dark ? 'dark' : 'light'; // 同步到 body 的 class
  }, [dark]);
  return (
    <button onClick={() => setDark(d => !d)}>
      切换到{dark ? '亮色' : '暗色'}模式
    </button>
  );
}
```

**详解**：把 `useLocalStorage`（持久化）和 `useEffect`（同步副作用）组合起来，实现"记住用户主题偏好"的功能——刷新后主题不丢失。`dark` 状态存进 localStorage，变化时通过 `useEffect` 把对应的 class 加到 `<body>` 上（配合 CSS 生效）。这展示了**自定义 Hook 的组合复用**：`ThemedApp` 直接站在 `useLocalStorage` 的肩膀上。

### （E）健壮性与综合大实战

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 249：错误边界（Error Boundary）</h3>

```jsx
import { Component } from 'react';

// 错误边界目前仍需用"类组件"实现（Hooks 暂无等价写法）
class ErrorBoundary extends Component {
  state = { hasError: false };
  static getDerivedStateFromError() {
    return { hasError: true }; // 出错时更新 state，触发降级 UI
  }
  componentDidCatch(error, info) {
    console.error('捕获到错误：', error, info); // 上报错误
  }
  render() {
    if (this.state.hasError) return <h2>页面出错了，请刷新重试</h2>;
    return this.props.children;
  }
}

function App() {
  return (
    <ErrorBoundary>
      <MaybeBuggyComponent />
    </ErrorBoundary>
  );
}
```

**详解**：如果某个组件渲染时抛出错误，默认会导致**整个应用白屏崩溃**。"错误边界"能捕获其子树的渲染错误，显示一个降级 UI（而非白屏），并把错误上报。它是 React 里**极少数必须用类组件写**的东西——`getDerivedStateFromError` 负责出错时切换到降级状态，`componentDidCatch` 负责记录/上报错误。把它包在应用外层或关键区块外，能大大提升健壮性（也用于捕获懒加载失败，第九章示例 195）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 250：错误边界 + Suspense 组合</h3>

```jsx
import { Suspense, lazy } from 'react';

const Chart = lazy(() => import('./Chart'));

function Dashboard() {
  return (
    <ErrorBoundary>                       {/* 管"出错"：加载失败/渲染报错 */}
      <Suspense fallback={<p>加载中...</p>}> {/* 管"等待"：加载中占位 */}
        <Chart />
      </Suspense>
    </ErrorBoundary>
  );
}
```

**详解**：这是健壮的异步 UI 的标准三层结构（第九章示例 195）：`ErrorBoundary` 负责"出错"（加载失败、渲染异常时显示降级 UI），`Suspense` 负责"等待"（加载中显示占位），最内层是真正的懒加载/异步组件。两者职责互补——Suspense 不处理错误，ErrorBoundary 不处理加载中。记住这个组合，就能优雅应对异步组件的所有状态。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 251：综合大实战——完整的 Todo 应用</h3>

```jsx
import { useState, useEffect } from 'react';

// 复用示例 247 的持久化 Hook
function useLocalStorage(key, initial) {
  const [value, setValue] = useState(() => {
    const s = localStorage.getItem(key);
    return s ? JSON.parse(s) : initial;
  });
  useEffect(() => { localStorage.setItem(key, JSON.stringify(value)); }, [key, value]);
  return [value, setValue];
}

function TodoApp() {
  const [todos, setTodos] = useLocalStorage('todos', []); // 持久化的任务列表
  const [text, setText] = useState('');
  const [filter, setFilter] = useState('all'); // all | active | done

  const add = () => {
    if (!text.trim()) return;
    setTodos([...todos, { id: Date.now(), text, done: false }]); // 增
    setText('');
  };
  const toggle = (id) =>
    setTodos(todos.map(t => t.id === id ? { ...t, done: !t.done } : t)); // 改
  const remove = (id) => setTodos(todos.filter(t => t.id !== id));       // 删

  // 根据过滤条件派生要显示的列表
  const visible = todos.filter(t =>
    filter === 'all' ? true : filter === 'done' ? t.done : !t.done
  );

  return (
    <div>
      <input
        value={text}
        onChange={e => setText(e.target.value)}
        onKeyDown={e => e.key === 'Enter' && add()}
        placeholder="输入任务，回车添加"
      />
      <button onClick={add}>添加</button>

      <div>
        {['all', 'active', 'done'].map(f => (
          <button key={f} onClick={() => setFilter(f)}
            style={{ fontWeight: filter === f ? 'bold' : 'normal' }}>
            {f === 'all' ? '全部' : f === 'active' ? '未完成' : '已完成'}
          </button>
        ))}
      </div>

      <ul>
        {visible.map(t => (
          <li key={t.id}>
            <span onClick={() => toggle(t.id)}
              style={{ textDecoration: t.done ? 'line-through' : 'none', cursor: 'pointer' }}>
              {t.text}
            </span>
            <button onClick={() => remove(t.id)}>×</button>
          </li>
        ))}
      </ul>

      <p>共 {todos.length} 项，完成 {todos.filter(t => t.done).length} 项</p>
    </div>
  );
}
```

**详解**：这是全书的"毕业设计"，把众多知识点融为一体：
1. **持久化状态**：任务列表用 `useLocalStorage`，刷新不丢（示例 247）；
2. **增删改**：数组不可变更新三板斧 `[...]` / `map` / `filter`（示例 71-73）；
3. **受控输入 + 回车添加**：`value`+`onChange`、`onKeyDown` 判断 Enter（示例 80、82）；
4. **过滤（派生数据）**：用 `filter` state + 计算 `visible`，而不是再存一个列表——**能从现有 state 算出来的，就不要单独存**；
5. **条件样式与统计**：完成项划线、底部统计。

如果你能独立写出这个 Todo 应用并理解每一行，说明你已经扎实掌握了 React 18 的核心。恭喜！接下来就可以去学 React Router（路由）、状态管理库、TypeScript 等进阶主题了。

---

## 十三、React Router 路由

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

**详解**：使用 React Router 的第一步，是在应用最外层包一个 **`<BrowserRouter>`**。它负责监听浏览器地址栏的变化、并把"当前 URL"提供给内部所有组件——就像第十一章的 Context Provider 一样。`BrowserRouter` 使用 HTML5 的 History API，URL 形如 `/about`（干净、无 `#`）。包好之后，内部才能使用 `<Routes>`、`<Link>`、`useNavigate` 等路由功能。

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

**详解**：很多页面需要"登录后才能访问"。做法是封装一个 `RequireAuth` 组件当"守卫"：它检查登录状态，已登录就渲染 `children`（真正的页面），未登录就用 `<Navigate>` 重定向到登录页。把需要保护的路由用它包起来即可。这是"路由守卫/权限控制"的常见实现思路，实际项目里 `isLoggedIn` 通常来自 Context 里的全局登录状态（第十一章）。

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

**详解**：路由是做"代码分割"最理想的边界——每个页面单独打包，用户访问哪个页才下载哪个页的代码，大幅减小首屏体积（第九章示例 193、第十章示例 215 讲过原理）。用 `React.lazy` 包裹每个页面组件，再用一个 `<Suspense>` 包住 `<Routes>` 提供加载中占位。这是中大型 React 应用几乎必备的性能优化。

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

## 十四、数据请求（React Query）

> **为什么需要 React Query？** 回顾第十二章示例 242——用 `useEffect` + 三个 state 手写数据请求，要处理加载态、错误态、竞态，还没有缓存、重试、后台刷新。每个组件都重复这套样板，很繁琐。**React Query（现名 TanStack Query）** 专门管理"**服务端状态**"（来自后端、你不完全掌控、会过期的数据），把这些都封装好了。
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

**详解**：这就是 React Query 的核心 `useQuery`，对比第十二章示例 242 的手写版本，代码大幅简化。它接收一个对象：
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

## 附录：常见易错点与学习建议

> 这份速查表汇总了初学 React 时最容易踩的坑，按主题分类。遇到"改了数据界面不动""effect 反复执行""列表行为异常"等问题时，先回来对照检查。

### 一、State 相关

1. **不要直接修改 state**：对象/数组要用展开语法生成**新引用**（`{...obj}`、`[...arr]`），否则 React 检测不到变化、不会重渲染（示例 66）。
2. **state 更新不是立即生效的**：`setCount(x)` 之后马上读 `count` 还是旧值。连续基于旧值更新要用**函数式更新** `setCount(c => c + 1)`（示例 67、68）。
3. **能派生的数据不要存进 state**：能从现有 state/props 算出来的值（如过滤后的列表、总数），直接在渲染时计算，不要单独用一个 state 存（示例 251）。
4. **对象/数组 state 更新要逐层展开**：修改嵌套字段时每一层都要复制（示例 70）。

### 二、useEffect 相关

5. **依赖数组要写全**：effect 里用到的每个组件内变量都应列进依赖，漏写会读到过期的闭包值（示例 83）。
6. **一定要清理副作用**：定时器、事件监听、订阅务必在 effect 的返回函数里清理，否则内存泄漏（示例 78、79）。
7. **警惕闭包陷阱**：空依赖的 effect 里若直接用 state，读到的永远是初始值；改用函数式更新（示例 82）。
8. **请求要防竞态**：快速变化的依赖会发多个请求，用 `ignore` 标志避免旧响应覆盖新数据（示例 81）。

### 三、渲染 / JSX 相关

9. **列表 key 要稳定唯一**：尽量用数据自带的 `id`，避免用数组下标（示例 53）。
10. **`&&` 短路要小心数字 0**：`0 && <X/>` 会渲染出"0"，左边要用明确的布尔值 `arr.length > 0 && ...`（示例 39）。
11. **`class` 要写成 `className`，`for` 写成 `htmlFor`**（示例 14）。
12. **事件绑定传函数引用，不要加括号调用**：`onClick={fn}` 对，`onClick={fn()}` 会在渲染时立即执行（示例 75）。
13. **组件名必须大写开头**，否则被当成原生 HTML 标签（示例 31）。

### 四、性能相关

14. **别过早优化**：先用 React DevTools Profiler 测量，确认瓶颈再动手（示例 202）。
15. **memo 常因引用失效**：给 memo 子组件传对象/函数时，要用 `useMemo`/`useCallback` 稳定引用（示例 206、209）。
16. **优先结构优化**：状态下放、组件拆分、children 传递，往往比到处加 memo 更有效（示例 212-214）。
17. **Context 的 value 用 useMemo 稳定**，否则消费它的组件会频繁重渲染（示例 232）。

### 五、React 18 相关

18. **用 `createRoot` 取代 `ReactDOM.render`**：这是启用并发特性的标准入口（示例 2）。
19. **StrictMode 下开发环境副作用执行两次**：这是有意的，用来暴露不干净的副作用；生产环境只执行一次，不要为此关掉它（示例 4）。
20. **区分紧急与非紧急更新**：昂贵的派生更新用 `startTransition`/`useDeferredValue`，保证输入等紧急交互流畅（第八章）。

### 六、推荐的学习路径

- **打好基础**：JSX → 组件与 Props → State 与事件 → 条件/列表渲染（第二~五章）。
- **掌握 Hooks**：useState、useEffect 是重中之重，再逐步掌握 useRef、useContext、useReducer、useMemo/useCallback（第七章）。
- **进阶**：表单、Context、性能优化、错误边界（第六、十、十一、十二章）。
- **React 18 特性**：并发、useTransition、Suspense（第八、九章），用到再深入。
- **常用生态库**：React Router 路由（第十三章）、React Query 数据请求（第十四章）——真实项目几乎必用，本文档已覆盖。
- **再下一步**：状态管理（Zustand / Redux）、TypeScript + React、测试（Vitest / React Testing Library）、以及 Next.js 等全栈框架。

---

至此全文共 283 个示例，覆盖 React 18 从入门到进阶实战的核心内容。**最好的学习方式是边读边动手敲**——把示例改一改、跑一跑，遇到报错去查、去想为什么，进步最快。祝你学得顺利！
