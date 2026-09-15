#!/bin/bash
# 将 chapters/ 下所有 markdown 按文件名顺序合成一个 Word 文档。
# 图片以 ../images/ 相对路径引用；在 chapters/ 目录内运行 pandoc 以正确解析。
set -e
DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$DIR/chapters"

FILES=$(ls *.md | sort)
echo "合成以下章节："
echo "$FILES"

pandoc $FILES \
  --reference-doc="$DIR/template/reference.docx" \
  --toc --toc-depth=2 \
  -f markdown+east_asian_line_breaks \
  -o "$DIR/ASP.NET-Core-最小API完全教程.docx"

echo "已生成: $DIR/ASP.NET-Core-最小API完全教程.docx"
