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

React 18 最重要的变化是引入了**并发渲染（Concurrent Rendering）**，并带来了：
- 新的根 API `createRoot`
- 自动批处理（Automatic Batching）
- `startTransition` / `useTransition` / `useDeferredValue`
- 新 Hook：`useId`、`useSyncExternalStore`、`useInsertionEffect`
- 更完善的 `Suspense` 支持

### 示例 1：创建项目

```bash
# 使用 Vite（推荐，速度快）
npm create vite@latest my-app -- --template react
cd my-app
npm install
npm run dev
```

### 示例 2：React 18 的入口写法（createRoot）

```jsx
// main.jsx —— React 18 使用 createRoot 取代 ReactDOM.render
import React from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';

const root = createRoot(document.getElementById('root'));
root.render(<App />);
```

### 示例 3：对比 React 17 的旧写法

```jsx
// React 17（已废弃）
import ReactDOM from 'react-dom';
ReactDOM.render(<App />, document.getElementById('root'));

// React 18（推荐）
import { createRoot } from 'react-dom/client';
createRoot(document.getElementById('root')).render(<App />);
```

### 示例 4：开启严格模式（StrictMode）

```jsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>
);
// 注意：StrictMode 在开发模式下会让副作用执行两次，用于帮助发现问题。
```

### 示例 5：卸载根节点

```jsx
const root = createRoot(document.getElementById('root'));
root.render(<App />);

// 需要时可以彻底卸载
root.unmount();
```

---

## 二、JSX 基础

### 示例 6：最简单的 JSX

```jsx
const element = <h1>Hello, React 18!</h1>;
```

### 示例 7：JSX 中嵌入表达式

```jsx
const name = '张三';
const element = <h1>你好，{name}</h1>;
```

### 示例 8：JSX 中使用运算

```jsx
const a = 3, b = 4;
const element = <p>{a} + {b} = {a + b}</p>;
```

### 示例 9：JSX 属性（className / style）

```jsx
const element = (
  <div className="box" style={{ color: 'red', fontSize: 20 }}>
    带样式的文字
  </div>
);
// 注意：class 要写成 className，style 接收一个对象。
```

### 示例 10：JSX 必须有单一根节点（Fragment）

```jsx
import { Fragment } from 'react';

function List() {
  return (
    <Fragment>
      <li>项目 1</li>
      <li>项目 2</li>
    </Fragment>
  );
}

// 简写形式：
function List2() {
  return (
    <>
      <li>项目 1</li>
      <li>项目 2</li>
    </>
  );
}
```

### 示例 11：JSX 中的注释

```jsx
function App() {
  return (
    <div>
      {/* 这是 JSX 中的注释 */}
      <p>内容</p>
    </div>
  );
}
```

### 示例 12：JSX 中渲染 HTML（谨慎使用）

```jsx
function RawHtml() {
  const html = '<b>加粗文字</b>';
  return <div dangerouslySetInnerHTML={{ __html: html }} />;
}
// 警告：dangerouslySetInnerHTML 可能带来 XSS 风险，只用于可信内容。
```

---

## 三、组件与 Props

### 示例 13：函数组件

```jsx
function Welcome() {
  return <h1>欢迎光临</h1>;
}
```

### 示例 14：箭头函数组件

```jsx
const Welcome = () => <h1>欢迎光临</h1>;
```

### 示例 15：接收 props

```jsx
function Welcome(props) {
  return <h1>你好，{props.name}</h1>;
}

// 使用：<Welcome name="李四" />
```

### 示例 16：解构 props

```jsx
function Welcome({ name, age }) {
  return <p>{name}，{age} 岁</p>;
}
```

### 示例 17：props 默认值

```jsx
function Button({ text = '点击' }) {
  return <button>{text}</button>;
}
// 不传 text 时显示"点击"
```

### 示例 18：children 属性

```jsx
function Card({ children }) {
  return <div className="card">{children}</div>;
}

// 使用：
// <Card><p>卡片内容</p></Card>
```

### 示例 19：组件组合（嵌套）

```jsx
function Avatar({ url }) {
  return <img src={url} alt="头像" />;
}

function UserInfo({ user }) {
  return (
    <div>
      <Avatar url={user.avatar} />
      <span>{user.name}</span>
    </div>
  );
}
```

### 示例 20：传递函数作为 prop

```jsx
function Child({ onAction }) {
  return <button onClick={onAction}>触发</button>;
}

function Parent() {
  const handle = () => alert('子组件触发了');
  return <Child onAction={handle} />;
}
```

### 示例 21：透传所有 props（展开运算符）

```jsx
function Input(props) {
  return <input {...props} />;
}

// 使用：<Input type="text" placeholder="请输入" disabled />
```


---

## 四、State 与事件

### 示例 22：useState 计数器

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

### 示例 23：函数式更新（依赖旧值时的正确写法）

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

### 示例 24：state 为对象

```jsx
function Profile() {
  const [user, setUser] = useState({ name: '张三', age: 20 });
  // 更新对象要展开旧值
  const grow = () => setUser({ ...user, age: user.age + 1 });
  return <button onClick={grow}>{user.name}: {user.age}</button>;
}
```

### 示例 25：state 为数组（添加元素）

```jsx
function TodoList() {
  const [items, setItems] = useState(['学习']);
  const add = () => setItems([...items, '新任务']);
  return (
    <div>
      <button onClick={add}>添加</button>
      <ul>{items.map((t, i) => <li key={i}>{t}</li>)}</ul>
    </div>
  );
}
```

### 示例 26：state 为数组（删除元素）

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

### 示例 27：惰性初始化 state

```jsx
function Expensive() {
  // 传函数，只在首次渲染时执行一次
  const [value] = useState(() => {
    console.log('只计算一次');
    return computeExpensiveValue();
  });
  return <p>{value}</p>;
}
function computeExpensiveValue() { return 42; }
```

### 示例 28：多个 state

```jsx
function Form() {
  const [name, setName] = useState('');
  const [age, setAge] = useState(0);
  return (
    <>
      <input value={name} onChange={e => setName(e.target.value)} />
      <input value={age} onChange={e => setAge(+e.target.value)} />
    </>
  );
}
```

### 示例 29：事件对象

```jsx
function ClickInfo() {
  const handle = (e) => {
    console.log('点击坐标：', e.clientX, e.clientY);
    e.preventDefault(); // 阻止默认行为
  };
  return <a href="/" onClick={handle}>点我</a>;
}
```

### 示例 30：传参给事件处理函数

```jsx
function Buttons() {
  const handle = (id) => alert('按钮 ' + id);
  return (
    <>
      <button onClick={() => handle(1)}>按钮1</button>
      <button onClick={() => handle(2)}>按钮2</button>
    </>
  );
}
```

### 示例 31：阻止事件冒泡

```jsx
function Box() {
  return (
    <div onClick={() => console.log('外层')}>
      <button onClick={(e) => { e.stopPropagation(); console.log('内层'); }}>
        点我不冒泡
      </button>
    </div>
  );
}
```

### 示例 32：键盘事件

```jsx
function SearchBox() {
  const onKeyDown = (e) => {
    if (e.key === 'Enter') alert('搜索：' + e.target.value);
  };
  return <input onKeyDown={onKeyDown} placeholder="回车搜索" />;
}
```

### 示例 33：自动批处理（React 18 新行为）

```jsx
function Batching() {
  const [a, setA] = useState(0);
  const [b, setB] = useState(0);
  // React 18 中，即使在 setTimeout / Promise 里，
  // 下面两次更新也会被合并成一次重新渲染
  const handle = () => {
    setTimeout(() => {
      setA(x => x + 1);
      setB(x => x + 1); // 只触发一次渲染
    }, 100);
  };
  return <button onClick={handle}>{a}-{b}</button>;
}
```

### 示例 34：退出批处理（flushSync）

```jsx
import { flushSync } from 'react-dom';

function Demo() {
  const [count, setCount] = useState(0);
  const handle = () => {
    flushSync(() => setCount(c => c + 1)); // 立即同步更新 DOM
    console.log('DOM 已更新');
  };
  return <button onClick={handle}>{count}</button>;
}
```

---

## 五、条件渲染与列表

> 本章从"最简单的条件判断"讲起，逐步过渡到列表渲染，再到两者结合的实战写法，共 30 个示例。
> 核心思想只有两条：**条件渲染 = 用 JavaScript 的判断决定返回什么 JSX**；**列表渲染 = 用数组的 `map` 把数据变成一组 JSX**。

### （A）条件渲染 —— 从最简单开始

### 示例 35：最简单的条件——提前 return

```jsx
function Greeting({ isLoggedIn }) {
  if (isLoggedIn) {
    return <p>欢迎回来</p>;
  }
  return <p>请先登录</p>;
}
```

**详解**：这是最直观的写法。组件本质是一个函数，你完全可以用普通的 `if` 判断，然后 `return` 不同的 JSX。命中第一个 `return` 后函数就结束了，所以下面那行只有在 `isLoggedIn` 为假时才会执行。适合"整块内容完全不同"的场景。

### 示例 36：三元运算符（内联在 JSX 里）

```jsx
function Status({ online }) {
  return <p>{online ? '在线' : '离线'}</p>;
}
```

**详解**：当只是"一小段内容"随条件变化时，用 `if` 拆成两个 `return` 太啰嗦。JSX 的 `{}` 里可以放**表达式**，而三元 `条件 ? A : B` 正是一个表达式。`online` 为真显示"在线"，否则"离线"。记住：`{}` 里不能放 `if` 语句，但可以放三元表达式。

### 示例 37：三元里返回 JSX 元素

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

**详解**：三元的两个分支不仅能返回字符串，也能返回完整的 JSX 元素。相比示例 35 的提前 return，这种写法能让"页面大部分相同、只有局部不同"的结构写在一起，一眼看清差异在哪。

### 示例 38：`&&` 短路渲染（有则显示，无则不显示）

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

### 示例 39：`&&` 的经典陷阱——数字 0 会被显示出来

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

### 示例 40：`||` 提供默认内容（兜底）

```jsx
function UserName({ name }) {
  return <p>{name || '匿名用户'}</p>;
}
```

**详解**：`A || B` 表示 `A` 为真用 `A`，否则用 `B`。当 `name` 是空字符串、`null`、`undefined` 等假值时，就显示"匿名用户"。这是给缺省数据做兜底的简洁写法。若你希望 `0` 或 `''` 也算有效值，应改用空值合并 `??`（见示例 49）。

### 示例 41：用 null 隐藏整个组件

```jsx
function Warning({ show }) {
  if (!show) return null; // 返回 null 表示"渲染但不产生任何 DOM"
  return <div className="warn">⚠️ 警告！</div>;
}
```

**详解**：组件返回 `null` 是完全合法的，表示"这个组件此刻不显示任何东西"。它和示例 38 的 `&&` 效果类似，但写在组件内部，适合"组件自己决定要不要显示"的封装场景（比如一个通用的提示框组件）。

### 示例 42：先把 JSX 存进变量，再渲染

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

### 示例 43：多分支 if / else if

```jsx
function Grade({ score }) {
  if (score >= 90) return <span>优秀</span>;
  if (score >= 60) return <span>及格</span>;
  return <span>不及格</span>;
}
```

**详解**：多个区间判断时，连续的 `if + return` 是最清晰的表达方式。命中即返回，无需写 `else`。注意判断顺序要"从高到低"，否则 `score >= 60` 会先把 95 分也拦下。

### 示例 44：用 switch 处理多状态

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

### 示例 45：用对象映射代替 switch（推荐）

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

### 示例 46：在 JSX 中用立即执行函数写复杂逻辑（IIFE）

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

**详解**：JSX 的 `{}` 里只能放表达式、不能放语句。当你确实想在此处写 `if/switch` 这类语句，可以用"立即执行函数"`(() => { ... })()` 把语句包起来——它整体是一个表达式。不过多数情况下，示例 42（变量存 JSX）更易读，IIFE 应谨慎使用。

### 示例 47：把条件判断抽成子组件

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

### 示例 48：加载 / 错误 / 成功三态渲染（实战常见）

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

### 示例 49：可选链 `?.` 与空值合并 `??` 结合条件渲染

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

### 示例 50：最简单的列表——map 渲染字符串数组

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

### 示例 51：map 带索引参数

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

**详解**：`map` 的回调第二个参数是当前项的下标 `index`（从 0 开始）。这里用 `index + 1` 显示排名。注意：**用 index 来显示序号没问题，但用它当 `key` 要谨慎**（见示例 53）。

### 示例 52：渲染对象数组

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

### 示例 53：key 的作用与"不要用 index 当 key"

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

### 示例 54：用 filter 过滤后再渲染

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

### 示例 55：用 sort 排序后渲染（先拷贝再排序）

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

### 示例 56：一次返回多个元素——带 key 的 Fragment

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

### 示例 57：列表 + 条件——每一项内部再做条件渲染

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

### 示例 58：空列表的友好提示

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

### 示例 59：嵌套列表（列表里再套列表）

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

### 示例 60：斑马纹 / 高亮——用 index 决定样式

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

### 示例 61：列表项绑定事件并传递该项数据

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

### 示例 62：把数据转成组件数组（渲染子组件列表）

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

### 示例 63：分组渲染（先用 reduce 分组，再渲染）

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

### 示例 64：综合实战——搜索过滤 + 排序 + 空态 + 计数

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

### 示例 65：受控输入框

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

### 示例 66：受控 textarea

```jsx
function Comment() {
  const [text, setText] = useState('');
  return <textarea value={text} onChange={e => setText(e.target.value)} />;
}
```

### 示例 67：受控 select 下拉框

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

### 示例 68：复选框（checkbox）

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

### 示例 69：单选按钮（radio）

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

### 示例 70：一个函数处理多个字段

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

### 示例 71：表单提交

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

### 示例 72：非受控组件（用 ref 读取值）

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

### 示例 73：文件上传

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

### 示例 74：useEffect 最简单的样子（每次渲染后执行）

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

### 示例 75：空依赖数组（只在挂载时执行一次）

```jsx
function OnMount() {
  useEffect(() => {
    console.log('组件挂载了，这句只打印一次');
  }, []); // 第二个参数是空数组
  return <p>Hello</p>;
}
```

**详解**：`useEffect` 的第二个参数叫"依赖数组"。传空数组 `[]` 表示"没有任何依赖"，于是这个 effect 只在组件**首次挂载后**执行一次，后续重渲染都不再执行。适合做只需一次的初始化，比如获取初始数据、注册全局监听。（注意：开发环境的 `StrictMode` 下会故意执行两次以帮你发现问题，生产环境只执行一次。）

### 示例 76：指定依赖（依赖变化时才执行）

```jsx
function Watcher({ userId }) {
  useEffect(() => {
    console.log('userId 变成了：', userId);
  }, [userId]); // 只有 userId 变化时才重新执行
  return <p>当前用户：{userId}</p>;
}
```

**详解**：依赖数组里列出的值，只要**任意一个**在两次渲染之间发生变化，effect 就会重新执行。这里 `userId` 不变时，即使组件因别的原因重渲染，effect 也不会跑。React 用 `Object.is` 逐个比较依赖项，所以依赖应放"原始值或稳定引用"。

### 示例 77：依赖数组的三种形态对比（重点总结）

```jsx
useEffect(() => { /* ... */ });          // ① 不传：每次渲染后都执行
useEffect(() => { /* ... */ }, []);      // ② 空数组：只在挂载后执行一次
useEffect(() => { /* ... */ }, [a, b]);  // ③ 有依赖：a 或 b 变化时执行
```

**详解**：这是理解 `useEffect` 的关键。记住这张对照表：
- **不传第二个参数** → 每次渲染后都执行（很少用，通常是没想清楚）；
- **`[]`** → 仅挂载时执行一次，卸载时执行清理；
- **`[a, b]`** → 挂载时执行，之后每当 `a` 或 `b` 变化时再执行。

选哪种，取决于你的副作用"依赖了哪些数据"。原则是：**effect 内部用到的每一个组件内变量（props、state、函数），都应出现在依赖数组里**（见示例 83）。

### 示例 78：清理函数（以定时器为例）

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

### 示例 79：清理事件监听

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

### 示例 80：在 useEffect 中请求数据（基础版）

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

### 示例 81：请求数据的竞态问题与 ignore 标志

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

### 示例 82：闭包陷阱——读到"过期"的 state

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

### 示例 83：不要漏写依赖（并理解为什么）

```jsx
function Search({ query, onResult }) {
  useEffect(() => {
    fetchData(query).then(onResult);
    // 依赖数组应包含 effect 内用到的所有外部变量
  }, [query, onResult]);
  return null;
}
```

**详解**：ESLint 的 `react-hooks/exhaustive-deps` 规则会提醒你补全依赖。漏写依赖的后果是：effect 内部读到的是某次渲染时"冻结"的旧值，行为难以预测。原则是**诚实地列出 effect 用到的每一个组件内变量**。如果某个依赖变化太频繁导致 effect 反复执行，正确做法不是删依赖，而是用 `useCallback`/`useMemo` 稳定它，或用函数式更新绕开（如示例 82）。

### （B）useRef —— 引用 DOM 与保存可变值

### 示例 84：useRef 引用 DOM 元素

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

### 示例 85：useRef 保存可变值（修改它不会触发渲染）

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

### 示例 86：useRef vs useState 的区别（对照理解）

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

### 示例 87：useRef 保存上一次的值（自定义 usePrevious）

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

### 示例 88：useContext 基础用法

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

### 示例 89：useContext 解决"逐层传递 props"（prop drilling）

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

### 示例 90：useReducer 计数器（入门）

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

### 示例 91：useReducer 管理复杂表单

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

### 示例 92：useReducer 管理列表

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

### 示例 93：useReducer vs useState 如何选择

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

### 示例 94：useMemo 缓存昂贵的计算结果

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

### 示例 95：useMemo 稳定对象引用（配合 React.memo）

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

### 示例 96：useCallback 缓存函数

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

**详解**：`useCallback(fn, deps)` 相当于 `useMemo(() => fn, deps)`，专门用来缓存"函数"。道理同示例 95：函数每次渲染都是新引用，会让接收它的 `memo` 子组件失效。用 `useCallback` 固定函数引用后，父组件计数变化不再连累 `Child` 重渲染。依赖数组里要放函数内部用到的会变化的变量。

### 示例 97：不要滥用 useMemo / useCallback

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

### 示例 98：useLayoutEffect 同步测量避免闪烁

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

### 示例 99：useLayoutEffect 与 useEffect 的区别

```jsx
useEffect(() => { /* 绘制后异步执行，不阻塞渲染，99% 情况用它 */ });
useLayoutEffect(() => { /* 绘制前同步执行，会阻塞渲染，仅测量/定位时用 */ });
```

**详解**：一句话记忆——**默认永远用 `useEffect`**。它在浏览器绘制后异步执行，不会拖慢首屏。只有当你遇到"用了 useEffect 会出现明显闪烁/跳动"（因为你需要在绘制前读布局并改 DOM）时，才换成 `useLayoutEffect`。两者 API 完全一样，区别只在执行时机与是否阻塞绘制。

### 示例 100：useImperativeHandle 向父组件暴露方法

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

### 示例 101：自定义 Hook：useToggle

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

### 示例 102：自定义 Hook：useFetch（含加载态与竞态处理）

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

**详解**：这个 `useFetch` 把"请求数据"这套通用逻辑——加载态、错误态、竞态处理（示例 81 的 `ignore` 标志）——全部封装。任何组件只要 `const { data, loading, error } = useFetch(url)` 就能拿到完整的请求状态，组件本身只关心怎么渲染。这正是自定义 Hook 的威力：把重复的副作用逻辑抽象成一个可复用的"能力"。

### 示例 103：自定义 Hook：useLocalStorage（与浏览器存储同步）

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

### 示例 104：useId 生成唯一 id

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

### 示例 105：useId 生成多个相关 id

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

### 示例 106：useTransition 标记非紧急更新

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

### 示例 107：useDeferredValue 延迟值

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

### 示例 108：startTransition（非 Hook 版本）

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

### 示例 109：useSyncExternalStore 订阅外部数据源

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

### 示例 110：useSyncExternalStore 订阅自定义 store

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

### 示例 111：useInsertionEffect（用于 CSS-in-JS 库）

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

### 示例 112：Suspense 配合 lazy 懒加载

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

### 示例 113：多个 lazy 组件共享一个 Suspense

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

### 示例 114：嵌套 Suspense

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

### 示例 115：Suspense + 数据请求（配合支持 Suspense 的库）

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

### 示例 116：hydrateRoot（服务端渲染注水）

```jsx
import { hydrateRoot } from 'react-dom/client';
import App from './App';

// SSR 场景下，将服务端生成的 HTML 与 React 关联
hydrateRoot(document.getElementById('root'), <App />);
```

### 示例 117：并发渲染避免卡顿的完整对比

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

### 示例 118：React.memo 缓存组件

```jsx
const Item = React.memo(function Item({ text }) {
  console.log('渲染 Item：', text);
  return <li>{text}</li>;
});
// props 不变时，Item 不会重新渲染
```

### 示例 119：React.memo 自定义比较函数

```jsx
const User = React.memo(
  function User({ user }) {
    return <p>{user.name}</p>;
  },
  (prev, next) => prev.user.id === next.user.id // 返回 true 表示不重渲染
);
```

### 示例 120：拆分组件减少渲染范围

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

### 示例 121：useMemo 缓存传给子组件的对象

```jsx
function Parent({ id }) {
  // 避免每次渲染生成新对象引用，导致 memo 子组件失效
  const config = useMemo(() => ({ id, theme: 'dark' }), [id]);
  return <Child config={config} />;
}
```

### 示例 122：懒加载路由组件

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

### 示例 123：列表虚拟化思路（只渲染可见项）

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

### 示例 124：创建可切换的主题 Context

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

### 示例 125：用 Context + useReducer 做全局状态

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

### 示例 126：多个 Context 组合

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

### 示例 127：子传父（回调函数）

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

### 示例 128：兄弟组件通信（状态提升）

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

### 示例 129：完整的 Todo 应用

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

### 示例 130：防抖搜索（自定义 Hook）

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

### 示例 131：错误边界（Error Boundary）

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

### 示例 132：Portal 渲染到 body（弹窗）

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

### 示例 133：分页数据加载

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

### 示例 134：倒计时组件

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

### 示例 135：Tab 切换组件

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

### 示例 136：受控 + 校验的表单

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

### 示例 137：主题切换 + localStorage 持久化

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

至此共 137 个示例，涵盖 React 18 从入门到进阶的核心用法。建议边读边动手运行，效果更佳。
