# -*- coding: utf-8 -*-
"""把 pandoc 从 docx 转出的 _full.md 按章拆成单独的 Markdown 文件。

- 前置信息 -> 00-封面.md（YAML 元数据，供 pandoc 生成标题页）
- "如何使用本教程" -> 01-...
- 第1~40章 -> 02 ~ 41
- 附录A/B/C、参考资料、审阅说明 -> 42 ~ 46
- 原静态"目录"章节丢弃（合成时用 pandoc --toc 生成可点击目录）
- 图片路径 _media/media/xxx.png -> ../images/xxx.png
"""
import os
import re

BASE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(BASE)
FULL = os.path.join(ROOT, "_full.md")
OUT = os.path.join(ROOT, "chapters")
os.makedirs(OUT, exist_ok=True)

text = open(FULL, encoding="utf-8").read()

# 图片路径改为 ../images/
text = text.replace("_media/media/", "../images/")

lines = text.split("\n")

# 找出所有 H1 行号
h1 = [(i, l[2:].strip()) for i, l in enumerate(lines) if l.startswith("# ")]

# 前置部分（第一个 H1 之前）
front = "\n".join(lines[: h1[0][0]]).strip()

# 切出每个 H1 段落
sections = []
for idx, (ln, title) in enumerate(h1):
    end = h1[idx + 1][0] if idx + 1 < len(h1) else len(lines)
    body = "\n".join(lines[ln:end]).rstrip()
    sections.append((title, body))


def safe(name):
    """生成文件系统安全的短名。"""
    n = name.replace("　", "-").replace("：", "-").replace(":", "-")
    n = n.replace("/", "-").replace("\\", "-").replace(" ", "")
    n = re.sub(r"-+", "-", n).strip("-")
    return n[:40]


# ---- 00 封面：用 YAML 元数据承载书名/副标题 ----
cover = """---
title: "ASP.NET Core 10 Minimal API 教程"
subtitle: "从 Hello World 到前后端交互与首次部署　·　第1至40章 · Kestrel专题与三项目附录版"
author: "适用版本：.NET SDK 10.x / ASP.NET Core 10 / C# 14"
date: "编写日期：2026-07-18"
---

> **【阅读提示】** 本文档中的项目名称、端口和路径均可替换。命令示例同时兼顾 PowerShell、命令提示符和常见 Unix Shell；若命令有差异，会单独说明。
"""
open(os.path.join(OUT, "00-封面.md"), "w", encoding="utf-8").write(cover)
print("written 00-封面.md")

# ---- 其余章节 ----
seq = 1
written = 0
for title, body in sections:
    if title.strip() == "目录":
        print("  (跳过静态目录，合成时由 pandoc --toc 生成)")
        continue
    fname = f"{seq:02d}-{safe(title)}.md"
    open(os.path.join(OUT, fname), "w", encoding="utf-8").write(body + "\n")
    print(f"written {fname}")
    seq += 1
    written += 1

print(f"\n共写出 {written + 1} 个文件（含封面）")
