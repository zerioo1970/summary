# React 19 props 完全教程（TypeScript 版）

> 版本基线：`react 19.2` + `typescript 5.9` + `vite 7`｜全篇 TypeScript，`strict` 开启，**零 `any`**
>
> 一个例子贯穿全程：**一张任务卡片 → 一份任务清单**。每节只加一个主题，代码只增不重写。
>
> 每节固定六件套：🖼 图解 · 💻 完整可运行代码 · 🔍 类型要点 · ⚠️ 踩坑警告（带真实报错原文）· 🎯 动手练习 · 📌 一句话总结

---

## 这份教程写给谁

- 会写代码，正在**从零学 React**，想把 props 一次性搞透的人。
- 已经会传 props，但遇到过这些问题：判别联合怎么写、`children` 和插槽怎么选、回调里的参数为什么变成了事件对象。

**特点**：不用其他语言/框架做类比，按 React 自身的概念体系讲；所有报错信息都是实跑出来的原文，不是凭印象写的。

---

## 进度

| 节 | 主题 | 状态 |
| --- | --- | --- |
| 第 1 节 props 的基本使用 | 类型定义 · 传值 · 接收 · 可选与默认值 · 只读 | ✅ |
| 第 2 节 用类型把 props 管住 | 联合类型 · 对象与数组 · 判别联合 · 继承原生属性 | ✅ |
| 第 3 节 children 与插槽 | children · 多插槽 · render prop | ✅ |
| **第 4 节 回调 props** | 三个级别 · 装配线与触发线（全书重点） | ✅ |
| 第 5 节 受控组件与状态提升 | `value` + `onChange` 闭环 · state 该放哪 | 📝 |
| 第 6 节 三个进阶用法 | 泛型组件 · `ref` 也是 prop · `memo` 与引用稳定性 | 📝 |
| 第 7 节 坑与反模式 | 约 12 条合集 · props 爆炸的拆分手法 | 📝 |
| 第 8 节 速查表 + 完整代码 | 一页速查 · 单文件终版 | 📝 |

---

## 目录

**[第 1 节 props 的基本使用](docs/ch01-props基础.md)**
- 1.1 组件就是函数，props 就是它的参数
- 1.2 🖼 图解：props 单向流（数据往下，事件往上）
- 1.3 定义 props 类型（`type` 还是 `interface`）
- 1.4 父组件怎么传：引号与花括号的规则
- 1.5 子组件怎么收：解构还是 `props.x`
- 1.6 可选 props 与默认值（React 19 **不用** `defaultProps`）
- 1.7 props 是只读的 —— ⚠️ **但 TypeScript 默认不会拦你**（实测）
- ⚠️ 六个必踩报错：漏传 / 类型不符 / 多传 / 拼错 / 布尔简写误用 / 改 props

**[第 2 节 用类型把 props 管住](docs/ch02-用类型管住props.md)**
- 2.1 联合类型：让取值只能是几个之一
- 2.2 `Record` 映射表：以后加了值，编译器提醒你
- 2.3 传对象：什么时候传整个对象、什么时候拆开传
- 2.4 传数组：`readonly` 的价值 + `key` 不是 props
- 2.5 **判别联合：让非法的 props 组合无法编译**（本节最值钱）
- 2.6 `ComponentProps` + `Omit` + spread：继承原生元素的全部属性
- ⚠️ 六个坑，其中 ⚠️ **忘了 `Omit` 掉 `children` 会静默丢数据、不报任何错**（实测）

**[第 3 节 children 与插槽](docs/ch03-children与插槽.md)**
- 3.1 `children`：写在标签之间的内容
- 3.2 `ReactNode` 到底能装什么（附实测渲染结果表）
- 3.3 🖼 图解：父组件写的东西落在子组件哪个位置
- 3.4 多个插槽：一个组件开几个口子
- 3.5 插槽还是 children？一张表决定
- 3.6 render prop：把「怎么渲染」交出去
- ⚠️ `&&` 遇到数字 `0` 会渲染出一个孤零零的 0（实测确认）

**[第 4 节 回调 props](docs/ch04-回调props.md)** ← 全书重点
- 4.1 为什么需要回调 props
- 4.2 本节的最小例子（28 行，与配套代码行号完全一致）
- 4.3 🖼 **图解：两根线，别混成一根** —— 装配线（渲染时，父→子）与触发线（点击时，子→父）
- 4.4 回调签名的解剖：括号里是去程，`void` 是回程
- 4.5 三个级别：只通知 / 带数据 / 子组件有自己的 state
- 4.6 三级怎么选（两个判断标准）
- 4.7 命名约定：`on*` 与 `handle*`
- ⚠️ 六个坑：多写括号 / 带参回调直接挂 onClick / 可选调用 / 改对象不替换 / 改 props / 滥用 `useCallback`

---

## 配套代码

一个可以直接跑起来的 Vite + React 19 + TypeScript 项目，五个页面对应各节：

```
code/
├── package.json
├── tsconfig.json          ← strict 开启，含 verbatimModuleSyntax
├── vite.config.ts
├── index.html
└── src/
    ├── main.tsx
    ├── App.tsx            ← 顶部导航，切换五个示例页
    ├── styles.css
    ├── ch01/TaskCardDemo.tsx    ← 第 1 节：props 基础
    ├── ch02/TypedPropsDemo.tsx  ← 第 2 节：联合类型 / 判别联合 / spread 透传
    ├── ch03/SlotsDemo.tsx       ← 第 3 节：children / 插槽 / render prop
    ├── ch04/TaskPanel.tsx       ← 第 4 节：最小形态（图 2、图 3 讲的就是这份代码）
    └── ch04/CallbackDemo.tsx    ← 第 4 节：回调三级完整版
```

跑起来：

```bash
cd code
npm install
npm run dev          # 打开终端里提示的地址
npm run typecheck    # 只做类型检查，应输出零错误
```

---

## 关于配图

每张图都有两个版本：

| 版本 | 位置 | 说明 |
| --- | --- | --- |
| **ASCII 版** | 直接写在正文的代码块里 | 任何 Markdown 查看器都能显示，中文按 2 列宽严格对齐 |
| **彩色矢量版** | `docs/assets/*.svg` | 曲线箭头 + 语法高亮，在 GitHub 上点开链接查看 |

正文用 ASCII 版而不是嵌入图片，是因为部分轻量 Markdown 查看器不加载相对路径的 SVG，嵌入会变成一个「加载失败」的占位符。两个版本由同一份定义生成，内容永远一致。

图清单：

| 图 | 用在 | 内容 |
| --- | --- | --- |
| 图 1 [fig1-flow.svg](docs/assets/fig1-flow.svg) | 第 1 节 | props 单向流：数据往下，事件往上 |
| 图 2 [fig2-wiring.svg](docs/assets/fig2-wiring.svg) | 第 4 节 | 装配线：渲染时父 → 子 |
| 图 3 [fig3-trigger.svg](docs/assets/fig3-trigger.svg) | 第 4 节 | 触发线：点击时子 → 父 |
| 图 4 [fig4-slots.svg](docs/assets/fig4-slots.svg) | 第 3 节 | children 与插槽的位置对应 |

---

## 关于代码可靠性

- **所有示例代码**：在 `react 19.2.8` + `@types/react 19.2` + `typescript 5.9.3` 下跑 `tsc --noEmit` 通过，`strict: true`，无 `any`。
- **所有报错原文**：故意写错代码、实跑编译器抓取，不是凭记忆写的。
- **运行时行为**：`ReactNode` 的渲染规则（`0` 会渲染、`false` 不会等）用 `renderToStaticMarkup` 实测验证。
- **有三处实测结果和常见说法不同，教程如实写出**：
  1. 「props 是只读的，改了会报错」—— 实际上 TypeScript **默认不报错**，要包 `Readonly<>` 才拦得住。
  2. 「`ComponentProps` 透传时忘了 `Omit` children 会报错」—— 实际上**不报错**，父组件传的内容被静默丢弃。
  3. 「`&&` 条件渲染是安全的」—— 左边是数字 `0` 时会把 `0` 渲染到页面上。

---

## 建议节奏

按每天 2 小时算：

| 天 | 内容 |
| --- | --- |
| 第 1 天 | 第 1 节 + 第 2 节 |
| 第 2 天 | 第 3 节 |
| 第 3 天 | **第 4 节**（重点，值得慢） |

第 4 节的两张图建议对着跑起来的页面点几次，比只读文字快得多。

---

## 版权与来源

本教程为原创教学内容。API 行为以 [React 官方文档](https://react.dev/) 与 `@types/react` 的类型定义为准，官方描述经改写以符合授权要求。
