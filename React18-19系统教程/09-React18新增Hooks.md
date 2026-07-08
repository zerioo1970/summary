# 第九章 · React 18 新增 Hooks

> 本文是《React 18 & 19 系统教程》的第 9 章。完整目录见 [README](README.md)。

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

**详解**：初学者容易误用——`useId` **不能**用来生成列表渲染的 `key`。原因：① Hook 不能在循环里调用（违反 Hook 规则）；② `key` 应该来自数据本身的稳定标识（如 `item.id`），用于让 React 追踪列表项的身份（见第六章）。`useId` 的定位是"为 DOM 元素生成唯一属性 id"，两者用途完全不同，别混淆。

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
- `useLayoutEffect`：DOM 变更后、绘制前，用于同步读取/修改布局（避免闪烁，见第八章）；
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

---
[← 上一章](08-核心Hooks.md) · [📖 目录](README.md) · [下一章 →](10-并发特性.md)
