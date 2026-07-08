# 第十二章 · Context 与组件通信

> 本文是《React 18 & 19 系统教程》的第 12 章。完整目录见 [README](README.md)。

> **组件之间怎么"说话"？** 组件不是孤岛，它们经常需要共享数据、互相通知。React 提供了几种通信方式，选哪种取决于两个组件的"距离"和数据流向：
> - **父 → 子**：用 props（最基础，第四章已讲）；
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

**详解**：最基础的通信——父组件把数据作为 props 传给子组件（第四章已详讲）。这是单向数据流的正方向，简单可靠。任何"父组件已有、子组件要用"的数据，直接通过 props 传即可。记住 props 是**只读**的，子组件不能修改。当要传递的数据需要子组件"反向影响"父组件时，就需要下面的回调方式。

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

**详解**：子组件不能直接改父组件的数据，但可以**调用父组件传下来的函数**，把数据"回传"上去。这里父组件把 `setMsg` 作为 `onSend` 传给子组件，子组件点击时调用 `onSend('...')`，实际执行的是父组件的 `setMsg`，从而更新父组件的状态。这就是"子 → 父"通信的本质：**数据向上流动是通过回调函数实现的**（第四章示例 52、53 也讲过）。

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

**详解**：状态提升有个度——**提升到"用到它的所有组件的最近共同祖先"即可，不要更高**。放太高会导致很多不相关的中间组件被卷入、重渲染范围变大（第十一章示例 213 的"状态下放"讲的正是反向优化）。如果发现状态被提升得很高、要穿过很多层才能到达使用者，那就是该考虑用 Context 的信号了。

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

**详解**：前面的通信都是"声明式"（通过数据）。偶尔父组件需要**命令式地调用子组件的方法**（如让子组件的输入框聚焦、让子组件的视频播放）。这时用 `forwardRef` 把父组件的 ref 转发给子组件，再用 `useImperativeHandle` 决定暴露哪些方法（第八章示例 100 详讲过）。**这是补充手段**——命令式通信不如声明式清晰，应优先用 props/state/回调，只在确实需要触发某个动作时才用它。

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

---
[← 上一章](11-性能优化.md) · [📖 目录](README.md) · [下一章 →](13-进阶与实战.md)
