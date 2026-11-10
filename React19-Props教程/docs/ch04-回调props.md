# 第 4 节 回调 props

> **本节目标**：彻底搞清消息怎么从子组件送回父组件 —— 这是整个教程的重点。
>
> **配套代码**：`code/src/ch04/TaskPanel.tsx`（最小形态）、`code/src/ch04/CallbackDemo.tsx`（三级完整版）
>
> 前三节的 props 都是父组件**给**子组件东西。这一节反过来：子组件怎么**告诉**父组件「发生了什么」。

---

## 4.1 为什么需要回调 props

props 是只读的，子组件不能改。可界面上的操作偏偏都发生在子组件里（按钮在它身上），而数据住在父组件的 state 里。

React 的解法很直接：**父组件把自己的函数当作 props 传下去，子组件在合适的时机调用它。**

被执行的是父组件的代码，改的是父组件自己的 state —— 规矩没被破坏，事情也办成了。

**核心理解**：子组件永远不知道自己调用的是谁、调用之后会发生什么。它只负责「喊一声」。这就是这个模式的价值 —— 同一个子组件换个父组件传别的函数，行为完全不同。

---

## 4.2 本节的最小例子

先把例子缩到最小，后面两张图讲的就是这 28 行。**这份代码就是 `code/src/ch04/TaskPanel.tsx`，行号完全一致**：

```tsx
import { useState } from 'react';

type TaskCardProps = {
  onToggle: () => void;
};

function TaskCard({ onToggle }: TaskCardProps) {
  return (
    <button type="button" onClick={onToggle}>
      切换完成
    </button>
  );
}

export default function TaskPanel() {
  const [done, setDone] = useState(false);

  function handleToggle(): void {
    setDone((d) => !d);
  }

  return (
    <div>
      <p>{done ? '已完成' : '进行中'}</p>
      <TaskCard onToggle={handleToggle} />
    </div>
  );
}
```

---

## 4.3 🖼 图解：两根线，别混成一根

理解回调 props 时最容易犯的错，是把两件事看成一根线。实际上这里有**两根方向相反的线**：

| | 何时发生 | 方向 | 干什么 |
|---|---|---|---|
| **装配线** | 每次渲染 | 父 → 子 | 把函数交下去，铺好通道 |
| **触发线** | 每次点击 | 子 → 父 | 沿着通道把消息送上去 |

像装电灯：先把电线从配电箱牵到开关（装配），之后按开关电流才流动（触发）。**线是提前铺好的，点击只是通电。**

### 装配线（渲染时，父 → 子）

主车道依次「停靠」参与传递的代码行，停靠顺序就是括号里的编号：

```
图 2　装配线 —— 渲染时发生，方向：父 -> 子（把函数交下去）

 1  import { useState } from 'react';
 2
 3  type TaskCardProps = {
 4    onToggle: () => void;                           <-- 契约：只核对名字和形状，不搬运东西
 5  };
 6
 7  function TaskCard({ onToggle }: TaskCardProps) {  <----+ (3)
 8    return (                                             |
 9      <button type="button" onClick={onToggle}>     <----+ (4)
10        切换完成                                         |
11      </button>                                          |
12    );                                                   |
13  }                                                      |
14                                                         |
15  export default function TaskPanel() {                  |
16    const [done, setDone] = useState(false);             |
17                                                         |
18    function handleToggle(): void {                 <----+ (1)
19      setDone((d) => !d);                                |
20    }                                                    |
21                                                         |
22    return (                                             |
23      <div>                                              |
24        <p>{done ? '已完成' : '进行中'}</p>              |
25        <TaskCard onToggle={handleToggle} />        <----+ (2)
26      </div>
27    );
28  }

装配在每次渲染时完成，此刻没有任何代码被执行 —— 只是把函数交付到位，像把电线牵到开关。
```

| | 行 | 发生了什么 |
|---|---|---|
| (1) | L18 | `handleToggle` 在父组件里诞生 |
| (2) | L25 | 被装进一个写着 `onToggle` 的口袋，递给子组件 |
| (3) | L07 | 子组件从口袋里取出，成为本地变量 `onToggle` |
| (4) | L09 | 挂到 `onClick` 上，等着被点击（**此刻还没有执行**） |

**第 4 行的类型契约不在这条线上** —— 它只核对名字和形状，本身不搬运任何东西。把它画成传递通道是最常见的误解。

彩色矢量版：[assets/fig2-wiring.svg](assets/fig2-wiring.svg)

### 触发线（点击时，子 → 父）

```
图 3　触发线 —— 点击时发生，方向：子 -> 父（把消息送上去）

 1  import { useState } from 'react';
 2
 3  type TaskCardProps = {
 4    onToggle: () => void;
 5  };
 6
 7  function TaskCard({ onToggle }: TaskCardProps) {
 8    return (
 9      <button type="button" onClick={onToggle}>     <----+ (1)
10        切换完成                                         |
11      </button>                                          |
12    );                                                   |
13  }                                                      |
14                                                         |
15  export default function TaskPanel() {                  |
16    const [done, setDone] = useState(false);        <----+ (4)
17                                                         |
18    function handleToggle(): void {                 <----+ (2)
19      setDone((d) => !d);                           <----+ (3)
20    }                                                    |
21                                                         |
22    return (                                             |
23      <div>                                              |
24        <p>{done ? '已完成' : '进行中'}</p>         <----+ (5)
25        <TaskCard onToggle={handleToggle} />
26      </div>
27    );
28  }

触发线每次点击都走一遍，走的正是装配时铺好的那根线，方向相反 —— 像按下开关后电流开始流动。
```

| | 行 | 发生了什么 |
|---|---|---|
| (1) | L09 | 用户点击，浏览器执行 `onClick` 里存的东西 |
| (2) | L18 | 子组件只知道调了 `onToggle`，真身是 `handleToggle` → **代码回到父组件** |
| (3) | L19 | `setDone` 改父组件自己的 state |
| (4) | L16 | `done` 拿到新值 |
| (5) | L24 | 重新渲染，新文字出现在界面上，闭环完成 |

彩色矢量版：[assets/fig3-trigger.svg](assets/fig3-trigger.svg)

### 一句话抓住本质

> **`onToggle` 不是一个新函数，它是 `handleToggle` 在子组件里的别名。**

第 9 行写 `onClick={onToggle}`，等于写 `onClick={父组件的 handleToggle}` —— 只是子组件没有权限、也不需要知道这个真名。

还有一点值得注意：**装配线的编号顺序是 18 → 25 → 7 → 9，在文件里是跳着走的**。代码的书写顺序和执行顺序本来就不一致 —— 子组件写在上面，但它是后被调用的。这是读 React 代码容易晕的原因之一。

---

## 4.4 回调签名的解剖

```
onToggle: () => void
          ▲▲▲▲    ▲▲▲▲
          去程     回程
          子 → 父   父 → 子
```

- **参数**（括号里）是子组件往上带的数据。没有数据要带就写 `()`。
- **返回值 `void`** 表示父组件处理完不给子组件任何答复。
- 类型声明本身**不传任何东西**，它只是一张形状说明书。真正的传递发生在 `onToggle()` 执行的那一刻。

`void` 常见到几乎成了默认，因为父组件的响应方式是改 state 触发重渲染 —— 子组件下次渲染自然拿到新 props，不需要回话。

### 返回值不是 `void` 的少数场景

当子组件需要**根据父组件的答复决定自己怎么做**时：

```tsx
type Props = {
  onBeforeDelete: () => boolean;   // 问父组件一句：现在能删吗
};

function TaskCard({ onBeforeDelete }: Props) {
  function handleClick(): void {
    const allowed: boolean = onBeforeDelete();   // 接住父组件的答复
    if (!allowed) return;                        // 父组件说不行就不删
    console.log('删除');
  }

  return <button type="button" onClick={handleClick}>删除</button>;
}
```

这种「向上询问」不常见，但存在。别把回调 props 和 `void` 绑死在一起理解。

---

## 4.5 三个级别

回调 props 的复杂度只有三档，递进关系很清楚：

| 级别 | 签名 | 子组件有自己的 state 吗 | 需要中间函数吗 | 典型场景 |
|---|---|---|---|---|
| 第 1 级 | `() => void` | 无 | 不需要 | 确认、关闭、切换 |
| 第 2 级 | `(data: T) => void` | 无 | 需要 | 删除某项、选中某项 |
| 第 3 级 | `(data: T) => void` | 有 | 需要 | 表单、内联编辑 |

### 第 1 级：只通知，不带数据

子组件除了「事情发生了」，没有任何数据要送上去。

```tsx
type TaskCardProps = {
  onToggle: () => void;
};

function TaskCard({ onToggle }: TaskCardProps) {
  // 点击 = 原封不动地通知父组件，中间无加工，所以直接挂
  return <button type="button" onClick={onToggle}>切换完成</button>;
}
```

**要点**：`onClick={onToggle}` 可以直接挂，因为不需要控制实参。React 会把鼠标事件对象作为第一个参数传进去，但 `() => void` 声明了不接收参数，多余实参被忽略，类型是安全的。

### 第 2 级：带一个数据上去

**判断标准只有一句**：子组件除了「我完成了」，还需不需要额外告诉父组件什么内容？

```tsx
type TaskCardProps = {
  task: Task;
  onDelete: (id: number) => void;   // 括号里是去程数据
};

function TaskCard({ task, onDelete }: TaskCardProps) {
  // 要传参，所以不能直接挂，必须用箭头函数包一层
  return (
    <button type="button" onClick={() => onDelete(task.id)}>
      删除
    </button>
  );
}
```

父组件接住：

```tsx
function handleDelete(id: number): void {
  console.log('要删的是', id);
}
```

**数据的旅程**（盯住这一个值走完全程，第 2 级就通了）：

```
子组件里的 task.id = 7
     |  作为实参传入
onDelete(7)
     |  onDelete 实际指向 handleDelete
handleDelete(id)  ->  id = 7
     |  父组件据此改自己的 state
界面上那条任务消失
```

`task.id` 和 `id` 是**同一个值的两个名字** —— 子组件里的实参，父组件里的形参。名字不必一致，位置一致就行。和普通函数调用完全是一回事，React 没有加任何魔法。

**要带多个数据时不要加第二第三个参数**，改成传一个对象，以后加字段不用管顺序：

```tsx
onDelete: (info: { id: number; reason: string }) => void;
// 调用：onDelete({ id: task.id, reason: '重复' })
```

### 第 3 级：子组件有自己的 state

子组件维护私有状态，点击后要先校验、再通知、最后收尾。

```tsx
function TaskCard({ task, onRename }: TaskCardProps) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(task.title);   // 输入中的内容归子组件

  function handleSave(): void {
    const next = draft.trim();
    if (next === '') return;   // ① 校验：子组件的职责，父组件不必操心
    onRename(next);            // ② 通知父组件
    setEditing(false);         // ③ 自己收尾
  }
  // ...
}
```

**职责划分是关键**：

| 谁的 | 什么数据 |
|---|---|
| 子组件的 state | 正在输入的草稿、是否处于编辑态 —— 过程性的、别人不关心的 |
| 父组件的 state | 保存后的最终标题 —— 结果性的、别人要用的 |

这两份 state 互不干扰，叫**状态封装**。

---

## 4.6 三级怎么选

从上往下问，第一个「是」就是答案：

1. 子组件除了「事情发生了」还要送数据上去吗？**不用** → 第 1 级
2. 子组件有自己的临时状态（输入中的内容、展开状态）吗？**没有** → 第 2 级
3. 以上都是「有」 → 第 3 级

**另一个更简单的判断**（决定要不要写中间函数）：

> 点击 = 单纯通知父组件 → 直接挂 `onClick={onToggle}`
> 点击 = 通知父组件 + 子组件自己还要干点别的 → 写中间函数 `handleClick`

---

## 4.7 命名约定

| 位置 | 前缀 | 例子 |
|---|---|---|
| 子组件的 props 名 | `on` | `onToggle` `onDelete` `onRename` |
| 父组件里的实现函数 | `handle` | `handleToggle` `handleDelete` |
| 子组件里的中间函数 | `handle` | `handleClick` `handleSave` |

纯社区约定，不是语法。三个名字各有分工，别混用。

---

## 💻 完整代码

`code/src/ch04/CallbackDemo.tsx` 把三级合在一个例子里。核心部分：

```tsx
type TaskCardProps = {
  task: Task;
  onToggle: () => void;                    // 第 1 级
  onDelete: (id: number) => void;          // 第 2 级
  onRename: (title: string) => void;       // 第 3 级
};

function TaskCard({ task, onToggle, onDelete, onRename }: TaskCardProps) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(task.title);

  function handleDraftChange(e: ChangeEvent<HTMLInputElement>): void {
    setDraft(e.target.value);
  }

  function handleSave(): void {
    const next = draft.trim();
    if (next === '') return;
    onRename(next);
    setEditing(false);
  }

  function handleCancel(): void {
    setDraft(task.title);
    setEditing(false);
  }

  if (editing) {
    return (
      <div className="card">
        <input value={draft} onChange={handleDraftChange} aria-label="任务标题" />
        <button type="button" onClick={handleSave}>保存</button>
        <button type="button" onClick={handleCancel}>取消</button>
      </div>
    );
  }

  return (
    <div className="card">
      <h3>{task.title}　<small>{task.done ? '已完成' : '进行中'}</small></h3>

      {/* 第 1 级：无需加工，直接挂 */}
      <button type="button" onClick={onToggle}>
        {task.done ? '标记未完成' : '标记完成'}
      </button>

      <button type="button" onClick={() => setEditing(true)}>改名</button>

      {/* 第 2 级：要传参，用箭头函数包一层 */}
      <button type="button" onClick={() => onDelete(task.id)}>删除</button>
    </div>
  );
}
```

父组件这一侧，注意**对象也要「返回新的」**：

```tsx
export default function CallbackDemo() {
  const [task, setTask] = useState<Task | null>(INITIAL_TASK);

  function handleToggle(): void {
    setTask((prev) => (prev === null ? prev : { ...prev, done: !prev.done }));
  }

  function handleDelete(id: number): void {
    setLog((prev) => [...prev, `收到子组件传上来的 id：${id}`]);
    setTask(null);
  }

  function handleRename(title: string): void {
    setTask((prev) => (prev === null ? prev : { ...prev, title }));
  }
  // ...
}
```

---

## ⚠️ 踩坑警告

### 坑 1：传回调时多写了括号

```tsx
<TaskCard onToggle={handleToggle()} />
```

```
error TS2322: Type 'void' is not assignable to type '() => void'.
```

加括号意味着「**立刻执行它，把返回值传下去**」。返回值是 `void`，所以报这个错。

```tsx
<TaskCard onToggle={handleToggle} />              // ✅ 传函数引用
<TaskCard onToggle={() => handleToggle('x')} />   // ✅ 需要预置参数时包一层
```

### 坑 2：带参数的回调直接挂给 `onClick`

```tsx
function TaskCard({ onDelete }: { onDelete: (id: number) => void }) {
  return <button type="button" onClick={onDelete} />;
}
```

```
error TS2322: Type '(id: number) => void' is not assignable to type
'MouseEventHandler<HTMLButtonElement>'.
  Types of parameters 'id' and 'event' are incompatible.
    Type 'MouseEvent<HTMLButtonElement, MouseEvent>' is not assignable to type 'number'.
```

报错说得很直白：**鼠标事件对象被当成 `id` 传进去了**。

这正是不用纯 JavaScript 的价值 —— 纯 JS 里 `id` 会悄悄变成一个事件对象，父组件收到后一脸茫然，只能在运行时靠打印才发现。

正确写法是包一层，自己控制实参：

```tsx
<button type="button" onClick={() => onDelete(task.id)} />
```

### 坑 3：可选回调没用可选调用

```tsx
type Props = { onDelete?: (id: number) => void };

onDelete(task.id);     // ❌ 父组件没传时：onDelete is not a function
onDelete?.(task.id);   // ✅
```

### 坑 4：改对象/数组而不是替换

```tsx
// ❌ 引用没变，React 认为没变化，不会重新渲染
setTask((prev) => { prev.done = !prev.done; return prev; });

// ✅ 返回一个新对象
setTask((prev) => ({ ...prev, done: !prev.done }));
```

数组同理：用 `[...prev, x]` 而不是 `prev.push(x)`。

### 坑 5：子组件试图直接改 props

```tsx
function TaskCard({ task }: TaskCardProps) {
  task.done = true;   // ❌ 不会重渲染，下次渲染被覆盖
}
```

想改父组件的数据，**只能通过回调请求父组件自己改**。这是整节的核心纪律。

### 坑 6：为了性能到处包 `useCallback`

以前需要担心「父组件每次渲染都新建 `handleToggle`，导致 `memo` 过的子组件白白重渲染」。**React 19 配合 React Compiler 后这类记忆化会自动处理。**

现阶段结论：**不要为了性能去包 `useCallback`，先把数据流写对。** 第 6 节会讲清楚什么时候才真的需要。

---

## 🎯 动手练习

1. 跑起 `4A · 回调最小形态`，点几次按钮，对着图 3 的编号在脑子里走一遍。
2. 把 `<TaskCard onToggle={handleToggle} />` 改成 `handleToggle()`，确认报错是坑 1 那句。
3. 给 `onDelete` 改成直接挂 `onClick={onDelete}`，确认报错是坑 2 那句。
4. 加一个第 2 级回调 `onPostpone: (days: number) => void`，点一下延期 3 天。
5. 把 `onRename` 改成必须先经过父组件确认：`onBeforeRename: () => boolean`，体会返回值不是 `void` 的写法。
6. 在 `handleToggle` 里改成 `prev.done = !prev.done; return prev;`，观察界面**不更新**，然后改回来。

---

## 📌 一句话总结

**回调 props 是父组件把自己的函数交给子组件、由子组件在合适的时机调用；装配（渲染时，父→子）和触发（点击时，子→父）是两根方向相反的线；要不要带参数、要不要中间函数，取决于子组件手里有没有父组件需要的东西。**

---

上一节：[第 3 节 children 与插槽](ch03-children与插槽.md)
下一节：第 5 节 受控组件与状态提升（待写）
