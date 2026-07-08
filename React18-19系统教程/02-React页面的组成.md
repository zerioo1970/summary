# 第二章 · React 页面的组成

> 本文是《React 18 & 19 系统教程》的第 2 章。完整目录见 [README](README.md)。

> **本章要回答一个最根本的问题：一张 React 页面到底是由什么拼起来的？**
>
> 很多刚从传统页面开发（比如 ASP.NET WebForm 的 `.aspx`、或直接写 HTML）转过来的同学，最大的不适应是——"我的页面在哪？为什么找不到一个完整的 HTML 页面文件？" 本章就用一张结构图，把 React "页面 = 组件树" 的心智模型彻底讲清楚，为后面的 JSX（第三章）、组件与 Props（第四章）、State（第五章）打好地基。
>
> 本章以概念为主、少量代码，读完你应该能画出任意一张页面的"组件树"。

---

## 一、核心结论：一张页面 = 一棵组件树

在 React 里，**没有"页面"这个东西**，只有**组件（Component）**。所谓"一张页面"，其实是许多组件像搭积木一样**嵌套、组合**出来的一棵树。

下面这张图就是最典型的一张页面的组成方式：

```text
一个页面 (Page)
├── 顶部导航   <Header />
├── 侧边栏     <Sidebar />
├── 表单区     <Form />
│   ├── <Input />
│   ├── <TextInput />
│   └── <button>
└── 底部       <Footer />
```

**怎么读这张图？**

- 最外层的 `Page` 是**根**，它自己几乎不写具体内容，只负责"把下面几块拼在一起"。
- `Header`、`Sidebar`、`Form`、`Footer` 是页面的四大区块，每一个都是一个**独立的组件**（一个 `<Xxx />`）。
- `Form` 内部又由更小的组件组成：两个输入框组件 `Input` / `TextInput`，加一个原生的 `<button>`。
- 每个 `<Xxx />` 都是一个**返回 JSX 的函数**（函数组件，详见第四章）。

> 一句话记住：**React 页面是"组件的组合"，就像一篇文档是"段落的组合"。** 你不是在写"一个页面"，而是在写"一堆可复用的小块"，再把它们拼起来。

### 和 WebForm 的对照

如果你熟悉 ASP.NET WebForm，这个模型其实一点都不陌生——它和"一个 `.aspx` 页面上摆放多个用户控件 `.ascx`"是**同一个思想**：

| WebForm 概念 | React 对应 | 说明 |
| --- | --- | --- |
| 一个 `.aspx` 页面 | 根组件（如 `App` / `Page`） | 页面的入口、负责组装 |
| 用户控件 `.ascx` | 一个子组件 `<Header />` | 可复用的界面块 |
| 控件的公共属性 | `props` | 父给子传数据（第四章） |
| 母版页 MasterPage | 布局组件 + `children` / 嵌套路由 | 共享外壳（第十四章 Router） |
| 控件里再放控件 | 组件里再嵌组件 | 任意层级嵌套 |

区别在于：WebForm 用"拖控件 + 后台 `.cs`"的方式组织，React 用"函数 + JSX"的方式组织；但"页面由可复用小块拼成"这个骨架是完全一致的。

---

## 二、每个"零件"是什么：函数组件

图里的每个 `<Xxx />`，本质都是一个**普通的 JavaScript 函数**，它返回一段 JSX（描述这块界面长什么样）：

```jsx
// 一个最简单的组件：返回 JSX 的函数
function Header() {
  return <header>我是顶部导航</header>;
}

function Footer() {
  return <footer>我是页脚</footer>;
}
```

**详解**：

- 组件名**必须大写开头**（`Header` 而不是 `header`）——否则 React 会把它当成原生 HTML 标签。这条规则第四章会详细讲。
- 定义好后，就能像标签一样使用它：`<Header />`、`<Footer />`。可以把组件理解为"你自己发明的 HTML 标签"。
- 组件只能返回**一个根元素**（要包多个元素时用 `<>...</>`，详见第三章）。

---

## 三、从"根"到"叶"：一张页面是怎么长出来的

React 应用有一个**唯一的入口**（第一章讲过的 `createRoot`），它把**最顶层的那个组件**挂到网页上，然后这个组件再往下渲染它的子组件，一层层展开，最终形成整棵树。

```jsx
// main.jsx —— 应用入口（第一章）
import { createRoot } from 'react-dom/client';
import App from './App';

createRoot(document.getElementById('root')).render(<App />);
//                                                  ↑ 整棵组件树的"根"
```

`App` 就是那棵树的树根。它自己不写太多东西，只负责把各个区块拼起来：

```jsx
function App() {
  return (
    <div className="page">
      <Header />
      <Sidebar />
      <Form />
      <Footer />
    </div>
  );
}
```

**详解**：`App` 渲染时，React 发现里面有 `<Header />`、`<Sidebar />`……于是继续去渲染这些子组件；子组件里若还有孙组件（比如 `Form` 里有 `Input`），就继续往下——这个"从根往叶递归展开"的过程，就是 React 把组件树变成真实网页的过程。对照那张结构图看，`App` 就是 `Page`。

---

## 四、把整张图写成代码（完整示例）

现在我们把开头那张结构图**一比一实现**出来。这段代码就是一张完整（虽然简单）的 React 页面：

```jsx
// ---------- 叶子组件（最小的零件）----------
function Header() {
  return <header className="header">🌐 顶部导航</header>;
}

function Sidebar() {
  return (
    <aside className="sidebar">
      <ul>
        <li>菜单一</li>
        <li>菜单二</li>
      </ul>
    </aside>
  );
}

function Footer() {
  return <footer className="footer">© 2026 我的网站</footer>;
}

// Input / TextInput 是两个可复用的输入零件
function Input({ label }) {
  return (
    <div className="field">
      <label>{label}</label>
      <input />
    </div>
  );
}

function TextInput(props) {
  return <textarea {...props} />; // 把外部属性一次性透传（第三章、第四章）
}

// ---------- 中间层：表单区，由更小的零件组成 ----------
function Form() {
  return (
    <form className="form">
      <Input label="用户名" />
      <Input label="邮箱" />
      <TextInput placeholder="留言…" rows={3} />
      <button type="submit">提交</button>
    </form>
  );
}

// ---------- 根组件：把四大区块拼成一张页面 ----------
function App() {
  return (
    <div className="page">
      <Header />
      <Sidebar />
      <Form />
      <Footer />
    </div>
  );
}

export default App;
```

**详解**：把这段代码和第一节的结构图对照着看，你会发现它们**一模一样**：

- `App` = `Page`（根）；
- `Header` / `Sidebar` / `Form` / `Footer` = 四大区块；
- `Form` 内部又组合了 `Input`、`TextInput` 和 `<button>`。

这就是 React 组织一张页面的标准方式——**层层嵌套、层层组合**。你写的每个组件都可以在别处重复使用（比如 `Input` 在表单里用了两次），这就是"组件化"带来的复用能力。

---

## 五、组件之间怎么"传数据"：三个方向

页面拆成一堆组件后，它们不是彼此孤立的，需要互相配合。数据在组件树里的流动有三个基本方向（本章先建立概念，细节分别在第四章、第五章、第十二章展开）：

```text
        ┌─────────────┐
        │   父组件     │
        └──────┬──────┘
      props ↓        ↑ 回调函数
        ┌──────┴──────┐
        │   子组件     │
        └─────────────┘
```

1. **父 → 子：用 `props`（往下传）。** 父组件把数据当作"属性"传给子组件，例如上面的 `<Input label="用户名" />`，`label` 就是一个 prop。这是最基础、最主要的数据流方向——**单向、自上而下**。

2. **子 → 父：用回调函数（往上抛）。** 子组件不能直接修改父组件的数据，但父组件可以把一个"函数"作为 prop 传下去，子组件在合适时机调用它，把数据回传上去。例如 `<Input onChange={handleChange} />`。

3. **跨越很多层 / 全局共享：用 Context。** 当很多层、很多组件都要用同一份数据（如登录用户、主题），一层层传 props 太痛苦，就用 Context 让深层组件直接读取（第十二章）。

> 口诀：**props 往下传，回调往上抛，全局用 Context。** 记住这句话，90% 的"组件间传值"问题都能对号入座。

这也正是 React 和 WebForm 手感差异最大的地方：WebForm 里你可以在后台代码里随手 `TextBox1.Text = TextBox2.Text` 互相读写，而 React 强制数据"单向流动"——前期看似繁琐，但页面一大，数据从哪来、到哪去一目了然，更好维护。

---

## 六、`children`：组件的"内容插槽"

除了用 props 传数据，父组件还能把**一整块 JSX 内容**塞进子组件——这块内容通过特殊的 `children` prop 传递。这让"容器型组件"（卡片、面板、布局外壳）成为可能：

```jsx
// 一个通用的"卡片"容器，它不关心里面装什么
function Card({ children }) {
  return <div className="card">{children}</div>;
}

// 使用：开闭标签之间的内容，就是 children
function App() {
  return (
    <Card>
      <h3>标题</h3>
      <p>这段内容会出现在卡片里</p>
    </Card>
  );
}
```

**详解**：`Card` 组件用 `{children}` 决定"外部塞进来的内容渲染在哪里"。这非常像 WebForm 里带模板的容器控件（如 `Repeater`/`Panel` 里的 `<ItemTemplate>`），也像母版页里的 `ContentPlaceHolder`——都是"我定好外框，内容你来填"。`children` 是构建可复用布局组件的关键，第四章会深入讲。

---

## 七、一个组件通常放一个文件

真实项目里，每个组件一般**单独放一个文件**，用 `export` 导出、`import` 导入，最后由根组件组装。上面那张页面在工程里的文件结构通常是这样：

```text
src/
├── main.jsx          # 入口：createRoot 挂载 <App />
├── App.jsx           # 根组件：组装页面
└── components/
    ├── Header.jsx
    ├── Sidebar.jsx
    ├── Footer.jsx
    ├── Form.jsx
    ├── Input.jsx
    └── TextInput.jsx
```

```jsx
// components/Header.jsx
export default function Header() {
  return <header>顶部导航</header>;
}

// App.jsx
import Header from './components/Header';
import Sidebar from './components/Sidebar';
import Form from './components/Form';
import Footer from './components/Footer';

export default function App() {
  return (
    <div className="page">
      <Header />
      <Sidebar />
      <Form />
      <Footer />
    </div>
  );
}
```

**详解**：这就相当于 WebForm 里"每个 `.ascx` 用户控件一个文件，页面 `.aspx` 里 `Register` 并摆放它们"。文件拆分让每个组件职责单一、便于查找和复用。导入/导出的细节（默认导出 vs 命名导出）第四章会讲。

---

## 八、怎么决定"拆成哪些组件"？

初学者常纠结"这块要不要单独拆一个组件"。给你几条实用的判断标准：

1. **一个组件只做一件事。** 当一个组件又长又杂、要滚很久才能看完，就该拆了。
2. **重复出现的 UI 一定要拆。** 像上面用了两次的 `Input`，拆成组件后改一处、处处生效。
3. **有独立状态/交互的块适合拆。** 比如一个会自己计时的时钟、一个自带展开/收起的面板，把它连同它的状态一起关进一个小组件（这对性能也有好处，见第十一章）。
4. **先粗后细。** 不必一开始就拆得很碎，先把大区块（Header/Content/Footer）分出来，随着复杂度增加再细分。

> 经验法则：**按"界面上的视觉区块 + 可复用性 + 是否有独立状态"来拆分。** 拆得好，页面就像一份结构清晰的目录；拆不好，一个组件几百行什么都往里塞，就退化成了"另一种形式的大杂烩"。

---

## 九、本章小结

- React 里**没有"页面"，只有"组件"**；一张页面 = 一棵**组件树**（回看第一节那张结构图）。
- 每个组件是一个**返回 JSX 的函数**，可以像自定义标签一样嵌套使用。
- 应用有唯一入口，把**根组件**挂载上去，再从根往叶递归渲染出整棵树。
- 组件间数据流动：**props 往下传、回调往上抛、全局用 Context**；`children` 用来传"内容插槽"。
- 工程里**一个组件一个文件**，由根组件负责组装——和 WebForm"`.aspx` + 多个 `.ascx`"是同一套思想。

掌握了"页面 = 组件树"这个心智模型，接下来就可以正式学习组成组件的语法与能力了：**第三章 JSX 基础**（组件返回的到底是什么）、**第四章 组件与 Props**（怎么定义组件、怎么传数据）、**第五章 State 与事件**（怎么让组件"动"起来）。

---
[← 上一章](01-React18简介与环境准备.md) · [📖 目录](README.md) · [下一章 →](03-JSX基础.md)
