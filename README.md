# summary —— 资料仓库

本仓库存放两套 **ASP.NET Core 10 Minimal API 教程**，以及一份 SQL 开发规则文档。

---

## ⚠️ 先分清：这里有两套教程，不要搞混

| | 📗 **40 章版** | 📘 **21 章版** |
|---|---|---|
| **文件夹** | `minimal-api-40章版/` | `dotnet10-最小API教程/` |
| **来源** | **用户自己上传**（2026-07-18，经 GitHub 网页上传） | **Kiro 在对话中从零编写** |
| **规模** | 40 章 + 3 附录，56 图，271 表 | 21 章 + 4 附录，34 图，25 表 |
| **正文量** | 约 23 万字 | 约 5 万字 |
| **定位** | **内容主线**，体系完整详尽 | 精炼版，含两块独有内容 |
| **独有内容** | Kestrel 专题、缓存、后台任务、调用外部服务、CI/CD、Docker、三个完整示例项目（TodoLite / TaskBoard / React 19 附录 C） | Windows + IIS 部署实操（EXE vs DLL、应用池设置、注册 Windows 服务）、前端调用后端的 12 种方法对照 |

> **一句话**：想系统学 → 看 **40 章版**；想查"怎么部署到 IIS"或"前端有哪些调用方式" → 看 **21 章版**。

两套**都已拆成分章 Markdown**，可以逐章修改后重新合成 Word。

---

## 📥 下载

### 40 章版
- **Word（推荐，由 MD 合成，带可点击目录 + 278 个代码块）**：`minimal-api-40章版/ASP.NET-Core-10-Minimal-API-教程-第1至40章.docx`
- 原始上传存档（未改动）：`minimal-api-40章版/ASP.NET-Core-10-Minimal-API-教程-第1至40章-附录C前端交互完整版.docx`
- 完整打包：`40章版-完整打包.zip`

### 21 章版
- **Word**：`dotnet10-最小API教程/ASP.NET-Core-最小API完全教程.docx`
- 完整打包：`21章版-完整打包.zip`

> ZIP 是快照，Word 会随 MD 修改而更新；改动后需重新打包 ZIP。

---

## 🛠 如何修改内容（重要）

两套教程的正文都在各自的 `chapters/` 目录里，**每章一个 Markdown 文件**。修改流程：

```bash
# 0) 沙箱重置后先恢复环境（约 1 分钟）
bash setup.sh

# 1) 改 chapters/ 下对应的那一个 .md 文件

# 2) 重新合成 Word
bash minimal-api-40章版/build.sh        # 40 章版
bash dotnet10-最小API教程/build.sh      # 21 章版
```

**只改动对应章节的那一个 MD 文件即可**，不必重跑全流程、不必重新生成图片。

### 目录结构（两套一致）

```
<教程文件夹>/
├── chapters/          # 正文，每章一个 .md（文件名前缀决定合成顺序）
├── images/            # 插图
├── template/reference.docx   # pandoc 样式模板（中文字体/代码块/提示框/A4）
├── build.sh           # 一键把 md + 图片合成 Word
└── src/               # 维护脚本（画图、拆章、样式定制）
```

### 何时需要跑 src/ 里的脚本

平时改文字**不需要**。仅在以下情况：

| 场景 | 命令 |
|---|---|
| 要改/新增示意图（仅 21 章版有生成脚本） | `cd dotnet10-最小API教程/src && python3 diagrams_p1.py`（p1~p4） |
| 要改 Word 整体样式（字体、配色、代码块底色） | `python3 <教程>/src/patch_reference.py` |
| 40 章版要从原始 docx 重新拆章 | `minimal-api-40章版/src/split_chapters.py` + `fix_codeblocks.py` |

---

## 📄 其它文件

- `SQL语句转换为存储过程规则.md` —— C# ASP.NET WebForm + SQL Server 环境下，把页面内联 SQL 改写为存储过程的固定规则。

---

## 📌 环境说明

沙箱重置后 pandoc、中文字体、Python 依赖会丢失，**运行 `bash setup.sh` 即可全部恢复**，无需从头摸索。字体路径 `/projects/sandbox/fonts/NotoSansSC.ttf` 被 `src/docbuilder.py` 引用，勿改动。
