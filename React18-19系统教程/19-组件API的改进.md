# 第十九章 · 组件 API 的改进

> 本文是《React 18 & 19 系统教程》的第 19 章。完整目录见 [README](README.md)。

> React 19 对日常写组件的几处"老痛点"做了改进：`ref` 终于可以像普通 prop 一样传递（告别 `forwardRef`），`ref` 回调可以返回清理函数，`<Context>` 本身可以直接当 Provider，`useDeferredValue` 支持初始值。

### （A）ref 作为普通 prop —— 告别 forwardRef

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 300：函数组件直接接收 ref</h3>

```jsx
// ✅ React 19：ref 就是一个普通 prop
function MyInput({ placeholder, ref }) {
  return <input placeholder={placeholder} ref={ref} />;
}

// 使用
function Form() {
  const inputRef = useRef(null);
  return (
    <>
      <MyInput placeholder="姓名" ref={inputRef} />
      <button onClick={() => inputRef.current.focus()}>聚焦</button>
    </>
  );
}
```

**详解**：React 19 里，**函数组件可以直接从 props 中解构出 `ref`** 并转发给内部 DOM。不再需要 `forwardRef` 包裹。这大幅简化了组件库和转发 ref 的写法。官方还提供了 codemod 自动把旧代码迁移过来；未来版本会正式废弃并移除 `forwardRef`。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 301：对比 React 18 的 forwardRef 老写法</h3>

```jsx
// ❌ React 18 的写法（19 里仍兼容，但不再必要）
import { forwardRef } from 'react';

const MyInput = forwardRef(function MyInput({ placeholder }, ref) {
  return <input placeholder={placeholder} ref={ref} />;
});
```

**详解**：老写法里 `ref` 是 `forwardRef` 回调的**第二个参数**，而不是 props 的一部分，写起来啰嗦、类型也麻烦。对比示例 300 的新写法，React 19 让 `ref` 回归"就是个 prop"的直觉。**注意**：class 组件的 `ref` 指向的是**实例**，因此不会作为 prop 传入，这条改进只针对函数组件。

### （B）ref 清理函数

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 302：ref 回调返回一个清理函数</h3>

```jsx
function Widget() {
  return (
    <input
      ref={(node) => {
        // 元素挂载：node 是 DOM
        console.log('挂载', node);

        // ✅ React 19：返回清理函数，元素卸载时自动调用
        return () => {
          console.log('卸载，做清理');
        };
      }}
    />
  );
}
```

**详解**：React 19 支持从 **ref 回调返回一个清理函数**，就像 `useEffect` 的返回值一样——当元素从 DOM 移除时，React 会调用它。这对于"绑定/解绑事件、初始化/销毁第三方库实例"特别方便。以前你只能在下一次回调里判断 `node === null` 来做清理，现在语义清晰得多。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 303：用 ref 清理函数集成第三方库</h3>

```jsx
function Chart({ data }) {
  return (
    <div
      ref={(node) => {
        if (!node) return;
        const chart = new SomeChartLib(node, { data }); // 初始化
        return () => chart.destroy();                   // 卸载时销毁
      }}
    />
  );
}
```

**详解**：初始化 → 返回销毁函数，一处代码把"生老病死"管完，避免内存泄漏。这个能力对 DOM ref、class 组件 ref、以及 `useImperativeHandle` 都有效。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 304：TypeScript 提示——不要用隐式返回</h3>

```jsx
// ❌ 隐式返回了赋值表达式的结果，TS 会报错（被当成"意外的清理函数"）
<div ref={(current) => (instance = current)} />

// ✅ 用花括号，明确"不返回任何东西"
<div ref={(current) => { instance = current; }} />
```

**详解**：因为 ref 回调的返回值现在有了新含义（清理函数），**箭头函数的隐式返回**会让 TypeScript 困惑——它分不清你是想返回清理函数还是手滑。解决办法就是加花括号，把它变成没有返回值的语句体。官方 codemod `no-implicit-ref-callback-return` 可批量修复。

### （C）`<Context>` 直接作为 Provider

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 305：用 &lt;Context&gt; 代替 &lt;Context.Provider&gt;</h3>

```jsx
import { createContext } from 'react';

const ThemeContext = createContext('');

function App({ children }) {
  // ✅ React 19：Context 本身就能当 Provider
  return <ThemeContext value="dark">{children}</ThemeContext>;
}
```

**详解**：React 19 里 `<ThemeContext value={...}>` 可以直接作为 Provider 使用，**不用再写 `.Provider`**。少敲几个字符，读起来也更顺。官方提供 codemod 转换现有代码，未来会废弃 `<Context.Provider>`。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 306：对比旧写法</h3>

```jsx
// ❌ React 18 及以前
<ThemeContext.Provider value="dark">
  {children}
</ThemeContext.Provider>

// ✅ React 19
<ThemeContext value="dark">
  {children}
</ThemeContext>
```

**详解**：两者行为完全一致，只是新写法省掉了 `.Provider`。消费端不变——依然用 `useContext(ThemeContext)` 或 `use(ThemeContext)` 读取。

### （D）useDeferredValue 的初始值

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 307：useDeferredValue 第二参数 initialValue</h3>

```jsx
import { useDeferredValue } from 'react';

function Search({ query }) {
  // 首次渲染返回 ''，随后在后台调度一次用真实 query 的重渲染
  const deferredQuery = useDeferredValue(query, '');
  return <Results query={deferredQuery} />;
}
```

**详解**：React 19 给 `useDeferredValue(value, initialValue?)` 增加了**第二个可选参数 `initialValue`**。**首次渲染**时直接返回 `initialValue`（这里是空字符串，可以立刻渲染出轻量骨架），然后在后台安排一次用真实 `value` 的重渲染。这解决了 React 18 里"首屏必须先用真实值渲染一次"的问题，让初始渲染更快、更可控。


---

---
[← 上一章](18-use-API.md) · [📖 目录](README.md) · [下一章 →](20-文档元数据与资源预加载.md)
