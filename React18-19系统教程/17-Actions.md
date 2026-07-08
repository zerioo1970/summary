# 第十七章 · Actions：用异步 transition 统一管理提交

> 本文是《React 18 & 19 系统教程》的第 17 章。完整目录见 [README](README.md)。

> "提交表单 → 请求接口 → 处理结果"是 React 应用里最高频的场景。React 18 里你得手动维护 `isPending`、`error`、乐观更新、请求顺序……样板代码一大堆。**React 19 把这些做成了内建能力，统称 Actions**：凡是在 `startTransition` 里执行的异步函数，React 就会自动帮你管理 pending 状态、错误、表单重置和乐观更新。
>
> 围绕 Actions，React 19 新增了 4 样东西：`useTransition` 支持异步、`useActionState`、`<form action>`、`useFormStatus`，以及 `useOptimistic`。本节逐个拆解。

### （A）Actions 的由来：从"手动"到"自动"

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 287：React 18 的老写法——一切都要手动管理</h3>

```jsx
// ❌ React 18：pending / error 全靠手写
function UpdateName() {
  const [name, setName] = useState('');
  const [error, setError] = useState(null);
  const [isPending, setIsPending] = useState(false);

  const handleSubmit = async () => {
    setIsPending(true);
    const error = await updateName(name); // 假设返回错误信息或 null
    setIsPending(false);
    if (error) {
      setError(error);
      return;
    }
    redirect('/path');
  };

  return (
    <div>
      <input value={name} onChange={(e) => setName(e.target.value)} />
      <button onClick={handleSubmit} disabled={isPending}>更新</button>
      {error && <p>{error}</p>}
    </div>
  );
}
```

**详解**：注意这里有 **3 个 state**（name、error、isPending），而且 `setIsPending(true)` / `setIsPending(false)` 必须小心地成对出现——一旦某个 `return` 分支漏了 `setIsPending(false)`，按钮就会一直卡在 disabled。这类样板代码正是 Actions 要消灭的。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 288：React 19 的过渡写法——用 useTransition 支持异步</h3>

```jsx
// ✅ React 19：把异步函数丢进 startTransition，pending 自动管理
function UpdateName() {
  const [name, setName] = useState('');
  const [error, setError] = useState(null);
  const [isPending, startTransition] = useTransition();

  const handleSubmit = () => {
    startTransition(async () => {
      const error = await updateName(name);
      if (error) {
        setError(error);
        return;
      }
      redirect('/path');
    });
  };

  return (
    <div>
      <input value={name} onChange={(e) => setName(e.target.value)} />
      <button onClick={handleSubmit} disabled={isPending}>更新</button>
      {error && <p>{error}</p>}
    </div>
  );
}
```

**详解**：React 19 里 `startTransition` **可以接收 async 函数**了（React 18 只能接同步函数）。这样的异步 transition 会**立刻把 `isPending` 置为 `true`**，等异步任务全部结束后自动置回 `false`——你再也不用手写 `setIsPending`。这类"跑在异步 transition 里的函数"，官方约定俗成称为 **Action**。

> **术语约定**：Action = 在异步 transition（`startTransition` / `<form action>` / `useActionState`）里运行的函数。Actions 自动提供：**pending 状态**、**乐观更新**（配合 `useOptimistic`）、**错误处理**（配合 Error Boundary，并会自动回滚乐观更新）、以及 **表单自动重置**。

### （B）useActionState —— 为 Actions 量身定制的 Hook

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 289：useActionState 基础用法</h3>

```jsx
import { useActionState } from 'react';

function ChangeName() {
  const [error, submitAction, isPending] = useActionState(
    async (previousState, formData) => {
      const error = await updateName(formData.get('name'));
      if (error) {
        return error; // 返回值会成为下一次的 state（这里存错误）
      }
      redirect('/path');
      return null;
    },
    null // 第二个参数：初始 state
  );

  return (
    <form action={submitAction}>
      <input type="text" name="name" />
      <button type="submit" disabled={isPending}>更新</button>
      {error && <p>{error}</p>}
    </form>
  );
}
```

**详解**：`useActionState(action, initialState)` 接收一个 Action 函数和初始 state，返回一个**三元组** `[state, wrappedAction, isPending]`：
- `state`：Action **上一次的返回值**（初次是 `initialState`）。这里我们用它存"错误信息"。
- `wrappedAction`：包装后的 Action，可以直接丢给 `<form action={...}>` 或按钮的 `formAction`。
- `isPending`：Action 是否正在执行，**自动管理**。

对比示例 287，原来的 3 个 state + 手动 pending 现在**一行搞定**，且不用维护受控 input 的 `value`（表单用原生 `name` 提交，通过 `formData.get('name')` 取值）。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 290：Action 函数的两个参数——previousState 与 payload</h3>

```jsx
import { useActionState } from 'react';

function Counter() {
  // 这次不配合 <form>，而是直接把 wrappedAction 当普通函数调用
  const [count, incrementAction, isPending] = useActionState(
    async (previousCount, amount) => {
      await new Promise((r) => setTimeout(r, 500)); // 模拟异步
      return previousCount + amount; // 返回值成为新的 count
    },
    0
  );

  return (
    <>
      <p>当前：{count}</p>
      <button onClick={() => incrementAction(1)} disabled={isPending}>+1</button>
      <button onClick={() => incrementAction(5)} disabled={isPending}>+5</button>
    </>
  );
}
```

**详解**：Action 函数的**第一个参数永远是"上一次的 state"**（`previousCount`），**第二个参数是你调用 `wrappedAction` 时传入的实参**（这里是 `amount`）。当它配合 `<form action>` 使用时，第二个参数就是浏览器自动传入的 `FormData` 对象（见示例 289）。因为 Action 可组合，`useActionState` 会把每次的返回值作为新的 state 缓存下来。

> **改名提示**：`useActionState` 在 Canary 阶段曾叫 `ReactDOM.useFormState`，正式版**改名并搬到了 `react` 包**（`import { useActionState } from 'react'`）。老的 `useFormState` 已废弃，见到旧教程要注意区分。

### （C）表单 Actions：`<form action={fn}>`

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 291：给 form 的 action 传函数</h3>

```jsx
function Search() {
  async function search(formData) {
    const query = formData.get('query');
    const results = await fetchResults(query);
    // ...更新界面
  }

  return (
    <form action={search}>
      <input name="query" />
      <button type="submit">搜索</button>
    </form>
  );
}
```

**详解**：React 19 里 `<form>`、`<input>`、`<button>` 的 `action` / `formAction` 属性**可以直接传一个函数**。提交时 React 会：① 自动阻止默认的页面刷新；② 把表单数据打包成 `FormData` 传给你的函数；③ 把这个函数当作 Action 跑在 transition 里（自动 pending）。**成功后，对于非受控表单 React 会自动清空输入框**。这让"简单表单"几乎不需要任何 `useState`。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 292：button 的 formAction 与手动重置 requestFormReset</h3>

```jsx
import { requestFormReset } from 'react-dom';

function MultiActionForm() {
  async function save(formData) { /* 保存草稿 */ }
  async function publish(formData) { /* 发布 */ }

  return (
    <form>
      <textarea name="content" />
      {/* 同一个表单里，不同按钮触发不同 Action */}
      <button formAction={save}>存草稿</button>
      <button formAction={publish}>发布</button>
    </form>
  );
}
```

**详解**：`<button formAction={fn}>` 让**同一个表单的不同按钮执行不同的 Action**——非常适合"保存 / 发布"这类多操作表单。默认情况下，非受控表单提交成功后 React 会自动重置；如果你想**手动控制何时重置**（比如失败时保留内容），可以调用 react-dom 的新 API `requestFormReset(formElement)`。

### （D）useFormStatus —— 读取父表单的提交状态

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 293：设计系统里的提交按钮，无需层层传 props</h3>

```jsx
import { useFormStatus } from 'react-dom';

// 这是一个通用按钮组件，它不知道具体表单，却能感知提交状态
function SubmitButton({ children }) {
  const { pending } = useFormStatus();
  return (
    <button type="submit" disabled={pending}>
      {pending ? '提交中…' : children}
    </button>
  );
}

// 使用：直接放进任意 <form> 内即可
function ContactForm() {
  async function send(formData) {
    await sendMessage(formData.get('msg'));
  }
  return (
    <form action={send}>
      <input name="msg" />
      <SubmitButton>发送</SubmitButton>
    </form>
  );
}
```

**详解**：`useFormStatus()` 来自 `react-dom`，它读取**最近的父级 `<form>` 的提交状态**，就好像表单是一个 Context Provider 一样。返回对象里最常用的是 `pending`（是否正在提交），还包含 `data`（正在提交的 `FormData`）、`method`、`action`。这样封装通用组件（如设计系统里的 Button）时，**不用把 pending 一层层往下传 props**。

> **注意**：`useFormStatus` 只能读取**父组件**里的 `<form>` 状态，不能读取组件自身渲染的 `<form>`。也就是说，`SubmitButton` 必须是 `<form>` 的**子孙**才生效。

### （E）useOptimistic —— 乐观更新

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 294：改名时先"乐观"显示新名字</h3>

```jsx
import { useOptimistic } from 'react';

function ChangeName({ currentName, onUpdateName }) {
  const [optimisticName, setOptimisticName] = useOptimistic(currentName);

  const submitAction = async (formData) => {
    const newName = formData.get('name');
    setOptimisticName(newName);          // 立刻乐观显示
    const updatedName = await updateName(newName); // 真正请求
    onUpdateName(updatedName);           // 用服务端结果更新真实状态
  };

  return (
    <form action={submitAction}>
      <p>你的名字：{optimisticName}</p>
      <label>改名：</label>
      <input type="text" name="name" disabled={currentName !== optimisticName} />
    </form>
  );
}
```

**详解**：`useOptimistic(realValue)` 返回 `[乐观值, 设置乐观值的函数]`。在异步请求进行时，界面**立刻**显示 `optimisticName`（用户感觉零延迟）；当 Action 结束或出错，React 会**自动丢弃乐观值、回到真实值 `currentName`**。这就是"乐观 UI"：先假设成功、立即反馈，出错再回滚。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 295：乐观更新一个列表（发消息场景）</h3>

```jsx
import { useOptimistic, useState, useRef } from 'react';

function Thread({ messages, sendMessageAction }) {
  const formRef = useRef();

  // 第二个参数是 reducer：如何把"乐观项"合并进现有列表
  const [optimisticMessages, addOptimistic] = useOptimistic(
    messages,
    (state, newText) => [
      ...state,
      { text: newText, sending: true }, // 标记为"发送中"
    ]
  );

  async function formAction(formData) {
    const text = formData.get('message');
    addOptimistic(text);       // 立刻在列表末尾插入"发送中"的气泡
    formRef.current.reset();   // 立刻清空输入框
    await sendMessageAction(text); // 真正发送
  }

  return (
    <>
      {optimisticMessages.map((m, i) => (
        <div key={i}>
          {m.text} {m.sending && <small>（发送中…）</small>}
        </div>
      ))}
      <form action={formAction} ref={formRef}>
        <input name="message" placeholder="输入消息…" />
        <button type="submit">发送</button>
      </form>
    </>
  );
}
```

**详解**：`useOptimistic(state, updateFn)` 的第二个参数是一个 **reducer**，定义"如何把乐观数据合并进当前 state"。这里我们在列表末尾追加一条带 `sending: true` 标记的消息，界面立即显示"发送中"气泡。等请求完成、父组件真正把新消息写入 `messages` 后，乐观列表会自动被真实列表替换。**若请求失败，那条乐观消息会自动消失**——非常适合聊天、点赞、评论等场景。

**Actions 章小结**：`useActionState` 管"提交 + 结果 + pending"，`<form action>` 管"表单自动化"，`useFormStatus` 让子组件感知提交状态，`useOptimistic` 管"即时反馈"。四者常常组合使用，把过去几十行样板代码压缩到几行。


---

---
[← 上一章](16-React19简介与升级.md) · [📖 目录](README.md) · [下一章 →](18-use-API.md)
