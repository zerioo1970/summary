# 第 2 节 用类型把 props 管住

> **本节目标**：把 props 从「能传」变成「只能传对的」。让非法的组合根本编译不过去。
>
> **配套代码**：`code/src/ch02/TypedPropsDemo.tsx`
>
> 第 1 节的类型只解决了「字段名和基本类型对不对」。这一节解决更值钱的问题：**取值范围、字段之间的依赖关系、以及怎么复用原生元素的属性**。

---

## 2.1 联合类型：让取值只能是几个之一

`string` 太宽松了 —— 优先级写成 `'高'`、`'HIGH'`、`'紧急'` 都能通过，然后你的 `if` 判断就漏了。

```tsx
// ❌ 松：任何字符串都行
type TaskCardProps = { priority: string };

// ✅ 紧：只能是这三个之一
type Priority = 'low' | 'normal' | 'high';
type TaskCardProps = { priority: Priority };
```

传错值时：

```tsx
<TaskCard priority="urgent" />
```

```
error TS2322: Type '"urgent"' is not assignable to type 'Priority'.
```

而且编辑器会**自动提示这三个值**，不用回头翻代码。

**什么时候用**：状态、模式、尺寸、颜色主题 —— 凡是「就那么几种」的，都别用 `string`。

---

## 2.2 `Record` 映射表：以后加了值，编译器提醒你

拿到 `priority` 要显示中文，第一反应可能是写 `if / else` 或 `switch`。更好的写法是映射表：

```tsx
const PRIORITY_LABEL: Record<Priority, string> = {
  low: '低',
  normal: '中',
  high: '高',
};

// 用的时候一行
<p>优先级：{PRIORITY_LABEL[task.priority]}</p>
```

`Record<Priority, string>` 的意思是「键必须是 `Priority` 的每一个值，值是字符串」。**少写一个键就报错**：

```
error TS2741: Property 'high' is missing in type '{ low: string; normal: string; }'
but required in type 'Record<Priority, string>'.
```

**这才是真正的好处**：将来给 `Priority` 加一个 `'urgent'`，所有映射表会立刻集体报错，告诉你哪些地方要补。用 `if / else` 就不会有人提醒你。

---

## 2.3 传对象：一次传一整个 task

props 一个个列，字段多了就啰嗦：

```tsx
// 太啰嗦，而且加字段要改三个地方
<TaskCard title={t.title} days={t.days} done={t.done} priority={t.priority} />
```

直接传对象：

```tsx
export type Task = {
  id: number;
  title: string;
  days: number;
  done: boolean;
  priority: Priority;
};

type TaskCardProps = { task: Task };

<TaskCard task={t} />
```

### 什么时候传对象、什么时候拆开传

| | 传对象 `task={t}` | 拆开传 `title=... days=...` |
|---|---|---|
| 字段多、来自同一数据源 | ✅ | 啰嗦 |
| 组件想通用（不绑定业务类型） | 绑死了 `Task` | ✅ 更灵活 |
| 只用其中一两个字段 | 传多了没必要 | ✅ |

**经验判断**：业务组件（`TaskCard`）传对象；通用组件（`Card`、`Button`）拆开传。

---

## 2.4 传数组：加上 `readonly`

```tsx
type TaskListProps = {
  tasks: readonly Task[];
};
```

`readonly` 让子组件**在类型层面就没有**修改方法：

```tsx
function TaskList({ tasks }: TaskListProps) {
  tasks.push({ id: 9 });
}
```

```
error TS2339: Property 'push' does not exist on type 'readonly Task[]'.
```

注意报错措辞是「不存在」而不是「不允许」—— `readonly Task[]` 这个类型上真的没有 `push`、`sort`、`splice`。这比第 1 节那个「改 props 不报错」的情况强得多，所以**数组和对象类型的 props 加 `readonly` 是很值的**。

### 渲染列表：`key` 不是 props

```tsx
{tasks.map((task) => (
  <TaskCard key={task.id} task={task} />
))}
```

`key` 长得像 props，但它**不是** —— 子组件里拿不到 `key`，写进 `TaskCardProps` 也没用。它是给 React 用的身份标识，用来判断列表项是「同一个还是换了一个」。

**用什么当 key**：数据的稳定 id。用数组下标 `index` 在列表会增删排序时会出问题（React 会认错人，导致输入框内容错位）。

---

## 2.5 判别联合：让非法组合无法编译

这是本节最值钱的一节。

**场景**：卡片有两种模式。`view` 只显示，`edit` 要额外传一个草稿文本 `draft`。

新手写法是把 `draft` 设成可选：

```tsx
// ❌ 松：编译器管不住
type TaskCardProps = {
  mode: 'view' | 'edit';
  task: Task;
  draft?: string;
};
```

问题在于：`mode="edit"` 却漏传 `draft`，编译器不管；`mode="view"` 传了 `draft`，编译器也不管。**字段之间的依赖关系没有被表达出来。**

### 正确写法：两种形状的联合

```tsx
type TaskCardBase = {
  task: Task;
  owner?: string;
};

type TaskCardProps =
  | (TaskCardBase & { mode: 'view' })
  | (TaskCardBase & { mode: 'edit'; draft: string });
```

`mode` 是**判别字段**（discriminant）—— 编译器靠它知道当前是哪一种形状。

现在两种非法写法都会被拦住：

```tsx
<TaskCard mode="view" task={t} draft="x" />
```

```
error TS2322: Property 'draft' does not exist on type
'IntrinsicAttributes & { mode: "view"; task: Task; owner?: string; }'.
```

```tsx
<TaskCard mode="edit" task={t} />
```

```
error TS2322: Property 'draft' is missing in type '{ mode: "edit"; task: Task; }'
but required in type '{ mode: "edit"; task: Task; draft: string; }'.
```

### 子组件里必须先判断，才能访问

```tsx
export function TaskCard(props: TaskCardProps) {
  const { task, owner = '未指派' } = props;   // 公共字段可以先解构

  return (
    <div>
      <h3>{task.title}</h3>
      {props.mode === 'edit' ? (
        <input defaultValue={props.draft} />   {/* 判断过 mode，这里才允许访问 draft */}
      ) : (
        <p>剩余 {task.days} 天</p>
      )}
    </div>
  );
}
```

不判断就访问：

```tsx
return <input defaultValue={props.draft} />;
```

```
error TS2339: Property 'draft' does not exist on type 'TaskCardProps'.
  Property 'draft' does not exist on type '{ mode: "view"; task: Task; }'.
```

报错说得很清楚：**因为可能是 view 那一支，而那一支没有这个字段。**

### ⚠️ 这就是第 1 节说的「有时不能解构」

```tsx
// ❌ 一次性全解构，收窄能力就没了
function TaskCard({ mode, task, draft }: TaskCardProps) { ... }
```

把 `mode` 和 `draft` 拆成两个独立变量之后，编译器无法再把「`mode` 的值」和「`draft` 存不存在」联系起来。

**规矩**：用了判别联合，就保留 `props` 对象，靠 `props.mode === '...'` 收窄。公共字段可以单独解构出来，像上面那样。

---

## 2.6 继承原生属性：`ComponentProps` + `Omit` + spread

**场景**：写一个 `IconButton`，除了自己的 `icon`，还希望父组件能用 `<button>` 的全部原生属性 —— `disabled`、`title`、`onClick`、`aria-label`……

一个个抄一遍是笨办法。正确做法：

```tsx
import type { ComponentProps } from 'react';

type IconButtonProps = {
  icon: string;
} & Omit<ComponentProps<'button'>, 'children'>;

export function IconButton({ icon, ...rest }: IconButtonProps) {
  return (
    <button type="button" {...rest}>
      {icon}
    </button>
  );
}
```

三个部件各干什么：

| 部件 | 作用 |
|---|---|
| `ComponentProps<'button'>` | 拿到原生 `<button>` 的全部 props 类型 |
| `Omit<..., 'children'>` | 去掉 `children`，因为内容由 `icon` 决定 |
| `{ icon, ...rest }` + `{...rest}` | 自己用掉 `icon`，其余原封不动透传 |

用起来：

```tsx
<IconButton icon="删除" title="删除这条任务" disabled={task.done} />
```

`title` 和 `disabled` 从没在 `IconButtonProps` 里定义过，但它们能用、有类型提示、拼错会报错。

### ⚠️ `{...rest}` 的位置决定谁覆盖谁

```tsx
<button type="button" {...rest} />   // 父组件能覆盖 type
<button {...rest} type="button" />   // 父组件永远覆盖不了 type
```

后写的赢。想给「可被覆盖的默认值」，就把默认值写在 `{...rest}` **前面**。

---

## 💻 完整代码

见 `code/src/ch02/TypedPropsDemo.tsx`。核心结构：

```tsx
import type { ComponentProps } from 'react';

export type Priority = 'low' | 'normal' | 'high';

export type Task = {
  id: number;
  title: string;
  days: number;
  done: boolean;
  priority: Priority;
};

const PRIORITY_LABEL: Record<Priority, string> = {
  low: '低',
  normal: '中',
  high: '高',
};

type IconButtonProps = {
  icon: string;
} & Omit<ComponentProps<'button'>, 'children'>;

export function IconButton({ icon, ...rest }: IconButtonProps) {
  return (
    <button type="button" {...rest}>
      {icon}
    </button>
  );
}

type TaskCardBase = {
  task: Task;
  owner?: string;
};

export type TaskCardProps =
  | (TaskCardBase & { mode: 'view' })
  | (TaskCardBase & { mode: 'edit'; draft: string });

export function TaskCard(props: TaskCardProps) {
  const { task, owner = '未指派' } = props;

  return (
    <div className="card">
      <h3>{task.title}</h3>
      <p>
        优先级：{PRIORITY_LABEL[task.priority]}｜负责人：{owner}
      </p>

      {props.mode === 'edit' ? (
        <input defaultValue={props.draft} aria-label="标题草稿" />
      ) : (
        <p>
          剩余 {task.days} 天｜{task.done ? '已完成' : '进行中'}
        </p>
      )}

      <IconButton icon="删除" title="删除这条任务" disabled={task.done} />
    </div>
  );
}

type TaskListProps = {
  tasks: readonly Task[];
};

export function TaskList({ tasks }: TaskListProps) {
  return (
    <div>
      {tasks.map((task) => (
        <TaskCard key={task.id} mode="view" task={task} />
      ))}
    </div>
  );
}
```

---

## ⚠️ 踩坑警告

### 坑 1：用 `object` 或 `any` 当对象类型

```tsx
type TaskCardProps = { task: object };   // ❌ 等于没写，访问 task.title 会报错
type TaskCardProps = { task: any };      // ❌ 彻底放弃类型检查
```

写出具体形状。真的通用就用泛型（第 6 节）。

### 坑 2：可选 props 直接当必填用

```tsx
type Props = { owner?: string };

function C({ owner }: Props) {
  return <p>{owner.length}</p>;
  //          ~~~~~ error TS18048: 'owner' is possibly 'undefined'.
}
```

三种解法：给默认值 `owner = '未指派'`、可选链 `owner?.length`、或者干脆改成必填。

### 坑 3：把可选字段当成依赖关系来表达

就是 2.5 说的那个 —— `draft?: string` 表达不了「edit 模式必须有」。**用判别联合。**

### 坑 4：用了判别联合还全解构

收窄能力会失效，报错是 `TS2339 Property 'draft' does not exist on type ...`。保留 `props` 对象。

### 坑 5：`ComponentProps` 忘了 `Omit` 掉 `children`

这条我实测过，**它不报错，但会静默丢数据**：

```tsx
type IconProps = { icon: string } & ComponentProps<'button'>;   // 没有 Omit

<Icon icon="删" title="删除">这段文字会被静默丢掉</Icon>
```

因为 `ComponentProps<'button'>` 里带着 `children`，所以父组件写 children **合法**；但组件内部 `<button {...rest}>{icon}</button>` 里的 `{icon}` 已经占了 children 的位置，父组件传的内容被覆盖 —— **没有任何报错，内容凭空消失**。

**这类「静默丢失」比报错危险得多**，所以内容位置已被占用的组件，务必 `Omit<..., 'children'>`。

### 坑 6：用数组下标当 `key`

```tsx
{tasks.map((task, index) => <TaskCard key={index} task={task} />)}
```

列表只增不减时看不出问题；一旦中间删除或排序，React 会认错人 —— 表现是**输入框里的内容跑到了别的行**。用 `task.id`。

---

## 🎯 动手练习

1. 给 `Priority` 加一个 `'urgent'`，看 `PRIORITY_LABEL` 报什么错，然后补上。
2. 写 `<TaskCard mode="view" task={t} draft="x" />`，确认报错和 2.5 那句一致。
3. 在 `TaskCard` 里改成一次性全解构 `{ mode, task, draft }`，看收窄怎么失效的。
4. 给 `IconButton` 传一个 `onClick`，确认它能正常工作（说明 spread 透传成功了）。
5. 把 `IconButton` 的 `Omit<..., 'children'>` 去掉，然后写 `<IconButton icon="删">文字</IconButton>`，确认文字**真的消失了而且没报错**。

---

## 📌 一句话总结

**能枚举的用联合类型，字段之间有依赖的用判别联合，包装原生元素的用 `ComponentProps` + `Omit` + spread —— 让编译器替你守住 props 的合法性。**

---

上一节：[第 1 节 props 的基本使用](ch01-props基础.md)
下一节：[第 3 节 children 与插槽](ch03-children与插槽.md) —— 怎么把「一段内容」当 props 传进去。
