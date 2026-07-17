# ASP.NET Core 最小 API 完全教程（.NET 10）

面向新手的图文并茂教程，从零基础到进阶实战，覆盖 .NET 10 最小 API（Minimal API）的方方面面。全书 **5 部分 20 章 + 4 附录**，含大量可运行代码示例与 31 张示意图。

## 如何阅读

- **想要成品 Word**：直接看 `ASP.NET-Core-最小API完全教程.docx`（用 Word / WPS 打开，图文并茂、带可点击目录）。
- **想在 GitHub 上直接读**：进入 `chapters/` 目录，每章一个 Markdown 文件，可在线渲染（含图片、代码高亮）。

## 目录结构

```
dotnet10-最小API教程/
├── ASP.NET-Core-最小API完全教程.docx   # 合成后的成品 Word
├── chapters/                          # 教程正文，每章一个 Markdown
│   ├── 00-封面.md                     # 书名/副标题（pandoc 元数据）
│   ├── 01-认识最小API.md ~ 20-综合实战.md
│   └── 21~24-附录A~D
├── images/                            # 全部示意图（31 张 PNG）
├── template/reference.docx            # pandoc 样式模板（中文字体/代码块/提示框）
├── build.sh                           # 一键把 md + 图片合成 Word
└── src/                               # 生成示意图的 Python 脚本
    ├── docbuilder.py                  # 画图工具库
    ├── diagrams_p1/p2/p3.py           # 各章示意图生成脚本
    └── patch_reference.py             # 定制 pandoc 样式模板
```

## 全书目录

**第一部分 入门与原理**：1 认识最小 API · 2 运行原理与请求处理管线 · 3 环境搭建与第一个程序
**第二部分 核心功能**：4 路由 · 5 参数绑定 · 6 返回结果 · 7 依赖注入
**第三部分 数据与校验**：8 请求校验（.NET 10 新特性）· 9 EF Core
**第四部分 文档/安全/实时**：10 OpenAPI（.NET 10 新特性）· 11 认证与授权 · 12 端点过滤器与中间件 · 13 SSE（.NET 10 新特性）
**第五部分 工程化与进阶**：14 配置日志 · 15 CORS/限流/健康检查 · 16 版本控制 · 17 测试 · 18 组织大型项目 · 19 部署与性能 · 20 综合实战
**附录**：A 前后端如何协作（含所有前端调用方法）· B 速查表 · C Controller 迁移对照 · D 常见错误排查

## 重新生成 Word

需要 `pandoc` 与 Python（`python-docx`、`matplotlib`）及一款中文字体。

```bash
# 1) 生成示意图（输出到 images/）
cd src
python3 patch_reference.py     # 定制样式模板（首次或改样式时）
python3 diagrams_p1.py
python3 diagrams_p2.py
python3 diagrams_p3.py
cd ..

# 2) 合成 Word（在项目根目录）
bash build.sh
```

> 环境版本：.NET 10 SDK / C# 14。编写工具 Visual Studio 2026 或 VS Code 均可。
> 说明：`docbuilder.py` 中的字体路径 `FONT_PATH` 需指向本机的中文字体（如 Noto Sans CJK）。
