# React 18 & 19 系统教程

> 本教程由原《React18-使用说明》长文按章节拆分而成，并**新增了第二章《React 页面的组成》**，系统讲解"一张 React 页面到底由什么拼起来"。
>
> 全教程共 **22 章 + 2 个附录**，包含 **320+ 个由浅入深的可运行小示例**（示例编号 1–322 沿用原文，全程连续），覆盖 React 18 从入门到实战，并补充 React 19 的全部新特性。
>
> 面向已了解 JavaScript / ES6 的开发者。**最好的学习方式是边读边动手敲。**

## 目录

### 基础篇（React 18）

| 章节 | 内容 | 示例 |
| --- | --- | --- |
| [第一章 · React 18 简介与环境准备](01-React18简介与环境准备.md) | createRoot、StrictMode、项目搭建 | 1–5 |
| [第二章 · React 页面的组成](02-React页面的组成.md) 🆕 | 页面 = 组件树、组件如何嵌套组合、与 WebForm 对照 | 概念章 |
| [第三章 · JSX 基础](03-JSX基础.md) | 表达式、属性、结构规则、内容渲染 | 6–29 |
| [第四章 · 组件与 Props](04-组件与Props.md) | 函数组件、props、children、组件通信 | 30–57 |
| [第五章 · State 与事件](05-State与事件.md) | useState、不可变更新、事件处理、自动批处理 | 58–85 |
| [第六章 · 条件渲染与列表](06-条件渲染与列表.md) | 三元 / `&&`、map、key | 86–115 |
| [第七章 · 表单处理](07-表单处理.md) | 受控组件、各类控件、校验、文件上传 | 116–138 |

### 进阶篇（React 18）

| 章节 | 内容 | 示例 |
| --- | --- | --- |
| [第八章 · 核心 Hooks](08-核心Hooks.md) | useEffect、useRef、useContext、useReducer、useMemo/useCallback、自定义 Hook | 139–168 |
| [第九章 · React 18 新增 Hooks](09-React18新增Hooks.md) | useId、useTransition、useDeferredValue、useSyncExternalStore、useInsertionEffect | 169–186 |
| [第十章 · 并发特性](10-并发特性.md) | 并发渲染、lazy + Suspense、流式 SSR | 187–201 |
| [第十一章 · 性能优化](11-性能优化.md) | React.memo、useMemo/useCallback、结构优化、虚拟化 | 202–218 |
| [第十二章 · Context 与组件通信](12-Context与组件通信.md) | 通信方式、Context 封装与陷阱 | 219–234 |
| [第十三章 · 进阶与实战](13-进阶与实战.md) | 常用交互组件、错误边界、完整 Todo | 235–251 |

### 生态篇

| 章节 | 内容 | 示例 |
| --- | --- | --- |
| [第十四章 · React Router 路由](14-ReactRouter路由.md) | 路由配置、导航、参数、嵌套路由、受保护路由 | 252–267 |
| [第十五章 · 数据请求（React Query）](15-数据请求ReactQuery.md) | useQuery、useMutation、缓存、分页、无限滚动 | 268–283 |

### React 19 新特性

| 章节 | 内容 | 示例 |
| --- | --- | --- |
| [第十六章 · React 19 简介与升级](16-React19简介与升级.md) | 安装、入口、升级前置要求 | 284–286 |
| [第十七章 · Actions](17-Actions.md) | useActionState、`<form action>`、useFormStatus、useOptimistic | 287–295 |
| [第十八章 · `use` API](18-use-API.md) | 渲染中读取 Promise 与 Context | 296–299 |
| [第十九章 · 组件 API 的改进](19-组件API的改进.md) | ref 作为 prop、ref 清理、`<Context>` 作 Provider | 300–307 |
| [第二十章 · 文档元数据与资源预加载](20-文档元数据与资源预加载.md) | title/meta、样式表、脚本、preload/preinit | 308–312 |
| [第二十一章 · Server Components](21-ServerComponents.md) | RSC、`use client` / `use server`（概念入门） | 313–315 |
| [第二十二章 · 其它改进与破坏性变更](22-其它改进与破坏性变更.md) | 错误处理、静态 API、被移除的 API | 316–322 |

### 附录

- [附录 A · 常见易错点与学习建议](附录A-常见易错点与学习建议.md)
- [附录 B · React 18 → React 19 升级速查](附录B-React18到React19升级速查.md)

---

## 说明

- **章节编号**：因新增了第二章《React 页面的组成》，原文第 2 章及之后的章节整体顺延一位（原第 2 章 JSX 现为第三章，依此类推）。**正文中所有"第 X 章"的交叉引用均已同步更新**。
- **示例编号**：示例 1–322 的编号保持原样、全程连续，方便与原文对照。新增的第二章为概念讲解，不占用示例编号。
- **原始单文件**：完整未拆分的原文仍保留在仓库根目录的 `React18-使用说明.md`。
