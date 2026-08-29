# 企业微信开发 API 新手教程

## 教程定位

面向第一次接触企业微信开发 API 的开发者，采用以下技术：

- **Python**：从基础消息与素材上传，逐步扩展到通讯录双向同步、企业内部项目群、Excel/考勤报表、定时任务、可靠性治理和回调增量同步
- **C# ASP.NET WebForms**：在第 6～11 章独立完成员工查询、身份登录、JS-SDK 和回调处理
- **React 19 + TypeScript + Vite**：在第 14 章构建手机端 H5
- **Ant Design Mobile**：提供适合企业微信手机端的交互组件
- **.NET 10 ASP.NET Core Minimal API**：在第 14 章负责 OAuth、企业微信 API、JS-SDK 签名和 SQL Server API
- **IIS**：部署 WebForms，以及通过 ANCM 承载 .NET 10 应用
- **SQL Server**：保存通讯录镜像、HR 主数据、消息任务、投递状态、考勤记录和业务日志

## 核心开发原则

同一个服务端业务只保留一套实现：

- Python 功能直接调用企业微信 API，不调用 C# WebForms 或 .NET 10 API
- C# WebForms 功能直接调用企业微信 API，不调用 Python
- 第 14 章的 React 只负责 UI、路由和调用本站 Minimal API，不直接调用需要 Secret 的企业微信服务端接口
- Secret、`access_token`、ticket 和数据库连接串只保存在服务端；浏览器只接收必要的业务 DTO 与签名结果
- 各服务端实现可以读写同一个 SQL Server，但不通过命令行互相调用
- Python、WebForms 和 .NET 10 各自管理自己的 `access_token` 缓存
- 第 11 章 C# 回调与第 24 章 Python 回调是两条**独立替代路线**：同一套企业微信回调配置只能选择一个消费者，不能让两者竞争同一 URL 或同一个 Inbox

## 目录

| 章节 | 语言 | 前置条件 |
|---|---|---|
| [01 开发准备与自建应用](./docs/01-开发准备与自建应用.md) | 配置 | 无 |
| [02 第一个示例：Python 发一条消息](./docs/02-第一个示例-Python发一条消息.md) | Python | 01 |
| [03 SQL Server 数据设计](./docs/03-SQL-Server数据设计.md) | SQL | 01 |
| [04 Python 群发通知](./docs/04-Python群发通知.md) | Python | 02、03 |
| [05 素材上传与发送图片文件](./docs/05-素材上传与发送图片文件.md) | Python | 02、04 |
| [06 C# WebForms 员工查询](./docs/06-CSharp-WebForms员工查询.md) | C# | 01、03 |
| [07 应用主页与可信域名](./docs/07-应用主页与可信域名.md) | 配置 | **需 HTTPS 域名** |
| [08 OAuth2.0 网页授权与身份登录](./docs/08-OAuth2.0网页授权与身份登录.md) | C# | 07 |
| [09 JS-SDK 接入与签名](./docs/09-JS-SDK接入与签名.md) | C# + JS | 07、08 |
| [10 JS-SDK 定位与经纬度转地址](./docs/10-JS-SDK定位与经纬度转地址.md) | C# + JS | 09 |
| [11 C# WebForms 回调处理](./docs/11-CSharp-WebForms回调处理.md) | C# | 07 |
| [12 部署与故障排查](./docs/12-部署与故障排查.md) | 运维 | 全部 |
| [13 Windows Server 2022 从零搭建](./docs/13-Windows-Server-2022搭建IIS服务器.md) | 运维 | 无（可最先读） |
| [14 React 19 + TypeScript + Vite + Ant Design Mobile + .NET 10 Minimal API](./docs/14-React19-TypeScript-Vite-AntDesignMobile-DotNet10-MinimalAPI.md) | TypeScript + C# | 01、03、07、08、09（12、13 按需） |
| [15 Python 修改企业微信通讯录](./docs/15-Python修改企业微信通讯录.md) | Python | 01、02 |
| [16 Python 同步企业微信通讯录到 SQL Server](./docs/16-Python同步企业微信通讯录到SQL-Server.md) | Python + SQL | 03、15 |
| [17 Python 从 SQL Server 同步通讯录到企业微信](./docs/17-Python从SQL-Server同步通讯录到企业微信.md) | Python + SQL | 15、16 |
| [18 Python 创建与管理企业内部项目群](./docs/18-Python创建与管理企业内部项目群.md) | Python | 15 |
| [19 Python 从 SQL Server 读取任务并发送项目群消息](./docs/19-Python从SQL-Server读取任务并发送项目群消息.md) | Python + SQL | 18 |
| [20 Python 生成 Excel 日报并发送](./docs/20-Python生成Excel日报并发送.md) | Python + SQL | 05、18、19 |
| [21 Python 获取打卡数据与生成考勤报表](./docs/21-Python获取打卡数据与生成考勤报表.md) | Python + SQL | 15、20 |
| [22 Python 使用 APScheduler 实现定时任务](./docs/22-Python使用APScheduler实现定时任务.md) | Python | 16～21（按需） |
| [23 Python 失败重试、幂等与日志脱敏](./docs/23-Python失败重试幂等与日志脱敏.md) | Python + SQL | 15～22 |
| [24 Python 回调事件与增量通讯录同步](./docs/24-Python回调事件与增量通讯录同步.md) | Python + SQL | 07、11、16、23 |

## 从裸机开始的读者请注意

**第 13 章是从零搭建服务器的操作手册，它应该在第 7 章之前做**，编号靠后只是因为后补。第 13 章覆盖 Windows Server、IIS、DNS、证书等通用基础，但明确不负责 ASP.NET Core 承载；第 14 章会继续安装 .NET 10 Hosting Bundle、配置 ANCM 和 No Managed Code 应用池。

```text
第 1 到 6 章（本机开发，无需服务器）
      ↓
第 13 章（Windows Server + IIS + SSL 通用基础）
      ↓
第 7 到 12 章（WebForms 路线）

或：01、03、07～09 的原理与配置
      ↓
第 14 章（React H5 + .NET 10 完整实战）
```

## 三个阶段

**阶段一（01–06）：不需要域名**

在本机就能完成。跑通 Python 发消息、素材上传、建库、群发、员工查询。

**阶段二（07–10）：必须有 HTTPS 域名**

从第 7 章开始，必须有外网可访问的 HTTPS 域名。应用主页、免登录、JS-SDK 定位都依赖这个条件。

**阶段三（11–12）：上线**

回调接收和生产部署。

**独立现代栈实战（14）**

第 14 章不依赖 WebForms 源码，从空项目开始建立 React 手机端、.NET 10 Minimal API、OAuth、通讯录同步、SQL Server、JS-SDK 扫码和 IIS 同源部署。建议先完成第 1、3、7～9 章的配置和原理部分；服务器通用准备参考第 13 章。

**现代 Python 企业微信实战（15–24）**

第 15～24 章统一复用第 15 章的 Python 客户端，从通讯录修改开始，依次完成企业微信与 SQL Server 的双向同步、企业内部项目群、任务消息、Excel 日报、考勤报表、APScheduler 调度、失败重试与幂等，以及 FastAPI 回调增量同步。第 16 章的入站镜像与第 17 章的 HR 出站主数据严格分离，避免双向同步形成数据循环；第 24 章是第 11 章 C# 回调的替代实现，不应同时启用。

## 编写方式

正文采用**递进式**：从几行的最简例子开始，每一版只加一两个功能点，先说明「上一版有什么问题」，再给代码，然后逐行解释新增部分，最后整合成可复用模块。

原理讲解穿插在需要它的版本里，配竖向窄图说明。

## 关于经纬度转地址

企业微信只提供经纬度，**不提供文字地址**。转换需要额外调用腾讯位置服务等地图服务，要单独申请密钥。详见第 10 章。

## 当前编写状态

| 章节 | 状态 | 规模 |
|---|---|---|
| 01 | 已完成 | 841 行，13 图 |
| 02 | 已完成 | 13 个版本递进，16 个功能点 |
| 03 | 已完成 | 11 个版本递进，7 张表 |
| 04 | 已完成 | 12 个版本递进 |
| 05 | 已完成 | 11 个版本递进 |
| 06 | 已完成 | 12 个版本递进 |
| 07 | 已完成 | 10 个版本递进 |
| 08 | 已完成 | 12 个版本递进 |
| 09 | 已完成 | 11 个版本递进 |
| 10 | 已完成 | 11 个版本递进 |
| 11 | 已完成 | 11 个版本递进 |
| 12 | 已完成 | 按上线阶段组织，含错误码速查与故障决策树 |
| 13 | 已完成 | 10 个阶段，每阶段一道验证关卡 |
| 14 | 已完成 | 12 个版本递进，React + Minimal API 端到端实战 |
| 15 | 已完成 | 963 行，通讯录修改与统一 Python 客户端 |
| 16 | 已完成 | 976 行，企业微信通讯录全量镜像入库 |
| 17 | 已完成 | 1551 行，HR 主数据出站同步与 Outbox |
| 18 | 已完成 | 649 行，企业内部应用群创建与管理 |
| 19 | 已完成 | 776 行，SQL Server 消息任务与未知结果处理 |
| 20 | 已完成 | 963 行，Excel 日报与双目标独立投递状态 |
| 21 | 已完成 | 619 行，打卡数据与考勤报表 |
| 22 | 已完成 | 575 行，APScheduler 独立调度进程 |
| 23 | 已完成 | 723 行，失败重试、幂等与日志脱敏 |
| 24 | 已完成 | 1174 行，FastAPI 回调与增量通讯录同步 |

**全部 24 章编写完成。**

## 编写标准

- 递进式：每版只加一两个功能点，用「上一版的问题」引出
- 每个新增行都解释作用，不只给代码
- 原理配图，图宽不超过 2 列节点
- 现代前后端章节标明浏览器、Minimal API、企业微信和 SQL Server 中哪一层发生变化
- 每章结尾有完整请求流、自测表和勾选式完成标准
- 明确列出会踩的坑：现象、原因、解决办法
