# 准备篇一：怎样创建并运行 React 19 + TypeScript 项目？

前置：无，从本篇开始。

## 一、本章要解决的问题

在电脑上建立本教程使用的练习项目，并让它在浏览器中运行起来。完成后，浏览器显示“项目已启动”；你修改代码并保存，页面会自动更新。

这是全书唯一一次从零建立项目。之后的每一章都在同一个 `react-daily-practice` 文件夹中修改文件。

## 二、必要原理

### 开始前的基础自检

本教程不要求你学过 React，但需要一点 HTML、CSS、JavaScript 基础。看看下面这些代码，能大致说出意思就可以开始：

| 代码 | 应能说出的意思 | 不会时去哪里补 |
| --- | --- | --- |
| `<h1>你好</h1>` | 一个一级标题，内容是“你好” | [MDN：HTML 基础](https://developer.mozilla.org/zh-CN/docs/Learn_web_development/Getting_started/Your_first_website/Creating_the_content) |
| `<img src="a.png" alt="头像">` | 显示图片 a.png，`alt` 是图片无法显示时的替代文字 | 同上 |
| `.card { color: blue; }` | 把 class 为 card 的元素文字设成蓝色 | [MDN：CSS 基础](https://developer.mozilla.org/zh-CN/docs/Learn_web_development/Getting_started/Your_first_website/Styling_the_content) |
| `const total = 3 + 4;` | 声明变量 total，值为 7 | [MDN：JavaScript 基础](https://developer.mozilla.org/zh-CN/docs/Learn_web_development/Getting_started/Your_first_website/Adding_interactivity) |
| `function add(a, b) { return a + b; }` | 定义函数 add，返回两个参数的和 | 同上 |
| `const user = { name: '小林', age: 20 };` | 对象 user 有 name 和 age 两个属性 | 同上 |
| `const list = [1, 2, 3];` | 数组 list 有三个数字 | 同上 |

前三行属于 HTML 和 CSS，后四行属于 JavaScript。如果后四行有一两项不太熟，也可以先开始，教程第一次用到时会再解释。数组的 `map`、`filter`、`async`/`await` 等写法不要求提前掌握，用到的章节会当场讲。

### 这些工具各管什么

- **Node.js**：让电脑能运行 JavaScript 工具程序。浏览器里的页面不依赖它，但开发工具需要它。
- **npm**：随 Node.js 一起安装，负责下载和管理项目用到的工具和库。
- **React**：用组件描述页面，数据变化时更新页面。
- **TypeScript**：在 JavaScript 基础上加入类型检查，在运行前发现数据用错的问题。
- **Vite**：开发时启动一个本地网站，把 TypeScript 和 JSX 转换成浏览器能运行的代码；修改代码后自动刷新页面。

### 页面是怎样显示出来的

浏览器打开开发地址后，先读取 `index.html`。这个文件里有一个空的 `<div id="root">` 容器，还有一行脚本指向 `src/main.tsx`。`main.tsx` 找到这个容器，把 `App` 组件显示进去。`App` 组件写在 `src/App.tsx` 中，以后大部分修改都从这里开始。

`.tsx` 是可以在 TypeScript 里写 JSX 的文件。JSX 是写在 JavaScript 中、看起来像 HTML 的标记，第 1 章会详细讲。

### 编辑器

推荐使用 [Visual Studio Code](https://code.visualstudio.com/)。它能直接显示 TypeScript 发现的错误，后面的章节会用到这一点。安装后用它打开项目文件夹即可，不需要额外配置。

实际工作中，新项目通常用 `npm create vite@latest` 命令自动生成。本教程改为手工建立 7 个文件，这样每个文件的作用都清楚，依赖的版本也和教程完全一致。两种方式得到的项目结构基本相同。

## 三、动手实现

### 第 1 步：安装 Node.js

从 [Node.js 官网下载页](https://nodejs.org/zh-cn/download) 下载 Windows 的 Node.js 24 LTS 安装包，按默认选项安装。安装完成后，**关闭并重新打开** PowerShell，输入：

```powershell
node --version
npm.cmd --version
```

能看到版本号就说明安装成功，例如 `v24.19.0` 和 `11.17.0`。本教程使用 Node.js 24.19.0 编写，24 系列的其他版本通常也可以。

本教程在 PowerShell 中统一写 `npm.cmd`，而不是 `npm`。有些 Windows 电脑的安全设置会阻止 PowerShell 运行 `npm` 脚本，`npm.cmd` 不受影响，作用完全相同。

### 第 2 步：建立项目文件夹

选一个自己方便找到的位置，例如 `D:\learn`。在资源管理器中进入这个文件夹，在空白处按住 Shift 并单击右键，选择“在此处打开 PowerShell 窗口”（或“在终端中打开”），然后执行：

```powershell
mkdir react-daily-practice
cd react-daily-practice
mkdir src
```

用 VS Code 打开 `react-daily-practice` 文件夹，按下面的路径建立 7 个文件，保存时编码选 UTF-8（VS Code 默认就是）：

```text
react-daily-practice/
  package.json
  tsconfig.json
  vite.config.js
  index.html
  .gitignore
  src/
    main.tsx
    App.tsx
```

### 第 3 步：填写配置文件

`package.json` 是项目清单，写明项目用到哪些依赖，以及有哪些快捷命令：

```json
{
  "name": "react-daily-practice",
  "private": true,
  "version": "0.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite --host 127.0.0.1",
    "typecheck": "tsc --noEmit",
    "build": "npm run typecheck && vite build",
    "preview": "vite preview --host 127.0.0.1"
  },
  "dependencies": {
    "react": "19.3.1",
    "react-dom": "19.3.1"
  },
  "devDependencies": {
    "@types/react": "19.3.0",
    "@types/react-dom": "19.3.0",
    "@vitejs/plugin-react": "6.2.1",
    "typescript": "7.0.2",
    "vite": "8.3.5"
  }
}
```

- `dependencies` 是页面运行时需要的库，`devDependencies` 是只在开发时使用的工具。
- 版本号前没有 `^` 符号，表示固定使用这个版本，避免以后安装到不同的版本导致和教程不一致。
- `scripts` 中的每一项都是一个快捷命令，例如 `dev` 可以用 `npm.cmd run dev` 运行。

`tsconfig.json` 是 TypeScript 的设置：

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "moduleResolution": "Bundler",
    "jsx": "react-jsx",
    "strict": true,
    "noEmit": true,
    "skipLibCheck": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "types": ["vite/client"]
  },
  "include": ["src"]
}
```

这里只需要知道三项：`strict` 打开严格检查，能发现更多问题；`jsx` 告诉 TypeScript 怎样处理 JSX；`include` 表示检查 `src` 文件夹里的代码。其他选项是这个项目的固定环境，照抄即可。

`vite.config.js` 让 Vite 使用 React 插件：

```js
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
});
```

`.gitignore` 列出不需要用 Git 保存的文件夹，准备篇三会用到：

```text
node_modules/
dist/
```

### 第 4 步：填写页面入口

`index.html`：

```html
<!doctype html>
<html lang="zh-CN">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="icon" href="data:," />
    <title>React 日常开发练习</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.tsx"></script>
  </body>
</html>
```

`<div id="root"></div>` 是空容器，React 会把页面放进这里。`<link rel="icon" href="data:," />` 告诉浏览器本项目没有网站图标，避免浏览器去请求一个不存在的图标文件。

`src/main.tsx`：

```tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';

const container = document.getElementById('root');

if (!container) {
  throw new Error('找不到 id 为 root 的页面容器');
}

createRoot(container).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
```

- `import` 从别的文件或库中取出需要的功能。`'./App'` 表示同一文件夹下的 `App.tsx`，扩展名可以省略。
- `document.getElementById('root')` 找到 `index.html` 中的容器。如果没找到，`throw` 会报出一条看得懂的错误，第 6 章会用到它。
- `createRoot(container).render(...)` 把 `App` 显示到容器中。
- `StrictMode` 是 React 的开发检查模式，帮助发现潜在问题。它不会在页面上多出任何内容。

`src/App.tsx`：

```tsx
export default function App() {
  return <h1>项目已启动</h1>;
}
```

`App` 是一个函数，返回要显示的内容。`export default` 让 `main.tsx` 能导入它。

### 第 5 步：安装依赖并启动

回到 PowerShell，确认当前位置是 `react-daily-practice` 文件夹（`package.json` 所在的位置），依次执行：

```powershell
npm.cmd install
npm.cmd run typecheck
npm.cmd run dev
```

- `install` 按 `package.json` 下载依赖，需要联网，可能要等一两分钟。完成后项目中会多出 `node_modules` 文件夹和 `package-lock.json` 文件。`package-lock.json` 记录了每个依赖的精确版本，不需要打开或修改它。
- `run typecheck` 运行 TypeScript 检查，没有输出任何错误就是通过。
- `run dev` 启动开发服务器。它会一直运行，终端中会显示一个 Local 地址。

## 四、运行与检查

1. 打开终端显示的 Local 地址，通常是 `http://127.0.0.1:5173/`，页面显示“项目已启动”。
2. 把 `App.tsx` 中的“项目已启动”改成“环境检查通过”，保存。浏览器不需要手动刷新，页面会自动更新。检查完再改回“项目已启动”。
3. 在运行 `dev` 的终端按 Ctrl+C，开发服务器停止。刷新浏览器，页面打不开了。
4. 再执行一次 `npm.cmd run dev`，页面恢复。

以后每次学习时，进入项目文件夹运行 `npm.cmd run dev` 就可以开始。只有章节中说明新增了依赖时，才需要再次运行 `npm.cmd install`。

**日常习惯**：开发服务器占用一个终端。需要运行检查等其他命令时，在 VS Code 中按 Ctrl+Shift+\`（反引号键，在 Esc 下方）新建一个终端，或者点击终端面板右上角的“+”，不要停止开发服务器。注意 Ctrl+\`（不按 Shift）只是显示或隐藏终端面板，不会新建终端。

## 五、常见错误

| 现象 | 原因 | 怎么改 |
| --- | --- | --- |
| 提示 `node` 不是可识别的命令 | 安装 Node.js 后没有重新打开终端，或安装没有完成 | 关闭所有 PowerShell 窗口后重新打开，再检查版本 |
| 提示找不到 `package.json` | 终端的当前位置不是项目文件夹 | 用 `cd` 进入 `react-daily-practice`，或在该文件夹中重新打开终端 |
| `install` 报 JSON 解析错误 | `package.json` 中用了中文引号、多了逗号或缺了括号 | 对照第三节逐字检查；最后一项后面不能有逗号 |
| `install` 报网络错误 | 网络无法访问 npm 仓库 | 检查网络后重试；先读报错的第一行，不要反复删除项目重建 |
| 双击 `index.html` 打开是空白页 | 浏览器直接打开文件时无法处理 `.tsx` | 必须先运行 `npm.cmd run dev`，用它显示的地址访问 |
| 修改代码后页面没变化 | 文件没有保存，或改的不是正在运行的项目 | 确认 VS Code 标签页上没有表示未保存的圆点 |

## 六、本章完整代码

本章新建的 7 个文件，内容都已在第三节给出：`package.json`、`tsconfig.json`、`vite.config.js`、`.gitignore`（第 3 步），`index.html`、`src/main.tsx`、`src/App.tsx`（第 4 步）。

## 七、本章改动小结

新建 7 个文件；运行 `npm.cmd install` 后自动生成 `node_modules` 文件夹和 `package-lock.json`。

---

下一章：[准备篇二：怎样启动教程配套的练习服务？](准备篇二-怎样启动教程配套的练习服务.md)
