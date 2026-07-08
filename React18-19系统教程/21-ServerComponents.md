# 第二十一章 · Server Components 与 Server Actions（概念入门）

> 本文是《React 18 & 19 系统教程》的第 21 章。完整目录见 [README](README.md)。

> React 19 正式纳入了 **React Server Components（RSC）** 相关能力。它们主要面向 **Next.js 等全栈框架**，普通的纯客户端（Vite + React）项目**用不到、也无需配置**。这里做概念性介绍，帮你看懂相关文章和框架文档。

### （A）Server Components

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 313：一个 Server Component 的样子</h3>

```jsx
// app/page.jsx —— 默认就是 Server Component（在框架里）
// 它在"服务器/构建期"运行，可以直接读数据库、读文件，不会打包进客户端 JS
async function Page() {
  const posts = await db.posts.findMany(); // 直接访问服务端资源
  return (
    <main>
      {posts.map((p) => (
        <article key={p.id}>{p.title}</article>
      ))}
    </main>
  );
}

export default Page;
```

**详解**：Server Component 是一种**在打包之前、在独立于客户端的环境里提前渲染**的组件——这个环境就是 RSC 里的"server"（可以是 CI 构建期，也可以是每次请求时的 Web 服务器）。它的代码**不会进客户端 bundle**，因此可以直接访问数据库、文件系统等服务端资源，还能减小前端体积。**注意：Server Component 没有专门的指令**——它是框架里的默认形态。

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 314："use client" 指令——标记客户端组件</h3>

```jsx
'use client'; // 文件顶部：这是客户端组件，可用 useState、事件、浏览器 API

import { useState } from 'react';

export default function Counter() {
  const [n, setN] = useState(0);
  return <button onClick={() => setN(n + 1)}>点了 {n} 次</button>;
}
```

**详解**：需要交互（`useState`、事件、`useEffect`、访问 `window` 等）的组件，要在文件顶部加 `'use client'`，把它标记为**客户端组件**。Server Component 里可以引入并渲染客户端组件，从而组成"服务端负责取数、客户端负责交互"的架构。

### （B）Server Actions

<br>
<h3 style="color: #FF8C00; font-size: 1.6em;">示例 315："use server" —— 客户端调用服务端函数</h3>

```jsx
// actions.js
'use server'; // 标记：以下导出的是 Server Action，实际在服务器执行

export async function createTodo(formData) {
  const text = formData.get('text');
  await db.todos.create({ text }); // 服务端逻辑
}
```

```jsx
// TodoForm.jsx（客户端组件）
'use client';
import { createTodo } from './actions';

export default function TodoForm() {
  // 直接把 Server Action 传给 <form action>
  return (
    <form action={createTodo}>
      <input name="text" />
      <button type="submit">添加</button>
    </form>
  );
}
```

**详解**：`'use server'` 指令用于声明 **Server Action**——它让**客户端组件可以调用在服务器上执行的异步函数**。框架会自动创建一个"函数引用"传给客户端；客户端调用时，React 发请求到服务器执行、再把结果返回。它天然与第十七章的 Actions（`<form action>`、`useActionState`）配合。

> **常见误解澄清**：`'use server'` **不是**用来标记 Server Component 的！它标记的是 **Server Action**。Server Component 没有指令。

---

---
[← 上一章](20-文档元数据与资源预加载.md) · [📖 目录](README.md) · [下一章 →](22-其它改进与破坏性变更.md)
