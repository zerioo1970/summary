---
inclusion: auto
---

# Minimal API 教程仓库 —— 工作约定

本仓库（`zerioo1970/summary`）存放两套 ASP.NET Core 10 Minimal API 教程。**动手前先读根目录 `README.md`**，它区分了两套教程并写明了修改流程。

## 关键事实（勿混淆两套教程）

| | 40 章版 | 21 章版 |
|---|---|---|
| 文件夹 | `minimal-api-40章版/` | `dotnet10-最小API教程/` |
| 来源 | 用户自己上传的 Word，后由 Kiro 转成分章 MD | Kiro 从零编写 |
| 规模 | 40 章 + 3 附录，56 图，约 23 万字 | 21 章 + 4 附录，34 图，约 5 万字 |
| 定位 | 内容主线 | 精炼版；独有 Windows+IIS 部署实操、前端 12 种调用方法 |

用户提到"教程"时，**先确认指哪一套**（除非上下文已明确）。

## 修改流程（不要从头重跑）

1. 环境若缺（沙箱重置后 pandoc / 中文字体 / python-docx 会丢失）：`bash setup.sh`，约 1 分钟。**不要自己一步步摸索安装。**
2. 只改 `chapters/` 下**对应那一个 .md 文件**。
3. 重新合成：`bash <教程文件夹>/build.sh`。
4. 提交推送到分支 `add-minimal-api-tutorial`（PR #9）。

**不需要**每次重新生成图片、重新拆章、重跑全流程。

## 技术要点

- Word 由 pandoc 从 markdown 合成；样式来自 `template/reference.docx`（由 `src/patch_reference.py` 定制：中文字体、代码块灰底、引用块作提示框、A4、页脚）。
- 目录由 pandoc `--toc` 自动生成（可点击），**不要**在 MD 里手写静态目录。
- 封面靠 `chapters/00-封面.md` 的 YAML 元数据（title/subtitle/author/date）。
- 章节顺序由文件名数字前缀决定；插入新章需重排前缀，并同步正文里的"第 N 章"交叉引用与小节号（21 章版有 `src/renumber.py` 可参考）。
- 图片在 MD 中以 `../images/xxx.png` 引用（build.sh 在 `chapters/` 目录内执行 pandoc）。
- 提示框写法：`> **【标签】** 正文`（渲染为带色边框的引用块）。
- 画图字体 `/projects/sandbox/fonts/NotoSansSC.ttf`，**不含 emoji**，图中勿用 emoji 字符（用绘制的色块代替）。

## 环境坑（已踩过，勿重复排查）

- 该系统 `dnf` 没有 pandoc 包，也没有 LibreOffice；pandoc 用官方静态二进制（setup.sh 已处理）。
- 沙箱可能在会话之间重置：本地 git 对象曾损坏过。若 `git status` 报 `bad object HEAD`，直接重新 clone 分支即可（远程是完整的），本地工作区文件先备份到仓库外再恢复。
- 用户在浏览器中，无文件系统访问权限，**不要**让其"打开文件""看编辑器"；需要给内容就贴进聊天，或给 GitHub raw 下载链接（中文路径需 percent-encode）。
