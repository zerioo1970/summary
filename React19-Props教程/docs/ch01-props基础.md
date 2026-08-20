# 第 1 节 props 的基本使用

> **本节目标**：读完能独立写出一个带 props 的组件，并知道每种类型怎么传。
>
> **配套代码**：`code/src/ch01/TaskCardDemo.tsx`
>
> 本节把「传值」这件事一次讲完 —— 类型定义、怎么传、怎么收、可选与默认值、只读性。不拆成五小节让你翻来翻去。

---

## 1.1 组件就是函数，props 就是它的参数

React 组件本质上是一个函数：**进去的是 props，出来的是界面**。

```tsx
function TaskCard(props: { title: string }) {
  return <h3>{props.title}</h3>;
}
```

调用它的时候写成标签的样子：

```tsx
<TaskCard title="写周报" />
```

React 在背后做的事，等价于这样调用一次函数：

```tsx
TaskCard({ title: '写周报' });
```

**关键理解**：你在标签上写的每个属性，都变成了那个参数对象里的一个字段。props 永远只有**一个**参数，它是个对象 —— 所以不存在「第二个 props」这种说法。

---

## 1.2 🖼 图解：props 单向流

数据往下流，事件往上走。整套 props 机制就这两个方向。

```
图 1　props 单向流 —— 数据往下，事件往上

        +----------------------------------------+
        | 父组件 App                             |
        | state 住在这里，它是数据的主人         |
        +----------------------------------------+
             |                          ^
  props 数据 |                          | 回调 props 事件
  title/days |                          | onToggle/onDelete
  只读、往下 v                          | 通知、往上
        +----------------------------------------+
        | 子组件 TaskCard                        |
        | 只能读 props，永远不能改它             |
        +----------------------------------------+

两条通道共用同一个 props 对象：普通值往下送数据，函数往上送消息。
```

彩色矢量版：[assets/fig1-flow.svg](assets/fig1-flow.svg)（在 GitHub 上点开可看）

本节只讲**往下**那条。往上那条（回调 props）是第 4 节的主题。

---

## 1.3 定义 props 的类型

不要把类型直接写在参数里，单独定义一个类型，可读性和复用性都更好：

```tsx
type TaskCardProps = {
  title: string;     // 必填
  days: number;      // 必填
  done: boolean;     // 必填
  owner?: string;    // 可选，问号表示父组件可以不传
};
```

这份类型就是**父子之间的契约**：父组件必须传什么、子组件能拿到什么，双方都受它约束。

### `type` 还是 `interface`？

两者在定义 props 时几乎等价。本教程统一用 `type`，理由是它能写联合类型（第 2 节要用），而 `interface` 不能：

```tsx
// 只有 type 能这么写，第 2 节的判别联合全靠它
type CardProps = { mode: 'view' } | { mode: 'edit'; draft: string };
```

**命名约定**：组件名 + `Props`，即 `TaskCardProps`。不是语法要求，但社区一致这么写。

---

## 1.4 父组件怎么传：引号与花括号

这是新手最容易含糊的地方，规则其实只有一条：

> **除了字符串字面量，其他一切都要用花括号。**

```tsx
<TaskCard
  title="写周报"        {/* 字符串：引号 */}
  days={3}              {/* 数字：必须花括号 */}
  done={false}          {/* 布尔：必须花括号 */}
  owner={ownerName}     {/* 变量：必须花括号 */}
/>
```

花括号的含义是「**这里面是 JavaScript 表达式**」。所以 `days={3}` 传的是数字 `3`，而 `days="3"` 传的是字符串 `"3"` —— 后者会被 TypeScript 拦住。

### 布尔值的简写

```tsx
<TaskCard title="改预算表" days={1} done />        {/* done 等价于 done={true} */}
<TaskCard title="改预算表" days={1} done={false} /> {/* false 必须写全 */}
```

只有 `true` 能简写。这也是为什么 `<TaskCard title days={1} />` 会报错 —— 简写意味着 `title={true}`，而 `title` 要的是 `string`。

---

## 1.5 子组件怎么收：解构还是 `props.x`

两种写法都对，各有适用场景：

```tsx
// 写法一：解构（推荐，本教程默认用它）
function TaskCard({ title, days, done }: TaskCardProps) {
  return <h3>{title}</h3>;
}

// 写法二：保留 props 对象
function TaskCard(props: TaskCardProps) {
  return <h3>{props.title}</h3>;
}
```

**解构更常用**，因为函数体里少写一层 `props.`，而且一眼能看出这个组件用到了哪几个字段。

**什么时候必须用写法二**：需要靠某个字段来收窄类型的时候（第 2 节的判别联合），解构会破坏 TypeScript 的收窄能力。到那节会具体说。

---

## 1.6 可选 props 与默认值

问号让 props 变成可选，默认值直接写在解构里：

```tsx
type TaskCardProps = {
  title: string;
  owner?: string;      // 可选：类型是 string | undefined
};

function TaskCard({ title, owner = '未指派' }: TaskCardProps) {
  return <p>{title}：{owner}</p>;
}
```

`owner = '未指派'` 是**函数默认参数**，父组件不传或传 `undefined` 时生效。

> ⚠️ **不要用 `defaultProps`**。那是类组件时代的东西，React 19 已对函数组件移除支持，用默认参数就是现在的标准做法。

### 可选和「可以传 undefined」是两件事

```tsx
owner?: string;              // 可以不传
owner: string | undefined;   // 必须传，但可以传 undefined
```

第二种写法要求父组件显式写 `owner={undefined}`，漏写就报错。日常用第一种。

---

## 1.7 props 是只读的 —— 但 TypeScript 默认不会拦你

这一点我实测过，结果和很多教程说的不一样，所以单独讲。

**先说规矩**：子组件永远不能修改 props。它是父组件的东西，子组件只有读的权利。

**但是**，下面这段代码 `tsc --noEmit` **不报任何错**：

```tsx
function TaskCard(props: TaskCardProps) {
  props.title = '我改了';   // ← TypeScript 默认不管你
  return <h3>{props.title}</h3>;
}
```

它不报错，但它是**错的** —— 因为：

1. 改了不会触发重新渲染，界面不会更新；
2. 父组件下次渲染会把你的修改覆盖掉；
3. 数据的真实来源被搞乱了，出 bug 极难查。

### 想让编译器帮你守住，包一层 `Readonly`

```tsx
function TaskCard(props: Readonly<TaskCardProps>) {
  props.title = '我改了';
  //    ~~~~~ error TS2540: Cannot assign to 'title'
  //          because it is a read-only property.
  return null;
}
```

或者字段级：

```tsx
type TaskCardProps = {
  readonly title: string;
};
```

**实用建议**：日常不必到处包 `Readonly` —— 只要养成「props 只读」的习惯就够了。但如果团队里有人反复犯这个错，给 props 类型加 `Readonly` 是最直接的拦法。

---

## 💻 完整代码

`code/src/ch01/TaskCardDemo.tsx`（可直接运行）：

```tsx
// props 的类型：一份父子之间的契约
type TaskCardProps = {
  title: string;     // 必填
  days: number;      // 必填
  done: boolean;     // 必填
  owner?: string;    // 可选，问号表示父组件可以不传
};

// 参数位置直接解构，owner 用默认参数兜底
function TaskCard({ title, days, done, owner = '未指派' }: TaskCardProps) {
  return (
    <div className="card">
      <h3>{title}</h3>
      <p>负责人：{owner}</p>
      <p>剩余 {days} 天</p>
      <p>状态：{done ? '已完成' : '进行中'}</p>
    </div>
  );
}

export default function TaskCardDemo() {
  return (
    <section>
      <h2>第 1 节 · props 的基本使用</h2>

      {/* 字符串可以用引号，其余类型一律用花括号 */}
      <TaskCard title="写周报" days={3} done={false} />

      {/* done 是布尔简写，等价于 done={true}；owner 传了就覆盖默认值 */}
      <TaskCard title="改预算表" days={1} done owner="老王" />

      {/* owner 不传，显示默认的「未指派」 */}
      <TaskCard title="核对发票" days={7} done={false} />
    </section>
  );
}
```

### 🔍 三个细节

**`{done ? '已完成' : '进行中'}`** —— JSX 里不能写 `if`，条件渲染用三元表达式。

**`{title}`** —— 花括号在 JSX 内容位置的含义是「把这个值渲染出来」，和属性位置的花括号是同一套规则：里面是 JavaScript 表达式。

**`className` 而不是 `class`** —— `class` 是 JavaScript 保留字，React 用 `className` 代替。

---

## ⚠️ 踩坑警告

下面每条报错都是在 **React 19.2 + TypeScript 5.9 + strict** 下实跑得到的原文。

### 坑 1：漏传必填 props

```tsx
<TaskCard title="x" days={1} />
```

```
error TS2741: Property 'done' is missing in type
'{ title: string; days: number; }' but required in type 'TaskCardProps'.
```

**这正是写类型的价值** —— 漏传在编译期就被抓住，不用等到页面上显示 `undefined`。

### 坑 2：数字传成了字符串

```tsx
<TaskCard title="x" days="3" done={false} />
```

```
error TS2322: Type 'string' is not assignable to type 'number'.
```

**记住**：`days="3"` 传字符串，`days={3}` 传数字。

### 坑 3：传了类型里没有的 props

```tsx
<TaskCard title="x" days={1} done={false} color="red" />
```

```
error TS2322: Property 'color' does not exist on type
'IntrinsicAttributes & TaskCardProps'.
```

多传的 props 不会被「悄悄忽略」，TypeScript 直接拒绝。

### 坑 4：props 名字拼错

```tsx
<TaskCard title="x" day={1} done={false} />
```

```
error TS2322: Property 'day' does not exist on type ...
  Did you mean 'days'?
```

编译器连拼写建议都给了。纯 JavaScript 里这个错误会静默通过，子组件拿到 `undefined`，页面上出现 `剩余 undefined 天`。

### 坑 5：布尔简写用在了非布尔字段上

```tsx
<TaskCard title days={1} done />
```

```
error TS2322: Type 'boolean' is not assignable to type 'string'.
```

`title` 简写成了 `title={true}`，而它要的是字符串。

### 坑 6：在子组件里改 props

如 1.7 所说，**默认不报错**，但确实是错的。想让编译器拦住就用 `Readonly<TaskCardProps>`，报错是 `TS2540`。

---

## 🎯 动手练习

1. 给 `TaskCard` 加一个可选 props `tag?: string`，默认值 `'普通'`，渲染在标题右边。
2. 故意漏传 `days`，看看报错信息是不是坑 1 那句。
3. 把 `days={3}` 改成 `days="3"`，确认报错是坑 2 那句。
4. 把 `owner` 的问号去掉，看哪几处调用会报错，想清楚为什么。
5. 把 props 类型改成 `Readonly<TaskCardProps>`，在函数体里写 `props.title = 'x'`，确认出现 `TS2540`。

---

## 📌 一句话总结

**props 是父组件传给子组件的唯一参数，它是个对象、是只读的；除了字符串字面量，传值一律用花括号。**

---

下一节：[第 2 节 用类型把 props 管住](ch02-用类型管住props.md) —— 让非法的 props 组合根本编译不过去。
