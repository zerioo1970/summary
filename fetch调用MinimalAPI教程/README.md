# HTML + fetch 调用 Minimal API 上手教程

> 版本基线：.NET 10 SDK（实测 10.0.403）+ C# 14 + ASP.NET Core Minimal API ｜ Visual Studio 2026
> 前端：原生 HTML + JavaScript + 浏览器自带 `fetch`，**不用 TypeScript、不用 npm、不用打包工具**；页面放在后端项目的 `wwwroot` 里，同一个网站提供页面和接口
> 数据：放在内存 `List<Product>` 里，程序重启恢复初始 5 条，不需要数据库
> 浏览器：Chrome / Edge（示例在 Chromium 无头浏览器实测）；第 6 章手机效果用 iPhone 13 尺寸模拟实测
>
> 本教程是独立教程，见《教程编写规范》6.1 节登记（2026-10-30 确认，前端允许纯 JavaScript）。

---

## 这本教程写给谁

- 会 C#，用过 ASP.NET，想做一张"不刷新页面就能查询、新增、修改、删除"的普通网页。
- 不想先学 React、Vite、npm 这一整套，只要一个 HTML 文件加一个 JS 文件就能干活。
- 目标是日常够用：列表、条件查询、表单提交、校验提示、修改、删除、错误提示，手机上也能用。

**最短路径**：第 1、3、4 章约 3.5 小时，做出能查询、新增（带校验）、删除的页面。
**完整路径**：6 章，约 7.5 小时，每天 2 小时 4 天左右，做出电脑和手机都能用的增删改查页面。

---

## 进度

| 章节 | 状态 |
| --- | --- |
| 第 1 章 页面打开，点一下按钮，把产品列表显示出来 | ✅ |
| 第 2 章 按条件查询、按 Id 查一条，查不到要说清楚 | ✅ |
| 第 3 章 新增一条数据，填错了要知道是哪一栏 | ✅ |
| 第 4 章 删除一条数据，并把各种错误统一处理 | ✅ |
| 第 5 章 在表格行上点「编辑」「删除」，修改一条数据 | ✅ |
| **第 6 章 让页面在手机上能看、能点，并用手机打开它** | ✅ |

---

## 目录

### 第一部分 读数据（2 章，约 2.5 小时）

**[第 1 章 页面打开，点一下按钮，把产品列表显示出来](docs/ch01-get-list.md)**
- 1.1 一张页面会发几次请求（同源、相对地址）
- 1.2 建项目：VS 2026「ASP.NET Core 空」或 `dotnet new web`，端口固定为 5080
- 1.3 `MapGet` 返回 List，JSON 字段名为什么变成小写开头
- 1.4 静态文件：`wwwroot` + `UseDefaultFiles` + `UseStaticFiles`
- 1.5 `fetch` + `await`，用 `textContent` 填表格
- 1.6 改坏四处：大写字段名、漏写 await、中间件顺序、双击打开 html

**[第 2 章 按条件查询、按 Id 查一条，查不到要说清楚](docs/ch02-query.md)**
- 2.1 查询字符串和路由参数分别用在哪
- 2.2 `string? keyword` 可选条件；`{id:int}` + `Results.NotFound()`
- 2.3 `URLSearchParams` 编码查询条件
- 2.4 加载中、没有数据、查不到三种提示
- 2.5 改坏三处：手工拼地址、对 404 调 `.json()`、去掉 `:int`

### 第二部分 写数据（3 章，约 4 小时）

**[第 3 章 新增一条数据，填错了要知道是哪一栏](docs/ch03-post.md)**
- 3.1 一次保存的来回
- 3.2 `MapPost` 从正文绑定对象，`Results.Created` 返回 201，`Results.ValidationProblem` 返回 400
- 3.3 `fetch` 的 `method`、`headers`、`body` 三个选项；`e.preventDefault()`
- 3.4 按字段显示红字
- 3.5 改坏四处：不拦表单提交、漏 Content-Type（415）、不 `JSON.stringify`、价格不转数字

**[第 4 章 删除一条数据，并把各种错误统一处理](docs/ch04-delete-api.md)**
- 4.1 一次请求可能出现的 8 种结果
- 4.2 `MapDelete` 返回 204
- 4.3 统一请求函数 `api()`：网络不通、空正文、非 JSON、错误状态码一处处理
- 4.4 后端停掉、数据已删除、请求格式错时页面的提示
- 4.5 改坏两处：不看 `response.ok`、对 204 调 `.json()`

**[第 5 章 在表格行上点「编辑」「删除」，修改一条数据](docs/ch05-put-row-buttons.md)**
- 5.1 从点「编辑」到保存完成
- 5.2 `MapPut`；新增和修改共用校验，查重排除自己
- 5.3 一张表单两种模式（`editingId`）；事件委托 + `data-` 属性
- 5.4 两个标签页演示"数据已被别人删除"
- 5.5 改坏两处：给每个按钮单独挂点击、查重不排除自己

### 第三部分 手机（1 章，约 1 小时）

**[第 6 章 让页面在手机上能看、能点，并用手机打开它](docs/ch06-mobile.md)**
- 6.1 手机怎么连到电脑上的网站（局域网 IP、防火墙、监听地址）
- 6.2 viewport 一行 + `@media (max-width: 600px)` 把表格变卡片，JS 不改
- 6.3 用 F12 设备模拟预览手机效果
- 6.4 `applicationUrl` 改成 `http://*:5080`
- 6.5 `ipconfig` 查 IP、`netsh` 放行防火墙、手机访问
- 6.6 改坏四处：删 viewport、写成 min-width、漏 box-sizing、只监听 localhost

---

## 配套代码

每章一个完整可运行的项目，练习的参考答案也是完整项目：

```text
code/
├── ch01/FetchDemo/           第 1 章正文
├── ch01/Answer1-Stock/       第 1 章练习 1 参考答案
├── ch01/Answer2-AutoLoad/    第 1 章练习 2 参考答案
├── ch02/FetchDemo/           第 2 章正文
├── ch02/Answer1-Enter/       第 2 章练习 1 参考答案
├── ch02/Answer2-MinPrice/    第 2 章练习 2 参考答案
├── ch03/FetchDemo/           第 3 章正文
├── ch03/Answer1-NameLength/  第 3 章练习 1 参考答案
├── ch03/Answer2-Focus/       第 3 章练习 2 参考答案
├── ch04/FetchDemo/           第 4 章正文
├── ch04/Answer1-GetOne/      第 4 章练习 1 参考答案
├── ch04/Answer2-Error500/    第 4 章练习 2 参考答案
├── ch05/FetchDemo/           第 5 章正文
├── ch05/Answer1-Cancel/      第 5 章练习 1 参考答案
├── ch05/Answer2-ConfirmName/ 第 5 章练习 2 参考答案
├── ch06/FetchDemo/           第 6 章正文
├── ch06/Answer1-HideId/      第 6 章练习 1 参考答案
├── ch06/Answer2-FullSave/    第 6 章练习 2 参考答案
```

运行方法（任选一种）：

- Visual Studio 2026：打开项目文件夹里的 `FetchDemo.csproj`，工具栏启动配置选 **http**，按 `Ctrl+F5`。
- 命令行：在项目文件夹（含 `FetchDemo.csproj` 的那一层）执行 `dotnet run`。

然后浏览器打开 `http://localhost:5080/`。
