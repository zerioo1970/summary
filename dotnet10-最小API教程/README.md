# ASP.NET Core 最小 API 完全教程（.NET 10）

面向新手的图文并茂教程，从零基础到进阶，覆盖 .NET 10 最小 API（Minimal API）的方方面面。

## 文件说明

- **`ASP.NET-Core-最小API完全教程.docx`** —— 教程正文（Word 文档，图文并茂，可直接用 Word / WPS 打开）。
- **`src/`** —— 生成该 Word 文档的 Python 脚本与示意图，便于复现与续写。
  - `docbuilder.py` —— 排版与画图工具库
  - `diagrams_p1.py` —— 第 1 章与附录 A 示意图生成脚本
  - `diagrams_p2.py` —— 第 2、3 章及附录 A 补充示意图脚本
  - `build_book.py` —— 文档总构建脚本（每次运行重生成整本 docx）
  - `images/` —— 已生成的示意图

## 当前进度

- [x] 封面 + 完整目录（20 章 + 4 附录）
- [x] 第 1 章　认识最小 API
- [x] 第 2 章　运行原理与请求处理管线
- [x] 第 3 章　环境搭建与第一个程序
- [x] 附录 A　前端与后端如何协作（含所有前端调用方法与示例）
- [ ] 第 4 章起（路由、参数绑定、返回结果……）持续续写中

## 复现方式

```bash
pip install python-docx matplotlib
# 需准备中文字体（Noto Sans CJK 等），并在 docbuilder.py 中配置 FONT_PATH
cd src
python3 diagrams_p1.py    # 生成第 1 批图
python3 diagrams_p2.py    # 生成第 2 批图
python3 build_book.py     # 生成 Word 文档
```

> 环境版本：.NET 10 SDK / C# 14。编写工具 Visual Studio 2026 或 VS Code 均可。
