# -*- coding: utf-8 -*-
"""修掉每张图显示两行图注的问题。

原始 Word 转换后，每张图变成：
    ![某段替代文字](../images/imageN.png){width=... height=...}

    图10-1　正确的图注

pandoc 会把「替代文字」渲染成图注，正文里那行「图10-1　…」又是一个段落，
于是 Word 里出现两行图注，而且替代文字里的编号还是旧的。

做法：把正文那行图注作为唯一图注写进替代文字，并删掉重复的段落。
这样 pandoc 只生成一个图注，且自动套用模板里的 Image Caption 样式（居中、灰色斜体）。

用法： python3 src/fix_captions.py [--dry]
"""
import glob
import os
import re
import sys

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CH = os.path.join(BASE, "chapters")
DRY = "--dry" in sys.argv

IMG = re.compile(r'^!\[(?P<alt>[^\]]*)\]\((?P<path>[^)]+)\)(?P<attr>\{[^}]*\})?\s*$')
# 图注：图10-1 / 图4-2A / 图A-1 / 图C-3
CAP = re.compile(r'^图\s*[0-9A-Za-z]+\s*-\s*[0-9A-Za-z]+[　\s]')

fixed_total = 0
report = []

for path in sorted(glob.glob(os.path.join(CH, "*.md"))):
    lines = open(path, encoding="utf-8").read().split("\n")
    out = []
    i = 0
    n_fixed = 0
    while i < len(lines):
        m = IMG.match(lines[i].strip())
        if not m:
            out.append(lines[i])
            i += 1
            continue

        # 向后找第一个非空行，看是否为图注段落
        j = i + 1
        while j < len(lines) and lines[j].strip() == "":
            j += 1

        if j < len(lines) and CAP.match(lines[j].strip()):
            caption = lines[j].strip()
            attr = m.group("attr") or ""
            # 用正文图注作为唯一图注（写进替代文字）
            out.append(f'![{caption}]({m.group("path")}){attr}')
            # 跳过原图注段落及其前面的空行，但保留一个空行分隔
            out.append("")
            i = j + 1
            # 跳过图注后紧跟的空行，避免出现连续多空行
            while i < len(lines) and lines[i].strip() == "":
                i += 1
            n_fixed += 1
        else:
            out.append(lines[i])
            i += 1

    if n_fixed:
        report.append((os.path.basename(path), n_fixed))
        fixed_total += n_fixed
        if not DRY:
            open(path, "w", encoding="utf-8").write("\n".join(out))

print(("[试运行] " if DRY else "") + f"共处理 {fixed_total} 张图，涉及 {len(report)} 个文件")
for name, n in report[:12]:
    print(f"  {name}: {n} 张")
if len(report) > 12:
    print(f"  ……另有 {len(report)-12} 个文件")
