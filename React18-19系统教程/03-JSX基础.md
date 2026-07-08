# 第三章 · JSX 基础

> 本文是《React 18 & 19 系统教程》的第 3 章。完整目录见 [README](README.md)。

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

**详解**：这是新手最容易困惑的点。`{}` 里只能放"表达式"，**不能放 `if`、`for`、`switch` 这类"语句"**。原因回到示例 7——`{}` 里的内容最终要作为参数传给 `createElement`，而参数必须是一个值，语句不产生值。遇到复杂逻辑有两种办法：① 用三元/`&&` 等表达式；② 把逻辑写在 JSX 外面，用变量存结果再嵌入（第六章会详细展开）。

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

**详解**：因为 `if` 语句不能进 `{}`，所以 JSX 里做条件判断最常用三元表达式 `条件 ? A : B`。它是个表达式，能算出一个值。这里根据 `isVip` 显示不同文字和图标。（更多条件渲染的写法——`&&`、多分支、空状态等——集中在第六章详细讲解。）

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

**详解**：因为 Fragment 用得很频繁，React 提供了简写：空标签 `<>` 和 `</>`。它和 `<Fragment>` 完全等价，且不用 `import`，更简洁。**唯一的限制**：简写形式不能带任何属性——如果你需要给 Fragment 加 `key`（比如在列表里循环生成，见第六章），就必须用完整的 `<Fragment key={...}>` 写法。日常包裹用 `<>` 即可。

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

**详解**：React 在渲染 `{}` 里的值时，对某些值会"忽略、什么都不显示"：`null`、`undefined`、`false`、`true` 都不渲染。**这正是条件渲染 `{条件 && <组件/>}` 能工作的基础**——条件为 `false` 时整体值是 `false`，于是什么都不显示。但要特别小心：数字 `0` 和空字符串会被当作有效内容渲染出来（`0` 会在页面上显示一个"0"），这是常见的坑（第六章示例会专门讲）。

<br>

<h3 style="color: #FF8C00; font-size: 1.6em;">示例 27：在 JSX 中渲染数组</h3>

```jsx
function List() {
  const items = [<li key="a">苹果</li>, <li key="b">香蕉</li>];
  return <ul>{items}</ul>;
}
```

**详解**：`{}` 里可以直接放一个"元素数组"，React 会依次渲染数组里的每个元素。这就是列表渲染的底层原理——平时用 `array.map(...)` 生成的正是这样一个元素数组。注意数组里的每个元素都需要一个唯一的 `key` 属性，帮助 React 识别每一项（第六章会深入讲 `key` 的作用）。

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

---
[← 上一章](02-React页面的组成.md) · [📖 目录](README.md) · [下一章 →](04-组件与Props.md)
