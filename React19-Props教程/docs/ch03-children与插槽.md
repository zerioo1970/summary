# 第 3 节 children 与插槽

> **本节目标**：学会把「一段内容」甚至「一段渲染逻辑」当 props 传进去。
>
> **配套代码**：`code/src/ch03/SlotsDemo.tsx`
>
> 前两节传的都是**数据**。这一节传的是**内容** —— 组件从此可以复用外壳、由父组件决定填什么。

---

## 3.1 `children`：写在标签之间的内容

写在开闭标签中间的东西，会作为一个名叫 `children` 的 props 传进去：

```tsx
<Card title="本周统计">
  <p>已完成 2 项</p>
</Card>
```

子组件这样接：

```tsx
import type { ReactNode } from 'react';

type CardProps = {
  title: string;
  children?: ReactNode;
};

function Card({ title, children }: CardProps) {
  return (
    <div className="card">
      <h3>{title}</h3>
      <div className="card-body">{children}</div>
    </div>
  );
}
```

`children` 是**普通 props，没有任何特殊待遇** —— 唯一的区别是它有语法糖：可以写在标签之间，而不用写成 `children={...}`。下面两种写法完全等价：

```tsx
<Card title="x"><p>内容</p></Card>
<Card title="x" children={<p>内容</p>} />   {/* 合法，但没人这么写 */}
```

**它解决什么问题**：`Card` 不需要知道自己里面装什么。同一个外壳，父组件今天放统计、明天放表单，组件本身不用改一行。

---

## 3.2 `ReactNode` 到底能装什么

我从 `@types/react` 19 的定义里翻出了完整清单：

```ts
type ReactNode =
  | ReactElement       // JSX 元素，如 <p>x</p>
  | string
  | number
  | bigint
  | Iterable<ReactNode>  // 数组等可迭代对象
  | ReactPortal
  | boolean
  | null
  | undefined
  | Promise<AwaitedReactNode>;   // React 19 新增
```

也就是说这些全都合法：

```tsx
<Card title="x">文字</Card>
<Card title="x">{42}</Card>
<Card title="x">{[<p key="1">一</p>, <p key="2">二</p>]}</Card>
<Card title="x">{null}</Card>
```

**渲染规则**（我用 `renderToStaticMarkup` 实测过）：

| 传进去的值 | 渲染结果 |
|---|---|
| `{null}` `{undefined}` `{false}` | 什么都不显示 |
| `{0}` | 显示 `0` ← **注意这个** |
| `{[1, 2]}` | 显示 `12`（数组会拼接） |

`0` 会被渲染出来，而 `false` 不会 —— 这个不对称是 3.6 节那个经典坑的根源。

### 三个容易混的类型

| 类型 | 含义 | 什么时候用 |
|---|---|---|
| `ReactNode` | 任何能渲染的东西 | **99% 的情况**，插槽和 children 都用它 |
| `ReactElement` | 必须是一个 JSX 元素 | 想禁止父组件传字符串时 |
| `JSX.Element` | 基本等同 `ReactElement` | 老代码里常见，新代码别用 |

**记一条就够**：插槽类型写 `ReactNode`。

---

## 3.3 🖼 图解：父组件写的东西落在哪

```
图 4　children 与插槽：父组件写的东西，落在子组件的哪个位置

    +----------------------------------------------+
    | [icon]   写周报                              |   <-- icon 插槽 + title
    +----------------------------------------------+
    |                                              |
    | 今天下班前给我                               |   <-- children
    |                                              |
    +----------------------------------------------+
    | [ 删除 ]                                     |   <-- footer 插槽
    +----------------------------------------------+

父组件写的                       落在子组件的
-----------------------------    ---------------
icon={<Star />}                  标题左边
title="写周报"                   标题位置
<Card> 今天下班前给我 </Card>    children 的位置
footer={<IconButton />}          卡片底部
```

彩色矢量版：[assets/fig4-slots.svg](assets/fig4-slots.svg)（在 GitHub 上点开可看）

**注意 `footer` 在 JSX 里写在 `children` 上面，渲染出来却在下面** —— props 的书写顺序和它出现的位置**毫无关系**，位置由子组件的模板决定。

---

## 3.4 多个插槽：一个组件开几个口子

`children` 只有一个。想在多个位置让父组件填东西，就多开几个 `ReactNode` 类型的 props：

```tsx
type CardProps = {
  title: string;
  icon?: ReactNode;      // 插槽 1：标题左边
  footer?: ReactNode;    // 插槽 2：卡片底部
  children?: ReactNode;  // 主体内容
};

function Card({ title, icon, footer, children }: CardProps) {
  return (
    <div className="card">
      <header className="card-head">
        {icon}
        <h3>{title}</h3>
      </header>

      <div className="card-body">{children}</div>

      {footer !== undefined && <footer className="card-foot">{footer}</footer>}
    </div>
  );
}
```

用起来：

```tsx
<Card
  title="写周报"
  icon={<span aria-hidden="true">★</span>}
  footer={<IconButton icon="删除" />}
>
  <p>今天下班前给我</p>
</Card>
```

**插槽名就是普通 props 名**，叫 `icon`、`header`、`extra`、`actions` 都行 —— 没有任何框架层面的规定。

---

## 3.5 插槽还是 children？

| 判断 | 用哪个 |
|---|---|
| 这是组件的**主体内容** | `children` |
| 这是某个**特定位置**的装饰或操作区 | 具名插槽 |
| 有**两处以上**要父组件填 | 至少有一处必须是具名插槽 |

**经验做法**：主体用 `children`，其余用具名插槽。全都用具名插槽也能跑，但父组件写起来啰嗦（内容得写成 `content={...}`）。

---

## 3.6 render prop：把「怎么渲染」交出去

插槽有个限制：**父组件写内容的时候，拿不到子组件手里的数据**。

比如「根据剩余天数显示紧急标签」—— 天数在子组件的 `task` 里，父组件写 `tag={...}` 时看不见它。

解法是传**函数**，而不是传内容：

```tsx
type TaskCardProps = {
  task: Task;
  renderTag?: (task: Task) => ReactNode;   // ← 给你数据，你告诉我渲染什么
  children?: ReactNode;
};

function TaskCard({ task, renderTag, children }: TaskCardProps) {
  return (
    <Card title={task.title}>
      {renderTag?.(task)}    {/* 可选调用：没传就什么都不渲染 */}
      {children}
    </Card>
  );
}
```

父组件用的时候，函数参数里就拿到了 `task`：

```tsx
<TaskCard
  task={task}
  renderTag={(t) => <span className="tag">{t.days <= 1 ? '紧急' : '正常'}</span>}
>
  <p>今天下班前给我</p>
</TaskCard>
```

### 插槽 vs render prop，一句话区分

> **插槽传的是「内容」，render prop 传的是「怎么根据我的数据生成内容」。**

**判断标准**：父组件写这段 JSX 时，需不需要用到只有子组件才有的数据？需要 → render prop；不需要 → 插槽。

---

## 💻 完整代码

`code/src/ch03/SlotsDemo.tsx`：

```tsx
import type { ReactNode } from 'react';

type Task = {
  id: number;
  title: string;
  days: number;
  done: boolean;
};

type CardProps = {
  title: string;
  icon?: ReactNode;
  footer?: ReactNode;
  children?: ReactNode;
};

export function Card({ title, icon, footer, children }: CardProps) {
  return (
    <div className="card">
      <header className="card-head">
        {icon}
        <h3>{title}</h3>
      </header>

      <div className="card-body">{children}</div>

      {/* 用 !== undefined 判断，避免 footer 传 0 时整块消失 */}
      {footer !== undefined && <footer className="card-foot">{footer}</footer>}
    </div>
  );
}

type TaskCardProps = {
  task: Task;
  renderTag?: (task: Task) => ReactNode;
  children?: ReactNode;
};

export function TaskCard({ task, renderTag, children }: TaskCardProps) {
  return (
    <Card
      title={task.title}
      icon={<span aria-hidden="true">★</span>}
      footer={<small>剩余 {task.days} 天</small>}
    >
      {renderTag?.(task)}
      {children}
    </Card>
  );
}
```

---

## ⚠️ 踩坑警告

### 坑 1：`&&` 条件渲染遇到数字 `0`

这是 React 最经典的坑，我实测确认了行为：

```tsx
{footer && <footer>{footer}</footer>}
```

| `footer` 的值 | 结果 |
|---|---|
| `undefined` / `null` / `''` | 正常，什么都不渲染 |
| `0` | **页面上出现一个孤零零的 `0`**，外面的 `<footer>` 没了 |

原因：`0 && <footer>...</footer>` 的结果是 `0`，而 React 会把数字 `0` 渲染出来（`false` 却不会）。

**三种解法**：

```tsx
{footer !== undefined && <footer>{footer}</footer>}   // ✅ 明确判断
{footer != null && <footer>{footer}</footer>}          // ✅ 同时排除 null 和 undefined
{footer ? <footer>{footer}</footer> : null}            // ✅ 三元
```

**养成习惯**：`&&` 左边只放**明确的布尔表达式**，不要直接放一个可能是数字的值。

### 坑 2：数组当 children 忘了 `key`

```tsx
<Card title="x">{[<p>一</p>, <p>二</p>]}</Card>
```

浏览器控制台会警告 `Each child in a list should have a unique "key" prop`。数组形式的 children 和 `map` 渲染一样需要 `key`。

### 坑 3：把「组件」和「元素」搞混

```tsx
icon={<Star />}    // ✅ 元素：已经调用好了，类型是 ReactNode
icon={Star}        // ❌ 组件本身：类型是函数，不是 ReactNode
```

传 `Star`（不带尖括号）时报错是 `Type '() => Element' is not assignable to type 'ReactNode'`。

**想传组件本身**（让子组件自己决定渲染几次）要写成：

```tsx
type Props = { Icon?: React.ComponentType };   // 注意首字母大写
function C({ Icon }: Props) {
  return <div>{Icon !== undefined && <Icon />}</div>;
}
```

这属于进阶用法，日常传元素就够。

### 坑 4：`children` 设成必填，然后忘了传

```tsx
type CardProps = { children: ReactNode };   // 没有问号 = 必填

<Card />
```

```
error TS2741: Property 'children' is missing in type '{}'
but required in type 'CardProps'.
```

**建议**：外壳类组件（`Card`、`Modal`）的 `children` 设成**必填**是好事 —— 一个空壳没有意义，让编译器提醒你。

### 坑 5：render prop 每次渲染都是新函数

```tsx
<TaskCard renderTag={(t) => <span>{t.days}</span>} />
```

父组件每次渲染都会新建这个箭头函数。**在 React 19 + React Compiler 下这不需要你操心**，不要为此去包 `useCallback` —— 第 6 节会讲清楚什么时候才真的需要。

---

## 🎯 动手练习

1. 给 `Card` 加第三个插槽 `extra?: ReactNode`，渲染在标题右边。
2. 把 `footer` 传成数字 `0`，观察坑 1 的现象，再改成 `!== undefined` 修好。
3. 把 `children` 改成必填，看哪些调用报错。
4. 写一个 `renderFooter?: (task: Task) => ReactNode`，让父组件根据 `done` 决定底部显示什么。
5. 试着传 `icon={Star}`（不带尖括号），看报错原文。

---

## 📌 一句话总结

**children 是主体内容的插槽，具名插槽用于特定位置，两者类型都写 `ReactNode`；当父组件需要用到子组件的数据才能决定渲染什么时，改用 render prop。**

---

上一节：[第 2 节 用类型把 props 管住](ch02-用类型管住props.md)
下一节：[第 4 节 回调 props](ch04-回调props.md) —— 本教程的重点，消息怎么从子组件送回父组件。
