# -*- coding: utf-8 -*-
"""在第7章之后插入新的第8章：把原第 8~40 章整体后移一位。

处理内容：
1. 各章 H1 标题「第N章」   N>=8 → N+1
2. 小节标题「## N.x」「### N.x」 N>=8 → N+1
3. 正文中的图号「图N-x」（该章自己的图）N>=8 → N+1
4. 全书交叉引用「第N章」「第N～M章」「第N---M章」中所有 N>=8 → N+1
仅改内容；文件重命名由 shell 用 git mv 完成。
用法： python3 src/renumber_insert_ch8.py [--dry]
"""
import glob
import os
import re
import sys

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CH = os.path.join(BASE, "chapters")
DRY = "--dry" in sys.argv

FIRST_MOVED = 8   # 从第8章开始后移


def bump(n):
    return n + 1 if n >= FIRST_MOVED else n


# ---- 1) 交叉引用：第N章 / 第N～M章 / 第N-M章 / 第N---M章 ----
# 捕获「第」+ 数字(+分隔符+数字)* + 「章」
REF = re.compile(r"第\s*(\d+(?:\s*(?:～|~|—|-{1,3}|、|至)\s*\d+)*)\s*章")


def fix_refs(text):
    def repl(m):
        inner = m.group(1)
        # 把其中每个数字按规则 +1，分隔符原样保留
        out = re.sub(r"\d+", lambda d: str(bump(int(d.group()))), inner)
        return "第" + out + "章"
    return REF.sub(repl, text)


# ---- 2) 本章的小节号与图号 ----
def fix_own_numbers(text, old_ch):
    new_ch = old_ch + 1
    lines = text.split("\n")
    out = []
    for ln in lines:
        # 小节标题 ## 8.1 / ### 8.1.2
        m = re.match(r"^(#{2,4}\s*)" + str(old_ch) + r"(\.\d)", ln)
        if m:
            ln = re.sub(r"^(#{2,4}\s*)" + str(old_ch) + r"(\.\d)",
                        r"\g<1>" + str(new_ch) + r"\g<2>", ln, count=1)
        # 图号：图8-1 → 图9-1（只改本章号）
        ln = re.sub(r"图\s*" + str(old_ch) + r"-(\d)",
                    "图" + str(new_ch) + r"-\1", ln)
        # 示例号：示例8-1 → 示例9-1（只改本章号）
        ln = re.sub(r"示例\s*" + str(old_ch) + r"-(\d)",
                    "示例" + str(new_ch) + r"-\1", ln)
        out.append(ln)
    return "\n".join(out)


files = sorted(glob.glob(os.path.join(CH, "*.md")))
report = []
for path in files:
    name = os.path.basename(path)
    m = re.match(r"^(\d{2})-", name)
    if not m:
        continue
    prefix = int(m.group(1))
    text = original = open(path, encoding="utf-8").read()

    # 文件前缀 = 章号 + 1（第1章 → 02）；附录前缀 >= 42
    chapter_no = prefix - 1 if 2 <= prefix <= 41 else None

    # 先修本章自己的编号（避免被交叉引用规则重复处理标题）
    if chapter_no is not None and chapter_no >= FIRST_MOVED:
        text = fix_own_numbers(text, chapter_no)

    # 再修全书交叉引用
    text = fix_refs(text)

    if text != original:
        n_refs = len(REF.findall(original))
        report.append((name, n_refs))
        if not DRY:
            open(path, "w", encoding="utf-8").write(text)

print(("[试运行] " if DRY else "") + f"内容有变更的文件：{len(report)} 个")
for name, n in report:
    print(f"  {name}（含 {n} 处「第N章」引用）")
