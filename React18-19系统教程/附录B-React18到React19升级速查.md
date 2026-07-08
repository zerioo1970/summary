# 附录 B · React 18 → React 19 升级速查

> 《React 18 & 19 系统教程》附录。完整目录见 [README](README.md)。

> 一张表快速回顾这一部分。升级时逐条对照即可。

| 主题 | React 18 | React 19 | 相关示例 |
| --- | --- | --- | --- |
| 异步 transition | `startTransition` 只接同步函数 | 可接 async 函数（Action） | 288 |
| 表单提交 | 手写 `isPending`/`error` | `useActionState` + `<form action>` | 289–292 |
| 提交状态下传 | 层层传 props 或 Context | `useFormStatus` | 293 |
| 乐观更新 | 手动维护临时状态 | `useOptimistic` | 294–295 |
| 读异步数据 | `useEffect` + `useState` | `use(promise)` + Suspense | 296–297 |
| 条件读 Context | 不行（Hook 规则） | `use(Context)` 可条件调用 | 298–299 |
| 转发 ref | `forwardRef` 包裹 | `ref` 作为普通 prop | 300–301 |
| ref 清理 | 判断 `node === null` | ref 回调返回清理函数 | 302–304 |
| Context Provider | `<Ctx.Provider>` | `<Ctx>` 直接用 | 305–306 |
| useDeferredValue | 无初始值 | 支持 `initialValue` | 307 |
| 文档元数据 | react-helmet / effect | 组件内直接写 `<title>` 等 | 308 |
| 样式表 | 手动放 `<head>` | `<link precedence>` 自动管理 | 309–310 |
| 资源预加载 | 手写 `<link rel>` | `preload`/`preinit` 等 API | 312 |
| 错误上报 | 重复日志 | `onCaughtError` 等回调 | 316–317 |
| 入口 | `ReactDOM.render`（废弃） | `createRoot`（`render` 已移除） | 322 |
| 类型检查 | `propTypes` | TypeScript | 320 |

**React 19 部分小结**：React 19 的主线是 **Actions**——把"提交 → 请求 → 反馈"这条最常见的链路做成内建能力，配合 `useActionState`、`<form action>`、`useFormStatus`、`useOptimistic` 大幅减少样板代码；`use` API 让读取异步数据和条件读 Context 更自然；`ref` 作为 prop、`<Context>` 直接作 Provider 等改进降低了日常心智负担；文档元数据/样式表/资源预加载则把过去依赖第三方库的能力收进了内核。对纯客户端项目，Server Components 可暂不关注；升级时最需要处理的是**被移除的旧 API**。

至此，本文档在 React 18（示例 1–283）之上，补充了 React 19 的全部新特性（示例 284–322）。**建议动手把这些新 Hook 敲一遍**——尤其是 `useActionState` + `<form action>` 这套组合，它会明显改变你写表单和数据变更的方式。祝你在 React 19 里写得更少、做得更多！

---
[📖 目录](README.md)
