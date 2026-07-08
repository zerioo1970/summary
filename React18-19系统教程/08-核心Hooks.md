# 第八章 · 核心 Hooks

> 本文是《React 18 & 19 系统教程》的第 8 章。完整目录见 [README](README.md)。

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

---
[← 上一章](07-表单处理.md) · [📖 目录](README.md) · [下一章 →](09-React18新增Hooks.md)
