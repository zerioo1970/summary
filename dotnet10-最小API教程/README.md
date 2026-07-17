# ASP.NET Core 最小 API 完全教程（.NET 10）

面向新手的图文并茂教程，从零基础到进阶，覆盖 .NET 10 最小 API（Minimal API）的方方面面。

## 文件说明

- **`ASP.NET-Core-最小API完全教程.docx`** —— 教程正文（Word 文档，图文并茂，可直接用 Word / WPS 打开）。
- **`src/`** —— 生成该 Word 文档的 Python 脚本与示意图，便于复现与续写。
  - `docbuilder.py` —— 排版与画图工具库
  - `diagrams_p1.py` —— 第 1、2 章示意图生成脚本
  - `build_batch1.py` —— 文档正文构建脚本（封面 + 目录 + 第 1、2 章）
  - `images/` —— 已生成的示意图

## 当前进度

- [x] 封面 + 完整目录（21 章 + 附录）
- [x] 第 1 章　认识最小 API
- [x] 第 2 章　前端与后端如何协作
- [ ] 第 3 章起　持续续写中……

## 复现方式

```bash
pip install python-docx matplotlib
# 需准备中文字体（Noto Sans CJK 等），并在 docbuilder.py 中配置 FONT_PATH
cd src
python3 build_batch1.py
```

> 环境版本：.NET 10 SDK / C# 14。编写工具 Visual Studio 2026 或 VS Code 均可。
