# React 18 详细使用说明（100+ 示例）

> 本文档面向已了解 JavaScript / ES6 的开发者，通过 100 多个由浅入深的小示例，系统讲解 React 18 的用法。
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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 116：受控输入框</h3>

```jsx
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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 117：受控 textarea</h3>

```jsx
function Comment() {
  const [text, setText] = useState('');
  return <textarea value={text} onChange={e => setText(e.target.value)} />;
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 118：受控 select 下拉框</h3>

```jsx
function CitySelect() {
  const [city, setCity] = useState('bj');
  return (
    <select value={city} onChange={e => setCity(e.target.value)}>
      <option value="bj">北京</option>
      <option value="sh">上海</option>
    </select>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 119：复选框（checkbox）</h3>

```jsx
function Agree() {
  const [checked, setChecked] = useState(false);
  return (
    <label>
      <input type="checkbox" checked={checked}
        onChange={e => setChecked(e.target.checked)} />
      同意协议
    </label>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 120：单选按钮（radio）</h3>

```jsx
function Gender() {
  const [gender, setGender] = useState('male');
  return (
    <>
      <label><input type="radio" value="male"
        checked={gender === 'male'} onChange={e => setGender(e.target.value)} />男</label>
      <label><input type="radio" value="female"
        checked={gender === 'female'} onChange={e => setGender(e.target.value)} />女</label>
    </>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 121：一个函数处理多个字段</h3>

```jsx
function Form() {
  const [form, setForm] = useState({ name: '', email: '' });
  const handle = (e) => {
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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 122：表单提交</h3>

```jsx
function LoginForm() {
  const [user, setUser] = useState('');
  const submit = (e) => {
    e.preventDefault(); // 阻止页面刷新
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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 123：非受控组件（用 ref 读取值）</h3>

```jsx
import { useRef } from 'react';

function UncontrolledForm() {
  const inputRef = useRef(null);
  const submit = () => alert(inputRef.current.value);
  return (
    <>
      <input ref={inputRef} defaultValue="默认值" />
      <button onClick={submit}>读取</button>
    </>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 124：文件上传</h3>

```jsx
function FileUpload() {
  const onChange = (e) => {
    const file = e.target.files[0];
    if (file) console.log('文件名：', file.name);
  };
  return <input type="file" onChange={onChange} />;
}
```


---

## 七、核心 Hooks

> Hooks 是 React 16.8 引入、在函数组件里"钩入" React 能力（状态、生命周期等）的函数。本章按 `useEffect → useRef → useContext → useReducer → useMemo/useCallback → useLayoutEffect/useImperativeHandle → 自定义 Hook` 的顺序，由浅入深讲 30 个示例。
>
> **两条铁律先记住**：① Hooks 只能在组件函数或自定义 Hook 的**顶层**调用，不能写在 `if`、循环、嵌套函数里；② Hook 名必须以 `use` 开头。这保证 React 每次渲染都能按相同顺序识别每个 Hook。

### （A）useEffect —— 处理副作用

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 125：useEffect 最简单的样子（每次渲染后执行）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 126：空依赖数组（只在挂载时执行一次）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 127：指定依赖（依赖变化时才执行）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 128：依赖数组的三种形态对比（重点总结）</h3>

```jsx
useEffect(() => { /* ... */ });          // ① 不传：每次渲染后都执行
useEffect(() => { /* ... */ }, []);      // ② 空数组：只在挂载后执行一次
useEffect(() => { /* ... */ }, [a, b]);  // ③ 有依赖：a 或 b 变化时执行
```

**详解**：这是理解 `useEffect` 的关键。记住这张对照表：
- **不传第二个参数** → 每次渲染后都执行（很少用，通常是没想清楚）；
- **`[]`** → 仅挂载时执行一次，卸载时执行清理；
- **`[a, b]`** → 挂载时执行，之后每当 `a` 或 `b` 变化时再执行。

选哪种，取决于你的副作用"依赖了哪些数据"。原则是：**effect 内部用到的每一个组件内变量（props、state、函数），都应出现在依赖数组里**（见示例 134）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 129：清理函数（以定时器为例）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 130：清理事件监听</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 131：在 useEffect 中请求数据（基础版）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 132：请求数据的竞态问题与 ignore 标志</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 133：闭包陷阱——读到"过期"的 state</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 134：不要漏写依赖（并理解为什么）</h3>

```jsx
function Search({ query, onResult }) {
  useEffect(() => {
    fetchData(query).then(onResult);
    // 依赖数组应包含 effect 内用到的所有外部变量
  }, [query, onResult]);
  return null;
}
```

**详解**：ESLint 的 `react-hooks/exhaustive-deps` 规则会提醒你补全依赖。漏写依赖的后果是：effect 内部读到的是某次渲染时"冻结"的旧值，行为难以预测。原则是**诚实地列出 effect 用到的每一个组件内变量**。如果某个依赖变化太频繁导致 effect 反复执行，正确做法不是删依赖，而是用 `useCallback`/`useMemo` 稳定它，或用函数式更新绕开（如示例 133）。

### （B）useRef —— 引用 DOM 与保存可变值

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 135：useRef 引用 DOM 元素</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 136：useRef 保存可变值（修改它不会触发渲染）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 137：useRef vs useState 的区别（对照理解）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 138：useRef 保存上一次的值（自定义 usePrevious）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 139：useContext 基础用法</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 140：useContext 解决"逐层传递 props"（prop drilling）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 141：useReducer 计数器（入门）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 142：useReducer 管理复杂表单</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 143：useReducer 管理列表</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 144：useReducer vs useState 如何选择</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 145：useMemo 缓存昂贵的计算结果</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 146：useMemo 稳定对象引用（配合 React.memo）</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 147：useCallback 缓存函数</h3>

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

**详解**：`useCallback(fn, deps)` 相当于 `useMemo(() => fn, deps)`，专门用来缓存"函数"。道理同示例 146：函数每次渲染都是新引用，会让接收它的 `memo` 子组件失效。用 `useCallback` 固定函数引用后，父组件计数变化不再连累 `Child` 重渲染。依赖数组里要放函数内部用到的会变化的变量。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 148：不要滥用 useMemo / useCallback</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 149：useLayoutEffect 同步测量避免闪烁</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 150：useLayoutEffect 与 useEffect 的区别</h3>

```jsx
useEffect(() => { /* 绘制后异步执行，不阻塞渲染，99% 情况用它 */ });
useLayoutEffect(() => { /* 绘制前同步执行，会阻塞渲染，仅测量/定位时用 */ });
```

**详解**：一句话记忆——**默认永远用 `useEffect`**。它在浏览器绘制后异步执行，不会拖慢首屏。只有当你遇到"用了 useEffect 会出现明显闪烁/跳动"（因为你需要在绘制前读布局并改 DOM）时，才换成 `useLayoutEffect`。两者 API 完全一样，区别只在执行时机与是否阻塞绘制。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 151：useImperativeHandle 向父组件暴露方法</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 152：自定义 Hook：useToggle</h3>

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

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 153：自定义 Hook：useFetch（含加载态与竞态处理）</h3>

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

**详解**：这个 `useFetch` 把"请求数据"这套通用逻辑——加载态、错误态、竞态处理（示例 132 的 `ignore` 标志）——全部封装。任何组件只要 `const { data, loading, error } = useFetch(url)` 就能拿到完整的请求状态，组件本身只关心怎么渲染。这正是自定义 Hook 的威力：把重复的副作用逻辑抽象成一个可复用的"能力"。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 154：自定义 Hook：useLocalStorage（与浏览器存储同步）</h3>

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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 155：useId 生成唯一 id</h3>

```jsx
import { useId } from 'react';

function Field() {
  const id = useId(); // 保证服务端和客户端一致，适合无障碍 label
  return (
    <div>
      <label htmlFor={id}>邮箱</label>
      <input id={id} type="email" />
    </div>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 156：useId 生成多个相关 id</h3>

```jsx
function Form() {
  const id = useId();
  return (
    <>
      <label htmlFor={`${id}-name`}>姓名</label>
      <input id={`${id}-name`} />
      <label htmlFor={`${id}-age`}>年龄</label>
      <input id={`${id}-age`} />
    </>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 157：useTransition 标记非紧急更新</h3>

```jsx
import { useState, useTransition } from 'react';

function SearchList({ allItems }) {
  const [query, setQuery] = useState('');
  const [list, setList] = useState(allItems);
  const [isPending, startTransition] = useTransition();

  const onChange = (e) => {
    const value = e.target.value;
    setQuery(value); // 紧急更新：输入框立即响应
    startTransition(() => {
      // 非紧急更新：过滤大列表，可被打断
      setList(allItems.filter(i => i.includes(value)));
    });
  };

  return (
    <>
      <input value={query} onChange={onChange} />
      {isPending && <span>更新中...</span>}
      <ul>{list.map((i, k) => <li key={k}>{i}</li>)}</ul>
    </>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 158：useDeferredValue 延迟值</h3>

```jsx
import { useState, useDeferredValue, useMemo } from 'react';

function Search({ allItems }) {
  const [text, setText] = useState('');
  const deferredText = useDeferredValue(text); // 延迟版本

  const results = useMemo(
    () => allItems.filter(i => i.includes(deferredText)),
    [allItems, deferredText]
  );

  return (
    <>
      <input value={text} onChange={e => setText(e.target.value)} />
      <ul>{results.map((r, k) => <li key={k}>{r}</li>)}</ul>
    </>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 159：startTransition（非 Hook 版本）</h3>

```jsx
import { startTransition } from 'react';

function TabButton({ onSelect }) {
  const click = () => {
    startTransition(() => {
      onSelect(); // 切换标签这类耗时更新标记为过渡
    });
  };
  return <button onClick={click}>切换</button>;
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 160：useSyncExternalStore 订阅外部数据源</h3>

```jsx
import { useSyncExternalStore } from 'react';

// 订阅浏览器在线状态
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
    () => navigator.onLine,      // 客户端快照
    () => true                    // 服务端快照
  );
}

function StatusBar() {
  const isOnline = useOnlineStatus();
  return <p>{isOnline ? '✅ 在线' : '❌ 离线'}</p>;
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 161：useSyncExternalStore 订阅自定义 store</h3>

```jsx
// 一个极简的外部 store
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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 162：useInsertionEffect（用于 CSS-in-JS 库）</h3>

```jsx
import { useInsertionEffect } from 'react';

// 主要给样式库作者使用，在 DOM 变更前注入样式
function useCss(rule) {
  useInsertionEffect(() => {
    const style = document.createElement('style');
    style.textContent = rule;
    document.head.appendChild(style);
    return () => document.head.removeChild(style);
  }, [rule]);
}
```

---

## 九、并发特性（Concurrent Features）

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 163：Suspense 配合 lazy 懒加载</h3>

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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 164：多个 lazy 组件共享一个 Suspense</h3>

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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 165：嵌套 Suspense</h3>

```jsx
function Page() {
  return (
    <Suspense fallback={<p>加载页面...</p>}>
      <Header />
      <Suspense fallback={<p>加载内容...</p>}>
        <Content />
      </Suspense>
    </Suspense>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 166：Suspense + 数据请求（配合支持 Suspense 的库）</h3>

```jsx
// 需要配合 React Query、Relay 等支持 Suspense 的数据方案
function Profile() {
  return (
    <Suspense fallback={<p>加载用户...</p>}>
      <UserDetails />
    </Suspense>
  );
}
// UserDetails 内部使用支持 suspense 的数据获取
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 167：hydrateRoot（服务端渲染注水）</h3>

```jsx
import { hydrateRoot } from 'react-dom/client';
import App from './App';

// SSR 场景下，将服务端生成的 HTML 与 React 关联
hydrateRoot(document.getElementById('root'), <App />);
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 168：并发渲染避免卡顿的完整对比</h3>

```jsx
function App() {
  const [tab, setTab] = useState('home');
  const [isPending, startTransition] = useTransition();

  const switchTab = (name) => {
    startTransition(() => setTab(name)); // 切换重内容时保持界面响应
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


---

## 十、性能优化

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 169：React.memo 缓存组件</h3>

```jsx
const Item = React.memo(function Item({ text }) {
  console.log('渲染 Item：', text);
  return <li>{text}</li>;
});
// props 不变时，Item 不会重新渲染
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 170：React.memo 自定义比较函数</h3>

```jsx
const User = React.memo(
  function User({ user }) {
    return <p>{user.name}</p>;
  },
  (prev, next) => prev.user.id === next.user.id // 返回 true 表示不重渲染
);
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 171：拆分组件减少渲染范围</h3>

```jsx
// 把频繁变化的部分独立成小组件，避免整棵树重渲染
function Page() {
  return (
    <div>
      <ExpensiveStaticPart />
      <LiveClock /> {/* 只有这里在不停更新 */}
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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 172：useMemo 缓存传给子组件的对象</h3>

```jsx
function Parent({ id }) {
  // 避免每次渲染生成新对象引用，导致 memo 子组件失效
  const config = useMemo(() => ({ id, theme: 'dark' }), [id]);
  return <Child config={config} />;
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 173：懒加载路由组件</h3>

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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 174：列表虚拟化思路（只渲染可见项）</h3>

```jsx
// 大列表建议用 react-window / react-virtualized
// 这里演示基本思想：根据滚动位置切片
function VirtualList({ items, itemHeight = 30, height = 300 }) {
  const [scrollTop, setScrollTop] = useState(0);
  const start = Math.floor(scrollTop / itemHeight);
  const count = Math.ceil(height / itemHeight);
  const visible = items.slice(start, start + count);
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

---

## 十一、Context 与组件通信

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 175：创建可切换的主题 Context</h3>

```jsx
const ThemeContext = createContext();

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
  return <button onClick={toggle}>当前：{theme}</button>;
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 176：用 Context + useReducer 做全局状态</h3>

```jsx
const StoreContext = createContext();

function StoreProvider({ children }) {
  const [state, dispatch] = useReducer(reducer, { count: 0 });
  return (
    <StoreContext.Provider value={{ state, dispatch }}>
      {children}
    </StoreContext.Provider>
  );
}

function useStore() {
  return useContext(StoreContext);
}

function CounterDisplay() {
  const { state, dispatch } = useStore();
  return <button onClick={() => dispatch({ type: 'inc' })}>{state.count}</button>;
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 177：多个 Context 组合</h3>

```jsx
function App() {
  return (
    <ThemeProvider>
      <StoreProvider>
        <AuthProvider>
          <Main />
        </AuthProvider>
      </StoreProvider>
    </ThemeProvider>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 178：子传父（回调函数）</h3>

```jsx
function Child({ onSend }) {
  return <button onClick={() => onSend('来自子组件的数据')}>发送</button>;
}

function Parent() {
  const [msg, setMsg] = useState('');
  return (
    <>
      <Child onSend={setMsg} />
      <p>收到：{msg}</p>
    </>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 179：兄弟组件通信（状态提升）</h3>

```jsx
function Parent() {
  const [value, setValue] = useState('');
  return (
    <>
      <InputBox onChange={setValue} />
      <Display value={value} />
    </>
  );
}
function InputBox({ onChange }) {
  return <input onChange={e => onChange(e.target.value)} />;
}
function Display({ value }) {
  return <p>{value}</p>;
}
```

---

## 十二、进阶与实战

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 180：完整的 Todo 应用</h3>

```jsx
import { useState } from 'react';

function TodoApp() {
  const [todos, setTodos] = useState([]);
  const [text, setText] = useState('');

  const add = () => {
    if (!text.trim()) return;
    setTodos([...todos, { id: Date.now(), text, done: false }]);
    setText('');
  };
  const toggle = (id) =>
    setTodos(todos.map(t => t.id === id ? { ...t, done: !t.done } : t));
  const remove = (id) => setTodos(todos.filter(t => t.id !== id));

  return (
    <div>
      <input value={text} onChange={e => setText(e.target.value)}
        onKeyDown={e => e.key === 'Enter' && add()} />
      <button onClick={add}>添加</button>
      <ul>
        {todos.map(t => (
          <li key={t.id}>
            <span style={{ textDecoration: t.done ? 'line-through' : 'none' }}
              onClick={() => toggle(t.id)}>{t.text}</span>
            <button onClick={() => remove(t.id)}>×</button>
          </li>
        ))}
      </ul>
      <p>共 {todos.length} 项，完成 {todos.filter(t => t.done).length} 项</p>
    </div>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 181：防抖搜索（自定义 Hook）</h3>

```jsx
function useDebounce(value, delay = 500) {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(id);
  }, [value, delay]);
  return debounced;
}

function Search() {
  const [text, setText] = useState('');
  const debounced = useDebounce(text, 500);
  useEffect(() => {
    if (debounced) console.log('发起搜索：', debounced);
  }, [debounced]);
  return <input value={text} onChange={e => setText(e.target.value)} />;
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 182：错误边界（Error Boundary）</h3>

```jsx
import { Component } from 'react';

// 错误边界目前仍需用类组件实现
class ErrorBoundary extends Component {
  state = { hasError: false };
  static getDerivedStateFromError() {
    return { hasError: true };
  }
  componentDidCatch(error, info) {
    console.error('捕获错误：', error, info);
  }
  render() {
    if (this.state.hasError) return <h2>出错了，请刷新页面</h2>;
    return this.props.children;
  }
}

function App() {
  return (
    <ErrorBoundary>
      <BuggyComponent />
    </ErrorBoundary>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 183：Portal 渲染到 body（弹窗）</h3>

```jsx
import { createPortal } from 'react-dom';

function Modal({ children, onClose }) {
  return createPortal(
    <div className="overlay" onClick={onClose}>
      <div className="modal" onClick={e => e.stopPropagation()}>
        {children}
      </div>
    </div>,
    document.body
  );
}

function App() {
  const [open, setOpen] = useState(false);
  return (
    <>
      <button onClick={() => setOpen(true)}>打开弹窗</button>
      {open && <Modal onClose={() => setOpen(false)}>弹窗内容</Modal>}
    </>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 184：分页数据加载</h3>

```jsx
function PagedList() {
  const [page, setPage] = useState(1);
  const [data, setData] = useState([]);
  useEffect(() => {
    fetch(`/api/list?page=${page}`).then(r => r.json()).then(setData);
  }, [page]);
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

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 185：倒计时组件</h3>

```jsx
function Countdown({ seconds = 60 }) {
  const [left, setLeft] = useState(seconds);
  useEffect(() => {
    if (left <= 0) return;
    const id = setTimeout(() => setLeft(left - 1), 1000);
    return () => clearTimeout(id);
  }, [left]);
  return <p>{left > 0 ? `剩余 ${left} 秒` : '结束！'}</p>;
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 186：Tab 切换组件</h3>

```jsx
function Tabs() {
  const [active, setActive] = useState(0);
  const tabs = ['介绍', '参数', '评价'];
  return (
    <div>
      {tabs.map((t, i) => (
        <button key={i} onClick={() => setActive(i)}
          style={{ fontWeight: active === i ? 'bold' : 'normal' }}>{t}</button>
      ))}
      <div>当前内容：{tabs[active]}</div>
    </div>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 187：受控 + 校验的表单</h3>

```jsx
function SignupForm() {
  const [email, setEmail] = useState('');
  const [error, setError] = useState('');
  const submit = (e) => {
    e.preventDefault();
    if (!/^\S+@\S+\.\S+$/.test(email)) {
      setError('邮箱格式不正确');
      return;
    }
    setError('');
    alert('注册成功：' + email);
  };
  return (
    <form onSubmit={submit}>
      <input value={email} onChange={e => setEmail(e.target.value)} />
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <button type="submit">注册</button>
    </form>
  );
}
```

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 188：主题切换 + localStorage 持久化</h3>

```jsx
function ThemedApp() {
  const [dark, setDark] = useLocalStorage('dark', false);
  useEffect(() => {
    document.body.className = dark ? 'dark' : 'light';
  }, [dark]);
  return <button onClick={() => setDark(d => !d)}>
    切换到{dark ? '亮色' : '暗色'}
  </button>;
}
```

---

## 附录：常见易错点小结

1. **不要直接修改 state**：数组/对象要用展开语法生成新引用，否则 React 检测不到变化。
2. **useState 更新是异步/批处理的**：连续依赖旧值请用函数式更新 `setX(prev => ...)`。
3. **useEffect 依赖数组要写全**：漏写依赖会导致读取到过期的闭包变量。
4. **列表 key 要稳定唯一**：尽量用数据 id，避免用数组下标。
5. **清理副作用**：定时器、事件监听、订阅一定要在 `useEffect` 返回函数里清理。
6. **StrictMode 下开发环境副作用会执行两次**，这是有意的，用来暴露不纯的副作用。
7. **`createRoot` 取代 `ReactDOM.render`**：这是 React 18 的标准入口。
8. **区分紧急与非紧急更新**：用户输入用普通更新，昂贵的派生更新用 `startTransition` / `useDeferredValue`。

---

至此共 188 个示例，涵盖 React 18 从入门到进阶的核心用法。建议边读边动手运行，效果更佳。
