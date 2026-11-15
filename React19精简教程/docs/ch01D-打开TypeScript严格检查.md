# 第 1D 章 打开 TypeScript 严格检查

## 这一章解决什么问题

本教程全程用 TypeScript，靠它在你写错时提醒你：数字传成了字符串、参数忘了写类型、变量声明了没用上。但上一章你已经看到，Vite 开发服务器只管翻译，**不检查类型**。本章把严格检查显式打开，并学会两种看到类型错误的办法：VS Code 里的红色波浪线，和 `npm run build`。

本章只讲一个技术点：**TypeScript 严格模式**。

## 开始之前

- 上一章你已经会了：看热更新（`hmr update`）和整页刷新（`page reload`）、读错误遮罩。
- 起点代码：仓库 `code/ch01B/price-list`（第 1C 章没有改代码）。在 `D:\react-study\price-list` 执行 `npm run dev`。
- 预计耗时：20 分钟。

## 正文

### 1D.1 严格模式是什么

**白话定义**：TypeScript 的严格模式（`strict`）要求每个变量和参数都有明确的类型，不允许悄悄变成 `any`（「任意类型，不检查」），可能为 `null` 的值使用前必须先判断。

**业务场景**：价格是数字、产品名是字符串。把 `'1280'`（字符串）当价格传进去，`price + 100` 会得到 `'1280100'` 而不是 `1380`。严格模式在你写代码时就指出来，而不是等用户看到错的报价。

**反例（什么时候不该用）**：不要为了让报错消失而把 `strict` 改成 `false`，那等于关掉了 TypeScript 最有用的检查（练习 2 亲手验证）。本教程全程开启严格模式、禁止 `any`。

### 1D.2 在 tsconfig.app.json 里写上 strict

TypeScript 7.0 默认已开启严格模式，但模板配置里没写出来。显式写上，换版本也不会变，别人看配置也一目了然。

#### 💻 最小可运行代码

打开 `tsconfig.app.json`，在 `/* Linting */` 这一行下面插入 `"strict": true,`，其他行不动。改完的完整文件：

```json
{
  "compilerOptions": {
    "tsBuildInfoFile": "./node_modules/.tmp/tsconfig.app.tsbuildinfo",
    "target": "es2023",
    "lib": ["ES2023", "DOM"],
    "module": "esnext",
    "types": ["vite/client"],
    "allowArbitraryExtensions": true,
    "skipLibCheck": true,

    /* Bundler mode */
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "verbatimModuleSyntax": true,
    "moduleDetection": "force",
    "noEmit": true,
    "jsx": "react-jsx",

    /* Linting */
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "erasableSyntaxOnly": true,
    "noFallthroughCasesInSwitch": true
  },
  "include": ["src"]
}
```

注意改的是 `tsconfig.app.json`，不是 `tsconfig.json`。项目里有三个配置文件：

| 文件 | 管哪些代码 |
| --- | --- |
| `tsconfig.json` | 自己不检查任何文件，只是把下面两个串起来 |
| `tsconfig.app.json` | `src` 文件夹里的页面代码，本教程写的代码都归它管 |
| `tsconfig.node.json` | `vite.config.ts` 这类在 Node.js 里运行的配置文件 |

#### 🔍 拆解

这个文件里几项和日常写代码关系最大的设置：

| 设置 | 作用 |
| --- | --- |
| `"strict": true` | 打开全部严格检查：参数必须有类型、可能为 `null` 的值使用前必须判断等 |
| `"noUnusedLocals"` / `"noUnusedParameters"` | 声明了却没用的变量、参数会报错。后面几章的「改坏一处」会经常看到它 |
| `"jsx": "react-jsx"` | 允许在 `.tsx` 文件里写 JSX |
| `"include": ["src"]` | 只检查 `src` 文件夹里的代码 |
| `"noEmit": true` | TypeScript 只做检查、不输出文件，翻译和打包交给 Vite |

### 1D.3 在哪里看到类型错误

要知道一件事：**Vite 开发服务器只负责翻译，不检查类型。** 实测时在 `App.tsx` 的 `function App() {` 下面加了两行：

```tsx
  const price: number = '1280'
  console.log(price)
```

`const price: number` 声明价格是数字，却赋了一个字符串。浏览器照样显示页面，控制台打印出 `1280`，终端也没有报错。类型错误只在下面这些地方出现：

| 在哪看 | 怎么看 |
| --- | --- |
| VS Code 编辑器 | 出错的代码下面有红色波浪线，鼠标停上去能看到错误说明；菜单「查看 → 问题」列出所有错误 |
| `npx tsc -b` | 只做类型检查，有错误就打印出来，没错误就什么都不输出 |
| `npm run build` | 打包。它会先运行 `tsc -b`，有类型错误就停止打包 |

#### 💻 动手

保留上面那两行，另开一个终端（点终端面板右上角的 `+`），执行：

```powershell
# 在 D:\react-study\price-list 目录执行（另开一个终端，不影响开发服务器）
npm run build
```

#### 🔍 拆解运行结果

```text
> price-list@0.0.0 build
> tsc -b && vite build

src/App.tsx(4,9): error TS2322: Type 'string' is not assignable to type 'number'.
```

| 部分 | 含义 |
| --- | --- |
| `tsc -b && vite build` | `package.json` 里 `build` 的命令：先类型检查，`&&` 表示前一步成功才执行后一步 |
| `src/App.tsx(4,9)` | 第 4 行第 9 列 |
| `TS2322` | 错误编号，搜索这个编号能找到很多解释 |
| `Type 'string' is not assignable to type 'number'` | 字符串不能赋给数字类型 |

把那两行删掉，再执行一次 `npm run build`，输出以 `✓ built in ...ms` 结尾并生成 `dist` 文件夹（第 10B 章再用，现在可以删掉）。

所以写代码时要留意编辑器里的红色波浪线，不要只看浏览器能不能跑。

#### 改坏一处试试

**① 参数不写类型。** 在 `function App() {` 上面加一个函数：

```tsx
function formatPrice(price) {
  return price.toLocaleString() + ' 元'
}
```

再在 `App` 里用一下它（例如 `console.log(formatPrice(12800))`），`npx tsc -b` 报：

```text
src/App.tsx(3,22): error TS7006: Parameter 'price' implicitly has an 'any' type.
```

`implicitly has an 'any' type` 的意思是「没写类型，被当成了 `any`」。严格模式不允许这样，写成 `price: number` 就好了。

**② 声明了不用。** 在 `function App() {` 下面加一行 `const total = 12800 * 2`，`npx tsc -b` 报：

```text
src/App.tsx(4,9): error TS6133: 'total' is declared but its value is never read.
```

这是 `noUnusedLocals` 的检查。VS Code 里这类变量会显示成灰色。

每次实验后删掉加的代码。

## ⚠️ 踩坑警告

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 代码里明明有类型错误，页面照样正常显示 | 开发服务器只翻译不检查类型 | 看 VS Code 里的红色波浪线；提交代码前执行一次 `npm run build` 或 `npx tsc -b` |
| `npm run build` 报了类型错误，但 `dist` 文件夹还在，以为打包成功了，拿去部署发现还是旧页面 | `tsc -b` 失败后 `vite build` 根本没执行，`dist` 里是**上一次**成功打包的文件（实测：报错后 `dist/assets` 里文件的修改时间没变） | 看输出最后一行是不是 `✓ built in ...ms`。有 `error TS` 就是没打包 |
| 为了消除报错把 `"strict"` 改成 `false`，`TS7006` 不见了 | 关掉了严格检查，参数没写类型也不报了，错误被藏起来而不是被修好 | 保持 `"strict": true`，按报错补上类型 |

## 🎯 动手练习

**练习 1：写一个带类型的格式化函数**

在 `App.tsx` 里写一个函数 `formatPrice`，接收一个数字价格，返回带千位分隔符和「元」的字符串。在页面上显示一行「示例单价：12,800 元」。

验收标准：

- 页面显示「示例单价：12,800 元」。
- 先故意不写参数类型，`npm run build` 报 `TS7006`；补上类型后 `npm run build` 以 `✓ built in` 结尾。

提示：JSX 里用花括号放表达式，例如 `<p>示例单价：{formatPrice(12800)}</p>`。花括号的规则第 2B 章细讲。

**练习 2：关掉 strict 看看会怎样**

在练习 1 「参数不写类型」的状态下，把 `tsconfig.app.json` 里的 `"strict": true` 改成 `false`，执行 `npx tsc -b`。

验收标准：

- `strict` 为 `false` 时，`npx tsc -b` 没有任何输出（不报 `TS7006`）。
- 改回 `true` 后，`TS7006` 又出现。
- 最后把 `strict` 保持为 `true`，参数类型补上。

卡住了看文末参考答案。

## 📌 一句话总结

开发服务器不查类型，看红色波浪线或跑 npm run build。

## 下一章

页面内容全写在一个 `App` 里会越来越长，第 2A 章把它拆成多个组件。

---

<!-- 以下是参考答案，先自己动手，卡住了再往下翻 -->































## 参考答案

### 练习 1

思路：参数写 `price: number`，返回值 TypeScript 能自己推断出是字符串，不用写。`toLocaleString()` 按系统地区加千位分隔符。

```tsx
// src/App.tsx
// 练习 1 做完后的版本
// 参数写了 : number，严格模式下不再报 TS7006
function formatPrice(price: number) {
  return price.toLocaleString() + ' 元'
}

function App() {
  return (
    <main>
      <h1>产品价格清单</h1>
      <p>第 1 章：项目跑起来了。</p>
      {/* JSX 里用花括号放表达式，第 2B 章细讲 */}
      <p>示例单价：{formatPrice(12800)}</p>
    </main>
  )
}

export default App
```

### 练习 2

不用写代码。实测：

| `strict` | 参数 `price` 不写类型时 `npx tsc -b` 的输出 |
| --- | --- |
| `true` | `src/App.tsx(3,22): error TS7006: Parameter 'price' implicitly has an 'any' type.` |
| `false` | 没有输出，检查通过 |

`false` 时 `price` 被当成 `any`，传什么都不报错，`formatPrice('abc')` 也能通过检查，到运行时才出问题。这就是本教程禁止 `any` 的原因。
