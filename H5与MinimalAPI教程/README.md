# H5 页面 + Minimal API 入门教程

> 版本基线：**.NET 10 SDK**（实测 `10.0.402`，`net10.0`）｜**TypeScript 5.9.x**（`strict` 开启，无 `any`）｜Node.js 任一维护期 LTS（只用来运行 `tsc`）
>
> 后端按本教程主题使用 .NET 10 Minimal API；前端只有 `index.html` + TypeScript，用 `tsc` 编译，**不用任何打包工具和框架**。

---

## 这本教程写给谁

- 写过多年 C# 后端，想弄清楚"手机上的 H5 页面怎么和后台接口交互"。
- 想从第一天起就用正确的目录结构：**前端源码全部放在 `Frontend/`，`Backend/wwwroot/` 只放构建产物**，后端只负责"发文件 + 提供接口"。
- 想要的是一个能在手机上真正跑起来的小例子，不是概念大全。

**最短路径**：第 1、3 章 ≈ 2.5 小时（第 2 章跳过，直接拿 `code/ch02/` 当起点），做出电脑上能用的待办页面。
**完整路径**：4 章，约 5 小时，每天 2 小时约 3 天。最后在自己的手机上打开并使用。

---

## 进度

| 章节 | 状态 |
| --- | --- |
| 第 1 章 先把接口立起来：Minimal API 返回 JSON | ✅ |
| 第 2 章 页面源码放 Frontend，产物交给后端发 | ✅ |
| 第 3 章 页面用 fetch 调接口：显示列表、新增待办 | ✅ |
| **第 4 章 在手机上打开：视口、局域网访问、触控细节** | ✅ |

---

## 目录

**[第 1 章 先把接口立起来：Minimal API 返回 JSON](docs/ch01-minimal-api-json.md)**：不写页面，用浏览器和 curl 验证 GET/POST 接口、JSON 和状态码。

**[第 2 章 页面源码放 Frontend，产物交给后端发](docs/ch02-tsc-build-static-files.md)**：`tsc` 把 TypeScript 编译到 `wwwroot`、复制 `index.html`，`UseDefaultFiles` + `UseStaticFiles` 把页面发给浏览器。

**[第 3 章 页面用 fetch 调接口：显示列表、新增待办](docs/ch03-fetch-json.md)**：fetch 读写 JSON，检查 `res.ok`，错误信息显示到页面上。

**[第 4 章 在手机上打开：视口、局域网访问、触控细节](docs/ch04-mobile.md)**：监听 `0.0.0.0`、加 viewport 声明、44px 按钮和长内容自动换行。

---

## 最终目录结构

```
h5-demo/
├── Backend/
│   ├── Program.cs              ← 接口 + 静态文件中间件
│   ├── Properties/launchSettings.json
│   └── wwwroot/                ← 构建产物，不手改，不提交（.gitignore）
└── Frontend/
    ├── index.html              ← 页面源码
    ├── style.css               ← 样式源码（第 4 章加入）
    ├── src/main.ts             ← 脚本源码
    ├── tsconfig.json           ← outDir: ../Backend/wwwroot
    └── package.json            ← npm run build = 检查类型 → 清空 → tsc 编译 → 复制 html/css
```

日常开发循环：

```bash
# 改完 Frontend 里的文件后（在 Frontend/ 目录）
npm run build
# 浏览器刷新即可，后端不用重启（前提：启动后端之前 wwwroot 已经存在）
```

---

## 配套代码

`code/` 下每章是一份**完整可运行**的快照，章与章之间互不依赖：

```
code/
├── .gitignore        ← bin/ obj/ node_modules/ 和 **/Backend/wwwroot/
├── ch01/Backend/
├── ch02/Backend/  ch02/Frontend/
├── ch03/Backend/  ch03/Frontend/
└── ch04/Backend/  ch04/Frontend/
```

运行任意一章（以 ch04 为例）：

```bash
cd code/ch04/Frontend
npm install
npm run build        # 必须先构建，再启动后端

cd ../Backend
dotnet run           # 打开 http://localhost:5080/
```

---

## 学完之后

| 想做的事 | 改哪里 |
| --- | --- |
| 前端改由 Nginx/CDN 单独部署 | 把 `tsconfig.json` 的 `outDir` 和 `package.json` 里 `clean`/`copy` 的目标都改成 `dist`，后端删掉 `UseDefaultFiles`/`UseStaticFiles`；用 Nginx 把 `/api` 反向代理到后端，保持同源，不用配 CORS |
| 改 `.ts` 后不想每次手动 build | 在 `Frontend/` 另开一个窗口运行 `npx tsc --watch`，保存即编译（改了 `index.html`/`style.css` 仍要 `npm run build`） |
| 脚本拆成多个文件 | `import` 路径写编译后的名字：`from "./api.js"`（见第 2 章踩坑表） |
| 要用 npm 上的第三方库、或者换成 React | 这时 `tsc` 不够用了，需要引入打包工具 |
| 数据落库 | 把内存 `List<Todo>` 换成数据库访问，接口签名不变，前端不用改 |

---

## 关于代码可靠性

- 后端在 .NET SDK `10.0.402` 上编译运行；前端用 TypeScript `5.9.3` 的 `tsc` 编译。
- 正文里的状态码、报错原文、构建输出、`innerWidth` 等数值，都来自实际运行（curl + 无头 Chromium 模拟 iPhone 14）。
- 以下内容沙箱里无法验证，正文中已标 ⚠️：Windows 防火墙命令、Windows `cmd` 下的 curl 写法、iOS Safari 对小于 16px 输入框的自动放大、刘海屏 safe-area 的实际效果。
