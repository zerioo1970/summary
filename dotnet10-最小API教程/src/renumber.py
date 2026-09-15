# -*- coding: utf-8 -*-
"""在 Part1 末尾插入新第4章后，把原 4~20 章整体后移一位（含标题、小节号、交叉引用）。
仅改文件内容；文件改名单独用 git mv 处理。"""
import re, os, glob

CH = "/projects/sandbox/summary/dotnet10-最小API教程/chapters"


def bump(n):
    return n + 1 if 4 <= n <= 20 else n


def fix_crossrefs(text):
    def repl(m):
        parts = re.split(r'([/、])', m.group(1))
        out = []
        for tok in parts:
            out.append(str(bump(int(tok))) if tok.isdigit() else tok)
        return '第 ' + ''.join(out) + ' 章'
    return re.sub(r'第\s*(\d+(?:[/、]\d+)*)\s*章', repl, text)


def fix_section_headings(text, old_ch):
    new_ch = old_ch + 1
    out = []
    for line in text.split('\n'):
        if re.match(r'^(#{2,3}\s*)' + str(old_ch) + r'\.', line):
            line = re.sub(r'^(#{2,3}\s*)' + str(old_ch) + r'\.',
                          r'\g<1>' + str(new_ch) + '.', line, count=1)
        out.append(line)
    return '\n'.join(out)


for path in sorted(glob.glob(os.path.join(CH, "*.md"))):
    name = os.path.basename(path)
    m = re.match(r'^(\d{2})-', name)
    if not m:
        continue
    prefix = int(m.group(1))
    text = open(path, encoding='utf-8').read()
    text = fix_crossrefs(text)
    if 4 <= prefix <= 20:
        text = fix_section_headings(text, prefix)
    if prefix == 19:
        text = text.replace("见 19.3", "见 20.3")
    open(path, 'w', encoding='utf-8').write(text)
    print(f"processed {name} (prefix {prefix})")
