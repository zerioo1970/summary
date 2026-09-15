# ASP.NET Core 10 Minimal API 教程（第 1 至 41 章）

从 Hello World 到前后端交互与首次部署 · Kestrel 专题与三项目附录版
适用版本：.NET SDK 10.x / ASP.NET Core 10 / C# 14 ｜ 编写日期：2026-07-18

全书 **41 章 + 3 附录**，含 58 张插图、271 个表格、278 段代码。

## 如何阅读

- **看成品 Word**：`ASP.NET-Core-10-Minimal-API-教程-第1至41章.docx`（由下面的 Markdown 合成，带可点击目录）
- **在 GitHub 上直接读**：进入 `chapters/`，每章一个 Markdown 文件，可在线渲染（图片、代码高亮都有）
- **原始上传版**：`ASP.NET-Core-10-Minimal-API-教程-第1至40章-附录C前端交互完整版.docx`（原封不动保留作为存档）

## 目录结构

```
minimal-api-40章版/
├── ASP.NET-Core-10-Minimal-API-教程-第1至41章.docx          # 由 MD 合成的成品
├── ...-附录C前端交互完整版.docx                              # 原始上传存档（未改动）
├── chapters/          # 48 个 Markdown（封面 + 使用说明 + 41 章 + 3 附录 + 参考资料/审阅说明）
├── images/            # 58 张插图
├── template/reference.docx   # pandoc 样式模板（中文字体/代码块/提示框/A4）
├── build.sh           # 一键把 md + 图片合成 Word
└── src/               # 转换与维护脚本
    ├── split_chapters.py   # 把 docx 转出的整篇 md 按章拆分
    ├── fix_codeblocks.py   # 把「示例」后的纯文本代码还原成代码围栏
    └── patch_reference.py  # 定制 pandoc 样式模板
```

## 全书目录

**基础入门（1–9）**：1 .NET 10 与 Minimal API · 2 准备开发环境 · 3 Hello World · 4 Kestrel 专题 · 5 VS Code 与 Visual Studio · 6 OpenAPI 3.1 与 Swagger · 7 第一次发布和部署 · **8 一台服务器部署多个 Minimal API（FDD + 独立目录/端口 + 服务托管）** · 9 前后端最常用的交互操作

**核心机制（10–15）**：10 路由与 HTTP 方法 · 11 参数绑定 · 12 返回值与响应生成 · 13 HttpContext · 14 依赖注入 · 15 配置、环境与密钥

**工程能力（16–24）**：16 日志 · 17 中间件与请求管道 · 18 Endpoint Filter · 19 .NET 10 内置验证 · 20 异常处理与统一错误响应 · 21 RESTful 设计 · 22 EF Core 10 · 23 DTO、映射与分层 · 24 拆分膨胀的 Program.cs

**安全与进阶（25–33）**：25 身份认证 · 26 授权与权限模型 · 27 Web API 安全 · 28 文件、流与实时响应 · 29 缓存 · 30 后台任务与消息处理 · 31 调用外部服务 · 32 性能原理与优化 · 33 健康检查与可观测性

**交付（34–38）**：34 自动化测试 · 35 代码质量与团队协作 · 36 高级发布与生产运行 · 37 Docker 容器化 · 38 CI/CD

**综合实战（39–41）**：39 需求与架构设计 · 40 实现业务功能 · 41 加固、测试与交付

**附录**：A TodoLite 单文件任务清单 · B TaskBoard 数据库任务系统 · C 从原生 fetch 到 Axios 与 React 19

## 重新生成 Word

需要 `pandoc` 与 Python 的 `python-docx`。

```bash
# 首次或修改样式时
python3 src/patch_reference.py

# 合成 Word
bash build.sh
```

## 转换说明

这套 Markdown 由原始 Word 文档转换而来，便于逐章修改与版本管理。转换时做了两处处理：

1. **静态目录已移除**，改由 pandoc `--toc` 生成可点击目录，避免重复。
2. **代码还原为代码围栏**：原 Word 用普通段落排版代码，转换后已还原成 ```` ``` ```` 代码块并自动标注语言，因此在 GitHub 上有语法高亮、在 Word 里显示为代码样式。

已校验：正文文字 100% 保留，插图 56 张、章节 46 个（不含原静态目录）全部完整。

> 注：原 Word 文档中的代码未保留缩进，转换后同样没有缩进，这是源文档的限制。
