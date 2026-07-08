# 第四章 · 组件与 Props

> 本文是《React 18 & 19 系统教程》的第 4 章。完整目录见 [README](README.md)。

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

**详解**：组件的返回值有几种合法情况：① 单个 JSX 元素；② 用 Fragment（`<>...</>`）包裹的多个元素；③ `null`（表示不渲染任何内容，常用于条件隐藏）；④ 字符串或数字（直接作为文本渲染）。**不合法**的是直接返回多个并列元素而不包裹——原因见第三章示例 21（一个函数只能返回一个值）。

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

---
[← 上一章](03-JSX基础.md) · [📖 目录](README.md) · [下一章 →](05-State与事件.md)
