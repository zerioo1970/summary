# -*- coding: utf-8 -*-
"""把「示例N-M」后面的纯文本代码段转成 Markdown 代码围栏。

原 Word 文档里的代码是用普通段落 + 软换行排的，pandoc 转出来后变成带反斜杠硬换行
且字符被转义（\\<、\\" 等）的普通文字。本脚本把它们还原成 ``` 代码块：
  - 识别 `**示例X-Y　标题**` 之后紧跟的连续非空行块
  - 去掉行尾硬换行反斜杠、还原 pandoc 转义
  - 自动判断语言（csharp / xml / json / bash / http / text）
  - 跳过表格（grid table）等非代码块
"""
import glob
import os
import re

CH = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "chapters")

# 「示例」标记有两种写法：加粗 **示例3-1　标题**、纯文本 示例9-1　标题
EXAMPLE = re.compile(r"^(?:\*\*)?示例[0-9A-Za-z]+[-－—][0-9]+[　\s].*?(?:\*\*)?\s*$")


def unescape(s):
    """还原 pandoc 的 markdown 转义。"""
    # 行尾硬换行反斜杠
    s = re.sub(r"\\$", "", s)
    # \<  \>  \"  \'  \*  \_  \[  \]  \|  \`  \#  \-  \.  \$  \{  \}  \\
    s = re.sub(r'\\([<>"\'*_\[\]|`#\-.$(){}~^&+=!?,;:/@%])', r"\1", s)
    s = s.replace("\\\\", "\\")
    return s


def guess_lang(block):
    t = "\n".join(block)
    if re.search(r"^\s*<\?xml|<Project|<PropertyGroup|<ItemGroup|</", t, re.M):
        return "xml"
    if re.search(r"\b(app\.Map|builder\.Services|var app|using |public |namespace |=>|await )", t):
        return "csharp"
    if re.search(r"^\s*[{\[]", t) and re.search(r'"\w+"\s*:', t):
        return "json"
    if re.search(r"^(dotnet|cd|git|docker|npm|curl|Get-ChildItem|sc\.exe|net |setx|mkdir)\b", t, re.M):
        return "bash"
    if re.search(r"^(GET|POST|PUT|DELETE|PATCH) https?://", t, re.M):
        return "http"
    if re.search(r"[├└│─]", t):        # 目录树
        return "text"
    if re.search(r"<\w+[^>]*>", t):     # HTML 片段
        return "html"
    return "text"


def is_table_or_list(line):
    s = line.strip()
    return (s.startswith("---") or s.startswith("+--") or s.startswith("|")
            or s.startswith("-   ") or s.startswith("#") or s.startswith(">")
            or s.startswith("```")      # 已经是代码块，避免重复包裹
            or s.startswith("!["))      # 图片


total = 0
for path in sorted(glob.glob(os.path.join(CH, "*.md"))):
    lines = open(path, encoding="utf-8").read().split("\n")
    out, i, n = [], 0, 0
    while i < len(lines):
        out.append(lines[i])
        if EXAMPLE.match(lines[i]):
            # 跳过紧随的空行
            j = i + 1
            blanks = []
            while j < len(lines) and lines[j].strip() == "":
                blanks.append(lines[j]); j += 1
            # 收集连续非空行作为候选代码块
            start = j
            while j < len(lines) and lines[j].strip() != "":
                j += 1
            block = lines[start:j]
            # 判断是否为代码块（非表格/列表/标题，且原文有硬换行或明显代码特征）
            if block and not is_table_or_list(block[0]):
                cleaned = [unescape(b) for b in block]
                lang = guess_lang(cleaned)
                out.extend(blanks)
                out.append(f"```{lang}")
                out.extend(cleaned)
                out.append("```")
                i = j
                n += 1
                continue
        i += 1
    if n:
        open(path, "w", encoding="utf-8").write("\n".join(out))
        print(f"{os.path.basename(path)}: 转换 {n} 个代码块")
        total += n

print(f"\n合计转换 {total} 个代码块")
