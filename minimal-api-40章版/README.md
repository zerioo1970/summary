# ASP.NET Core 10 Minimal API 教程（第 1 至 40 章）

从 Hello World 到前后端交互与首次部署 · Kestrel 专题与三项目附录版
适用版本：.NET SDK 10.x / ASP.NET Core 10 / C# 14 ｜ 编写日期：2026-07-18

全书 **40 章 + 3 附录**，含 56 张插图、271 个表格、278 段代码。

## 如何阅读

- **看成品 Word**：`ASP.NET-Core-10-Minimal-API-教程-第1至40章.docx`（由下面的 Markdown 合成，带可点击目录）
- **在 GitHub 上直接读**：进入 `chapters/`，每章一个 Markdown 文件，可在线渲染（图片、代码高亮都有）
- **原始上传版**：`ASP.NET-Core-10-Minimal-API-教程-第1至40章-附录C前端交互完整版.docx`（原封不动保留作为存档）

## 目录结构

```
minimal-api-40章版/
├── ASP.NET-Core-10-Minimal-API-教程-第1至40章.docx          # 由 MD 合成的成品
├── ...-附录C前端交互完整版.docx                              # 原始上传存档（未改动）
├── chapters/          # 47 个 Markdown（封面 + 使用说明 + 40 章 + 3 附录 + 参考资料/审阅说明）
├── images/            # 56 张插图
├── template/reference.docx   # pandoc 样式模板（中文字体/代码块/提示框/A4）
├── build.sh           # 一键把 md + 图片合成 Word
└── src/               # 转换与维护脚本
    ├── split_chapters.py   # 把 docx 转出的整篇 md 按章拆分
    ├── fix_codeblocks.py   # 把「示例」后的纯文本代码还原成代码围栏
    └── patch_reference.py  # 定制 pandoc 样式模板
```

## 全书目录

**基础入门（1–8）**：1 .NET 10 与 Minimal API · 2 准备开发环境 · 3 Hello World · 4 Kestrel 专题 · 5 VS Code 与 Visual Studio · 6 OpenAPI 3.1 与 Swagger · 7 第一次发布和部署 · 8 前后端最常用的交互操作

**核心机制（9–14）**：9 路由与 HTTP 方法 · 10 参数绑定 · 11 返回值与响应生成 · 12 HttpContext · 13 依赖注入 · 14 配置、环境与密钥

**工程能力（15–23）**：15 日志 · 16 中间件与请求管道 · 17 Endpoint Filter · 18 .NET 10 内置验证 · 19 异常处理与统一错误响应 · 20 RESTful 设计 · 21 EF Core 10 · 22 DTO、映射与分层 · 23 拆分膨胀的 Program.cs

**安全与进阶（24–32）**：24 身份认证 · 25 授权与权限模型 · 26 Web API 安全 · 27 文件、流与实时响应 · 28 缓存 · 29 后台任务与消息处理 · 30 调用外部服务 · 31 性能原理与优化 · 32 健康检查与可观测性

**交付（33–37）**：33 自动化测试 · 34 代码质量与团队协作 · 35 高级发布与生产运行 · 36 Docker 容器化 · 37 CI/CD

**综合实战（38–40）**：38 需求与架构设计 · 39 实现业务功能 · 40 加固、测试与交付

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
